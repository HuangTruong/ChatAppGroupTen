using ChatApp.Helpers.Ui;
using ChatApp.Helpers.UI;
using ChatApp.Models.Users;
using ChatApp.Services.Auth;
using ChatApp.Services.Firebase;
using FireSharp.Interfaces;
using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ChatApp
{
    /// <summary>
    /// Form Trang chủ sau khi người dùng đăng nhập thành công.
    /// Hiển thị lời chào, cho phép mở màn hình nhắn tin, cài đặt và đăng xuất.
    /// </summary>
    public partial class TrangChu : Form
    {
        #region ======== Biến / Services ========

        /// <summary>
        /// Tên hiển thị lấy từ form đăng nhập (có thể là tên người dùng).
        /// </summary>
        private readonly string _ten;

        /// <summary>
        /// Tài khoản (username) dùng làm khóa truy vấn Firebase.
        /// </summary>
        private readonly string _taiKhoan;

        /// <summary>
        /// Client Firebase dùng để lấy thông tin người dùng.
        /// </summary>
        private readonly IFirebaseClient _fbClient;

        /// <summary>
        /// Service dùng để cập nhật trạng thái online/offline.
        /// </summary>
        private readonly AuthService _authService;

        /// <summary>
        /// Form nhắn tin (chat) – được mở từ Trang chủ.
        /// </summary>
        private NhanTin _nhanTinForm;

        /// <summary>
        /// Cờ tránh bấm mở Nhắn tin nhiều lần liên tục.
        /// </summary>
        private bool _isOpeningNhanTin = false;

        /// <summary>
        /// Label hiển thị dòng chào mừng ở phần header.
        /// </summary>
        private Label _lblChaoMung;

        /// <summary>
        /// Tooltip để hiển thị đầy đủ tên người dùng khi hover vào label chào mừng.
        /// </summary>
        private readonly ToolTip _tipTen = new ToolTip();

        /// <summary>
        /// Dữ liệu user hiện tại lấy từ Firebase.
        /// </summary>
        private User _currentUser;

        #endregion

        #region ======== Khởi tạo Form ========

        /// <summary>
        /// Khởi tạo form Trang chủ với tên hiển thị và tài khoản tương ứng.
        /// </summary>
        /// <param name="ten">Tên hiển thị (từ form login).</param>
        /// <param name="taiKhoan">Tài khoản dùng để truy vấn Firebase.</param>
        public TrangChu(string ten, string taiKhoan)
        {
            InitializeComponent();

            _ten = ten;
            _taiKhoan = taiKhoan;

            _fbClient = FirebaseClientFactory.Create();
            _authService = new AuthService(_fbClient);

            TaoLabelChaoMung();

            // Căn lại label chào mừng khi header thay đổi kích thước
            LayHeaderContainer().Resize += delegate (object s, EventArgs e)
            {
                CanhGiuaChaoMung();
            };
        }

        #endregion

        #region ======== Label chào mừng ========

        /// <summary>
        /// Tạo label chào mừng và thêm vào header.
        /// </summary>
        private void TaoLabelChaoMung()
        {
            _lblChaoMung = new Label();
            _lblChaoMung.AutoSize = true;
            _lblChaoMung.Text = "Chào mừng";
            _lblChaoMung.Font = new Font("Segoe UI", 14f, FontStyle.Bold);
            _lblChaoMung.ForeColor = Color.White;
            _lblChaoMung.BackColor = Color.Transparent;
            _lblChaoMung.TextAlign = ContentAlignment.MiddleCenter;
            _lblChaoMung.Padding = new Padding(6, 0, 6, 0);

            Control header = LayHeaderContainer();
            header.Controls.Add(_lblChaoMung);

            CanhGiuaChaoMung();
        }

        /// <summary>
        /// Lấy container phần header (parent của lblTenDangNhap nếu có, ngược lại là form).
        /// </summary>
        /// <returns>Control dùng làm vùng đặt label chào mừng.</returns>
        private Control LayHeaderContainer()
        {
            if (lblTenDangNhap != null && lblTenDangNhap.Parent != null)
            {
                return lblTenDangNhap.Parent;
            }

            return this;
        }

        /// <summary>
        /// Đặt nội dung cho label chào mừng dựa trên tên hiển thị.
        /// </summary>
        /// <param name="tenHienThi">
        /// Tên muốn hiển thị; nếu rỗng sẽ fallback về _ten hoặc _taiKhoan.
        /// </param>
        private void DatNoiDungChaoMung(string tenHienThi)
        {
            string display;
            if (string.IsNullOrWhiteSpace(tenHienThi))
            {
                if (!string.IsNullOrEmpty(_ten))
                {
                    display = _ten;
                }
                else
                {
                    display = _taiKhoan;
                }
            }
            else
            {
                display = tenHienThi;
            }

            _lblChaoMung.Text = "Chào mừng '" + display + "'";
            _tipTen.SetToolTip(_lblChaoMung, display);

            if (lblTenDangNhap != null)
            {
                lblTenDangNhap.Text = string.Empty;
            }

            CanhGiuaChaoMung();
        }

        /// <summary>
        /// Căn giữa label chào mừng trong vùng header.
        /// </summary>
        private void CanhGiuaChaoMung()
        {
            if (_lblChaoMung == null)
                return;

            Control header = LayHeaderContainer();

            int newLeft = (header.ClientSize.Width - _lblChaoMung.Width) / 2;
            if (newLeft < 8)
            {
                newLeft = 8;
            }

            int newTop = (header.ClientSize.Height - _lblChaoMung.Height) / 2;
            if (newTop < 0)
            {
                newTop = 0;
            }

            _lblChaoMung.Left = newLeft;
            _lblChaoMung.Top = newTop;
        }

        #endregion

        #region ======== Load form – Lấy dữ liệu user ========

        /// <summary>
        /// Sự kiện Load của form:
        /// - Gán event cho các control Nhắn tin.
        /// - Gán event cho icon Cài đặt.
        /// - Load thông tin user từ Firebase.
        /// </summary>
        /// <param name="sender">Form Trang chủ.</param>
        /// <param name="e">Thông tin sự kiện.</param>
        private async void TrangChu_Load(object sender, EventArgs e)
        {
            // Gán event Nhắn tin cho panel, picture và label
            Control[] nhanTinTargets = new Control[] { pnlNhanTin, picNhanTin, lblNhanTin };
            foreach (Control c in nhanTinTargets)
            {
                c.Click -= pnlNhanTin_Click;          // tránh gắn trùng
                c.DoubleClick -= pnlNhanTin_Click;

                c.Click += pnlNhanTin_Click;
                c.DoubleClick += pnlNhanTin_Click;
            }

            // Gán event Cài đặt
            if (picCaiDat != null)
            {
                picCaiDat.Cursor = Cursors.Hand;

                picCaiDat.Click -= picCaiDat_Click;
                picCaiDat.Click += picCaiDat_Click;
            }

            // Load mặc định chế độ ban ngày
            ThemeManager.ApplyDayTheme(this);
            picDayNightMode.Image = Properties.Resources.Sun;

            await LoadUserFromFirebase();
        }

        /// <summary>
        /// Lấy thông tin user từ Firebase và cập nhật nội dung chào mừng.
        /// </summary>
        private async Task LoadUserFromFirebase()
        {
            try
            {
                // chỉnh path cho đúng cấu trúc DB của bạn: users/{_taiKhoan}
                var res = await _fbClient.GetAsync("users/" + _taiKhoan);
                _currentUser = res.ResultAs<User>();

                string tenHienThi;
                if (_currentUser != null && !string.IsNullOrEmpty(_currentUser.Ten))
                {
                    tenHienThi = _currentUser.Ten;
                }
                else if (!string.IsNullOrEmpty(_ten))
                {
                    tenHienThi = _ten;
                }
                else
                {
                    tenHienThi = _taiKhoan;
                }

                DatNoiDungChaoMung(tenHienThi);
            }
            catch
            {
                _currentUser = null;

                string fallback = !string.IsNullOrEmpty(_ten) ? _ten : _taiKhoan;
                DatNoiDungChaoMung(fallback);
            }
        }

        #endregion

        #region ======== Nhắn tin ========

        /// <summary>
        /// Sự kiện click / double click khu vực Nhắn tin:
        /// - Tránh mở nhiều lần nếu đang xử lý.
        /// - Nếu form Nhắn tin đang mở thì chỉ Show + Activate.
        /// - Nếu chưa có thì tạo form NhắnTin mới, gán FormClosed để quay lại Trang chủ.
        /// </summary>
        /// <param name="sender">Control được click (panel, picture hoặc label).</param>
        /// <param name="e">Thông tin sự kiện.</param>
        private void pnlNhanTin_Click(object sender, EventArgs e)
        {
            if (_isOpeningNhanTin)
                return;

            _isOpeningNhanTin = true;
            ToggleNhanTinTargets(false);

            try
            {
                // Nếu đã có form Nhắn tin thì show lại
                if (_nhanTinForm != null && !_nhanTinForm.IsDisposed)
                {
                    _nhanTinForm.WindowState = FormWindowState.Normal;
                    _nhanTinForm.Show();
                    _nhanTinForm.Activate();
                    _nhanTinForm.BringToFront();

                    this.Hide();
                    return;
                }

                // Tạo form NhắnTin mới
                _nhanTinForm = new NhanTin(_taiKhoan, _ten, ThemeManager.IsDarkMode);
                _nhanTinForm.StartPosition = FormStartPosition.CenterParent;

                _nhanTinForm.FormClosed += delegate (object s, FormClosedEventArgs args)
                {
                    _nhanTinForm = null;
                    this.Show();
                };

                _nhanTinForm.Show(this);
                this.Hide();
            }
            finally
            {
                _isOpeningNhanTin = false;
                ToggleNhanTinTargets(true);
            }
        }

        /// <summary>
        /// Bật/tắt state cho các control khu vực Nhắn tin + cursor chờ.
        /// </summary>
        /// <param name="enabled">true để enable; false để disable.</param>
        private void ToggleNhanTinTargets(bool enabled)
        {
            pnlNhanTin.Enabled = enabled;
            picNhanTin.Enabled = enabled;
            lblNhanTin.Enabled = enabled;

            this.UseWaitCursor = !enabled;
        }

        #endregion

        #region ======== Cài đặt ========

        /// <summary>
        /// Sự kiện click icon Cài đặt:
        /// - Kiểm tra đã có thông tin user chưa.
        /// - Mở form cài đặt tài khoản (CatDat) với tài khoản và email tương ứng.
        /// </summary>
        /// <param name="sender">PictureBox Cài đặt.</param>
        /// <param name="e">Thông tin sự kiện click.</param>
        private void picCaiDat_Click(object sender, EventArgs e)
        {
            if (_currentUser == null)
            {
                MessageBox.Show(
                    "Không lấy được thông tin tài khoản từ Firebase.",
                    "Lỗi",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

                return;
            }

            string taiKhoan = !string.IsNullOrEmpty(_currentUser.TaiKhoan)
                ? _currentUser.TaiKhoan
                : _taiKhoan;

            string email = _currentUser.Email ?? string.Empty;

            using (CatDat frm = new CatDat(taiKhoan, email))
            {
                frm.StartPosition = FormStartPosition.CenterParent;
                frm.ShowDialog(this);
            }
        }

        #endregion

        #region ======== Đăng xuất ========

        /// <summary>
        /// Sự kiện click icon Đăng xuất:
        /// - Nếu không có thông tin user, thông báo lỗi.
        /// - Ngược lại, đóng form (OnFormClosing sẽ lo việc cập nhật status).
        /// </summary>
        /// <param name="sender">PictureBox Đăng xuất.</param>
        /// <param name="e">Thông tin sự kiện click.</param>
        private void picDangXuat_Click(object sender, EventArgs e)
        {
            if (_currentUser == null)
            {
                MessageBox.Show(
                    "Không lấy được thông tin tài khoản từ Firebase.",
                    "Lỗi",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

                return;
            }

            this.Close();
        }

        /// <summary>
        /// Override OnFormClosing:
        /// - Đóng form Nhắn tin (nếu có) để nó hủy stream/timer.
        /// - Cập nhật trạng thái người dùng sang offline qua AuthService.
        /// </summary>
        /// <param name="e">Thông tin sự kiện đóng form.</param>
        protected override async void OnFormClosing(FormClosingEventArgs e)
        {
            // Nếu đang có form Nhắn tin thì đóng lại để nó tự dọn Firebase stream + timer
            if (_nhanTinForm != null && !_nhanTinForm.IsDisposed)
            {
                try
                {
                    _nhanTinForm.Close();
                    _nhanTinForm = null;
                }
                catch
                {
                    // Bỏ qua lỗi nếu có
                }
            }

            try
            {
                await _authService.UpdateStatusAsync(_ten, "offline");
            }
            catch
            {
                // Không làm crash app nếu lỗi mạng/Firebase
            }

            base.OnFormClosing(e);
        }

        #endregion

        #region ======== Chế độ ngày đêm ========
        private void picDayNightMode_Click(object sender, EventArgs e)
        {
            if (ThemeManager.IsDarkMode)
            {
                picDayNightMode.Image = Properties.Resources.Sun;
                ThemeManager.ApplyDayTheme(this);
            }

            else
            {
                picDayNightMode.Image = Properties.Resources.CrescentMoon;
                ThemeManager.ApplyNightTheme(this);
            }
        }
        #endregion
    }
}
