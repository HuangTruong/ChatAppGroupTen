using System;
using System.Windows.Forms;
using Guna.UI2.WinForms;

using ChatApp.Controllers;

namespace ChatApp
{
    public partial class CatDat : Form, ICaiDatView
    {
        private readonly CaiDatController _controller;

        public CatDat(string taiKhoan, string email)
        {
            InitializeComponent();

            // Tạo controller, truyền view + data ban đầu
            _controller = new CaiDatController(this, taiKhoan, email);
        }

        // ===== IMPLEMENT ICaiDatView =====

        public Panel PnlMain => pnlMain;
        public Label LblTitle => lblTitle;
        public Label LblTenDangNhap => lblTenDangNhap;
        public Label LblEmail => lblEmail;

        public Control TxtTenDangNhap => txtTenDangNhap;
        public Control TxtEmail => txtEmail;

        public Control BtnCopyUsername => btnCopyUsername;
        public Control BtnCopyEmail => btnCopyEmail;
        public Control BtnDoiMatKhau => btnDoiMatKhau;
        public Control BtnDoiEmail => btnDoiEmail;
        public Control BtnDong => btnDong;

        public Guna2CirclePictureBox PicAvatar => picAvatar;
        public Control BtnDoiAvatar => btnDoiAvatar;

        // ===== EVENT HANDLERS – CHỈ GỌI CONTROLLER =====

        private async void CatDat_Load(object sender, EventArgs e)
        {
            await _controller.OnLoadAsync();
        }

        private void picAvatar_Paint(object sender, PaintEventArgs e)
        {
            _controller.OnAvatarPaint(sender, e);
        }

        private async void btnDoiAvatar_Click(object sender, EventArgs e)
        {
            await _controller.OnDoiAvatarAsync();
        }

        private void btnDoiMatKhau_Click(object sender, EventArgs e)
        {
            _controller.OnDoiMatKhau();
        }

        private async void btnDoiEmail_Click(object sender, EventArgs e)
        {
            await _controller.OnDoiEmailAsync();
        }

        private void btnCopyUsername_Click(object sender, EventArgs e)
        {
            _controller.OnCopyUsername();
        }

        private void btnCopyEmail_Click(object sender, EventArgs e)
        {
            _controller.OnCopyEmail();
        }

        private void btnDong_Click(object sender, EventArgs e)
        {
            _controller.OnDong();
        }
    }
}
