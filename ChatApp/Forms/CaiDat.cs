using System;
using System.Drawing;
using System.Windows.Forms;
using Guna.UI2.WinForms;

using ChatApp.Controllers;

namespace ChatApp
{
    /// <summary>
    /// Form Cài đặt tài khoản:
    /// - Đổi avatar.
    /// - Đổi mật khẩu.
    /// - Đổi tên hiển thị.
    /// </summary>
    public partial class CatDat : Form
    {
        #region ====== FIELDS ======

        /// <summary>
        /// Controller xử lý logic cài đặt tài khoản (avatar, mật khẩu, tên hiển thị).
        /// </summary>
        private readonly CaiDatController _controller;

        /// <summary>
        /// Mã người dùng Firebase (localId).
        /// </summary>
        private readonly string _localId;

        /// <summary>
        /// Token đăng nhập hiện tại.
        /// </summary>
        private readonly string _token;

        #endregion

        #region ====== KHỞI TẠO FORM ======

        /// <summary>
        /// Khởi tạo form Cài đặt với localId và token hiện tại.
        /// </summary>
        /// <param name="localId">Mã người dùng Firebase.</param>
        /// <param name="token">Token đăng nhập.</param>
        public CatDat(string localId, string token)
        {
            InitializeComponent();

            _localId = localId;
            _token = token;

            // Tạo controller, truyền dữ liệu ban đầu
            _controller = new CaiDatController(localId, token);
        }

        #endregion

        #region ====== SỰ KIỆN FORM LOAD ======

        /// <summary>
        /// Khi form Cài đặt được load:
        /// - Tải avatar hiện tại của người dùng và hiển thị lên picAvatar.
        /// </summary>
        private async void CatDat_Load(object sender, EventArgs e)
        {
            var img = await _controller.LoadAvatarAsync();
            if (img != null)
            {
                picAvatar.Image = img;
            }
        }

        #endregion

        #region ====== ĐỔI AVATAR ======

        /// <summary>
        /// Sự kiện nút "Đổi avatar":
        /// - Mở hộp thoại chọn file ảnh.
        /// - Nếu chọn xong thì gọi controller cập nhật avatar.
        /// - Cập nhật lại ảnh hiển thị nếu thành công.
        /// </summary>
        private async void btnDoiAvatar_Click(object sender, EventArgs e)
        {
            using (var ofd = new OpenFileDialog())
            {
                ofd.Filter = "Ảnh đại diện|*.jpg;*.jpeg;*.png;*.bmp";

                // Nếu người dùng không chọn file thì thoát
                if (ofd.ShowDialog() != DialogResult.OK)
                {
                    return;
                }

                bool ok = await _controller.UpdateAvatarAsync(ofd.FileName);

                if (ok)
                {
                    picAvatar.Image = Image.FromFile(ofd.FileName);
                    MessageBox.Show("Cập nhật avatar thành công!", "Thông báo",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        #endregion

        #region ====== ĐỔI MẬT KHẨU ======

        /// <summary>
        /// Sự kiện nút "Đổi mật khẩu":
        /// - Lấy mật khẩu mới từ textbox.
        /// - Gọi controller để đổi mật khẩu trên Firebase.
        /// </summary>
        private async void btnDoiMatKhau_Click(object sender, EventArgs e)
        {
            string newPass = txtMatKhau.Text;

            bool ok = await _controller.ChangePasswordAsync(newPass);

            if (ok)
            {
                MessageBox.Show("Đổi mật khẩu thành công!", "Thông báo",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        #endregion

        #region ====== ĐỔI TÊN HIỂN THỊ ======

        /// <summary>
        /// Sự kiện nút "Đổi tên đăng nhập" / "Đổi tên hiển thị":
        /// - Lấy tên mới từ textbox.
        /// - Gọi controller cập nhật tên hiển thị trong database.
        /// </summary>
        private async void btnDoiTenDangNhap_Click(object sender, EventArgs e)
        {
            string name = txtTenDangNhap.Text.Trim();

            bool ok = await _controller.ChangeUsernameAsync(name);

            if (ok)
            {
                MessageBox.Show("Đổi tên hiển thị thành công!", "Thông báo",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        #endregion

        #region ====== ĐÓNG FORM ======

        /// <summary>
        /// Sự kiện nút "Đóng" – đóng form Cài đặt.
        /// </summary>
        private void btnDong_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        #endregion
    }
}
