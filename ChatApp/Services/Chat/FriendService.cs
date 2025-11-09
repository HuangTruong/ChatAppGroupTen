using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ChatApp.Models.Chat;
using FireSharp.Interfaces;
using FireSharp.Response;

namespace ChatApp.Services.Chat
{
    public class FriendService
    {
        private readonly IFirebaseClient _firebase;     // Kết nối Firebase
        private readonly string _tenHienTai;            // Tên người dùng hiện tại

        public FriendService(IFirebaseClient firebase, string tenHienTai)
        {
            _firebase = firebase ?? throw new ArgumentNullException(nameof(firebase));
            _tenHienTai = tenHienTai ?? throw new ArgumentNullException(nameof(tenHienTai));
        }

        // Lấy danh sách bạn bè, lời mời đã gửi và lời mời nhận
        public async Task<(HashSet<string> BanBe,
                           HashSet<string> DaMoi,
                           HashSet<string> MoiDen)> LoadFriendStatesAsync()
        {
            var banBe = new HashSet<string>();
            var daMoi = new HashSet<string>();
            var moiDen = new HashSet<string>();

            // Lấy danh sách bạn bè
            var f1 = await _firebase.GetAsync("friends/" + _tenHienTai);
            var dataFriends = f1.ResultAs<Dictionary<string, bool>>();
            if (dataFriends != null)
                foreach (var kv in dataFriends)
                    banBe.Add(kv.Key);

            // Lấy các lời mời mình đã gửi
            var allPending = await _firebase.GetAsync("friendRequests/pending");
            var dataPending = allPending.ResultAs<Dictionary<string, Dictionary<string, bool>>>();
            if (dataPending != null)
            {
                foreach (var entry in dataPending)
                {
                    string nguoiNhan = entry.Key;
                    foreach (var kv in entry.Value)
                    {
                        string nguoiGui = kv.Key;
                        if (nguoiGui == _tenHienTai)
                            daMoi.Add(nguoiNhan);
                    }
                }
            }

            // Lấy các lời mời gửi đến mình
            var mePending = await _firebase.GetAsync("friendRequests/pending/" + _tenHienTai);
            var dataToMe = mePending.ResultAs<Dictionary<string, bool>>();
            if (dataToMe != null)
                foreach (var kv in dataToMe)
                    moiDen.Add(kv.Key);

            return (banBe, daMoi, moiDen);
        }

        // Gửi lời mời kết bạn
        public async Task GuiLoiMoiAsync(string ten)
        {
            await _firebase.SetAsync($"friendRequests/pending/{ten}/{_tenHienTai}", true);
        }

        // Chấp nhận lời mời kết bạn
        public async Task ChapNhanAsync(string ten)
        {
            await _firebase.SetAsync($"friends/{_tenHienTai}/{ten}", true);
            await _firebase.SetAsync($"friends/{ten}/{_tenHienTai}", true);
            await _firebase.DeleteAsync($"friendRequests/pending/{_tenHienTai}/{ten}");
        }

        // Huỷ kết bạn
        public async Task HuyKetBanAsync(string ten)
        {
            await _firebase.DeleteAsync($"friends/{_tenHienTai}/{ten}");
            await _firebase.DeleteAsync($"friends/{ten}/{_tenHienTai}");
        }
    }
}
