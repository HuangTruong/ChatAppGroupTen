using System;
using System.Windows.Forms;
using Guna.UI2.WinForms;

namespace ChatApp
{
    public partial class CatDat : Form
    {
        private readonly string _taiKhoan;
        private readonly string _email;

        // Truyền tài khoản + email từ form gọi (VD: TrangChu)
        public CatDat(string taiKhoan, string email)
        {
            InitializeComponent();

            _taiKhoan = taiKhoan;
            _email = email;

            txtTenDangNhap.Text = _taiKhoan;
            txtEmail.Text = _email;

            txtTenDangNhap.ReadOnly = true;
            txtEmail.ReadOnly = true;
        }

        private void CatDat_Load(object sender, EventArgs e)
        {
        }

        // 👉 Đổi mật khẩu: mở form DoiMatKhau với đúng tài khoản
        private void btnDoiMatKhau_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_taiKhoan))
            {
                MessageBox.Show("Không xác định được tài khoản hiện tại.", "Lỗi",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var frm = new DoiMatKhau(_taiKhoan)
            {
                StartPosition = FormStartPosition.CenterParent
            };

            // Modal: chờ đổi xong rồi quay lại Cài đặt
            frm.ShowDialog(this);
        }

        // 👉 Đổi email: tạm để TODO, sau này bạn gắn flow xác thực email
        private void btnDoiEmail_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Chức năng đổi Email sẽ được bổ sung sau.",
                "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void btnDong_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
