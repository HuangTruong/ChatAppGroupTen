using System;
using System.Windows.Forms;

// Các thư viện tự tạo
using ChatApp.Controllers;

namespace ChatApp
{
    /// <summary>
    /// Form Đăng nhập:
    /// - Cho phép người dùng nhập email / mật khẩu.
    /// - Xử lý đăng nhập qua LoginController.
    /// - Điều hướng sang các form Đăng ký, Quên mật khẩu, Trang chủ.
    /// </summary>
    public partial class DangNhap : Form
    {
        #region ====== FIELDS ======

        /// <summary>
        /// Controller xử lý logic đăng nhập với Firebase / backend.
        /// </summary>
        private readonly LoginController _loginController = new LoginController();

        /// <summary>
        /// Cờ theo dõi trạng thái ẩn / hiện mật khẩu.
        /// true  = đang ẩn mật khẩu;
        /// false = đang hiển thị mật khẩu.
        /// </summary>
        private bool isMatKhau = true;

        #endregion

        #region ====== KHỞI TẠO FORM ======

        /// <summary>
        /// Khởi tạo form Đăng nhập, thiết lập focus và phím tắt ENTER.
        /// </summary>
        public DangNhap()
        {
            InitializeComponent();

            // Khi form mở, con trỏ nằm sẵn ở ô tài khoản
            txtEmail.Focus();

            // Cho phép nhấn ENTER để kích hoạt nút Đăng nhập
            this.AcceptButton = btnLogin;
        }

        #endregion

        #region ====== ĐIỀU HƯỚNG: ĐĂNG KÝ / QUÊN MẬT KHẨU ======

        /// <summary>
        /// Mở form Đăng ký khi người dùng bấm nút "Đăng ký".
        /// </summary>
        private void btnDangKy_Click(object sender, EventArgs e)
        {
            var dangKyForm = new DangKy();

            // Khi form Đăng ký đóng, hiện lại form Đăng nhập
            dangKyForm.FormClosed += (s, args) => this.Show();

            dangKyForm.Show();
            this.Hide();
        }

        /// <summary>
        /// Mở form Quên mật khẩu khi người dùng bấm link "Quên mật khẩu".
        /// </summary>
        private void lnkQuenMatKhau_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            var quenMkForm = new QuenMatKhau();

            // Khi form Quên mật khẩu đóng, hiện lại form Đăng nhập
            quenMkForm.FormClosed += (s, args) => this.Show();

            quenMkForm.Show();
            this.Hide();
        }

        #endregion

        #region ====== XỬ LÝ ĐĂNG NHẬP ======

        /// <summary>
        /// Sự kiện bấm nút Đăng nhập.
        /// Gọi LoginController để kiểm tra tài khoản, nếu thành công thì chuyển sang form Trang chủ.
        /// </summary>
        private async void btnDangNhap_Click(object sender, EventArgs e)
        {
            // Nếu nút đang bị disable (đang xử lý) thì bỏ qua
            if (!btnLogin.Enabled)
            {
                return;
            }

            // Vô hiệu hóa nút đăng nhập để tránh spam
            btnLogin.Enabled = false;
            this.UseWaitCursor = true;

            try
            {
                // Gọi controller xử lý đăng nhập (Firebase / backend)
                var (localId, token) = await _loginController.DangNhapAsync(
                    txtEmail.Text,
                    txtPassword.Text);

                MessageBox.Show("Đăng nhập thành công!", "Thông báo",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);

                // Ẩn form đăng nhập, mở form Trang chủ
                this.Hide();
                var home = new TrangChu(localId, token);

                // Khi form Trang chủ đóng, hiện lại form Đăng nhập
                home.FormClosed += (s, e2) => this.Show();
                home.Show();

                // Reset lại input
                txtEmail.Clear();
                txtPassword.Clear();
                txtEmail.Focus();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi đăng nhập: " + ex.Message,
                    "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);

                // Nếu sai mật khẩu -> xóa mật khẩu, giữ nguyên tài khoản
                if (ex.Message.Contains("Mật khẩu không đúng"))
                {
                    txtPassword.Clear();
                    txtPassword.Focus();
                }
            }
            finally
            {
                // Bật lại nút và tắt con trỏ chờ
                btnLogin.Enabled = true;
                this.UseWaitCursor = false;
            }
        }

        #endregion

        #region ====== ẨN / HIỆN MẬT KHẨU ======

        /// <summary>
        /// Sự kiện click vào icon con mắt bên phải ô mật khẩu
        /// để bật / tắt chế độ ẩn mật khẩu.
        /// </summary>
        private void txtMatKhau_IconRightClick(object sender, EventArgs e)
        {
            // Đảo trạng thái cờ
            isMatKhau = !isMatKhau;

            // Nếu isMatKhau = true -> dùng ký tự '●', ngược lại hiển thị rõ
            txtPassword.PasswordChar = isMatKhau ? '●' : '\0';

            // Thay đổi icon tương ứng (ẩn / hiện mật khẩu)
            txtPassword.IconRight = isMatKhau
                ? Properties.Resources.AnMatKhau
                : Properties.Resources.HienMatKhau;
        }

        #endregion

        #region ====== VÒNG ĐỜI FORM ======

        /// <summary>
        /// Khi form Đăng nhập bị đóng (user tắt cửa sổ),
        /// thoát toàn bộ ứng dụng.
        /// </summary>
        private void DangNhap_FormClosed(object sender, FormClosedEventArgs e)
        {
            Application.Exit();
            Environment.Exit(0);
        }

        #endregion
    }
}
