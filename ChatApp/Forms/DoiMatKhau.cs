using System;
using System.Windows.Forms;

using ChatApp.Controllers;

namespace ChatApp
{
    /// <summary>
    /// Form đổi mật khẩu cho tài khoản đã được xác thực trước đó
    /// (thường được mở sau khi người dùng xác nhận OTP quên mật khẩu).
    /// </summary>
    /// <remarks>
    /// - Nhận tài khoản cần đổi mật khẩu qua constructor.
    /// - Kiểm tra mật khẩu mới, xác nhận mật khẩu và độ dài tối thiểu.
    /// - Gọi <see cref="ChangePasswordController"/> để cập nhật mật khẩu.
    /// - Nếu thành công: thông báo và quay về form trước đó (nếu có).
    /// </remarks>
    public partial class DoiMatKhau : Form
    {
        #region ======== Biến / Controllers ========

        /// <summary>
        /// Tài khoản (username) cần đổi mật khẩu.
        /// </summary>
        private readonly string _taiKhoan;

        /// <summary>
        /// Controller xử lý logic đổi mật khẩu.
        /// </summary>
        private readonly ChangePasswordController _controller = new ChangePasswordController();

        #endregion

        #region ======== Khởi tạo Form ========

        /// <summary>
        /// Khởi tạo form đổi mật khẩu với tài khoản tương ứng.
        /// </summary>
        /// <param name="taiKhoan">Tài khoản sẽ được đổi mật khẩu.</param>
        public DoiMatKhau(string taiKhoan)
        {
            InitializeComponent();

            _taiKhoan = taiKhoan;

            // Thiết lập Enter để kích hoạt nút Đổi mật khẩu
            this.AcceptButton = btnDoiMatKhau;
        }

        #endregion

        #region ======== Nút Đổi mật khẩu – Xử lý đổi mật khẩu ========

        /// <summary>
        /// Xử lý sự kiện khi người dùng bấm nút Đổi mật khẩu:
        /// - Ngăn spam nhấn bằng cách disable nút trong lúc xử lý.
        /// - Kiểm tra mật khẩu mới và mật khẩu xác nhận:
        ///   + Không được để trống.
        ///   + Phải trùng nhau.
        ///   + Độ dài tối thiểu 6 ký tự.
        /// - Gọi controller để cập nhật mật khẩu trong backend.
        /// - Nếu thành công: thông báo, đóng form và quay lại form trước đó (nếu tồn tại).
        /// </summary>
        /// <param name="sender">Nút Đổi mật khẩu.</param>
        /// <param name="e">Thông tin sự kiện click.</param>
        private async void btnDoiMatKhau_Click(object sender, EventArgs e)
        {
            // Ngăn spam nhấn nút
            if (!btnDoiMatKhau.Enabled)
                return;

            btnDoiMatKhau.Enabled = false;
            this.UseWaitCursor = true;

            try
            {
                string mkMoi = txtMatKhau.Text.Trim();
                string mkXn = txtXacNhan.Text.Trim();

                // Kiểm tra không để trống
                if (string.IsNullOrWhiteSpace(mkMoi) || string.IsNullOrWhiteSpace(mkXn))
                {
                    MessageBox.Show(
                        "Vui lòng nhập đầy đủ mật khẩu!",
                        "Thông báo",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);

                    return;
                }

                // Kiểm tra khớp mật khẩu
                if (mkMoi != mkXn)
                {
                    MessageBox.Show(
                        "Mật khẩu xác nhận không khớp!",
                        "Lỗi",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);

                    return;
                }

                // Kiểm tra độ dài tối thiểu
                if (mkMoi.Length < 6)
                {
                    MessageBox.Show(
                        "Mật khẩu phải có ít nhất 6 ký tự.",
                        "Thông báo",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);

                    return;
                }

                // Gọi Controller để đổi mật khẩu
                bool thanhCong = await _controller.DoiMatKhauAsync(_taiKhoan, mkMoi);

                if (thanhCong)
                {
                    MessageBox.Show(
                        "Đổi mật khẩu thành công!",
                        "Thông báo",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);

                    // Nếu form trước đó còn tồn tại thì show lại
                    Form prev = this.Tag as Form;
                    if (prev != null && !prev.IsDisposed)
                    {
                        prev.Show();
                    }

                    this.Close();
                }
                else
                {
                    MessageBox.Show(
                        "Có lỗi xảy ra khi đổi mật khẩu!",
                        "Lỗi",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
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
                btnDoiMatKhau.Enabled = true;
                this.UseWaitCursor = false;
            }
        }

        #endregion
    }
}
