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
using System.Collections.Concurrent;
using System.Threading;
using System.Net;
using System.Globalization;

namespace ChatApp
{
    public partial class NhanTin : Form
    {
        private readonly ConcurrentQueue<TinNhan> hangChoRender = new ConcurrentQueue<TinNhan>();
        private System.Windows.Forms.Timer timerRender;

        private readonly SemaphoreSlim khoaTaiTinNhan = new SemaphoreSlim(1, 1);

        private System.Windows.Forms.Timer timerGomSuKienChat;
        private DateTimeOffset lanCuoiTai1_1 = DateTimeOffset.MinValue;
        private DateTimeOffset lanCuoiTaiNhom = DateTimeOffset.MinValue;

        private IFirebaseClient firebase;

        private string tenHienTai;
        private string tenDoiPhuong = "";

        private bool currentChatIsGroup = false;
        private string currentGroupId = "";

        private Dictionary<string, HashSet<string>> dsTinNhanDaCoTheoDoanChat = new Dictionary<string, HashSet<string>>();
        private Dictionary<string, List<string>> thuTuTinNhanTheoDoanChat = new Dictionary<string, List<string>>();

        private System.Windows.Forms.Timer timerTinNhanMoi;

        private EventStreamResponse streamChatHienTai;
        private EventStreamResponse streamFriendReq, streamFriends;
        private EventStreamResponse streamNhom;

        private EventStreamResponse streamTyping;
        private System.Windows.Forms.Timer timerDangNhap;
        private long hetHanTyping = 0;

        private Label lblTyping;

        private Dictionary<string, bool> danhSachBanBe = new Dictionary<string, bool>();
        private HashSet<string> danhSachDaGuiLoiMoi = new HashSet<string>();
        private HashSet<string> danhSachLoiMoiNhanDuoc = new HashSet<string>();

        private bool dangTaiDanhSachNguoiDung = false;

        private EventStreamResponse streamTrangThai;
        private Dictionary<string, string> trangThaiNguoiDung = new Dictionary<string, string>();

        private System.Windows.Forms.Timer timerResize;

        public NhanTin(string tenDangNhap)
        {
            InitializeComponent();

            ServicePointManager.DefaultConnectionLimit = Math.Max(100, ServicePointManager.DefaultConnectionLimit);

            // Khung chat
            flbKhungChat.FlowDirection = FlowDirection.TopDown;
            flbKhungChat.WrapContents = false;
            flbKhungChat.AutoScroll = true;
            flbKhungChat.Padding = new Padding(0);

            tenHienTai = tenDangNhap;

            IFirebaseConfig config = new FirebaseConfig
            {
                AuthSecret = "j0kBCfIQBOBtgq5j0RaocJLgCuJO1AMn2GS5qXqH",
                BasePath = "https://chatapp-ca701-default-rtdb.asia-southeast1.firebasedatabase.app/"
            };
            firebase = new FireSharp.FirebaseClient(config);
            if (firebase == null) MessageBox.Show("Không kết nối được Firebase!");

            // “Đang nhập…”
            lblTyping = new Label
            {
                AutoSize = true,
                ForeColor = Color.DimGray,
                Text = "",
                Visible = false
            };
            lblTyping.Location = new Point(lblTenDangNhapGiua.Left, lblTenDangNhapGiua.Bottom + 4);
            this.Controls.Add(lblTyping);

            // ======= ENTER để gửi tin (Shift+Enter xuống dòng) =======
            this.KeyPreview = true;             // form bắt phím trước
            this.AcceptButton = btnGui;         // ENTER = btnGui (fallback toàn form)
            txtNhapTinNhan.KeyDown += TxtNhapTinNhan_KeyDown;
            // =========================================================

            // Debounce resize
            timerResize = new System.Windows.Forms.Timer { Interval = 120 };
            timerResize.Tick += (s, e) =>
            {
                timerResize.Stop();
                RecomputeBubbleWidths(true);
            };
            flbKhungChat.Resize += (s, e) =>
            {
                timerResize.Stop();
                timerResize.Start();
            };
        }

        private async void NhanTin_Load(object sender, EventArgs e)
        {
            lblTenDangNhapPhai.Text = tenHienTai;

            await CapNhatTrangThai("online");
            BatRealtimeTrangThai();

            await NapTrangThaiKetBan();
            BatRealtimeKetBan();

            await TaiDanhSachNguoiDung();

            await TaiDanhSachNhom();
            BatRealtimeNhom();

            KhoiTaoTyping();
            KhoiTaoRenderMem();

            RecomputeBubbleWidths(true);
        }

        // ===== ENTER handler cho ô nhập =====
        private void TxtNhapTinNhan_KeyDown(object sender, KeyEventArgs e)
        {
            // Enter thường: gửi; Shift+Enter hoặc Ctrl+Enter: xuống dòng
            if (e.KeyCode == Keys.Enter && !e.Shift && !e.Control)
            {
                e.SuppressKeyPress = true;   // không chèn newline, không beep
                if (btnGui.Enabled) btnGui.PerformClick();
            }
        }

