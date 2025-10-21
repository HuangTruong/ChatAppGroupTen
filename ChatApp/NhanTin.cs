using FireSharp.Config;
using FireSharp.Interfaces;
using FireSharp.Response;
using FireSharp.EventStreaming; 
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

        // Ghi nhớ id tin nhắn đã render theo từng cuộc chat
        private Dictionary<string, HashSet<string>> dsTinNhanDaCoTheoDoanChat = new Dictionary<string, HashSet<string>>();

        // Timer dự phòng (poll)
        private Timer timerTinNhanMoi;

        // Realtime stream cho cuộc chat hiện tại
        private EventStreamResponse streamChatHienTai;

        // Realtime stream cho kết bạn
        private EventStreamResponse streamFriendReq, streamFriends;

        // Trạng thái bạn bè / lời mời
        private Dictionary<string, bool> danhSachBanBe = new Dictionary<string, bool>();
        private HashSet<string> danhSachDaGuiLoiMoi = new HashSet<string>();
        private HashSet<string> danhSachLoiMoiNhanDuoc = new HashSet<string>();

        public NhanTin(string tenDangNhap)
        {
            InitializeComponent();

            tenHienTai = tenDangNhap; 

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
            lblTenDangNhapPhai.Text = tenHienTai;

            // nạp trạng thái kết bạn + bật realtime kết bạn
            await NapTrangThaiKetBan();
            BatRealtimeKetBan();

            // tải danh sách user ban đầu
            await TaiDanhSachNguoiDung();
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            try { streamChatHienTai?.Dispose(); } catch { }
            try { streamFriendReq?.Dispose(); } catch { }
            try { streamFriends?.Dispose(); } catch { }
            try { timerTinNhanMoi?.Stop(); timerTinNhanMoi?.Dispose(); } catch { }
            base.OnFormClosed(e);
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
                    flpDanhSachChat.Controls.Add(new Label() { Text = "Không có người dùng nào!", AutoSize = true });
                    return;
                }

                foreach (var item in data)
                {
                    var user = item.Value;
                    if (user == null) continue;
                    if (string.Equals(user.Ten, tenHienTai, StringComparison.OrdinalIgnoreCase))
                        continue;

                    // trạng thái hiển thị
                    string trangThai = "";
                    if (danhSachBanBe.ContainsKey(user.Ten))
                        trangThai = " (Bạn bè)";
                    else if (danhSachDaGuiLoiMoi.Contains(user.Ten))
                        trangThai = " (Đã mời)";
                    else if (danhSachLoiMoiNhanDuoc.Contains(user.Ten))
                        trangThai = " (Mời bạn)";

                    var btn = new Button
                    {
                        Text = $"{user.Ten}{trangThai}",
                        Tag = user.Ten,
                        Width = flpDanhSachChat.Width - 25,
                        Height = 40,
                        TextAlign = ContentAlignment.MiddleLeft,
                        BackColor = Color.WhiteSmoke,
                        FlatStyle = FlatStyle.Flat
                    };

                    // menu chuột phải: kết bạn / chấp nhận / huỷ
                    var cm = new ContextMenuStrip();
                    if (!danhSachBanBe.ContainsKey(user.Ten) &&
                        !danhSachDaGuiLoiMoi.Contains(user.Ten) &&
                        !danhSachLoiMoiNhanDuoc.Contains(user.Ten))
                    {
                        cm.Items.Add("Kết bạn", null, async (s, e2) =>
                        {
                            await GuiLoiMoiKetBan(user.Ten);
                            await NapTrangThaiKetBan();
                            await TaiDanhSachNguoiDung();
                        });
                    }
                    if (danhSachLoiMoiNhanDuoc.Contains(user.Ten))
                    {
                        cm.Items.Add("Chấp nhận kết bạn", null, async (s, e2) =>
                        {
                            await ChapNhanKetBan(user.Ten);
                            await NapTrangThaiKetBan();
                            await TaiDanhSachNguoiDung();
                        });
                    }
                    if (danhSachBanBe.ContainsKey(user.Ten))
                    {
                        cm.Items.Add("Huỷ kết bạn", null, async (s, e2) =>
                        {
                            await HuyKetBan(user.Ten);
                            await NapTrangThaiKetBan();
                            await TaiDanhSachNguoiDung();
                        });
                    }
                    btn.ContextMenuStrip = cm;

                    // click trái: mở chat (bật realtime + timer)
                    btn.Click += async (s, e) =>
                    {
                        tenDoiPhuong = (string)((Button)s).Tag;
                        lblTenDangNhapGiua.Text = tenDoiPhuong;
                        flbKhungChat.Controls.Clear();

                        string cid = TaoIdCuocTroChuyen(tenHienTai, tenDoiPhuong);
                        if (!dsTinNhanDaCoTheoDoanChat.ContainsKey(cid))
                            dsTinNhanDaCoTheoDoanChat[cid] = new HashSet<string>();

                        // nạp ngay lịch sử
                        await CapTinNhanMoi();

                        // bật realtime + timer fallback
                        BatRealtimeChatHienTai();
                        BatTimerKiemTraTinNhanMoi();
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

            btnGui.Enabled = false;
            Cursor.Current = Cursors.WaitCursor;

            try
            {
                string cid = TaoIdCuocTroChuyen(tenHienTai, tenDoiPhuong);

                TinNhan tn = new TinNhan
                {
                    guiBoi = tenHienTai,
                    nhanBoi = tenDoiPhuong,
                    noiDung = noiDung,
                    // ISO 8601 UTC để sắp xếp an toàn (lexicographical OK)
                    thoiGian = DateTime.UtcNow.ToString("o")
                };

                var push = await firebase.PushAsync($"cuocTroChuyen/{cid}/", tn);
                tn.id = push.Result.name;
                await firebase.SetAsync($"cuocTroChuyen/{cid}/{tn.id}", tn);

                // đánh dấu để không render trùng khi poll/stream cùng lúc
                if (!dsTinNhanDaCoTheoDoanChat.ContainsKey(cid))
                    dsTinNhanDaCoTheoDoanChat[cid] = new HashSet<string>();
                if (dsTinNhanDaCoTheoDoanChat[cid].Add(tn.id))
                    ThemTinNhanVaoKhung(tn);

                txtNhapTinNhan.Clear();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi gửi tin nhắn: " + ex.Message);
            }
            finally
            {
                btnGui.Enabled = true;
                Cursor.Current = Cursors.Default;
            }
        }

        // ==================== LOAD & THEO DÕI TIN NHẮN ====================
        // Nạp phần còn thiếu (tin mới so với dsTinNhanDaCoTheoDoanChat)
        private async Task CapTinNhanMoi()
        {
            if (string.IsNullOrEmpty(tenDoiPhuong)) return;

            string cid = TaoIdCuocTroChuyen(tenHienTai, tenDoiPhuong);
            var res = await firebase.GetAsync($"cuocTroChuyen/{cid}");
            var data = res.ResultAs<Dictionary<string, TinNhan>>();

            if (!dsTinNhanDaCoTheoDoanChat.ContainsKey(cid))
                dsTinNhanDaCoTheoDoanChat[cid] = new HashSet<string>();
            var dsDaCo = dsTinNhanDaCoTheoDoanChat[cid];

            if (data != null)
            {
                foreach (var tn in data.Values.OrderBy(x => x.thoiGian))
                {
                    if (string.IsNullOrEmpty(tn.id)) continue; // phòng rủi ro
                    if (dsDaCo.Add(tn.id))
                        ThemTinNhanVaoKhung(tn);
                }
            }
        }

        // Bật timer kiểm tra tin nhắn mới (dự phòng)
        private void BatTimerKiemTraTinNhanMoi()
        {
            try
            {
                if (timerTinNhanMoi != null)
                {
                    timerTinNhanMoi.Stop();
                    timerTinNhanMoi.Dispose();
                }
            }
            catch { }

            timerTinNhanMoi = new Timer();
            timerTinNhanMoi.Interval = 2000;
            timerTinNhanMoi.Tick += async (s, e) => await CapTinNhanMoi();
            timerTinNhanMoi.Start();
        }

        // Bật realtime cho cuộc chat hiện tại
        private async void BatRealtimeChatHienTai()
        {
            try { streamChatHienTai?.Dispose(); } catch { }
            if (string.IsNullOrEmpty(tenDoiPhuong)) return;

            string cid = TaoIdCuocTroChuyen(tenHienTai, tenDoiPhuong);

            streamChatHienTai = await firebase.OnAsync(
                $"cuocTroChuyen/{cid}",
                added: async (s, a, c) => await UiCapNhatTinNhan(),
                changed: async (s, a, c) => await UiCapNhatTinNhan(),
                removed: async (s, a, c) => await UiCapNhatTinNhan()
            );
        }

        private Task UiCapNhatTinNhan()
        {
            if (!IsHandleCreated) return Task.CompletedTask;
            BeginInvoke(new Action(async () => await CapTinNhanMoi()));
            return Task.CompletedTask;
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
            // Ghép theo thứ tự chữ cái để 2 bên cùng trỏ 1 node
            return string.CompareOrdinal(a, b) < 0 ? $"{a}_{b}" : $"{b}_{a}";
        }

        // ==================== KẾT BẠN ====================
        private async Task NapTrangThaiKetBan()
        {
            danhSachBanBe.Clear();
            danhSachDaGuiLoiMoi.Clear();
            danhSachLoiMoiNhanDuoc.Clear();

            // Lấy danh sách bạn bè
            var f1 = await firebase.GetAsync($"friends/{tenHienTai}");
            var duLieuBanBe = f1.ResultAs<Dictionary<string, bool>>();
            if (duLieuBanBe != null)
                danhSachBanBe = duLieuBanBe;

            // Lấy danh sách mình mời người khác
            var tatCaLoiMoi = await firebase.GetAsync("friendRequests/pending");
            var duLieuLoiMoiCho = tatCaLoiMoi.ResultAs<Dictionary<string, Dictionary<string, bool>>>();
            if (duLieuLoiMoiCho != null)
            {
                foreach (var loiMoiDenNguoiNhan in duLieuLoiMoiCho)
                {
                    string nguoiNhan = loiMoiDenNguoiNhan.Key;
                    foreach (var loiMoiTuNguoiGui in loiMoiDenNguoiNhan.Value)
                    {
                        string nguoiGui = loiMoiTuNguoiGui.Key;
                        if (nguoiGui == tenHienTai)
                            danhSachDaGuiLoiMoi.Add(nguoiNhan);
                    }
                }
            }

            // Lấy danh sách người khác mời mình
            var f2 = await firebase.GetAsync($"friendRequests/pending/{tenHienTai}");
            var danhSachNguoiGuiLoiMoi = f2.ResultAs<Dictionary<string, bool>>();
            if (danhSachNguoiGuiLoiMoi != null)
                danhSachLoiMoiNhanDuoc = danhSachNguoiGuiLoiMoi.Keys.ToHashSet();
        }

        private async Task GuiLoiMoiKetBan(string ten)
        {
            await firebase.SetAsync($"friendRequests/pending/{ten}/{tenHienTai}", true);
            MessageBox.Show($"Đã gửi lời mời kết bạn đến {ten}!");
            danhSachDaGuiLoiMoi.Add(ten);
        }

        private async Task ChapNhanKetBan(string ten)
        {
            await firebase.SetAsync($"friends/{tenHienTai}/{ten}", true);
            await firebase.SetAsync($"friends/{ten}/{tenHienTai}", true);
            await firebase.DeleteAsync($"friendRequests/pending/{tenHienTai}/{ten}");

            MessageBox.Show($"Bạn và {ten} đã trở thành bạn bè!");
            danhSachBanBe[ten] = true;
            danhSachLoiMoiNhanDuoc.Remove(ten);
        }

        private async Task HuyKetBan(string ten)
        {
            await firebase.DeleteAsync($"friends/{tenHienTai}/{ten}");
            await firebase.DeleteAsync($"friends/{ten}/{tenHienTai}");
            danhSachBanBe.Remove(ten);
            MessageBox.Show($"Đã hủy kết bạn với {ten}.");
        }

        private void BatRealtimeKetBan()
        {
            try { streamFriendReq?.Dispose(); } catch { }
            try { streamFriends?.Dispose(); } catch { }

            // Lời mời đến mình
            firebase.OnAsync($"friendRequests/pending/{tenHienTai}",
                added: (s, a, c) => Invoke(new Action(async () =>
                {
                    await NapTrangThaiKetBan();
                    await TaiDanhSachNguoiDung();
                })),
                changed: (s, a, c) => Invoke(new Action(async () =>
                {
                    await NapTrangThaiKetBan();
                    await TaiDanhSachNguoiDung();
                })),
                removed: (s, a, c) => Invoke(new Action(async () =>
                {
                    await NapTrangThaiKetBan();
                    await TaiDanhSachNguoiDung();
                }))
            ).ContinueWith(t => { if (t.Status == TaskStatus.RanToCompletion) streamFriendReq = t.Result; });

            // Danh sách bạn
            firebase.OnAsync($"friends/{tenHienTai}",
                added: (s, a, c) => Invoke(new Action(async () =>
                {
                    await NapTrangThaiKetBan();
                    await TaiDanhSachNguoiDung();
                })),
                changed: (s, a, c) => Invoke(new Action(async () =>
                {
                    await NapTrangThaiKetBan();
                    await TaiDanhSachNguoiDung();
                })),
                removed: (s, a, c) => Invoke(new Action(async () =>
                {
                    await NapTrangThaiKetBan();
                    await TaiDanhSachNguoiDung();
                }))
            ).ContinueWith(t => { if (t.Status == TaskStatus.RanToCompletion) streamFriends = t.Result; });
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
