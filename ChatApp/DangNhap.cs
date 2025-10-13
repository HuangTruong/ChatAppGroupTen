using FireSharp.Config;
using FireSharp.Interfaces;
using FireSharp.Response;
using System;
using System.Windows.Forms;

namespace ChatApp
{
    public partial class DangNhap : Form
    {
        // Biến để kết nối tới Firebase
        private IFirebaseClient firebaseClient;

        public DangNhap()
        {
            InitializeComponent();

            // Cấu hình Firebase (gồm AuthSecret và BasePath)
            IFirebaseConfig MinhHoangDaVietCaiNay = new FirebaseConfig
            {
                // AuthSecret là khóa bí mật để xác thực kết nối tới database (Copy trong dự án firebase)
                AuthSecret = "RBVYwGVpeA360cuFw7YcoiPKAf07ZpFHrZma2mx0",

                // BasePath là đường dẫn đến Realtime Database trên Firebase
                BasePath = "https://fir-client-1d344-default-rtdb.firebaseio.com/"
            };

            // Tạo đối tượng FirebaseClient để làm việc với database
            firebaseClient = new FireSharp.FirebaseClient(MinhHoangDaVietCaiNay);

            // Kiểm tra nếu không kết nối được thì báo lỗi
            if (firebaseClient == null)
            {
                MessageBox.Show("Không kết nối được Firebase.");
            }
        }

        private void DangNhap_Load(object sender, EventArgs e)
        {
            
        }

        // Khi bấm nút “Đăng ký” thì mở form Đăng Ký
        private void btnDangKy_Click(object sender, EventArgs e)
        {
            var DangKyForm = new DangKy();
            DangKyForm.Tag = this;   // Dùng Tag để có thể quay lại form này nếu cần
            DangKyForm.Show();
            this.Hide();              // Ẩn form đăng nhập
        }

        // Khi click vào link “Quên mật khẩu” thì mở form Quên Mật Khẩu
        private void lnkQuenMatKhau_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            var QuenMKForm = new QuenMatKhau();
            QuenMKForm.Tag = this;
            QuenMKForm.Show();
            this.Hide();
        }

        // Xử lý đăng nhập khi bấm nút “Đăng nhập”
        private async void btnDangNhap_Click(object sender, EventArgs e)
        {
            string taiKhoan = txtTaiKhoan.Text;
            string matKhau = txtMatKhau.Text;

            // Kiểm tra người dùng có để trống tài khoản không
            if (string.IsNullOrWhiteSpace(taiKhoan))
            {
                MessageBox.Show("Vui lòng nhập tên đăng nhập!", "Thông báo",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Kiểm tra mật khẩu có bị bỏ trống không
            if (string.IsNullOrWhiteSpace(matKhau))
            {
                MessageBox.Show("Vui lòng nhập mật khẩu!", "Thông báo",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                // Gửi yêu cầu GET đến Firebase để lấy thông tin người dùng theo tài khoản
                FirebaseResponse userResponse = await firebaseClient.GetAsync($"users/{taiKhoan}");

                // Nếu trả về “null” thì tức là tài khoản không tồn tại
                if (userResponse.Body == "null")
                {
                    MessageBox.Show("Tài khoản không tồn tại!", "Lỗi",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Chuyển dữ liệu từ Firebase (JSON) thành đối tượng UserDto
                var user = userResponse.ResultAs<UserDto>();

                // So sánh mật khẩu nhập vào với mật khẩu trong database
                if (user == null || user.MatKhau != matKhau)
                {
                    MessageBox.Show("Mật khẩu không đúng!", "Lỗi",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Nếu tài khoản và mật khẩu đều đúng thì thông báo đăng nhập thành công
                MessageBox.Show("Đăng nhập thành công!", "Thông báo",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);

                // Xóa dữ liệu cũ trong ô nhập
                txtTaiKhoan.Clear();
                txtMatKhau.Clear();

                // Ẩn form đăng nhập và mở form Trang Chủ
                this.Hide();
                new TrangChu().Show();
            }
            catch (Exception ex)
            {
                // Nếu có lỗi (ví dụ: lỗi mạng, lỗi kết nối Firebase)
                MessageBox.Show("Lỗi đăng nhập: " + ex.Message, "Lỗi",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }

    // Class này đại diện cho thông tin người dùng lưu trên Firebase
    public class UserDto
    {
        public string TaiKhoan { get; set; }  // Tên đăng nhập
        public string MatKhau { get; set; }   // Mật khẩu người dùng
        public string Email { get; set; }     // Email người dùng
        public string Ten { get; set; }       // Họ tên đầy đủ
        public string Ngaysinh { get; set; }  // Ngày sinh
        public string Gioitinh { get; set; }  // Giới tính
    }
}
/*
 * từ khóa async là để cho phép chương trình chạy bất đồng bộ, cho phép vừa truy cập firebase mà không làm form bị đứng yên
 * khi gọi firebase response thì phải kèm thêm await để chương trình có thể hoạt động khi yêu cầu dữ liệu (hoặc update dữ liệu) từ firebase
 */