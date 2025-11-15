using ChatApp.Helpers;
using FireSharp.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ChatApp.Services.Status
{
    public class StatusService
    {
        private readonly IFirebaseClient _firebase; // Kết nối Firebase


        public StatusService(IFirebaseClient firebase)
        {
            _firebase = firebase ?? throw new ArgumentNullException(nameof(firebase));
        }

        // Cập nhật trạng thái online/offline của người dùng
        public async Task UpdateAsync(string tenHienThi, string trangThai)
        {
            if (string.IsNullOrWhiteSpace(tenHienThi)) return;
            var key = KeySanitizer.SafeKey(tenHienThi); // Chuẩn hoá key trước khi lưu
            await _firebase.SetAsync($"status/{key}", trangThai);
        }

        // Lấy danh sách trạng thái của tất cả người dùng
        public async Task<Dictionary<string, string>> GetAllAsync()
        {
            var res = await _firebase.GetAsync("status");
            var data = res.ResultAs<Dictionary<string, string>>();
            return data ?? new Dictionary<string, string>();
        }
    }
}
