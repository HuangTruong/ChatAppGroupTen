using ChatApp.Controllers;
using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ChatApp.Controls
{
    /// <summary>
    /// Item hiển thị một cuộc trò chuyện trong danh sách chat.
    /// - Hiện tên (fullName)
    /// - Hiện avatar (nếu không có avatar trên Firebase => dùng default_avatar.png)
    /// - Click vào bất kỳ vùng nào cũng coi là click item
    /// </summary>
    public partial class Conversations : UserControl
    {
        #region ====== THUỘC TÍNH & SỰ KIỆN ======

        /// <summary>
        /// Id người dùng / id nhóm (tuỳ bạn set trong Tag).
        /// </summary>
        public string UserId { get; set; }

        /// <summary>
        /// Sự kiện bắn ra khi người dùng click vào item.
        /// </summary>
        public event EventHandler ItemClicked;

        #endregion

        #region ====== AVATAR ======

        private readonly AvatarController _avatarController = new AvatarController();

        /// <summary>
        /// Load avatar theo senderId (localId) và gán vào picAvatar.
        /// Nếu không có avatar => AvatarController tự dùng default_avatar.png.
        /// </summary>
        public async Task LoadAvatarAsync(string senderId)
        {
            if (string.IsNullOrWhiteSpace(senderId)) return;
            if (picAvatar == null || picAvatar.IsDisposed) return;

            await _avatarController.LoadAvatarToPictureBoxAsync(senderId, picAvatar).ConfigureAwait(true);
        }

        #endregion

        #region ====== KHỞI TẠO & GẮN SỰ KIỆN CLICK ======

        public Conversations()
        {
            InitializeComponent();

            AutoSize = false;
            Height = 76;
            MinimumSize = new Size(0, 76);
            MaximumSize = new Size(0, 76);

            // Click toàn item: click vào label/panel/avatar đều tính là click item
            if (lblDisplayName != null) lblDisplayName.Click += OnItemClick;
            if (pnlBackground != null) pnlBackground.Click += OnItemClick;
            if (picAvatar != null) picAvatar.Click += OnItemClick;
        }

        #endregion

        #region ====== API HIỂN THỊ ======

        /// <summary>
        /// Set thông tin hiển thị (bản async - nên await).
        /// </summary>
        public async Task SetInfoAsync(string fullName, string userId)
        {
            if (lblDisplayName != null) lblDisplayName.Text = fullName ?? string.Empty;
            UserId = userId;

            await SafeLoadAvatarAsync(userId).ConfigureAwait(true);
        }

        /// <summary>
        /// Set thông tin hiển thị (bản sync để khỏi sửa chỗ gọi).
        /// Avatar sẽ tự load bất đồng bộ (không block UI).
        /// </summary>
        public void SetInfo(string fullName, string userId)
        {
            if (lblDisplayName != null) lblDisplayName.Text = fullName ?? string.Empty;
            UserId = userId;

            _ = SafeLoadAvatarAsync(userId);
        }

        #endregion

        #region ====== HỖ TRỢ NỘI BỘ ======

        private async Task SafeLoadAvatarAsync(string userId)
        {
            try
            {
                await LoadAvatarAsync(userId).ConfigureAwait(true);
            }
            catch
            {
                // Nuốt lỗi để không crash UI thread (nếu muốn bạn có thể log ra)
            }
        }

        private void OnItemClick(object sender, EventArgs e)
        {
            if (ItemClicked != null) ItemClicked(this, EventArgs.Empty);
        }

        public void ApplyTheme(bool isDark)
        {
            BackColor = isDark ? Color.FromArgb(25, 25, 25) : Color.White;
            ForeColor = isDark ? Color.White : Color.Black;
            Invalidate();
        }

        #endregion
    }
}
