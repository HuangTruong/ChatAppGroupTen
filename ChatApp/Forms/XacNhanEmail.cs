using System;
using System.Windows.Forms;
using ChatApp.Services.Auth;
using ChatApp.Services.Email;

namespace ChatApp
{
    public partial class XacNhanEmail : Form
    {
        private readonly string _email;
        private int _countdown;

        public XacNhanEmail(string email)
        {
            InitializeComponent();
            _email = email;
        }

        private async void XacNhanEmail_Load(object sender, EventArgs e)
        {
            lblEmail.Text = _email;

            if (EmailVerificationService.CanResend(_email, out _))
            {
                try
                {
                    await EmailVerificationService.SendNewCodeAsync(_email);
                    BatDemNguoc(60);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Không thể gửi mã xác nhận: " + ex.Message,
                        "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void BatDemNguoc(int seconds)
        {
            _countdown = seconds;
            btnGuiLai.Enabled = false;
            timerCooldown.Interval = 1000;
            timerCooldown.Start();
            CapNhatNhanDemNguoc();
        }

        private void CapNhatNhanDemNguoc()
        {
            lblDemNguoc.Text = _countdown > 0
                ? $"Gửi lại mã sau: {_countdown}s"
                : "Bạn có thể gửi lại mã.";
        }

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

        private async void btnGuiLai_Click(object sender, EventArgs e)
        {
            if (!EmailVerificationService.CanResend(_email, out var wait))
            {
                MessageBox.Show($"Vui lòng đợi {wait}s nữa rồi thử lại.",
                    "Chưa thể gửi lại", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                await EmailVerificationService.SendNewCodeAsync(_email);
                BatDemNguoc(60);
                MessageBox.Show("Đã gửi lại mã xác nhận.",
                    "Đã gửi", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Không thể gửi mã: " + ex.Message,
                    "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnXacNhan_Click(object sender, EventArgs e)
        {
            var code = txtMa.Text;
            if (string.IsNullOrWhiteSpace(code))
            {
                MessageBox.Show("Vui lòng nhập mã xác nhận.",
                    "Thiếu mã", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (EmailVerificationService.Verify(_email, code, out var error))
            {
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            else
            {
                MessageBox.Show(error, "Sai mã", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void btnHuy_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}
