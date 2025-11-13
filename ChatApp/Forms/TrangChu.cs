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
    public partial class TrangChu : Form
    {
        private readonly string _ten;          // Tên hiển thị (từ form login)
        private readonly string _taiKhoan;     // Khóa để query Firebase
        private readonly AuthService _authService;


        private readonly IFirebaseClient _fbClient;

        private NhanTin _nhanTinForm;
        private bool _isOpeningNhanTin = false;

        private Label lblChaoMung;
        private readonly ToolTip _tipTen = new ToolTip();

        private User _currentUser;             // dữ liệu user lấy từ Firebase

        public TrangChu(string ten, string taiKhoan)
        {
            InitializeComponent();

            _ten = ten;
            _taiKhoan = taiKhoan;
            _fbClient = FirebaseClientFactory.Create();
            _authService = new AuthService(_fbClient);


            TaoLabelChaoMung();
            LayHeaderContainer().Resize += (s, e) => CanhGiuaChaoMung();
        }

        #region Label chào mừng

        private void TaoLabelChaoMung()
        {
            lblChaoMung = new Label
            {
                AutoSize = true,
                Text = "Chào mừng",
                Font = new Font("Segoe UI", 14f, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.Transparent,
                TextAlign = ContentAlignment.MiddleCenter,
                Padding = new Padding(6, 0, 6, 0)
            };

            var header = LayHeaderContainer();
            header.Controls.Add(lblChaoMung);
            CanhGiuaChaoMung();
        }

        private Control LayHeaderContainer()
        {
            return (lblTenDangNhap != null && lblTenDangNhap.Parent != null)
                ? (Control)lblTenDangNhap.Parent
                : this;
        }

        private void DatNoiDungChaoMung(string tenHienThi)
        {
            string display = string.IsNullOrWhiteSpace(tenHienThi)
                ? (_ten ?? _taiKhoan)
                : tenHienThi;

            lblChaoMung.Text = $"Chào mừng '{display}'";
            _tipTen.SetToolTip(lblChaoMung, display);

            if (lblTenDangNhap != null)
                lblTenDangNhap.Text = string.Empty;

            CanhGiuaChaoMung();
        }

        private void CanhGiuaChaoMung()
        {
            if (lblChaoMung == null) return;

            Control header = LayHeaderContainer();
            lblChaoMung.Left = Math.Max((header.ClientSize.Width - lblChaoMung.Width) / 2, 8);
            lblChaoMung.Top = Math.Max((header.ClientSize.Height - lblChaoMung.Height) / 2, 0);
        }

        #endregion

        #region Load form

        private async void TrangChu_Load(object sender, EventArgs e)
        {
            // Gán event Nhắn tin
            foreach (Control c in new Control[] { pnlNhanTin, picNhanTin, lblNhanTin })
            {
                c.Click += pnlNhanTin_Click;
                c.DoubleClick += pnlNhanTin_Click;
            }

            // Gán event Cài đặt
            if (picCaiDat != null)
            {
                picCaiDat.Cursor = Cursors.Hand;
                picCaiDat.Click += picCaiDat_Click;
            }

            await LoadUserFromFirebase();
        }

        private async Task LoadUserFromFirebase()
        {
            try
            {
                // chỉnh path cho đúng cấu trúc DB của bạn
                var res = await _fbClient.GetAsync($"users/{_taiKhoan}");
                _currentUser = res.ResultAs<User>();

                DatNoiDungChaoMung(_currentUser?.Ten ?? _ten ?? _taiKhoan);
            }
            catch
            {
                _currentUser = null;
                DatNoiDungChaoMung(_ten ?? _taiKhoan);
            }
        }

        #endregion

        #region Nhắn tin

        private void pnlNhanTin_Click(object sender, EventArgs e)
        {
            if (_isOpeningNhanTin) return;
            _isOpeningNhanTin = true;
            ToggleNhanTinTargets(false);

            try
            {
                if (_nhanTinForm != null && !_nhanTinForm.IsDisposed)
                {
                    _nhanTinForm.WindowState = FormWindowState.Normal;
                    _nhanTinForm.Show();
                    _nhanTinForm.Activate();
                    _nhanTinForm.BringToFront();
                    this.Hide();
                    return;
                }

                _nhanTinForm = new NhanTin(_taiKhoan, _ten) // truyền taiKhoan nếu form chat cần
                {
                    StartPosition = FormStartPosition.CenterParent
                };

                _nhanTinForm.FormClosed += (s, args) =>
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

        private void ToggleNhanTinTargets(bool enabled)
        {
            pnlNhanTin.Enabled = enabled;
            picNhanTin.Enabled = enabled;
            lblNhanTin.Enabled = enabled;
            this.UseWaitCursor = !enabled;
        }

        #endregion

        #region Cài đặt

        private void picCaiDat_Click(object sender, EventArgs e)
        {
            if (_currentUser == null)
            {
                MessageBox.Show("Không lấy được thông tin tài khoản từ Firebase.",
                    "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string taiKhoan = _currentUser.TaiKhoan ?? _taiKhoan;
            string email = _currentUser.Email ?? string.Empty;

            using (var frm = new CatDat(taiKhoan, email))
            {
                frm.StartPosition = FormStartPosition.CenterParent;
                frm.ShowDialog(this);
            }
        }

        #endregion

        #region Đăng xuất

        private void picDangXuat_Click(object sender, EventArgs e)
        {
            if (_nhanTinForm != null && !_nhanTinForm.IsDisposed)
            {
                _nhanTinForm.Close();
                _nhanTinForm = null;
            }

            this.Close();
        }
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

            // Cập nhật trạng thái OFFLINE lên Firebase
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
    }
}
