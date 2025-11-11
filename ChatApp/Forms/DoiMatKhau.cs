using System;
using System.Windows.Forms;

using ChatApp.Controllers;

namespace ChatApp
{
    public partial class DoiMatKhau : Form
    {
        private readonly string _taiKhoan;
        private readonly ChangePasswordController _controller = new ChangePasswordController();

        public DoiMatKhau(string taiKhoan)
        {
            InitializeComponent();
            _taiKhoan = taiKhoan;

            // Thiết lập Enter để nhấn nút Đổi mật khẩu
            this.AcceptButton = btnDoiMatKhau;
        }

        private async void btnDoiMatKhau_Click(object sender, EventArgs e)
        {
            // Ngăn spam nhấn nút
            if (!btnDoiMatKhau.Enabled) return;

            btnDoiMatKhau.Enabled = false;
            this.UseWaitCursor = true;

            try
            {
                string mkMoi = txtMatKhau.Text.Trim();
                string mkXn = txtXacNhan.Text.Trim();

                if (string.IsNullOrWhiteSpace(mkMoi) || string.IsNullOrWhiteSpace(mkXn))
                {
                    MessageBox.Show("Vui lòng nhập đầy đủ mật khẩu!", "Thông báo",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (mkMoi != mkXn)
                {
                    MessageBox.Show("Mật khẩu xác nhận không khớp!", "Lỗi",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                if (mkMoi.Length < 6)
                {
                    MessageBox.Show("Mật khẩu phải có ít nhất 6 ký tự.", "Thông báo",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Gọi Controller để đổi mật khẩu
                bool thanhCong = await _controller.DoiMatKhauAsync(_taiKhoan, mkMoi);

                if (thanhCong)
                {
                    MessageBox.Show("Đổi mật khẩu thành công!", "Thông báo",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);

                    if (this.Tag is Form prev && !prev.IsDisposed)
                        prev.Show();

                    this.Close();
                }
                else
                {
                    MessageBox.Show("Có lỗi xảy ra khi đổi mật khẩu!", "Lỗi",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi: " + ex.Message, "Thông báo",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnDoiMatKhau.Enabled = true;
                this.UseWaitCursor = false;
            }
        }


    }
}
