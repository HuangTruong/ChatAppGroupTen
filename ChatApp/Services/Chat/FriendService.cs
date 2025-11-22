using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FireSharp.Interfaces;

namespace ChatApp.Services.Chat
{
    /// <summary>
    /// Service quản lý quan hệ bạn bè và lời mời kết bạn trên Firebase:
    /// - Lấy snapshot tình trạng bạn bè / đã mời / được mời.
    /// - Gửi, huỷ lời mời kết bạn.
    /// - Chấp nhận kết bạn, huỷ kết bạn 2 chiều.
    /// </summary>
    public class FriendService
    {
        #region ======== Trường / Khởi tạo ========

        /// <summary>
        /// Client Firebase dùng để đọc/ghi dữ liệu bạn bè.
        /// </summary>
        private readonly IFirebaseClient _firebase;

        /// <summary>
        /// Tên người dùng hiện tại trong Firebase (username).
        /// </summary>
        private readonly string _tenHienTai;

        /// <summary>
        /// Khởi tạo <see cref="FriendService"/> với client Firebase và tên user hiện tại.
        /// </summary>
        /// <param name="firebase">Client Firebase đã cấu hình.</param>
        /// <param name="tenHienTai">Tên người dùng hiện tại (dùng làm key trong node friends/pending).</param>
        public FriendService(IFirebaseClient firebase, string tenHienTai)
        {
            _firebase = firebase ?? throw new ArgumentNullException("firebase");
            _tenHienTai = tenHienTai ?? throw new ArgumentNullException("tenHienTai");
        }

        #endregion

        #region ======== Lấy snapshot trạng thái bạn bè / lời mời ========

        /// <summary>
        /// Lấy snapshot trạng thái bạn bè của người dùng hiện tại:
        /// - <c>BanBe</c>: username đã là bạn với mình (node <c>friends/{me}</c>).
        /// - <c>DaMoi</c>: username mình đã gửi lời mời (node <c>friendRequests/pending/*</c>).
        /// - <c>MoiDen</c>: username đã gửi lời mời kết bạn cho mình (node <c>friendRequests/pending/{me}</c>).
        /// </summary>
        /// <returns>
        /// Bộ 3 <see cref="HashSet{T}"/>:
        /// <c>BanBe</c>, <c>DaMoi</c>, <c>MoiDen</c>, so sánh không phân biệt hoa thường.
        /// </returns>
        public async Task<(HashSet<string> BanBe,
                           HashSet<string> DaMoi,
                           HashSet<string> MoiDen)> LoadFriendStatesAsync()
        {
            var banBe = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var daMoi = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var moiDen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            // ----- friends/<me> -----
            var f1 = await _firebase.GetAsync("friends/" + _tenHienTai);
            var dataFriends = f1.ResultAs<Dictionary<string, bool>>();
            if (dataFriends != null)
            {
                foreach (var kv in dataFriends)
                {
                    if (!string.IsNullOrWhiteSpace(kv.Key))
                    {
                        banBe.Add(kv.Key);
                    }
                }
            }

            // ----- friendRequests/pending (toàn bộ) => mình là người gửi -----
            var allPending = await _firebase.GetAsync("friendRequests/pending");
            var dataPending = allPending.ResultAs<Dictionary<string, Dictionary<string, bool>>>();
            if (dataPending != null)
            {
                foreach (var entry in dataPending)
                {
                    // friendRequests/pending/<nguoiNhan>/{nguoiGui:true}
                    string nguoiNhan = entry.Key;
                    var guiDict = entry.Value;
                    if (guiDict == null)
                    {
                        continue;
                    }

                    foreach (var kv in guiDict)
                    {
                        string nguoiGui = kv.Key;
                        if (nguoiGui.Equals(_tenHienTai, StringComparison.OrdinalIgnoreCase) &&
                            !string.IsNullOrWhiteSpace(nguoiNhan))
                        {
                            daMoi.Add(nguoiNhan);
                        }
                    }
                }
            }

            // ----- friendRequests/pending/<me> => những người mời mình -----
            var mePending = await _firebase.GetAsync("friendRequests/pending/" + _tenHienTai);
            var dataToMe = mePending.ResultAs<Dictionary<string, bool>>();
            if (dataToMe != null)
            {
                foreach (var kv in dataToMe)
                {
                    if (!string.IsNullOrWhiteSpace(kv.Key))
                    {
                        moiDen.Add(kv.Key);
                    }
                }
            }

            return (banBe, daMoi, moiDen);
        }

        #endregion

        #region ======== Gửi / Huỷ lời mời kết bạn ========

        /// <summary>
        /// Gửi lời mời kết bạn từ user hiện tại tới <paramref name="ten"/>.
        /// </summary>
        /// <param name="ten">Tên người cần mời kết bạn.</param>
        public async Task GuiLoiMoiAsync(string ten)
        {
            if (string.IsNullOrWhiteSpace(ten))
            {
                return;
            }

            await _firebase.SetAsync("friendRequests/pending/" + ten + "/" + _tenHienTai, true);
        }

        /// <summary>
        /// Huỷ lời mời kết bạn mà user hiện tại đã gửi tới <paramref name="ten"/>.
        /// </summary>
        /// <param name="ten">Tên người đã được mình gửi lời mời.</param>
        public async Task HuyLoiMoiAsync(string ten)
        {
            if (string.IsNullOrWhiteSpace(ten))
            {
                return;
            }

            await _firebase.DeleteAsync("friendRequests/pending/" + ten + "/" + _tenHienTai);
        }

        #endregion

        #region ======== Chấp nhận / Huỷ kết bạn ========

        /// <summary>
        /// Chấp nhận lời mời kết bạn từ <paramref name="ten"/>:
        /// - Ghi node friends 2 chiều.
        /// - Xoá pending ở node <c>friendRequests/pending/{me}/{ten}</c>.
        /// </summary>
        /// <param name="ten">Người gửi lời mời cho mình.</param>
        public async Task ChapNhanAsync(string ten)
        {
            if (string.IsNullOrWhiteSpace(ten))
            {
                return;
            }

            // Thêm vào friends 2 chiều
            await _firebase.SetAsync("friends/" + _tenHienTai + "/" + ten, true);
            await _firebase.SetAsync("friends/" + ten + "/" + _tenHienTai, true);

            // Xoá pending (lời mời gửi đến mình)
            await _firebase.DeleteAsync("friendRequests/pending/" + _tenHienTai + "/" + ten);
        }

        /// <summary>
        /// Huỷ kết bạn 2 chiều giữa user hiện tại và <paramref name="ten"/>.
        /// </summary>
        /// <param name="ten">Tên người cần huỷ kết bạn.</param>
        public async Task HuyKetBanAsync(string ten)
        {
            if (string.IsNullOrWhiteSpace(ten))
            {
                return;
            }

            await _firebase.DeleteAsync("friends/" + _tenHienTai + "/" + ten);
            await _firebase.DeleteAsync("friends/" + ten + "/" + _tenHienTai);
        }

        #endregion
    }
}
