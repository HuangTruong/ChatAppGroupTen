using ChatApp.Models.Users;
using FireSharp;
using FireSharp.Config;
using FireSharp.EventStreaming;
using FireSharp.Interfaces;
using FireSharp.Response;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FirebaseAppConfig = ChatApp.Services.Firebase.FirebaseConfig;

namespace ChatApp.Controllers
{
    /// <summary>
    /// Mô tả 1 tin nhắn trong cuộc trò chuyện.
    /// </summary>
    public class ChatMessage
    {
        /// <summary>
        /// localId người gửi.
        /// </summary>
        public string SenderId { get; set; }

        /// <summary>
        /// localId người nhận.
        /// </summary>
        public string ReceiverId { get; set; }

        /// <summary>
        /// Nội dung tin nhắn (text).
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// Thời điểm gửi (Unix time milliseconds).
        /// </summary>
        public long Timestamp { get; set; }

        /// <summary>
        /// Có phải tin nhắn của user hiện tại hay không (phục vụ UI).
        /// </summary>
        public bool IsMine { get; set; }
    }

    /// <summary>
    /// Controller cho form Nhắn tin:
    /// - Kết nối Firebase Realtime Database qua FireSharp.
    /// - Lấy danh sách user.
    /// - Gửi tin nhắn.
    /// - Lắng nghe realtime tin nhắn.
    /// </summary>
    public class NhanTinController : IDisposable
    {
        #region ====== BIẾN THÀNH VIÊN ======

        /// <summary>
        /// localId của user hiện tại.
        /// </summary>
        private readonly string idNguoiDungHienTai;

        /// <summary>
        /// Token đăng nhập (để dành, nếu sau này cần).
        /// </summary>
        private readonly string tokenDangNhap;

        /// <summary>
        /// Client của FireSharp để gọi lên Firebase.
        /// </summary>
        private readonly IFirebaseClient firebaseClient;

        /// <summary>
        /// Stream hiện tại đang lắng nghe OnAsync.
        /// </summary>
        private EventStreamResponse luongSuKienHienTai;

        /// <summary>
        /// Id cuộc trò chuyện đang được lắng nghe.
        /// </summary>
        private string idCuocTroChuyenDangNghe;

        #endregion

        #region ====== HÀM KHỞI TẠO ======

        /// <summary>
        /// Khởi tạo controller với localId và token hiện tại.
        /// </summary>
        public NhanTinController(string currentUserId, string token)
        {
            idNguoiDungHienTai = currentUserId;
            tokenDangNhap = token;

            IFirebaseConfig cauHinh = new FirebaseConfig();
            cauHinh.BasePath = FirebaseAppConfig.DatabaseUrl;

            cauHinh.AuthSecret = string.Empty;

            firebaseClient = new FirebaseClient(cauHinh);
        }

        #endregion

        #region ====== HÀM PHỤ TRỢ ======

        /// <summary>
        /// Tạo conversationId ổn định cho 2 user:
        /// ghép 2 localId theo thứ tự từ điển.
        /// </summary>
        private string BuildConversationId(string userId1, string userId2)
        {
            int soSanh = string.CompareOrdinal(userId1, userId2);
            if (soSanh < 0)
            {
                return userId1 + "_" + userId2;
            }

            return userId2 + "_" + userId1;
        }

        /// <summary>
        /// So sánh 2 tin nhắn theo thời gian gửi.
        /// </summary>
        private int SoSanhTinNhanTheoThoiGian(ChatMessage tin1, ChatMessage tin2)
        {
            long thoiGian1 = 0;
            long thoiGian2 = 0;

            if (tin1 != null)
            {
                thoiGian1 = tin1.Timestamp;
            }

            if (tin2 != null)
            {
                thoiGian2 = tin2.Timestamp;
            }

            if (thoiGian1 < thoiGian2)
            {
                return -1;
            }

            if (thoiGian1 > thoiGian2)
            {
                return 1;
            }

            return 0;
        }

