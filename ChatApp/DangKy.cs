using System;
using System.Text;
using System.Windows.Forms;
using FireSharp.Config;
using FireSharp.Interfaces;
using FireSharp.Response;

// [ADDED] chống re-entry & async hỗ trợ
using System.Threading;
using System.Threading.Tasks;

namespace ChatApp
{
    public partial class DangKy : Form
    {
        // Biến dùng để kết nối Firebase
        private IFirebaseClient firebaseClient;

        // [ADDED] — chặn spam click (double/triple click)
        private bool _isRegistering = false;
        private readonly SemaphoreSlim _registerGate = new SemaphoreSlim(1, 1);

        public DangKy()
        {
            InitializeComponent();

            // Cấu hình Firebase (gồm khóa bảo mật và đường dẫn database)
            IFirebaseConfig MinhHoangDaCodeCaiNay = new FirebaseConfig
            {
                AuthSecret = "RBVYwGVpeA360cuFw7YcoiPKAf07ZpFHrZma2mx0",
                BasePath = "https://fir-client-1d344-default-rtdb.firebaseio.com/"
            };

            // Tạo đối tượng FirebaseClient để giao tiếp với Firebase
            firebaseClient = new FireSharp.FirebaseClient(MinhHoangDaCodeCaiNay);

            // Nếu kết nối thất bại thì báo lỗi
            if (firebaseClient == null)
                MessageBox.Show("Không kết nối được Firebase.");
        }

        private void DangKy_Load(object sender, EventArgs e)
        {

        }

        // Nút “Quay lại đăng nhập”
        private void btnQuayLaiDangNhap_Click(object sender, EventArgs e)
        {
            // Nếu form đăng nhập trước đó còn tồn tại thì quay lại form đó
            Form DangNhapForm = this.Tag as Form;
            if (DangNhapForm != null && !DangNhapForm.IsDisposed)
            {
                DangNhapForm.Show();
                DangNhapForm.Activate(); // [ADDED] tránh mở trùng
                this.Close();
            }
            else
            {
                // Nếu form đăng nhập trước đó đã bị đóng → mở form mới
                var newLogin = new ChatApp.DangNhap();
                newLogin.Show();
                this.Close();
            }
        }

