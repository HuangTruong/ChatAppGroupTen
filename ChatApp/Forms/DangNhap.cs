using System;
using System.Windows.Forms;

// Các thư viện tự tạo
using ChatApp.Controllers;

namespace ChatApp
{
    public partial class DangNhap : Form
    {
        private readonly LoginController _loginController = new LoginController();
        bool isMatKhau = true;  // Theo dõi trạng thái ẩn/hiện mật khẩu

        public DangNhap()
        {
            InitializeComponent();

            // Khi form mở, con trỏ nằm sẵn ở ô tài khoản
            txtTaiKhoan.Focus();

            // Cho phép nhấn ENTER để kích hoạt nút Đăng nhập
            this.AcceptButton = btnDangNhap;
        }

        // Đóng toàn bộ ứng dụng khi form đăng nhập bị tắt
        private void DangNhap_FormClosed(object sender, FormClosedEventArgs e)
        {
            Application.Exit();
        }

        // Mở form Đăng ký
        private void btnDangKy_Click(object sender, EventArgs e)
        {
            var DangKyForm = new DangKy();
            DangKyForm.Tag = this;  // Giữ tham chiếu form hiện tại để quay lại sau
            DangKyForm.Show();
            this.Hide();
        }

        // Mở form Quên mật khẩu
        private void lnkQuenMatKhau_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            var QuenMKForm = new QuenMatKhau();
            QuenMKForm.Tag = this;
            QuenMKForm.Show();
            this.Hide();
        }

        // Nút Đăng nhập
        private async void btnDangNhap_Click(object sender, EventArgs e)
        {
            // Chặn spam click
            if (!btnDangNhap.Enabled) return;

            btnDangNhap.Enabled = false;
            this.UseWaitCursor = true;  // Hiện con trỏ chờ trong lúc xử lý

            try
            {
                // Gọi controller để xử lý đăng nhập (Firebase hoặc logic riêng)
                var user = await _loginController.DangNhapAsync(
                    txtTaiKhoan.Text, txtMatKhau.Text);

                MessageBox.Show("Đăng nhập thành công!", "Thông báo",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);

                // Mở form Trang chủ và ẩn form hiện tại
                this.Hide();
                var home = new TrangChu(user.Ten);
                home.FormClosed += (s, e2) => this.Close();  // Khi đóng home -> thoát luôn
                home.Show();
            }
            catch (Exception ex)
            {
                // Báo lỗi đăng nhập (lỗi hệ thống hoặc Firebase)
                MessageBox.Show("Lỗi đăng nhập: " + ex.Message, "Lỗi",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                // Luôn khôi phục trạng thái UI
                btnDangNhap.Enabled = true;
                this.UseWaitCursor = false;
            }
        }

        // Sự kiện click vào icon con mắt để ẩn/hiện mật khẩu
        private void txtMatKhau_IconRightClick(object sender, EventArgs e)
        {
            isMatKhau = !isMatKhau;
            txtMatKhau.PasswordChar = isMatKhau ? '●' : '\0';
            txtMatKhau.IconRight = isMatKhau
                ? Properties.Resources.AnMatKhau
                : Properties.Resources.HienMatKhau;
        }
    }
}
