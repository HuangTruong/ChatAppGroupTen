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

        // Người đang được chọn để chat (tên hiển thị) - dành cho 1-1
        private string tenDoiPhuong = "";

        // Nếu đang chat nhóm thì id nhóm ở đây, và currentChatIsGroup = true
        private bool currentChatIsGroup = false;
        private string currentGroupId = "";

        // Ghi nhớ id tin nhắn đã render theo từng cuộc chat (key có thể là cid cho 1-1 hoặc idNhom)
        private Dictionary<string, HashSet<string>> dsTinNhanDaCoTheoDoanChat = new Dictionary<string, HashSet<string>>();

        // Timer dự phòng (poll)
        private Timer timerTinNhanMoi;

        // Realtime stream cho cuộc chat hiện tại
        private EventStreamResponse streamChatHienTai;

        // Realtime stream cho kết bạn
        private EventStreamResponse streamFriendReq, streamFriends;

        private EventStreamResponse streamNhom;

        // Trạng thái bạn bè / lời mời
        private Dictionary<string, bool> danhSachBanBe = new Dictionary<string, bool>();
        private HashSet<string> danhSachDaGuiLoiMoi = new HashSet<string>();
        private HashSet<string> danhSachLoiMoiNhanDuoc = new HashSet<string>();

        private bool dangTaiDanhSachNguoiDung = false; // tránh load trùng

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

            // tải danh sách nhóm
            await TaiDanhSachNhom();
            BatRealtimeNhom();
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
            // tránh trùng lặp do realtime kích hoạt liên tục
            if (dangTaiDanhSachNguoiDung) return;
            dangTaiDanhSachNguoiDung = true;

            try
            {
                var res = await firebase.GetAsync("users");
                var data = res.ResultAs<Dictionary<string, UserFirebase>>();

                // Dọn UI an toàn trong luồng chính
                if (InvokeRequired)
                {
                    Invoke(new Action(() => flpDanhSachChat.Controls.Clear()));
                }
                else
                {
                    flpDanhSachChat.Controls.Clear();
                }

                if (data == null)
                {
                    Invoke(new Action(() =>
                    {
                        flpDanhSachChat.Controls.Add(new Label() { Text = "Không có người dùng nào!", AutoSize = true });
                    }));
                    return;
                }

                // Duyệt danh sách user
                foreach (var item in data)
                {
                    var user = item.Value;
                    if (user == null) continue;
                    if (string.Equals(user.Ten, tenHienTai, StringComparison.OrdinalIgnoreCase))
                        continue;

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
                        Tag = $"user:{user.Ten}",
                        Width = flpDanhSachChat.Width - 25,
                        Height = 40,
                        TextAlign = ContentAlignment.MiddleLeft,
                        BackColor = Color.WhiteSmoke,
                        FlatStyle = FlatStyle.Flat
                    };

                    // Menu chuột phải: kết bạn / chấp nhận / huỷ
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

                    // Mở chat 1-1
                    btn.Click += async (s, e) =>
                    {
                        tenDoiPhuong = user.Ten;
                        currentChatIsGroup = false;
                        currentGroupId = "";
                        lblTenDangNhapGiua.Text = tenDoiPhuong;
                        flbKhungChat.Controls.Clear();

                        string cid = TaoIdCuocTroChuyen(tenHienTai, tenDoiPhuong);
                        dsTinNhanDaCoTheoDoanChat[cid] = new HashSet<string>();

                        await CapTinNhanMoi();
                        BatRealtimeChatHienTai();
                        BatTimerKiemTraTinNhanMoi();
                    };

                    // Thêm nút vào UI thread an toàn
                    if (InvokeRequired)
                        Invoke(new Action(() => flpDanhSachChat.Controls.Add(btn)));
                    else
                        flpDanhSachChat.Controls.Add(btn);
                }

                // Sau khi load xong user mới load nhóm (đảm bảo không bị lặp)
                await TaiDanhSachNhom();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải danh sách người dùng: " + ex.Message);
            }
            finally
            {
                dangTaiDanhSachNguoiDung = false;
            }
        }


        // ==================== TẠO NHÓM ====================
        private async void btnTaoNhom_Click(object sender, EventArgs e)
        {
            string tenNhom = Microsoft.VisualBasic.Interaction.InputBox("Nhập tên nhóm:", "Tạo nhóm");
            if (string.IsNullOrEmpty(tenNhom)) return;

            // Chọn bạn bè thêm vào nhóm - đơn giản hiện dialog chọn từng người bằng MessageBox (có thể thay bằng form chọn)
            var thanhVien = new Dictionary<string, bool> { { tenHienTai, true } };
            // Tạo nhóm object
            var nhom = new Nhom
            {
                id = Guid.NewGuid().ToString(),
                tenNhom = tenNhom,
                thanhVien = thanhVien,
                taoBoi = tenHienTai
            };

            try
            {
                await firebase.SetAsync($"nhom/{nhom.id}", nhom);
                MessageBox.Show($"Đã tạo nhóm: {tenNhom}");
                await TaiDanhSachNhom();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi tạo nhóm: " + ex.Message);
            }
        }

        // ==================== TẢI DANH SÁCH NHÓM ====================
        private async Task TaiDanhSachNhom()
        {
            try
            {
                var res = await firebase.GetAsync("nhom");
                var data = res.ResultAs<Dictionary<string, Nhom>>();

                // Xóa các button nhóm cũ trước khi thêm (tag bắt đầu bằng "group:")
                var groupButtons = flpDanhSachChat.Controls.Cast<Control>()
                    .OfType<Button>()
                    .Where(b => b.Tag is string t && t.StartsWith("group:"))
                    .ToList();
                foreach (var b in groupButtons) flpDanhSachChat.Controls.Remove(b);

                if (data == null) return;

                foreach (var item in data)
                {
                    var nhom = item.Value;
                    if (nhom == null || nhom.thanhVien == null) continue;
                    if (!nhom.thanhVien.ContainsKey(tenHienTai)) continue; // chỉ hiển thị nhóm mình là thành viên

                    var btn = new Button
                    {
                        Text = $"[Nhóm] {nhom.tenNhom}",
                        Tag = $"group:{nhom.id}",
                        Width = flpDanhSachChat.Width - 25,
                        Height = 40,
                        BackColor = Color.LightYellow,
                        FlatStyle = FlatStyle.Flat,
                        TextAlign = ContentAlignment.MiddleLeft
                    };

                    // Menu chuột phải cho nhóm
                    var cm = new ContextMenuStrip();
                    cm.Items.Add("Thêm thành viên", null, async (s2, e2) =>
                    {
                        await ThemThanhVienVaoNhom(nhom.id);
                    });

                    // Nếu mình là người tạo nhóm thì thêm mục "Xóa nhóm"
                    if (nhom.taoBoi == tenHienTai)
                    {
                        cm.Items.Add("Xóa nhóm", null, async (s2, e2) =>
                        {
                            var confirm = MessageBox.Show(
                                $"Bạn có chắc muốn xóa nhóm \"{nhom.tenNhom}\" không?",
                                "Xóa nhóm",
                                MessageBoxButtons.YesNo,
                                MessageBoxIcon.Warning
                            );
                            if (confirm == DialogResult.Yes)
                            {
                                await XoaNhom(nhom.id);
                            }
                        });
                    }

                    btn.ContextMenuStrip = cm;

                    btn.Click += async (s, e) =>
                    {
                        var tag = (string)((Button)s).Tag;
                        if (tag != null && tag.StartsWith("group:"))
                        {
                            currentChatIsGroup = true;
                            currentGroupId = tag.Substring("group:".Length);
                            tenDoiPhuong = "";
                            lblTenDangNhapGiua.Text = nhom.tenNhom;
                            flbKhungChat.Controls.Clear();

                            dsTinNhanDaCoTheoDoanChat[currentGroupId] = new HashSet<string>();

                            await CapTinNhanMoiNhom(currentGroupId);
                            BatRealtimeChatNhom(currentGroupId);
                            BatTimerKiemTraTinNhanMoi();
                        }
                    };

                    flpDanhSachChat.Controls.Add(btn);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải nhóm: " + ex.Message);
            }
        }

        private async Task ThemThanhVienVaoNhom(string idNhom)
        {
            try
            {
                // Lấy nhóm hiện tại từ Firebase
                var res = await firebase.GetAsync($"nhom/{idNhom}");
                var nhom = res.ResultAs<Nhom>();
                if (nhom == null)
                {
                    MessageBox.Show("Không tìm thấy nhóm!");
                    return;
                }

                // Lọc ra danh sách bạn bè chưa có trong nhóm
                var banChuaCo = danhSachBanBe.Keys
                    .Where(b => !nhom.thanhVien.ContainsKey(b))
                    .ToList();

                // Mở form chọn thành viên
                using (var form = new ChonThanhVien(banChuaCo))
                {
                    if (form.ShowDialog() == DialogResult.OK)
                    {
                        var duocChon = form.ThanhVienDuocChon;
                        if (duocChon.Count == 0)
                        {
                            MessageBox.Show("Chưa chọn ai để thêm!");
                            return;
                        }

                        foreach (var ten in duocChon)
                        {
                            nhom.thanhVien[ten] = true;
                            await firebase.SetAsync($"nhom/{idNhom}/thanhVien/{ten}", true);
                        }

                        MessageBox.Show($"Đã thêm {duocChon.Count} thành viên vào nhóm!");
                        await TaiDanhSachNhom();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi thêm thành viên: " + ex.Message);
            }
        }

        private async Task XoaNhom(string idNhom)
        {
            try
            {
                // Xóa nhóm trong Firebase
                await firebase.DeleteAsync($"nhom/{idNhom}");
                await firebase.DeleteAsync($"cuocTroChuyenNhom/{idNhom}");

                // Cập nhật lại giao diện
                MessageBox.Show("Đã xóa nhóm thành công!");
                await TaiDanhSachNhom();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi xóa nhóm: " + ex.Message);
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

            // Nếu đang chat nhóm
            if (currentChatIsGroup)
            {
                if (string.IsNullOrEmpty(currentGroupId))
                {
                    MessageBox.Show("Hãy chọn một nhóm để trò chuyện!");
                    return;
                }

                btnGui.Enabled = false;
                Cursor.Current = Cursors.WaitCursor;
                try
                {
                    TinNhan tn = new TinNhan
                    {
                        guiBoi = tenHienTai,
                        nhanBoi = "", // không cần trường nhận cá nhân cho nhóm
                        noiDung = noiDung,
                        thoiGian = DateTime.UtcNow.ToString("o")
                    };

                    var push = await firebase.PushAsync($"cuocTroChuyenNhom/{currentGroupId}/", tn);
                    tn.id = push.Result.name;
                    await firebase.SetAsync($"cuocTroChuyenNhom/{currentGroupId}/{tn.id}", tn);

                    if (!dsTinNhanDaCoTheoDoanChat.ContainsKey(currentGroupId))
                        dsTinNhanDaCoTheoDoanChat[currentGroupId] = new HashSet<string>();
                    if (dsTinNhanDaCoTheoDoanChat[currentGroupId].Add(tn.id))
                        ThemTinNhanVaoKhung(tn);

                    txtNhapTinNhan.Clear();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi khi gửi tin nhắn nhóm: " + ex.Message);
                }
                finally
                {
                    btnGui.Enabled = true;
                    Cursor.Current = Cursors.Default;
                }
                return;
            }

            // Nếu chat 1-1 (mặc định)
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
        // Nạp phần còn thiếu (tin mới so với dsTinNhanDaCoTheoDoanChat) - cho 1-1
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

        // Nạp cho nhóm
        private async Task CapTinNhanMoiNhom(string idNhom)
        {
            if (string.IsNullOrEmpty(idNhom)) return;

            var res = await firebase.GetAsync($"cuocTroChuyenNhom/{idNhom}");
            var data = res.ResultAs<Dictionary<string, TinNhan>>();

            if (!dsTinNhanDaCoTheoDoanChat.ContainsKey(idNhom))
                dsTinNhanDaCoTheoDoanChat[idNhom] = new HashSet<string>();
            var dsDaCo = dsTinNhanDaCoTheoDoanChat[idNhom];

            if (data != null)
            {
                foreach (var tn in data.Values.OrderBy(x => x.thoiGian))
                {
                    if (string.IsNullOrEmpty(tn.id)) continue;
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
            timerTinNhanMoi.Tick += async (s, e) =>
            {
                if (currentChatIsGroup)
                {
                    if (!string.IsNullOrEmpty(currentGroupId))
                        await CapTinNhanMoiNhom(currentGroupId);
                }
                else
                {
                    await CapTinNhanMoi();
                }
            };
            timerTinNhanMoi.Start();
        }

        // Bật realtime cho cuộc chat hiện tại (1-1)
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

        // Bật realtime cho nhóm
        private async void BatRealtimeChatNhom(string idNhom)
        {
            try { streamChatHienTai?.Dispose(); } catch { }
            if (string.IsNullOrEmpty(idNhom)) return;

            streamChatHienTai = await firebase.OnAsync(
                $"cuocTroChuyenNhom/{idNhom}",
                added: async (s, a, c) => await UiCapNhatTinNhan(),
                changed: async (s, a, c) => await UiCapNhatTinNhan(),
                removed: async (s, a, c) => await UiCapNhatTinNhan()
            );
        }

        private Task UiCapNhatTinNhan()
        {
            if (!IsHandleCreated) return Task.CompletedTask;
            BeginInvoke(new Action(async () =>
            {
                if (currentChatIsGroup)
                {
                    if (!string.IsNullOrEmpty(currentGroupId))
                        await CapTinNhanMoiNhom(currentGroupId);
                }
                else
                {
                    await CapTinNhanMoi();
                }
            }));
            return Task.CompletedTask;
        }

        // ==================== HIỂN THỊ TIN NHẮN ====================
        private void ThemTinNhanVaoKhung(TinNhan tn)
        {
            Label lbl = new Label();
            lbl.AutoSize = true;
            lbl.MaximumSize = new Size(400, 0);

            // Nếu là nhóm, hiển thị tên người gửi trước nội dung
            string hienThiNoiDung;
            if (currentChatIsGroup)
            {
                // Hiển thị: "Người gửi: Nội dung"
                hienThiNoiDung = $"{tn.guiBoi}: {tn.noiDung}\n({tn.thoiGian})";
            }
            else
            {
                hienThiNoiDung = $"{tn.noiDung}\n({tn.thoiGian})";
            }

            lbl.Text = hienThiNoiDung;
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
            try { streamFriends?.Dispose(); } catch { }
            try { streamFriendReq?.Dispose(); } catch { }

            // Realtime bạn bè (khi đã là bạn)
            firebase.OnAsync($"friends/{tenHienTai}",
                added: async (s, a, c) => await RefreshBanBe(),
                changed: async (s, a, c) => await RefreshBanBe(),
                removed: async (s, a, c) => await RefreshBanBe()
            ).ContinueWith(t =>
            {
                if (t.Status == TaskStatus.RanToCompletion)
                    streamFriends = t.Result;
            });

            // Realtime lời mời kết bạn đến mình
            firebase.OnAsync($"friendRequests/pending/{tenHienTai}",
                added: (s, a, c) =>
                {
                    // dùng Invoke để cập nhật UI an toàn
                    BeginInvoke(new Action(async () =>
                    {
                        await NapTrangThaiKetBan();

                        string nguoiGui = a.Path?.TrimStart('/');
                        if (!string.IsNullOrEmpty(nguoiGui))
                        {
                            // hiện popup thông báo nhẹ
                            MessageBox.Show($"📩 Bạn có lời mời kết bạn mới từ {nguoiGui}!", "Kết bạn mới");
                        }

                        // chỉ cập nhật nhãn/trạng thái, không reload toàn bộ danh sách
                        await TaiDanhSachNguoiDung();
                    }));
                },
                removed: (s, a, c) =>
                {
                    // khi lời mời bị huỷ hoặc chấp nhận thì chỉ cập nhật trạng thái
                    BeginInvoke(new Action(async () =>
                    {
                        await NapTrangThaiKetBan();
                        await TaiDanhSachNguoiDung();
                    }));
                }
            ).ContinueWith(t =>
            {
                if (t.Status == TaskStatus.RanToCompletion)
                    streamFriendReq = t.Result;
            });
        }

        private async Task RefreshBanBe()
        {
            try
            {
                await NapTrangThaiKetBan();
                // đảm bảo gọi lại UI trong thread chính
                if (InvokeRequired)
                    BeginInvoke(new Action(async () => await TaiDanhSachNguoiDung()));
                else
                    await TaiDanhSachNguoiDung();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi làm mới danh sách bạn bè: " + ex.Message);
            }
        }

        private void BatRealtimeNhom()
        {
            try { streamNhom?.Dispose(); } catch { }

            firebase.OnAsync("nhom",
                added: (s, a, c) => XuLyCapNhatNhom(),
                changed: (s, a, c) => XuLyCapNhatNhom(),
                removed: (s, a, c) => XuLyCapNhatNhom()
            ).ContinueWith(t =>
            {
                if (t.Status == TaskStatus.RanToCompletion)
                    streamNhom = t.Result;
            });
        }

        private void XuLyCapNhatNhom()
        {
            // Dùng BeginInvoke để tránh cross-thread
            BeginInvoke(new Action(async () =>
            {
                await TaiDanhSachNhom();
            }));
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

    public class Nhom
    {
        public string id { get; set; }
        public string tenNhom { get; set; }
        public Dictionary<string, bool> thanhVien { get; set; } = new Dictionary<string, bool>();
        public string taoBoi { get; set; }
    }
}
