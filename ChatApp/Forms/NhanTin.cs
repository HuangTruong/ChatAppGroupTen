using ChatApp.Helpers;
using FireSharp.Config;
using FireSharp.EventStreaming;
using FireSharp.Interfaces;
using FireSharp.Response;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ChatApp
{
    public partial class NhanTin : Form
    {
        // Hàng đợi chứa tin nhắn cần vẽ ra giao diện
        private readonly ConcurrentQueue<TinNhan> hangChoRender = new ConcurrentQueue<TinNhan>();

        // Timer dùng để định kỳ vẽ các tin trong hàng đợi
        private System.Windows.Forms.Timer timerRender;

        // Khóa tránh việc tải/đọc tin nhắn đồng thời
        private readonly SemaphoreSlim khoaTaiTinNhan = new SemaphoreSlim(1, 1);

        // Timer gom nhiều sự kiện realtime rồi mới tải tin (giảm số lần gọi)
        private System.Windows.Forms.Timer timerGomSuKienChat;

        // Thời điểm cuối cùng tải tin nhắn 1-1 và nhóm 
        private DateTimeOffset lanCuoiTai1_1 = DateTimeOffset.MinValue;
        private DateTimeOffset lanCuoiTaiNhom = DateTimeOffset.MinValue;

        // Kết nối Firebase
        private IFirebaseClient firebase;

        // Tên user hiện tại và người đang trò chuyện 1-1
        private string tenHienTai;
        private string tenDoiPhuong = string.Empty;

        // Trạng thái đang mở chat nhóm hay không
        private bool currentChatIsGroup = false;
        private string currentGroupId = string.Empty;

        // Lưu các id tin nhắn đã có cho từng cuộc trò chuyện (tránh trùng)
        private Dictionary<string, HashSet<string>> dsTinNhanDaCoTheoDoanChat = new Dictionary<string, HashSet<string>>();

        // Lưu thứ tự id tin nhắn theo từng cuộc trò chuyện (dùng cho đánh dấu đã xem)
        private Dictionary<string, List<string>> thuTuTinNhanTheoDoanChat = new Dictionary<string, List<string>>();

        // Timer dự phòng: nếu mất stream realtime thì định kỳ kiểm tra tin mới
        private System.Windows.Forms.Timer timerTinNhanMoi;

        // Các stream realtime
        private EventStreamResponse streamChatHienTai;
        private EventStreamResponse streamFriendReq;
        private EventStreamResponse streamFriends;
        private EventStreamResponse streamNhom;
        private EventStreamResponse streamTyping;
        private EventStreamResponse streamTrangThai;

        // Hỗ trợ hiển thị "đang nhập..."
        private System.Windows.Forms.Timer timerDangNhap;
        private long hetHanTyping = 0;
        private Label lblTyping;

        // Dữ liệu bạn bè và lời mời kết bạn
        private Dictionary<string, bool> danhSachBanBe = new Dictionary<string, bool>();
        private HashSet<string> danhSachDaGuiLoiMoi = new HashSet<string>();
        private HashSet<string> danhSachLoiMoiNhanDuoc = new HashSet<string>();

        // Tránh load danh sách người dùng nhiều lần cùng lúc
        private bool dangTaiDanhSachNguoiDung = false;

        // Trạng thái online/offline của người dùng
        private Dictionary<string, string> trangThaiNguoiDung = new Dictionary<string, string>();

        // Timer debounce khi thay đổi kích thước khung chat
        private System.Windows.Forms.Timer timerResize;

        // Giới hạn số lượng bong bóng tin nhắn trên màn hình
        private const int MAX_BUBBLES = 300;

        public NhanTin(string tenDangNhap)
        {
            InitializeComponent();

            // Tăng giới hạn kết nối HTTP song song
            ServicePointManager.DefaultConnectionLimit = Math.Max(100, ServicePointManager.DefaultConnectionLimit);

            // Cấu hình khung chat chính
            flbKhungChat.FlowDirection = FlowDirection.TopDown; // Tin từ trên xuống
            flbKhungChat.WrapContents = false;                  // Không quấn sang cột khác
            flbKhungChat.AutoScroll = true;                     // Có thanh cuộn khi nhiều tin
            flbKhungChat.Padding = new Padding(0);

            // Lưu tên người dùng hiện tại
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

            // Label hiển thị trạng thái "Đang nhập..."
            lblTyping = new Label();
            lblTyping.AutoSize = true;
            lblTyping.ForeColor = Color.DimGray;
            lblTyping.Text = string.Empty;
            lblTyping.Visible = false;
            lblTyping.Location = new Point(lblTenDangNhapGiua.Left, lblTenDangNhapGiua.Bottom + 4);
            this.Controls.Add(lblTyping);

            // Thiết lập phím Enter để gửi tin (Shift/Ctrl+Enter để xuống dòng)
            this.KeyPreview = true;
            this.AcceptButton = btnGui;
            txtNhapTinNhan.KeyDown += TxtNhapTinNhan_KeyDown;

            // Khởi tạo timer debounce resize
            timerResize = new System.Windows.Forms.Timer();
            timerResize.Interval = 120;
            timerResize.Tick += TimerResize_Tick;
            flbKhungChat.Resize += FlbKhungChat_Resize;
        }

        private async void NhanTin_Load(object sender, EventArgs e)
        {
            // Hiển thị tên người dùng bên phải
            lblTenDangNhapPhai.Text = tenHienTai;

            // Cập nhật trạng thái online và lắng nghe realtime trạng thái
            await CapNhatTrangThai("online");
            BatRealtimeTrangThai();

            // Nạp trạng thái kết bạn và bật realtime bạn bè
            await NapTrangThaiKetBan();
            BatRealtimeKetBan();

            // Tải danh sách người dùng
            await TaiDanhSachNguoiDung();

            // Tải danh sách nhóm và bật realtime nhóm
            await TaiDanhSachNhom();
            BatRealtimeNhom();

            // Khởi tạo xử lý "đang nhập..."
            KhoiTaoTyping();

            // Khởi tạo cơ chế render batch tin nhắn
            KhoiTaoRenderMem();

            // Tính lại độ rộng bong bóng chat ban đầu
            RecomputeBubbleWidths(true);
        }

        private void TxtNhapTinNhan_KeyDown(object sender, KeyEventArgs e)
        {
            // Enter: gửi; Shift+Enter hoặc Ctrl+Enter: xuống dòng
            ChatInputHandler.HandleKeyDown(e, btnGui);
        }

        private void TimerResize_Tick(object sender, EventArgs e)
        {
            // Khi người dùng dừng resize một lúc thì mới tính lại layout
            timerResize.Stop();
            RecomputeBubbleWidths(true);
        }

        private void FlbKhungChat_Resize(object sender, EventArgs e)
        {
            // Khi resize khung chat: khởi động debounce
            timerResize.Stop();
            timerResize.Start();
        }

        private void KhoiTaoRenderMem()
        {
            // Bật DoubleBuffered cho panel để vẽ mượt hơn, giảm nháy
            PropertyInfo prop = typeof(Panel).GetProperty(
                "DoubleBuffered",
                BindingFlags.Instance | BindingFlags.NonPublic);

            if (prop != null)
            {
                prop.SetValue(flbKhungChat, true, null);
            }

            // Timer định kỳ lấy tin trong hàng đợi ra vẽ
            timerRender = new System.Windows.Forms.Timer();
            timerRender.Interval = 80; // 80ms: gộp các tin lại, vẽ một lần
            timerRender.Tick += TimerRender_Tick;
            timerRender.Start();
        }

        private void TimerRender_Tick(object sender, EventArgs e)
        {
            FlushRender();
        }

        private int GetMaxBubbleWidth()
        {
            // Tính độ rộng tối đa của bong bóng dựa theo chiều rộng khung chat
            int scrollWidth = 0;

            if (flbKhungChat.VerticalScroll.Visible)
            {
                scrollWidth = SystemInformation.VerticalScrollBarWidth;
            }

            int maxWidth = flbKhungChat.ClientSize.Width - (100 + scrollWidth);

            if (maxWidth < 220)
            {
                maxWidth = 220;
            }

            return maxWidth;
        }

        private static DateTime ParseTime(string s)
        {
            // Hàm parse chuỗi thời gian từ nhiều dạng khác nhau về UTC
            if (string.IsNullOrWhiteSpace(s))
            {
                return DateTime.UtcNow;
            }

            long ms;

            if (long.TryParse(s, out ms) &&
                ms > 946684800000L &&
                ms < 4102444800000L)
            {
                try
                {
                    return DateTimeOffset.FromUnixTimeMilliseconds(ms).UtcDateTime;
                }
                catch
                {
                }
            }

            DateTimeOffset dtoExact;

            if (DateTimeOffset.TryParseExact(
                    s,
                    "o",
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
                    out dtoExact))
            {
                return dtoExact.UtcDateTime;
            }

            DateTimeOffset dto;

            if (DateTimeOffset.TryParse(
                    s,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
                    out dto))
            {
                return dto.UtcDateTime;
            }

            DateTime dt;

            if (DateTime.TryParse(
                    s,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
                    out dt))
            {
                return dt;
            }

            return DateTime.UtcNow;
        }

        private void RecomputeBubbleWidths(bool realign)
        {
            // Cập nhật lại độ rộng và căn lề của tất cả bong bóng khi khung chat đổi kích thước
            if (!IsHandleCreated || IsDisposed)
            {
                return;
            }

            int panelWidth = flbKhungChat.ClientSize.Width;
            int textMaxWidth = GetMaxBubbleWidth();

            flbKhungChat.SuspendLayout();

            foreach (Control ctrl in flbKhungChat.Controls)
            {
                Panel row = ctrl as Panel;

                if (row == null)
                {
                    continue;
                }

                if (row.Width != panelWidth)
                {
                    row.Width = panelWidth;
                }

                if (row.Controls.Count > 0)
                {
                    Panel bubble = row.Controls[0] as Panel;

                    if (bubble != null)
                    {
                        FlowLayoutPanel contentPanel = null;

                        if (bubble.Controls.Count > 0)
                        {
                            contentPanel = bubble.Controls[0] as FlowLayoutPanel;
                        }

                        if (contentPanel != null)
                        {
                            foreach (Control c in contentPanel.Controls)
                            {
                                Label lbl = c as Label;

                                if (lbl != null && lbl.Name == "lblMsg")
                                {
                                    int cap = textMaxWidth - bubble.Padding.Horizontal;

                                    if (cap < 50)
                                    {
                                        cap = 50;
                                    }

                                    if (lbl.MaximumSize.Width != cap)
                                    {
                                        lbl.MaximumSize = new Size(cap, 0);
                                    }
                                }
                            }
                        }

                        if (realign)
                        {
                            AlignBubbleInRow(row);
                        }
                    }
                }
            }

            flbKhungChat.ResumeLayout(true);
        }

        private void FlushRender()
        {
            // Lấy tin từ hàng đợi và vẽ ra khung chat theo lô
            if (!IsHandleCreated || IsDisposed)
            {
                return;
            }

            if (hangChoRender.IsEmpty)
            {
                return;
            }

            List<TinNhan> batch = new List<TinNhan>(50);
            TinNhan temp;

            while (batch.Count < 50 && hangChoRender.TryDequeue(out temp))
            {
                batch.Add(temp);
            }

            if (batch.Count == 0)
            {
                return;
            }

            // Kiểm tra có đang ở cuối khung chat hay không
            bool oCuoiKhungChat;

            if (flbKhungChat.VerticalScroll.Visible)
            {
                int maxValue = Math.Max(
                    0,
                    flbKhungChat.VerticalScroll.Maximum - flbKhungChat.VerticalScroll.LargeChange);

                oCuoiKhungChat = flbKhungChat.VerticalScroll.Value >= maxValue;
            }
            else
            {
                oCuoiKhungChat = true;
            }

            flbKhungChat.SuspendLayout();

            bool oldAutoScroll = flbKhungChat.AutoScroll;
            flbKhungChat.AutoScroll = false;

            // Tạo bong bóng cho từng tin
            foreach (TinNhan tn in batch)
            {
                TaoBubbleVaChen(tn);
            }

            // Nếu vượt quá số bong bóng cho phép thì xoá bớt tin cũ
            int over = flbKhungChat.Controls.Count - MAX_BUBBLES;

            if (over > 0)
            {
                for (int i = 0; i < over; i++)
                {
                    Control control = flbKhungChat.Controls[i];

                    if (control != null)
                    {
                        control.Dispose();
                    }
                }

                for (int i = 0; i < over; i++)
                {
                    flbKhungChat.Controls.RemoveAt(0);
                }
            }

            flbKhungChat.AutoScroll = oldAutoScroll;
            flbKhungChat.ResumeLayout(true);

            // Nếu trước đó đang ở cuối, sau khi thêm tin mới thì cuộn xuống cuối
            if (oCuoiKhungChat && flbKhungChat.Controls.Count > 0)
            {
                Control last = flbKhungChat.Controls[flbKhungChat.Controls.Count - 1];
                flbKhungChat.ScrollControlIntoView(last);
            }
        }

        private void TaoBubbleVaChen(TinNhan tn)
        {
            // Tạo UI cho một tin nhắn (bong bóng + thời gian + người gửi nếu nhóm)
            bool laNhom = currentChatIsGroup;
            bool laCuaToi = string.Equals(
                tn.guiBoi,
                tenHienTai,
                StringComparison.OrdinalIgnoreCase);

            // Panel bao ngoài để canh trái/phải
            Panel row = new Panel();
            row.AutoSize = true;
            row.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            row.Margin = new Padding(0, 2, 0, 2);
            row.Width = flbKhungChat.ClientSize.Width;
            row.Tag = laCuaToi;

            if (laCuaToi)
            {
                row.Padding = new Padding(60, 2, 8, 8);
            }
            else
            {
                row.Padding = new Padding(8, 2, 60, 8);
            }

            // Panel bong bóng tin nhắn
            Panel bubble = new Panel();
            bubble.AutoSize = true;
            bubble.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            bubble.Padding = new Padding(10, 6, 10, 6);
            bubble.BorderStyle = BorderStyle.None;

            if (laCuaToi)
            {
                bubble.BackColor = Color.FromArgb(222, 242, 255);
            }
            else
            {
                bubble.BackColor = Color.White;
            }

            // Panel xếp dọc: (tên nếu nhóm) + nội dung + thời gian
            FlowLayoutPanel stack = new FlowLayoutPanel();
            stack.FlowDirection = FlowDirection.TopDown;
            stack.WrapContents = false;
            stack.AutoSize = true;
            stack.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            stack.Margin = Padding.Empty;
            stack.Padding = Padding.Empty;

            // Nếu là nhóm: hiện tên người gửi
            if (laNhom)
            {
                Label lblSender = new Label();
                lblSender.AutoSize = true;
                lblSender.Text = tn.guiBoi ?? string.Empty;
                lblSender.Font = new Font("Segoe UI", 8.5f, FontStyle.Bold);
                lblSender.ForeColor = Color.DimGray;
                lblSender.Margin = new Padding(0, 0, 0, 2);
                lblSender.Name = "lblSender";
                stack.Controls.Add(lblSender);
            }

            // Nội dung tin nhắn
            string text = tn.noiDung ?? string.Empty;

            Label lblMsg = new Label();
            lblMsg.AutoSize = true;
            lblMsg.Text = text.Length == 0 ? " " : text;
            lblMsg.Font = new Font("Segoe UI", 10f, FontStyle.Regular);
            lblMsg.ForeColor = Color.Black;
            lblMsg.Margin = new Padding(0, 0, 0, 4);
            lblMsg.Name = "lblMsg";
            lblMsg.UseMnemonic = false;
            lblMsg.AutoEllipsis = false;

            int maxWidth = GetMaxBubbleWidth() - bubble.Padding.Horizontal;

            if (maxWidth < 50)
            {
                maxWidth = 50;
            }

            lblMsg.MaximumSize = new Size(maxWidth, 0);

            // Thời gian gửi
            Label lblTime = new Label();
            lblTime.AutoSize = true;
            lblTime.Text = ParseTime(tn.thoiGian)
                .ToLocalTime()
                .ToString("HH:mm dd/MM/yyyy");
            lblTime.Font = new Font("Segoe UI", 8.5f, FontStyle.Italic);
            lblTime.ForeColor = Color.DimGray;
            lblTime.Margin = new Padding(0, 0, 0, 0);
            lblTime.Name = "lblTime";

            // Thêm các control vào stack, bubble, row
            stack.Controls.Add(lblMsg);
            stack.Controls.Add(lblTime);
            bubble.Controls.Add(stack);

            // Menu chuột phải trên bong bóng
            ContextMenuStrip menu = new ContextMenuStrip();

            menu.Items.Add(
                "Trả lời (trích dẫn)",
                null,
                delegate { /* Chưa triển khai */ });

            menu.Items.Add(
                "Sao ⭐",
                null,
                async delegate { /* await DanhDauSao(tn); */ });

            menu.Items.Add(
                "Ghim 📌",
                null,
                async delegate { /* await GhimTinNhan(tn); */ });

            menu.Items.Add(
                "Sao chép",
                null,
                delegate
                {
                    try
                    {
                        Clipboard.SetText(tn.noiDung ?? string.Empty);
                    }
                    catch
                    {
                    }
                });

            if (laCuaToi)
            {
                menu.Items.Add(
                    "Xoá",
                    null,
                    async delegate { /* await XoaTinNhan(tn); */ });
            }

            bubble.ContextMenuStrip = menu;

            row.Controls.Add(bubble);

            // Căn trái/phải bong bóng
            AlignBubbleInRow(row);

            // Khi row đổi kích thước: giữ full width và căn lại
            row.SizeChanged +=
                delegate
                {
                    if (row.Width != flbKhungChat.ClientSize.Width)
                    {
                        row.Width = flbKhungChat.ClientSize.Width;
                    }

                    AlignBubbleInRow(row);
                };

            // Khi bubble đổi kích thước: căn lại
            bubble.SizeChanged +=
                delegate
                {
                    AlignBubbleInRow(row);
                };

            // Thêm row vào khung chat
            flbKhungChat.Controls.Add(row);
        }

        private static void AlignBubbleInRow(Panel row)
        {
            // Căn bong bóng sang trái hoặc phải trong một dòng
            if (row == null || row.Controls.Count == 0)
            {
                return;
            }

            Control bubble = row.Controls[0];
            bool laCuaToi = false;

            if (row.Tag is bool)
            {
                laCuaToi = (bool)row.Tag;
            }

            if (laCuaToi)
            {
                int x = row.ClientSize.Width - row.Padding.Right - bubble.Width;

                if (x < row.Padding.Left)
                {
                    x = row.Padding.Left;
                }

                bubble.Left = x;
            }
            else
            {
                bubble.Left = row.Padding.Left;
            }
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            // Dọn các stream realtime
            try { if (streamChatHienTai != null) streamChatHienTai.Dispose(); } catch { }
            try { if (streamFriendReq != null) streamFriendReq.Dispose(); } catch { }
            try { if (streamFriends != null) streamFriends.Dispose(); } catch { }
            try { if (streamTrangThai != null) streamTrangThai.Dispose(); } catch { }
            try { if (streamNhom != null) streamNhom.Dispose(); } catch { }
            try { if (streamTyping != null) streamTyping.Dispose(); } catch { }

            // Dừng các timer
            try { if (timerRender != null) { timerRender.Stop(); timerRender.Dispose(); } } catch { }
            try { if (timerGomSuKienChat != null) { timerGomSuKienChat.Stop(); timerGomSuKienChat.Dispose(); } } catch { }
            try { if (timerTinNhanMoi != null) { timerTinNhanMoi.Stop(); timerTinNhanMoi.Dispose(); } } catch { }
            try { if (timerDangNhap != null) { timerDangNhap.Stop(); timerDangNhap.Dispose(); } } catch { }
            try { if (timerResize != null) { timerResize.Stop(); timerResize.Dispose(); } } catch { }

            // Cập nhật trạng thái offline (không chờ)
            CapNhatTrangThai("offline").ConfigureAwait(false);

            base.OnFormClosed(e);
        }

        private void ClearRenderQueue()
        {
            // Xoá hết các tin còn trong hàng đợi render
            TinNhan boQua;

            while (hangChoRender.TryDequeue(out boQua))
            {
            }
        }

        private async Task TaiDanhSachNguoiDung()
        {
            // Tải danh sách người dùng và hiển thị ở panel bên trái
            if (dangTaiDanhSachNguoiDung)
            {
                return;
            }

            dangTaiDanhSachNguoiDung = true;

            try
            {
                FirebaseResponse res = await firebase.GetAsync("users");
                Dictionary<string, UserFirebase> data =
                    res.ResultAs<Dictionary<string, UserFirebase>>();

                // Xoá danh sách hiện tại
                if (InvokeRequired)
                {
                    Invoke(new Action(delegate { flpDanhSachChat.Controls.Clear(); }));
                }
                else
                {
                    flpDanhSachChat.Controls.Clear();
                }

                if (data == null || data.Count == 0)
                {
                    // Nếu không có user nào
                    Action addNoUser = delegate
                    {
                        Label lbl = new Label();
                        lbl.Text = "Không có người dùng nào!";
                        lbl.AutoSize = true;
                        flpDanhSachChat.Controls.Add(lbl);
                    };

                    if (InvokeRequired)
                    {
                        Invoke(addNoUser);
                    }
                    else
                    {
                        addNoUser();
                    }

                    return;
                }

                // Tạo nút cho từng user
                foreach (KeyValuePair<string, UserFirebase> item in data)
                {
                    UserFirebase user = item.Value;

                    if (user == null)
                    {
                        continue;
                    }

                    if (string.Equals(user.Ten, tenHienTai, StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    string trangThai = string.Empty;

                    if (danhSachBanBe.ContainsKey(user.Ten))
                    {
                        string trangThaiOnline;

                        if (trangThaiNguoiDung.TryGetValue(user.Ten, out trangThaiOnline) &&
                            trangThaiOnline == "online")
                        {
                            trangThai = "(online)";
                        }
                        else
                        {
                            trangThai = "(offline)";
                        }
                    }

                    string ghiChuTrangThai = string.Empty;

                    if (danhSachBanBe.ContainsKey(user.Ten))
                    {
                        ghiChuTrangThai = " (Bạn bè)";
                    }
                    else if (danhSachDaGuiLoiMoi.Contains(user.Ten))
                    {
                        ghiChuTrangThai = " (Đã mời)";
                    }
                    else if (danhSachLoiMoiNhanDuoc.Contains(user.Ten))
                    {
                        ghiChuTrangThai = " (Mời bạn)";
                    }

                    Button btn = new Button();
                    btn.Text = string.Format("{0} {1}{2}", trangThai, user.Ten, ghiChuTrangThai);
                    btn.Tag = "user:" + user.Ten;
                    btn.Width = flpDanhSachChat.Width - 25;
                    btn.Height = 40;
                    btn.TextAlign = ContentAlignment.MiddleLeft;
                    btn.BackColor = Color.WhiteSmoke;
                    btn.FlatStyle = FlatStyle.Flat;

                    // Menu phải: kết bạn / chấp nhận / huỷ kết bạn
                    ContextMenuStrip cm = new ContextMenuStrip();

                    if (!danhSachBanBe.ContainsKey(user.Ten)
                        && !danhSachDaGuiLoiMoi.Contains(user.Ten)
                        && !danhSachLoiMoiNhanDuoc.Contains(user.Ten))
                    {
                        cm.Items.Add(
                            "Kết bạn",
                            null,
                            async delegate
                            {
                                await GuiLoiMoiKetBan(user.Ten);
                                await NapTrangThaiKetBan();
                                await TaiDanhSachNguoiDung();
                            });
                    }

                    if (danhSachLoiMoiNhanDuoc.Contains(user.Ten))
                    {
                        cm.Items.Add(
                            "Chấp nhận kết bạn",
                            null,
                            async delegate
                            {
                                await ChapNhanKetBan(user.Ten);
                                await NapTrangThaiKetBan();
                                await TaiDanhSachNguoiDung();
                            });
                    }

                    if (danhSachBanBe.ContainsKey(user.Ten))
                    {
                        cm.Items.Add(
                            "Huỷ kết bạn",
                            null,
                            async delegate
                            {
                                await HuyKetBan(user.Ten);
                                await NapTrangThaiKetBan();
                                await TaiDanhSachNguoiDung();
                            });
                    }

                    btn.ContextMenuStrip = cm;

                    // Click mở chat 1-1
                    btn.Click +=
                        async delegate
                        {
                            flbKhungChat.Controls.Clear();
                            ClearRenderQueue();

                            tenDoiPhuong = user.Ten;
                            currentChatIsGroup = false;
                            currentGroupId = string.Empty;

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

                    if (InvokeRequired)
                    {
                        Invoke(new Action(delegate { flpDanhSachChat.Controls.Add(btn); }));
                    }
                    else
                    {
                        flpDanhSachChat.Controls.Add(btn);
                    }
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

        private async Task TaiDanhSachNhom()
        {
            // Tải danh sách nhóm mà user hiện tại tham gia
            try
            {
                FirebaseResponse res = await firebase.GetAsync("nhom");
                Dictionary<string, Nhom> data =
                    res.ResultAs<Dictionary<string, Nhom>>();

                // Xoá các nút nhóm cũ
                List<Button> groupButtons = new List<Button>();

                foreach (Control control in flpDanhSachChat.Controls)
                {
                    Button button = control as Button;

                    if (button != null && button.Tag is string)
                    {
                        string tag = (string)button.Tag;

                        if (tag.StartsWith("group:"))
                        {
                            groupButtons.Add(button);
                        }
                    }
                }

                foreach (Button button in groupButtons)
                {
                    flpDanhSachChat.Controls.Remove(button);
                }

                if (data == null)
                {
                    return;
                }

                // Tạo nút nhóm
                foreach (KeyValuePair<string, Nhom> item in data)
                {
                    Nhom nhom = item.Value;

                    if (nhom == null || nhom.thanhVien == null)
                    {
                        continue;
                    }

                    if (!nhom.thanhVien.ContainsKey(tenHienTai))
                    {
                        continue;
                    }

                    Button btn = new Button();
                    btn.Text = "[Nhóm] " + nhom.tenNhom;
                    btn.Tag = "group:" + nhom.id;
                    btn.Width = flpDanhSachChat.Width - 25;
                    btn.Height = 40;
                    btn.BackColor = Color.LightYellow;
                    btn.FlatStyle = FlatStyle.Flat;
                    btn.TextAlign = ContentAlignment.MiddleLeft;

                    ContextMenuStrip cm = new ContextMenuStrip();

                    cm.Items.Add(
                        "Thêm thành viên",
                        null,
                        async delegate { await ThemThanhVienVaoNhom(nhom.id); });

                    if (nhom.taoBoi == tenHienTai)
                    {
                        cm.Items.Add(
                            "Xóa nhóm",
                            null,
                            async delegate
                            {
                                DialogResult confirm = MessageBox.Show(
                                    "Bạn có chắc muốn xóa nhóm \"" + nhom.tenNhom + "\" không?",
                                    "Xóa nhóm",
                                    MessageBoxButtons.YesNo,
                                    MessageBoxIcon.Warning);

                                if (confirm == DialogResult.Yes)
                                {
                                    await XoaNhom(nhom.id);
                                }
                            });
                    }

                    btn.ContextMenuStrip = cm;

                    // Click mở chat nhóm
                    btn.Click +=
                        async delegate
                        {
                            string tag = btn.Tag as string;

                            if (!string.IsNullOrEmpty(tag) && tag.StartsWith("group:"))
                            {
                                flbKhungChat.Controls.Clear();
                                ClearRenderQueue();

                                currentChatIsGroup = true;
                                currentGroupId = tag.Substring("group:".Length);
                                tenDoiPhuong = string.Empty;

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
            // Thêm bạn bè vào nhóm
            try
            {
                FirebaseResponse res = await firebase.GetAsync("nhom/" + idNhom);
                Nhom nhom = res.ResultAs<Nhom>();

                if (nhom == null)
                {
                    MessageBox.Show("Không tìm thấy nhóm!");
                    return;
                }

                List<string> banChuaCo = new List<string>();

                foreach (string ban in danhSachBanBe.Keys)
                {
                    if (!nhom.thanhVien.ContainsKey(ban))
                    {
                        banChuaCo.Add(ban);
                    }
                }

                using (ChonThanhVien form = new ChonThanhVien(banChuaCo))
                {
                    DialogResult result = form.ShowDialog();

                    if (result == DialogResult.OK)
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

                        MessageBox.Show("Đã thêm " + duocChon.Count + " thành viên vào nhóm!");
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
            // Xoá nhóm và toàn bộ cuộc trò chuyện nhóm
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

        private async void btnGui_Click(object sender, EventArgs e)
        {
            // Gửi tin nhắn (cho nhóm hoặc 1-1)
            string noiDung = txtNhapTinNhan.Text.Trim();

            if (string.IsNullOrEmpty(noiDung))
            {
                MessageBox.Show("Nhập nội dung tin nhắn trước khi gửi!");
                return;
            }

            // Gửi cho nhóm
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
                    TinNhan tn = new TinNhan();
                    tn.guiBoi = tenHienTai;
                    tn.nhanBoi = string.Empty;
                    tn.noiDung = noiDung;
                    tn.thoiGian = DateTime.UtcNow.ToString("o");

                    PushResponse push =
                        await firebase.PushAsync("cuocTroChuyenNhom/" + currentGroupId + "/", tn);

                    tn.id = push.Result.name;

                    await firebase.SetAsync(
                        "cuocTroChuyenNhom/" + currentGroupId + "/" + tn.id,
                        tn);

                    if (!dsTinNhanDaCoTheoDoanChat.ContainsKey(currentGroupId))
                    {
                        dsTinNhanDaCoTheoDoanChat[currentGroupId] = new HashSet<string>();
                    }

                    if (dsTinNhanDaCoTheoDoanChat[currentGroupId].Add(tn.id))
                    {
                        if (tn.noiDung == null)
                        {
                            tn.noiDung = string.Empty;
                        }
                    }

                    hangChoRender.Enqueue(tn);

                    if (!thuTuTinNhanTheoDoanChat.ContainsKey(currentGroupId))
                    {
                        thuTuTinNhanTheoDoanChat[currentGroupId] = new List<string>();
                    }

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

            // Gửi 1-1
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

                TinNhan tn = new TinNhan();
                tn.guiBoi = tenHienTai;
                tn.nhanBoi = tenDoiPhuong;
                tn.noiDung = noiDung;
                tn.thoiGian = DateTime.UtcNow.ToString("o");

                PushResponse push =
                    await firebase.PushAsync("cuocTroChuyen/" + cid + "/", tn);

                tn.id = push.Result.name;

                await firebase.SetAsync("cuocTroChuyen/" + cid + "/" + tn.id, tn);

                if (!dsTinNhanDaCoTheoDoanChat.ContainsKey(cid))
                {
                    dsTinNhanDaCoTheoDoanChat[cid] = new HashSet<string>();
                }

                if (dsTinNhanDaCoTheoDoanChat[cid].Add(tn.id))
                {
                    if (tn.noiDung == null)
                    {
                        tn.noiDung = string.Empty;
                    }
                }

                hangChoRender.Enqueue(tn);

                if (!thuTuTinNhanTheoDoanChat.ContainsKey(cid))
                {
                    thuTuTinNhanTheoDoanChat[cid] = new List<string>();
                }

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

        private async Task CapTinNhanMoi()
        {
            // Tải tin mới cho cuộc trò chuyện 1-1 hiện tại
            if (string.IsNullOrEmpty(tenDoiPhuong))
            {
                return;
            }

            DateTimeOffset now = DateTimeOffset.UtcNow;

            if ((now - lanCuoiTai1_1).TotalMilliseconds < 600)
            {
                return;
            }

            if (!await khoaTaiTinNhan.WaitAsync(0))
            {
                return;
            }

            try
            {
                string cid = TaoIdCuocTroChuyen(tenHienTai, tenDoiPhuong);

                FirebaseResponse res =
                    await firebase.GetAsync("cuocTroChuyen/" + cid);

                Dictionary<string, TinNhan> data =
                    res.ResultAs<Dictionary<string, TinNhan>>();

                if (!dsTinNhanDaCoTheoDoanChat.ContainsKey(cid))
                {
                    dsTinNhanDaCoTheoDoanChat[cid] = new HashSet<string>();
                }

                HashSet<string> dsDaCo = dsTinNhanDaCoTheoDoanChat[cid];

                if (!thuTuTinNhanTheoDoanChat.ContainsKey(cid))
                {
                    thuTuTinNhanTheoDoanChat[cid] = new List<string>();
                }

                if (data != null && data.Count > 0)
                {
                    // Sắp xếp theo thời gian gửi
                    var dsSapXep = data.OrderBy(
                        delegate (KeyValuePair<string, TinNhan> item)
                        {
                            if (item.Value != null)
                            {
                                return ParseTime(item.Value.thoiGian);
                            }

                            return ParseTime(null);
                        });

                    foreach (KeyValuePair<string, TinNhan> kv in dsSapXep)
                    {
                        TinNhan tn = kv.Value ?? new TinNhan();

                        if (string.IsNullOrEmpty(tn.id))
                        {
                            tn.id = kv.Key;
                        }

                        if (string.IsNullOrEmpty(tn.id))
                        {
                            continue;
                        }

                        if (dsDaCo.Add(tn.id))
                        {
                            if (tn.noiDung == null)
                            {
                                tn.noiDung = string.Empty;
                            }

                            thuTuTinNhanTheoDoanChat[cid].Add(tn.id);
                            hangChoRender.Enqueue(tn);
                        }
                    }
                }

                lanCuoiTai1_1 = now;

                await DanhDauDaXemCuoi();
            }
            catch
            {
            }
            finally
            {
                try
                {
                    khoaTaiTinNhan.Release();
                }
                catch
                {
                }
            }
        }

        private async Task CapTinNhanMoiNhom(string idNhom)
        {
            // Tải tin mới cho nhóm hiện tại
            if (string.IsNullOrEmpty(idNhom))
            {
                return;
            }

            DateTimeOffset now = DateTimeOffset.UtcNow;

            if ((now - lanCuoiTaiNhom).TotalMilliseconds < 600)
            {
                return;
            }

            if (!await khoaTaiTinNhan.WaitAsync(0))
            {
                return;
            }

            try
            {
                FirebaseResponse res =
                    await firebase.GetAsync("cuocTroChuyenNhom/" + idNhom);

                Dictionary<string, TinNhan> data =
                    res.ResultAs<Dictionary<string, TinNhan>>();

                if (!dsTinNhanDaCoTheoDoanChat.ContainsKey(idNhom))
                {
                    dsTinNhanDaCoTheoDoanChat[idNhom] = new HashSet<string>();
                }

                HashSet<string> dsDaCo = dsTinNhanDaCoTheoDoanChat[idNhom];

                if (!thuTuTinNhanTheoDoanChat.ContainsKey(idNhom))
                {
                    thuTuTinNhanTheoDoanChat[idNhom] = new List<string>();
                }

                if (data != null && data.Count > 0)
                {
                    var dsSapXep = data.OrderBy(
                        delegate (KeyValuePair<string, TinNhan> item)
                        {
                            if (item.Value != null)
                            {
                                return ParseTime(item.Value.thoiGian);
                            }

                            return ParseTime(null);
                        });

                    foreach (KeyValuePair<string, TinNhan> kv in dsSapXep)
                    {
                        TinNhan tn = kv.Value ?? new TinNhan();

                        if (string.IsNullOrEmpty(tn.id))
                        {
                            tn.id = kv.Key;
                        }

                        if (string.IsNullOrEmpty(tn.id))
                        {
                            continue;
                        }

                        if (dsDaCo.Add(tn.id))
                        {
                            if (tn.noiDung == null)
                            {
                                tn.noiDung = string.Empty;
                            }

                            thuTuTinNhanTheoDoanChat[idNhom].Add(tn.id);
                            hangChoRender.Enqueue(tn);
                        }
                    }
                }

                lanCuoiTaiNhom = now;

                await DanhDauDaXemCuoi();
            }
            catch
            {
            }
            finally
            {
                try
                {
                    khoaTaiTinNhan.Release();
                }
                catch
                {
                }
            }
        }

        private void BatTimerKiemTraTinNhanMoi()
        {
            // Bật timer dự phòng: nếu không có stream thì thỉnh thoảng tự tải tin mới
            try
            {
                if (timerTinNhanMoi != null)
                {
                    timerTinNhanMoi.Stop();
                    timerTinNhanMoi.Dispose();
                }
            }
            catch
            {
            }

            timerTinNhanMoi = new System.Windows.Forms.Timer();
            timerTinNhanMoi.Interval = 10000;

            timerTinNhanMoi.Tick +=
                async delegate
                {
                    if (streamChatHienTai == null)
                    {
                        if (currentChatIsGroup)
                        {
                            if (!string.IsNullOrEmpty(currentGroupId))
                            {
                                await CapTinNhanMoiNhom(currentGroupId);
                            }
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
            // Bật realtime cho cuộc trò chuyện 1-1 đang chọn
            try
            {
                if (streamChatHienTai != null)
                {
                    streamChatHienTai.Dispose();
                }
            }
            catch
            {
            }

            if (string.IsNullOrEmpty(tenDoiPhuong))
            {
                return;
            }

            string cid = TaoIdCuocTroChuyen(tenHienTai, tenDoiPhuong);

            firebase.OnAsync(
                "cuocTroChuyen/" + cid,
                added: delegate { UiCapNhatTinNhan(); },
                changed: delegate { UiCapNhatTinNhan(); },
                removed: delegate { UiCapNhatTinNhan(); }
            ).ContinueWith(
                delegate (Task<EventStreamResponse> t)
                {
                    if (t.Status == TaskStatus.RanToCompletion)
                    {
                        streamChatHienTai = t.Result;
                    }
                });
        }

        private async void BatRealtimeChatNhom(string idNhom)
        {
            // Bật realtime cho nhóm đang chọn
            try
            {
                if (streamChatHienTai != null)
                {
                    streamChatHienTai.Dispose();
                }
            }
            catch
            {
            }

            if (string.IsNullOrEmpty(idNhom))
            {
                return;
            }

            firebase.OnAsync(
                "cuocTroChuyenNhom/" + idNhom,
                added: delegate { UiCapNhatTinNhan(); },
                changed: delegate { UiCapNhatTinNhan(); },
                removed: delegate { UiCapNhatTinNhan(); }
            ).ContinueWith(
                delegate (Task<EventStreamResponse> t)
                {
                    if (t.Status == TaskStatus.RanToCompletion)
                    {
                        streamChatHienTai = t.Result;
                    }
                });
        }

        private Task UiCapNhatTinNhan()
        {
            // Gom các sự kiện realtime lại, đợi 300ms rồi mới tải tin (tránh tải liên tục)
            if (!IsHandleCreated || IsDisposed)
            {
                return Task.CompletedTask;
            }

            if (timerGomSuKienChat == null)
            {
                timerGomSuKienChat = new System.Windows.Forms.Timer();
                timerGomSuKienChat.Interval = 300;

                timerGomSuKienChat.Tick +=
                    async delegate
                    {
                        timerGomSuKienChat.Stop();

                        try
                        {
                            if (currentChatIsGroup)
                            {
                                if (!string.IsNullOrEmpty(currentGroupId))
                                {
                                    await CapTinNhanMoiNhom(currentGroupId);
                                }
                            }
                            else
                            {
                                await CapTinNhanMoi();
                            }
                        }
                        catch
                        {
                        }
                    };
            }

            timerGomSuKienChat.Stop();
            timerGomSuKienChat.Start();

            return Task.CompletedTask;
        }

        private string TaoIdCuocTroChuyen(string u1, string u2)
        {
            // Tạo id chung cho 2 người, không phụ thuộc thứ tự
            if (string.CompareOrdinal(u1, u2) < 0)
            {
                return u1 + "__" + u2;
            }

            return u2 + "__" + u1;
        }

        private async Task DanhDauSao(TinNhan tn)
        {
            // Đánh dấu tin nhắn với sao
            try
            {
                string key;

                if (currentChatIsGroup)
                {
                    key = currentGroupId;
                }
                else
                {
                    key = TaoIdCuocTroChuyen(tenHienTai, tenDoiPhuong);
                }

                await firebase.SetAsync(
                    "stars/" + tenHienTai + "/" + key + "/" + tn.id,
                    true);

                MessageBox.Show("Đã đánh dấu sao ⭐");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi đánh dấu sao: " + ex.Message);
            }
        }

        private async Task GhimTinNhan(TinNhan tn)
        {
            // Ghim tin nhắn
            try
            {
                string key;

                if (currentChatIsGroup)
                {
                    key = currentGroupId;
                }
                else
                {
                    key = TaoIdCuocTroChuyen(tenHienTai, tenDoiPhuong);
                }

                await firebase.SetAsync(
                    "pins/" + key + "/" + tn.id,
                    true);

                MessageBox.Show("Đã ghim 📌");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi ghim: " + ex.Message);
            }
        }

        private async Task XoaTinNhan(TinNhan tn)
        {
            // Xoá tin nhắn khỏi Firebase
            try
            {
                if (currentChatIsGroup)
                {
                    await firebase.DeleteAsync(
                        "cuocTroChuyenNhom/" + currentGroupId + "/" + tn.id);
                }
                else
                {
                    string cid =
                        TaoIdCuocTroChuyen(tenHienTai, tenDoiPhuong);

                    await firebase.DeleteAsync(
                        "cuocTroChuyen/" + cid + "/" + tn.id);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi xoá tin: " + ex.Message);
            }
        }

        private async Task NapTrangThaiKetBan()
        {
            // Nạp danh sách bạn bè và lời mời kết bạn
            danhSachBanBe.Clear();
            danhSachDaGuiLoiMoi.Clear();
            danhSachLoiMoiNhanDuoc.Clear();

            FirebaseResponse f1 =
                await firebase.GetAsync("friends/" + tenHienTai);

            Dictionary<string, bool> duLieuBanBe =
                f1.ResultAs<Dictionary<string, bool>>();

            if (duLieuBanBe != null)
            {
                danhSachBanBe = duLieuBanBe;
            }

            FirebaseResponse tatCaLoiMoi =
                await firebase.GetAsync("friendRequests/pending");

            Dictionary<string, Dictionary<string, bool>> duLieuLoiMoiCho =
                tatCaLoiMoi.ResultAs<Dictionary<string, Dictionary<string, bool>>>();

            if (duLieuLoiMoiCho != null)
            {
                foreach (KeyValuePair<string, Dictionary<string, bool>> loiMoiDenNguoiNhan in duLieuLoiMoiCho)
                {
                    string nguoiNhan = loiMoiDenNguoiNhan.Key;

                    foreach (KeyValuePair<string, bool> loiMoiTuNguoiGui in loiMoiDenNguoiNhan.Value)
                    {
                        string nguoiGui = loiMoiTuNguoiGui.Key;

                        if (nguoiGui == tenHienTai)
                        {
                            danhSachDaGuiLoiMoi.Add(nguoiNhan);
                        }
                    }
                }
            }

            FirebaseResponse f2 =
                await firebase.GetAsync("friendRequests/pending/" + tenHienTai);

            Dictionary<string, bool> danhSachNguoiGuiLoiMoi =
                f2.ResultAs<Dictionary<string, bool>>();

            if (danhSachNguoiGuiLoiMoi != null)
            {
                danhSachLoiMoiNhanDuoc =
                    new HashSet<string>(danhSachNguoiGuiLoiMoi.Keys);
            }
        }

        private async Task GuiLoiMoiKetBan(string ten)
        {
            // Gửi lời mời kết bạn
            await firebase.SetAsync(
                "friendRequests/pending/" + ten + "/" + tenHienTai,
                true);

            MessageBox.Show("Đã gửi lời mời kết bạn đến " + ten + "!");
            danhSachDaGuiLoiMoi.Add(ten);
        }

        private async Task ChapNhanKetBan(string ten)
        {
            // Chấp nhận kết bạn
            await firebase.SetAsync(
                "friends/" + tenHienTai + "/" + ten,
                true);

            await firebase.SetAsync(
                "friends/" + ten + "/" + tenHienTai,
                true);

            await firebase.DeleteAsync(
                "friendRequests/pending/" + tenHienTai + "/" + ten);

            MessageBox.Show("Bạn và " + ten + " đã trở thành bạn bè!");
            danhSachBanBe[ten] = true;
            danhSachLoiMoiNhanDuoc.Remove(ten);
        }

        private async Task HuyKetBan(string ten)
        {
            // Huỷ kết bạn
            await firebase.DeleteAsync(
                "friends/" + tenHienTai + "/" + ten);

            await firebase.DeleteAsync(
                "friends/" + ten + "/" + tenHienTai);

            danhSachBanBe.Remove(ten);

            MessageBox.Show("Đã hủy kết bạn với " + ten + ".");
        }

        private void BatRealtimeKetBan()
        {
            // Bật realtime cho friends và friendRequests
            try
            {
                if (streamFriends != null)
                {
                    streamFriends.Dispose();
                }

                if (streamFriendReq != null)
                {
                    streamFriendReq.Dispose();
                }
            }
            catch
            {
            }

            // Lắng nghe thay đổi danh sách bạn bè
            firebase.OnAsync(
                "friends/" + tenHienTai,
                added: async delegate { await RefreshBanBe(); },
                changed: async delegate { await RefreshBanBe(); },
                removed: async delegate { await RefreshBanBe(); }
            ).ContinueWith(
                delegate (Task<EventStreamResponse> t)
                {
                    if (t.Status == TaskStatus.RanToCompletion)
                    {
                        streamFriends = t.Result;
                    }
                });

            // Lắng nghe lời mời kết bạn tới mình
            firebase.OnAsync(
                "friendRequests/pending/" + tenHienTai,
                added: delegate (object sender, ValueAddedEventArgs args, object context)
                {
                    if (!IsHandleCreated || IsDisposed)
                    {
                        return;
                    }

                    BeginInvoke(
                        new Action(
                            async delegate
                            {
                                await NapTrangThaiKetBan();

                                string nguoiGui = string.Empty;

                                if (args != null && !string.IsNullOrEmpty(args.Path))
                                {
                                    nguoiGui = args.Path.TrimStart('/');
                                }

                                if (!string.IsNullOrEmpty(nguoiGui))
                                {
                                    MessageBox.Show(
                                        "📩 Bạn có lời mời kết bạn mới từ " + nguoiGui + "!",
                                        "Kết bạn mới");
                                }

                                await TaiDanhSachNguoiDung();
                            }));
                },
                removed: delegate
                {
                    if (!IsHandleCreated || IsDisposed)
                    {
                        return;
                    }

                    BeginInvoke(
                        new Action(
                            async delegate
                            {
                                await NapTrangThaiKetBan();
                                await TaiDanhSachNguoiDung();
                            }));
                }
            ).ContinueWith(
                delegate (Task<EventStreamResponse> t)
                {
                    if (t.Status == TaskStatus.RanToCompletion)
                    {
                        streamFriendReq = t.Result;
                    }
                });
        }

        private async Task RefreshBanBe()
        {
            // Reload lại trạng thái bạn bè và cập nhật UI
            try
            {
                await NapTrangThaiKetBan();

                if (InvokeRequired)
                {
                    BeginInvoke(
                        new Action(
                            async delegate
                            {
                                await TaiDanhSachNguoiDung();
                            }));
                }
                else
                {
                    await TaiDanhSachNguoiDung();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi làm mới danh sách bạn bè: " + ex.Message);
            }
        }

        private void BatRealtimeNhom()
        {
            // Bật realtime cho node nhóm
            try
            {
                if (streamNhom != null)
                {
                    streamNhom.Dispose();
                }
            }
            catch
            {
            }

            firebase.OnAsync(
                "nhom",
                added: delegate { XuLyCapNhatNhom(); },
                changed: delegate { XuLyCapNhatNhom(); },
                removed: delegate { XuLyCapNhatNhom(); }
            ).ContinueWith(
                delegate (Task<EventStreamResponse> t)
                {
                    if (t.Status == TaskStatus.RanToCompletion)
                    {
                        streamNhom = t.Result;
                    }
                });
        }

        private void XuLyCapNhatNhom()
        {
            // Khi nhóm có thay đổi: tải lại danh sách nhóm
            if (!IsHandleCreated || IsDisposed)
            {
                return;
            }

            BeginInvoke(
                new Action(
                    async delegate
                    {
                        await TaiDanhSachNhom();
                    }));
        }

        private async Task CapNhatTrangThai(string trangThai)
        {
            // Ghi trạng thái online/offline của chính mình lên Firebase
            if (firebase == null || string.IsNullOrEmpty(tenHienTai))
            {
                return;
            }

            try
            {
                string key = tenHienTai
                    .Replace(".", "_")
                    .Replace("$", "_");

                await firebase.SetAsync("status/" + key, trangThai);
            }
            catch
            {
            }
        }

        private void BatRealtimeTrangThai()
        {
            // Bật realtime cho node status
            try
            {
                if (streamTrangThai != null)
                {
                    streamTrangThai.Dispose();
                }
            }
            catch
            {
            }

            firebase.OnAsync(
                "status",
                added: delegate { CapNhatTrangThaiUI(); },
                changed: delegate { CapNhatTrangThaiUI(); },
                removed: delegate { CapNhatTrangThaiUI(); }
            ).ContinueWith(
                delegate (Task<EventStreamResponse> t)
                {
                    if (t.Status == TaskStatus.RanToCompletion)
                    {
                        streamTrangThai = t.Result;
                    }
                });
        }

        private void CapNhatTrangThaiUI()
        {
            // Đọc danh sách status từ Firebase và cập nhật list user
            if (IsDisposed || !IsHandleCreated)
            {
                return;
            }

            BeginInvoke(
                new Action(
                    async delegate
                    {
                        try
                        {
                            FirebaseResponse res =
                                await firebase.GetAsync("status");

                            Dictionary<string, string> data =
                                res.ResultAs<Dictionary<string, string>>();

                            if (data != null)
                            {
                                trangThaiNguoiDung = data;
                            }
                            else
                            {
                                trangThaiNguoiDung =
                                    new Dictionary<string, string>();
                            }

                            await TaiDanhSachNguoiDung();
                        }
                        catch
                        {
                        }
                    }));
        }

        private void KhoiTaoTyping()
        {
            // Khởi tạo cơ chế gửi trạng thái "đang nhập..."
            if (timerDangNhap != null)
            {
                return;
            }

            timerDangNhap = new System.Windows.Forms.Timer();
            timerDangNhap.Interval = 700; // Gõ xong 0.7s thì gửi trạng thái

            timerDangNhap.Tick +=
                async delegate
                {
                    timerDangNhap.Stop();

                    if (string.IsNullOrWhiteSpace(txtNhapTinNhan.Text))
                    {
                        return;
                    }

                    await GuiTrangThaiDangNhap();
                };

            txtNhapTinNhan.TextChanged +=
                delegate
                {
                    timerDangNhap.Stop();
                    timerDangNhap.Start();
                };
        }

        private async Task GuiTrangThaiDangNhap()
        {
            // Gửi thông tin đang nhập cho cuộc trò chuyện hiện tại
            string key = string.Empty;

            if (currentChatIsGroup)
            {
                key = currentGroupId;
            }
            else
            {
                if (!string.IsNullOrEmpty(tenDoiPhuong))
                {
                    key = TaoIdCuocTroChuyen(tenHienTai, tenDoiPhuong);
                }
            }

            if (string.IsNullOrEmpty(key))
            {
                return;
            }

            hetHanTyping = DateTimeOffset.UtcNow
                .AddSeconds(4)
                .ToUnixTimeMilliseconds();

            await firebase.SetAsync(
                "typing/" + key + "/" + tenHienTai,
                new { until = hetHanTyping });
        }

        private void BatRealtimeTyping()
        {
            // Lắng nghe trạng thái "đang nhập..." trong cuộc trò chuyện hiện tại
            try
            {
                if (streamTyping != null)
                {
                    streamTyping.Dispose();
                }
            }
            catch
            {
            }

            string key = string.Empty;

            if (currentChatIsGroup)
            {
                key = currentGroupId;
            }
            else
            {
                if (!string.IsNullOrEmpty(tenDoiPhuong))
                {
                    key = TaoIdCuocTroChuyen(tenHienTai, tenDoiPhuong);
                }
            }

            if (string.IsNullOrEmpty(key))
            {
                return;
            }

            firebase.OnAsync(
                "typing/" + key,
                added: delegate { CapNhatTypingUI(); },
                changed: delegate { CapNhatTypingUI(); },
                removed: delegate { CapNhatTypingUI(); }
            ).ContinueWith(
                delegate (Task<EventStreamResponse> t)
                {
                    if (t.Status == TaskStatus.RanToCompletion)
                    {
                        streamTyping = t.Result;
                    }
                });
        }

        private async void CapNhatTypingUI()
        {
            // Đọc node typing và hiển thị "đang nhập..."
            try
            {
                string key = string.Empty;

                if (currentChatIsGroup)
                {
                    key = currentGroupId;
                }
                else
                {
                    if (!string.IsNullOrEmpty(tenDoiPhuong))
                    {
                        key = TaoIdCuocTroChuyen(tenHienTai, tenDoiPhuong);
                    }
                }

                if (string.IsNullOrEmpty(key))
                {
                    return;
                }

                FirebaseResponse res =
                    await firebase.GetAsync("typing/" + key);

                Dictionary<string, Dictionary<string, long>> data =
                    res.ResultAs<Dictionary<string, Dictionary<string, long>>>();

                long now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

                bool coNguoiDangNhap = false;
                string tenNguoiDangNhap = string.Empty;

                if (data != null)
                {
                    foreach (KeyValuePair<string, Dictionary<string, long>> kv in data)
                    {
                        string ten = kv.Key;

                        if (string.Equals(
                            ten,
                            tenHienTai,
                            StringComparison.OrdinalIgnoreCase))
                        {
                            continue;
                        }

                        if (kv.Value != null)
                        {
                            long until;

                            if (kv.Value.TryGetValue("until", out until) &&
                                until > now)
                            {
                                coNguoiDangNhap = true;
                                tenNguoiDangNhap = ten;
                                break;
                            }
                        }
                    }
                }

                if (!IsDisposed && IsHandleCreated)
                {
                    BeginInvoke(
                        new Action(
                            delegate
                            {
                                lblTyping.Visible = coNguoiDangNhap;

                                if (coNguoiDangNhap)
                                {
                                    if (currentChatIsGroup)
                                    {
                                        lblTyping.Text =
                                            tenNguoiDangNhap + " đang nhập...";
                                    }
                                    else
                                    {
                                        lblTyping.Text = "Đang nhập...";
                                    }
                                }
                                else
                                {
                                    lblTyping.Text = string.Empty;
                                }
                            }));
                }
            }
            catch
            {
            }
        }

        private async Task DanhDauDaXemCuoi()
        {
            // Đánh dấu tin cuối cùng đã xem trong cuộc trò chuyện hiện tại
            try
            {
                string key = string.Empty;

                if (currentChatIsGroup)
                {
                    key = currentGroupId;
                }
                else
                {
                    if (!string.IsNullOrEmpty(tenDoiPhuong))
                    {
                        key = TaoIdCuocTroChuyen(tenHienTai, tenDoiPhuong);
                    }
                }

                if (string.IsNullOrEmpty(key))
                {
                    return;
                }

                List<string> list;

                if (!thuTuTinNhanTheoDoanChat.TryGetValue(key, out list) ||
                    list.Count == 0)
                {
                    return;
                }

                string msgIdCuoi = list[list.Count - 1];

                long nowMs =
                    DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

                if (currentChatIsGroup)
                {
                    await firebase.SetAsync(
                        "cuocTroChuyenNhom/" + key + "/" + msgIdCuoi +
                        "/reads/" + tenHienTai,
                        nowMs);
                }
                else
                {
                    string cid =
                        TaoIdCuocTroChuyen(tenHienTai, tenDoiPhuong);

                    await firebase.SetAsync(
                        "cuocTroChuyen/" + cid + "/" + msgIdCuoi +
                        "/reads/" + tenHienTai,
                        nowMs);
                }
            }
            catch
            {
            }
        }

        private void RefocusInput()
        {
            // Đưa focus về ô nhập tin nhắn
            if (IsDisposed || !IsHandleCreated)
            {
                return;
            }

            try
            {
                BeginInvoke(
                    new Action(
                        delegate
                        {
                            try
                            {
                                txtNhapTinNhan.Focus();
                                txtNhapTinNhan.SelectionStart =
                                    txtNhapTinNhan.TextLength;
                                txtNhapTinNhan.SelectionLength = 0;
                            }
                            catch
                            {
                            }
                        }));
            }
            catch
            {
            }
        }
    }

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
        public Dictionary<string, bool> thanhVien { get; set; }
        public string taoBoi { get; set; }

        public Nhom()
        {
            thanhVien = new Dictionary<string, bool>();
        }
    }
}

//  TÍNH NĂNG   

// 1. Xem danh sách người dùng
// Cách dùng: Khi mở form, panel bên trái sẽ hiển thị danh sách tất cả tài khoản (trừ chính mình).

// 2. Thấy trạng thái bạn bè / lời mời trên danh sách người dùng
// Cách dùng: Nhìn vào tên trong danh sách:
// - Có "(Bạn bè)" là đã là bạn.
// - Có "(Đã mời)" là mình đã gửi lời mời kết bạn cho họ.
// - Có "(Mời bạn)" là họ đã gửi lời mời cho mình.
// Nếu là bạn: sẽ hiển thị thêm (online)/(offline) tùy trạng thái.

// 3. Mở chat 1-1 với người khác
// Cách dùng: Click vào nút tên người dùng trong danh sách bên trái.
// Khi click:
// - Tiêu đề giữa màn hình thành tên người đó.
// - Khung chat bên phải hiển thị tin nhắn giữa mình và người đó.
// - Từ giờ gửi tin sẽ gửi cho người đó.

// 4. Gửi tin nhắn 1-1
// Cách dùng:
// - Gõ nội dung vào ô nhập.
// - Nhấn Enter để gửi.
// - Nhấn Shift+Enter hoặc Ctrl+Enter để xuống dòng mà không gửi.
// - Hoặc nhấn nút "Gửi".
// Sau khi gửi: tin của mình hiện bên phải (màu xanh nhạt), có giờ gửi.

// 5. Xem và đọc tin nhắn 1-1 theo thời gian
// Cách dùng: Khi mở cuộc trò chuyện, ứng dụng tự tải tất cả tin nhắn theo thứ tự thời gian từ Firebase và hiển thị dạng bong bóng.

// 6. Xem danh sách nhóm chat mà mình đang tham gia
// Cách dùng: Trong panel bên trái, các nút có dạng "[Nhóm] TênNhóm" là các nhóm mình đang ở.
// Các nhóm này được load từ Firebase node "nhom" và lọc theo thành viên có chứa tên mình.

// 7. Mở chat nhóm
// Cách dùng: Click vào nút "[Nhóm] TênNhóm".
// Khi click:
// - Tiêu đề giữa là tên nhóm.
// - Khung chat hiển thị tin nhắn của nhóm đó.
// - Mỗi tin nhắn trong nhóm có hiện tên người gửi ở trên nội dung.

// 8. Gửi tin nhắn trong nhóm
// Cách dùng:
// - Chọn nhóm ở bên trái.
// - Gõ tin ở ô nhập.
// - Nhấn Enter hoặc nút "Gửi".
// Tin nhắn sẽ được gửi lên node "cuocTroChuyenNhom" và hiển thị cho tất cả thành viên nhóm.

// 9. Thêm thành viên vào nhóm
// Cách dùng:
// - Chuột phải vào nút "[Nhóm] TênNhóm".
// - Chọn "Thêm thành viên".
// - Chọn trong danh sách bạn bè (chưa ở trong nhóm).
// - Bấm OK để thêm. Các thành viên được thêm sẽ xuất hiện trong nhóm đó.

// 10. Xóa nhóm (chỉ người tạo nhóm)
// Cách dùng:
// - Chuột phải vào nút "[Nhóm] TênNhóm" mà mình là người tạo.
// - Chọn "Xóa nhóm".
// - Xác nhận Yes.
// Nhóm và toàn bộ tin nhắn nhóm đó sẽ bị xóa khỏi Firebase.

// 11. Gửi lời mời kết bạn
// Cách dùng:
// - Chuột phải vào một người trong danh sách không phải bạn, không có trạng thái mời.
// - Chọn "Kết bạn".
// - Hệ thống lưu lời mời và hiển thị "(Đã mời)" bên cạnh tên người đó.

// 12. Chấp nhận lời mời kết bạn
// Cách dùng:
// - Chuột phải vào người có "(Mời bạn)".
// - Chọn "Chấp nhận kết bạn".
// - Sau đó hai bên trở thành bạn, hiển thị "(Bạn bè)".

// 13. Hủy kết bạn
// Cách dùng:
// - Chuột phải vào người có "(Bạn bè)".
// - Chọn "Huỷ kết bạn".
// - Quan hệ bạn bè bị xóa ở cả hai phía.

// 14. Nhận thông báo popup khi có lời mời kết bạn mới
// Cách dùng:
// - Khi ai đó gửi lời mời kết bạn cho mình, app tự hiện MessageBox:
//   "📩 Bạn có lời mời kết bạn mới từ XYZ!".

// 15. Hiển thị trạng thái online/offline
// Cách dùng:
// - Trạng thái của mình được set "online" khi mở form, "offline" khi đóng form.
// - Danh sách user sẽ dựa vào node "status" để hiển thị (online)/(offline) cho bạn bè.
// Người dùng chỉ cần mở app, trạng thái tự cập nhật.

// 16. Hiển thị "Đang nhập..." trong cuộc trò chuyện
// Cách dùng:
// - Khi mình gõ, app gửi trạng thái typing lên Firebase.
// - Khi người khác gõ với mình (1-1) hoặc trong nhóm hiện tại:
//   + 1-1: hiện "Đang nhập..." dưới tiêu đề.
//   + Nhóm: hiện "TênNguoiDung đang nhập...".
// Người dùng không cần thao tác, tự động hiển thị.

// 17. Sao chép nội dung tin nhắn
// Cách dùng:
// - Chuột phải vào bong bóng tin nhắn bất kỳ.
// - Chọn "Sao chép".
// - Nội dung tin nhắn được copy vào Clipboard của Windows.

// 18. Tự động cuộn xuống tin mới khi đang ở cuối khung chat
// Cách dùng:
// - Nếu thanh cuộn đang ở cuối, khi có tin nhắn mới, app tự cuộn xuống để luôn thấy tin mới.
// - Nếu người dùng đang kéo lên xem tin cũ, app sẽ không tự kéo xuống (để không làm khó chịu).

// 19. Bong bóng tin nhắn căn trái/phải theo người gửi
// Cách dùng:
// - Tin của mình: nằm bên phải, màu khác.
// - Tin của người khác: nằm bên trái.
// Người dùng chỉ cần nhìn là phân biệt được.

// 20. Khung chat tự co giãn theo độ rộng cửa sổ
// Cách dùng:
// - Khi phóng to/thu nhỏ form, app tự tính lại độ rộng bong bóng, tránh tràn, dễ đọc.
// Người dùng không cần chỉnh tay.