        // ===== Render batch =====
        private void KhoiTaoRenderMem()
        {
            var prop = typeof(Panel).GetProperty("DoubleBuffered",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            if (prop != null) prop.SetValue(flbKhungChat, true, null);

            timerRender = new System.Windows.Forms.Timer();
            timerRender.Interval = 80;
            timerRender.Tick += (s, e) => FlushRender();
            timerRender.Start();
        }

        const int MAX_BUBBLES = 300;

        private int GetMaxBubbleWidth()
        {
            int scrollW = flbKhungChat.VerticalScroll.Visible ? SystemInformation.VerticalScrollBarWidth : 0;
            return Math.Max(220, flbKhungChat.ClientSize.Width - (100 + scrollW));
        }

        private static DateTime ParseTime(string s)
        {
            if (string.IsNullOrWhiteSpace(s))
                return DateTime.UtcNow;

            long ms;
            if (long.TryParse(s, out ms) && ms > 946684800000L && ms < 4102444800000L)
            {
                try { return DateTimeOffset.FromUnixTimeMilliseconds(ms).UtcDateTime; }
                catch { }
            }

            DateTimeOffset dtoExact;
            if (DateTimeOffset.TryParseExact(
                    s, "o", CultureInfo.InvariantCulture,
                    DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
                    out dtoExact))
                return dtoExact.UtcDateTime;

            DateTimeOffset dto;
            if (DateTimeOffset.TryParse(
                    s, CultureInfo.InvariantCulture,
                    DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
                    out dto))
                return dto.UtcDateTime;

            DateTime dt;
            if (DateTime.TryParse(
                    s, CultureInfo.InvariantCulture,
                    DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
                    out dt))
                return dt;

            return DateTime.UtcNow;
        }

        private void RecomputeBubbleWidths(bool realign)
        {
            if (!IsHandleCreated || IsDisposed) return;

            int panelW = flbKhungChat.ClientSize.Width;
            int textMax = GetMaxBubbleWidth();

            flbKhungChat.SuspendLayout();
            foreach (Control ctrl in flbKhungChat.Controls)
            {
                Panel row = ctrl as Panel;
                if (row == null) continue;

                if (row.Width != panelW)
                    row.Width = panelW;

                if (row.Controls.Count > 0)
                {
                    Panel bubble = row.Controls[0] as Panel;
                    if (bubble != null)
                    {
                        var content = bubble.Controls.Count > 0 ? bubble.Controls[0] as FlowLayoutPanel : null;
                        if (content != null)
                        {
                            foreach (Control c in content.Controls)
                            {
                                Label lbl = c as Label;
                                if (lbl != null && lbl.Name == "lblMsg")
                                {
                                    int cap = Math.Max(50, textMax - bubble.Padding.Horizontal);
                                    if (lbl.MaximumSize.Width != cap)
                                        lbl.MaximumSize = new Size(cap, 0);
                                }
                            }
                        }

                        if (realign) AlignBubbleInRow(row);
                    }
                }
            }
            flbKhungChat.ResumeLayout(true);
        }

        private void FlushRender()
        {
            if (!this.IsHandleCreated || this.IsDisposed) return;
            if (hangChoRender.IsEmpty) return;

            List<TinNhan> gom = new List<TinNhan>(50);
            TinNhan tnTmp;
            while (gom.Count < 50 && hangChoRender.TryDequeue(out tnTmp))
                gom.Add(tnTmp);
            if (gom.Count == 0) return;

            bool oCuoiKhungChat = false;
            if (flbKhungChat.VerticalScroll.Visible)
            {
                int max = Math.Max(0, flbKhungChat.VerticalScroll.Maximum - flbKhungChat.VerticalScroll.LargeChange);
                oCuoiKhungChat = flbKhungChat.VerticalScroll.Value >= max;
            }
            else
            {
                oCuoiKhungChat = true;
            }

            flbKhungChat.SuspendLayout();
            bool oldAuto = flbKhungChat.AutoScroll;
            flbKhungChat.AutoScroll = false;

            foreach (TinNhan tn in gom)
                TaoBubbleVaChen(tn);

            int over = flbKhungChat.Controls.Count - MAX_BUBBLES;
            if (over > 0)
            {
                for (int i = 0; i < over; i++)
                    flbKhungChat.Controls[i].Dispose();
                for (int i = 0; i < over; i++)
                    flbKhungChat.Controls.RemoveAt(0);
            }

            flbKhungChat.AutoScroll = oldAuto;
            flbKhungChat.ResumeLayout(true);

            if (oCuoiKhungChat && flbKhungChat.Controls.Count > 0)
            {
                flbKhungChat.ScrollControlIntoView(flbKhungChat.Controls[flbKhungChat.Controls.Count - 1]);
            }
        }

        // ===== Tạo bubble thủ công (không dùng TinNhanBubble) =====
        private void TaoBubbleVaChen(TinNhan tn)
        {
            bool laNhom = currentChatIsGroup;
            bool laCuaToi = string.Equals(tn.guiBoi, tenHienTai, StringComparison.OrdinalIgnoreCase);

            Panel row = new Panel
            {
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Padding = laCuaToi ? new Padding(60, 2, 8, 8) : new Padding(8, 2, 60, 8),
                Margin = new Padding(0, 2, 0, 2),
                Width = flbKhungChat.ClientSize.Width,
                Tag = laCuaToi
            };

            Panel bubble = new Panel
            {
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Padding = new Padding(10, 6, 10, 6),
                BackColor = laCuaToi ? Color.FromArgb(222, 242, 255) : Color.White,
                BorderStyle = BorderStyle.None
            };

            FlowLayoutPanel stack = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Margin = Padding.Empty,
                Padding = Padding.Empty
            };

            if (laNhom)
            {
                Label lblSender = new Label
                {
                    AutoSize = true,
                    Text = tn.guiBoi ?? "",
                    Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                    ForeColor = Color.DimGray,
                    Margin = new Padding(0, 0, 0, 2),
                    Name = "lblSender"
                };
                stack.Controls.Add(lblSender);
            }

            string text = tn.noiDung ?? "";
            Label lblMsg = new Label
            {
                AutoSize = true,
                MaximumSize = new Size(Math.Max(50, GetMaxBubbleWidth() - bubble.Padding.Horizontal), 0),
                Text = text.Length == 0 ? " " : text,
                Font = new Font("Segoe UI", 10f, FontStyle.Regular),
                ForeColor = Color.Black,
                Margin = new Padding(0, 0, 0, 4),
                Name = "lblMsg",
                UseMnemonic = false
            };
            lblMsg.AutoEllipsis = false;

            Label lblTime = new Label
            {
                AutoSize = true,
                Text = ParseTime(tn.thoiGian).ToLocalTime().ToString("HH:mm dd/MM/yyyy"),
                Font = new Font("Segoe UI", 8.5f, FontStyle.Italic),
                ForeColor = Color.DimGray,
                Margin = new Padding(0, 0, 0, 0),
                Name = "lblTime"
            };

            stack.Controls.Add(lblMsg);
            stack.Controls.Add(lblTime);
            bubble.Controls.Add(stack);

            ContextMenuStrip menu = new ContextMenuStrip();
            menu.Items.Add("Trả lời (trích dẫn)", null, (s, e) => { /* ChenTrichDan(tn); */ });
            menu.Items.Add("Sao ⭐", null, async (s, e) => { /* await DanhDauSao(tn); */ });
            menu.Items.Add("Ghim 📌", null, async (s, e) => { /* await GhimTinNhan(tn); */ });
            menu.Items.Add("Sao chép", null, (s, e) => { try { Clipboard.SetText(tn.noiDung ?? ""); } catch { } });
            if (laCuaToi)
                menu.Items.Add("Xoá", null, async (s, e) => { /* await XoaTinNhan(tn); */ });
            bubble.ContextMenuStrip = menu;

            row.Controls.Add(bubble);

            AlignBubbleInRow(row);
            row.SizeChanged += (ss, ee) =>
            {
                if (row.Width != flbKhungChat.ClientSize.Width)
                    row.Width = flbKhungChat.ClientSize.Width;
                AlignBubbleInRow(row);
            };
            bubble.SizeChanged += (ss, ee) => AlignBubbleInRow(row);

            flbKhungChat.Controls.Add(row);
        }

