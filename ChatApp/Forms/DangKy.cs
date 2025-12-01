using System;
using System.Windows.Forms;

// Thư viện tự tạo
using ChatApp.Controllers;
using ChatApp.Models.Users;
using ChatApp.Services.Email;
using ChatApp.Services.Auth;

namespace ChatApp
{
    /// <summary>
    /// Form Đăng ký:
    /// - Thu thập thông tin người dùng (họ tên, username, email, ngày sinh, giới tính, mật khẩu).
    /// - Gửi mã xác thực email và yêu cầu người dùng xác nhận.
    /// - Gọi controller để thực hiện đăng ký tài khoản mới.
    /// </summary>
    public partial class DangKy : Form
    {
        #region ====== FIELDS ======

        /// <summary>
        /// Controller xử lý logic đăng ký người dùng (kiểm tra, gọi AuthService, lưu DB,...).
        /// </summary>
        private readonly DangKyController _registerController = new DangKyController();

        /// <summary>
        /// Cờ đánh dấu người dùng bấm nút "Quay lại đăng nhập".
        /// Dùng để phân biệt với trường hợp user tắt form bằng nút X.
        /// </summary>
        private bool isBtnQuayLaiClicked = false;

        /// <summary>
        /// Cờ theo dõi trạng thái ẩn / hiện mật khẩu (cho cả 2 ô password & confirm).
        /// true  = đang ẩn mật khẩu;
        /// false = đang hiển thị mật khẩu.
        /// </summary>
        private bool isMatKhau = true;

        #endregion

        #region ====== KHỞI TẠO FORM ======

        /// <summary>
        /// Khởi tạo form Đăng ký, gán sự kiện cho nút ENTER và icon ẩn/hiện mật khẩu.
        /// </summary>
        public DangKy()
        {
            InitializeComponent();

            HideShowPassWord(); // Gán sự kiện cho icon ẩn/hiện mật khẩu (ô xác nhận)
            EnterToRegister();  // Cho phép bấm Enter để đăng ký nhanh
        }

        #endregion

        #region ====== ENTER ĐỂ ĐĂNG KÝ ======

        /// <summary>
        /// Thiết lập phím ENTER tương ứng với nút Đăng ký.
        /// </summary>
        private void EnterToRegister()
        {
            // Cho phép nhấn ENTER để kích hoạt btnRegister
            this.AcceptButton = btnRegister;
        }

        #endregion

        #region ====== NÚT QUAY LẠI TRANG ĐĂNG NHẬP ======

        /// <summary>
        /// Sự kiện nút "Quay lại đăng nhập".
        /// Đặt cờ và đóng form, để form gọi (Đăng Nhập) xử lý tiếp.
        /// </summary>
        private void btnQuayLaiDangNhap_Click(object sender, EventArgs e)
        {
            // Đánh dấu là user chủ động quay lại form đăng nhập
            isBtnQuayLaiClicked = true;
            this.Close();
        }

        #endregion

        #region ====== NÚT ĐĂNG KÝ ======

