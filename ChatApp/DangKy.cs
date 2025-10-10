using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using FireSharp.Config;
using FireSharp.Interfaces;
using FireSharp.Response;

namespace ChatApp
{
    public partial class DangKy : Form
    {
        private IFirebaseClient firebaseClient;
        public DangKy()
        {
            InitializeComponent();

            IFirebaseConfig config = new FirebaseConfig
            {
                AuthSecret = "PFejsR6CHWL2zIGqFqZ1w3Orw0ljzeHnHubtuQN8",
                BasePath = "https://fir-client-1d344-default-rtdb.firebaseio.com/"
            };

            firebaseClient = new FireSharp.FirebaseClient(config);
            if (firebaseClient == null)
                MessageBox.Show("Không kết nối được Firebase.");
        }

        private void DangKy_Load(object sender, EventArgs e)
        {

        }

        private void btnQuayLaiDangNhap_Click(object sender, EventArgs e)
        {
            Form DangNhapForm = this.Tag as Form;
            if (DangNhapForm != null && !DangNhapForm.IsDisposed)
            {
                DangNhapForm.Show();
                this.Close();
            }
            else
            {
                var newLogin = new ChatApp.DangNhap();
                newLogin.Show();
                this.Close();
            }
        }

        private async void btnDangKy_Click(object sender, EventArgs e)
        {
            string taiKhoan = txtTaiKhoan.Text;
            string matKhau = txtMatKhau.Text;
            string xacNhanMatKhau = txtXacNhanMatKhau.Text;
            string email = txtEmail.Text;
            string encodedEmail = Convert.ToBase64String(Encoding.UTF8.GetBytes(email));
            string ten = txtTen.Text;
            string ngaySinh = dtpNgaySinh.Text;
            string gioiTinh = cbbGioiTinh.Text;

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

            if (matKhau != xacNhanMatKhau)
            {
                MessageBox.Show("Mật khẩu và xác nhận mật khẩu không khớp!",
                    "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                var userExistsResponse = await firebaseClient.GetAsync($"users/{taiKhoan}");
                if (userExistsResponse.Body != "null")
                {
                    MessageBox.Show("Tên tài khoản đã tồn tại!", "Lỗi",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                var emailExistsResponse = await firebaseClient.GetAsync($"emails/{encodedEmail}");
                if (emailExistsResponse.Body != "null")
                {
                    MessageBox.Show("Email đã tồn tại!", "Lỗi",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                var usernameExistsResponse = await firebaseClient.GetAsync($"Username/{ten}");
                if (usernameExistsResponse.Body != "null")
                {
                    MessageBox.Show("Tên hiển thị đã tồn tại!", "Lỗi",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                var newUser = new UserDK
                {
                    TaiKhoan = taiKhoan,
                    MatKhau = matKhau,
                    Email = email,
                    Ten = ten,
                    Ngaysinh = ngaySinh,
                    Gioitinh = gioiTinh
                };

                await firebaseClient.SetAsync($"users/{taiKhoan}", newUser);
                await firebaseClient.SetAsync($"emails/{encodedEmail}", true);
           


                MessageBox.Show("Đăng ký thành công!", "Thông báo",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);

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
                MessageBox.Show("Đã xảy ra lỗi: " + ex.Message, "Lỗi",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
    public class UserDK
    {
        public string TaiKhoan { get; set; }
        public string MatKhau { get; set; }
        public string Email { get; set; }
        public string Ten { get; set; }
        public string Ngaysinh { get; set; }
        public string Gioitinh { get; set; }
    }
}
