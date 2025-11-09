using System;
using System.Windows.Forms;

// Thư viện tự tạo
using ChatApp.Controllers;
using ChatApp.Models.Users;

namespace ChatApp
{
    public partial class DangKy : Form
    {
        // Controller xử lý logic đăng ký người dùng
        private readonly RegisterController _registerController = new RegisterController();
        private bool isMatKhau = true;   // Biến theo dõi trạng thái ẩn/hiện mật khẩu

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
                    e.SuppressKeyPress = true;   // Ngăn tiếng “bíp”
                    btnDangKy.PerformClick();    // Gọi sự kiện click nút “Đăng ký”
                }
            };
        }

        private void DangKy_Load(object sender, EventArgs e)
        {
            // Khi click vào icon “mắt” của ô xác nhận mật khẩu
            // sẽ gọi cùng sự kiện với ô mật khẩu chính
            txtXacNhanMatKhau.IconRightClick += txtMatKhau_IconRightClick;
        }

        // Nút “Quay lại đăng nhập”
        private void btnQuayLaiDangNhap_Click(object sender, EventArgs e)
        {
            // Nếu form đăng nhập đang bị ẩn -> mở lại
            Form DangNhapForm = this.Tag as Form;
            if (DangNhapForm != null && !DangNhapForm.IsDisposed)
            {
                DangNhapForm.Show();
                DangNhapForm.Activate();
                this.Close();
            }
            else
            {
                // Nếu không có form đăng nhập trước đó -> tạo mới
                var newLogin = new DangNhap();
                newLogin.Show();
                this.Close();
            }
        }

        // Nút “Đăng ký”
        private async void btnDangKy_Click(object sender, EventArgs e)
        {
            // Chống spam click
            if (!btnDangKy.Enabled) return;

            btnDangKy.Enabled = false;
            this.UseWaitCursor = true; // Hiển thị con trỏ chờ

            try
            {
                // Lấy thông tin người dùng nhập
                var newUser = new User
                {
                    TaiKhoan = txtTaiKhoan.Text.Trim(),
                    MatKhau = txtMatKhau.Text.Trim(),
                    Email = txtEmail.Text.Trim(),
                    Ten = txtTen.Text.Trim(),
                    Ngaysinh = dtpNgaySinh.Text,
                    Gioitinh = cbbGioiTinh.Text
                };

                var confirmPass = txtXacNhanMatKhau.Text.Trim();

                // Gọi controller để xử lý đăng ký (kiểm tra, lưu Firebase,...)
                await _registerController.DangKyAsync(newUser, confirmPass);

                MessageBox.Show("Đăng ký thành công!", "Thông báo",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);

                // Xóa dữ liệu nhập sau khi đăng ký thành công
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
                // Thông báo lỗi (Firebase, logic hoặc kết nối)
                MessageBox.Show("Lỗi: " + ex.Message, "Thông báo",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                // Luôn khôi phục lại trạng thái ban đầu
                btnDangKy.Enabled = true;
                this.UseWaitCursor = false;
            }
        }

        // Sự kiện click vào icon “mắt” để ẩn/hiện mật khẩu
        private void txtMatKhau_IconRightClick(object sender, EventArgs e)
        {
            isMatKhau = !isMatKhau; // Đảo trạng thái

            // Nếu đang ẩn → hiển thị, ngược lại thì che
            char c = isMatKhau ? '●' : '\0';
            var icon = isMatKhau
                ? Properties.Resources.AnMatKhau
                : Properties.Resources.HienMatKhau;

            // Cập nhật cho cả 2 ô
            txtMatKhau.PasswordChar = c;
            txtMatKhau.IconRight = icon;
            txtXacNhanMatKhau.PasswordChar = c;
            txtXacNhanMatKhau.IconRight = icon;
        }
    }
}
