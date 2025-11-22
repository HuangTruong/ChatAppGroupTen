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
    #region ======== ICaiDatView – Interface cho Form Cài đặt ========

    /// <summary>
    /// Interface view cho màn hình cài đặt (Form <c>CaiDat</c> sẽ implement).
    /// Cung cấp các control cần thiết để controller thao tác:
    /// - Panel bố cục chính.
    /// - Label tiêu đề, tên đăng nhập, email.
    /// - Textbox hiển thị username, email.
    /// - Các nút thao tác (copy, đổi mật khẩu, đổi email, đóng).
    /// - Avatar và nút đổi avatar.
    /// </summary>
    public interface ICaiDatView
    {
        /// <summary>
        /// Panel chứa nội dung chính của form cài đặt.
        /// </summary>
        Panel PnlMain { get; }

        /// <summary>
        /// Label tiêu đề cài đặt.
        /// </summary>
        Label LblTitle { get; }

        /// <summary>
        /// Label mô tả dòng "Tên đăng nhập".
        /// </summary>
        Label LblTenDangNhap { get; }

        /// <summary>
        /// Label mô tả dòng "Email".
        /// </summary>
        Label LblEmail { get; }

        /// <summary>
        /// Control hiển thị tên đăng nhập (thường là TextBox).
        /// </summary>
        Control TxtTenDangNhap { get; }

        /// <summary>
        /// Control hiển thị email (thường là TextBox).
        /// </summary>
        Control TxtEmail { get; }

        /// <summary>
        /// Nút copy tên đăng nhập vào clipboard.
        /// </summary>
        Control BtnCopyUsername { get; }

        /// <summary>
        /// Nút copy email vào clipboard.
        /// </summary>
        Control BtnCopyEmail { get; }

        /// <summary>
        /// Nút mở form đổi mật khẩu.
        /// </summary>
        Control BtnDoiMatKhau { get; }

        /// <summary>
        /// Nút thực hiện đổi email.
        /// </summary>
        Control BtnDoiEmail { get; }

        /// <summary>
        /// Nút đóng form cài đặt.
        /// </summary>
        Control BtnDong { get; }

        /// <summary>
        /// Ảnh avatar hình tròn của người dùng.
        /// </summary>
        Guna2CirclePictureBox PicAvatar { get; }

        /// <summary>
        /// Nút chọn file ảnh và đổi avatar.
        /// </summary>
        Control BtnDoiAvatar { get; }
    }

    #endregion

    /// <summary>
    /// Controller xử lý logic cho màn hình cài đặt tài khoản:
    /// - Hiển thị username, email hiện tại.
    /// - Tải và vẽ avatar từ Firebase.
    /// - Cho phép đổi avatar (chọn file, upload base64).
    /// - Cho phép đổi email (kèm kiểm tra hợp lệ, trùng, tồn tại).
    /// - Mở form đổi mật khẩu.
    /// - Cung cấp các tiện ích copy username/email, đóng form.
    /// </summary>
    public class CaiDatController
    {
        #region ======== Trường / Services / State ========

        /// <summary>
        /// View cài đặt mà controller điều khiển.
        /// </summary>
        private readonly ICaiDatView _view;

        /// <summary>
        /// Service xác thực dùng để cập nhật avatar, email, kiểm tra tồn tại email.
        /// </summary>
        private readonly AuthService _authService;

        /// <summary>
        /// Tài khoản (username) hiện tại.
        /// </summary>
        private readonly string _taiKhoan;

        /// <summary>
        /// Email hiện tại (cache local, đồng bộ với Firebase sau khi đổi).
        /// </summary>
        private string _email;

        #endregion

        #region ======== Constructor ========

        /// <summary>
        /// Khởi tạo controller cài đặt:
        /// - Lưu lại view và thông tin tài khoản/email.
        /// - Khởi tạo <see cref="AuthService"/> với Firebase client mới.
        /// </summary>
        /// <param name="view">View cài đặt (Form implement <see cref="ICaiDatView"/>).</param>
        /// <param name="taiKhoan">Tên tài khoản hiện tại.</param>
        /// <param name="email">Email hiện tại.</param>
        public CaiDatController(ICaiDatView view, string taiKhoan, string email)
        {
            _view = view ?? throw new ArgumentNullException(nameof(view));
            _taiKhoan = !string.IsNullOrWhiteSpace(taiKhoan)
                ? taiKhoan
                : throw new ArgumentNullException(nameof(taiKhoan));
            _email = email ?? string.Empty;

            _authService = new AuthService(FirebaseClientFactory.Create());
        }

        #endregion

        #region ======== LIFECYCLE – OnLoad ========

        /// <summary>
        /// Xử lý khi form cài đặt được load:
        /// - Đổ dữ liệu username + email vào textbox.
        /// - Khoá sửa username (chỉ đọc).
        /// - Căn layout tiêu đề, avatar, các dòng username/email, các nút phía dưới
        ///   bằng <see cref="CaiDatLayoutHelper"/>.
        /// - Tải avatar từ Firebase (nếu có).
        /// </summary>
        public async Task OnLoadAsync()
        {
            _view.TxtTenDangNhap.Text = _taiKhoan;
            _view.TxtEmail.Text = _email;

            // Khoá sửa username
            if (_view.TxtTenDangNhap is TextBox tb)
            {
                tb.ReadOnly = true;
            }
            else
            {
                _view.TxtTenDangNhap.Enabled = false;
            }

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

        /// <summary>
        /// Tải avatar hiện tại của người dùng từ Firebase:
        /// - Gọi <see cref="AuthService.GetAvatarAsync"/> lấy chuỗi base64.
        /// - Nếu có dữ liệu, convert sang <see cref="Image"/> và gán vào <see cref="ICaiDatView.PicAvatar"/>.
        /// - Bắt mọi lỗi network/base64 nhưng bỏ qua để form vẫn chạy bình thường.
        /// </summary>
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

        #endregion

        #region ======== VẼ VIỀN AVATAR ========

        /// <summary>
        /// Vẽ viền avatar hình tròn:
        /// - Dùng màu xanh <c>Color.FromArgb(0, 120, 215)</c>.
        /// - Độ dày 3px, anti-alias để đường tròn mịn.
        /// </summary>
        /// <param name="sender">Control avatar, cần là <see cref="Guna2CirclePictureBox"/>.</param>
        /// <param name="e">Đối tượng <see cref="PaintEventArgs"/> cung cấp Graphics.</param>
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

        #endregion

        #region ======== ĐỔI AVATAR ========

        /// <summary>
        /// Xử lý luồng đổi avatar:
        /// - Kiểm tra tài khoản hiện tại hợp lệ.
        /// - Mở <see cref="OpenFileDialog"/> cho phép chọn file ảnh (jpg, jpeg, png, bmp).
        /// - Preview ảnh mới trên UI.
        /// - Đọc file -> base64 và gọi <see cref="AuthService.UpdateAvatarAsync"/> để lưu lên Firebase.
        /// - Trong lúc upload, disable nút "Đổi avatar" và bật UseWaitCursor nếu view là Form.
        /// - Hiển thị thông báo thành công / lỗi cho người dùng.
        /// </summary>
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

        #endregion

        #region ======== ĐỔI MẬT KHẨU ========

        /// <summary>
        /// Mở form đổi mật khẩu:
        /// - Kiểm tra đã xác định được tài khoản hiện tại hay chưa.
        /// - Kiểm tra view có phải là <see cref="Form"/> để làm owner cho dialog.
        /// - Mở form <see cref="DoiMatKhau"/> dạng modal (ShowDialog).
        /// </summary>
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

        #endregion

        #region ======== ĐỔI EMAIL ========

        /// <summary>
        /// Xử lý luồng đổi email:
        /// - Lấy email mới từ textbox.
        /// - Kiểm tra rỗng / định dạng đơn giản (chứa '@' và '.').
        /// - Kiểm tra có trùng email hiện tại hay không.
        /// - Hỏi confirm người dùng trước khi đổi.
        /// - Disable nút "Đổi email" và bật UseWaitCursor trong quá trình xử lý.
        /// - Kiểm tra email đã tồn tại trên hệ thống bằng <see cref="AuthService.EmailExistsAsync"/>.
        /// - Nếu hợp lệ, gọi <see cref="AuthService.UpdateEmailAsync"/> để cập nhật.
        /// - Cập nhật lại biến <see cref="_email"/> và thông báo kết quả.
        /// </summary>
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
                "Bạn chắc chắn muốn đổi email sang:\n" + emailMoi + "?",
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

        #endregion

        #region ======== TIỆN ÍCH (COPY / ĐÓNG) ========

        /// <summary>
        /// Sao chép tên đăng nhập hiện tại vào clipboard và thông báo cho người dùng.
        /// </summary>
        public void OnCopyUsername()
        {
            if (!string.IsNullOrWhiteSpace(_taiKhoan))
            {
                Clipboard.SetText(_taiKhoan);
                MessageBox.Show("Đã sao chép tên đăng nhập vào clipboard.", "Thông báo",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        /// <summary>
        /// Sao chép email hiện tại (trong textbox) vào clipboard và thông báo cho người dùng.
        /// </summary>
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

        /// <summary>
        /// Đóng form cài đặt nếu view là <see cref="Form"/>.
        /// </summary>
        public void OnDong()
        {
            if (_view is Form f)
                f.Close();
        }

        #endregion
    }
}
