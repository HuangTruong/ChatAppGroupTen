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

            // Thiết lập cấu hình kết nối Firebase
            var MinhHoangDaLamCaiNay = new FirebaseConfig
            {
                AuthSecret = "RBVYwGVpeA360cuFw7YcoiPKAf07ZpFHrZma2mx0",
                BasePath = "https://fir-client-1d344-default-rtdb.firebaseio.com/"
            };
            _firebase = new FireSharp.FirebaseClient(MinhHoangDaLamCaiNay);

            
        }

        // Hàm chuyển ký tự đặc biệt trong tài khoản thành key hợp lệ của Firebase
        private static string SafeKey(string s)
        {
            return Regex.Replace(s, @"[.#$\[\]/]", "_");
        }

        // Xử lý khi người dùng nhấn nút "Đổi mật khẩu"
        private async void btnDoiMatKhau_Click(object sender, EventArgs e)
        {
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

            try
            {
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
        }
        private void DoiMatKhau_Load(object sender, EventArgs e) { }
    }
}
