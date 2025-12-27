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
        private readonly AuthService _authService = new AuthService();
        private readonly string _localId;

        public FriendController(string localId)
        {
            _localId = localId;
        }
        #region ====== TẢI DANH SÁCH NGƯỜI DÙNG ĐỂ TÌM KIẾM ======

        public async Task<List<User>> LoadAllUsersForDisplayAsync()
        {
            // 1. Tải danh sách tất cả người dùng (Dictionary<LocalId, User>)
            var allUsersDict = await _friendService.GetAllUsersAsync(_localId);

            // 2. Tải danh sách các ID cần loại trừ
            // Lấy danh sách bạn bè
            var friendDict = await _friendService.GetFriendListAsync(_localId);
            var friendIds = friendDict.Keys.ToHashSet();

            // Lấy danh sách lời mời đã gửi đi (Outgoing Requests)
            var outgoingDict = await _friendService.GetOutgoingRequestsAsync(_localId);
            var outgoingIds = outgoingDict
                                .Where(kvp => kvp.Value.status == "pending")
                                .Select(kvp => kvp.Key)
                                .ToHashSet();

            // 3. Thực hiện lọc
            var filteredUsers = allUsersDict.Values
                .Where(user =>
                    // Loại trừ chính mình
                    user.LocalId != _localId &&

                    // Loại trừ những người đã là bạn bè
                    !friendIds.Contains(user.LocalId)
                )
                .ToList();

            return filteredUsers;
        }

        /// <summary>
        /// Lấy danh sách ID của những người đã được gửi lời mời (pending).
        /// </summary>
        public async Task<HashSet<string>> GetOutgoingRequestIdsAsync()
        {
            var outgoingDict = await _friendService.GetOutgoingRequestsAsync(_localId);
            return outgoingDict
                .Where(kvp => kvp.Value.status == "pending")
                .Select(kvp => kvp.Key)
                .ToHashSet();
        }

        #endregion

        #region ====== GỬI LỜI MỜI KẾT BẠN ======
        public async Task SendRequestAsync(string receiverId)
        {
            if (string.IsNullOrEmpty(receiverId)) return;

            await _friendService.SendFriendRequestAsync(_localId, receiverId);
        }

        #endregion

        #region ====== TẢI DỮ LIỆU LỜI MỜI VÀ GÁN PROFILE ======

        /// <summary>
        /// Tải tất cả các lời mời kết bạn đang chờ nhận của người dùng hiện tại, 
        /// sau đó tải và gán thông tin hồ sơ (Profile) của người gửi vào từng lời mời.
        /// </summary>
        public async Task<List<FriendRequest>> LoadIncomingRequestsForDisplayAsync()
        {
            // 1. Tải các request đang chờ
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
                request.Profile = senderProfile;
                request.OtherUserId = senderId;

                resultList.Add(request);
            }

            return resultList;
        }

        /// <summary>
        /// Tải danh sách người dùng đã gửi lời mời đến user hiện tại (trích xuất User profile).
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

        #endregion

        #region ====== XỬ LÝ LỜI MỜI KẾT BẠN ======

        /// <summary>
        /// Chấp nhận lời mời kết bạn.
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

        /// <summary>
        /// Hủy lời mời kết bạn đã gửi.
        /// </summary>
        public async Task CancelFriendRequestAsync(string receiverId)
        {
            await _friendService.CancelFriendRequestAsync(_localId, receiverId);
        }

        /// <summary>
        /// Hủy kết bạn với người dùng khác.
        /// </summary>
        public async Task UnfriendAsync(string friendId)
        {
            await _friendService.UnfriendAsync(_localId, friendId);
        }

        #endregion
    }
}