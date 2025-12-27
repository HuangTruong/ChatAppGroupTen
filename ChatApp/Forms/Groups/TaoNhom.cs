using ChatApp.Helpers;
using ChatApp.Models.Users;
using ChatApp.Services.Firebase;
using ChatApp.Services.UI;
using Guna.UI2.WinForms;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Windows.Forms;

namespace ChatApp.Forms
{
    public partial class TaoNhom : Form
    {
        #region ====== DATA ======
        /// <summary>
        /// localId của user hiện tại.
        /// </summary>
        private readonly string LocalId;

        /// <summary>
        /// Token đăng nhập (để dành nếu dùng).
        /// </summary>
        private readonly string Token;

        private readonly List<KeyValuePair<string, User>> _friends;

        public string GroupName { get; private set; }
        public List<string> SelectedMemberIds { get; private set; }

        /// <summary>
        /// Dịch vụ để cập nhật chế độ ngày đêm (dark/light).
        /// </summary>
        private readonly ThemeService _themeService = new ThemeService();

        private  readonly GroupService _groupService = new GroupService();
        public string GroupAvatarBase64 { get; private set; }
        #endregion

        #region ====== CTOR ======

        public TaoNhom(Dictionary<string, User> friends, string localId, string token)
        {
            InitializeComponent();

            LocalId = localId;
            Token = token;

            _friends = (friends ?? new Dictionary<string, User>())
                .Where(x => x.Value != null)
                .OrderBy(x => x.Value.DisplayName ?? x.Value.UserName ?? x.Value.Email ?? "")
                .ToList();

            SelectedMemberIds = new List<string>();
            GroupAvatarBase64 = string.Empty;

            LoadAvatar();
            LoadFriends();
            LoadTheme();
        }

        #endregion

        #region ====== LOAD DATA ======

        private async void LoadTheme()
        {
            // Load chế độ ngày đêm
            bool isDark = await _themeService.GetThemeAsync(LocalId);
            ThemeManager.ApplyTheme(this, isDark);
        }
        private async void LoadAvatar()
        {
            string anh = await _groupService.GetAvatarGroupAsync(LocalId);
            picAvatarPreview.Image = ImageBase64.Base64ToImage (anh);
        }
        private void LoadFriends()
        {
            pnlMembers.Controls.Clear();

            int y = 12;

            foreach (var kv in _friends)
            {
                string id = kv.Key;
                User u = kv.Value;

                var chk = new Guna2CheckBox
                {
                    Text = GetName(u, id),
                    Tag = id,                // lưu localId
                    AutoSize = true,
                    Dock = DockStyle.Top,
                    Font = new Font("Segoe UI", 10F),
                    Cursor = Cursors.Hand,
                };

                pnlMembers.Controls.Add(chk);
                y += chk.Height + 12;
            }
        }

        private static string GetName(User u, string id)
        {
            if (u == null) return id ?? "Người dùng";

            return (u.DisplayName
                ?? u.UserName
                ?? u.Email
                ?? id).Trim();
        }

        #endregion

        #region ====== EVENTS ======

        private void btnTao_Click(object sender, EventArgs e)
        {
            string ten = txtTenNhom.Text.Trim();
            if (string.IsNullOrEmpty(ten))
            {
                MessageBox.Show("Vui lòng nhập tên nhóm.");
                txtTenNhom.Focus();
                return;
            }

            var selected = pnlMembers.Controls
            .OfType<Guna2CheckBox>()
            .Where(c => c.Checked)
            .Select(c => c.Tag?.ToString())
            .Where(id => !string.IsNullOrEmpty(id))
            .ToList();

            //var selected = new List<string>();

            //foreach (var obj in clbThanhVien.CheckedItems)
            //{
            //    if (obj is Item it && !string.IsNullOrEmpty(it.Id))
            //        selected.Add(it.Id);
            //}

            if (selected.Count == 0)
            {
                MessageBox.Show("Chọn ít nhất 1 thành viên.");
                return;
            }

            GroupName = ten;
            SelectedMemberIds = selected;

            DialogResult = DialogResult.OK;
            Close();
        }

        private void btnHuy_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        #endregion

        #region ====== INTERNAL ITEM ======

        private class Item
        {
            public string Id;
            public string Text;
            public override string ToString() => Text;
        }

        #endregion

        private void btnAddAvatar_Click(object sender, EventArgs e)
        {
            try
            {
                using (OpenFileDialog dlg = new OpenFileDialog())
                {
                    dlg.Title = "Chọn avatar nhóm";
                    dlg.Filter = "Ảnh (*.png;*.jpg;*.jpeg;*.bmp;*.gif)|*.png;*.jpg;*.jpeg;*.bmp;*.gif";
                    dlg.Multiselect = false;

                    if (dlg.ShowDialog(this) != DialogResult.OK) return;

                    string base64 = TryLoadAndResizeImageAsBase64(dlg.FileName, 256);
                    if (string.IsNullOrWhiteSpace(base64))
                    {
                        MessageBox.Show("Không đọc được ảnh. Vui lòng thử ảnh khác.");
                        return;
                    }

                    GroupAvatarBase64 = base64;
                    btnAddAvatar.Text = "Đã chọn Avatar";
                }
            }
            catch
            {
                MessageBox.Show("Có lỗi khi chọn avatar. Vui lòng thử lại.");
            }
        }

        private static string TryLoadAndResizeImageAsBase64(string filePath, int maxSize)
        {
            if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath)) return null;

            using (Image img = Image.FromFile(filePath))
            {
                int w = img.Width;
                int h = img.Height;
                if (w <= 0 || h <= 0) return null;

                float scale = 1f;
                if (w > maxSize || h > maxSize)
                {
                    float sw = (float)maxSize / (float)w;
                    float sh = (float)maxSize / (float)h;
                    scale = sw < sh ? sw : sh;
                }

                int nw = (int)Math.Max(1, Math.Round(w * scale));
                int nh = (int)Math.Max(1, Math.Round(h * scale));

                using (Bitmap bmp = new Bitmap(nw, nh))
                {
                    using (Graphics g = Graphics.FromImage(bmp))
                    {
                        g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                        g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
                        g.DrawImage(img, 0, 0, nw, nh);
                    }

                    using (MemoryStream ms = new MemoryStream())
                    {
                        bmp.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                        return Convert.ToBase64String(ms.ToArray());
                    }
                }
            }
        }

    }
}
