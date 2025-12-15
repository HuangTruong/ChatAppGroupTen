using ChatApp.Helpers;
using ChatApp.Models.Themes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatApp.Services.Firebase
{
    /// <summary>
    /// Service quản lý chế độ giao diện Ngày / Đêm của người dùng
    /// </summary>
    public class ThemeService
    {
        private readonly HttpService _http = new HttpService();

        #region ====== HELPER METHOD ======

        /// <summary>
        /// Helper: Tạo URL truy vấn Firebase Realtime Database
        /// </summary>
        private string Db(string path, string token = null)
        {
            string auth = string.IsNullOrEmpty(token) ? "" : $"?auth={token}";
            return $"{FirebaseConfig.DatabaseUrl}/{path}.json{auth}";
        }

        #endregion

        #region ====== THEME MODE ======

        /// <summary>
        /// Lưu chế độ Ngày / Đêm của người dùng
        /// </summary>
        public async Task SaveThemeAsync(string localId, bool isDarkMode)
        {
            string safeId = KeySanitizer.SafeKey(localId);

            var themeData = new ThemeSetting
            {
                IsDarkMode = isDarkMode
            };

            string url = Db($"themes/{safeId}");
            await _http.PutAsync(url, themeData);
        }

        /// <summary>
        /// Lấy chế độ Ngày / Đêm của người dùng
        /// </summary>
        /// <returns>
        /// true = Dark Mode  
        /// false = Light Mode (mặc định nếu chưa có dữ liệu)
        /// </returns>
        public async Task<bool> GetThemeAsync(string localId)
        {
            string safeId = KeySanitizer.SafeKey(localId);
            string url = Db($"themes/{safeId}");

            var theme = await _http.GetAsync<ThemeSetting>(url);

            // Nếu chưa từng lưu → mặc định Light Mode
            if (theme == null)
                return false;

            return theme.IsDarkMode;
        }

        #endregion
    }
}
