using System;
using System.Windows.Forms;

// Thư viện tự tạo
using ChatApp.Controllers;
using ChatApp.Models.Users;
using ChatApp.Services.Email;
using ChatApp.Services.Auth;

namespace ChatApp
{
    public partial class DangKy : Form
    {
        // Controller xử lý logic đăng ký người dùng
        private readonly DangKyController _registerController = new DangKyController();

        public DangKy()
        {
            InitializeComponent();

            HideShowPassWord(); // Gán thêm sự kiện cho ẩn hiện mật khẩu cho nút xác nhận
            EnterToRegister();  // Bấm Enter để đăng ký nhanh không cần bấm nút.
        }

        #region ====== SỰ KIỆN BẤM NÚT ENTER ======
        private void EnterToRegister()
        {
            // Cho phép nhấn ENTER để đăng ký
            this.AcceptButton = btnRegister;
        }
        #endregion

        #region ====== SỰ KIỆN NÚT QUAY LẠI TRANG ĐĂNG NHẬP ======
        // Nút “Quay lại đăng nhập”
        private void btnQuayLaiDangNhap_Click(object sender, EventArgs e)
        {
            // Nếu form đăng nhập cũ còn tồn tại thì bật lại
            this.Close();
        }
        #endregion

        // Nút “Đăng ký”
        private async void btnDangKy_Click(object sender, EventArgs e)
        {
            if (!btnRegister.Enabled) return;

            // Lấy dữ liệu từ form
            var taiKhoan = txtUserName.Text.Trim();
            var matKhau = txtPassword.Text.Trim();
            var xacNhanMk = txtConfirmPassword.Text.Trim();
            var email = txtEmail.Text.Trim();
            var ten = txtFullName.Text.Trim();
            var ngaySinh = dtpBirthday.Value;
            var gioiTinh = cbbGender.Text.Trim();

            // Vô hiệu hoá nút để tránh spam click
            btnRegister.Enabled = false;
            this.UseWaitCursor = true;

            try
            {
                // Gửi xác nhận mail
                await EmailVerificationService.SendNewCodeAsync(email);

                using (var dlg = new XacNhanEmail(email))
                {
                    var result = dlg.ShowDialog(this);
                    if (result != DialogResult.OK)
                    {
                        MessageBox.Show("Bạn đã hủy xác nhận email. Đăng ký chưa được thực hiện.",
                            "Đã hủy", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }
                }

                var newUser = new User
                {
                    TaiKhoan = taiKhoan,
                    MatKhau = matKhau,
                    Email = email,
                    Ten = ten,
                    Ngaysinh = ngaySinh.ToString("yyyy-MM-dd"),
                    Gioitinh = gioiTinh
                };

                await _registerController.DangKyAsync(newUser, xacNhanMk);

                MessageBox.Show("Đăng ký thành công!", "Thông báo",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);

                // clear
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
                MessageBox.Show("Lỗi: " + ex.Message, "Thông báo",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnRegister.Enabled = true;
                this.UseWaitCursor = false;
            }
        }

        #region ====== ẨN HIỂN MẬT KHẨU ======
        // Biến theo dõi trạng thái ẩn/hiện mật khẩu
        private bool isMatKhau = true;
        // Gán thêm sự kiện cho ẩn hiện mật khẩu cho nút xác nhận
        private void HideShowPassWord()
        {
            // Khi click icon "mắt" ở ô xác nhận mật khẩu
            txtConfirmPassword.IconRightClick += txtMatKhau_IconRightClick;
        }
        // Sự kiện click icon “mắt” để ẩn/hiện mật khẩu
        private void txtMatKhau_IconRightClick(object sender, EventArgs e)
        {
            isMatKhau = !isMatKhau;

            // Đang ẩn -> hiện, đang hiện -> ẩn
            char c = isMatKhau ? '●' : '\0';
            var icon = isMatKhau
                ? Properties.Resources.AnMatKhau
                : Properties.Resources.HienMatKhau;

            // Áp dụng cho cả 2 ô mật khẩu
            txtPassword.PasswordChar = c;
            txtPassword.IconRight = icon;
            txtConfirmPassword.PasswordChar = c;
            txtConfirmPassword.IconRight = icon;
        }
        #endregion

        private void DangKy_FormClosed(object sender, FormClosedEventArgs e)
        {
            this.Close();
        }
    }
}
