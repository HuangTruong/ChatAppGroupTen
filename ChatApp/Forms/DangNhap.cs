using FireSharp.Config;
using FireSharp.Interfaces;
using FireSharp.Response;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

// Các thư viện tự tạo
using ChatApp.Services.Auth; 

namespace ChatApp
{
    public partial class DangNhap : Form
    {
        // Dịch vụ xử lý đăng nhập, đăng ký, truy xuất người dùng từ Firebase
        private readonly AuthService _authService = new AuthService();

        public DangNhap()
        {
            InitializeComponent();

            // Cho phép form bắt phím ENTER
            this.KeyPreview = true;          // Form nhận sự kiện phím trước control con
            this.AcceptButton = btnDangNhap; // ENTER sẽ tự động nhấn nút Đăng nhập
            this.KeyDown += DangNhap_KeyDown; // Dự phòng nếu control chặn AcceptButton

            // Gắn phím Enter cho 2 ô nhập tài khoản và mật khẩu
            txtTaiKhoan.KeyDown += TextBox_EnterToLogin;
            txtMatKhau.KeyDown += TextBox_EnterToLogin;

            // TODO: Có thể thêm phần kiểm tra Firebase Config hoặc load trước ở đây
        }

        private void DangNhap_Load(object sender, EventArgs e)
        {
            // Khi form mở, focus vào ô tài khoản
            txtTaiKhoan.Focus();

            // Đảm bảo lại AcceptButton (đôi khi Designer ghi đè)
            this.AcceptButton = btnDangNhap;
        }
        private void DangNhap_FormClosed(object sender, FormClosedEventArgs e)
        {
            // Đóng ứng dụng hoàn toàn nếu form đăng nhập bị đóng
            Application.Exit();
        }

        private void btnDangKy_Click(object sender, EventArgs e)
        {
            // Mở form Đăng ký
            var DangKyForm = new DangKy();
            DangKyForm.Tag = this;  // Gửi tham chiếu form hiện tại sang form mới
            DangKyForm.Show();
            this.Hide();            // Ẩn form đăng nhập
        }

        private void lnkQuenMatKhau_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            // Mở form Quên mật khẩu
            var QuenMKForm = new QuenMatKhau();
            QuenMKForm.Tag = this;
            QuenMKForm.Show();
            this.Hide();
        }

        // Dự phòng: nếu control nào đó ngăn AcceptButton hoạt động
        private void DangNhap_KeyDown(object sender, KeyEventArgs e)
        {
            // Nếu nhấn Enter và nút đang được phép nhấn
            if (e.KeyCode == Keys.Enter && btnDangNhap.Enabled)
            {
                e.SuppressKeyPress = true;    // Chặn "ding" âm thanh của Windows
                btnDangNhap.PerformClick();   // Giả lập nhấn nút Đăng nhập
            }
        }

        // Cho phép Enter trong các textbox thực hiện đăng nhập
        private void TextBox_EnterToLogin(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter && btnDangNhap.Enabled)
            {
                e.SuppressKeyPress = true;
                btnDangNhap.PerformClick();
            }
        }

        // Nút đăng nhập
        private async void btnDangNhap_Click(object sender, EventArgs e)
        {
            // Chặn spam click liên tiếp
            if (!btnDangNhap.Enabled) return;

            btnDangNhap.Enabled = false;  // Tạm thời vô hiệu hoá nút
            this.UseWaitCursor = true;    // Hiển thị con trỏ chờ

            try
            {
                string taiKhoan = txtTaiKhoan.Text;
                string matKhau = txtMatKhau.Text;

                // Kiểm tra rỗng
                if (string.IsNullOrWhiteSpace(taiKhoan))
                {
                    MessageBox.Show("Vui lòng nhập tên đăng nhập!", "Thông báo",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                if (string.IsNullOrWhiteSpace(matKhau))
                {
                    MessageBox.Show("Vui lòng nhập mật khẩu!", "Thông báo",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                //----------------------------------------------------------
                // Lấy thông tin người dùng từ Firebase
                var user = await _authService.GetUserAsync(taiKhoan);

                if (user == null)
                {
                    MessageBox.Show("Tài khoản không tồn tại!", "Lỗi",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // So sánh mật khẩu
                if (user.MatKhau != matKhau)
                {
                    MessageBox.Show("Mật khẩu không đúng!", "Lỗi",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Nếu hợp lệ => thông báo và chuyển sang form chính
                MessageBox.Show("Đăng nhập thành công!", "Thông báo",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);

                // Xoá nội dung ô nhập
                txtTaiKhoan.Clear();
                txtMatKhau.Clear();

                // Ẩn form đăng nhập, mở form Trang chủ
                this.Hide();
                var home = new TrangChu(user.Ten);
                home.FormClosed += (s, e2) => this.Close();  // Khi home đóng -> đóng luôn login
                home.Show();
            }
            catch (Exception ex)
            {
                // Nếu có lỗi (Firebase hoặc hệ thống)
                MessageBox.Show("Lỗi đăng nhập: " + ex.Message, "Lỗi",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                // Luôn bật lại nút và tắt con trỏ chờ
                btnDangNhap.Enabled = true;
                this.UseWaitCursor = false;
            }
        }

        // Biến dùng để lưu trạng thái ẩn/hiện mật khẩu
        bool isMatKhau = true;  // Mặc định: đang ẩn

        private void txtMatKhau_IconRightClick(object sender, EventArgs e)
        {
            // Khi nhấn vào icon bên phải ô mật khẩu
            if (isMatKhau)
            {
                txtMatKhau.PasswordChar = '\0'; // Hiện mật khẩu
                txtMatKhau.IconRight = Properties.Resources.HienMatKhau;
                isMatKhau = false;
            }
            else
            {
                txtMatKhau.PasswordChar = '●'; // Ẩn lại
                txtMatKhau.IconRight = Properties.Resources.AnMatKhau;
                isMatKhau = true;
            }
        }
    }
}