        private static void AlignBubbleInRow(Panel row)
        {
            if (row == null || row.Controls.Count == 0) return;
            Control bubble = row.Controls[0];
            bool laCuaToi = false;
            if (row.Tag is bool) laCuaToi = (bool)row.Tag;

            if (laCuaToi)
            {
                bubble.Left = Math.Max(row.Padding.Left,
                    row.ClientSize.Width - row.Padding.Right - bubble.Width);
            }
            else
            {
                bubble.Left = row.Padding.Left;
            }
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            try { streamChatHienTai?.Dispose(); } catch { }
            try { streamFriendReq?.Dispose(); } catch { }
            try { streamFriends?.Dispose(); } catch { }
            try { streamTrangThai?.Dispose(); } catch { }
            try { streamNhom?.Dispose(); } catch { }
            try { streamTyping?.Dispose(); } catch { }

            try { timerRender?.Stop(); timerRender?.Dispose(); } catch { }
            try { timerGomSuKienChat?.Stop(); timerGomSuKienChat?.Dispose(); } catch { }
            try { timerTinNhanMoi?.Stop(); timerTinNhanMoi?.Dispose(); } catch { }
            try { timerDangNhap?.Stop(); timerDangNhap?.Dispose(); } catch { }
            try { timerResize?.Stop(); timerResize?.Dispose(); } catch { }

            CapNhatTrangThai("offline").ConfigureAwait(false);
            base.OnFormClosed(e);
        }

        private void ClearRenderQueue()
        {
            TinNhan _; while (hangChoRender.TryDequeue(out _)) { }
        }

