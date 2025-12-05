using ChatApp.Helpers;
using ChatApp.Models.Friends;
using ChatApp.Models.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ChatApp.Services.Firebase
{
    public class FriendService
    {
        private readonly HttpService _http = new HttpService();

        // Helper: Tạo URL truy vấn Realtime Database
        private string Db(string path, string token = null)
        {
            // Thêm token vào query string để xác thực
            string auth = string.IsNullOrEmpty(token) ? "" : $"?auth={token}";
            return $"{FirebaseConfig.DatabaseUrl}/{path}.json{auth}";
        }

        /*==============================================================
         * FORM 1: LOAD TẤT CẢ USER VÀ GỬI LỜI MỜI
         *==============================================================*/

        /// <summary>
        /// Tải tất cả user trên hệ thống (dùng để tìm kiếm bạn bè).
        /// </summary>
        /// <param name="currentLocalId">ID của người dùng hiện tại (để lọc chính mình).</param>
        public async Task<Dictionary<string, User>> GetAllUsersAsync(string currentLocalId)
        {
            // 1. Tải tất cả user từ node 'users'
            string url = Db("users");
            var usersData = await _http.GetAsync<Dictionary<string, User>>(url);

            if (usersData == null) return new Dictionary<string, User>();

            // 2. Lọc bỏ chính người dùng hiện tại
            var filteredUsers = new Dictionary<string, User>();
            string safeId = KeySanitizer.SafeKey(currentLocalId);

            foreach (var kvp in usersData)
            {
                // Key trong DB là localId (đã được "làm sạch")
                if (kvp.Key != safeId)
                {
                    // Gán lại key (localId) vào user object để tiện sử dụng
                    kvp.Value.LocalId = kvp.Key;
                    filteredUsers.Add(kvp.Key, kvp.Value);
                }
            }

            return filteredUsers;
        }

        /// <summary>
        /// Gửi lời mời kết bạn (ghi vào 2 node: outgoing và friendRequests).
        /// </summary>
        public async Task SendFriendRequestAsync(string senderId, string receiverId)
        {
            string safeSenderId = KeySanitizer.SafeKey(senderId);
            string safeReceiverId = KeySanitizer.SafeKey(receiverId);

            var requestData = new FriendRequest
            {
                status = "pending",
                createdAt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
            };

            // 1. Ghi vào node Outgoing (Đã gửi)
            string outgoingPath = $"outgoingRequests/{safeSenderId}/{safeReceiverId}";
            await _http.PutAsync(Db(outgoingPath), requestData);

            // 2. Ghi vào node FriendRequests (Nhận được)
            string incomingPath = $"friendRequests/{safeReceiverId}/{safeSenderId}";
            await _http.PutAsync(Db(incomingPath), requestData);
        }

        /*==============================================================
         * FORM 2: LOAD LỜI MỜI KẾT BẠN ĐANG CHỜ
         *==============================================================*/

        /// <summary>
        /// Tải tất cả lời mời kết bạn đang chờ của người dùng hiện tại.
        /// </summary>
        public async Task<Dictionary<string, FriendRequest>> GetIncomingRequestsAsync(string currentLocalId)
        {
            string safeId = KeySanitizer.SafeKey(currentLocalId);
            string url = Db($"friendRequests/{safeId}"); 

            // Kết quả trả về là Dictionary<SenderId, FriendRequest>
            var requestsData = await _http.GetAsync<Dictionary<string, FriendRequest>>(url);

            if (requestsData == null)
            {
                MessageBox.Show($"Firebase trả về NULL hoặc rỗng (URL: {url}).", "DEBUG DATA");
                return new Dictionary<string, FriendRequest>();
            }

            // Chỉ giữ lại những request có status là "pending"
            var pendingRequests = new Dictionary<string, FriendRequest>();
            {
                foreach (var kvp in requestsData)
                {
                    if (kvp.Value.status == "pending")
                    {
                        kvp.Value.OtherUserId = kvp.Key; 
                        pendingRequests.Add(kvp.Key, kvp.Value);
                    }
                }
            }

            return pendingRequests;
        }

        /// <summary>
        /// Chấp nhận lời mời kết bạn: 
        /// 1. Ghi vào node friends của cả 2 người.
        /// 2. Cập nhật status thành 'accepted' ở cả 2 node request.
        /// </summary>
        public async Task AcceptFriendRequestAsync(string currentLocalId, string senderId)
        {
            string safeCurrentId = KeySanitizer.SafeKey(currentLocalId);
            string safeSenderId = KeySanitizer.SafeKey(senderId);

            var friendLink = true;

            // 1. Ghi vào node friends (BẮT BUỘC SỬ DỤNG PUTAsync như đã đề xuất)
            // A. Current User -> Sender
            var userToSenderPayload = new Dictionary<string, object> 
            { 
                { safeSenderId, friendLink } 
            };
            await _http.PutAsync(Db($"friends/{safeCurrentId}"), userToSenderPayload);
    
            // B. Sender -> Current User
            var senderToUserPayload = new Dictionary<string, object> 
            { 
                { safeCurrentId, friendLink } 
            };
            await _http.PutAsync(Db($"friends/{safeSenderId}"), senderToUserPayload); // 👈 Đã sửa thành PUTAsync
    
            // 2. XÓA status request (Phần đã sửa)
            // A. Incoming request (tại Current User)
            // 💥 Dùng DELETEAsync để xóa node lời mời đã nhận
            await _http.DeleteAsync(Db($"friendRequests/{safeCurrentId}/{safeSenderId}"));
    
            // B. Outgoing request (tại Sender)
            // 💥 Dùng DELETEAsync để xóa node lời mời đã gửi
            await _http.DeleteAsync(Db($"outgoingRequests/{safeSenderId}/{safeCurrentId}")); 
        }

        /// <summary>
        /// Từ chối lời mời kết bạn (chỉ cập nhật status thành 'rejected').
        /// </summary>
        public async Task RejectFriendRequestAsync(string currentLocalId, string senderId)
        {
            string safeCurrentId = KeySanitizer.SafeKey(currentLocalId);
            string safeSenderId = KeySanitizer.SafeKey(senderId);

            // 1. XÓA node request tại người nhận (currentLocalId là người từ chối)
            // Path: friendRequests/{currentLocalId}/{senderId}
            // Hành động này xóa lời mời khỏi danh sách "Lời mời nhận được" của người từ chối.
            await _http.DeleteAsync(Db($"friendRequests/{safeCurrentId}/{safeSenderId}"));

            // 2. XÓA node request tại người gửi (senderId)
            // Path: outgoingRequests/{senderId}/{currentLocalId}
            // Hành động này xóa lời mời khỏi danh sách "Lời mời đã gửi" của người gửi, 
            // cho phép họ gửi lại sau này.
            await _http.DeleteAsync(Db($"outgoingRequests/{safeSenderId}/{safeCurrentId}"));
        }

        /// <summary>
        /// Tải danh sách bạn bè chính thức của người dùng hiện tại (chỉ trả về Dictionary<FriendId, bool>).
        /// </summary>
        public async Task<Dictionary<string, bool>> GetFriendListAsync(string currentLocalId)
        {
            string safeId = KeySanitizer.SafeKey(currentLocalId);
            // Lưu ý: Tên node PHẢI khớp với tên bạn đang dùng khi ACCEPT (ví dụ: friends)
            string url = Db($"friends/{safeId}");

            // Bạn bè được lưu dưới dạng Dictionary<FriendId, true>
            var friends = await _http.GetAsync<Dictionary<string, bool>>(url);

            return friends ?? new Dictionary<string, bool>();
        }

        /// <summary>
        /// Tải danh sách lời mời đã gửi đi của người dùng hiện tại (chỉ trả về Dictionary<ReceiverId, FriendRequest>).
        /// </summary>
        public async Task<Dictionary<string, FriendRequest>> GetOutgoingRequestsAsync(string currentLocalId)
        {
            string safeId = KeySanitizer.SafeKey(currentLocalId);
            string url = Db($"outgoingRequests/{safeId}");

            // Kết quả trả về là Dictionary<ReceiverId, FriendRequest>
            var requests = await _http.GetAsync<Dictionary<string, FriendRequest>>(url);

            return requests ?? new Dictionary<string, FriendRequest>();
        }
    }
}