using ChatApp.Helpers;
using ChatApp.Models.Users;
using ChatApp.Services.Firebase;
using ChatApp.Services.UI;
using System;
using System.Drawing;
using System.Globalization;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ChatApp.Forms
{
    public partial class ThongTinBanBe : Form
    {
        #region ====== FIELDS ======

        private readonly string _friendId;
        private User _cachedUser;

        private readonly AuthService _authService = new AuthService();
        private readonly ThemeService _themeService = new ThemeService();
        private readonly string _localId;

        #endregion

        #region ====== CTOR ======

        /// <summary>
        /// Form hiển thị thông tin bạn bè (read-only).
        /// </summary>
        public ThongTinBanBe(string friendId, User cachedUser, string localId)
        {
            InitializeComponent();

            _friendId = friendId;
            _cachedUser = cachedUser;
            _localId = localId;

            MakeReadOnly();
            HookLoad();
        }

        #endregion

        #region ====== INIT ======

        private void HookLoad()
        {
            this.Load -= ThongTinBanBe_Load;
            this.Load += ThongTinBanBe_Load;
        }

        private void MakeReadOnly()
        {
            try
            {
                txtTen.ReadOnly = true;
                txtGioiTinh.ReadOnly = true;
                txtNgaySinh.ReadOnly = true;

                txtTen.TabStop = false;
                txtGioiTinh.TabStop = false;
                txtNgaySinh.TabStop = false;
            }
            catch { }
        }

        #endregion

        #region ====== LOAD ======

        private async void ThongTinBanBe_Load(object sender, EventArgs e)
        {
            // Theme (best-effort)
            try
            {
                bool isDark = await _themeService.GetThemeAsync(_localId);
                ThemeManager.ApplyTheme(this, isDark);
            }
            catch { }

            // Fill nhanh từ cache trước
            FillFromUser(_cachedUser);

            // Rồi fetch lại từ Firebase cho “chuẩn”
            await LoadRemoteAsync();
        }

        private async Task LoadRemoteAsync()
        {
            if (string.IsNullOrWhiteSpace(_friendId)) return;

            try
            {
                // user
                User u = null;
                try { u = await _authService.GetUserByIdAsync(_friendId); } catch { u = null; }

                if (u != null)
                {
                    _cachedUser = u;
                    FillFromUser(u);
                }

                // avatar
                string base64 = null;
                try { base64 = await _authService.GetAvatarAsync(_friendId); } catch { base64 = null; }

                Image img = ImageBase64.Base64ToImage(base64);
                picAvatar.Image = img ?? Properties.Resources.DefaultAvatar;
            }
            catch
            {
                // ignore (view-only)
            }
        }

        #endregion

        #region ====== FILL UI ======

        private void FillFromUser(User u)
        {
            txtTen.Text = GetDisplayName(u, _friendId);

            txtGioiTinh.Text = GetPropString(u,"Gender");
            if (string.IsNullOrWhiteSpace(txtGioiTinh.Text)) txtGioiTinh.Text = "—";

            string ns = GetPropString(u,"BirthDay");
            txtNgaySinh.Text = NormalizeDate(ns);
            if (string.IsNullOrWhiteSpace(txtNgaySinh.Text)) txtNgaySinh.Text = "—";
        }

        private static string GetDisplayName(User u, string fallbackId)
        {
            if (u == null) return string.IsNullOrWhiteSpace(fallbackId) ? "Người dùng" : fallbackId;

            string ten = u.DisplayName;
            if (string.IsNullOrWhiteSpace(ten)) ten = u.DisplayName;

            if (string.IsNullOrWhiteSpace(ten))
            {
                string email = u.Email;
                if (!string.IsNullOrWhiteSpace(email))
                {
                    int at = email.IndexOf('@');
                    ten = (at > 0) ? email.Substring(0, at) : email;
                }
            }

            ten = string.IsNullOrWhiteSpace(ten) ? fallbackId : ten;
            return (ten ?? "Người dùng").Trim();
        }

        private static string GetPropString(object obj, params string[] names)
        {
            if (obj == null || names == null || names.Length == 0) return string.Empty;

            Type t = obj.GetType();

            for (int i = 0; i < names.Length; i++)
            {
                string n = names[i];
                if (string.IsNullOrWhiteSpace(n)) continue;

                PropertyInfo p = t.GetProperty(n, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                if (p == null) continue;

                object v = null;
                try { v = p.GetValue(obj, null); } catch { v = null; }
                if (v == null) continue;

                string s = Convert.ToString(v, CultureInfo.InvariantCulture);
                if (!string.IsNullOrWhiteSpace(s)) return s.Trim();
            }

            return string.Empty;
        }

        private static string NormalizeDate(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return string.Empty;

            raw = raw.Trim();

            // Nếu là số (timestamp) thì bỏ qua cho an toàn (tuỳ bạn muốn parse thêm)
            long tmp;
            if (long.TryParse(raw, out tmp)) return raw;

            DateTime dt;
            if (DateTime.TryParse(raw, out dt))
            {
                return dt.ToString("dd/MM/yyyy");
            }

            return raw;
        }

        #endregion

        private void btnDong_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
