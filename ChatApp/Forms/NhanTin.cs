using ChatApp.Controllers;
using ChatApp.Helpers.UI;
using Guna.UI2.WinForms;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace ChatApp
{
    /// <summary>
    /// Form màn hình Nhắn tin chính:
    /// - Hiển thị danh sách cuộc trò chuyện / bạn bè.
    /// - Hiển thị khung chat giữa người dùng và đối tượng đang chọn.
    /// - Gửi / nhận tin nhắn, hiển thị trạng thái "đang nhập...".
    /// </summary>
    public partial class NhanTin : Form, INhanTinView
    {
        #region ======== Biến / Controller ========

        /// <summary>
        /// Tên đăng nhập (ví dụ: email đăng nhập).
        /// </summary>
        private readonly string _tenDangNhap;

        /// <summary>
        /// Tên người dùng (username) trong Firebase.
        /// </summary>
        private readonly string _tenNguoiDung;

        /// <summary>
        /// Controller xử lý logic cho màn hình Nhắn tin.
        /// </summary>
        private readonly NhanTinController _controller;

        /// <summary>
        /// Label hiển thị trạng thái "đang nhập..." dưới tiêu đề hội thoại.
        /// </summary>
        private readonly Label _lblTyping;

        #endregion

        #region ======== Khởi tạo Form ========

        /// <summary>
        /// Khởi tạo form Nhắn tin với thông tin đăng nhập và tên người dùng.
        /// </summary>
        /// <param name="tenDangNhap">Tên đăng nhập (email hoặc username đăng nhập).</param>
        /// <param name="tenNguoiDung">Tên người dùng hiển thị trong Firebase.</param>
        public NhanTin(string tenDangNhap, string tenNguoiDung, bool DayNightMode)
        {
            InitializeComponent();

            if (string.IsNullOrWhiteSpace(tenDangNhap))
                throw new ArgumentNullException("tenDangNhap");
            if (string.IsNullOrWhiteSpace(tenNguoiDung))
                throw new ArgumentNullException("tenNguoiDung");

            _tenDangNhap = tenDangNhap;
            _tenNguoiDung = tenNguoiDung;

            // Tạo label "đang nhập..."
            _lblTyping = new Label();
            _lblTyping.AutoSize = true;
            _lblTyping.ForeColor = Color.DimGray;
            _lblTyping.Text = string.Empty;
            _lblTyping.Visible = false;
            _lblTyping.Location = new Point(
                lblTenDangNhapGiua.Left,
                lblTenDangNhapGiua.Bottom + 4
            );

            pnlNguoiChat.Controls.Add(_lblTyping);
            _lblTyping.BringToFront();

            // Cho phép form bắt phím trước khi control con xử lý
            this.KeyPreview = true;

            // Khởi tạo controller
            _controller = new NhanTinController(this, _tenNguoiDung);

            // Gán event nếu chưa gắn trong Designer
            //this.Load += NhanTin_Load;
            //btnGui.Click += btnGui_Click;
            txtTimKiem.TextChanged += txtTimKiem_TextChanged;

            // Set chế độ ngày đêm cho Nhắn Tin
            if (!DayNightMode)
                ThemeManager.ApplyDayTheme(this);
            else
                ThemeManager.ApplyNightTheme(this);
        }

        #endregion

        #region ======== SỰ KIỆN FORM ========

        /// <summary>
        /// Sự kiện Load của form Nhắn tin:
        /// - Gọi controller để khởi tạo dữ liệu (danh sách chat, kết nối Firebase...).
        /// </summary>
        private async void NhanTin_Load(object sender, EventArgs e)
        {
            try
            {
                await _controller.InitAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Lỗi khởi tạo màn hình nhắn tin: " + ex.Message,
                    "Lỗi",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }

        /// <summary>
        /// Sự kiện khi form Nhắn tin bị đóng:
        /// - Gọi controller set offline (nếu có logic).
        /// - Dispose controller để giải phóng tài nguyên.
        /// </summary>
        /// <param name="e">Thông tin sự kiện đóng form.</param>
        protected override async void OnFormClosed(FormClosedEventArgs e)
        {
            try
            {
                await _controller.SetOfflineAsync();
            }
            catch
            {
                // Bỏ qua lỗi nhỏ (mạng, Firebase...)
            }

            if (_controller != null)
            {
                _controller.Dispose();
            }

            base.OnFormClosed(e);
        }

        /// <summary>
        /// Sự kiện click nút Gửi:
        /// - Gọi controller để gửi tin nhắn hiện tại trong textbox.
        /// </summary>
        private async void btnGui_Click(object sender, EventArgs e)
        {
            await _controller.GuiTinNhanHienTaiAsync();
        }

        #endregion

        #region ======== TRIỂN KHAI INhanTinView ========

        /// <summary>
        /// Panel hiển thị danh sách cuộc hội thoại / bạn bè.
        /// </summary>
        public FlowLayoutPanel DanhSachChatPanel
        {
            get { return flpDanhSachChat; }
        }

        /// <summary>
        /// Panel hiển thị các tin nhắn trong cuộc hội thoại hiện tại.
        /// </summary>
        public FlowLayoutPanel KhungChatPanel
        {
            get { return flbKhungChat; }
        }

        /// <summary>
        /// TextBox nhập nội dung tin nhắn.
        /// </summary>
        public Guna2TextBox TxtNhapTin
        {
            get { return txtNhapTinNhan; }
        }

        /// <summary>
        /// Label tiêu đề ở giữa (tên người đang chat / nhóm).
        /// </summary>
        public Label LblTieuDeGiua
        {
            get { return lblTenDangNhapGiua; }
        }

        /// <summary>
        /// Label hiển thị tên đăng nhập ở góc phải (nếu có).
        /// </summary>
        public Label LblTenDangNhapPhai
        {
            get { return lblTenDangNhapPhai; }
        }

        /// <summary>
        /// Label hiển thị trạng thái "đang nhập..." bên dưới tiêu đề.
        /// </summary>
        public Label LblTyping
        {
            get { return _lblTyping; }
        }

        /// <summary>
        /// Từ khóa tìm kiếm hiện tại trong textbox tìm kiếm.
        /// </summary>
        public string CurrentSearchKeyword
        {
            get { return txtTimKiem.Text; }
        }

        /// <summary>
        /// Hiển thị thông báo dạng MessageBox thông thường.
        /// </summary>
        /// <param name="message">Nội dung thông báo.</param>
        public void ShowInfo(string message)
        {
            MessageBox.Show(
                message,
                "Thông báo",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information
            );
        }

        /// <summary>
        /// Hiển thị hộp thoại xác nhận Yes/No.
        /// </summary>
        /// <param name="message">Nội dung câu hỏi.</param>
        /// <param name="title">Tiêu đề hộp thoại.</param>
        /// <returns>Kết quả người dùng chọn Yes hoặc No.</returns>
        public DialogResult ShowConfirm(string message, string title)
        {
            return MessageBox.Show(
                message,
                title,
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );
        }

        #endregion

        #region ======== TÌM KIẾM NGƯỜI DÙNG / TẠO NHÓM ========

        /// <summary>
        /// Sự kiện khi text tìm kiếm thay đổi:
        /// - Gọi controller để xử lý filter danh sách người dùng / hội thoại.
        /// </summary>
        private async void txtTimKiem_TextChanged(object sender, EventArgs e)
        {
            await _controller.HandleSearchTextChangedAsync(txtTimKiem.Text);
        }

        /// <summary>
        /// Sự kiện click nút Tạo nhóm:
        /// - Gọi controller để xử lý flow tạo nhóm mới.
        /// </summary>
        private async void btnTaoNhom_Click(object sender, EventArgs e)
        {
            try
            {
                await _controller.HandleCreateGroupClickedAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Không thể tạo nhóm: " + ex.Message,
                    "Lỗi",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }

        #endregion
    }
}
