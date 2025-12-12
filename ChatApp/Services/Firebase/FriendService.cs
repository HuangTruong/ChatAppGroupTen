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
    /// <summary>
    /// Cung cấp các phương thức giao tiếp với Firebase Realtime Database 
    /// liên quan đến các hoạt động quản lý bạn bè (Friend Management).
    /// </summary>
    public class FriendService
    {
        private readonly HttpService _http = new HttpService();

        #region ====== HELPER METHOD ======

        /// <summary>
        /// Helper: Tạo URL truy vấn Realtime Database.
        /// </summary>
        private string Db(string path, string token = null)
        {
            // Thêm token vào query string để xác thực
            string auth = string.IsNullOrEmpty(token) ? "" : $"?auth={token}";
            return $"{FirebaseConfig.DatabaseUrl}/{path}.json{auth}";
        }

        #endregion

        #region ====== TẢI DỮ LIỆU USER VÀ DANH SÁCH BẠN BÈ ======

        /// <summary>
        /// Tải tất cả user trên hệ thống (dùng để tìm kiếm bạn bè).
        /// </summary>
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
        /// Tải danh sách bạn bè chính thức của người dùng hiện tại.
        /// </summary>
        /// <param name="currentLocalId">ID của người dùng hiện tại.</param>
        /// <returns>Dictionary&lt;FriendId, bool&gt; với 'bool' là giá trị tượng trưng (thường là true).</returns>
        public async Task<Dictionary<string, bool>> GetFriendListAsync(string currentLocalId)
        {
            string safeId = KeySanitizer.SafeKey(currentLocalId);
            // Lưu ý: Tên node PHẢI khớp với tên bạn đang dùng khi ACCEPT (ví dụ: friends)
            string url = Db($"friends/{safeId}");

            // Bạn bè được lưu dưới dạng Dictionary<FriendId, true>
            var friends = await _http.GetAsync<Dictionary<string, bool>>(url);

            return friends ?? new Dictionary<string, bool>();
        }

        #endregion

        #region ====== QUẢN LÝ LỜI MỜI (REQUESTS) ======

        /// <summary>
        /// Gửi lời mời kết bạn (ghi đồng thời vào 2 node: outgoingRequests và friendRequests).
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

            // 1. Ghi vào node Outgoing (Đã gửi): /outgoingRequests/{senderId}/{receiverId}
            string outgoingPath = $"outgoingRequests/{safeSenderId}/{safeReceiverId}";
            await _http.PutAsync(Db(outgoingPath), requestData);

            // 2. Ghi vào node Incoming (Nhận được): /friendRequests/{receiverId}/{senderId}
            string incomingPath = $"friendRequests/{safeReceiverId}/{safeSenderId}";
            await _http.PutAsync(Db(incomingPath), requestData);
        }

        /// <summary>
        /// Tải tất cả lời mời kết bạn đang chờ của người dùng hiện tại (lời mời nhận được).
        /// </summary>
        public async Task<Dictionary<string, FriendRequest>> GetIncomingRequestsAsync(string currentLocalId)
        {
            string safeId = KeySanitizer.SafeKey(currentLocalId);
            string url = Db($"friendRequests/{safeId}");

            // Kết quả trả về là Dictionary<SenderId, FriendRequest>
            var requestsData = await _http.GetAsync<Dictionary<string, FriendRequest>>(url);

            if (requestsData == null)
            {
                return new Dictionary<string, FriendRequest>();
            }

            // Chỉ giữ lại những request có status là "pending" và gán OtherUserId
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
        /// Tải danh sách lời mời đã gửi đi của người dùng hiện tại.
        /// </summary>
        public async Task<Dictionary<string, FriendRequest>> GetOutgoingRequestsAsync(string currentLocalId)
        {
            string safeId = KeySanitizer.SafeKey(currentLocalId);
            string url = Db($"outgoingRequests/{safeId}");

            // Kết quả trả về là Dictionary<ReceiverId, FriendRequest>
            var requests = await _http.GetAsync<Dictionary<string, FriendRequest>>(url);

            return requests ?? new Dictionary<string, FriendRequest>();
        }

        #endregion

        #region ====== CHẤP NHẬN/TỪ CHỐI LỜI MỜI ======

        /// <summary>
        /// Chấp nhận lời mời kết bạn: 
        /// 1. Ghi liên kết bạn bè 2 chiều vào node 'friends'.
        /// 2. XÓA lời mời ở cả 2 node request (incoming và outgoing).
        /// </summary>
        /// 
        public async Task AcceptFriendRequestAsync(string currentLocalId, string senderId)
        {
            string safeCurrentId = KeySanitizer.SafeKey(currentLocalId);
            string safeSenderId = KeySanitizer.SafeKey(senderId);

            var friendLink = true; // Giá trị để đánh dấu liên kết bạn bè

            // 1. Ghi vào node friends (Liên kết 2 chiều)
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
            await _http.PutAsync(Db($"friends/{safeSenderId}"), senderToUserPayload);

            // 2. XÓA status request (dùng DELETEAsync)
            // A. Incoming request (tại Current User): /friendRequests/{currentLocalId}/{senderId}
            await _http.DeleteAsync(Db($"friendRequests/{safeCurrentId}/{safeSenderId}"));

            // B. Outgoing request (tại Sender): /outgoingRequests/{senderId}/{currentLocalId}
            await _http.DeleteAsync(Db($"outgoingRequests/{safeSenderId}/{safeCurrentId}"));
        }

        /// <summary>
        /// Từ chối lời mời kết bạn: XÓA lời mời ở cả 2 node request (incoming và outgoing).
        /// </summary>
        public async Task RejectFriendRequestAsync(string currentLocalId, string senderId)
        {
            string safeCurrentId = KeySanitizer.SafeKey(currentLocalId);
            string safeSenderId = KeySanitizer.SafeKey(senderId);

            // 1. XÓA node request tại người nhận (currentLocalId là người từ chối)
            // Path: /friendRequests/{currentLocalId}/{senderId}
            await _http.DeleteAsync(Db($"friendRequests/{safeCurrentId}/{safeSenderId}"));

            // 2. XÓA node request tại người gửi (senderId)
            // Path: /outgoingRequests/{senderId}/{currentLocalId}
            await _http.DeleteAsync(Db($"outgoingRequests/{safeSenderId}/{safeCurrentId}"));
        }

        #endregion
    }
}