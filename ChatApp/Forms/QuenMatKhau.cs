using System;
using System.Windows.Forms;
using Guna.UI2.WinForms;

using ChatApp.Controllers;

namespace ChatApp
{
    /// <summary>
    /// Form quên mật khẩu:
    /// - Nhập email đã đăng ký để nhận mã OTP.
    /// - Xác nhận OTP.
    /// - Nếu OTP hợp lệ thì mở form đổi mật khẩu.
    /// </summary>
    /// <remarks>
    /// Sử dụng <see cref="ForgotPasswordController"/> để:
    /// - Tìm tài khoản theo email.
    /// - Tạo và lưu OTP.
    /// - Kiểm tra OTP hợp lệ.
    /// - Gửi OTP qua email.
    /// </remarks>
    public partial class QuenMatKhau : Form
    {
        #region ======== Biến / Controllers ========

        /// <summary>
        /// Controller xử lý logic quên mật khẩu (tìm tài khoản, OTP, email...).
        /// </summary>
        private readonly ForgotPasswordController _controller = new ForgotPasswordController();

        /// <summary>
        /// Lưu tài khoản đang trong quá trình xác nhận OTP.
        /// </summary>
        private string _taiKhoanDangXacNhan;

        /// <summary>
        /// Form đổi mật khẩu sẽ được mở sau khi xác nhận OTP thành công.
        /// </summary>
        private DoiMatKhau _doiMatKhauForm;

        // private Timer _otpTimer; // Timer để hạn chế gửi OTP liên tục (nếu cần dùng sau này)

        #endregion

        #region ======== Khởi tạo ========

        /// <summary>
        /// Khởi tạo form quên mật khẩu.
        /// </summary>
        public QuenMatKhau()
        {
            InitializeComponent();

            this.Load += QuenMatKhau_Load;
        }

        /// <summary>
        /// Sự kiện Load form:
        /// - Gán sự kiện click cho nút Gửi mã xác nhận.
        /// - Gán sự kiện click cho nút Xác nhận.
        /// </summary>
        /// <param name="sender">Form quên mật khẩu.</param>
        /// <param name="e">Thông tin sự kiện.</param>
        private void QuenMatKhau_Load(object sender, EventArgs e)
        {
            // Gán sự kiện click cho nút gửi OTP
            btnGuiMaXacNhan.Click -= btnGuiMaXacNhan_Click;
            btnGuiMaXacNhan.Click += btnGuiMaXacNhan_Click;

            // Gán sự kiện click cho nút xác nhận OTP
            btnXacNhan.Click -= btnXacNhan_Click;
            btnXacNhan.Click += btnXacNhan_Click;
        }

        #endregion

        #region ======== Gửi OTP ========

