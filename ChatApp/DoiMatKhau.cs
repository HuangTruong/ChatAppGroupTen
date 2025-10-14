using FireSharp.Config;
using FireSharp.Interfaces;
using FireSharp.Response;
using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ChatApp
{
    public partial class DoiMatKhau : Form
    {
        private readonly string _taiKhoan;     // Tài khoản cần đổi mật khẩu
        private IFirebaseClient _firebase;     // Đối tượng kết nối Firebase

        public DoiMatKhau(string taiKhoan)
        {
            InitializeComponent();
            _taiKhoan = taiKhoan;

            // gắn lại event
            btnDoiMatKhau.Click -= btnDoiMatKhau_Click_1;
            btnDoiMatKhau.Click += btnDoiMatKhau_Click;

            try
            {
                // Thiết lập cấu hình Firebase
                var config = new FirebaseConfig
                {
                    AuthSecret = "RBVYwGVpeA360cuFw7YcoiPKAf07ZpFHrZma2mx0",
                    BasePath = "https://fir-client-1d344-default-rtdb.firebaseio.com/"
                };

                _firebase = new FireSharp.FirebaseClient(config);

                if (_firebase == null)
                {
                    MessageBox.Show("Không thể khởi tạo kết nối Firebase.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khởi tạo Firebase: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Chuyển ký tự đặc biệt sang key hợp lệ của Firebase
        private static string SafeKey(string s)
        {
            return Regex.Replace(s, @"[.#$\[\]/]", "_");
        }

        private async void btnDoiMatKhau_Click(object sender, EventArgs e)
        {
            string mkMoi = txtMatKhau.Text.Trim();
            string mkXn = txtXacNhan.Text.Trim();

            if (string.IsNullOrWhiteSpace(mkMoi) || string.IsNullOrWhiteSpace(mkXn))
            {
                MessageBox.Show("Vui lòng nhập đầy đủ mật khẩu!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (mkMoi != mkXn)
            {
                MessageBox.Show("Mật khẩu xác nhận không khớp!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (mkMoi.Length < 6)
            {
                MessageBox.Show("Mật khẩu phải có ít nhất 6 ký tự.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                if (_firebase == null)
                {
                    MessageBox.Show("Chưa kết nối được với Firebase.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                string key = SafeKey(_taiKhoan);

                // Kiểm tra tài khoản có tồn tại không
                FirebaseResponse response = await _firebase.GetAsync($"users/{key}");
                if (response.Body == "null")
                {
                    MessageBox.Show("Tài khoản không tồn tại trong hệ thống!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Cập nhật mật khẩu
                var capNhat = new { MatKhau = mkMoi };
                await _firebase.UpdateAsync($"users/{key}", capNhat);

                // Xóa OTP đã dùng
                await _firebase.DeleteAsync($"otp/{key}");

                MessageBox.Show("Đổi mật khẩu thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);

                // Quay lại form trước nếu có
                if (this.Tag is Form prev && !prev.IsDisposed)
                    prev.Show();

                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi cập nhật mật khẩu: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DoiMatKhau_Load(object sender, EventArgs e) { }

        private void btnDoiMatKhau_Click_1(object sender, EventArgs e)
        {

        }
    }
}