        /// <summary>
        /// Đọc toàn bộ tin nhắn của 1 cuộc trò chuyện, sắp xếp theo thời gian
        /// và đánh dấu IsMine cho từng tin nhắn.
        /// </summary>
        private async Task<List<ChatMessage>> LoadConversationAsync(string conversationId)
        {
            string duongDan = "messages/" + conversationId;

            FirebaseResponse phanHoi = await firebaseClient.GetAsync(duongDan);

            Dictionary<string, ChatMessage> duLieu =
                phanHoi.ResultAs<Dictionary<string, ChatMessage>>();

            List<ChatMessage> danhSachTinNhan = new List<ChatMessage>();

            if (duLieu != null)
            {
                foreach (KeyValuePair<string, ChatMessage> cap in duLieu)
                {
                    ChatMessage tin = cap.Value;
                    if (tin == null)
                    {
                        continue;
                    }

                    // Đánh dấu tin của mình
                    if (string.Equals(tin.SenderId, idNguoiDungHienTai, StringComparison.Ordinal))
                    {
                        tin.IsMine = true;
                    }
                    else
                    {
                        tin.IsMine = false;
                    }

                    danhSachTinNhan.Add(tin);
                }

                // Sắp xếp theo thời gian
                danhSachTinNhan.Sort(SoSanhTinNhanTheoThoiGian);
            }

            return danhSachTinNhan;
        }

        #endregion

        #region ====== USERS API ======

        /// <summary>
        /// Lấy toàn bộ user từ node "users" trong Realtime Database.
        /// </summary>
        public async Task<Dictionary<string, User>> GetAllUsersAsync()
        {
            FirebaseResponse phanHoi = await firebaseClient.GetAsync("friends");

            Dictionary<string, User> duLieu =
                phanHoi.ResultAs<Dictionary<string, User>>();

            if (duLieu == null)
            {
                return new Dictionary<string, User>();
            }

            return duLieu;
        }

        #endregion

        #region ====== MESSAGES API ======

        /// <summary>
        /// Gửi 1 tin nhắn text tới user khác.
        /// </summary>
        public async Task SendMessageAsync(string toUserId, string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return;
            }

            string conversationId = BuildConversationId(idNguoiDungHienTai, toUserId);
            string duongDan = "messages/" + conversationId;

            ChatMessage tinNhan = new ChatMessage();
            tinNhan.SenderId = idNguoiDungHienTai;
            tinNhan.ReceiverId = toUserId;
            tinNhan.Text = text;
            tinNhan.Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            tinNhan.IsMine = true;

            await firebaseClient.PushAsync(duongDan, tinNhan);
        }

        /// <summary>
        /// Bắt đầu lắng nghe realtime một cuộc trò chuyện với otherUserId.
        /// Khi dữ liệu trong "messages/{conversationId}" thay đổi sẽ gọi callback.
        /// </summary>
        public async void StartListenConversation(
            string otherUserId,
            Action<List<ChatMessage>> onMessagesChanged)
        {
            // Dừng stream cũ nếu có
            StopListen();

            if (onMessagesChanged == null)
            {
                return;
            }

            string conversationId = BuildConversationId(idNguoiDungHienTai, otherUserId);
            idCuocTroChuyenDangNghe = conversationId;

            string duongDan = "messages/" + conversationId;

            try
            {
                luongSuKienHienTai = await firebaseClient.OnAsync(
                    duongDan,
                    added: (gui, suKienThem, nguCanh) =>
                    {
                        XuLySuKienStream(conversationId, onMessagesChanged);
                    },
                    changed: (gui, suKienDoi, nguCanh) =>
                    {
                        XuLySuKienStream(conversationId, onMessagesChanged);
                    },
                    removed: (gui, suKienXoa, nguCanh) =>
                    {
                        XuLySuKienStream(conversationId, onMessagesChanged);
                    });
            }
            catch
            {
                // Có lỗi thì thôi, không cho ứng dụng crash.
            }
        }

