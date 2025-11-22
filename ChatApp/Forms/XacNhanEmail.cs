using System;
using System.Windows.Forms;

using ChatApp.Services.Auth;
using ChatApp.Services.Email;

namespace ChatApp
{
    /// <summary>
    /// Form xác nhận email bằng mã OTP:
    /// - Hiển thị email cần xác nhận.
    /// - Gửi mã OTP lần đầu và cho phép gửi lại sau thời gian chờ.
    /// - Người dùng nhập mã để xác nhận.
    /// </summary>
    /// <remarks>
    /// Sử dụng <see cref="EmailVerificationService"/> để:
    /// - Gửi mã xác nhận mới.
    /// - Kiểm tra giới hạn gửi lại.
    /// - Xác thực mã OTP người dùng nhập.
    /// </remarks>
    public partial class XacNhanEmail : Form
    {
        #region ======== Biến / Fields ========

        /// <summary>
        /// Email cần xác nhận.
        /// </summary>
        private readonly string _email;

        /// <summary>
        /// Service gửi email (SMTP / API).
        /// </summary>
        private readonly IEmailSender _sender;

        /// <summary>
        /// Biến đếm ngược số giây còn lại trước khi có thể gửi lại mã.
        /// </summary>
        private int _countdown;

        #endregion

        #region ======== Khởi tạo ========

        /// <summary>
        /// Khởi tạo form xác nhận email với email và dịch vụ gửi mail tương ứng.
        /// </summary>
        /// <param name="email">Email cần xác nhận.</param>
        /// <param name="sender">Đối tượng gửi email.</param>
        public XacNhanEmail(string email, IEmailSender sender)
        {
            InitializeComponent();

            _email = email;
            _sender = sender;
        }

        /// <summary>
        /// Sự kiện Load của form:
        /// - Hiển thị email lên label.
        /// - Nếu có thể gửi mã mới thì gửi luôn và bắt đầu đếm ngược.
        /// </summary>
        /// <param name="sender">Form xác nhận email.</param>
        /// <param name="e">Thông tin sự kiện.</param>
        private async void XacNhanEmail_Load(object sender, EventArgs e)
        {
            lblEmail.Text = _email;

            int wait;
            if (EmailVerificationService.CanResend(_email, out wait))
            {
                try
                {
                    await EmailVerificationService.SendNewCodeAsync(_email, _sender);
                    BatDemNguoc(60);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        "Không thể gửi mã xác nhận: " + ex.Message,
                        "Lỗi",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
            }
            else
            {
                // Nếu không thể gửi ngay (theo giới hạn service), có thể hiển thị trạng thái nếu cần
                BatDemNguoc(wait);
            }
        }

        #endregion

        #region ======== Đếm ngược / Cooldown gửi lại ========

        /// <summary>
        /// Bắt đầu đếm ngược số giây trước khi có thể gửi lại mã.
        /// - Disable nút Gửi lại.
        /// - Khởi chạy timer.
        /// </summary>
        /// <param name="seconds">Số giây cần đếm ngược.</param>
        private void BatDemNguoc(int seconds)
        {
            _countdown = seconds;
            btnGuiLai.Enabled = false;

            timerCooldown.Interval = 1000; // 1 giây
            timerCooldown.Start();

            CapNhatNhanDemNguoc();
        }

        /// <summary>
        /// Cập nhật nội dung label hiển thị thời gian đếm ngược.
        /// </summary>
        private void CapNhatNhanDemNguoc()
        {
            if (_countdown > 0)
            {
                lblDemNguoc.Text = "Gửi lại mã sau: " + _countdown + "s";
            }
            else
            {
                lblDemNguoc.Text = "Bạn có thể gửi lại mã.";
            }
        }

        /// <summary>
        /// Tick của timer cooldown:
        /// - Giảm biến đếm ngược.
        /// - Khi về 0 thì dừng timer và bật lại nút Gửi lại.
        /// </summary>
        /// <param name="sender">Timer cooldown.</param>
        /// <param name="e">Thông tin sự kiện.</param>
        private void timerCooldown_Tick(object sender, EventArgs e)
        {
            _countdown--;

            if (_countdown <= 0)
            {
                timerCooldown.Stop();
                btnGuiLai.Enabled = true;
                CapNhatNhanDemNguoc();
            }
            else
            {
                CapNhatNhanDemNguoc();
            }
        }

        #endregion

        #region ======== Nút Hủy ========

        /// <summary>
        /// Người dùng hủy xác nhận email:
        /// - Đặt <see cref="Form.DialogResult"/> = <see cref="DialogResult.Cancel"/>.
        /// - Đóng form.
        /// </summary>
        /// <param name="sender">Nút Hủy.</param>
        /// <param name="e">Thông tin sự kiện click.</param>
        private void btnHuy_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        #endregion

        #region ======== Nút Xác nhận mã ========

        /// <summary>
        /// Xử lý khi người dùng bấm nút Xác nhận:
        /// - Kiểm tra người dùng đã nhập mã hay chưa.
        /// - Gọi service để xác thực mã.
        /// - Nếu đúng: DialogResult = OK và đóng form.
        /// - Nếu sai: hiển thị lỗi từ service.
        /// </summary>
        /// <param name="sender">Nút Xác nhận.</param>
        /// <param name="e">Thông tin sự kiện click.</param>
        private void btnXacNhan_Click(object sender, EventArgs e)
        {
            string code = txtMa.Text;

            if (string.IsNullOrWhiteSpace(code))
            {
                MessageBox.Show(
                    "Vui lòng nhập mã xác nhận.",
                    "Thiếu mã",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                return;
            }

            string error;
            if (EmailVerificationService.Verify(_email, code, out error))
            {
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            else
            {
                MessageBox.Show(
                    error,
                    "Sai mã",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
        }

        #endregion

        #region ======== Nút Gửi lại mã ========

        /// <summary>
        /// Xử lý khi người dùng bấm nút Gửi lại:
        /// - Kiểm tra có được phép gửi lại (theo giới hạn thời gian) hay không.
        /// - Nếu chưa đủ thời gian: thông báo còn phải chờ bao lâu.
        /// - Nếu được phép: gửi lại mã, bắt đầu đếm ngược mới.
        /// </summary>
        /// <param name="sender">Nút Gửi lại.</param>
        /// <param name="e">Thông tin sự kiện click.</param>
        private async void btnGuiLai_Click(object sender, EventArgs e)
        {
            int wait;
            if (!EmailVerificationService.CanResend(_email, out wait))
            {
                MessageBox.Show(
                    "Vui lòng đợi " + wait + "s nữa rồi thử lại.",
                    "Chưa thể gửi lại",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                return;
            }

            try
            {
                await EmailVerificationService.SendNewCodeAsync(_email, _sender);
                BatDemNguoc(60);

                MessageBox.Show(
                    "Đã gửi lại mã xác nhận.",
                    "Đã gửi",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Không thể gửi mã: " + ex.Message,
                    "Lỗi",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        #endregion
    }
}
