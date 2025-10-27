using FireSharp.Config;
using FireSharp.Interfaces;
using FireSharp.Response;
using System;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ChatApp
{
    public partial class DoiMatKhau : Form
    {
        private readonly string _taiKhoan;     // Tài khoản cần đổi mật khẩu
        private IFirebaseClient _firebase;     // Đối tượng kết nối Firebase

        // Chặn enter lặp khi đang xử lý
        private bool _isSubmitting = false;
        private readonly SemaphoreSlim _gate = new SemaphoreSlim(1, 1);

        public DoiMatKhau(string taiKhoan)
        {
            InitializeComponent();
            _taiKhoan = taiKhoan;

            // ENTER trên form sẽ kích hoạt btnDoiMatKhau
            this.KeyPreview = true;
            this.AcceptButton = btnDoiMatKhau;
            this.KeyDown += DoiMatKhau_KeyDown;           // dự phòng
            txtMatKhau.KeyDown += TextBox_EnterToSubmit;  // enter trong textbox
            txtXacNhan.KeyDown += TextBox_EnterToSubmit;

            // Thiết lập cấu hình kết nối Firebase
            var MinhHoangDaLamCaiNay = new FirebaseConfig
            {
                AuthSecret = "j0kBCfIQBOBtgq5j0RaocJLgCuJO1AMn2GS5qXqH",
                BasePath = "https://chatapp-ca701-default-rtdb.asia-southeast1.firebasedatabase.app/"
            };

            // Tạo client Firebase
            _firebase = new FireSharp.FirebaseClient(MinhHoangDaLamCaiNay);

            // Gắn lại event
            btnDoiMatKhau.Click -= btnDoiMatKhau_Click_1;
            btnDoiMatKhau.Click += btnDoiMatKhau_Click;
        }

        // Hàm chuyển ký tự đặc biệt trong tài khoản thành key hợp lệ của Firebase
        private static string SafeKey(string s)
        {
            return Regex.Replace(s, @"[.#$\[\]/]", "_");
        }

        // ENTER toàn form (trường hợp control nào đó chặn AcceptButton)
        private void DoiMatKhau_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter && btnDoiMatKhau.Enabled && !_isSubmitting)
            {
                e.SuppressKeyPress = true;
                btnDoiMatKhau.PerformClick();
            }
        }

        // ENTER trong textbox
        private void TextBox_EnterToSubmit(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter && btnDoiMatKhau.Enabled && !_isSubmitting)
            {
                e.SuppressKeyPress = true;
                btnDoiMatKhau.PerformClick();
            }
        }

        // Xử lý khi người dùng nhấn nút "Đổi mật khẩu"
        private async void btnDoiMatKhau_Click(object sender, EventArgs e)
        {
            if (_isSubmitting) return;
            _isSubmitting = true;

            // tắt tạm AcceptButton để tránh Enter lặp khi đang await
            var oldAccept = this.AcceptButton;
            this.AcceptButton = null;

            bool oldEnabled = btnDoiMatKhau.Enabled;
            btnDoiMatKhau.Enabled = false;
            this.UseWaitCursor = true;

            try
            {
                await _gate.WaitAsync();

                string mkMoi = txtMatKhau.Text.Trim();     // Mật khẩu mới
                string mkXn = txtXacNhan.Text.Trim();      // Mật khẩu xác nhận

                // Kiểm tra các trường nhập
                if (string.IsNullOrWhiteSpace(mkMoi) || string.IsNullOrWhiteSpace(mkXn))
                {
                    MessageBox.Show("Vui lòng nhập đầy đủ mật khẩu!", "Thông báo",
                                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // So sánh mật khẩu xác nhận
                if (mkMoi != mkXn)
                {
                    MessageBox.Show("Mật khẩu xác nhận không khớp!", "Lỗi",
                                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Kiểm tra độ dài mật khẩu
                if (mkMoi.Length < 6)
                {
                    MessageBox.Show("Mật khẩu phải có ít nhất 6 ký tự.", "Thông báo",
                                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Kiểm tra kết nối Firebase
                if (_firebase == null)
                {
                    MessageBox.Show("Không thể kết nối Firebase.", "Lỗi",
                                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                string key = SafeKey(_taiKhoan);

                // Cập nhật mật khẩu mới cho người dùng
                var capNhat = new { MatKhau = mkMoi };
                await _firebase.UpdateAsync($"users/{key}", capNhat);

                // Xoá OTP đã sử dụng (nếu có lưu trong Firebase)
                await _firebase.DeleteAsync($"otp/{key}");

                MessageBox.Show("Đổi mật khẩu thành công!", "Thông báo",
                                MessageBoxButtons.OK, MessageBoxIcon.Information);

                // Quay lại form trước (nếu có)
                if (this.Tag is Form prev && !prev.IsDisposed)
                    prev.Show();

                // Đóng form hiện tại
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Có lỗi khi cập nhật mật khẩu: " + ex.Message, "Lỗi",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                if (_gate.CurrentCount == 0) _gate.Release();

                _isSubmitting = false;
                btnDoiMatKhau.Enabled = oldEnabled;
                this.AcceptButton = oldAccept ?? btnDoiMatKhau; // bật lại Enter
                this.UseWaitCursor = false;
            }
        }

        private void DoiMatKhau_Load(object sender, EventArgs e) { }

        private void btnDoiMatKhau_Click_1(object sender, EventArgs e)
        {
            // Event trống để tránh xung đột
        }
    }
}
