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

            // Lấy dữ liệu từ form
            var taiKhoan = txtTaiKhoan.Text.Trim();
            var matKhau = txtMatKhau.Text.Trim();
            var xacNhanMk = txtXacNhanMatKhau.Text.Trim();
            var email = txtEmail.Text.Trim();
            var ten = txtTen.Text.Trim();
            var ngaySinh = dtpNgaySinh.Value;
            var gioiTinh = cbbGioiTinh.Text.Trim();


            if (string.IsNullOrWhiteSpace(taiKhoan) ||
                string.IsNullOrWhiteSpace(matKhau) ||
                string.IsNullOrWhiteSpace(xacNhanMk) ||
                string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(ten) ||
                string.IsNullOrWhiteSpace(gioiTinh))
            {
                MessageBox.Show("Vui lòng nhập đầy đủ Tên, Tên đăng nhập, Mật khẩu, Xác nhận mật khẩu, Email và Giới tính.",
                    "Thiếu thông tin", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }


            // Xác nhận mật khẩu
            if (!string.Equals(matKhau, xacNhanMk, StringComparison.Ordinal))
            {
                MessageBox.Show("Mật khẩu xác nhận không khớp.",
                    "Sai xác nhận mật khẩu", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Validate email format
            try
            {
                var mailAddr = new System.Net.Mail.MailAddress(email);
                if (mailAddr.Address != email)
                    throw new Exception();
            }
            catch
            {
                MessageBox.Show("Định dạng email không hợp lệ.",
                    "Email không hợp lệ", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            btnDangKy.Enabled = false;
            this.UseWaitCursor = true;

            try
            {
                // Trùng tài khoản
                if (await _registerController.KiemTraTaiKhoanTonTaiAsync(taiKhoan))
                {
                    MessageBox.Show("Tên đăng nhập đã tồn tại. Vui lòng chọn tên khác.",
                        "Trùng tên đăng nhập", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Trùng email
                if (await _registerController.KiemTraEmailTonTaiAsync(email))
                {
                    MessageBox.Show("Email này đã được sử dụng. Vui lòng dùng email khác.",
                        "Email đã tồn tại", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Gửi xác nhận mail

                await EmailVerificationService.SendNewCodeAsync(email, _emailSender);

                using (var dlg = new XacNhanEmail(email, _emailSender))
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

                // CLEAR 

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
                MessageBox.Show("Lỗi: " + ex.Message, "Thông báo",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
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
