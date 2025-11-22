using System;
using System.Windows.Forms;

// Các thư viện tự tạo
using ChatApp.Controllers;

namespace ChatApp
{
    /// <summary>
    /// Form đăng nhập chính của ứng dụng ChatApp.
    /// </summary>
    /// <remarks>
    /// - Cho phép người dùng nhập tài khoản và mật khẩu để đăng nhập.
    /// - Điều hướng đến form Đăng ký khi người dùng chưa có tài khoản.
    /// - Điều hướng đến form Quên mật khẩu khi người dùng cần khôi phục tài khoản.
    /// - Gọi <see cref="LoginController"/> để xử lý logic đăng nhập.
    /// - Hỗ trợ ENTER để đăng nhập và icon con mắt để ẩn/hiện mật khẩu.
    /// </remarks>
    public partial class DangNhap : Form
    {
        #region ======== Biến / Controllers ========

        /// <summary>
        /// Controller xử lý logic đăng nhập người dùng.
        /// </summary>
        private readonly LoginController _loginController = new LoginController();

        /// <summary>
        /// Cờ theo dõi trạng thái ẩn/hiện mật khẩu.
        /// true = đang ẩn, false = đang hiện.
        /// </summary>
        private bool _isMatKhauHidden = true;

        #endregion

        #region ======== Khởi tạo Form ========

        /// <summary>
        /// Khởi tạo form <see cref="DangNhap"/>:
        /// - Đặt focus vào ô tài khoản.
        /// - Cho phép nhấn ENTER để kích hoạt nút Đăng nhập.
        /// </summary>
        public DangNhap()
        {
            InitializeComponent();

            // Khi form mở, con trỏ nằm sẵn ở ô tài khoản
            txtTaiKhoan.Focus();

            // Cho phép nhấn ENTER để kích hoạt nút Đăng nhập
            this.AcceptButton = btnDangNhap;
        }

        #endregion

        #region ======== Nút Đăng ký – Mở form Đăng ký ========

        /// <summary>
        /// Mở form <see cref="DangKy"/> khi người dùng bấm nút Đăng ký.
        /// Ẩn form đăng nhập hiện tại nhưng vẫn giữ tham chiếu để quay lại khi cần.
        /// </summary>
        /// <param name="sender">Nút Đăng ký.</param>
        /// <param name="e">Thông tin sự kiện click.</param>
        private void btnDangKy_Click(object sender, EventArgs e)
        {
            var dangKyForm = new DangKy();
            dangKyForm.Tag = this; // Giữ tham chiếu form hiện tại để quay lại sau
            dangKyForm.Show();
            this.Hide();
        }

        #endregion

        #region ======== Link Quên mật khẩu – Mở form Quên mật khẩu ========

        /// <summary>
        /// Mở form <see cref="QuenMatKhau"/> khi người dùng bấm vào link "Quên mật khẩu".
        /// Ẩn form đăng nhập hiện tại nhưng vẫn giữ tham chiếu để quay lại khi cần.
        /// </summary>
        /// <param name="sender">Link "Quên mật khẩu".</param>
        /// <param name="e">Thông tin sự kiện click link.</param>
        private void lnkQuenMatKhau_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            var quenMKForm = new QuenMatKhau();
            quenMKForm.Tag = this;
            quenMKForm.Show();
            this.Hide();
        }

        #endregion

        #region ======== Nút Đăng nhập – Xử lý đăng nhập ========

        /// <summary>
        /// Xử lý logic đăng nhập khi người dùng bấm nút Đăng nhập:
        /// - Disable nút trong lúc xử lý để tránh bấm nhiều lần.
        /// - Gọi <see cref="LoginController.DangNhapAsync(string, string)"/> để xác thực.
        /// - Nếu thành công: mở form <see cref="TrangChu"/> và reset thông tin đăng nhập.
        /// - Nếu thất bại: hiển thị thông báo lỗi, xóa mật khẩu nếu sai mật khẩu.
        /// </summary>
        /// <param name="sender">Nút Đăng nhập.</param>
        /// <param name="e">Thông tin sự kiện click.</param>
        private async void btnDangNhap_Click(object sender, EventArgs e)
        {
            if (!btnDangNhap.Enabled)
                return;

            btnDangNhap.Enabled = false;
            this.UseWaitCursor = true;

            try
            {
                // Gọi controller để đăng nhập
                var user = await _loginController.DangNhapAsync(
                    txtTaiKhoan.Text,
                    txtMatKhau.Text);

                MessageBox.Show(
                    "Đăng nhập thành công!",
                    "Thông báo",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                // Mở form TrangChu
                this.Hide();
                var home = new TrangChu(user.Ten, user.TaiKhoan);

                // Khi form TrangChu đóng thì show lại form đăng nhập
                home.FormClosed += delegate (object s, FormClosedEventArgs e2)
                {
                    this.Show();
                };

                home.Show();

                // Reset thông tin đăng nhập
                txtTaiKhoan.Clear();
                txtMatKhau.Clear();
                txtTaiKhoan.Focus();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Lỗi đăng nhập: " + ex.Message,
                    "Lỗi",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

                // Nếu sai mật khẩu -> xóa mật khẩu, giữ nguyên tài khoản
                if (ex.Message != null && ex.Message.Contains("Mật khẩu không đúng"))
                {
                    txtMatKhau.Clear();
                    txtMatKhau.Focus();
                }
            }
            finally
            {
                btnDangNhap.Enabled = true;
                this.UseWaitCursor = false;
            }
        }

        #endregion

        #region ======== Icon con mắt – Ẩn / Hiện mật khẩu ========

        /// <summary>
        /// Khi người dùng click vào icon con mắt:
        /// - Đảo trạng thái ẩn/hiện mật khẩu.
        /// - Cập nhật lại ký tự PasswordChar cho textbox mật khẩu.
        /// - Đổi icon con mắt tương ứng (ẩn/hiện).
        /// </summary>
        /// <param name="sender">Textbox mật khẩu có icon bên phải.</param>
        /// <param name="e">Thông tin sự kiện click icon.</param>
        private void txtMatKhau_IconRightClick(object sender, EventArgs e)
        {
            _isMatKhauHidden = !_isMatKhauHidden;

            txtMatKhau.PasswordChar = _isMatKhauHidden ? '●' : '\0';
            txtMatKhau.IconRight = _isMatKhauHidden
                ? Properties.Resources.AnMatKhau
                : Properties.Resources.HienMatKhau;
        }

        #endregion

        #region ======== Vòng đời form – Đóng ứng dụng ========

        /// <summary>
        /// Khi form đăng nhập bị đóng:
        /// - Gọi <see cref="Application.Exit"/> và <see cref="Environment.Exit(int)"/>
        /// để đảm bảo toàn bộ ứng dụng thoát hẳn.
        /// </summary>
        /// <param name="sender">Form đăng nhập.</param>
        /// <param name="e">Thông tin sự kiện đóng form.</param>
        private void DangNhap_FormClosed(object sender, FormClosedEventArgs e)
        {
            Application.Exit();
            Environment.Exit(0);
        }

        #endregion
    }
}
