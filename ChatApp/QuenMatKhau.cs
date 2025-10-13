using FireSharp;
using FireSharp.Config;
using FireSharp.Interfaces;
using FireSharp.Response;
using System;
using System.Net;
using System.Net.Mail;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace ChatApp
{
    public partial class QuenMatKhau : Form
    {
        private IFirebaseClient ketNoiFirebase;

        // Các biến phục vụ cho quá trình xác nhận OTP
        private string maXacNhanHienTai;     // Mã OTP hiện tại
        private DateTime hanSuDungMa;        // Thời hạn của mã OTP
        private string taiKhoanDangXacNhan;  // Tài khoản đang khôi phục mật khẩu

        public QuenMatKhau()
        {
            InitializeComponent();

            // Cấu hình Firebase
            IFirebaseConfig cauHinh = new FirebaseConfig
            {
                AuthSecret = "RBVYwGVpeA360cuFw7YcoiPKAf07ZpFHrZma2mx0",
                BasePath = "https://fir-client-1d344-default-rtdb.firebaseio.com/"
            };

            ketNoiFirebase = new FireSharp.FirebaseClient(cauHinh);
        }

        

        // Gửi mã xác nhận qua email
        private async void btnGuiMaXacNhan_Click(object sender, EventArgs e)
        {
            string emailNhap = txtEmail.Text.Trim();
            string taiKhoanNhap = txtTaiKhoan.Text.Trim();

            if (string.IsNullOrWhiteSpace(emailNhap) || string.IsNullOrWhiteSpace(taiKhoanNhap))
            {
                MessageBox.Show("Vui lòng nhập đầy đủ thông tin!", "Thông báo",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                if (ketNoiFirebase == null)
                {
                    MessageBox.Show("Không thể kết nối Firebase!", "Lỗi",
                                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                string taiKhoanKey = ChuyenKeyHopLe(taiKhoanNhap);

                // Lấy thông tin người dùng từ Firebase
                FirebaseResponse phanHoi = await ketNoiFirebase.GetAsync($"users/{taiKhoanKey}");
                if (phanHoi.Body == "null")
                {
                    MessageBox.Show("Tài khoản không tồn tại!", "Lỗi",
                                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                var nguoiDung = phanHoi.ResultAs<NguoiDungFirebase>();
                if (!string.Equals(nguoiDung.Email?.Trim(), emailNhap, StringComparison.OrdinalIgnoreCase))
                {
                    MessageBox.Show("Email không trùng với tài khoản!", "Lỗi",
                                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Tạo mã xác nhận ngẫu nhiên và đặt hạn sử dụng
                maXacNhanHienTai = new Random().Next(100000, 999999).ToString();
                hanSuDungMa = DateTime.UtcNow.AddMinutes(5);
                taiKhoanDangXacNhan = taiKhoanNhap;

                // Lưu mã OTP lên Firebase (nếu cần)
                var thongTinMa = new { Ma = maXacNhanHienTai, HetHanLuc = hanSuDungMa.ToString("o") };
                await ketNoiFirebase.SetAsync($"otp/{taiKhoanKey}", thongTinMa);

                // Gửi mã OTP qua email
                GuiEmailXacNhan(emailNhap, maXacNhanHienTai);

                MessageBox.Show("Đã gửi mã xác nhận qua email (có hạn trong 5 phút).", "Thông báo",
                                MessageBoxButtons.OK, MessageBoxIcon.Information);

                txtMaXacNhan.Focus();
                txtMaXacNhan.SelectAll();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi gửi mã xác nhận: " + ex.Message, "Lỗi",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Xác nhận mã người dùng nhập
        private async void btnXacNhan_Click(object sender, EventArgs e)
        {
            string maNguoiDungNhap = txtMaXacNhan.Text.Trim();

            if (string.IsNullOrWhiteSpace(maNguoiDungNhap))
            {
                MessageBox.Show("Vui lòng nhập mã xác nhận!", "Thông báo",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(taiKhoanDangXacNhan))
            {
                MessageBox.Show("Bạn chưa gửi mã xác nhận!", "Thông báo",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                // Kiểm tra mã có hợp lệ và còn hạn không
                bool maHopLe = (DateTime.UtcNow <= hanSuDungMa) && (maNguoiDungNhap == maXacNhanHienTai);

                string taiKhoanKey = ChuyenKeyHopLe(taiKhoanDangXacNhan);
                var phanHoiMa = await ketNoiFirebase.GetAsync($"otp/{taiKhoanKey}");
                if (phanHoiMa.Body != "null")
                {
                    var maTuFirebase = phanHoiMa.ResultAs<ThongTinMaFirebase>();
                    if (DateTime.TryParse(maTuFirebase?.HetHanLuc, out DateTime thoiGianHetHan))
                    {
                        bool conHan = DateTime.UtcNow <= thoiGianHetHan;
                        bool dungMa = string.Equals(maTuFirebase?.Ma, maNguoiDungNhap, StringComparison.Ordinal);
                        maHopLe = maHopLe || (conHan && dungMa);
                    }
                }

                if (!maHopLe)
                {
                    MessageBox.Show("Mã xác nhận không đúng hoặc đã hết hạn!", "Lỗi",
                                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                MessageBox.Show("Xác nhân thành công, Vui lòng đổi mật khẩu mới!", "Thông báo",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);

                // Mở form đổi mật khẩu
                var formDoiMatKhau = new DoiMatKhau(taiKhoanDangXacNhan);
                formDoiMatKhau.Tag = this;
                formDoiMatKhau.Show();
                this.Hide();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi xác nhận mã: " + ex.Message, "Lỗi",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Chuyển ký tự đặc biệt trong tài khoản thành dạng hợp lệ với Firebase
        private static string ChuyenKeyHopLe(string text)
        {
            return Regex.Replace(text, @"[.#$\[\]/]", "_");
        }

        // Gửi email chứa mã xác nhận
        private void GuiEmailXacNhan(string emailNhan, string maGui)
        {
            try
            {
                string emailGui = "hnhom17@gmail.com";
                string tieuDe = "Mã xác nhận đổi mật khẩu";
                string noiDung = $"Xin chào,\n\nMã xác nhận của bạn là: {maGui}\n" +
                                 $"Mã có hiệu lực trong 5 phút.\n\nTrân trọng!";

                using (MailMessage thu = new MailMessage(emailGui, emailNhan, tieuDe, noiDung))
                using (SmtpClient smtp = new SmtpClient("smtp.gmail.com", 587))
                {
                    smtp.EnableSsl = true;
                    smtp.UseDefaultCredentials = false;
                    smtp.Credentials = new NetworkCredential(emailGui, "gcgq xzja ivub klbo");
                    smtp.Send(thu);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Không thể gửi email: " + ex.Message, "Lỗi",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Quay lại form đăng nhập
        private void btnQuayLaiDangNhap_Click(object sender, EventArgs e)
        {
            Form formDangNhap = this.Tag as Form;
            if (formDangNhap != null && !formDangNhap.IsDisposed)
            {
                formDangNhap.Show();
                this.Close();
            }
            else
            {
                var formMoi = new ChatApp.DangNhap();
                formMoi.Show();
                this.Close();
            }
        }
    }

    // Cấu trúc dữ liệu mã OTP trong Firebase
    public class ThongTinMaFirebase
    {
        public string Ma { get; set; }
        public string HetHanLuc { get; set; }
    }

    // Cấu trúc dữ liệu người dùng trong Firebase
    public class NguoiDungFirebase
    {
        public string TaiKhoan { get; set; }
        public string MatKhau { get; set; }
        public string Email { get; set; }
        public string Ten { get; set; }
        public string NgaySinh { get; set; }
        public string GioiTinh { get; set; }
    }
}
