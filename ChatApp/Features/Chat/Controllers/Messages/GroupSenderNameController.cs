using ChatApp.Models.Users;
using ChatApp.Services.Firebase;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ChatApp.Controllers
{
    /// <summary>
    /// Quản lý cache FullName của sender trong chat nhóm.
    /// - Tránh gọi Firebase trùng
    /// - Tự callback về UI khi tải xong để form update bubble
    /// </summary>
    public class GroupSenderNameController : IDisposable
    {
        #region ====== KHAI BÁO BIẾN ======

        private readonly AuthService _authService;

        private readonly object _lock = new object();
        private readonly Dictionary<string, string> _cache = new Dictionary<string, string>(StringComparer.Ordinal);
        private readonly HashSet<string> _loading = new HashSet<string>(StringComparer.Ordinal);

        #endregion

        #region ====== HÀM KHỞI TẠO ======

        public GroupSenderNameController(AuthService authService)
        {
            _authService = authService ?? throw new ArgumentNullException("authService");
        }

        #endregion

        #region ====== HÀM CÔNG KHAI ======

        /// <summary>
        /// Lấy display name (ưu tiên cache). Nếu chưa có cache => trả về senderId.
        /// </summary>
        public string GetDisplayName(string senderId)
        {
            if (string.IsNullOrWhiteSpace(senderId)) return "Người dùng";

            lock (_lock)
            {
                string name;
                if (_cache.TryGetValue(senderId, out name))
                {
                    if (!string.IsNullOrWhiteSpace(name)) return name;
                }
            }

            return senderId;
        }

        /// <summary>
        /// Đảm bảo đã load FullName. Khi load xong sẽ gọi callback UI.
        /// </summary>
        public void EnsureLoadedAsync(
            string senderId,
            Func<string, string> normalizeFullName,
            Control uiOwner,
            Action<string> onLoadedUpdateUi)
        {
            if (string.IsNullOrWhiteSpace(senderId)) return;
            if (uiOwner == null || uiOwner.IsDisposed) return;

            lock (_lock)
            {
                if (_cache.ContainsKey(senderId)) return;
                if (_loading.Contains(senderId)) return;
                _loading.Add(senderId);
            }

            Task.Run(async delegate
            {
                string fullName = null;

                try
                {
                    User u = await _authService.GetUserByIdAsync(senderId).ConfigureAwait(false);
                    if (u != null && normalizeFullName != null)
                    {
                        fullName = normalizeFullName(senderId);
                        // normalizeFullName ở đây nên là hàm GetUserFullName(user),
                        // nhưng để form truyền vào cho đúng logic của bạn.
                    }
                }
                catch
                {
                    fullName = null;
                }

                lock (_lock)
                {
                    _loading.Remove(senderId);
                    _cache[senderId] = fullName; // cache cả null để khỏi gọi lại
                }

                try
                {
                    if (uiOwner.IsDisposed) return;
                    uiOwner.BeginInvoke((Action)delegate
                    {
                        if (uiOwner.IsDisposed) return;
                        if (onLoadedUpdateUi != null) onLoadedUpdateUi(senderId);
                    });
                }
                catch
                {
                    // ignore
                }
            });
        }

        /// <summary>
        /// Set cache trực tiếp (nếu form đã có fullName).
        /// </summary>
        public void SetCache(string senderId, string fullName)
        {
            if (string.IsNullOrWhiteSpace(senderId)) return;

            lock (_lock)
            {
                _cache[senderId] = fullName;
            }
        }

        #endregion

        #region ====== GIẢI PHÓNG TÀI NGUYÊN ======

        public void Dispose()
        {
            // currently nothing to dispose
        }

        #endregion
    }
}
