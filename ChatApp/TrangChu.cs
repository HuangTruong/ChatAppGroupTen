using FireSharp.Config;
using FireSharp.Interfaces;
using FireSharp.Response;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Runtime.CompilerServices.RuntimeHelpers;

namespace ChatApp
{
    public partial class TrangChu : Form
    {

        // Biến để kết nối tới Firebase
        private IFirebaseClient firebaseClient;

        // [ADDED] chỉ mở 1 cửa sổ NhanTin
        private NhanTin _nhanTinForm;
        // [ADDED] chống double-click spam
        private bool _isOpeningNhanTin = false;
        private readonly string _ten;
        public TrangChu(string ten)
        {
            InitializeComponent();
            // Cấu hình Firebase (gồm AuthSecret và BasePath)
            IFirebaseConfig config = new FirebaseConfig
            {
                // AuthSecret là khóa bí mật để xác thực kết nối tới database (Copy trong dự án firebase)
                AuthSecret = "RBVYwGVpeA360cuFw7YcoiPKAf07ZpFHrZma2mx0",

                // BasePath là đường dẫn đến Realtime Database trên Firebase
                BasePath = "https://fir-client-1d344-default-rtdb.firebaseio.com/"
            };

            // Tạo đối tượng FirebaseClient để làm việc với database
            firebaseClient = new FireSharp.FirebaseClient(config);
            _ten = ten;
        }

        // Nhấn vào panel + picturebox + label Nhan Tin
        private void pnlNhanTin_Click(object sender, EventArgs e)
        {
            // [ADDED] nếu đang mở thì bỏ qua
            if (_isOpeningNhanTin) return;
            _isOpeningNhanTin = true;

            try
            {
                // [ADDED] nếu đã có thì bring-to-front thay vì tạo mới
                if (_nhanTinForm != null && !_nhanTinForm.IsDisposed)
                {
                    _nhanTinForm.WindowState = FormWindowState.Normal;
                    _nhanTinForm.Show();       // đảm bảo visible
                    _nhanTinForm.Activate();   // focus
                    _nhanTinForm.BringToFront();
                    return;
                }

                // [ADDED] tạo mới, gắn Owner để quản lý vòng đời
                _nhanTinForm = new NhanTin();
                _nhanTinForm.StartPosition = FormStartPosition.CenterParent;
                _nhanTinForm.FormClosed += (s, args) =>
                {
                    // [ADDED] khi đóng thì cho phép tạo lại
                    _nhanTinForm = null;
                };

                // [ADDED] trong lúc show, tạm khóa click để khỏi spam
                ToggleNhanTinTargets(false);
                _nhanTinForm.Show(this);
            }
            finally
            {
                // [ADDED] mở lại click
                _isOpeningNhanTin = false;
                ToggleNhanTinTargets(true);
            }
        }

        // [ADDED] tiện ích bật/tắt click trên 3 control
        private void ToggleNhanTinTargets(bool enabled)
        {
            try
            {
                pnlNhanTin.Enabled = enabled;
                picNhanTin.Enabled = enabled;
                lblNhanTin.Enabled = enabled;
                this.UseWaitCursor = !enabled;
            }
            catch { /* an toàn */ }
        }

        // Gắn sự kiện cho cả panel, picturebox, label
        private async void TrangChu_Load(object sender, EventArgs e)
        {
            // [NOTE] Load chạy 1 lần/instance, nên attach ở đây là ổn
            pnlNhanTin.Click += pnlNhanTin_Click;
            picNhanTin.Click += pnlNhanTin_Click;
            lblNhanTin.Click += pnlNhanTin_Click;

            // [ADDED] cũng bắt luôn double-click nếu có
            pnlNhanTin.DoubleClick += pnlNhanTin_Click;
            picNhanTin.DoubleClick += pnlNhanTin_Click;
            lblNhanTin.DoubleClick += pnlNhanTin_Click;

            // ---- HIỂN THỊ TÊN ĐĂNG NHẬP Ở HEADER ----
            if (firebaseClient == null)
            {
                lblTenDangNhap.Text = _ten; // fallback
                return;
            }

            try
            {
                // đổi key cho hợp lệ (tránh . # $ [ ] /)
                var tkkey = ChuyenKeyHopLe(_ten);

                // đọc users/{taiKhoan}
                FirebaseResponse resp = await firebaseClient.GetAsync($"users/{tkkey}");
                var user = resp.ResultAs<UserTrangChu>(); // map theo class

                // Ưu tiên tên (Ten), nếu trống dùng tài khoản
                string display = (!string.IsNullOrWhiteSpace(user?.Ten))
                    ? user.Ten
                    : _ten;

                lblTenDangNhap.Text = display;
            }
            catch (Exception ex)
            {
                // tránh bể UI khi lỗi mạng/không có node
                lblTenDangNhap.Text = _ten ;
                // bạn có thể log ra nếu muốn:
                // MessageBox.Show("Không lấy được tên người dùng: " + ex.Message);
            }

        }

        private void guna2GradientPanel1_Paint(object sender, PaintEventArgs e)
        {

        }

        // Chuyển ký tự đặc biệt trong tài khoản thành dạng hợp lệ với Firebase
        private static string ChuyenKeyHopLe(string text)
        {
            return Regex.Replace(text, @"[.#$\[\]/]", "_");
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