        // ===== Danh sách người dùng =====
        private async Task TaiDanhSachNguoiDung()
        {
            if (dangTaiDanhSachNguoiDung) return;
            dangTaiDanhSachNguoiDung = true;

            try
            {
                FirebaseResponse res = await firebase.GetAsync("users");
                Dictionary<string, UserFirebase> data = res.ResultAs<Dictionary<string, UserFirebase>>();

                if (InvokeRequired) Invoke(new Action(() => flpDanhSachChat.Controls.Clear()));
                else flpDanhSachChat.Controls.Clear();

                if (data == null || data.Count == 0)
                {
                    Invoke(new Action(() =>
                    {
                        flpDanhSachChat.Controls.Add(new Label() { Text = "Không có người dùng nào!", AutoSize = true });
                    }));
                    return;
                }

                foreach (KeyValuePair<string, UserFirebase> item in data)
                {
                    UserFirebase user = item.Value;
                    if (user == null) continue;
                    if (string.Equals(user.Ten, tenHienTai, StringComparison.OrdinalIgnoreCase)) continue;

                    string trangThai = "";
                    if (danhSachBanBe.ContainsKey(user.Ten))
                    {
                        string trangThaiOnline;
                        if (trangThaiNguoiDung.TryGetValue(user.Ten, out trangThaiOnline) && trangThaiOnline == "online")
                            trangThai = "(online)";
                        else
                            trangThai = "(offline)";
                    }

                    string ghiChuTrangThai = "";
                    if (danhSachBanBe.ContainsKey(user.Ten)) ghiChuTrangThai = " (Bạn bè)";
                    else if (danhSachDaGuiLoiMoi.Contains(user.Ten)) ghiChuTrangThai = " (Đã mời)";
                    else if (danhSachLoiMoiNhanDuoc.Contains(user.Ten)) ghiChuTrangThai = " (Mời bạn)";

                    Button btn = new Button
                    {
                        Text = string.Format("{0} {1}{2}", trangThai, user.Ten, ghiChuTrangThai),
                        Tag = "user:" + user.Ten,
                        Width = flpDanhSachChat.Width - 25,
                        Height = 40,
                        TextAlign = ContentAlignment.MiddleLeft,
                        BackColor = Color.WhiteSmoke,
                        FlatStyle = FlatStyle.Flat
                    };

                    ContextMenuStrip cm = new ContextMenuStrip();
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
                        flbKhungChat.Controls.Clear();
                        ClearRenderQueue();
                        tenDoiPhuong = user.Ten;
                        currentChatIsGroup = false;
                        currentGroupId = "";
                        lblTenDangNhapGiua.Text = tenDoiPhuong;
                        flbKhungChat.Controls.Clear();

                        string cid = TaoIdCuocTroChuyen(tenHienTai, tenDoiPhuong);
                        dsTinNhanDaCoTheoDoanChat[cid] = new HashSet<string>();
                        thuTuTinNhanTheoDoanChat[cid] = new List<string>();

                        await CapTinNhanMoi();
                        BatRealtimeChatHienTai();
                        BatRealtimeTyping();
                        BatTimerKiemTraTinNhanMoi();

                        RecomputeBubbleWidths(true);
                    };

                    if (InvokeRequired) Invoke(new Action(() => flpDanhSachChat.Controls.Add(btn)));
                    else flpDanhSachChat.Controls.Add(btn);
                }

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

        // ===== Nhóm =====
        private async Task TaiDanhSachNhom()
        {
            try
            {
                FirebaseResponse res = await firebase.GetAsync("nhom");
                Dictionary<string, Nhom> data = res.ResultAs<Dictionary<string, Nhom>>();

                List<Button> groupButtons = flpDanhSachChat.Controls.Cast<Control>()
                    .OfType<Button>()
                    .Where(b => b.Tag is string && ((string)b.Tag).StartsWith("group:"))
                    .ToList();
                foreach (Button b in groupButtons) flpDanhSachChat.Controls.Remove(b);

                if (data == null) return;

                foreach (KeyValuePair<string, Nhom> item in data)
                {
                    Nhom nhom = item.Value;
                    if (nhom == null || nhom.thanhVien == null) continue;
                    if (!nhom.thanhVien.ContainsKey(tenHienTai)) continue;

                    Button btn = new Button
                    {
                        Text = "[Nhóm] " + nhom.tenNhom,
                        Tag = "group:" + nhom.id,
                        Width = flpDanhSachChat.Width - 25,
                        Height = 40,
                        BackColor = Color.LightYellow,
                        FlatStyle = FlatStyle.Flat,
                        TextAlign = ContentAlignment.MiddleLeft
                    };

                    ContextMenuStrip cm = new ContextMenuStrip();
                    cm.Items.Add("Thêm thành viên", null, async (s2, e2) => { await ThemThanhVienVaoNhom(nhom.id); });

                    if (nhom.taoBoi == tenHienTai)
                    {
                        cm.Items.Add("Xóa nhóm", null, async (s2, e2) =>
                        {
                            DialogResult confirm = MessageBox.Show(
                                string.Format("Bạn có chắc muốn xóa nhóm \"{0}\" không?", nhom.tenNhom),
                                "Xóa nhóm", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                            if (confirm == DialogResult.Yes)
                                await XoaNhom(nhom.id);
                        });
                    }
                    btn.ContextMenuStrip = cm;

                    btn.Click += async (s, e) =>
                    {
                        string tag = btn.Tag as string;
                        if (!string.IsNullOrEmpty(tag) && tag.StartsWith("group:"))
                        {
                            flbKhungChat.Controls.Clear();
                            ClearRenderQueue();
                            currentChatIsGroup = true;
                            currentGroupId = tag.Substring("group:".Length);
                            tenDoiPhuong = "";
                            lblTenDangNhapGiua.Text = nhom.tenNhom;
                            flbKhungChat.Controls.Clear();

                            dsTinNhanDaCoTheoDoanChat[currentGroupId] = new HashSet<string>();
                            thuTuTinNhanTheoDoanChat[currentGroupId] = new List<string>();

                            await CapTinNhanMoiNhom(currentGroupId);
                            BatRealtimeChatNhom(currentGroupId);
                            BatRealtimeTyping();
                            BatTimerKiemTraTinNhanMoi();

                            RecomputeBubbleWidths(true);
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
                FirebaseResponse res = await firebase.GetAsync("nhom/" + idNhom);
                Nhom nhom = res.ResultAs<Nhom>();
                if (nhom == null)
                {
                    MessageBox.Show("Không tìm thấy nhóm!");
                    return;
                }

                List<string> banChuaCo = danhSachBanBe.Keys.Where(b => !nhom.thanhVien.ContainsKey(b)).ToList();

                using (var form = new ChonThanhVien(banChuaCo))
                {
                    if (form.ShowDialog() == DialogResult.OK)
                    {
                        List<string> duocChon = form.ThanhVienDuocChon;
                        if (duocChon.Count == 0)
                        {
                            MessageBox.Show("Chưa chọn ai để thêm!");
                            return;
                        }

                        foreach (string ten in duocChon)
                        {
                            nhom.thanhVien[ten] = true;
                            await firebase.SetAsync("nhom/" + idNhom + "/thanhVien/" + ten, true);
                        }

                        MessageBox.Show(string.Format("Đã thêm {0} thành viên vào nhóm!", duocChon.Count));
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
                await firebase.DeleteAsync("nhom/" + idNhom);
                await firebase.DeleteAsync("cuocTroChuyenNhom/" + idNhom);

                MessageBox.Show("Đã xóa nhóm thành công!");
                await TaiDanhSachNhom();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi xóa nhóm: " + ex.Message);
            }
        }

        // ===== Gửi tin =====
        private async void btnGui_Click(object sender, EventArgs e)
        {
            string noiDung = txtNhapTinNhan.Text.Trim();
            if (string.IsNullOrEmpty(noiDung))
            {
                MessageBox.Show("Nhập nội dung tin nhắn trước khi gửi!");
                return;
            }

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
                        nhanBoi = "",
                        noiDung = noiDung,
                        thoiGian = DateTime.UtcNow.ToString("o")
                    };

                    PushResponse push = await firebase.PushAsync("cuocTroChuyenNhom/" + currentGroupId + "/", tn);
                    tn.id = push.Result.name;
                    await firebase.SetAsync("cuocTroChuyenNhom/" + currentGroupId + "/" + tn.id, tn);

                    if (!dsTinNhanDaCoTheoDoanChat.ContainsKey(currentGroupId))
                        dsTinNhanDaCoTheoDoanChat[currentGroupId] = new HashSet<string>();
                    if (dsTinNhanDaCoTheoDoanChat[currentGroupId].Add(tn.id))
                    {
                        if (tn.noiDung == null) tn.noiDung = "";
                    }
                    hangChoRender.Enqueue(tn);

                    if (!thuTuTinNhanTheoDoanChat.ContainsKey(currentGroupId))
                        thuTuTinNhanTheoDoanChat[currentGroupId] = new List<string>();
                    thuTuTinNhanTheoDoanChat[currentGroupId].Add(tn.id);

                    txtNhapTinNhan.Clear();
                    RefocusInput();
                    await DanhDauDaXemCuoi();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi khi gửi tin nhắn nhóm: " + ex.Message);
                }
                finally
                {
                    btnGui.Enabled = true;
                    Cursor.Current = Cursors.Default;
                    RefocusInput();
                }
                return;
            }

            // 1-1
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
                    thoiGian = DateTime.UtcNow.ToString("o")
                };

                PushResponse push = await firebase.PushAsync("cuocTroChuyen/" + cid + "/", tn);
                tn.id = push.Result.name;
                await firebase.SetAsync("cuocTroChuyen/" + cid + "/" + tn.id, tn);

                if (!dsTinNhanDaCoTheoDoanChat.ContainsKey(cid))
                    dsTinNhanDaCoTheoDoanChat[cid] = new HashSet<string>();
                if (dsTinNhanDaCoTheoDoanChat[cid].Add(tn.id))
                {
                    if (tn.noiDung == null) tn.noiDung = "";
                }
                hangChoRender.Enqueue(tn);

                if (!thuTuTinNhanTheoDoanChat.ContainsKey(cid))
                    thuTuTinNhanTheoDoanChat[cid] = new List<string>();
                thuTuTinNhanTheoDoanChat[cid].Add(tn.id);

                txtNhapTinNhan.Clear();
                RefocusInput();
                await DanhDauDaXemCuoi();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi gửi tin nhắn: " + ex.Message);
            }
            finally
            {
                btnGui.Enabled = true;
                Cursor.Current = Cursors.Default;
                RefocusInput();
            }
        }

        // ===== Load & theo dõi tin =====
        private async Task CapTinNhanMoi() // 1-1
        {
            if (string.IsNullOrEmpty(tenDoiPhuong)) return;

            DateTimeOffset now = DateTimeOffset.UtcNow;
            if ((now - lanCuoiTai1_1).TotalMilliseconds < 600) return;

            if (!await khoaTaiTinNhan.WaitAsync(0)) return;

            try
            {
                string cid = TaoIdCuocTroChuyen(tenHienTai, tenDoiPhuong);
                FirebaseResponse res = await firebase.GetAsync("cuocTroChuyen/" + cid);
                Dictionary<string, TinNhan> data = res.ResultAs<Dictionary<string, TinNhan>>();

                if (!dsTinNhanDaCoTheoDoanChat.ContainsKey(cid))
                    dsTinNhanDaCoTheoDoanChat[cid] = new HashSet<string>();
                HashSet<string> dsDaCo = dsTinNhanDaCoTheoDoanChat[cid];

                if (!thuTuTinNhanTheoDoanChat.ContainsKey(cid))
                    thuTuTinNhanTheoDoanChat[cid] = new List<string>();

                if (data != null && data.Count > 0)
                {
                    foreach (KeyValuePair<string, TinNhan> kv in data.OrderBy(k => ParseTime(k.Value != null ? k.Value.thoiGian : null)))
                    {
                        TinNhan tn = kv.Value ?? new TinNhan();
                        if (string.IsNullOrEmpty(tn.id)) tn.id = kv.Key;
                        if (string.IsNullOrEmpty(tn.id)) continue;

                        if (dsDaCo.Add(tn.id))
                        {
                            if (tn.noiDung == null) tn.noiDung = "";
                            thuTuTinNhanTheoDoanChat[cid].Add(tn.id);
                            hangChoRender.Enqueue(tn);
                        }
                    }
                }

                lanCuoiTai1_1 = now;
                await DanhDauDaXemCuoi();
            }
            catch { }
            finally
            {
                try { khoaTaiTinNhan.Release(); } catch { }
            }
        }

        private async Task CapTinNhanMoiNhom(string idNhom)
        {
            if (string.IsNullOrEmpty(idNhom)) return;

            DateTimeOffset now = DateTimeOffset.UtcNow;
            if ((now - lanCuoiTaiNhom).TotalMilliseconds < 600) return;

            if (!await khoaTaiTinNhan.WaitAsync(0)) return;

            try
            {
                FirebaseResponse res = await firebase.GetAsync("cuocTroChuyenNhom/" + idNhom);
                Dictionary<string, TinNhan> data = res.ResultAs<Dictionary<string, TinNhan>>();

                if (!dsTinNhanDaCoTheoDoanChat.ContainsKey(idNhom))
                    dsTinNhanDaCoTheoDoanChat[idNhom] = new HashSet<string>();
                HashSet<string> dsDaCo = dsTinNhanDaCoTheoDoanChat[idNhom];

                if (!thuTuTinNhanTheoDoanChat.ContainsKey(idNhom))
                    thuTuTinNhanTheoDoanChat[idNhom] = new List<string>();

                if (data != null && data.Count > 0)
                {
                    foreach (KeyValuePair<string, TinNhan> kv in data.OrderBy(k => ParseTime(k.Value != null ? k.Value.thoiGian : null)))
                    {
                        TinNhan tn = kv.Value ?? new TinNhan();
                        if (string.IsNullOrEmpty(tn.id)) tn.id = kv.Key;
                        if (string.IsNullOrEmpty(tn.id)) continue;

                        if (dsDaCo.Add(tn.id))
                        {
                            if (tn.noiDung == null) tn.noiDung = "";
                            thuTuTinNhanTheoDoanChat[idNhom].Add(tn.id);
                            hangChoRender.Enqueue(tn);
                        }
                    }
                }

                lanCuoiTaiNhom = now;
                await DanhDauDaXemCuoi();
            }
            catch { }
            finally
            {
                try { khoaTaiTinNhan.Release(); } catch { }
            }
        }

        private void BatTimerKiemTraTinNhanMoi()
        {
            try { timerTinNhanMoi?.Stop(); timerTinNhanMoi?.Dispose(); } catch { }

            timerTinNhanMoi = new System.Windows.Forms.Timer();
            timerTinNhanMoi.Interval = 10000;
            timerTinNhanMoi.Tick += async (s, e) =>
            {
                if (streamChatHienTai == null)
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
                }
            };
            timerTinNhanMoi.Start();
        }

        private async void BatRealtimeChatHienTai()
        {
            try { streamChatHienTai?.Dispose(); } catch { }
            if (string.IsNullOrEmpty(tenDoiPhuong)) return;

            string cid = TaoIdCuocTroChuyen(tenHienTai, tenDoiPhuong);

            streamChatHienTai = await firebase.OnAsync(
                "cuocTroChuyen/" + cid,
                added: async (s, a, c) => await UiCapNhatTinNhan(),
                changed: async (s, a, c) => await UiCapNhatTinNhan(),
                removed: async (s, a, c) => await UiCapNhatTinNhan()
            );
        }

        private async void BatRealtimeChatNhom(string idNhom)
        {
            try { streamChatHienTai?.Dispose(); } catch { }
            if (string.IsNullOrEmpty(idNhom)) return;

            streamChatHienTai = await firebase.OnAsync(
                "cuocTroChuyenNhom/" + idNhom,
                added: async (s, a, c) => await UiCapNhatTinNhan(),
                changed: async (s, a, c) => await UiCapNhatTinNhan(),
                removed: async (s, a, c) => await UiCapNhatTinNhan()
            );
        }

        private Task UiCapNhatTinNhan()
        {
            if (!IsHandleCreated || this.IsDisposed) return Task.CompletedTask;

            if (timerGomSuKienChat == null)
            {
                timerGomSuKienChat = new System.Windows.Forms.Timer();
                timerGomSuKienChat.Interval = 300;
                timerGomSuKienChat.Tick += async (s, e) =>
                {
                    timerGomSuKienChat.Stop();
                    try
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
                    }
                    catch { }
                };
            }

            timerGomSuKienChat.Stop();
            timerGomSuKienChat.Start();
            return Task.CompletedTask;
        }

