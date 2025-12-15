using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using FireSharp.Interfaces;

using ChatApp.Models.Users;
using ChatApp.Services.Firebase;
using ChatApp.Services.UI;

namespace ChatApp
{
    /// <summary>
    /// Form Trang chủ:
    /// - Là màn hình chính sau khi đăng nhập.
    /// - Điều hướng tới:
    ///   + Màn hình Nhắn tin.
    ///   + Màn hình Cài đặt tài khoản.
    ///   + Đăng xuất (cập nhật trạng thái offline).
    /// </summary>
    public partial class TrangChu : Form
    {
        #region ====== FIELDS ======

        /// <summary>
        /// Mã người dùng Firebase (localId).
        /// </summary>
        private readonly string _localId;

        /// <summary>
        /// Token đăng nhập hiện tại.
        /// </summary>
        private readonly string _token;

        /// <summary>
        /// Dịch vụ Auth để cập nhật trạng thái người dùng (online/offline).
        /// </summary>
        private readonly AuthService _authService;

        /// <summary>
        /// Dịch vụ để cập nhật chế độ ngày đêm (dark/light).
        /// </summary>
        private readonly ThemeService _themeService = new ThemeService();

        /// <summary>
        /// Form nhắn tin (màn hình chat chính).
        /// </summary>
        private NhanTin _nhanTinForm;

        /// <summary>
        /// Cờ đánh dấu đang mở form Nhắn tin (tránh double-click mở nhiều lần).
        /// </summary>
        private bool _isOpeningNhanTin = false;

        #endregion

        #region ====== KHỞI TẠO FORM ======

        /// <summary>
        /// Khởi tạo form Trang chủ với localId và token hiện tại.
        /// </summary>
        /// <param name="localId">Mã người dùng Firebase.</param>
        /// <param name="token">Token đăng nhập.</param>
        public TrangChu(string localId, string token)
        {
            InitializeComponent();

            _localId = localId;
            _token = token;

            _authService = new AuthService();

            // Chế độ ngày đêm
            _themeService = new ThemeService();
        }

        #endregion

        #region ====== LOAD FORM ======

        /// <summary>
        /// Sự kiện khi form Trang chủ được load:
        /// - Gán event click/double-click cho cụm "Nhắn tin".
        /// - Gán event click cho icon Cài đặt.
        /// - Gán event click cho icon Đăng xuất.
        /// </summary>
        private async void TrangChu_Load(object sender, EventArgs e)
        {
            // ===== Nút Nhắn tin =====
            Control[] nhanTinControls = { pnlNhanTin, picNhanTin, lblNhanTin };
            foreach (var c in nhanTinControls)
            {
                c.Cursor = Cursors.Hand;

                // Tránh gán trùng event trước đó
                c.Click -= pnlNhanTin_Click;
                c.DoubleClick -= pnlNhanTin_Click;

                c.Click += pnlNhanTin_Click;
                c.DoubleClick += pnlNhanTin_Click;
            }

            // ===== Nút Cài đặt =====
            if (picCaiDat != null)
            {
                picCaiDat.Cursor = Cursors.Hand;
                picCaiDat.Click -= picCaiDat_Click;
                picCaiDat.Click += picCaiDat_Click;
            }

            // ===== Nút Đăng xuất =====
            if (picDangXuat != null)
            {
                picDangXuat.Cursor = Cursors.Hand;
                picDangXuat.Click -= picDangXuat_Click;
                picDangXuat.Click += picDangXuat_Click;
            }

            // Nếu sau này bạn muốn: có thể load thêm thông tin user, avatar, status... ở đây
            
            // Load chế độ ngày đêm
            bool isDark = await _themeService.GetThemeAsync(_localId);
            ThemeManager.ApplyTheme(this, isDark);
            if (isDark) picDayNight.Image = Properties.Resources.Moon;
            else picDayNight.Image = Properties.Resources.Sun;
        }

        #endregion

        #region ====== NHẮN TIN ======

        /// <summary>
        /// Sự kiện click/double-click vào vùng Nhắn tin:
        /// - Mở form NhanTin (màn hình chat chính).
        /// - Ẩn form Trang chủ trong lúc chat.
        /// </summary>
        private void pnlNhanTin_Click(object sender, EventArgs e)
        {
            // Nếu đang trong quá trình mở thì bỏ qua (tránh double click)
            if (_isOpeningNhanTin)
            {
                return;
            }

            _isOpeningNhanTin = true;

            // Vô hiệu hoá vùng Nhắn tin để tránh click liên tiếp
            ToggleNhanTinTargets(false);

            try
            {
                _nhanTinForm = new NhanTin(_localId, _token);

                // Khi form Nhắn tin đóng thì hiện lại Trang chủ
                _nhanTinForm.FormClosed += (s, args) => this.Show();

                // Show form Nhắn tin, Trang chủ làm owner
                _nhanTinForm.Show(this);

                // Ẩn form Trang chủ khi đang ở màn hình Nhắn tin
                this.Hide();
            }
            finally
            {
                _isOpeningNhanTin = false;
                ToggleNhanTinTargets(true);
            }
        }

        /// <summary>
        /// Bật / tắt trạng thái enable cho cụm điều khiển Nhắn tin
        /// và hiển thị cursor chờ khi cần.
        /// </summary>
        /// <param name="enabled">true nếu cho phép click; false để khóa.</param>
        private void ToggleNhanTinTargets(bool enabled)
        {
            pnlNhanTin.Enabled = enabled;
            picNhanTin.Enabled = enabled;
            lblNhanTin.Enabled = enabled;

            // Khi đang xử lý mở form, hiển thị cursor chờ
            this.UseWaitCursor = !enabled;
        }

        #endregion

        #region ====== CÀI ĐẶT ======

        /// <summary>
        /// Sự kiện click vào icon Cài đặt:
        /// - Mở form CatDat để đổi avatar, mật khẩu, tên hiển thị.
        /// </summary>
        private void picCaiDat_Click(object sender, EventArgs e)
        {
            using (var frm = new CatDat(_localId, _token))
            {
                frm.ShowDialog(this);
            }
        }

        #endregion

        #region ====== ĐĂNG XUẤT ======

        /// <summary>
        /// Sự kiện click icon Đăng xuất:
        /// - Cập nhật trạng thái người dùng sang "offline".
        /// - Đóng form Trang chủ (có thể trả control về form Đăng nhập bên ngoài).
        /// </summary>
        private async void picDangXuat_Click(object sender, EventArgs e)
        {
            // Cập nhật trạng thái offline trước khi đóng
            await _authService.UpdateStatusAsync(_localId, "offline");

            this.Close();
        }

        #endregion

        #region ====== NIGHT MODE ======

        /// <summary>
        /// Sự kiện click icon DayNight:
        /// - Cập nhật chế độ ngày đêm (Day/Night).
        /// </summary>
        private async void picDayNight_Click(object sender, EventArgs e)
        {
            bool newMode = !ThemeManager.IsDark;
            ThemeManager.ApplyTheme(this, newMode);
            await _themeService.SaveThemeAsync(_localId, newMode);
            if (newMode) picDayNight.Image = Properties.Resources.Moon;
            else picDayNight.Image = Properties.Resources.Sun;
        }
        #endregion
    }
}