        /// <summary>
        /// Sự kiện bấm nút "Đăng ký".
        /// - Lấy dữ liệu trên form.
        /// - Gửi mã xác nhận email.
        /// - Hiển thị form xác nhận email.
        /// - Nếu xác nhận thành công thì gọi controller để đăng ký tài khoản.
        /// </summary>
        private async void btnDangKy_Click(object sender, EventArgs e)
        {
            // Nếu nút đăng ký đang bị vô hiệu hoá thì bỏ qua (tránh spam)
            if (!btnRegister.Enabled)
            {
                return;
            }

            // Lấy dữ liệu từ form
            string displayName = txtUserName.Text.Trim();
            string password = txtPassword.Text.Trim();
            string confirmPassword = txtConfirmPassword.Text.Trim();
            string email = txtEmail.Text.Trim();
            string fullName = txtFullName.Text.Trim();
            DateTime birthday = dtpBirthday.Value;
            string gender = cbbGender.Text.Trim();

            // Vô hiệu hoá nút để tránh click liên tục
            btnRegister.Enabled = false;
            this.UseWaitCursor = true;

            try
            {
                // Gửi mã xác nhận email
                await EmailVerificationService.SendNewCodeAsync(email);

                // Mở dialog xác nhận email
                using (var dlg = new XacNhanEmail(email))
                {
                    var result = dlg.ShowDialog(this);
                    if (result != DialogResult.OK)
                    {
                        MessageBox.Show(
                            "Bạn đã hủy xác nhận email. Đăng ký chưa được thực hiện.",
                            "Đã hủy",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information);

                        return;
                    }
                }

                // Tạo đối tượng User mới từ dữ liệu form
                var newUser = new User
                {
                    DisplayName = displayName,
                    FullName = fullName,
                    Email = email,
                    Birthday = birthday.ToString("yyyy-MM-dd"),
                    Gender = gender
                };

                // Gói logic đăng ký vào controller (gồm validate + gọi Firebase)
                await _registerController.DangKyAsync(newUser, password, confirmPassword);

                MessageBox.Show(
                    "Đăng ký thành công!",
                    "Thông báo",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                // Xóa dữ liệu form, đưa về trạng thái mặc định
                txtFullName.Clear();
                txtUserName.Clear();
                txtPassword.Clear();
                txtConfirmPassword.Clear();
                txtEmail.Clear();
                dtpBirthday.Value = DateTime.Today;
                cbbGender.SelectedIndex = -1;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Lỗi: " + ex.Message,
                    "Thông báo",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            finally
            {
                // Bật lại nút và tắt con trỏ chờ
                btnRegister.Enabled = true;
                this.UseWaitCursor = false;
            }
        }

        #endregion

        #region ====== ẨN / HIỆN MẬT KHẨU ======

        /// <summary>
        /// Gán sự kiện IconRightClick cho ô xác nhận mật khẩu
        /// để dùng chung logic ẩn/hiện mật khẩu.
        /// </summary>
        private void HideShowPassWord()
        {
            // Khi click icon "mắt" ở ô xác nhận mật khẩu
            txtConfirmPassword.IconRightClick += txtMatKhau_IconRightClick;
        }

        /// <summary>
        /// Sự kiện click icon “mắt” để ẩn/hiện mật khẩu.
        /// Áp dụng cho cả 2 ô: mật khẩu và xác nhận mật khẩu.
        /// </summary>
        private void txtMatKhau_IconRightClick(object sender, EventArgs e)
        {
            // Đảo trạng thái cờ: đang ẩn -> hiện, đang hiện -> ẩn
            isMatKhau = !isMatKhau;

            char passwordChar = isMatKhau ? '●' : '\0';
            var icon = isMatKhau
                ? Properties.Resources.AnMatKhau
                : Properties.Resources.HienMatKhau;

            // Áp dụng cho ô mật khẩu
            txtPassword.PasswordChar = passwordChar;
            txtPassword.IconRight = icon;

            // Áp dụng cho ô xác nhận mật khẩu
            txtConfirmPassword.PasswordChar = passwordChar;
            txtConfirmPassword.IconRight = icon;
        }

        #endregion

        #region ====== VÒNG ĐỜI FORM ======

        /// <summary>
        /// Sự kiện khi Form Đăng ký đóng.
        /// Ở đây dùng cờ isBtnQuayLaiClicked để phân biệt:
        /// - Nếu người dùng bấm "Quay lại đăng nhập": form gọi sẽ tự xử lý.
        /// - Nếu user tắt form bằng nút X, có thể tùy logic của bạn (hiện form đăng nhập / thoát app...).
        /// </summary>
        private void DangKy_FormClosed(object sender, FormClosedEventArgs e)
        {
            // Nếu không bấm nút quay lại thì xử lý riêng (tùy ý bạn mở rộng thêm).
            if (!isBtnQuayLaiClicked)
            {
                this.Close();
            }
        }

        #endregion
    }
}
