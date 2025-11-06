using System;
using System.Threading.Tasks;
using ChatApp.Helpers;
using ChatApp.Models.Otp;
using ChatApp.Services.Firebase;

namespace ChatApp.Services.Auth
{
    public class OtpService
    {
        public async Task SaveOtpAsync(string taiKhoan, string ma, DateTime hetHan)
        {
            var client = FirebaseClientFactory.Create();
            await client.SetAsync($"otp/{KeySanitizer.SafeKey(taiKhoan)}",
                new ThongTinMaFirebase { Ma = ma, HetHanLuc = hetHan.ToString("o") });
        }

        public async Task<ThongTinMaFirebase> GetOtpAsync(string taiKhoan)
        {
            var client = FirebaseClientFactory.Create();
            var res = await client.GetAsync($"otp/{KeySanitizer.SafeKey(taiKhoan)}");
            return res.Body == "null" ? null : res.ResultAs<ThongTinMaFirebase>();
        }

        public async Task DeleteOtpAsync(string taiKhoan)
        {
            var client = FirebaseClientFactory.Create();
            await client.DeleteAsync($"otp/{KeySanitizer.SafeKey(taiKhoan)}");
        }
    }
}