        // ===== Phụ =====
        private string TaoIdCuocTroChuyen(string u1, string u2)
        {
            return string.CompareOrdinal(u1, u2) < 0 ? (u1 + "__" + u2) : (u2 + "__" + u1);
        }

        private async Task DanhDauSao(TinNhan tn)
        {
            try
            {
                string key = currentChatIsGroup ? currentGroupId : TaoIdCuocTroChuyen(tenHienTai, tenDoiPhuong);
                await firebase.SetAsync("stars/" + tenHienTai + "/" + key + "/" + tn.id, true);
                MessageBox.Show("Đã đánh dấu sao ⭐");
            }
            catch (Exception ex) { MessageBox.Show("Lỗi đánh dấu sao: " + ex.Message); }
        }

        private async Task GhimTinNhan(TinNhan tn)
        {
            try
            {
                string key = currentChatIsGroup ? currentGroupId : TaoIdCuocTroChuyen(tenHienTai, tenDoiPhuong);
                await firebase.SetAsync("pins/" + key + "/" + tn.id, true);
                MessageBox.Show("Đã ghim 📌");
            }
            catch (Exception ex) { MessageBox.Show("Lỗi ghim: " + ex.Message); }
        }

        private async Task XoaTinNhan(TinNhan tn)
        {
            try
            {
                if (currentChatIsGroup)
                    await firebase.DeleteAsync("cuocTroChuyenNhom/" + currentGroupId + "/" + tn.id);
                else
                {
                    string cid = TaoIdCuocTroChuyen(tenHienTai, tenDoiPhuong);
                    await firebase.DeleteAsync("cuocTroChuyen/" + cid + "/" + tn.id);
                }
            }
            catch (Exception ex) { MessageBox.Show("Lỗi xoá tin: " + ex.Message); }
        }

