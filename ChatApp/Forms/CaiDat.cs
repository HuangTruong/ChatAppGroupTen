using System;
using System.Windows.Forms;
using Guna.UI2.WinForms;

using ChatApp.Controllers;

namespace ChatApp
{
    /// <summary>
    /// Form Cài đặt tài khoản:
    /// - Hiển thị thông tin username, email, avatar.
    /// - Cho phép đổi mật khẩu, đổi email, đổi avatar.
    /// - Cho phép copy username, email.
    /// Toàn bộ logic xử lý được ủy quyền cho <see cref="CaiDatController"/>.
    /// </summary>
    public partial class CatDat : Form, ICaiDatView
    {
        #region ======== Biến / Controller ========

        /// <summary>
        /// Controller xử lý logic cho màn hình Cài đặt.
        /// </summary>
        private readonly CaiDatController _controller;

        #endregion

        #region ======== Khởi tạo Form ========

        /// <summary>
        /// Khởi tạo form Cài đặt với tài khoản và email ban đầu.
        /// </summary>
        /// <param name="taiKhoan">Tài khoản (username) của người dùng.</param>
        /// <param name="email">Email hiện tại của người dùng.</param>
        public CatDat(string taiKhoan, string email)
        {
            InitializeComponent();

            // Tạo controller, truyền view + data ban đầu
            _controller = new CaiDatController(this, taiKhoan, email);
        }

        #endregion

        #region ======== IMPLEMENT ICaiDatView ========

        /// <summary>
        /// Panel chính của màn hình cài đặt (chứa nội dung).
        /// </summary>
        public Panel PnlMain
        {
            get { return pnlMain; }
        }

        /// <summary>
        /// Label tiêu đề "Cài đặt" hoặc tương tự.
        /// </summary>
        public Label LblTitle
        {
            get { return lblTitle; }
        }

        /// <summary>
        /// Label hiển thị tên đăng nhập (username).
        /// </summary>
        public Label LblTenDangNhap
        {
            get { return lblTenDangNhap; }
        }

        /// <summary>
        /// Label hiển thị email.
        /// </summary>
        public Label LblEmail
        {
            get { return lblEmail; }
        }

        /// <summary>
        /// Control textbox tên đăng nhập.
        /// </summary>
        public Control TxtTenDangNhap
        {
            get { return txtTenDangNhap; }
        }

        /// <summary>
        /// Control textbox email.
        /// </summary>
        public Control TxtEmail
        {
            get { return txtEmail; }
        }

        /// <summary>
        /// Nút copy username.
        /// </summary>
        public Control BtnCopyUsername
        {
            get { return btnCopyUsername; }
        }

        /// <summary>
        /// Nút copy email.
        /// </summary>
        public Control BtnCopyEmail
        {
            get { return btnCopyEmail; }
        }

        /// <summary>
        /// Nút đổi mật khẩu.
        /// </summary>
        public Control BtnDoiMatKhau
        {
            get { return btnDoiMatKhau; }
        }

        /// <summary>
        /// Nút đổi email.
        /// </summary>
        public Control BtnDoiEmail
        {
            get { return btnDoiEmail; }
        }

        /// <summary>
        /// Nút đóng form cài đặt.
        /// </summary>
        public Control BtnDong
        {
            get { return btnDong; }
        }

        /// <summary>
        /// Ảnh đại diện (avatar) hình tròn.
        /// </summary>
        public Guna2CirclePictureBox PicAvatar
        {
            get { return picAvatar; }
        }

        /// <summary>
        /// Nút đổi avatar.
        /// </summary>
        public Control BtnDoiAvatar
        {
            get { return btnDoiAvatar; }
        }

        #endregion

        #region ======== EVENT HANDLERS – GỌI CONTROLLER ========

        /// <summary>
        /// Sự kiện Load form Cài đặt:
        /// - Gọi controller để load dữ liệu ban đầu (username, email, avatar...).
        /// </summary>
        private async void CatDat_Load(object sender, EventArgs e)
        {
            await _controller.OnLoadAsync();
        }

        /// <summary>
        /// Sự kiện Paint của control avatar:
        /// - Ủy quyền cho controller vẽ border, overlay... (nếu có).
        /// </summary>
        private void picAvatar_Paint(object sender, PaintEventArgs e)
        {
            _controller.OnAvatarPaint(sender, e);
        }

        /// <summary>
        /// Sự kiện click nút Đổi avatar:
        /// - Gọi controller xử lý việc chọn ảnh mới và cập nhật avatar.
        /// </summary>
        private async void btnDoiAvatar_Click(object sender, EventArgs e)
        {
            await _controller.OnDoiAvatarAsync();
        }

        /// <summary>
        /// Sự kiện click nút Đổi mật khẩu:
        /// - Gọi controller để mở flow đổi mật khẩu (form DoiMatKhau).
        /// </summary>
        private void btnDoiMatKhau_Click(object sender, EventArgs e)
        {
            _controller.OnDoiMatKhau();
        }

        /// <summary>
        /// Sự kiện click nút Đổi email:
        /// - Gọi controller để xử lý đổi email (gửi mã, xác thực, cập nhật...).
        /// </summary>
        private async void btnDoiEmail_Click(object sender, EventArgs e)
        {
            await _controller.OnDoiEmailAsync();
        }

        /// <summary>
        /// Sự kiện click nút Copy username:
        /// - Gọi controller để copy username vào clipboard và hiển thị thông báo (nếu có).
        /// </summary>
        private void btnCopyUsername_Click(object sender, EventArgs e)
        {
            _controller.OnCopyUsername();
        }

        /// <summary>
        /// Sự kiện click nút Copy email:
        /// - Gọi controller để copy email vào clipboard và hiển thị thông báo (nếu có).
        /// </summary>
        private void btnCopyEmail_Click(object sender, EventArgs e)
        {
            _controller.OnCopyEmail();
        }

        /// <summary>
        /// Sự kiện click nút Đóng:
        /// - Gọi controller xử lý logic đóng form (ẩn/đóng, cleanup nếu cần).
        /// </summary>
        private void btnDong_Click(object sender, EventArgs e)
        {
            _controller.OnDong();
        }

        #endregion
    }
}
