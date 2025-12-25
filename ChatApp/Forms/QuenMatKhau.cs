using System;
using System.Windows.Forms;
using Guna.UI2.WinForms;
using ChatApp.Controllers;

namespace ChatApp
{
    /// <summary>
    /// Form Quên mật khẩu:
    /// - Nhập email đã đăng ký.
    /// - Gửi yêu cầu reset mật khẩu (link Firebase gửi qua email).
    /// - Cho phép quay lại form Đăng nhập.
    /// </summary>
    public partial class QuenMatKhau : Form
    {
        #region ====== FIELDS ======

        /// <summary>
        /// Controller xử lý logic quên mật khẩu (gọi Firebase / AuthService).
        /// </summary>
        private readonly ForgotPasswordController _controller = new ForgotPasswordController();

        /// <summary>
        /// Cờ phân biệt trường hợp user bấm nút "Quay lại đăng nhập"
        /// với việc tắt form bằng nút X.
        /// </summary>
        private bool isClosed = false;

        #endregion

        #region ====== KHỞI TẠO FORM ======

        /// <summary>
        /// Khởi tạo form Quên mật khẩu, gán sự kiện load form.
        /// </summary>
        public QuenMatKhau()
        {
            InitializeComponent();

            // Gán sự kiện load form
            this.Load += QuenMatKhau_Load;
        }

        /// <summary>
        /// Khi form được load:
        /// - Gắn sự kiện click cho nút Xác nhận.
        /// </summary>
        private void QuenMatKhau_Load(object sender, EventArgs e)
        {
            // Đảm bảo chỉ gắn handler một lần
            btnXacNhan.Click -= btnXacNhan_Click;
            btnXacNhan.Click += btnXacNhan_Click;
        }

        #endregion

        #region ====== XỬ LÝ XÁC NHẬN (GỬI LINK RESET) ======

        /// <summary>
        /// Sự kiện bấm nút "Xác nhận":
        /// - Kiểm tra email hợp lệ.
        /// - Gọi controller để gửi email reset mật khẩu.
        /// - Thông báo kết quả cho người dùng.
        /// </summary>
        private async void btnXacNhan_Click(object sender, EventArgs e)
        {
            // Nếu đang disable (đã bấm rồi) thì bỏ qua
            if (!btnXacNhan.Enabled)
            {
                return;
            }

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

            // Từ đây trở đi bắt đầu xử lý → khóa nút ngay để tránh spam
            DoiTrangThaiNut(btnXacNhan, false);

            try
            {
                // 1) Gửi yêu cầu quên mật khẩu (Firebase sẽ gửi link reset về email)
                bool success = await _controller.QuenMatKhauAsync(email);

                if (success)
                {
                    MessageBox.Show(
                        "Đã gửi link đặt lại mật khẩu đến email của bạn. Vui lòng kiểm tra hộp thư.",
                        "Thông báo",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show(
                        "Không tìm thấy tài khoản với email này hoặc có lỗi xảy ra!",
                        "Lỗi",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Lỗi khi gửi email: " + ex.Message,
                    "Lỗi",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            finally
            {
                // Mở lại nút, tắt cursor chờ
                DoiTrangThaiNut(btnXacNhan, true);
            }
        }

        #endregion

        #region ====== TIỆN ÍCH ======

        /// <summary>
        /// Thay đổi trạng thái nút (enable/disable),
        /// đồng thời quản lý AcceptButton và con trỏ chờ.
        /// </summary>
        /// <param name="btn">Nút Guna2Button cần thay đổi trạng thái.</param>
        /// <param name="enable">true nếu bật nút, false nếu vô hiệu hóa.</param>
        private void DoiTrangThaiNut(Guna2Button btn, bool enable)
        {
            btn.Enabled = enable;

            // ENTER chỉ active khi nút đang enable
            this.AcceptButton = enable ? btn : null;

            // Hiển thị cursor chờ khi đang xử lý
            this.UseWaitCursor = !enable;
        }

        #endregion

        #region ====== QUAY LẠI ĐĂNG NHẬP / ĐÓNG FORM ======

        /// <summary>
        /// Sự kiện nút "Quay lại đăng nhập":
        /// - Đặt cờ isClosed và đóng form.
        /// Form gọi bên ngoài sẽ quyết định hiển thị lại form Đăng nhập.
        /// </summary>
        private void btnQuayLaiDangNhap_Click(object sender, EventArgs e)
        {
            isClosed = true;
            this.Close();
        }

        /// <summary>
        /// Sự kiện khi form Quên mật khẩu bị đóng.
        /// Ở đây có cờ isClosed để tránh vòng lặp đóng form.
        /// </summary>
        private void QuenMatKhau_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (!isClosed)
            {
                this.Close();
            }
        }

        #endregion
    }
}
