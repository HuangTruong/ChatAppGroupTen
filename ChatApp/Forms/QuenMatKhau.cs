using Guna.UI2.WinForms;
using System;
using System.Windows.Forms;

using ChatApp.Controllers;

namespace ChatApp
{
    public partial class QuenMatKhau : Form
    {
        #region === Biến ===

        private readonly ForgotPasswordController _controller = new ForgotPasswordController(); // Controller xử lý logic quên mật khẩu

        private string _taiKhoanDangXacNhan; // Lưu tài khoản đang xác nhận OTP
        private DoiMatKhau _doiMatKhauForm;  // Form đổi mật khẩu

        private Timer _otpTimer; // Timer để hạn chế gửi OTP liên tục

        #endregion

        #region === Khởi tạo ===

        public QuenMatKhau()
        {
            InitializeComponent();
            this.Load += QuenMatKhau_Load; // Gán sự kiện load form
        }

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

        #region === Gửi OTP ===

        // Xử lý gửi mã OTP đến email
        private async void btnGuiMaXacNhan_Click(object sender, EventArgs e)
        {
            if (!btnGuiMaXacNhan.Enabled) return; // Nếu nút bị disable thì không thực hiện

            string email = txtEmail.Text.Trim();
            string taiKhoan = txtTaiKhoan.Text.Trim();

            // Kiểm tra thông tin nhập
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(taiKhoan))
            {
                MessageBox.Show("Vui lòng nhập đầy đủ thông tin!", "Thông báo",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                // Tạo OTP và lưu vào Firebase
                string otp = await _controller.TaoVaLuuOtpAsync(taiKhoan, email);
                if (otp == null)
                {
                    MessageBox.Show("Tài khoản hoặc email không hợp lệ!", "Lỗi",
                                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                _taiKhoanDangXacNhan = taiKhoan;
                _controller.GuiEmailOtp(email, otp); // Gửi OTP qua email

                MessageBox.Show("Đã gửi mã xác nhận qua email (hạn trong 5 phút).",
                                "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                txtMaXacNhan.Focus();

                // Disable nút gửi OTP 60 giây
                DoiTrangThaiNut(btnGuiMaXacNhan, false);
                _otpTimer?.Stop();
                _otpTimer = new Timer { Interval = 60000 };
                _otpTimer.Tick += (s, ev) =>
                {
                    DoiTrangThaiNut(btnGuiMaXacNhan, true);
                    _otpTimer.Stop();
                };
                _otpTimer.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi gửi mã xác nhận: " + ex.Message, "Lỗi",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        #endregion

        #region === Xác nhận OTP ===

        // Xử lý xác nhận OTP và mở form đổi mật khẩu
        private async void btnXacNhan_Click(object sender, EventArgs e)
        {
            DoiTrangThaiNut(btnXacNhan, false); // Disable nút khi đang xử lý

            try
            {
                string maNhap = txtMaXacNhan.Text.Trim();

                if (string.IsNullOrWhiteSpace(maNhap))
                {
                    MessageBox.Show("Vui lòng nhập mã xác nhận!", "Thông báo",
                                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (string.IsNullOrWhiteSpace(_taiKhoanDangXacNhan))
                {
                    MessageBox.Show("Bạn chưa gửi mã xác nhận!", "Thông báo",
                                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Kiểm tra OTP hợp lệ
                bool hopLe = await _controller.KiemTraOtpHopLeAsync(_taiKhoanDangXacNhan, maNhap);

                if (!hopLe)
                {
                    MessageBox.Show("Mã xác nhận không đúng hoặc đã hết hạn!", "Lỗi",
                                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                MessageBox.Show("Xác nhận thành công! Vui lòng đổi mật khẩu mới.",
                                "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);

                MoFormDoiMatKhau(_taiKhoanDangXacNhan); // Mở form đổi mật khẩu
                this.Hide();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi xác nhận mã: " + ex.Message, "Lỗi",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                DoiTrangThaiNut(btnXacNhan, true); // Bật lại nút
            }
        }

        #endregion

        #region === Tiện ích ===

        // Thay đổi trạng thái nút (enable/disable)
        private void DoiTrangThaiNut(Guna2Button btn, bool enable)
        {
            btn.Enabled = enable;
            this.AcceptButton = enable ? btn : null; // ENTER chỉ active khi nút enable
            this.UseWaitCursor = !enable; // Hiển thị cursor chờ khi disable
        }

        // Mở form đổi mật khẩu, nếu đã mở thì focus
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
                _doiMatKhauForm.FormClosed += (s, _) => _doiMatKhauForm = null;
                _doiMatKhauForm.Show();
            }
        }

        #endregion

        #region === Quay lại đăng nhập ===

        // Quay về form đăng nhập
        private void btnQuayLaiDangNhap_Click(object sender, EventArgs e)
        {
            if (this.Tag is Form formDangNhap && !formDangNhap.IsDisposed)
                formDangNhap.Show();
            else
                new DangNhap().Show();

            this.Close();
        }

        #endregion
    }
}
