using FireSharp.Config;
using FireSharp.Interfaces;
using FireSharp.Response;
using System;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ChatApp
{
    public partial class TrangChu : Form
    {
        // Firebase
        private IFirebaseClient firebaseClient;

        // Chỉ mở 1 cửa sổ NhanTin
        private NhanTin _nhanTinForm;
        private bool _isOpeningNhanTin = false;
        private readonly string _ten;

        // Label chào mừng (đặt giữa header)
        private Label lblChaoMung;

        // Tooltip (nếu muốn hover hiện tên đầy đủ)
        private readonly ToolTip _tipTen = new ToolTip();

        public TrangChu(string ten)
        {
            InitializeComponent();

            // Tạo label "Chào mừng ..."
            TaoLabelChaoMung();

            // Lắng nghe resize để luôn giữ giữa header
            Control header = LayHeaderContainer();
            header.Resize += (s, e) => CanhGiuaChaoMung();

            // Firebase
            IFirebaseConfig config = new FirebaseConfig
            {
                AuthSecret = "j0kBCfIQBOBtgq5j0RaocJLgCuJO1AMn2GS5qXqH",
                BasePath = "https://chatapp-ca701-default-rtdb.asia-southeast1.firebasedatabase.app/"
            };
            firebaseClient = new FireSharp.FirebaseClient(config);
            _ten = ten;
        }

        // ===== Khởi tạo label chào mừng & canh giữa =====
        private void TaoLabelChaoMung()
        {
            // Ẩn label cũ để không bị lặp hiển thị
            if (lblTenDangNhap != null) lblTenDangNhap.Visible = false;

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
            // Đặt label vào cùng container với lblTenDangNhap (thường là panel header xanh)
            // nếu không có thì dùng Form làm container.
            return (lblTenDangNhap != null && lblTenDangNhap.Parent != null) ? (Control)lblTenDangNhap.Parent : this;
        }

        private void DatNoiDungChaoMung(string tenHienThi)
        {
            string display = string.IsNullOrWhiteSpace(tenHienThi) ? _ten : tenHienThi;
            lblChaoMung.Text = "Chào mừng '" + display + "'";
            _tipTen.SetToolTip(lblChaoMung, display);
            CanhGiuaChaoMung();
        }

        private void CanhGiuaChaoMung()
        {
            if (lblChaoMung == null) return;
            Control header = LayHeaderContainer();

            // đo lại kích thước text để canh giữa chính xác
            Size sz = TextRenderer.MeasureText(lblChaoMung.Text, lblChaoMung.Font);
            lblChaoMung.Size = sz;

            int left = (header.ClientSize.Width - lblChaoMung.Width) / 2;
            int top = (header.ClientSize.Height - lblChaoMung.Height) / 2;

            if (left < 8) left = 8;
            if (top < 0) top = 0;

            lblChaoMung.Left = left;
            lblChaoMung.Top = top;
        }

        // ===== Mở form nhắn tin =====
        private void pnlNhanTin_Click(object sender, EventArgs e)
        {
            if (_isOpeningNhanTin) return;
            _isOpeningNhanTin = true;

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

                _nhanTinForm = new NhanTin(_ten);
                _nhanTinForm.StartPosition = FormStartPosition.CenterParent;
                _nhanTinForm.FormClosed += (s, args) =>
                {
                    _nhanTinForm = null;
                    this.Show();
                };

                ToggleNhanTinTargets(false);
                _nhanTinForm.Show(this);
                this.Hide();
            }
            finally
            {
                _isOpeningNhanTin = false;
                ToggleNhanTinTargets(true);
            }
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            CapNhatTrangThai("offline");
            base.OnFormClosed(e);
            Application.Exit();
        }

        private void ToggleNhanTinTargets(bool enabled)
        {
            try
            {
                pnlNhanTin.Enabled = enabled;
                picNhanTin.Enabled = enabled;
                lblNhanTin.Enabled = enabled;
                this.UseWaitCursor = !enabled;
            }
            catch { }
        }

        // ===== Load form =====
        private async void TrangChu_Load(object sender, EventArgs e)
        {
            // Gắn click/ double-click
            pnlNhanTin.Click += pnlNhanTin_Click;
            picNhanTin.Click += pnlNhanTin_Click;
            lblNhanTin.Click += pnlNhanTin_Click;

            pnlNhanTin.DoubleClick += pnlNhanTin_Click;
            picNhanTin.DoubleClick += pnlNhanTin_Click;
            lblNhanTin.DoubleClick += pnlNhanTin_Click;

            // Load tên hiển thị và set chào mừng
            if (firebaseClient == null)
            {
                DatNoiDungChaoMung(_ten);
                return;
            }

            try
            {
                string tkkey = ChuyenKeyHopLe(_ten);
                FirebaseResponse resp = await firebaseClient.GetAsync("users/" + tkkey);
                var user = resp.ResultAs<UserTrangChu>();

                string display = (!string.IsNullOrWhiteSpace(user?.Ten)) ? user.Ten : _ten;
                DatNoiDungChaoMung(display);
            }
            catch
            {
                DatNoiDungChaoMung(_ten);
            }
        }

        // ===== Tiện ích =====
        private static string ChuyenKeyHopLe(string text)
        {
            return Regex.Replace(text, @"[.#$\[\]/]", "_");
        }

        private void picDangXuat_Click(object sender, EventArgs e)
        {
            if (_nhanTinForm != null && !_nhanTinForm.IsDisposed)
            {
                _nhanTinForm.Close();
                _nhanTinForm = null;
            }

            CapNhatTrangThai("offline");
            this.Hide();
            var loginForm = new DangNhap();
            loginForm.FormClosed += (s, args) => this.Close();
            loginForm.Show();
        }

        private async Task CapNhatTrangThai(string trangThai)
        {
            try
            {
                await firebaseClient.SetAsync("status/" + _ten, trangThai);
            }
            catch { }
        }
    }

    public class UserTrangChu
    {
        public string TaiKhoan { get; set; }
        public string MatKhau { get; set; }
        public string Email { get; set; }
        public string Ten { get; set; }
        public string NgaySinh { get; set; }
        public string GioiTinh { get; set; }
    }
}
