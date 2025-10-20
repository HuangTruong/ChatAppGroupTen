using FireSharp.Config;
using FireSharp.Interfaces;
using FireSharp.Response;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ChatApp
{
    public partial class NhanTin : Form
    {
        private IFirebaseClient firebase;

        // Người dùng hiện tại (tên hiển thị)
        private string tenHienTai;

        // Người đang được chọn để chat (tên hiển thị)
        private string tenDoiPhuong = "";

        public NhanTin(string tenDangNhap)
        {
            InitializeComponent();

            tenHienTai = tenDangNhap; // tenDangNhap ở đây là "Ten" trong Firebase (VD: tph)

            // Cấu hình Firebase
            IFirebaseConfig config = new FirebaseConfig
            {
                AuthSecret = "j0kBCfIQBOBtgq5j0RaocJLgCuJO1AMn2GS5qXqH",
                BasePath = "https://chatapp-ca701-default-rtdb.asia-southeast1.firebasedatabase.app/"
            };

            firebase = new FireSharp.FirebaseClient(config);

            if (firebase == null)
            {
                MessageBox.Show("Không kết nối được Firebase!");
            }
        }

        private async void NhanTin_Load(object sender, EventArgs e)
        {
            lblTenDangNhapPhai.Text = tenHienTai; // góc phải: tên mình
            await TaiDanhSachNguoiDung(); // danh sách user
        }

        // ==================== TẢI DANH SÁCH NGƯỜI DÙNG ====================
        private async Task TaiDanhSachNguoiDung()
        {
            try
            {
                var res = await firebase.GetAsync("users");
                var data = res.ResultAs<Dictionary<string, UserFirebase>>();

                flpDanhSachChat.Controls.Clear();

                if (data == null)
                {
                    Label lbl = new Label() { Text = "Không có người dùng nào!", AutoSize = true };
                    flpDanhSachChat.Controls.Add(lbl);
                    return;
                }

                foreach (var item in data)
                {
                    var user = item.Value;
                    if (user == null) continue;

                    // Bỏ qua chính mình (so sánh theo "Ten")
                    if (string.Equals(user.Ten, tenHienTai, StringComparison.OrdinalIgnoreCase))
                        continue;

                    // Tạo nút hiển thị người dùng
                    var btn = new Button();
                    btn.Text = !string.IsNullOrEmpty(user.Ten) ? user.Ten : user.TaiKhoan ?? item.Key;
                    btn.Tag = user.Ten; // DÙNG TEN làm định danh hội thoại
                    btn.Width = flpDanhSachChat.Width - 25;
                    btn.Height = 40;
                    btn.TextAlign = ContentAlignment.MiddleLeft;
                    btn.BackColor = Color.WhiteSmoke;
                    btn.FlatStyle = FlatStyle.Flat;

                    btn.Click += async (s, e) =>
                    {
                        tenDoiPhuong = (string)((Button)s).Tag;
                        lblTenDangNhapGiua.Text = tenDoiPhuong;
                        lblTenDangNhapPhai.Text = tenDoiPhuong;
                        await TaiLichSuTinNhan();
                    };

                    flpDanhSachChat.Controls.Add(btn);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải danh sách người dùng: " + ex.Message);
            }
        }

        // ==================== GỬI TIN NHẮN ====================
        private async void btnGui_Click(object sender, EventArgs e)
        {
            string noiDung = txtNhapTinNhan.Text.Trim();
            if (string.IsNullOrEmpty(noiDung))
            {
                MessageBox.Show("Nhập nội dung tin nhắn trước khi gửi!");
                return;
            }

            if (string.IsNullOrEmpty(tenDoiPhuong))
            {
                MessageBox.Show("Hãy chọn một người để trò chuyện!");
                return;
            }

            string cid = TaoIdCuocTroChuyen(tenHienTai, tenDoiPhuong);

            TinNhan tn = new TinNhan
            {
                guiBoi = tenHienTai,
                nhanBoi = tenDoiPhuong,
                noiDung = noiDung,
                thoiGian = DateTime.Now.ToString("HH:mm:ss dd/MM/yyyy")
            };

            var push = await firebase.PushAsync($"cuocTroChuyen/{cid}/", tn);
            tn.id = push.Result.name;
            await firebase.SetAsync($"cuocTroChuyen/{cid}/{tn.id}", tn);

            ThemTinNhanVaoKhung(tn);
            txtNhapTinNhan.Text = "";
        }

        // ==================== TẢI LỊCH SỬ TIN NHẮN ====================
        private async Task TaiLichSuTinNhan()
        {
            if (string.IsNullOrEmpty(tenDoiPhuong)) return;

            string cid = TaoIdCuocTroChuyen(tenHienTai, tenDoiPhuong);
            var res = await firebase.GetAsync($"cuocTroChuyen/{cid}");
            var data = res.ResultAs<Dictionary<string, TinNhan>>();

            flbKhungChat.Controls.Clear();

            if (data != null)
            {
                foreach (var tn in data.Values.OrderBy(x => x.thoiGian))
                {
                    ThemTinNhanVaoKhung(tn);
                }
            }
        }

        // ==================== HIỂN THỊ TIN NHẮN ====================
        private void ThemTinNhanVaoKhung(TinNhan tn)
        {
            Label lbl = new Label();
            lbl.AutoSize = true;
            lbl.MaximumSize = new Size(400, 0);
            lbl.Text = $"{tn.noiDung}\n({tn.thoiGian})";
            lbl.Padding = new Padding(10);
            lbl.Margin = new Padding(5);

            if (tn.guiBoi == tenHienTai)
            {
                lbl.BackColor = Color.LightSkyBlue;
                lbl.TextAlign = ContentAlignment.MiddleRight;
                lbl.Anchor = AnchorStyles.Right;
            }
            else
            {
                lbl.BackColor = Color.LightGray;
                lbl.TextAlign = ContentAlignment.MiddleLeft;
                lbl.Anchor = AnchorStyles.Left;
            }

            flbKhungChat.Controls.Add(lbl);
            flbKhungChat.ScrollControlIntoView(lbl);
        }

        // ==================== HÀM PHỤ ====================
        private string TaoIdCuocTroChuyen(string a, string b)
        {
            // Ghép tên theo thứ tự chữ cái để 2 bên cùng trỏ 1 node Firebase
            return string.CompareOrdinal(a, b) < 0 ? $"{a}_{b}" : $"{b}_{a}";
        }
    }

    // ==================== MÔ HÌNH DỮ LIỆU ====================
    public class TinNhan
    {
        public string id { get; set; }
        public string guiBoi { get; set; }
        public string nhanBoi { get; set; }
        public string noiDung { get; set; }
        public string thoiGian { get; set; }
    }

    public class UserFirebase
    {
        public string TaiKhoan { get; set; }
        public string MatKhau { get; set; }
        public string Email { get; set; }
        public string Ten { get; set; }
        public string Gioitinh { get; set; }
        public string Ngaysinh { get; set; }
    }
}
