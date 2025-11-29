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

            txtTaiKhoan.Focus(); // Khi form mở, con trỏ nằm sẵn ở ô tài khoản
            this.AcceptButton = btnDangNhap; // Cho phép nhấn ENTER để kích hoạt nút Đăng nhập
        }

        #region Sư kiện nút Đăng ký ,mở form Đăng ký
        private void btnDangKy_Click(object sender, EventArgs e)
        {
            var DangKyForm = new DangKy();
            DangKyForm.FormClosed += (s,args) => this.Show();  // Quay lại Form Đăng nhập khi Form Đăng kí đóng
            DangKyForm.Show();
            this.Hide();
        }
        #endregion

        #region Sự kiện link Quên mật khẩu , mở form Quên mật khẩu
        private void lnkQuenMatKhau_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            var QuenMKForm = new QuenMatKhau();
            QuenMKForm.Tag = this;
            QuenMKForm.Show();
            this.Hide();
        }
        #endregion

        #region Sự kiện nút Đăng nhập
        private async void btnDangNhap_Click(object sender, EventArgs e)
        {
            if (!btnDangNhap.Enabled) return;

            btnDangNhap.Enabled = false;
            this.UseWaitCursor = true;

            try
            {
                var user = await _loginController.DangNhapAsync(txtTaiKhoan.Text, txtMatKhau.Text);

                MessageBox.Show("Đăng nhập thành công!", "Thông báo",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);

                this.Hide();
                var home = new TrangChu(user.Ten, user.TaiKhoan);
                home.FormClosed += (s, e2) => this.Show();
                home.Show();

                txtTaiKhoan.Clear();
                txtMatKhau.Clear();
                txtTaiKhoan.Focus();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi đăng nhập: " + ex.Message,
                    "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);

                // Nếu sai mật khẩu -> xóa mật khẩu, giữ nguyên tài khoản
                if (ex.Message.Contains("Mật khẩu không đúng"))
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

        #region Sự kiện click vào icon con mắt để ẩn/hiện mật khẩu
        private void txtMatKhau_IconRightClick(object sender, EventArgs e)
        {
            isMatKhau = !isMatKhau;
            txtMatKhau.PasswordChar = isMatKhau ? '●' : '\0';
            txtMatKhau.IconRight = isMatKhau
                ? Properties.Resources.AnMatKhau
                : Properties.Resources.HienMatKhau;
        }
        #endregion

        #region Sự kiện form closed. Đóng toàn bộ ứng dụng khi form đăng nhập bị tắt
        private void DangNhap_FormClosed(object sender, FormClosedEventArgs e)
        {
            Application.Exit();
            Environment.Exit(0);
        }
        #endregion
    }
}