        // Xử lý khi nhấn nút “Đăng ký”
        private async void btnDangKy_Click(object sender, EventArgs e)
        {
            // [ADDED] — nếu đang có phiên đăng ký chạy, bỏ qua click mới
            if (_isRegistering) return;

            // [ADDED] — set cờ ngay lập tức để chặn double-click trước khi await
            _isRegistering = true;

            // [ADDED] — vô hiệu hóa nút & Enter, hiển thị wait cursor
            var oldAccept = this.AcceptButton;
            this.AcceptButton = null;               // tắt Enter trong lúc xử lý
            bool oldEnabled = btnDangKy.Enabled;
            btnDangKy.Enabled = false;
            this.UseWaitCursor = true;

            try
            {
                // [ADDED] — đảm bảo tuyệt đối chỉ 1 luồng đăng ký chạy
                await _registerGate.WaitAsync();

                // Lấy dữ liệu người dùng nhập vào
                string taiKhoan = txtTaiKhoan.Text;
                string matKhau = txtMatKhau.Text;
                string xacNhanMatKhau = txtXacNhanMatKhau.Text;
                string email = txtEmail.Text;
                string encodedEmail = Convert.ToBase64String(Encoding.UTF8.GetBytes(email)); // Mã hóa email để lưu an toàn hơn
                string ten = txtTen.Text;
                string ngaySinh = dtpNgaySinh.Text;
                string gioiTinh = cbbGioiTinh.Text;

                // Kiểm tra xem người dùng có bỏ trống thông tin nào không
                if (string.IsNullOrWhiteSpace(taiKhoan) ||
                    string.IsNullOrWhiteSpace(matKhau) ||
                    string.IsNullOrWhiteSpace(xacNhanMatKhau) ||
                    string.IsNullOrWhiteSpace(email) ||
                    string.IsNullOrWhiteSpace(ten) ||
                    string.IsNullOrWhiteSpace(ngaySinh) ||
                    string.IsNullOrWhiteSpace(gioiTinh))
                {
                    MessageBox.Show("Vui lòng điền đầy đủ thông tin!", "Thông báo",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Kiểm tra xem mật khẩu nhập lại có khớp không
                if (matKhau != xacNhanMatKhau)
                {
                    MessageBox.Show("Mật khẩu và xác nhận mật khẩu không khớp!",
                        "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // (tuỳ chọn) “hút” double-click siêu nhanh
                // await Task.Delay(150);

                try
                {
                    // Kiểm tra xem tài khoản đã tồn tại trong database chưa
                    var userExistsResponse = await firebaseClient.GetAsync($"users/{taiKhoan}");
                    if (userExistsResponse.Body != "null")
                    {
                        MessageBox.Show("Tên tài khoản đã tồn tại!", "Lỗi",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    // Kiểm tra email (đã được mã hóa Base64)
                    var emailExistsResponse = await firebaseClient.GetAsync($"emails/{encodedEmail}");
                    if (emailExistsResponse.Body != "null")
                    {
                        MessageBox.Show("Email đã tồn tại!", "Lỗi",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    // Kiểm tra xem tên hiển thị (nickname) đã có ai dùng chưa
                    var usernameExistsResponse = await firebaseClient.GetAsync($"Username/{ten}");
                    if (usernameExistsResponse.Body != "null")
                    {
                        MessageBox.Show("Tên hiển thị đã tồn tại!", "Lỗi",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    // Nếu mọi thứ hợp lệ thì tạo đối tượng người dùng mới
                    var newUser = new UserDK
                    {
                        TaiKhoan = taiKhoan,
                        MatKhau = matKhau,
                        Email = email,
                        Ten = ten,
                        Ngaysinh = ngaySinh,
                        Gioitinh = gioiTinh
                    };

                    // Gửi dữ liệu người dùng lên Firebase (tạo mới)
                    await firebaseClient.SetAsync($"users/{taiKhoan}", newUser);

                    // Lưu email đã dùng vào danh sách emails (để tránh trùng)
                    await firebaseClient.SetAsync($"emails/{encodedEmail}", true);

                    MessageBox.Show("Đăng ký thành công!", "Thông báo",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);

                    // Xóa toàn bộ thông tin vừa nhập (reset form)
                    txtTen.Clear();
                    txtTaiKhoan.Clear();
                    txtMatKhau.Clear();
                    txtXacNhanMatKhau.Clear();
                    txtEmail.Clear();
                    dtpNgaySinh.Value = DateTime.Today;
                    cbbGioiTinh.SelectedIndex = -1;
                }
                catch (Exception ex)
                {
                    // Nếu có lỗi trong quá trình xử lý (ví dụ lỗi mạng, Firebase lỗi)
                    MessageBox.Show("Đã xảy ra lỗi: " + ex.Message, "Lỗi",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            finally
            {
                // [ADDED] — nhả khóa và khôi phục UI
                if (_registerGate.CurrentCount == 0)
                    _registerGate.Release();

                _isRegistering = false;
                btnDangKy.Enabled = oldEnabled;
                this.AcceptButton = oldAccept;
                this.UseWaitCursor = false;
            }
        }

        private void pnlBackground_Paint(object sender, PaintEventArgs e)
        {

        }
    }

    // Class mô tả cấu trúc dữ liệu người dùng lưu trong Firebase
    public class UserDK
    {
        public string TaiKhoan { get; set; }  // Tên đăng nhập
        public string MatKhau { get; set; }   // Mật khẩu
        public string Email { get; set; }     // Email
        public string Ten { get; set; }       // Họ tên đầy đủ
        public string Ngaysinh { get; set; }  // Ngày sinh
        public string Gioitinh { get; set; }  // Giới tính
    }
}