        /// <summary>
        /// Khi stream báo thêm / đổi / xóa:
        /// load lại danh sách tin nhắn và gọi callback.
        /// </summary>
        private void XuLySuKienStream(
            string conversationId,
            Action<List<ChatMessage>> callback)
        {
            if (!string.Equals(idCuocTroChuyenDangNghe, conversationId, StringComparison.Ordinal))
            {
                return;
            }

            Task.Run(
                async () =>
                {
                    try
                    {
                        List<ChatMessage> danhSachTinNhan =
                            await LoadConversationAsync(conversationId);

                        callback(danhSachTinNhan);
                    }
                    catch
                    {
                        // Bỏ qua lỗi để không crash background thread.
                    }
                });
        }

        /// <summary>
        /// Dừng lắng nghe cuộc trò chuyện hiện tại.
        /// </summary>
        public void StopListen()
        {
            idCuocTroChuyenDangNghe = null;

            if (luongSuKienHienTai != null)
            {
                luongSuKienHienTai.Dispose();
                luongSuKienHienTai = null;
            }
        }

        #endregion

        #region ====== DISPOSE ======

        public void Dispose()
        {
            StopListen();
        }

        #endregion
    }
}
/*
Ý TƯỞNG CHÍNH CỦA NhanTinController

- Lớp này chỉ lo phần "logic nói chuyện với Firebase", không dính UI.
- Mỗi cuộc trò chuyện giữa 2 user được lưu tại node:
    messages/{conversationId}

1. Cách tạo conversationId
   - Để cả 2 phía cùng truy cập đúng 1 chỗ:
       conversationId = localId nhỏ hơn + "_" + localId lớn hơn (so sánh theo thứ tự từ điển).
   - Nhờ vậy:
       + User A và user B, dù ai tạo trước cũng dùng chung 1 conversationId.
       + Node trong Firebase luôn ổn định: "messages/A_B" hoặc "messages/B_C", ...

2. Lưu và đọc tin nhắn
   - Mỗi tin nhắn là 1 ChatMessage: SenderId, ReceiverId, Text, Timestamp, IsMine.
   - Gửi tin (SendMessageAsync):
       + Tính conversationId từ (idNguoiDungHienTai, idNguoiNhan).
       + Push 1 ChatMessage mới vào "messages/{conversationId}".
       + Timestamp dùng Unix time (ms) để dễ sort theo thời gian.
   - Đọc tin (LoadConversationAsync):
       + Get toàn bộ node "messages/{conversationId}".
       + Đưa về List<ChatMessage>, đánh dấu IsMine = true nếu SenderId == user hiện tại.
       + Sắp xếp danh sách theo Timestamp tăng dần rồi trả về cho UI.

3. Cơ chế realtime (OnAsync)
   - StartListenConversation(otherUserId, callback):
       + Tính conversationId dựa trên user hiện tại và otherUserId.
       + Gọi OnAsync trên "messages/{conversationId}" với 3 event:
           * added   : khi có tin nhắn mới.
           * changed : khi tin nhắn bị sửa.
           * removed : khi tin nhắn bị xóa.
       + Cả 3 event đều gọi XuLySuKienStream():
           * Kiểm tra có đúng cuộc trò chuyện đang mở không.
           * Nếu đúng thì Load lại toàn bộ danh sách tin nhắn.
           * Gọi callback(danhSachTinNhan) để UI vẽ lại.
   - StopListen():
       + Dừng stream hiện tại (Dispose EventStreamResponse).
       + Dùng khi:
           * Đổi sang cuộc trò chuyện khác.
           * Đóng form.

4. Mục tiêu
   - Tách sạch phần giao tiếp Firebase ra khỏi Form.
   - Form chỉ cần:
       + Gọi GetAllUsersAsync để lấy user.
       + Gọi SendMessageAsync để gửi.
       + Gọi StartListenConversation để đăng ký nghe realtime.
   - Toàn bộ "realtime" (OnAsync, xử lý event, reload dữ liệu, callback) đều nằm trong controller này.
*/
