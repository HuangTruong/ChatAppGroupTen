using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FireSharp.Interfaces;

namespace ChatApp.Services.Chat
{
    /// <summary>
    /// Quản lý trạng thái bạn bè và lời mời kết bạn trên Firebase.
    /// </summary>
    public class FriendService
    {
        private readonly IFirebaseClient _firebase;     // Kết nối Firebase
        private readonly string _tenHienTai;            // Tên user hiện tại trong Firebase

        public FriendService(IFirebaseClient firebase, string tenHienTai)
        {
            _firebase = firebase ?? throw new ArgumentNullException(nameof(firebase));
            _tenHienTai = tenHienTai ?? throw new ArgumentNullException(nameof(tenHienTai));
        }

        /// <summary>
        /// Lấy:
        /// - BanBe: username đã là bạn với mình
        /// - DaMoi: username mình đã gửi lời mời
        /// - MoiDen: username đã gửi lời mời kết bạn cho mình
        /// </summary>
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
                        banBe.Add(kv.Key);
                }
            }

            // ----- friendRequests/pending (toàn bộ) => mình là người gửi -----
            var allPending = await _firebase.GetAsync("friendRequests/pending");
            var dataPending = allPending.ResultAs<Dictionary<string, Dictionary<string, bool>>>();
            if (dataPending != null)
            {
                foreach (var entry in dataPending)
                {
                    string nguoiNhan = entry.Key; // friendRequests/pending/<nguoiNhan>/{nguoiGui:true}
                    var guiDict = entry.Value;
                    if (guiDict == null) continue;

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
                        moiDen.Add(kv.Key);
                }
            }

            return (banBe, daMoi, moiDen);
        }

        /// <summary> Gửi lời mời kết bạn cho 'ten'. </summary>
        public async Task GuiLoiMoiAsync(string ten)
        {
            if (string.IsNullOrWhiteSpace(ten)) return;
            await _firebase.SetAsync($"friendRequests/pending/{ten}/{_tenHienTai}", true);
        }

        /// <summary> Huỷ lời mời đã gửi. </summary>
        public async Task HuyLoiMoiAsync(string ten)
        {
            if (string.IsNullOrWhiteSpace(ten)) return;
            await _firebase.DeleteAsync($"friendRequests/pending/{ten}/{_tenHienTai}");
        }

        /// <summary> Chấp nhận lời mời kết bạn từ 'ten'. </summary>
        public async Task ChapNhanAsync(string ten)
        {
            if (string.IsNullOrWhiteSpace(ten)) return;

            // Thêm vào friends 2 chiều
            await _firebase.SetAsync($"friends/{_tenHienTai}/{ten}", true);
            await _firebase.SetAsync($"friends/{ten}/{_tenHienTai}", true);

            // Xoá pending (lời mời gửi đến mình)
            await _firebase.DeleteAsync($"friendRequests/pending/{_tenHienTai}/{ten}");
        }

        /// <summary> Huỷ kết bạn với 'ten'. </summary>
        public async Task HuyKetBanAsync(string ten)
        {
            if (string.IsNullOrWhiteSpace(ten)) return;

            await _firebase.DeleteAsync($"friends/{_tenHienTai}/{ten}");
            await _firebase.DeleteAsync($"friends/{ten}/{_tenHienTai}");
        }
    }
}