        // ===== Kết bạn / Trạng thái =====
        private async Task NapTrangThaiKetBan()
        {
            danhSachBanBe.Clear();
            danhSachDaGuiLoiMoi.Clear();
            danhSachLoiMoiNhanDuoc.Clear();

            FirebaseResponse f1 = await firebase.GetAsync("friends/" + tenHienTai);
            Dictionary<string, bool> duLieuBanBe = f1.ResultAs<Dictionary<string, bool>>();
            if (duLieuBanBe != null) danhSachBanBe = duLieuBanBe;

            FirebaseResponse tatCaLoiMoi = await firebase.GetAsync("friendRequests/pending");
            Dictionary<string, Dictionary<string, bool>> duLieuLoiMoiCho = tatCaLoiMoi.ResultAs<Dictionary<string, Dictionary<string, bool>>>();
            if (duLieuLoiMoiCho != null)
            {
                foreach (KeyValuePair<string, Dictionary<string, bool>> loiMoiDenNguoiNhan in duLieuLoiMoiCho)
                {
                    string nguoiNhan = loiMoiDenNguoiNhan.Key;
                    foreach (KeyValuePair<string, bool> loiMoiTuNguoiGui in loiMoiDenNguoiNhan.Value)
                    {
                        string nguoiGui = loiMoiTuNguoiGui.Key;
                        if (nguoiGui == tenHienTai) danhSachDaGuiLoiMoi.Add(nguoiNhan);
                    }
                }
            }

            FirebaseResponse f2 = await firebase.GetAsync("friendRequests/pending/" + tenHienTai);
            Dictionary<string, bool> danhSachNguoiGuiLoiMoi = f2.ResultAs<Dictionary<string, bool>>();
            if (danhSachNguoiGuiLoiMoi != null)
                danhSachLoiMoiNhanDuoc = new HashSet<string>(danhSachNguoiGuiLoiMoi.Keys);
        }

