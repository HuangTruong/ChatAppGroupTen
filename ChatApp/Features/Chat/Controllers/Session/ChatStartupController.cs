using ChatApp.Models.Users;
using ChatApp.Services.Firebase;
using ChatApp.Services.UI;
using System;
using System.Threading.Tasks;

namespace ChatApp.Controllers
{
    /// <summary>
    /// Gom logic khởi tạo NhanTin:
    /// - Reload danh sách hội thoại
    /// - Apply theme
    /// - Load tên user hiện tại
    /// </summary>
    public class ChatStartupController
    {
        #region ====== KHAI BÁO BIẾN ======

        private readonly string _currentUserId;
        private readonly ThemeService _themeService;
        private readonly AuthService _authService;
        private readonly ConversationListController _conversationListController;

        private readonly Action<bool> _applyTheme;
        private readonly Action<string> _setMeName;

        #endregion

        #region ====== HÀM KHỞI TẠO ======

        public ChatStartupController(
            string currentUserId,
            ThemeService themeService,
            AuthService authService,
            ConversationListController conversationListController,
            Action<bool> applyTheme,
            Action<string> setMeName)
        {
            _currentUserId = currentUserId;
            _themeService = themeService;
            _authService = authService;
            _conversationListController = conversationListController;

            _applyTheme = applyTheme;
            _setMeName = setMeName;
        }

        #endregion

        #region ====== KHỞI TẠO BAN ĐẦU ======

        public async Task InitializeAsync()
        {
            if (_conversationListController != null)
            {
                await _conversationListController.ReloadAsync().ConfigureAwait(true);
            }

            try
            {
                bool isDark = await _themeService.GetThemeAsync(_currentUserId).ConfigureAwait(true);
                if (_applyTheme != null) _applyTheme(isDark);
            }
            catch { }

            try
            {
                User me = await _authService.GetUserByIdAsync(_currentUserId).ConfigureAwait(true);
                if (me != null && _setMeName != null) _setMeName(me.FullName);
            }
            catch { }
        }

        #endregion
    }
}
