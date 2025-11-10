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
        private readonly RegisterController _registerController = new RegisterController();

        // Service gửi email xác nhận
        private readonly IEmailSender _emailSender = new SmtpEmailSender();

        // Biến theo dõi trạng thái ẩn/hiện mật khẩu
        private bool isMatKhau = true;

        public DangKy()
        {
            InitializeComponent();

            // Cho phép nhấn ENTER để thực hiện “Đăng ký”
            this.AcceptButton = btnDangKy;
            this.KeyPreview = true;

            // Bắt sự kiện nhấn ENTER trên toàn form
            this.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Enter && btnDangKy.Enabled)
                {
                    e.SuppressKeyPress = true;   // Chặn tiếng "bíp"
                    btnDangKy.PerformClick();    // Gọi sự kiện click nút Đăng ký
                }
            };
        }

        private void DangKy_Load(object sender, EventArgs e)
        {
            // Khi click icon "mắt" ở ô xác nhận mật khẩu
            // -> dùng chung handler với ô mật khẩu chính
            txtXacNhanMatKhau.IconRightClick += txtMatKhau_IconRightClick;
        }

        // Nút “Quay lại đăng nhập”
        private void btnQuayLaiDangNhap_Click(object sender, EventArgs e)
        {
            // Nếu form đăng nhập cũ còn tồn tại thì bật lại
            Form DangNhapForm = this.Tag as Form;
            if (DangNhapForm != null && !DangNhapForm.IsDisposed)
            {
                DangNhapForm.Show();
                DangNhapForm.Activate();
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

        // Nút “Đăng ký”
        private async void btnDangKy_Click(object sender, EventArgs e)
        {
            if (!btnDangKy.Enabled) return;

            // Kiểm tra thông tin bắt buộc
            if (string.IsNullOrWhiteSpace(txtTaiKhoan.Text) ||
                string.IsNullOrWhiteSpace(txtMatKhau.Text) ||
                string.IsNullOrWhiteSpace(txtXacNhanMatKhau.Text) ||
                string.IsNullOrWhiteSpace(txtEmail.Text))
            {
                MessageBox.Show("Vui lòng nhập đầy đủ Tên đăng nhập, Mật khẩu, Xác nhận mật khẩu và Email.",
                    "Thiếu thông tin", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var email = txtEmail.Text.Trim();

            btnDangKy.Enabled = false;
            this.UseWaitCursor = true;

            try
            {
                // Kiểm tra email đã tồn tại chưa (trước khi gửi mã)
                bool emailTonTai = await _registerController.KiemTraEmailTonTaiAsync(email);
                if (emailTonTai)
                {
                    MessageBox.Show("Email này đã được sử dụng. Vui lòng dùng email khác.",
                        "Email đã tồn tại", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return; // Dừng, không gửi OTP, không đăng ký
                }

                // Gửi mã xác nhận tới email
                await EmailVerificationService.SendNewCodeAsync(email, _emailSender);

                // Mở form phụ để người dùng nhập mã OTP
                using (var dlg = new XacNhanEmail(email, _emailSender))
                {
                    var result = dlg.ShowDialog(this);

                    // Nếu người dùng bấm Hủy hoặc đóng form -> không đăng ký
                    if (result != DialogResult.OK)
                    {
                        MessageBox.Show("Bạn đã hủy xác nhận email. Đăng ký chưa được thực hiện.",
                            "Đã hủy", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }
                }

                // Mã đúng -> tạo đối tượng User và gọi RegisterController
                var newUser = new User
                {
                    TaiKhoan = txtTaiKhoan.Text.Trim(),
                    MatKhau = txtMatKhau.Text.Trim(),
                    Email = email,
                    Ten = txtTen.Text.Trim(),
                    Ngaysinh = dtpNgaySinh.Text,
                    Gioitinh = cbbGioiTinh.Text
                };

                var confirmPass = txtXacNhanMatKhau.Text.Trim();

                await _registerController.DangKyAsync(newUser, confirmPass);

                MessageBox.Show("Đăng ký thành công!", "Thông báo",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);

                // Xóa dữ liệu sau khi đăng ký thành công
                txtTen.Clear();
                txtTaiKhoan.Clear();
                txtMatKhau.Clear();
                txtXacNhanMatKhau.Clear();
                txtEmail.Clear();
                dtpNgaySinh.Value = DateTime.Today;
                cbbGioiTinh.SelectedIndex = -1;
            }
            catch (Exception ex)
            {
                // Lỗi từ AuthService, Firebase hoặc OTP -> đều báo ra
                MessageBox.Show("Lỗi: " + ex.Message, "Thông báo",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                // Luôn khôi phục trạng thái nút / con trỏ
                btnDangKy.Enabled = true;
                this.UseWaitCursor = false;
            }
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
            txtMatKhau.PasswordChar = c;
            txtMatKhau.IconRight = icon;
            txtXacNhanMatKhau.PasswordChar = c;
            txtXacNhanMatKhau.IconRight = icon;
        }
        private void DangKy_FormClosed(object sender, FormClosedEventArgs e)
        {
            Application.Exit();
        }
    }
}
