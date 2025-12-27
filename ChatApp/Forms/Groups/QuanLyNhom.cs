using ChatApp.Helpers;
using ChatApp.Models.Users;
using ChatApp.Services.Firebase;
using ChatApp.Services.UI;
using Guna.UI2.WinForms;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace ChatApp.Forms
{
    public partial class QuanLyNhom : Form
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

        /// <summary>
        /// groupId của nhóm đang quản lý.
        /// </summary>
        private readonly string _groupId;

        private readonly List<KeyValuePair<string, User>> _friends;

        public string GroupName { get; private set; }
        public List<string> SelectedMemberIds { get; private set; }

        /// <summary>
        /// Dịch vụ để cập nhật chế độ ngày đêm (dark/light).
        /// </summary>
        private readonly ThemeService _themeService = new ThemeService();

        /// <summary>
        /// Service nhóm: đổi tên + thêm thành viên.
        /// </summary>
        private readonly GroupService _groupService = new GroupService();

        public string GroupAvatarBase64 { get; private set; }

        #endregion

        #region ====== CONSTRUCTOR ======
        // NEW: thêm groupId vào constructor (giữ code bạn, chỉ thêm tham số)
        public QuanLyNhom(Dictionary<string, User> friends, string groupId, string localId, string token)
        {
            InitializeComponent();

            _groupId = groupId;
            LocalId = localId;
            Token = token;

            _friends = (friends ?? new Dictionary<string, User>())
                .Where(x => x.Value != null)
                .OrderBy(x => x.Value.DisplayName ?? x.Value.UserName ?? x.Value.Email ?? "")
                .ToList();

            SelectedMemberIds = new List<string>();
            GroupAvatarBase64 = string.Empty;

            // load avatar
            LoadAvatar();
            // load friends sau khi form đã tạo xong
            LoadFriendsAsync();
            //Load theme +load list(có lọc member)
            LoadTheme();
        }
        #endregion

        #region ====== LOAD DATA ======

        private async void LoadTheme()
        {
            bool isDark = await _themeService.GetThemeAsync(LocalId);
            ThemeManager.ApplyTheme(this, isDark);
        }


        private async void LoadAvatar()
        {
            string anh = await _groupService.GetAvatarGroupAsync(_groupId);
            picAvatarPreview.Image = ImageBase64.Base64ToImage(anh);
        }
        // async để lấy member map -> disable những người đã trong nhóm
        private async Task LoadFriendsAsync()
        {
            pnlMembers.Controls.Clear();

            Dictionary<string, bool> memberMap = new Dictionary<string, bool>();
            try
            {
                // groups/{gid}/members
                memberMap = await _groupService.GetMemberMapAsync(_groupId, Token);
                if (memberMap == null) memberMap = new Dictionary<string, bool>();
            }
            catch
            {
                memberMap = new Dictionary<string, bool>();
            }

            foreach (var kv in _friends)
            {
                string id = kv.Key;
                User u = kv.Value;

                bool existed = false;
                if (memberMap != null) memberMap.TryGetValue(id, out existed);

                var chk = new Guna2CheckBox
                {
                    Text = GetName(u, id) + (existed ? " (đã trong nhóm)" : ""),
                    Tag = id,                // lưu localId
                    AutoSize = true,
                    Dock = DockStyle.Top,
                    Font = new Font("Segoe UI", 10F),
                    Enabled = !existed,
                    Cursor = Cursors.Hand
                };

                pnlMembers.Controls.Add(chk);
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

        // NEW: Thêm thành viên
        private async void btnTao_Click(object sender, EventArgs e)
        {
            try
            {
                SelectedMemberIds.Clear();

                foreach (Control c in pnlMembers.Controls)
                {
                    Guna2CheckBox chk = c as Guna2CheckBox;
                    if (chk == null) continue;

                    if (chk.Enabled && chk.Checked)
                    {
                        string id = chk.Tag as string;
                        if (!string.IsNullOrWhiteSpace(id))
                        {
                            SelectedMemberIds.Add(id);
                        }
                    }
                }

                if (SelectedMemberIds.Count == 0)
                {
                    MessageBox.Show("Chọn ít nhất 1 người để thêm.");
                    return;
                }

                int added = await _groupService.AddMembersAsync(_groupId, SelectedMemberIds, Token);
                MessageBox.Show("Đã thêm " + added + " thành viên.");

                // reload danh sách (disable những người vừa thêm)
                await LoadFriendsAsync();

                DialogResult = DialogResult.OK;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Thêm thành viên thất bại: " + ex.Message);
            }
        }

        // NEW: Đổi tên nhóm
        private async void btnDoiTenNhom_Click(object sender, EventArgs e)
        {
            string ten = (txtTenNhom.Text ?? "").Trim();
            if (string.IsNullOrEmpty(ten))
            {
                MessageBox.Show("Vui lòng nhập tên nhóm.");
                txtTenNhom.Focus();
                return;
            }

            try
            {
                await _groupService.UpdateGroupNameAsync(_groupId, ten, Token);
                GroupName = ten;

                MessageBox.Show("Đổi tên nhóm thành công.");
                DialogResult = DialogResult.OK;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Đổi tên nhóm thất bại: " + ex.Message);
            }
        }

        private async void btnDoiAvatar_Click(object sender, EventArgs e)
        {
            try
            {
                using (OpenFileDialog dlg = new OpenFileDialog())
                {
                    dlg.Title = "Chọn avatar nhóm";
                    dlg.Filter = "Ảnh (*.png;*.jpg;*.jpeg;*.bmp;*.gif)|*.png;*.jpg;*.jpeg;*.bmp;*.gif";
                    dlg.Multiselect = false;

                    if (dlg.ShowDialog(this) != DialogResult.OK)
                    {
                        return;
                    }

                    string base64 = TryLoadAndResizeImageAsBase64(dlg.FileName, 256);
                    if (string.IsNullOrWhiteSpace(base64))
                    {
                        MessageBox.Show("Không đọc được ảnh. Vui lòng thử ảnh khác.");
                        return;
                    }

                    // Lưu property để form cha có thể dùng nếu cần
                    GroupAvatarBase64 = base64;

                    // Ghi Firebase: groups/{groupId}/avatar
                    await _groupService.UpdateAvatarAsync(_groupId, base64, Token);

                    MessageBox.Show("Đổi avatar nhóm thành công.");
                    DialogResult = DialogResult.OK;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Đổi avatar nhóm thất bại: " + ex.Message);
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
                        g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                        g.SmoothingMode = SmoothingMode.HighQuality;
                        g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                        g.DrawImage(img, 0, 0, nw, nh);
                    }

                    using (MemoryStream ms = new MemoryStream())
                    {
                        bmp.Save(ms, ImageFormat.Png);
                        return Convert.ToBase64String(ms.ToArray());
                    }
                }
            }
        }

    }
}