        private async Task GuiLoiMoiKetBan(string ten)
        {
            await firebase.SetAsync("friendRequests/pending/" + ten + "/" + tenHienTai, true);
            MessageBox.Show("Đã gửi lời mời kết bạn đến " + ten + "!");
            danhSachDaGuiLoiMoi.Add(ten);
        }

        private async Task ChapNhanKetBan(string ten)
        {
            await firebase.SetAsync("friends/" + tenHienTai + "/" + ten, true);
            await firebase.SetAsync("friends/" + ten + "/" + tenHienTai, true);
            await firebase.DeleteAsync("friendRequests/pending/" + tenHienTai + "/" + ten);

            MessageBox.Show("Bạn và " + ten + " đã trở thành bạn bè!");
            danhSachBanBe[ten] = true;
            danhSachLoiMoiNhanDuoc.Remove(ten);
        }

        private async Task HuyKetBan(string ten)
        {
            await firebase.DeleteAsync("friends/" + tenHienTai + "/" + ten);
            await firebase.DeleteAsync("friends/" + ten + "/" + tenHienTai);
            danhSachBanBe.Remove(ten);
            MessageBox.Show("Đã hủy kết bạn với " + ten + ".");
        }

        private void BatRealtimeKetBan()
        {
            try { streamFriends?.Dispose(); } catch { }
            try { streamFriendReq?.Dispose(); } catch { }

            firebase.OnAsync("friends/" + tenHienTai,
                added: async (s, a, c) => await RefreshBanBe(),
                changed: async (s, a, c) => await RefreshBanBe(),
                removed: async (s, a, c) => await RefreshBanBe()
            ).ContinueWith(t =>
            {
                if (t.Status == TaskStatus.RanToCompletion) streamFriends = t.Result;
            });

            firebase.OnAsync("friendRequests/pending/" + tenHienTai,
                added: (s, a, c) =>
                {
                    BeginInvoke(new Action(async () =>
                    {
                        await NapTrangThaiKetBan();
                        string nguoiGui = a.Path != null ? a.Path.TrimStart('/') : "";
                        if (!string.IsNullOrEmpty(nguoiGui))
                        {
                            MessageBox.Show("📩 Bạn có lời mời kết bạn mới từ " + nguoiGui + "!", "Kết bạn mới");
                        }
                        await TaiDanhSachNguoiDung();
                    }));
                },
                removed: (s, a, c) =>
                {
                    BeginInvoke(new Action(async () =>
                    {
                        await NapTrangThaiKetBan();
                        await TaiDanhSachNguoiDung();
                    }));
                }
            ).ContinueWith(t =>
            {
                if (t.Status == TaskStatus.RanToCompletion) streamFriendReq = t.Result;
            });
        }

        private async Task RefreshBanBe()
        {
            try
            {
                await NapTrangThaiKetBan();
                if (InvokeRequired) BeginInvoke(new Action(async () => await TaiDanhSachNguoiDung()));
                else await TaiDanhSachNguoiDung();
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
                if (t.Status == TaskStatus.RanToCompletion) streamNhom = t.Result;
            });
        }

        private void XuLyCapNhatNhom()
        {
            BeginInvoke(new Action(async () => { await TaiDanhSachNhom(); }));
        }

        // Trạng thái online/offline
        private async Task CapNhatTrangThai(string trangThai)
        {
            if (firebase == null || string.IsNullOrEmpty(tenHienTai)) return;
            try
            {
                string key = tenHienTai.Replace(".", "_").Replace("$", "_");
                await firebase.SetAsync("status/" + key, trangThai);
            }
            catch { }
        }

        private void BatRealtimeTrangThai()
        {
            try { streamTrangThai?.Dispose(); } catch { }

            firebase.OnAsync("status",
                added: (s, a, c) => CapNhatTrangThaiUI(),
                changed: (s, a, c) => CapNhatTrangThaiUI(),
                removed: (s, a, c) => CapNhatTrangThaiUI()
            ).ContinueWith(t =>
            {
                if (t.Status == TaskStatus.RanToCompletion) streamTrangThai = t.Result;
            });
        }

