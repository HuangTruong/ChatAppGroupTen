using FireSharp.Config;
using FireSharp.Interfaces;
using FireSharp.Response;
using System;
using System.Drawing;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using ChatApp.Services.Auth;
using ChatApp.Models.Users;

namespace ChatApp
{
    public partial class DangKy : Form
    {
        // Dịch vụ xử lý xác thực người dùng (đăng ký, kiểm tra trùng, lưu Firebase,...)
        private readonly AuthService _authService = new AuthService();

        public DangKy()
        {
            InitializeComponent();

            // Cho phép nhấn ENTER để kích hoạt nút “Đăng ký”
            this.KeyPreview = true;        // Form bắt sự kiện phím trước các control con
            this.AcceptButton = btnDangKy; // ENTER = click btnDangKy
            this.KeyDown += DangKy_KeyDown;

            // Gán sự kiện ENTER cho từng ô nhập để tiện người dùng thao tác
            txtTaiKhoan.KeyDown += TextBox_EnterToSubmit;
            txtMatKhau.KeyDown += TextBox_EnterToSubmit;
            txtXacNhanMatKhau.KeyDown += TextBox_EnterToSubmit;
            txtEmail.KeyDown += TextBox_EnterToSubmit;
            txtTen.KeyDown += TextBox_EnterToSubmit;
            cbbGioiTinh.KeyDown += TextBox_EnterToSubmit;
            dtpNgaySinh.KeyDown += TextBox_EnterToSubmit;
        }

        private void DangKy_Load(object sender, EventArgs e)
        {
            // Gán sự kiện ẩn/hiện mật khẩu cho ô Xác nhận mật khẩu (dùng chung biểu tượng mắt)
            txtXacNhanMatKhau.IconRightClick += txtMatKhau_IconRightClick;

            // Đảm bảo nút ENTER luôn gắn với nút Đăng ký
            this.AcceptButton = btnDangKy;
        }

        // Sự kiện nhấn ENTER toàn form (nếu không focus ở control cụ thể)
        private void DangKy_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter && btnDangKy.Enabled)
            {
                e.SuppressKeyPress = true;  // Ngăn tiếng “bíp” của phím Enter
                btnDangKy.PerformClick();   // Kích hoạt hành động đăng ký
            }
        }

        // ENTER trong các TextBox/ComboBox cũng sẽ kích hoạt Đăng ký
        private void TextBox_EnterToSubmit(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter && btnDangKy.Enabled)
            {
                e.SuppressKeyPress = true;
                btnDangKy.PerformClick();
            }
        }

        // Nút “Quay lại đăng nhập”
        private void btnQuayLaiDangNhap_Click(object sender, EventArgs e)
        {
            // Nếu form đăng nhập đang ẩn thì mở lại
            Form DangNhapForm = this.Tag as Form;
            if (DangNhapForm != null && !DangNhapForm.IsDisposed)
            {
                DangNhapForm.Show();
                DangNhapForm.Activate();
                this.Close();
            }
            else
            {
                // Nếu không có form cũ -> tạo form mới
                var newLogin = new ChatApp.DangNhap();
                newLogin.Show();
                this.Close();
            }
        }

        // Nút “Đăng ký” được nhấn
        private async void btnDangKy_Click(object sender, EventArgs e)
        {
            // Nếu nút đang bị disable (đang xử lý) thì thoát —> chống spam click
            if (!btnDangKy.Enabled) return;

            // Vô hiệu nút trong lúc xử lý để tránh người dùng bấm liên tục
            btnDangKy.Enabled = false;
            this.UseWaitCursor = true; // Hiển thị con trỏ chờ (đồng hồ cát)

            try
            {
                // Lấy dữ liệu người dùng nhập vào
                string taiKhoan = txtTaiKhoan.Text.Trim();
                string matKhau = txtMatKhau.Text.Trim();
                string xacNhanMatKhau = txtXacNhanMatKhau.Text.Trim();
                string email = txtEmail.Text.Trim();
                string ten = txtTen.Text.Trim();
                string ngaySinh = dtpNgaySinh.Text;
                string gioiTinh = cbbGioiTinh.Text;

                // ✅ Kiểm tra nhập đủ thông tin
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

                // ✅ Kiểm tra xác nhận mật khẩu
                if (matKhau != xacNhanMatKhau)
                {
                    MessageBox.Show("Mật khẩu và xác nhận mật khẩu không khớp!",
                        "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // ✅ Kiểm tra tài khoản trùng (theo username Firebase)
                if (await _authService.GetUserAsync(taiKhoan) != null)
                {
                    MessageBox.Show("Tên tài khoản đã tồn tại!", "Lỗi",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // ✅ Kiểm tra email trùng
                if (await _authService.EmailExistsAsync(email))
                {
                    MessageBox.Show("Email đã tồn tại!", "Lỗi",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // ✅ Kiểm tra tên hiển thị trùng
                if (await _authService.UsernameExistsAsync(ten))
                {
                    MessageBox.Show("Tên hiển thị đã tồn tại!", "Lỗi",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // ✅ Tạo đối tượng người dùng mới
                var newUser = new UserDK
                {
                    TaiKhoan = taiKhoan,
                    MatKhau = matKhau,
                    Email = email,
                    Ten = ten,
                    Ngaysinh = ngaySinh,
                    Gioitinh = gioiTinh
                };

                // ✅ Gửi yêu cầu đăng ký lên Firebase
                await _authService.RegisterAsync(newUser);

                // ✅ Thông báo thành công
                MessageBox.Show("Đăng ký thành công!", "Thông báo",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);

                // ✅ Reset form sau khi đăng ký
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
                // Nếu có lỗi bất ngờ (Firebase, kết nối, logic, v.v.)
                MessageBox.Show("Đã xảy ra lỗi: " + ex.Message, "Lỗi",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                // Dù thành công hay thất bại vẫn bật lại nút và tắt con trỏ chờ
                btnDangKy.Enabled = true;
                this.UseWaitCursor = false;
            }
        }

        // ==========================
        // ẨN / HIỆN MẬT KHẨU
        // ==========================

        bool isMatKhau = true;  // Biến đánh dấu trạng thái đang ẩn hay đang hiện

        private void txtMatKhau_IconRightClick(object sender, EventArgs e)
        {
            if (isMatKhau)
            {
                // Hiện mật khẩu
                txtMatKhau.PasswordChar = '\0'; // '\0' = không che ký tự
                txtMatKhau.IconRight = Properties.Resources.HienMatKhau;

                // Đồng bộ với ô Xác nhận mật khẩu
                txtXacNhanMatKhau.PasswordChar = '\0';
                txtXacNhanMatKhau.IconRight = Properties.Resources.HienMatKhau;

                isMatKhau = false;
            }
            else
            {
                // Ẩn mật khẩu
                txtMatKhau.PasswordChar = '●';
                txtMatKhau.IconRight = Properties.Resources.AnMatKhau;

                txtXacNhanMatKhau.PasswordChar = '●';
                txtXacNhanMatKhau.IconRight = Properties.Resources.AnMatKhau;

                isMatKhau = true;
            }
        }
    }
}
