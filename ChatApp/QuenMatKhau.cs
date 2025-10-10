using FireSharp;
using FireSharp.Config;
using FireSharp.Interfaces;
using FireSharp.Response;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ChatApp
{
    public partial class QuenMatKhau : Form
    {
        private IFirebaseClient firebaseClient;
        public QuenMatKhau()
        {
            InitializeComponent();
            IFirebaseConfig config = new FirebaseConfig
            {
                AuthSecret = "PFejsR6CHWL2zIGqFqZ1w3Orw0ljzeHnHubtuQN8",
                BasePath = "https://fir-client-1d344-default-rtdb.firebaseio.com/"
            };
            // Khởi tạo FirebaseClient
            firebaseClient = new FireSharp.FirebaseClient(config);
        }

        private void QuenMatKhau_Load(object sender, EventArgs e)
        {

        }

        private async void btnGuiMaXacNhan_Click(object sender, EventArgs e)
        {
            string email = txtEmail.Text.Trim();
            string taikhoan = txtTaiKhoan.Text.Trim();

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(taikhoan))
            {
                MessageBox.Show("Vui lòng điền đầy đủ thông tin!", "Thông báo",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                // Dùng cùng BasePath/AuthSecret với form đăng ký
                if (firebaseClient == null)
                {
                    MessageBox.Show("Không kết nối được Firebase.", "Lỗi",
                                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                var taiKhoanKey = SafeKey(taikhoan);

                FirebaseResponse userResponse = await firebaseClient.GetAsync($"users/{taiKhoanKey}");
                if (userResponse.Body == "null")
                {
                    MessageBox.Show("Tài khoản không tồn tại!", "Lỗi",
                                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                var user = userResponse.ResultAs<UserQMK>();   // class phải khớp field trong DB
                                                            // Nếu lúc đăng ký bạn lưu email thô:
                if (!string.Equals(user.Email?.Trim(), email, StringComparison.OrdinalIgnoreCase))
                {
                    MessageBox.Show("Email không đúng vui lòng nhập lại!", "Lỗi",
                                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // GỬI TẠM MẬT KHẨU 
                string password = user.MatKhau;
                GuiEmailMatKhau(email, password);

                MessageBox.Show("Mật khẩu đã được gửi đến email của bạn!", "Thông báo",
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Đã xảy ra lỗi: " + ex.Message, "Lỗi",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private static string SafeKey(string s)
        {
            return System.Text.RegularExpressions.Regex.Replace(s, @"[.#$\[\]/]", "_");
        }
        private void GuiEmailMatKhau(string email, string password)
        {
            try
            {
                string fromAddress = "hnhom17@gmail.com"; // Email của bạn
                string toAddress = email; // Email của người dùng
                string subject = "Password Recovery"; // Tiêu đề email
                string body = $"Mật khẩu của bạn: {password}"; // Nội dung email

                using (MailMessage mail = new MailMessage(fromAddress, toAddress, subject, body))
                {
                    using (SmtpClient smtp = new SmtpClient("smtp.gmail.com", 587))
                    {
                        smtp.EnableSsl = true;
                        smtp.UseDefaultCredentials = false;
                        smtp.Credentials = new NetworkCredential(fromAddress, "gcgq xzja ivub klbo"); // mật khẩu
                        smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                        smtp.Timeout = 15000; // 15s
                        smtp.Send(mail);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Đã xảy ra lỗi khi gửi email: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
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
    }
    public class UserQMK
    {
        public string TaiKhoan { get; set; }
        public string MatKhau { get; set; }
        public string Email { get; set; }
        public string Ten { get; set; }
        public string Ngaysinh { get; set; }
        public string Gioitinh { get; set; }
    }
}
