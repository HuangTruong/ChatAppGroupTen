using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ChatApp.Models.Friends;
using ChatApp.Models.Users;
using ChatApp.Services.Firebase;

namespace ChatApp.Controllers
{
    /// <summary>
    /// Controller xử lý logic bạn bè, lời mời kết bạn.
    /// </summary>
    public class FriendController
    {
        private readonly FriendService _friendService = new FriendService();
        private readonly AuthService _authService = new AuthService(); // Dùng lại AuthService để lấy profile user
        private readonly string _localId;

        // 💥 ĐIỂM BỔ SUNG 1: Expose AuthService cho UI (để tải ảnh)
        public AuthService AuthService => _authService;

        public FriendController(string localId)
        {
            _localId = localId;
        }

        /*==============================================================
         * FORM 1: DANH SÁCH TẤT CẢ USER (Để gửi lời mời)
         *==============================================================*/

        /// <summary>
        /// Tải danh sách tất cả người dùng (trừ người dùng hiện tại).
        /// </summary>
        public async Task<List<User>> LoadAllUsersForDisplayAsync()
        {
            // 1. Tải danh sách tất cả người dùng (Dictionary<LocalId, User>)
            var allUsersDict = await _friendService.GetAllUsersAsync(_localId);

            // 2. Tải danh sách các ID cần loại trừ
            // Lấy danh sách bạn bè
            var friendDict = await _friendService.GetFriendListAsync(_localId);
            var friendIds = friendDict.Keys.ToHashSet(); // Dùng HashSet để tìm kiếm nhanh hơn

            // Lấy danh sách lời mời đã gửi đi (Outgoing Requests)
            var outgoingDict = await _friendService.GetOutgoingRequestsAsync(_localId);
            var outgoingIds = outgoingDict
                                .Where(kvp => kvp.Value.status == "pending") // Chỉ lấy những cái đang chờ
                                .Select(kvp => kvp.Key)
                                .ToHashSet();

            // 3. Thực hiện lọc
            var filteredUsers = allUsersDict.Values
                .Where(user =>
                    // 🛑 A. Loại trừ chính mình (Logic đã có trong Service, nhưng giữ lại để an toàn)
                    user.LocalId != _localId &&

                    // 🛑 B. Loại trừ những người đã là bạn bè
                    !friendIds.Contains(user.LocalId) &&

                    // 🛑 C. Loại trừ những người đã gửi lời mời (đang chờ)
                    !outgoingIds.Contains(user.LocalId)
                )
                .ToList();

            return filteredUsers;
        }

        /// <summary>
        /// Xử lý gửi lời mời kết bạn.
        /// </summary>
        /// <param name="receiverId">ID của người nhận lời mời.</param>
        public async Task SendRequestAsync(string receiverId)
        {
            if (string.IsNullOrEmpty(receiverId)) return;

            await _friendService.SendFriendRequestAsync(_localId, receiverId);
        }

        /*==============================================================
         * FORM 2: DANH SÁCH LỜI MỜI KẾT BẠN ĐANG CHỜ
         *==============================================================*/

        /// <summary>
        /// Tải danh sách lời mời kết bạn đang chờ và kèm theo thông tin hồ sơ của người gửi.
        /// (Dùng nội bộ và cho hàm LoadFriendRequestsAsync)
        /// </summary>
        public async Task<List<FriendRequest>> LoadIncomingRequestsForDisplayAsync()
        {
            // 1. Tải các request đang chờ
            // Kết quả là Dictionary<SenderId, FriendRequest>
            var requestsDict = await _friendService.GetIncomingRequestsAsync(_localId);

            if (requestsDict == null || requestsDict.Count == 0)
            {
                return new List<FriendRequest>();
            }

            var resultList = new List<FriendRequest>();

            // 2. Lấy thông tin chi tiết (profile) cho từng người gửi
            foreach (var kvp in requestsDict)
            {
                string senderId = kvp.Key;
                FriendRequest request = kvp.Value;

                // Sử dụng hàm GetUserByIdAsync trong AuthService để lấy profile người gửi
                User senderProfile = await _authService.GetUserByIdAsync(senderId);

                // Gán profile vào request để UI có thể hiển thị Tên, Avatar...
                // Giả định FriendRequest model của bạn có thuộc tính Profile và OtherUserId
                // để lưu profile và ID của người khác (sender)
                request.Profile = senderProfile;
                request.OtherUserId = senderId;

                resultList.Add(request);
            }

            return resultList;
        }

        // 💥 ĐIỂM BỔ SUNG 2: Hàm công khai mà Form LoiMoiKetBan gọi
        /// <summary>
        /// Tải danh sách người dùng đã gửi lời mời đến user hiện tại (trả về dưới dạng User profile).
        /// </summary>
        public async Task<List<User>> LoadFriendRequestsAsync()
        {
            // 1. Tải các request kèm theo profile người gửi
            var requests = await LoadIncomingRequestsForDisplayAsync();

            // 2. Trích xuất User profile để UI có thể hiển thị Tên, Avatar
            return requests
                .Where(r => r.Profile != null) // Chỉ lấy request có profile hợp lệ
                .Select(r => r.Profile)
                .ToList();
        }

        /// <summary>
        /// Chấp nhận lời mời kết bạn (Hàm gốc gọi đến Service).
        /// </summary>
        public async Task AcceptFriendRequestAsync(string senderId)
        {
            await _friendService.AcceptFriendRequestAsync(_localId, senderId);
        }

        /// <summary>
        /// Từ chối lời mời kết bạn.
        /// </summary>
        public async Task RejectFriendRequestAsync(string senderId)
        {
            await _friendService.RejectFriendRequestAsync(_localId, senderId);
        }
    }
}