using ChatApp.Controllers;
using ChatApp.Controls;
using ChatApp.Forms;
using ChatApp.Models.Users;
using ChatApp.Services.Firebase;
using ChatApp.Services.UI;
using Guna.UI2.WinForms;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ChatApp
{
    /// <summary>
    /// Form Nhắn tin:
    /// - Bên trái: danh sách user (flpDanhSachChat).
    /// - Giữa: khung chat (pnlKhungChat, txtNhapTinNhan, btnGui).
    /// - Bên phải: thông tin user đang chat.
    /// </summary>
    public partial class NhanTin : Form
    {
        #region ====== BIẾN THÀNH VIÊN ======

        /// <summary>
        /// localId của user hiện tại.
        /// </summary>
        private readonly string idDangNhap;

        /// <summary>
        /// Token đăng nhập (để dành nếu dùng).
        /// </summary>
        private readonly string tokenDangNhap;

        /// <summary>
        /// Controller xử lý logic nhắn tin.
        /// </summary>
        private readonly NhanTinController boDieuKhienNhanTin;

        /// <summary>
        /// Toàn bộ user lấy từ Firebase.
        /// </summary>
        private Dictionary<string, User> tatCaNguoiDung =
            new Dictionary<string, User>();

        /// <summary>
        /// localId user đang được chọn để chat.
        /// </summary>
        private string idNguoiDangChat;

        /// <summary>
        /// Danh sách tin nhắn đã vẽ lên UI.
        /// </summary>
        private readonly List<ChatMessage> danhSachTinNhanDangVe =
            new List<ChatMessage>();

        /// <summary>
        /// Dịch vụ để cập nhật chế độ ngày đêm (dark/light).
        /// </summary>
        private readonly ThemeService _themeService = new ThemeService();

        #endregion

        #region ====== HÀM KHỞI TẠO ======

        /// <summary>
        /// Khởi tạo form Nhắn tin với localId + token hiện tại.
        /// </summary>
        public NhanTin(string localId, string token)
        {
            InitializeComponent();

            idDangNhap = localId;
            tokenDangNhap = token;
            boDieuKhienNhanTin = new NhanTinController(localId, token);

            // Event form
            this.Load += NhanTin_Load;
            this.FormClosed += NhanTin_FormClosed;

            // Event control
            txtTimKiem.TextChanged += txtTimKiem_TextChanged;
            btnGui.Click += btnGui_Click;
            txtNhapTinNhan.KeyDown += TxtNhapTinNhan_KeyDown;

            // Chế độ ngày đêm
            // _themeService = new ThemeService(); // không cần set lại vì field đã new sẵn

            // ======SEND FILE: gắn click cho icon gửi file ======
            PicSendFile.Click -= PicSendFile_Click;
            PicSendFile.Click += PicSendFile_Click;
            PicSendFile.Enabled = true;
            PicSendFile.Visible = true;
            PicSendFile.Cursor = Cursors.Hand;
            PicSendFile.BringToFront();

            // ======SEND FILE: bật TLS 1.2 để tải HTTPS ổn định hơn ======
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            ServicePointManager.Expect100Continue = false;
            ServicePointManager.CheckCertificateRevocationList = false;
        }

        #endregion

        #region ====== ENTER ĐỂ GỬI ======

        /// <summary>
        /// Nhấn Enter để gửi, Shift+Enter để xuống dòng.
        /// </summary>
        private void TxtNhapTinNhan_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter && !e.Shift)
            {
                e.SuppressKeyPress = true;
                btnGui.PerformClick();
            }
        }

        #endregion

        #region ====== FORM EVENTS ======

        private async void NhanTin_Load(object sender, EventArgs e)
        {
            await LoadUsersAsync();

            // Load chế độ ngày đêm
            bool isDark = await _themeService.GetThemeAsync(idDangNhap);
            ThemeManager.ApplyTheme(this, isDark);
        }

        private void NhanTin_FormClosed(object sender, FormClosedEventArgs e)
        {
            boDieuKhienNhanTin.Dispose();
        }

        #endregion

        #region ====== LOAD & LỌC DANH SÁCH USER ======

        /// <summary>
        /// Load toàn bộ user từ Firebase và đưa lên flpDanhSachChat.
        /// </summary>
        private async Task LoadUsersAsync()
        {
            // Clear danh sách người dùng để load lại
            flpDanhSachChat.Controls.Clear();

            try
            {
                tatCaNguoiDung = await boDieuKhienNhanTin.GetAllUsersAsync();

                foreach (KeyValuePair<string, User> cap in tatCaNguoiDung)
                {
                    string idNguoiDung = cap.Key;
                    User nguoiDung = cap.Value;

                    // Bỏ qua chính mình
                    if (string.Equals(idNguoiDung, idDangNhap, StringComparison.Ordinal))
                    {
                        continue;
                    }

                    AddUserItem(idNguoiDung, nguoiDung);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Lỗi tải danh sách người dùng: " + ex.Message,
                    "Lỗi",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Tạo 1 nút user trong flpDanhSachChat.
        /// </summary>
        private void AddUserItem(string userId, User user)
        {
            Conversations conversations = new Conversations();
            conversations.Cursor = Cursors.Hand;
            conversations.Width = flpDanhSachChat.ClientSize.Width - 10;
            conversations.SetInfo(GetUserFullName(user), userId);
            conversations.Tag = userId;

            // Gán sự kiện click
            conversations.ItemClicked += UserItem_Click;

            flpDanhSachChat.Controls.Add(conversations);
        }

        /// <summary>
        /// Lấy tên hiển thị: FullName, nếu trống thì "Unknown".
        /// </summary>
        private static string GetUserFullName(User user)
        {
            if (user == null)
            {
                return "Unknown";
            }

            if (!string.IsNullOrEmpty(user.FullName))
            {
                return user.FullName;
            }

            return "Unknown";
        }

        /// <summary>
        /// Lọc danh sách user khi gõ vào ô txtTimKiem.
        /// </summary>
        private void txtTimKiem_TextChanged(object sender, EventArgs e)
        {
            string tuKhoa = string.Empty;

            if (txtTimKiem.Text != null)
            {
                tuKhoa = txtTimKiem.Text.Trim().ToLowerInvariant();
            }

            flpDanhSachChat.SuspendLayout();
            flpDanhSachChat.Controls.Clear();

            foreach (KeyValuePair<string, User> cap in tatCaNguoiDung)
            {
                string idNguoiDung = cap.Key;
                User nguoiDung = cap.Value;

                if (string.Equals(idNguoiDung, idDangNhap, StringComparison.Ordinal))
                {
                    continue;
                }

                string ten = GetUserFullName(nguoiDung);

            }

            flpDanhSachChat.ResumeLayout();
        }

        #endregion

        #region ====== CHỌN USER & MỞ CUỘC TRÒ CHUYỆN ======

        private void UserItem_Click(object sender, EventArgs e)
        {
            var conversations = sender as Conversations;
            if (conversations == null)
                return;

            string idNguoiDung = conversations.Tag as string;
            if (string.IsNullOrEmpty(idNguoiDung))
                return;

            OpenConversation(idNguoiDung);
        }

        /// <summary>
        /// Mở cuộc trò chuyện với 1 user:
        /// - Cập nhật tên ở panel giữa/phải.
        /// - Xoá khung chat cũ.
        /// - Lắng nghe realtime cuộc trò chuyện đó.
        /// </summary>
        private void OpenConversation(string otherUserId)
        {
            idNguoiDangChat = otherUserId;

            // Cập nhật label tên người đang chat
            User nguoiDung;
            if (tatCaNguoiDung != null && tatCaNguoiDung.TryGetValue(otherUserId, out nguoiDung))
            {
                string ten = GetUserFullName(nguoiDung);
                lblTenDangNhapGiua.Text = ten;
                lblTenDangNhapPhai.Text = ten;
            }

            pnlKhungChat.Controls.Clear();
            danhSachTinNhanDangVe.Clear();

            boDieuKhienNhanTin.StartListenConversation(
                otherUserId,
                delegate (List<ChatMessage> danhSachTinNhan)
                {
                    // Callback này chạy ở thread khác, phải Invoke về UI.
                    if (!IsHandleCreated)
                    {
                        return;
                    }

                    try
                    {
                        BeginInvoke(
                            new Action(
                                delegate
                                {
                                    RenderMessages(danhSachTinNhan, otherUserId);
                                }));
                    }
                    catch
                    {
                        // giữ nguyên
                    }
                });
        }

        #endregion

        #region ====== VẼ TIN NHẮN LÊN KHUNG CHAT ======

        /// <summary>
        /// Vẽ tin nhắn:
        /// - Nếu lần đầu: vẽ toàn bộ.
        /// - Nếu thêm mới: chỉ append phần mới.
        /// - Nếu số lượng giảm: vẽ lại toàn bộ.
        /// </summary>
        private void RenderMessages(IList<ChatMessage> messages, string ownerUserId)
        {
            // Nếu user đã đổi cuộc chat thì bỏ qua batch này.
            if (!string.Equals(idNguoiDangChat, ownerUserId, StringComparison.Ordinal))
            {
                return;
            }

            if (messages == null || messages.Count == 0)
            {
                pnlKhungChat.SuspendLayout();
                pnlKhungChat.Controls.Clear();
                pnlKhungChat.ResumeLayout();
                danhSachTinNhanDangVe.Clear();
                return;
            }

            int soLuongCu = danhSachTinNhanDangVe.Count;
            int soLuongMoi = messages.Count;

            pnlKhungChat.SuspendLayout();

            try
            {
                if (soLuongCu == 0 || soLuongMoi < soLuongCu)
                {
                    // Lần đầu hoặc có thay đổi bất thường -> vẽ lại toàn bộ
                    pnlKhungChat.Controls.Clear();
                    danhSachTinNhanDangVe.Clear();

                    int i = 0;
                    while (i < soLuongMoi)
                    {
                        ChatMessage tinNhan = messages[i];
                        AddMessageBubble(tinNhan);
                        danhSachTinNhanDangVe.Add(tinNhan);
                        i++;
                    }
                }
                else
                {
                    // Bình thường thì chỉ vẽ thêm phần mới
                    int i = soLuongCu;
                    while (i < soLuongMoi)
                    {
                        ChatMessage tinNhan = messages[i];
                        AddMessageBubble(tinNhan);
                        danhSachTinNhanDangVe.Add(tinNhan);
                        i++;
                    }
                }

                if (pnlKhungChat.Controls.Count > 0)
                {
                    Control controlCuoi =
                        pnlKhungChat.Controls[pnlKhungChat.Controls.Count - 1];
                    pnlKhungChat.ScrollControlIntoView(controlCuoi);
                }
            }
            finally
            {
                pnlKhungChat.ResumeLayout();
            }
        }

        /// <summary>
        /// Thêm 1 bong bóng tin nhắn (trái/phải) vào khung chat.
        /// </summary>
        private void AddMessageBubble(ChatMessage msg)
        {
            if (msg == null)
            {
                return;
            }

            bool isMine = msg.IsMine;

            // Tên hiển thị: mình thì "Bạn", người kia thì lấy label giữa
            string displayName = isMine ? "Bạn" : lblTenDangNhapGiua.Text;

            // ====== SEND FILE : xử lý tin nhắn file ======
            bool laTinFile = string.Equals(msg.MessageType, "file", StringComparison.OrdinalIgnoreCase);

            string message;
            if (laTinFile)
            {
                // Không hiển thị URL ra UI (chỉ hiển thị tên + dung lượng)
                string ten = string.IsNullOrEmpty(msg.FileName) ? "file" : msg.FileName;
                message = ten + " (" + FormatBytes(msg.FileSize) + ")";
            }
            else
            {
                message = msg.Text ?? "";
            }

            string time = FormatTimestamp(msg.Timestamp);

            MessageBubbles bubble = new MessageBubbles();
            bubble.SetMessage(displayName, message, time, isMine);
            bubble.Dock = DockStyle.Top;

            // ====== SEND FILE: click để tải ======
            if (laTinFile)
            {
                // Tag lưu nguyên ChatMessage để handler lấy FileUrl/FileName
                bubble.Tag = msg;
                bubble.Cursor = Cursors.Hand;

                // Bấm trúng label/khung con bên trong vẫn tải được
                GanClickDeTaiFile(bubble);
            }

            pnlKhungChat.Controls.Add(bubble);
            pnlKhungChat.Controls.SetChildIndex(bubble, 0); // giữ thứ tự như bạn đang làm
        }

        /// <summary>
        /// Chuyển timestamp (ms) thành chuỗi dd/MM/yyyy HH:mm.
        /// </summary>
        private static string FormatTimestamp(long timestamp)
        {
            if (timestamp <= 0)
            {
                return string.Empty;
            }

            try
            {
                DateTime thoiGian =
                    DateTimeOffset.FromUnixTimeMilliseconds(timestamp).LocalDateTime;
                return thoiGian.ToString("dd/MM/yyyy HH:mm");
            }
            catch
            {
                return string.Empty;
            }
        }

        #endregion

        #region ====== GỬI TIN NHẮN ======

        private async void btnGui_Click(object sender, EventArgs e)
        {
            string noiDungTin = string.Empty;

            if (txtNhapTinNhan.Text != null)
            {
                noiDungTin = txtNhapTinNhan.Text.Trim();
            }

            if (string.IsNullOrEmpty(noiDungTin))
            {
                return;
            }

            if (string.IsNullOrEmpty(idNguoiDangChat))
            {
                MessageBox.Show(
                    "Vui lòng chọn người cần nhắn tin ở danh sách bên trái.",
                    "Thông báo",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return;
            }

            txtNhapTinNhan.Clear();

            try
            {
                await boDieuKhienNhanTin.SendMessageAsync(idNguoiDangChat, noiDungTin);
                // Tin sẽ tự hiện lên nhờ realtime và RenderMessages.
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Lỗi gửi tin nhắn: " + ex.Message,
                    "Lỗi",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        #endregion

        #region ====== NÚT SEND FILE ======

        /// <summary>
        /// Chặn click gửi file bị gọi nhiều lần (spam click / dính event).
        /// </summary>
        private bool dangGuiFile = false;

        /// <summary>
        /// Chặn click tải file bị gọi nhiều lần.
        /// Tránh trường hợp bấm liên tục => 2 luồng ghi file cùng lúc.
        /// </summary>
        private bool dangTaiFile = false;

        /// <summary>
        /// HttpClient dùng chung cho tải file (download).
        /// Dùng handler có:
        /// - Proxy hệ thống (lab/uni hay dính)
        /// - AutoDecompression gzip/deflate
        /// - User-Agent
        /// </summary>
        private static readonly HttpClient httpClient = TaoHttpClientTaiFile();

        /// <summary>
        /// Tạo HttpClient để giảm lỗi request trong môi trường lab.
        /// </summary>
        private static HttpClient TaoHttpClientTaiFile()
        {
            HttpClientHandler handler = new HttpClientHandler();
            handler.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

            handler.UseProxy = true;
            handler.Proxy = WebRequest.DefaultWebProxy;

            HttpClient c = new HttpClient(handler);
            c.Timeout = TimeSpan.FromMinutes(5);

            c.DefaultRequestHeaders.UserAgent.Clear();
            c.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0");

            return c;
        }

        /// <summary>
        /// Click icon gửi file:
        /// - Mở OpenFileDialog chọn file
        /// - Controller upload + push message "file"
        /// </summary>
        private async void PicSendFile_Click(object sender, EventArgs e)
        {
            if (dangGuiFile) return;
            dangGuiFile = true;

            try
            {
                if (string.IsNullOrEmpty(idNguoiDangChat))
                {
                    MessageBox.Show("Chọn người để chat trước đã.");
                    return;
                }

                using (OpenFileDialog ofd = new OpenFileDialog())
                {
                    ofd.Title = "Chọn file để gửi";
                    ofd.Filter = "All files (*.*)|*.*";

                    if (ofd.ShowDialog() != DialogResult.OK)
                    {
                        return;
                    }

                    // Upload + push message dạng "file"
                    await boDieuKhienNhanTin.SendFileMessageAsync(idNguoiDangChat, ofd.FileName);

                    // Ép load lại để thấy bong bóng file ngay
                    List<ChatMessage> ds = await boDieuKhienNhanTin.GetConversationAsync(idNguoiDangChat);
                    RenderMessages(ds, idNguoiDangChat);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi gửi file: " + ex.Message);
            }
            finally
            {
                dangGuiFile = false;
            }
        }

        /// <summary>
        /// Đổi bytes -> B/KB/MB/GB để hiển thị dung lượng file dễ đọc.
        /// </summary>
        private static string FormatBytes(long bytes)
        {
            double b = bytes;
            string[] u = { "B", "KB", "MB", "GB" };
            int i = 0;

            while (b >= 1024 && i < u.Length - 1)
            {
                b /= 1024;
                i++;
            }

            return $"{b:0.##} {u[i]}";
        }

        /// <summary>
        /// Gắn click tải file cho bubble và toàn bộ control con.
        /// Mục tiêu: user bấm vào đâu trong bong bóng cũng tải được.
        /// </summary>
        private void GanClickDeTaiFile(Control root)
        {
            if (root == null) return;

            // Tránh gắn trùng handler khi render lại nhiều lần
            root.Click -= BubbleFile_Click;
            root.Click += BubbleFile_Click;

            foreach (Control child in root.Controls)
            {
                GanClickDeTaiFile(child);
            }
        }

        /// <summary>
        /// Vì MessageBubbles là UserControl, click có thể trúng label/control con,
        /// nên phải leo lên Parent để tìm Tag chứa ChatMessage.
        /// </summary>
        private ChatMessage LayChatMessageTuTag(Control start)
        {
            Control c = start;
            while (c != null)
            {
                ChatMessage msg = c.Tag as ChatMessage;
                if (msg != null)
                {
                    return msg;
                }
                c = c.Parent;
            }
            return null;
        }

        /// <summary>
        /// Click bong bóng file -> mở SaveFileDialog và tải file về máy.
        /// </summary>
        private async void BubbleFile_Click(object sender, EventArgs e)
        {
            if (dangTaiFile) return;
            dangTaiFile = true;

            try
            {
                ChatMessage msg = LayChatMessageTuTag(sender as Control);

                if (msg == null)
                {
                    MessageBox.Show("Không lấy được dữ liệu file (Tag bị thiếu).");
                    return;
                }

                if (!string.Equals(msg.MessageType, "file", StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }

                if (string.IsNullOrWhiteSpace(msg.FileUrl))
                {
                    MessageBox.Show("Tin nhắn file không có URL để tải.");
                    return;
                }

                using (SaveFileDialog sfd = new SaveFileDialog())
                {
                    // Gợi ý lưu ra Downloads cho đỡ dính bin\Debug
                    string downloads = Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                        "Downloads");
                    if (Directory.Exists(downloads))
                    {
                        sfd.InitialDirectory = downloads;
                    }

                    sfd.FileName = string.IsNullOrEmpty(msg.FileName) ? "download.bin" : msg.FileName;
                    sfd.OverwritePrompt = true;

                    if (sfd.ShowDialog() != DialogResult.OK)
                    {
                        return;
                    }

                    // Download + ghi file
                    using (HttpResponseMessage resp =
                        await httpClient.GetAsync(msg.FileUrl.Trim(), HttpCompletionOption.ResponseHeadersRead))
                    {
                        resp.EnsureSuccessStatusCode();

                        using (Stream net = await resp.Content.ReadAsStreamAsync())
                        using (FileStream fs = new FileStream(sfd.FileName, FileMode.Create, FileAccess.Write, FileShare.None))
                        {
                            await net.CopyToAsync(fs);
                        }
                    }

                    MessageBox.Show("Tải xong:\n" + sfd.FileName);
                }
            }
            catch (IOException)
            {
                // Lỗi hay gặp: file đang mở bằng Edge/Adobe => bị lock
                MessageBox.Show("Không ghi được file vì file đang được mở/đang bị khóa.\nĐóng file đó hoặc chọn tên khác rồi tải lại.");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Tải file lỗi:\n" + ex.ToString());
            }
            finally
            {
                dangTaiFile = false;
            }
        }

        #endregion

        private void btnSearchFriends_Click(object sender, EventArgs e)
        {
            TimKiemBanBe timKiemBanBe = new TimKiemBanBe(idDangNhap, tokenDangNhap);
            timKiemBanBe.Show();
        }

        private void btnRequest_Click(object sender, EventArgs e)
        {
            FormLoiMoiKetBan formLoiMoiKetBan = new FormLoiMoiKetBan(idDangNhap, tokenDangNhap);
            formLoiMoiKetBan.Show();
        }
    }
}
