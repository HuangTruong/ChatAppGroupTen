using System;
using System.Drawing;
using System.Windows.Forms;
using ChatApp.Controllers;
using Guna.UI2.WinForms;

namespace ChatApp
{
    public partial class NhanTin : Form, INhanTinView
    {
        private readonly string _tenDangNhap;   // vd: email đăng nhập
        private readonly string _tenNguoiDung;  // username trong Firebase
        private readonly NhanTinController _controller;

        // Label hiển thị trạng thái "đang nhập..."
        private readonly Label _lblTyping;

        public NhanTin(string tenDangNhap, string tenNguoiDung)
        {
            InitializeComponent();

            if (string.IsNullOrWhiteSpace(tenDangNhap))
                throw new ArgumentNullException(nameof(tenDangNhap));
            if (string.IsNullOrWhiteSpace(tenNguoiDung))
                throw new ArgumentNullException(nameof(tenNguoiDung));

            _tenDangNhap = tenDangNhap;
            _tenNguoiDung = tenNguoiDung;

            // Label "đang nhập..."
            _lblTyping = new Label
            {
                AutoSize = true,
                ForeColor = Color.DimGray,
                Text = string.Empty,
                Visible = false
            };
            _lblTyping.Location = new Point(
                lblTenDangNhapGiua.Left,
                lblTenDangNhapGiua.Bottom + 4
            );
            pnlNguoiChat.Controls.Add(_lblTyping);
            _lblTyping.BringToFront();

            this.KeyPreview = true;

            // Khởi tạo controller
            _controller = new NhanTinController(this, _tenNguoiDung);

            // Hook event nếu chưa gắn trong Designer
            this.Load += NhanTin_Load;
            btnGui.Click += btnGui_Click;
            txtTimKiem.TextChanged += txtTimKiem_TextChanged;
        }

        // ================== SỰ KIỆN FORM ==================

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

        protected override async void OnFormClosed(FormClosedEventArgs e)
        {
            try
            {
                await _controller.SetOfflineAsync();
            }
            catch
            {
                // bỏ qua lỗi nhỏ
            }

            _controller?.Dispose();
            base.OnFormClosed(e);
        }

        private async void btnGui_Click(object sender, EventArgs e)
        {
            await _controller.GuiTinNhanHienTaiAsync();
        }

        // ================== TRIỂN KHAI INhanTinView ==================

        public FlowLayoutPanel DanhSachChatPanel => flpDanhSachChat;
        public FlowLayoutPanel KhungChatPanel => flbKhungChat;
        public Guna2TextBox TxtNhapTin => txtNhapTinNhan;
        public Label LblTieuDeGiua => lblTenDangNhapGiua;
        public Label LblTenDangNhapPhai => lblTenDangNhapPhai;
        public Label LblTyping => _lblTyping;

        public string CurrentSearchKeyword => txtTimKiem.Text;

        public void ShowInfo(string message)
        {
            MessageBox.Show(
                message,
                "Thông báo",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information
            );
        }

        public DialogResult ShowConfirm(string message, string title)
        {
            return MessageBox.Show(
                message,
                title,
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );
        }

        // ================== TÌM KIẾM NGƯỜI DÙNG ==================

        private async void txtTimKiem_TextChanged(object sender, EventArgs e)
        {
            await _controller.HandleSearchTextChangedAsync(txtTimKiem.Text);
        }
    }
}
