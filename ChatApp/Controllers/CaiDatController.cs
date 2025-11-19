using System;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using Guna.UI2.WinForms;

using ChatApp.Helpers.Ui;
using ChatApp.Services.Auth;
using ChatApp.Services.Firebase;

namespace ChatApp.Controllers
{
    // View interface: form CatDat sẽ implement cái này
    public interface ICaiDatView
    {
        Panel PnlMain { get; }
        Label LblTitle { get; }
        Label LblTenDangNhap { get; }
        Label LblEmail { get; }

        Control TxtTenDangNhap { get; }
        Control TxtEmail { get; }

        Control BtnCopyUsername { get; }
        Control BtnCopyEmail { get; }
        Control BtnDoiMatKhau { get; }
        Control BtnDoiEmail { get; }
        Control BtnDong { get; }

        Guna2CirclePictureBox PicAvatar { get; }
        Control BtnDoiAvatar { get; }
    }

    public class CaiDatController
    {
        private readonly ICaiDatView _view;
        private readonly AuthService _authService;

        private readonly string _taiKhoan;
        private string _email;

        public CaiDatController(ICaiDatView view, string taiKhoan, string email)
        {
            _view = view ?? throw new ArgumentNullException(nameof(view));
            _taiKhoan = !string.IsNullOrWhiteSpace(taiKhoan)
                ? taiKhoan
                : throw new ArgumentNullException(nameof(taiKhoan));
            _email = email ?? string.Empty;

            _authService = new AuthService(FirebaseClientFactory.Create());
        }

        // ====== LIFECYCLE ======

        public async Task OnLoadAsync()
        {
            _view.TxtTenDangNhap.Text = _taiKhoan;
            _view.TxtEmail.Text = _email;

            // Khoá sửa username
            if (_view.TxtTenDangNhap is TextBox tb)
                tb.ReadOnly = true;
            else
                _view.TxtTenDangNhap.Enabled = false;

            // Layout dùng helper
            CaiDatLayoutHelper.CenterTitle(_view.LblTitle, _view.PnlMain);
            CaiDatLayoutHelper.CenterAvatarAndButton(
                _view.PicAvatar,
                _view.BtnDoiAvatar,
                _view.PnlMain);

            CaiDatLayoutHelper.AlignAccountEmailRows(
                _view.LblTenDangNhap, _view.TxtTenDangNhap, _view.BtnCopyUsername,
                _view.LblEmail, _view.TxtEmail, _view.BtnCopyEmail,
                _view.PnlMain);

            CaiDatLayoutHelper.CenterBottomButtons(
                _view.BtnDoiMatKhau,
                _view.BtnDoiEmail,
                _view.BtnDong,
                _view.PnlMain);

            await LoadAvatarAsync();
        }

        private async Task LoadAvatarAsync()
        {
            if (string.IsNullOrWhiteSpace(_taiKhoan)) return;

            try
            {
                string base64 = await _authService.GetAvatarAsync(_taiKhoan);
                if (string.IsNullOrWhiteSpace(base64)) return;

                byte[] bytes = Convert.FromBase64String(base64);
                using (var ms = new MemoryStream(bytes))
                {
                    _view.PicAvatar.Image = Image.FromStream(ms);
                }
            }
            catch
            {
                // Ignore lỗi (network/base64) để form vẫn chạy bình thường
            }
        }

        // ====== VẼ VIỀN AVATAR ======

        public void OnAvatarPaint(object sender, PaintEventArgs e)
        {
            var box = sender as Guna2CirclePictureBox;
            if (box == null) return;

            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            using (var pen = new Pen(Color.FromArgb(0, 120, 215), 3)) // viền xanh
            {
                var rect = new Rectangle(2, 2, box.Width - 4, box.Height - 4);
                e.Graphics.DrawEllipse(pen, rect);
            }
        }

        // ====== ĐỔI AVATAR ======

