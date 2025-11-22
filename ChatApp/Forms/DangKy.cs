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
    /// Form đăng ký tài khoản mới cho người dùng.
    /// Thực hiện:
    /// - Thu thập thông tin đăng ký
    /// - Kiểm tra hợp lệ dữ liệu
    /// - Gửi email xác thực (OTP)
    /// - Lưu user mới vào backend
    /// </summary>
    public partial class DangKy : Form
    {
        #region ======== Fields & Services ========

        /// <summary>
        /// Controller xử lý logic đăng ký (kiểm tra trùng tài khoản/email, lưu user).
        /// </summary>
        private readonly DangKyController _registerController = new DangKyController();

        /// <summary>
        /// Service gửi email xác nhận cho người dùng (SMTP).
        /// </summary>
        private readonly IEmailSender _emailSender = new SmtpEmailSender();

        /// <summary>
        /// Cờ theo dõi trạng thái ẩn/hiện mật khẩu.
        /// true = đang ẩn, false = đang hiện.
        /// </summary>
        private bool _isMatKhauHidden = true;

        #endregion

        #region ======== Khởi tạo form ========

        /// <summary>
        /// Khởi tạo form đăng ký.
        /// Thiết lập:
        /// - AcceptButton = btnDangKy để Enter kích hoạt đăng ký
        /// - KeyPreview = true để lắng nghe phím Enter trên form
        /// </summary>
        public DangKy()
        {
            InitializeComponent();

            // Nhấn Enter để kích hoạt nút Đăng ký
            this.AcceptButton = btnDangKy;
            this.KeyPreview = true;

            this.KeyDown += DangKy_KeyDown;
        }

        /// <summary>
        /// Xử lý phím Enter trên toàn form.
        /// Nếu nút Đăng ký đang Enabled thì Enter sẽ gọi btnDangKy.PerformClick().
        /// </summary>
        /// <param name="sender">Form hiện tại.</param>
        /// <param name="e">Thông tin sự kiện phím bấm.</param>
        private void DangKy_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter && btnDangKy.Enabled)
            {
                e.SuppressKeyPress = true;  // Chặn tiếng "bíp"
                btnDangKy.PerformClick();
            }
        }

        /// <summary>
        /// Sự kiện Load của form.
        /// Dùng để gán xử lý IconRightClick cho ô xác nhận mật khẩu.
        /// </summary>
        /// <param name="sender">Form đăng ký.</param>
        /// <param name="e">Thông tin sự kiện.</param>
        private void DangKy_Load(object sender, EventArgs e)
        {
            txtXacNhanMatKhau.IconRightClick += txtMatKhau_IconRightClick;
        }

        #endregion

        #region ======== Nút: Quay lại đăng nhập ========

        /// <summary>
        /// Xử lý nút "Quay lại đăng nhập".
        /// Nếu form đăng nhập cũ tồn tại (được truyền qua Tag) thì show lại,
        /// ngược lại tạo form đăng nhập mới.
        /// </summary>
        /// <param name="sender">Nút bấm.</param>
        /// <param name="e">Thông tin sự kiện click.</param>
        private void btnQuayLaiDangNhap_Click(object sender, EventArgs e)
        {
            // Nếu form đăng nhập cũ còn tồn tại thì bật lại
            Form dangNhapForm = this.Tag as Form;
            if (dangNhapForm != null && !dangNhapForm.IsDisposed)
            {
                dangNhapForm.Show();
                dangNhapForm.Activate();
                this.Close();
            }
            else
            {
                // Nếu không có -> tạo form đăng nhập mới
                var newLogin = new DangNhap();
                newLogin.Show();
                this.Close();
            }
        }

        #endregion

        #region ======== Nút: Đăng ký tài khoản ========

        /// <summary>
        /// Xử lý toàn bộ luồng đăng ký tài khoản:
        /// 1. Lấy dữ liệu từ form
        /// 2. Kiểm tra nhập thiếu / mật khẩu không khớp / email sai định dạng
        /// 3. Kiểm tra trùng tài khoản, trùng email qua controller
        /// 4. Gửi email OTP xác thực
        /// 5. Nếu xác thực thành công thì tạo User mới và lưu vào backend
        /// </summary>
        /// <param name="sender">Nút Đăng ký.</param>
        /// <param name="e">Thông tin sự kiện click.</param>
        private async void btnDangKy_Click(object sender, EventArgs e)
        {
            if (!btnDangKy.Enabled)
                return;

            // 1. Lấy dữ liệu từ form
            string taiKhoan = txtTaiKhoan.Text.Trim();
            string matKhau = txtMatKhau.Text.Trim();
            string xacNhanMk = txtXacNhanMatKhau.Text.Trim();
            string email = txtEmail.Text.Trim();
            string ten = txtTen.Text.Trim();
            DateTime ngaySinh = dtpNgaySinh.Value;
            string gioiTinh = cbbGioiTinh.Text.Trim();

            // 2. Kiểm tra thiếu dữ liệu
            if (string.IsNullOrWhiteSpace(taiKhoan) ||
                string.IsNullOrWhiteSpace(matKhau) ||
                string.IsNullOrWhiteSpace(xacNhanMk) ||
                string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(ten) ||
                string.IsNullOrWhiteSpace(gioiTinh))
            {
                MessageBox.Show(
                    "Vui lòng nhập đầy đủ Tên, Tên đăng nhập, Mật khẩu, Xác nhận mật khẩu, Email và Giới tính.",
                    "Thiếu thông tin",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                return;
            }

            // 3. Kiểm tra khớp mật khẩu
            if (!string.Equals(matKhau, xacNhanMk, StringComparison.Ordinal))
            {
                MessageBox.Show(
                    "Mật khẩu xác nhận không khớp.",
                    "Sai xác nhận mật khẩu",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                return;
            }

            // 4. Validate định dạng email
            if (!IsValidEmail(email))
            {
                MessageBox.Show(
                    "Định dạng email không hợp lệ.",
                    "Email không hợp lệ",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                return;
            }

            btnDangKy.Enabled = false;
            this.UseWaitCursor = true;

            try
            {
                // 5. Kiểm tra trùng tài khoản
                if (await _registerController.KiemTraTaiKhoanTonTaiAsync(taiKhoan))
                {
                    MessageBox.Show(
                        "Tên đăng nhập đã tồn tại. Vui lòng chọn tên khác.",
                        "Trùng tên đăng nhập",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);

                    return;
                }

                // 6. Kiểm tra trùng email
                if (await _registerController.KiemTraEmailTonTaiAsync(email))
                {
                    MessageBox.Show(
                        "Email này đã được sử dụng. Vui lòng dùng email khác.",
                        "Email đã tồn tại",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);

                    return;
                }

                // 7. Gửi OTP email
                await EmailVerificationService.SendNewCodeAsync(email, _emailSender);

                // 8. Mở form xác nhận OTP
                using (var dlg = new XacNhanEmail(email, _emailSender))
                {
                    DialogResult result = dlg.ShowDialog(this);
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

                // 9. Tạo user mới
                var newUser = new User
                {
                    TaiKhoan = taiKhoan,
                    MatKhau = matKhau,
                    Email = email,
                    Ten = ten,
                    Ngaysinh = ngaySinh.ToString("yyyy-MM-dd"),
                    Gioitinh = gioiTinh
                };

                // 10. Lưu user vào backend
                await _registerController.DangKyAsync(newUser, xacNhanMk);

                MessageBox.Show(
                    "Đăng ký thành công!",
                    "Thông báo",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                // 11. Xóa sạch form
                ResetForm();
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
                btnDangKy.Enabled = true;
                this.UseWaitCursor = false;
            }
        }

        /// <summary>
        /// Kiểm tra định dạng email bằng <see cref="System.Net.Mail.MailAddress"/>.
        /// </summary>
        /// <param name="email">Chuỗi email cần kiểm tra.</param>
        /// <returns>true nếu email hợp lệ; ngược lại false.</returns>
        private static bool IsValidEmail(string email)
        {
            try
            {
                var mailAddr = new System.Net.Mail.MailAddress(email);
                return mailAddr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Xóa toàn bộ dữ liệu trên form, reset về trạng thái ban đầu.
        /// </summary>
        private void ResetForm()
        {
            txtTen.Clear();
            txtTaiKhoan.Clear();
            txtMatKhau.Clear();
            txtXacNhanMatKhau.Clear();
            txtEmail.Clear();
            dtpNgaySinh.Value = DateTime.Today;
            cbbGioiTinh.SelectedIndex = -1;
        }

        #endregion

        #region ======== Icon mắt mật khẩu ========

        /// <summary>
        /// Xử lý click icon "mắt" để ẩn/hiện mật khẩu.
        /// Áp dụng cho cả hai ô: mật khẩu và xác nhận mật khẩu.
        /// </summary>
        /// <param name="sender">Control text box có icon.</param>
        /// <param name="e">Thông tin sự kiện click.</param>
        private void txtMatKhau_IconRightClick(object sender, EventArgs e)
        {
            _isMatKhauHidden = !_isMatKhauHidden;

            char passwordChar = _isMatKhauHidden ? '●' : '\0';
            var icon = _isMatKhauHidden
                ? Properties.Resources.AnMatKhau
                : Properties.Resources.HienMatKhau;

            // Áp dụng cho 2 ô mật khẩu
            txtMatKhau.PasswordChar = passwordChar;
            txtMatKhau.IconRight = icon;

            txtXacNhanMatKhau.PasswordChar = passwordChar;
            txtXacNhanMatKhau.IconRight = icon;
        }

        #endregion

        #region ======== Vòng đời form ========

        /// <summary>
        /// Đảm bảo thoát ứng dụng khi form đăng ký bị đóng.
        /// </summary>
        /// <param name="sender">Form đăng ký.</param>
        /// <param name="e">Thông tin sự kiện đóng form.</param>
        private void DangKy_FormClosed(object sender, FormClosedEventArgs e)
        {
            Application.Exit();
        }

        #endregion
    }
}
