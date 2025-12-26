using System;
using System.Drawing;
using System.Windows.Forms;

using ChatApp.Controllers;

namespace ChatApp
{
    public partial class CatDat : Form
    {
        #region ====== FIELDS ======

        private readonly CaiDatController _controller;
        private readonly string _localId;
        private readonly string _token;

        #endregion

        public CatDat(string localId, string token)
        {
            InitializeComponent();

            _localId = localId;
            _token = token;

            _controller = new CaiDatController(localId, token);
        }

        private async void CatDat_Load(object sender, EventArgs e)
        {
            var profile = await _controller.LoadProfileAsync();
            if (profile != null)
            {
                txtEmail.Text = profile.Email ?? string.Empty;
                txtTenDangNhap.Text = profile.UserName ?? string.Empty;
                txtTenHienThi.Text = profile.DisplayName ?? string.Empty;
                txtGioiTinh.Text = profile.Gender ?? string.Empty;
                txtNgaySinh.Text = profile.Birthday ?? string.Empty;
            }

            var img = await _controller.LoadAvatarAsync();
            picAvatar.Image = img ?? Properties.Resources.DefaultAvatar;
        }

        private async void btnDoiAvatar_Click(object sender, EventArgs e)
        {
            using (var ofd = new OpenFileDialog())
            {
                ofd.Filter = "Ảnh đại diện|*.jpg;*.jpeg;*.png;*.bmp";

                if (ofd.ShowDialog() != DialogResult.OK) return;

                bool ok = await _controller.UpdateAvatarAsync(ofd.FileName);
                if (ok)
                {
                    picAvatar.Image = Image.FromFile(ofd.FileName);
                    MessageBox.Show("Cập nhật avatar thành công!", "Thông báo",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        private async void btnDoiMatKhau_Click(object sender, EventArgs e)
        {
            bool ok = await _controller.ChangePasswordAsync(txtMatKhau.Text);
            if (ok)
            {
                MessageBox.Show("Đổi mật khẩu thành công!", "Thông báo",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private async void btnDoiTenDangNhap_Click(object sender, EventArgs e)
        {
            bool ok = await _controller.ChangeUserNameAsync(txtTenDangNhap.Text);
            if (ok)
            {
                MessageBox.Show("Đổi tên đăng nhập thành công!", "Thông báo",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private async void btnDoiTenHienThi_Click(object sender, EventArgs e)
        {
            bool ok = await _controller.ChangeDisplayNameAsync(txtTenHienThi.Text);
            if (ok)
            {
                MessageBox.Show("Đổi tên hiển thị thành công!", "Thông báo",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private async void btnDoiGioiTinh_Click(object sender, EventArgs e)
        {
            bool ok = await _controller.ChangeGenderAsync(txtGioiTinh.Text);
            if (ok)
            {
                MessageBox.Show("Cập nhật giới tính thành công!", "Thông báo",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private async void btnDoiNgaySinh_Click(object sender, EventArgs e)
        {
            bool ok = await _controller.ChangeBirthdayAsync(txtNgaySinh.Text);
            if (ok)
            {
                MessageBox.Show("Cập nhật ngày sinh thành công!", "Thông báo",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void btnDong_Click(object sender, EventArgs e)
        {
            Close();
        }

    }
}
