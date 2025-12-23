using System;
using System.Windows.Forms;
using ChatApp.Services.Auth;
using ChatApp.Services.Email;

namespace ChatApp
{
    /// <summary>
    /// Form xác nhận email:
    /// - Hiển thị email cần xác nhận.
    /// - Nhập mã xác nhận đã gửi qua email.
    /// - Cho phép gửi lại mã với cơ chế đếm ngược / cooldown.
    /// </summary>
    public partial class XacNhanEmail : Form
    {
        #region ====== FIELDS ======

        /// <summary>
        /// Địa chỉ email cần xác nhận.
        /// </summary>
        private readonly string _email;

        /// <summary>
        /// Thời gian đếm ngược (tính theo giây) cho lần gửi lại mã.
        /// </summary>
        private int _countdown;

        #endregion

        #region ====== KHỞI TẠO & LOAD ======

        /// <summary>
        /// Khởi tạo form xác nhận email với email cần xác thực.
        /// </summary>
        /// <param name="email">Email của người dùng.</param>
        public XacNhanEmail(string email)
        {
            InitializeComponent();
            _email = email;
        }

        /// <summary>
        /// Sự kiện khi form được load:
        /// - Hiển thị email.
        /// - Nếu được phép gửi mã, gửi mã mới và bắt đầu đếm ngược.
        /// </summary>
        private async void XacNhanEmail_Load(object sender, EventArgs e)
        {
            lblEmail.Text = _email;

            // Nếu còn được phép gửi mã
            if (EmailVerificationService.CanResend(_email, out _))
            {
                try
                {
                    // Gửi mã xác nhận lần đầu
                    await EmailVerificationService.SendNewCodeAsync(_email);
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
        }

        #endregion

        #region ====== XỬ LÝ ĐẾM NGƯỢC ======

        /// <summary>
        /// Bắt đầu đếm ngược cho nút gửi lại mã.
        /// </summary>
        /// <param name="seconds">Số giây cần đếm ngược.</param>
        private void BatDemNguoc(int seconds)
        {
            _countdown = seconds;
            btnGuiLai.Enabled = false;
            timerCooldown.Interval = 1000;
            timerCooldown.Start();
            CapNhatNhanDemNguoc();
        }

        /// <summary>
        /// Cập nhật nội dung label hiển thị thời gian đếm ngược.
        /// </summary>
        private void CapNhatNhanDemNguoc()
        {
            lblDemNguoc.Text = _countdown > 0
                ? $"Gửi lại mã sau: {_countdown}s"
                : "Bạn có thể gửi lại mã.";
        }

        /// <summary>
        /// Sự kiện Tick của timer đếm ngược:
        /// - Giảm thời gian.
        /// - Khi hết giờ thì cho phép gửi lại mã.
        /// </summary>
        private void timerCooldown_Tick(object sender, EventArgs e)
        {
            _countdown--;

            if (_countdown <= 0)
            {
                timerCooldown.Stop();
                btnGuiLai.Enabled = true;
            }

            CapNhatNhanDemNguoc();
        }

        #endregion

        #region ====== NÚT GỬI LẠI MÃ ======

        /// <summary>
        /// Sự kiện nút "Gửi lại mã":
        /// - Kiểm tra còn trong thời gian chờ hay không.
        /// - Nếu được phép, gửi lại mã mới và bắt đầu đếm ngược.
        /// </summary>
        private async void btnGuiLai_Click(object sender, EventArgs e)
        {
            if (!EmailVerificationService.CanResend(_email, out var wait))
            {
                MessageBox.Show(
                    $"Vui lòng đợi {wait}s nữa rồi thử lại.",
                    "Chưa thể gửi lại",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                return;
            }

            try
            {
                await EmailVerificationService.SendNewCodeAsync(_email);
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

        #region ====== NÚT XÁC NHẬN MÃ ======

        /// <summary>
        /// Sự kiện nút "Xác nhận":
        /// - Kiểm tra người dùng đã nhập mã hay chưa.
        /// - Gọi EmailVerificationService.Verify để xác thực mã.
        /// - Thành công → DialogResult = OK, thất bại → hiển thị lỗi.
        /// </summary>
        private void btnXacNhan_Click(object sender, EventArgs e)
        {
            var code = txtMa.Text;

            if (string.IsNullOrWhiteSpace(code))
            {
                MessageBox.Show(
                    "Vui lòng nhập mã xác nhận.",
                    "Thiếu mã",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                return;
            }

            if (EmailVerificationService.Verify(_email, code, out var error))
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

        #region ====== NÚT HỦY ======

        /// <summary>
        /// Sự kiện nút "Hủy": đóng form và trả về DialogResult.Cancel.
        /// </summary>
        private void btnHuy_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        #endregion
    }
}
