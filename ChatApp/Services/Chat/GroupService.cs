using ChatApp.Models.Chat;
using ChatApp.Helpers;
using FireSharp.Interfaces;
using FireSharp.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChatApp.Services.Chat
{
    public class GroupService
    {
        private readonly IFirebaseClient _firebase; // Kết nối Firebase

        public GroupService(IFirebaseClient firebase)
        {
            _firebase = firebase ?? throw new ArgumentNullException(nameof(firebase));
        }

        // Lấy danh sách tất cả nhóm
        public async Task<Dictionary<string, Nhom>> GetAllAsync()
        {
            var res = await _firebase.GetAsync("nhom");
            var data = res.ResultAs<Dictionary<string, Nhom>>();
            return data ?? new Dictionary<string, Nhom>();
        }

        // Gửi tin nhắn vào nhóm
        public async Task<TinNhan> SendGroupAsync(string groupId, string from, string content)
        {
            var tn = new TinNhan
            {
                guiBoi = from,
                nhanBoi = string.Empty,
                noiDung = content ?? string.Empty,
                thoiGian = DateTime.UtcNow.ToString("o")
            };

            var push = await _firebase.PushAsync($"cuocTroChuyenNhom/{groupId}/", tn);
            tn.id = push.Result.name;
            await _firebase.SetAsync($"cuocTroChuyenNhom/{groupId}/{tn.id}", tn);

            return tn;
        }

        // Tải lịch sử tin nhắn nhóm
        public async Task<List<TinNhan>> LoadGroupAsync(string groupId)
        {
            var res = await _firebase.GetAsync($"cuocTroChuyenNhom/{groupId}");
            var data = res.ResultAs<Dictionary<string, TinNhan>>();

            if (data == null) return new List<TinNhan>();

            return data
                .Select(kv =>
                {
                    var t = kv.Value ?? new TinNhan();
                    if (string.IsNullOrEmpty(t.id)) t.id = kv.Key;
                    if (t.noiDung == null) t.noiDung = string.Empty;
                    return t;
                })
                .OrderBy(t => TimeParser.ToUtc(t.thoiGian))
                .ToList();
        }

        // Thêm thành viên vào nhóm
        public async Task AddMembersAsync(string groupId, IEnumerable<string> names)
        {
            foreach (var ten in names)
            {
                await _firebase.SetAsync($"nhom/{groupId}/thanhVien/{ten}", true);
            }
        }

        // Xoá nhóm và toàn bộ tin nhắn trong nhóm
        public async Task DeleteGroupAsync(string groupId)
        {
            await _firebase.DeleteAsync("nhom/" + groupId);
            await _firebase.DeleteAsync("cuocTroChuyenNhom/" + groupId);
        }

        // Đánh dấu đã xem tin nhắn cuối cùng
        public async Task MarkLastSeenAsync(string username, string groupId, string msgId)
        {
            if (string.IsNullOrEmpty(msgId)) return;
            long now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            await _firebase.SetAsync(
                $"cuocTroChuyenNhom/{groupId}/{msgId}/reads/{username}",
                now);
        }
    }
}