        /// <summary>
        /// Xử lý gửi mã OTP đến email:
        /// - Kiểm tra email nhập vào.
        /// - Tìm tài khoản theo email.
        /// - Tạo OTP và lưu vào Firebase.
        /// - Gửi OTP qua email.
        /// - Thông báo trạng thái cho người dùng.
        /// </summary>
        /// <param name="sender">Nút Gửi mã xác nhận.</param>
        /// <param name="e">Thông tin sự kiện click.</param>
        private async void btnGuiMaXacNhan_Click(object sender, EventArgs e)
        {
            // Nếu đang disable (đã bấm rồi) thì bỏ qua
            if (!btnGuiMaXacNhan.Enabled)
                return;

            string email = txtEmail.Text.Trim();

            // Kiểm tra email trước, chưa khóa nút vội
            if (string.IsNullOrWhiteSpace(email))
            {
                MessageBox.Show(
                    "Vui lòng nhập email đã đăng ký!",
                    "Thông báo",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                return;
            }

            // Từ đây trở đi bắt đầu xử lý gửi OTP → khóa nút ngay
            DoiTrangThaiNut(btnGuiMaXacNhan, false);

            try
            {
                // 1) Tìm tài khoản trong Firebase theo email
                string taiKhoan = await _controller.TimTaiKhoanBangEmailAsync(email);
                if (taiKhoan == null)
                {
                    MessageBox.Show(
                        "Không tìm thấy tài khoản nào với email này!",
                        "Lỗi",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);

                    // Gửi thất bại → mở lại nút cho user thử lại
                    DoiTrangThaiNut(btnGuiMaXacNhan, true);
                    return;
                }

                // Lưu lại để xài cho bước xác nhận OTP và đổi mật khẩu
                _taiKhoanDangXacNhan = taiKhoan;

                // 2) Tạo OTP và lưu vào Firebase theo tài khoản tìm được
                string otp = await _controller.TaoVaLuuOtpAsync(taiKhoan, email);
                if (otp == null)
                {
                    MessageBox.Show(
                        "Không thể tạo mã xác nhận. Vui lòng thử lại sau!",
                        "Lỗi",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);

                    // Gửi thất bại → mở lại nút
                    DoiTrangThaiNut(btnGuiMaXacNhan, true);
                    return;
                }

                // 3) Gửi OTP qua email
                _controller.GuiEmailOtp(email, otp);

                MessageBox.Show(
                    "Đã gửi mã xác nhận qua email (hạn trong 5 phút).",
                    "Thông báo",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                txtMaXacNhan.Focus();

                DoiTrangThaiNut(btnGuiMaXacNhan, true);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Lỗi gửi mã xác nhận: " + ex.Message,
                    "Lỗi",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

                // Có exception → mở lại nút để user thử lại
                DoiTrangThaiNut(btnGuiMaXacNhan, true);
            }
        }

        #endregion

        #region ======== Xác nhận OTP ========

        /// <summary>
        /// Xử lý xác nhận OTP và mở form đổi mật khẩu:
        /// - Kiểm tra mã nhập vào.
        /// - Kiểm tra đã gửi OTP chưa.
        /// - Gọi controller kiểm tra OTP hợp lệ.
        /// - Nếu hợp lệ: mở form đổi mật khẩu tương ứng với tài khoản.
        /// </summary>
        /// <param name="sender">Nút Xác nhận.</param>
        /// <param name="e">Thông tin sự kiện click.</param>
        private async void btnXacNhan_Click(object sender, EventArgs e)
        {
            DoiTrangThaiNut(btnXacNhan, false); // Disable nút khi đang xử lý

            try
            {
                string maNhap = txtMaXacNhan.Text.Trim();

                if (string.IsNullOrWhiteSpace(maNhap))
                {
                    MessageBox.Show(
                        "Vui lòng nhập mã xác nhận!",
                        "Thông báo",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);

                    return;
                }

                if (string.IsNullOrWhiteSpace(_taiKhoanDangXacNhan))
                {
                    MessageBox.Show(
                        "Bạn chưa gửi mã xác nhận!",
                        "Thông báo",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);

                    return;
                }

                // Kiểm tra OTP hợp lệ
                bool hopLe = await _controller.KiemTraOtpHopLeAsync(_taiKhoanDangXacNhan, maNhap);

                if (!hopLe)
                {
                    MessageBox.Show(
                        "Mã xác nhận không đúng hoặc đã hết hạn!",
                        "Lỗi",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);

                    return;
                }

                MessageBox.Show(
                    "Xác nhận thành công! Vui lòng đổi mật khẩu mới.",
                    "Thông báo",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                // Mở form đổi mật khẩu
                MoFormDoiMatKhau(_taiKhoanDangXacNhan);
                this.Hide();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Lỗi khi xác nhận mã: " + ex.Message,
                    "Lỗi",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            finally
            {
                DoiTrangThaiNut(btnXacNhan, true); // Bật lại nút
            }
        }

        #endregion

        #region ======== Tiện ích ========

        /// <summary>
        /// Thay đổi trạng thái nút (enable/disable) và:
        /// - Gán <see cref="Form.AcceptButton"/> tương ứng (ENTER chỉ active khi nút enable).
        /// - Hiển thị/ẩn cursor chờ trong form.
        /// </summary>
        /// <param name="btn">Nút cần thay đổi trạng thái.</param>
        /// <param name="enable">true để enable, false để disable.</param>
        private void DoiTrangThaiNut(Guna2Button btn, bool enable)
        {
            btn.Enabled = enable;
            this.AcceptButton = enable ? btn : null;  // ENTER chỉ active khi nút enable
            this.UseWaitCursor = !enable;             // Hiển thị cursor chờ khi disable
        }

        /// <summary>
        /// Mở form đổi mật khẩu cho tài khoản đã xác nhận.
        /// - Nếu form đã mở thì chỉ cần Show + Activate.
        /// - Nếu chưa có thì tạo form mới, gán Tag để quay lại, và lắng nghe FormClosed để giải phóng biến tham chiếu.
        /// </summary>
        /// <param name="taiKhoan">Tài khoản cần đổi mật khẩu.</param>
        private void MoFormDoiMatKhau(string taiKhoan)
        {
            if (_doiMatKhauForm != null && !_doiMatKhauForm.IsDisposed)
            {
                _doiMatKhauForm.Show();
                _doiMatKhauForm.Activate();
            }
            else
            {
                _doiMatKhauForm = new DoiMatKhau(taiKhoan);
                _doiMatKhauForm.Tag = this; // Lưu form hiện tại để truy cập khi cần

                _doiMatKhauForm.FormClosed += delegate (object sender, FormClosedEventArgs e)
                {
                    _doiMatKhauForm = null;
                };

                _doiMatKhauForm.Show();
            }
        }

        #endregion

        #region ======== Quay lại đăng nhập ========

        /// <summary>
        /// Quay về form đăng nhập:
        /// - Nếu Tag đang lưu form đăng nhập cũ thì Show lại form đó.
        /// - Nếu không có thì tạo form <see cref="DangNhap"/> mới.
        /// Sau đó đóng form quên mật khẩu hiện tại.
        /// </summary>
        /// <param name="sender">Nút "Quay lại đăng nhập".</param>
        /// <param name="e">Thông tin sự kiện click.</param>
        private void btnQuayLaiDangNhap_Click(object sender, EventArgs e)
        {
            Form formDangNhap = this.Tag as Form;
            if (formDangNhap != null && !formDangNhap.IsDisposed)
            {
                formDangNhap.Show();
            }
            else
            {
                var frm = new DangNhap();
                frm.Show();
            }

            this.Close();
        }

        #endregion

        #region ======== Vòng đời form – Thoát ứng dụng ========

        /// <summary>
        /// Khi form Quên mật khẩu bị đóng:
        /// - Thoát toàn bộ ứng dụng.
        /// </summary>
        /// <param name="sender">Form quên mật khẩu.</param>
        /// <param name="e">Thông tin sự kiện đóng form.</param>
        private void QuenMatKhau_FormClosed(object sender, FormClosedEventArgs e)
        {
            Application.Exit();
        }

        #endregion
    }
}