        public async Task OnDoiAvatarAsync()
        {
            if (string.IsNullOrWhiteSpace(_taiKhoan))
            {
                MessageBox.Show("Không xác định được tài khoản hiện tại.", "Lỗi",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            using (var ofd = new OpenFileDialog())
            {
                ofd.Filter = "Ảnh đại diện (*.jpg;*.jpeg;*.png;*.bmp)|*.jpg;*.jpeg;*.png;*.bmp";
                ofd.Title = "Chọn ảnh đại diện";

                if (ofd.ShowDialog() != DialogResult.OK)
                    return;

                try
                {
                    // Preview trước trên UI
                    using (var img = Image.FromFile(ofd.FileName))
                    {
                        _view.PicAvatar.Image = new Bitmap(img);
                    }

                    // File -> base64
                    byte[] bytes = File.ReadAllBytes(ofd.FileName);
                    string base64 = Convert.ToBase64String(bytes);

                    _view.BtnDoiAvatar.Enabled = false;
                    _view.BtnDoiAvatar.Text = "Đang tải lên...";

                    if (_view is Form f1) f1.UseWaitCursor = true;

                    await _authService.UpdateAvatarAsync(_taiKhoan, base64);

                    MessageBox.Show("Cập nhật ảnh đại diện thành công!", "Thông báo",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi khi cập nhật ảnh đại diện: " + ex.Message, "Lỗi",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                finally
                {
                    _view.BtnDoiAvatar.Enabled = true;
                    _view.BtnDoiAvatar.Text = "Đổi avatar";
                    if (_view is Form f2) f2.UseWaitCursor = false;
                }
            }
        }

        // ====== ĐỔI MẬT KHẨU ======

        public void OnDoiMatKhau()
        {
            if (string.IsNullOrWhiteSpace(_taiKhoan))
            {
                MessageBox.Show("Không xác định được tài khoản hiện tại.", "Lỗi",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (!(_view is Form owner))
            {
                MessageBox.Show("Không thể mở form đổi mật khẩu.", "Lỗi",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var frm = new DoiMatKhau(_taiKhoan)
            {
                StartPosition = FormStartPosition.CenterParent
            };
            frm.ShowDialog(owner);
        }

        // ====== ĐỔI EMAIL ======

        public async Task OnDoiEmailAsync()
        {
            string emailMoi = _view.TxtEmail.Text.Trim();

            if (string.IsNullOrWhiteSpace(emailMoi))
            {
                MessageBox.Show("Email không được để trống.", "Thông báo",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!emailMoi.Contains("@") || !emailMoi.Contains("."))
            {
                MessageBox.Show("Vui lòng nhập email hợp lệ.", "Thông báo",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.Equals(emailMoi, _email, StringComparison.OrdinalIgnoreCase))
            {
                MessageBox.Show("Email mới đang trùng với email hiện tại.", "Thông báo",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var confirm = MessageBox.Show(
                $"Bạn chắc chắn muốn đổi email sang:\n{emailMoi}?",
                "Xác nhận",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (confirm != DialogResult.Yes) return;

            _view.BtnDoiEmail.Enabled = false;
            _view.BtnDoiEmail.Text = "Đang lưu...";
            if (_view is Form f1) f1.UseWaitCursor = true;

            try
            {
                bool exists = await _authService.EmailExistsAsync(emailMoi);
                if (exists)
                {
                    MessageBox.Show("Email này đã được sử dụng cho tài khoản khác.", "Lỗi",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                await _authService.UpdateEmailAsync(_taiKhoan, emailMoi);
                _email = emailMoi;

                MessageBox.Show("Đổi email thành công!", "Thông báo",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Không thể đổi email: " + ex.Message, "Lỗi",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                _view.BtnDoiEmail.Enabled = true;
                _view.BtnDoiEmail.Text = "Đổi email";
                if (_view is Form f2) f2.UseWaitCursor = false;
            }
        }

        // ====== TIỆN ÍCH ======

        public void OnCopyUsername()
        {
            if (!string.IsNullOrWhiteSpace(_taiKhoan))
            {
                Clipboard.SetText(_taiKhoan);
                MessageBox.Show("Đã sao chép tên đăng nhập vào clipboard.", "Thông báo",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        public void OnCopyEmail()
        {
            string email = _view.TxtEmail.Text.Trim();
            if (!string.IsNullOrEmpty(email))
            {
                Clipboard.SetText(email);
                MessageBox.Show("Đã sao chép email vào clipboard.", "Thông báo",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        public void OnDong()
        {
            if (_view is Form f)
                f.Close();
        }
    }
}
