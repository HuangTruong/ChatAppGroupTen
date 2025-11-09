using System;
using System.Drawing;
using System.Windows.Forms;

using ChatApp.Controllers;
using ChatApp.Models.Users;

namespace ChatApp
{
    public partial class TrangChu : Form
    {
        private readonly string _ten; // Tên đăng nhập của user hiện tại
        private readonly TrangChuController _controller = new TrangChuController();

        private NhanTin _nhanTinForm; // Form NhanTin đang mở
        private bool _isOpeningNhanTin = false; // Ngăn mở nhiều form NhanTin cùng lúc

        private Label lblChaoMung; // Label hiển thị chào mừng
        private readonly ToolTip _tipTen = new ToolTip(); // Tooltip hiển thị tên user khi hover

        public TrangChu(string ten)
        {
            InitializeComponent();
            _ten = ten;

            TaoLabelChaoMung();

            // Lắng nghe sự kiện resize của header để căn giữa label chào mừng
            LayHeaderContainer().Resize += (s, e) => CanhGiuaChaoMung();
        }

        #region === Label chào mừng ===

        /// <summary>
        /// Tạo Label chào mừng và thêm vào container header
        /// </summary>
        private void TaoLabelChaoMung()
        {
            lblChaoMung = new Label
            {
                AutoSize = true,
                Text = "Chào mừng",
                Font = new Font("Segoe UI", 14f, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.Transparent,
                TextAlign = ContentAlignment.MiddleCenter,
                Padding = new Padding(6, 0, 6, 0)
            };

            var header = LayHeaderContainer();
            header.Controls.Add(lblChaoMung);
            CanhGiuaChaoMung();
        }

        /// <summary>
        /// Lấy container header để đặt label chào mừng
        /// Nếu lblTenDangNhap tồn tại thì dùng parent của nó, ngược lại trả về form chính
        /// </summary>
        private Control LayHeaderContainer()
        {
            return (lblTenDangNhap != null && lblTenDangNhap.Parent != null) ? (Control)lblTenDangNhap.Parent : this;
        }

        /// <summary>
        /// Cập nhật nội dung label chào mừng theo tên hiển thị
        /// </summary>
        /// <param name="tenHienThi">Tên hiển thị của user</param>
        private void DatNoiDungChaoMung(string tenHienThi)
        {
            string display = string.IsNullOrWhiteSpace(tenHienThi) ? _ten : tenHienThi;
            lblChaoMung.Text = $"Chào mừng '{display}'";
            _tipTen.SetToolTip(lblChaoMung, display);
            CanhGiuaChaoMung();
        }

        /// <summary>
        /// Căn giữa label chào mừng trong container
        /// </summary>
        private void CanhGiuaChaoMung()
        {
            if (lblChaoMung == null) return;
            Control header = LayHeaderContainer();

            lblChaoMung.Left = Math.Max((header.ClientSize.Width - lblChaoMung.Width) / 2, 8);
            lblChaoMung.Top = Math.Max((header.ClientSize.Height - lblChaoMung.Height) / 2, 0);
        }

        #endregion

        #region === Load form ===

        /// <summary>
        /// Xử lý khi form TrangChu load
        /// - Gắn sự kiện Click và DoubleClick cho panel, icon, label NhanTin
        /// - Lấy thông tin user từ controller và hiển thị trên label chào mừng
        /// </summary>
        private async void TrangChu_Load(object sender, EventArgs e)
        {
            foreach (Control c in new Control[] { pnlNhanTin, picNhanTin, lblNhanTin })
            {
                c.Click += pnlNhanTin_Click;
                c.DoubleClick += pnlNhanTin_Click;
            }

            try
            {
                User user = await _controller.GetUserAsync(_ten);
                DatNoiDungChaoMung(user?.Ten ?? _ten);
            }
            catch
            {
                DatNoiDungChaoMung(_ten);
            }
        }

        #endregion

        #region === Mở form NhanTin ===

        /// <summary>
        /// Xử lý sự kiện click/double click mở form NhanTin
        /// - Ngăn mở nhiều form cùng lúc
        /// - Nếu form đã mở, chỉ bring to front
        /// - Khi form NhanTin đóng, show lại TrangChu
        /// </summary>
        private void pnlNhanTin_Click(object sender, EventArgs e)
        {
            if (_isOpeningNhanTin) return;
            _isOpeningNhanTin = true;
            ToggleNhanTinTargets(false);

            try
            {
                if (_nhanTinForm != null && !_nhanTinForm.IsDisposed)
                {
                    _nhanTinForm.WindowState = FormWindowState.Normal;
                    _nhanTinForm.Show();
                    _nhanTinForm.Activate();
                    _nhanTinForm.BringToFront();
                    this.Hide();
                    return;
                }

                _nhanTinForm = new NhanTin(_ten)
                {
                    StartPosition = FormStartPosition.CenterParent
                };
                _nhanTinForm.FormClosed += (s, args) =>
                {
                    _nhanTinForm = null;
                    this.Show();
                };

                _nhanTinForm.Show(this);
                this.Hide();
            }
            finally
            {
                _isOpeningNhanTin = false;
                ToggleNhanTinTargets(true);
            }
        }

        /// <summary>
        /// Bật hoặc tắt các target liên quan đến NhanTin và cursor chờ
        /// </summary>
        private void ToggleNhanTinTargets(bool enabled)
        {
            pnlNhanTin.Enabled = enabled;
            picNhanTin.Enabled = enabled;
            lblNhanTin.Enabled = enabled;
            this.UseWaitCursor = !enabled;
        }

        #endregion

        #region === Đăng xuất / đóng form ===

        /// <summary>
        /// Xử lý sự kiện click đăng xuất
        /// - Đóng form NhanTin nếu đang mở
        /// - Đóng form TrangChu
        /// </summary>
        private void picDangXuat_Click(object sender, EventArgs e)
        {
            if (_nhanTinForm != null && !_nhanTinForm.IsDisposed)
            {
                _nhanTinForm.Close();
                _nhanTinForm = null;
            }

            this.Close(); // Cập nhật trạng thái offline sẽ được xử lý trong OnFormClosed
        }

        /// <summary>
        /// Xử lý khi form đóng
        /// - Cập nhật trạng thái user thành offline
        /// </summary>
        protected override async void OnFormClosed(FormClosedEventArgs e)
        {
            await _controller.CapNhatTrangThaiAsync(_ten, "offline");
            base.OnFormClosed(e);
        }

        #endregion
    }
}
