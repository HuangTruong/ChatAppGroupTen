using FireSharp.Config;
using FireSharp.Interfaces;
using FireSharp.Response;
using System;
using System.Windows.Forms;
using System.Threading;
using System.Threading.Tasks;

namespace ChatApp
{
    public partial class DangNhap : Form
    {
        // Kết nối Firebase
        private IFirebaseClient firebaseClient;

        // chặn spam click (double/triple click)
        private bool _isLoggingIn = false;
        private readonly SemaphoreSlim _loginGate = new SemaphoreSlim(1, 1);

        public DangNhap()
        {
            InitializeComponent();

            // Enter trên form sẽ kích hoạt btnDangNhap
            this.KeyPreview = true;                // form bắt phím trước
            this.AcceptButton = btnDangNhap;       // ENTER = btnDangNhap
            this.KeyDown += DangNhap_KeyDown;      // dự phòng

            // gắn Enter trực tiếp cho các textbox (kể cả control bên thứ 3)
            txtTaiKhoan.KeyDown += TextBox_EnterToLogin;
            txtMatKhau.KeyDown += TextBox_EnterToLogin;

            // Cấu hình Firebase
            IFirebaseConfig MinhHoangDaVietCaiNay = new FirebaseConfig
            {
                AuthSecret = "j0kBCfIQBOBtgq5j0RaocJLgCuJO1AMn2GS5qXqH",
                BasePath = "https://chatapp-ca701-default-rtdb.asia-southeast1.firebasedatabase.app/"
            };
            firebaseClient = new FireSharp.FirebaseClient(MinhHoangDaVietCaiNay);

            if (firebaseClient == null)
            {
                MessageBox.Show("Không kết nối được Firebase.");
            }
        }

        private void DangNhap_Load(object sender, EventArgs e)
        {
            txtTaiKhoan.Focus();
            // đảm bảo AcceptButton đã được set (nếu Designer ghi đè)
            this.AcceptButton = btnDangNhap;
        }

        private void DangNhap_FormClosed(object sender, FormClosedEventArgs e)
        {
            Application.Exit();
        }

        private void btnDangKy_Click(object sender, EventArgs e)
        {
            var DangKyForm = new DangKy();
            DangKyForm.Tag = this;
            DangKyForm.Show();
            this.Hide();
        }

        private void lnkQuenMatKhau_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            var QuenMKForm = new QuenMatKhau();
            QuenMKForm.Tag = this;
            QuenMKForm.Show();
            this.Hide();
        }

        // ENTER trên form (dự phòng nếu control nào đó chặn AcceptButton)
        private void DangNhap_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter && btnDangNhap.Enabled && !_isLoggingIn)
            {
                e.SuppressKeyPress = true;
                btnDangNhap.PerformClick();
            }
        }

        // ENTER trong các textbox
        private void TextBox_EnterToLogin(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter && btnDangNhap.Enabled && !_isLoggingIn)
            {
                e.SuppressKeyPress = true;
                btnDangNhap.PerformClick();
            }
        }

        // Xử lý đăng nhập
        private async void btnDangNhap_Click(object sender, EventArgs e)
        {
            if (_isLoggingIn) return;
            _isLoggingIn = true;

            // vô hiệu hóa AcceptButton tạm thời để tránh Enter lặp khi đang đợi
            var oldAccept = this.AcceptButton;
            this.AcceptButton = null;

            bool oldEnabled = btnDangNhap.Enabled;
            btnDangNhap.Enabled = false;
            this.UseWaitCursor = true;

            try
            {
                await _loginGate.WaitAsync();

                string taiKhoan = txtTaiKhoan.Text;
                string matKhau = txtMatKhau.Text;

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

                FirebaseResponse userResponse = await firebaseClient.GetAsync($"users/{taiKhoan}");

                if (userResponse.Body == "null")
                {
                    MessageBox.Show("Tài khoản không tồn tại!", "Lỗi",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                var user = userResponse.ResultAs<UserDto>();

                if (user == null || user.MatKhau != matKhau)
                {
                    MessageBox.Show("Mật khẩu không đúng!", "Lỗi",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                MessageBox.Show("Đăng nhập thành công!", "Thông báo",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);

                txtTaiKhoan.Clear();
                txtMatKhau.Clear();

                this.Hide();
                var home = new TrangChu(user.Ten);
                home.FormClosed += (s, e2) => this.Close();
                home.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi đăng nhập: " + ex.Message, "Lỗi",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                if (_loginGate.CurrentCount == 0) _loginGate.Release();

                _isLoggingIn = false;
                btnDangNhap.Enabled = oldEnabled;
                this.AcceptButton = oldAccept ?? btnDangNhap;   // bật lại Enter
                this.UseWaitCursor = false;
            }
        }

        bool isMatKhau = true;  // Ban đầu đang ẩn
        private void txtMatKhau_IconRightClick(object sender, EventArgs e)
        {
            if (isMatKhau)
            {
                txtMatKhau.PasswordChar = '\0';
                txtMatKhau.IconRight = Properties.Resources.HienMatKhau;
                isMatKhau = false;
            }
            else
            {
                txtMatKhau.PasswordChar = '●';
                txtMatKhau.IconRight = Properties.Resources.AnMatKhau;
                isMatKhau = true;
            }
        }
    }

    // Model người dùng
    public class UserDto
    {
        public string TaiKhoan { get; set; }
        public string MatKhau { get; set; }
        public string Email { get; set; }
        public string Ten { get; set; }
        public string Ngaysinh { get; set; }
        public string Gioitinh { get; set; }
    }
}