        private void CapNhatTrangThaiUI()
        {
            if (!this.IsDisposed && this.IsHandleCreated)
                BeginInvoke(new Action(async () =>
                {
                    try
                    {
                        FirebaseResponse res = await firebase.GetAsync("status");
                        Dictionary<string, string> data = res.ResultAs<Dictionary<string, string>>();
                        trangThaiNguoiDung = (data != null) ? data : new Dictionary<string, string>();
                        await TaiDanhSachNguoiDung();
                    }
                    catch { }
                }));
        }

        // ===== Typing =====
        private void KhoiTaoTyping()
        {
            if (timerDangNhap != null) return;

            timerDangNhap = new System.Windows.Forms.Timer { Interval = 700 };
            timerDangNhap.Tick += async (s, e) =>
            {
                timerDangNhap.Stop();
                if (string.IsNullOrWhiteSpace(txtNhapTinNhan.Text)) return;
                await GuiTrangThaiDangNhap();
            };

            txtNhapTinNhan.TextChanged += (s, e2) =>
            {
                timerDangNhap.Stop();
                timerDangNhap.Start();
            };
        }

        private async Task GuiTrangThaiDangNhap()
        {
            string key = currentChatIsGroup ? currentGroupId
                : (!string.IsNullOrEmpty(tenDoiPhuong) ? TaoIdCuocTroChuyen(tenHienTai, tenDoiPhuong) : "");
            if (string.IsNullOrEmpty(key)) return;

            hetHanTyping = DateTimeOffset.UtcNow.AddSeconds(4).ToUnixTimeMilliseconds();
            await firebase.SetAsync("typing/" + key + "/" + tenHienTai, new { until = hetHanTyping });
        }

        private void BatRealtimeTyping()
        {
            try { streamTyping?.Dispose(); } catch { }

            string key = currentChatIsGroup ? currentGroupId
                : (!string.IsNullOrEmpty(tenDoiPhuong) ? TaoIdCuocTroChuyen(tenHienTai, tenDoiPhuong) : "");
            if (string.IsNullOrEmpty(key)) return;

            firebase.OnAsync("typing/" + key,
                added: (s, a, c) => CapNhatTypingUI(),
                changed: (s, a, c) => CapNhatTypingUI(),
                removed: (s, a, c) => CapNhatTypingUI()
            ).ContinueWith(t =>
            {
                if (t.Status == TaskStatus.RanToCompletion) streamTyping = t.Result;
            });
        }

        private async void CapNhatTypingUI()
        {
            try
            {
                string key = currentChatIsGroup ? currentGroupId
                    : (!string.IsNullOrEmpty(tenDoiPhuong) ? TaoIdCuocTroChuyen(tenHienTai, tenDoiPhuong) : "");
                if (string.IsNullOrEmpty(key)) return;

                FirebaseResponse res = await firebase.GetAsync("typing/" + key);
                Dictionary<string, Dictionary<string, long>> data = res.ResultAs<Dictionary<string, Dictionary<string, long>>>();
                long now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

                bool coNguoiDangNhap = false;
                string tenNguoi = "";
                if (data != null)
                {
                    foreach (KeyValuePair<string, Dictionary<string, long>> kv in data)
                    {
                        string ten = kv.Key;
                        if (string.Equals(ten, tenHienTai, StringComparison.OrdinalIgnoreCase)) continue;

                        long until;
                        if (kv.Value != null && kv.Value.TryGetValue("until", out until) && until > now)
                        {
                            coNguoiDangNhap = true;
                            tenNguoi = ten;
                            break;
                        }
                    }
                }

                if (!this.IsDisposed && this.IsHandleCreated)
                {
                    BeginInvoke(new Action(() =>
                    {
                        lblTyping.Visible = coNguoiDangNhap;
                        lblTyping.Text = coNguoiDangNhap
                            ? (currentChatIsGroup ? (tenNguoi + " đang nhập...") : "Đang nhập...")
                            : "";
                    }));
                }
            }
            catch { }
        }

        // ===== Read receipts =====
        private async Task DanhDauDaXemCuoi()
        {
            try
            {
                string key = currentChatIsGroup ? currentGroupId
                    : (!string.IsNullOrEmpty(tenDoiPhuong) ? TaoIdCuocTroChuyen(tenHienTai, tenDoiPhuong) : "");
                if (string.IsNullOrEmpty(key)) return;

                List<string> list;
                if (!thuTuTinNhanTheoDoanChat.TryGetValue(key, out list) || list.Count == 0) return;

                string msgIdCuoi = list[list.Count - 1];
                long nowMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

                if (currentChatIsGroup)
                    await firebase.SetAsync("cuocTroChuyenNhom/" + key + "/" + msgIdCuoi + "/reads/" + tenHienTai, nowMs);
                else
                {
                    string cid = TaoIdCuocTroChuyen(tenHienTai, tenDoiPhuong);
                    await firebase.SetAsync("cuocTroChuyen/" + cid + "/" + msgIdCuoi + "/reads/" + tenHienTai, nowMs);
                }
            }
            catch { }
        }
        private void RefocusInput()
        {
            if (this.IsDisposed || !this.IsHandleCreated) return;
            try
            {
                // Đảm bảo gọi trên UI thread
                BeginInvoke(new Action(() =>
                {
                    try
                    {
                        txtNhapTinNhan.Focus();
                        // Đặt caret về cuối (sau khi Clear là 0)
                        txtNhapTinNhan.SelectionStart = txtNhapTinNhan.TextLength;
                        txtNhapTinNhan.SelectionLength = 0;
                    }
                    catch { /* an toàn */ }
                }));
            }
            catch { /* an toàn */ }
        }

    }

    // ===== Models =====
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
