using ChatApp.Controllers;
using ChatApp.Controls;
using ChatApp.Forms;
using ChatApp.Helpers;
using ChatApp.Models.Groups;
using ChatApp.Models.Messages;
using ChatApp.Models.Users;
using ChatApp.Services.Firebase;
using ChatApp.Services.UI;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ChatApp
{
    public partial class NhanTin : Form
    {
        #region ====== 1) BIẾN THÀNH VIÊN / DỊCH VỤ ======

        /// <summary>
        /// localId của user hiện tại.
        /// </summary>
        private readonly string idDangNhap;

        /// <summary>
        /// Token đăng nhập (để dành nếu dùng).
        /// </summary>
        private readonly string tokenDangNhap;

        /// <summary>
        /// Controller xử lý logic nhắn tin 1-1.
        /// </summary>
        private readonly NhanTinController boDieuKhienNhanTin;

        /// <summary>
        /// Controller xử lý logic nhắn tin nhóm.
        /// </summary>
        private readonly NhanTinNhomController boDieuKhienNhanTinNhom;

        /// <summary>
        /// Danh sách nhóm của user (key = groupId).
        /// </summary>
        private Dictionary<string, GroupInfo> tatCaNhom = new Dictionary<string, GroupInfo>();

        /// <summary>
        /// Flag đang chat nhóm hay 1-1.
        /// </summary>
        private bool dangChatNhom = false;

        /// <summary>
        /// Prefix tag để phân biệt item nhóm trong danh sách chat.
        /// </summary>
        private const string GROUP_TAG_PREFIX = "GROUP:";

        /// <summary>
        /// Danh sách bạn bè (key = safeId).
        /// </summary>
        private Dictionary<string, User> tatCaNguoiDung = new Dictionary<string, User>();

        /// <summary>
        /// Id (userId/groupId) đang chat hiện tại.
        /// </summary>
        private string idNguoiDangChat;

        /// <summary>
        /// Service lưu/đọc theme Dark/Light.
        /// </summary>
        private readonly ThemeService _themeService = new ThemeService();

        /// <summary>
        /// Service thao tác dữ liệu nhóm (ví dụ avatar nhóm).
        /// </summary>
        private readonly GroupService _groupService = new GroupService();

        /// <summary>
        /// Service thao tác user (avatar, status, thông tin người gửi...).
        /// </summary>
        private readonly AuthService _authService = new AuthService();

        /// <summary>
        /// Controller xử lý logic bạn bè (hủy kết bạn...).
        /// </summary>
        private readonly FriendController _friendController;

        /// <summary>
        /// Danh sách tin nhắn đã render (giữ state + hỗ trợ append).
        /// </summary>
        private readonly List<ChatMessage> danhSachTinNhanDangVe = new List<ChatMessage>();

        // ====== BATCH UI: gom nhiều tin mới rồi vẽ 1 lượt (giảm giật/lag) ======
        private readonly object _pendingLock = new object();
        private readonly List<ChatMessage> _pendingAdds = new List<ChatMessage>();
        private System.Windows.Forms.Timer _flushTimer;

        // ====== DEDUPE: đánh dấu tin đã vẽ để không vẽ trùng ======
        private readonly HashSet<string> _drawnMessageIds = new HashSet<string>(StringComparer.Ordinal);
        private readonly object _drawnLock = new object();

        // ====== VERSION: đổi chat => tăng version để callback listener cũ tự bị vô hiệu ======
        private int _listenVersion = 0;

        #endregion

        #region ====== 2) KHỞI TẠO FORM ======

        /// <summary>
        /// Khởi tạo form Nhắn tin với localId + token hiện tại.
        /// </summary>
        public NhanTin(string localId, string token)
        {
            InitializeComponent();

            // Lưu thông tin đăng nhập hiện tại
            idDangNhap = localId;
            tokenDangNhap = token;

            // Khởi tạo controller/service cần thiết
            boDieuKhienNhanTin = new NhanTinController(localId, token);
            boDieuKhienNhanTinNhom = new NhanTinNhomController(localId, token);
            _friendController = new FriendController(localId);

            // Bật TLS 1.2 để download HTTPS ổn định hơn
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            ServicePointManager.Expect100Continue = false;
            ServicePointManager.CheckCertificateRevocationList = false;
        }

        #endregion

        #region ====== 3) SỰ KIỆN FORM ======

        /// <summary>
        /// Load dữ liệu ban đầu: danh sách chat + theme.
        /// </summary>
        private async void NhanTin_Load(object sender, EventArgs e)
        {
            // Load bạn bè + nhóm vào panel trái
            await LoadUsersAsync();

            // Áp dụng theme
            bool isDark = await _themeService.GetThemeAsync(idDangNhap);
            ThemeManager.ApplyTheme(this, isDark);
        }

        /// <summary>
        /// Dọn dẹp listener/timer khi đóng form.
        /// </summary>
        private void NhanTin_FormClosed(object sender, FormClosedEventArgs e)
        {
            // Stop timer batch nếu có
            try
            {
                if (_flushTimer != null)
                {
                    _flushTimer.Stop();
                    _flushTimer.Dispose();
                    _flushTimer = null;
                }
            }
            catch { }

            // Dispose listener 1-1
            boDieuKhienNhanTin.Dispose();

            // Dispose listener nhóm
            try { boDieuKhienNhanTinNhom.Dispose(); } catch { }
        }

        #endregion

        #region ====== 4) FLOW CHAT (flpKhungChat) ======

        /// <summary>
        /// Khi đổi size panel chat thì cập nhật width cho bubble.
        /// </summary>
        private void FlpKhungChat_SizeChanged(object sender, EventArgs e)
        {
            if (flpKhungChat == null) return;

            int w = GetBubbleWidth();
            if (w < 50) return;

            flpKhungChat.SuspendLayout();
            try
            {
                // Set lại width để bubble không bị vỡ layout
                foreach (Control c in flpKhungChat.Controls)
                {
                    c.Width = w;
                }
            }
            finally
            {
                flpKhungChat.ResumeLayout();
            }
        }

        /// <summary>
        /// Tính độ rộng bubble theo client width của FlowLayoutPanel.
        /// </summary>
        private int GetBubbleWidth()
        {
            if (flpKhungChat == null) return 300;

            int w = flpKhungChat.ClientSize.Width
                    - flpKhungChat.Padding.Left
                    - flpKhungChat.Padding.Right
                    - 5;

            return w;
        }

        /// <summary>
        /// Cuộn xuống cuối khung chat.
        /// </summary>
        private void ScrollToBottom()
        {
            if (flpKhungChat == null) return;
            if (flpKhungChat.Controls.Count == 0) return;

            Control last = flpKhungChat.Controls[flpKhungChat.Controls.Count - 1];
            flpKhungChat.ScrollControlIntoView(last);
        }

        #endregion

        #region ====== 5) NHẤN ENTER ĐỂ GỬI ======

        /// <summary>
        /// Enter = gửi, Shift+Enter = xuống dòng.
        /// </summary>
        private void TxtNhapTinNhan_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter && !e.Shift)
            {
                // Không cho xuống dòng mặc định
                e.SuppressKeyPress = true;
                // Gọi click gửi
                btnGui.PerformClick();
            }
        }

        #endregion

        #region ====== 6) LOAD DANH SÁCH BẠN BÈ / NHÓM ======

        /// <summary>
        /// Load danh sách bạn bè + nhóm và đổ lên panel trái.
        /// </summary>
        private async Task LoadUsersAsync()
        {
            pnlDanhSachChat.SuspendLayout();
            pnlDanhSachChat.Controls.Clear();

            try
            {
                // Lấy danh sách bạn bè từ controller
                tatCaNguoiDung = await boDieuKhienNhanTin.GetFriendUsersAsync(idDangNhap);

                // Load nhóm trước để hiển thị nhóm lên trên
                await LoadGroupsIntoListAsync();

                if (tatCaNguoiDung == null || tatCaNguoiDung.Count == 0)
                {
                    return;
                }

                // Add từng user vào danh sách chat
                foreach (KeyValuePair<string, User> cap in tatCaNguoiDung)
                {
                    string idNguoiDung = cap.Key; // safeId
                    User nguoiDung = cap.Value;

                    // Gắn LocalId cho model nếu thiếu
                    if (nguoiDung != null)
                    {
                        nguoiDung.LocalId = idNguoiDung;
                    }

                    AddUserItem(idNguoiDung, nguoiDung);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Lỗi tải danh sách bạn bè: " + ex.Message,
                    "Lỗi",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            finally
            {
                pnlDanhSachChat.ResumeLayout();
            }
        }

        #endregion

        #region ====== 7) ITEM DANH SÁCH CHAT (UI) ======

        /// <summary>
        /// Tạo item user trong danh sách chat (panel trái).
        /// </summary>
        private void AddUserItem(string userId, User user)
        {
            Conversations conversations = new Conversations();
            conversations.Cursor = Cursors.Hand;

            // Set tiêu đề hiển thị
            conversations.SetInfo(GetUserDisplayName(user), userId);
            conversations.Tag = userId;

            // Đảm bảo không bị gắn handler nhiều lần
            conversations.ItemClicked -= UserItem_Click;
            conversations.ItemClicked += UserItem_Click;

            // Cho phép hủy kết bạn ở item user
            conversations.picCancelRequest.Visible = true;

            // Xử lý click hủy kết bạn
            conversations.CancelClicked += async (s, e) =>
            {
                var confirm = MessageBox.Show(
                    string.Format("Bạn có chắc chắn muốn hủy kết bạn với {0}?", user.DisplayName),
                    "Xác nhận",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (confirm == DialogResult.Yes)
                {
                    try
                    {
                        // 1) Xóa lịch sử chat
                        await boDieuKhienNhanTin.DeleteFullConversationAsync(user.LocalId);

                        // 2) Hủy kết bạn
                        await _friendController.UnfriendAsync(user.LocalId);

                        // 3) Xóa item khỏi UI
                        pnlDanhSachChat.Controls.Remove(conversations);
                        conversations.Dispose();

                        MessageBox.Show("Đã hủy kết bạn thành công.");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Lỗi khi hủy kết bạn: " + ex.Message);
                    }
                }
            };

            conversations.Dock = DockStyle.Top;
            pnlDanhSachChat.Controls.Add(conversations);
        }

        /// <summary>
        /// Chuẩn hóa tên hiển thị: DisplayName -> UserName -> email prefix.
        /// </summary>
        private static string GetUserDisplayName(User user)
        {
            if (user == null)
            {
                return "Người dùng";
            }

            // Ưu tiên DisplayName
            string ten = user.DisplayName;

            // Fallback sang UserName
            if (string.IsNullOrWhiteSpace(ten))
            {
                ten = user.UserName;
            }

            // Fallback sang email prefix
            if (string.IsNullOrWhiteSpace(ten))
            {
                string email = user.Email;
                if (!string.IsNullOrWhiteSpace(email))
                {
                    int at = email.IndexOf('@');
                    ten = (at > 0) ? email.Substring(0, at) : email;
                }
            }

            if (string.IsNullOrWhiteSpace(ten))
            {
                return "Người dùng";
            }

            // Gom nhiều khoảng trắng
            ten = Regex.Replace(ten.Trim(), "\\s+", " ");

            // TitleCase (best-effort)
            try
            {
                CultureInfo vi = new CultureInfo("vi-VN");
                ten = vi.TextInfo.ToTitleCase(ten.ToLower(vi));
            }
            catch
            {
                // ignore
            }

            return ten;
        }

        #endregion

        #region ====== 8) CHỌN ITEM & MỞ CUỘC TRÒ CHUYỆN ======

        /// <summary>
        /// Click item user/group để mở cuộc chat tương ứng.
        /// </summary>
        private async void UserItem_Click(object sender, EventArgs e)
        {
            Conversations conversations = sender as Conversations;
            if (conversations == null) return;

            string idNguoiDung = conversations.Tag as string;
            if (string.IsNullOrEmpty(idNguoiDung)) return;

            // Nếu item là nhóm
            if (idNguoiDung.StartsWith(GROUP_TAG_PREFIX, StringComparison.Ordinal))
            {
                string gid = idNguoiDung.Substring(GROUP_TAG_PREFIX.Length);

                // Load avatar nhóm
                string base64nhom = await _groupService.GetAvatarGroupAsync(gid);
                picAnhDaiDienGiua.Image = ImageBase64.Base64ToImage(base64nhom);

                // Mở chat nhóm
                OpenGroupConversation(gid);
                return;
            }

            // Item là user: load avatar user
            string base64 = await _authService.GetAvatarAsync(idNguoiDung);
            picAnhDaiDienGiua.Image = ImageBase64.Base64ToImage(base64) ?? Properties.Resources.DefaultAvatar;

            // Mở chat 1-1
            OpenConversation(idNguoiDung);
        }

        /// <summary>
        /// Cập nhật trạng thái hiển thị (online/offline...) của user đang chat.
        /// </summary>
        private async void CapNhatTrangThaiNguoiDung(string otherUserId)
        {
            string id = otherUserId;
            string u = await _authService.GetStatusAsync(id);
            if (u != null)
            {
                lblTrangThai.Text = u;
            }
        }

        /// <summary>
        /// Mở cuộc trò chuyện 1-1: clear state + start listen.
        /// </summary>
        private void OpenConversation(string otherUserId)
        {
            // Set cuộc chat hiện tại
            idNguoiDangChat = otherUserId;

            // Tăng version để callback cũ tự bỏ
            _listenVersion++;

            // Chế độ 1-1
            dangChatNhom = false;

            // Stop listen nhóm nếu có
            try { boDieuKhienNhanTinNhom.StopListen(); } catch { }

            // Reset UI + state
            ClearChatUIAndState();

            // Cập nhật trạng thái user đang chat
            CapNhatTrangThaiNguoiDung(idNguoiDangChat);

            // Set tên hiển thị
            User nguoiDung;
            if (tatCaNguoiDung != null && tatCaNguoiDung.TryGetValue(otherUserId, out nguoiDung))
            {
                lblTenDangNhapGiua.Text = GetUserDisplayName(nguoiDung);
            }
            else
            {
                lblTenDangNhapGiua.Text = string.Empty;
            }

            // Capture version để chặn listener cũ
            int myVersion = _listenVersion;

            // Start listen conversation (3 callback: initial/add/reset)
            boDieuKhienNhanTin.StartListenConversation(
                otherUserId,
                onInitialLoaded: delegate (List<ChatMessage> initial)
                {
                    // Chặn callback cũ / sai cuộc chat
                    if (!IsHandleCreated) return;
                    if (myVersion != _listenVersion) return;
                    if (!string.Equals(idNguoiDangChat, otherUserId, StringComparison.Ordinal)) return;

                    try
                    {
                        BeginInvoke(new Action(async delegate
                        {
                            if (myVersion != _listenVersion) return;
                            await RenderMessagesAsync(initial, otherUserId);
                        }));
                    }
                    catch { }
                },
                onMessageAdded: delegate (ChatMessage msg)
                {
                    // Chặn callback cũ / sai cuộc chat
                    if (!IsHandleCreated) return;
                    if (myVersion != _listenVersion) return;
                    if (!string.Equals(idNguoiDangChat, otherUserId, StringComparison.Ordinal)) return;

                    // Đẩy vào batch để vẽ theo lượt
                    EnqueueIncomingMessage(msg);
                },
                onReset: delegate (List<ChatMessage> full)
                {
                    // Chặn callback cũ / sai cuộc chat
                    if (!IsHandleCreated) return;
                    if (myVersion != _listenVersion) return;
                    if (!string.Equals(idNguoiDangChat, otherUserId, StringComparison.Ordinal)) return;

                    try
                    {
                        BeginInvoke(new Action(async delegate
                        {
                            if (myVersion != _listenVersion) return;
                            await RenderMessagesAsync(full, otherUserId);
                        }));
                    }
                    catch { }
                });
        }

        #endregion

        #region ====== 9) VẼ TIN NHẮN (FLOW) ======

        /// <summary>
        /// Render danh sách tin nhắn lên UI theo thứ tự thời gian.
        /// </summary>
        private async Task RenderMessagesAsync(IList<ChatMessage> messages, string ownerUserId)
        {
            // Nếu user đã đổi cuộc chat thì bỏ qua batch này
            if (!string.Equals(idNguoiDangChat, ownerUserId, StringComparison.Ordinal))
                return;

            // Không có tin => clear UI
            if (messages == null || messages.Count == 0)
            {
                flpKhungChat.SuspendLayout();
                try
                {
                    flpKhungChat.Controls.Clear();
                    danhSachTinNhanDangVe.Clear();

                    // Reset dedupe
                    lock (_drawnLock) { _drawnMessageIds.Clear(); }
                }
                finally { flpKhungChat.ResumeLayout(); }
                return;
            }

            // Sort theo Timestamp để vẽ đúng thứ tự
            List<ChatMessage> ordered = messages
                .Where(m => m != null)
                .OrderBy(m => m.Timestamp)
                .ToList();

            int soLuongCu = danhSachTinNhanDangVe.Count;
            int soLuongMoi = ordered.Count;

            flpKhungChat.SuspendLayout();
            try
            {
                // Nếu lần đầu hoặc bị giảm số lượng => vẽ lại toàn bộ
                if (soLuongCu == 0 || soLuongMoi < soLuongCu)
                {
                    flpKhungChat.Controls.Clear();
                    danhSachTinNhanDangVe.Clear();

                    // Reset dedupe
                    lock (_drawnLock) { _drawnMessageIds.Clear(); }

                    // Vẽ toàn bộ
                    for (int i = 0; i < soLuongMoi; i++)
                    {
                        ChatMessage m = ordered[i];
                        bool added = await AddMessageBubble(m);
                        if (added)
                        {
                            InsertRenderedMessageToList(m);
                        }
                    }
                }
                else
                {
                    // Append phần tin mới
                    for (int i = soLuongCu; i < soLuongMoi; i++)
                    {
                        ChatMessage m = ordered[i];
                        bool added = await AddMessageBubble(m);
                        if (added)
                        {
                            InsertRenderedMessageToList(m);
                        }
                    }
                }
            }
            finally { flpKhungChat.ResumeLayout(); }

            // Cuộn xuống cuối sau khi render
            ScrollToBottom();
        }

        /// <summary>
        /// Tạo bubble UI cho 1 tin nhắn và chèn vào đúng vị trí theo Timestamp.
        /// </summary>
        private async Task<bool> AddMessageBubble(ChatMessage msg)
        {
            if (msg == null) return false;
            if (flpKhungChat == null) return false;
            if (this.IsDisposed) return false;

            // Dedupe ngay từ đầu: tránh vẽ trùng do listener/batch
            if (!TryMarkAsDrawn(msg))
            {
                return false;
            }

            bool isMine = msg.IsMine;

            // Lấy thông tin người gửi (để show tên khi là nhóm / tin người khác)
            string senderId = msg.SenderId;
            User thongTinNguoiGui = null;

            try
            {
                if (!string.IsNullOrWhiteSpace(senderId))
                {
                    thongTinNguoiGui = await _authService.GetUserByIdAsync(senderId);
                }
            }
            catch
            {
                // ignore (best-effort)
            }

            // Chuẩn bị tên hiển thị trên bubble
            string displayName = isMine
                ? "Bạn"
                : (thongTinNguoiGui != null && !string.IsNullOrWhiteSpace(thongTinNguoiGui.DisplayName)
                    ? thongTinNguoiGui.DisplayName
                    : "Người dùng");

            // Format thời gian
            string time = FormatTimestamp(msg.Timestamp);

            // Chuẩn hóa type
            string type = (msg.MessageType ?? string.Empty).ToLowerInvariant();

            // Tạo bubble
            MessageBubbles bubble = new MessageBubbles();
            bubble.Tag = msg;
            bubble.Margin = new Padding(0, 6, 0, 6);

            // Set width theo panel
            int w = GetBubbleWidth();
            if (w > 50) bubble.Width = w;

            // Nếu là file/image thì hiển thị tên file + size và gắn click download
            if (type == "file" || type == "image")
            {
                string ten = string.IsNullOrEmpty(msg.FileName) ? "file" : msg.FileName;
                string noiDung;

                if (msg.FileSize > 0)
                {
                    noiDung = string.Format("{0} ({1})", ten, FormatBytes(msg.FileSize));
                }
                else
                {
                    noiDung = ten;
                }

                bubble.SetMessage(displayName, noiDung, time, isMine, senderId);
                bubble.Cursor = Cursors.Hand;

                // Gắn click cho toàn bộ control con để tải file
                GanClickDeTaiFile(bubble);
            }
            else
            {
                // Text bình thường
                bubble.SetMessage(displayName, msg.Text ?? string.Empty, time, isMine, senderId);
            }

            // Add bubble vào UI (đúng thứ tự theo Timestamp)
            try
            {
                if (flpKhungChat.InvokeRequired)
                {
                    flpKhungChat.BeginInvoke(new Action(delegate
                    {
                        if (this.IsDisposed) return;

                        int insertIndex = FindInsertIndexByTimestamp(msg.Timestamp);
                        flpKhungChat.Controls.Add(bubble);
                        flpKhungChat.Controls.SetChildIndex(bubble, insertIndex);

                        ScrollToBottom();
                    }));
                }
                else
                {
                    int insertIndex = FindInsertIndexByTimestamp(msg.Timestamp);
                    flpKhungChat.Controls.Add(bubble);
                    flpKhungChat.Controls.SetChildIndex(bubble, insertIndex);

                    ScrollToBottom();
                }
            }
            catch
            {
                // ignore UI errors
            }

            return true;
        }

        #endregion

        #region ====== 10) HỖ TRỢ CHÈN ĐÚNG THỨ TỰ / FORMAT TIME ======

        /// <summary>
        /// Tìm index chèn bubble dựa trên Timestamp để UI luôn đúng thứ tự.
        /// </summary>
        private int FindInsertIndexByTimestamp(long ts)
        {
            if (flpKhungChat == null) return 0;

            for (int i = 0; i < flpKhungChat.Controls.Count; i++)
            {
                ChatMessage m = flpKhungChat.Controls[i].Tag as ChatMessage;

                // Nếu Tag không phải ChatMessage => coi như cuối
                long t = (m != null) ? m.Timestamp : long.MaxValue;

                if (t > ts) return i;
            }

            return flpKhungChat.Controls.Count;
        }

        /// <summary>
        /// Chèn msg vào danhSachTinNhanDangVe theo Timestamp (giữ list sorted).
        /// </summary>
        private void InsertRenderedMessageToList(ChatMessage msg)
        {
            if (msg == null) return;

            int idx = 0;
            while (idx < danhSachTinNhanDangVe.Count)
            {
                ChatMessage cur = danhSachTinNhanDangVe[idx];
                long t = (cur != null) ? cur.Timestamp : 0;

                if (t > msg.Timestamp) break;
                idx++;
            }

            danhSachTinNhanDangVe.Insert(idx, msg);
        }

        /// <summary>
        /// Chuyển timestamp (ms) => dd/MM/yyyy HH:mm (local time).
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

        #region ====== 11) GỬI TIN NHẮN ======

        /// <summary>
        /// Click nút gửi: gửi text cho user hoặc nhóm.
        /// </summary>
        private async void btnGui_Click(object sender, EventArgs e)
        {
            // Lấy nội dung và trim
            string noiDungTin = (txtNhapTinNhan.Text ?? string.Empty).Trim();

            // Không có nội dung => bỏ
            if (string.IsNullOrEmpty(noiDungTin))
            {
                return;
            }

            // Chưa chọn cuộc chat => báo
            if (string.IsNullOrEmpty(idNguoiDangChat))
            {
                MessageBox.Show(
                    "Vui lòng chọn người hoặc nhóm cần nhắn tin ở danh sách bên trái.",
                    "Thông báo",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return;
            }

            // Clear UI input sớm để UX mượt
            txtNhapTinNhan.Clear();

            try
            {
                if (dangChatNhom)
                {
                    // Gửi tin nhắn nhóm
                    ChatMessage sent = await boDieuKhienNhanTinNhom.SendGroupMessageAsync(idNguoiDangChat, noiDungTin);
                    if (sent != null)
                    {
                        // Listener sẽ tự append
                    }
                }
                else
                {
                    // Gửi tin nhắn 1-1
                    await boDieuKhienNhanTin.SendMessageAsync(idNguoiDangChat, noiDungTin);
                }
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

        #region ====== 12) NHÓM CHAT ======

        /// <summary>
        /// Load nhóm của user và add vào danh sách chat (hiển thị trước bạn bè).
        /// </summary>
        private async Task LoadGroupsIntoListAsync()
        {
            try
            {
                // Lấy nhóm của user
                tatCaNhom = await boDieuKhienNhanTinNhom.GetMyGroupsAsync();
                if (tatCaNhom == null) tatCaNhom = new Dictionary<string, GroupInfo>();

                // Sort nhóm theo LastMessageAt giảm dần
                List<GroupInfo> list = tatCaNhom.Values.ToList();
                list.Sort(delegate (GroupInfo a, GroupInfo b)
                {
                    long ta = a != null ? a.LastMessageAt : 0;
                    long tb = b != null ? b.LastMessageAt : 0;
                    return tb.CompareTo(ta);
                });

                // Add từng nhóm vào panel
                foreach (GroupInfo g in list)
                {
                    AddGroupItem(g);
                }
            }
            catch
            {
                // ignore (best-effort)
            }
        }

        /// <summary>
        /// Tạo item nhóm trong danh sách chat.
        /// </summary>
        private void AddGroupItem(GroupInfo g)
        {
            if (g == null) return;

            Conversations item = new Conversations();
            item.Dock = DockStyle.Top;
            item.ItemClicked += UserItem_Click;

            // Tag dạng "GROUP:{groupId}"
            item.Tag = GROUP_TAG_PREFIX + g.GroupId;

            // Tiêu đề nhóm
            string title = string.IsNullOrWhiteSpace(g.Name) ? ("Nhóm " + g.GroupId) : g.Name;

            // Subtitle: last message hoặc member count
            string subtitle = g.LastMessage;
            if (string.IsNullOrWhiteSpace(subtitle))
            {
                subtitle = g.MemberCount > 0 ? (g.MemberCount + " thành viên") : "Nhóm chat";
            }

            // Nhóm không có nút hủy kết bạn
            item.picCancelRequest.Visible = false;

            item.SetInfo(title, item.Tag as string);
            pnlDanhSachChat.Controls.Add(item);
        }

        /// <summary>
        /// Mở cuộc trò chuyện nhóm: clear state + start listen group.
        /// </summary>
        private void OpenGroupConversation(string groupId)
        {
            if (string.IsNullOrWhiteSpace(groupId)) return;

            // Set cuộc chat hiện tại
            idNguoiDangChat = groupId;

            // Tăng version để callback cũ tự bỏ
            _listenVersion++;

            // Chế độ nhóm
            dangChatNhom = true;

            // Dừng listen 1-1 để tránh append nhầm
            try { boDieuKhienNhanTin.StopListen(); } catch { }

            // Reset UI + state
            ClearChatUIAndState();

            // Set tiêu đề
            GroupInfo g;
            if (tatCaNhom != null && tatCaNhom.TryGetValue(groupId, out g) && g != null)
            {
                lblTenDangNhapGiua.Text = g.Name;
            }
            else
            {
                lblTenDangNhapGiua.Text = "Nhóm chat";
            }

            // Nhóm không hiển thị trạng thái user
            lblTrangThai.Text = string.Empty;

            // Capture version để chặn listener cũ
            int myVersion = _listenVersion;

            // Start listen group (3 callback: initial/add/reset)
            boDieuKhienNhanTinNhom.StartListenGroup(
                groupId,
                onInitialLoaded: delegate (List<ChatMessage> initial)
                {
                    // Chặn callback cũ / sai cuộc chat
                    if (this.IsDisposed) return;
                    if (myVersion != _listenVersion) return;
                    if (!string.Equals(idNguoiDangChat, groupId, StringComparison.Ordinal)) return;

                    BeginInvoke(new Action(async delegate
                    {
                        if (myVersion != _listenVersion) return;
                        await RenderMessagesAsync(initial, groupId);
                    }));
                },
                onMessageAdded: delegate (ChatMessage msg)
                {
                    // Chặn callback cũ / sai cuộc chat
                    if (this.IsDisposed) return;
                    if (myVersion != _listenVersion) return;
                    if (!string.Equals(idNguoiDangChat, groupId, StringComparison.Ordinal)) return;

                    // Đẩy vào batch để vẽ theo lượt
                    EnqueueIncomingMessage(msg);
                },
                onReset: delegate (List<ChatMessage> full)
                {
                    // Chặn callback cũ / sai cuộc chat
                    if (this.IsDisposed) return;
                    if (myVersion != _listenVersion) return;
                    if (!string.Equals(idNguoiDangChat, groupId, StringComparison.Ordinal)) return;

                    BeginInvoke(new Action(async delegate
                    {
                        if (myVersion != _listenVersion) return;
                        await RenderMessagesAsync(full, groupId);
                    }));
                });
        }

        /// <summary>
        /// Tạo nhóm mới: chọn bạn, đặt tên, avatar (nếu có) rồi tạo.
        /// </summary>
        private async void btnTaoNhom_Click(object sender, EventArgs e)
        {
            try
            {
                // Không có bạn => không tạo được nhóm
                if (tatCaNguoiDung == null || tatCaNguoiDung.Count == 0)
                {
                    MessageBox.Show("Chưa có danh sách bạn bè để tạo nhóm.", "Thông báo",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                using (TaoNhom f = new TaoNhom(tatCaNguoiDung, idDangNhap, tokenDangNhap))
                {
                    if (f.ShowDialog() != DialogResult.OK)
                    {
                        return;
                    }

                    // Lấy dữ liệu từ form tạo nhóm
                    string groupName = f.GroupName;
                    List<string> members = f.SelectedMemberIds;

                    // Avatar base64 (có thể null)
                    string avatarBase64 = null;
                    try { avatarBase64 = f.GroupAvatarBase64; } catch { avatarBase64 = null; }

                    string newGroupId;

                    // Gọi create group (có avatar thì gửi kèm)
                    if (!string.IsNullOrWhiteSpace(avatarBase64))
                    {
                        newGroupId = await boDieuKhienNhanTinNhom.CreateGroupAsync(groupName, members, avatarBase64);
                    }
                    else
                    {
                        newGroupId = await boDieuKhienNhanTinNhom.CreateGroupAsync(groupName, members, null);
                    }

                    // Reload list để thấy nhóm mới
                    await LoadUsersAsync();

                    // Auto open nhóm mới
                    if (!string.IsNullOrWhiteSpace(newGroupId))
                    {
                        OpenGroupConversation(newGroupId);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Tạo nhóm thất bại: " + ex.Message, "Lỗi",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        #endregion

        #region ====== 13) SEND FILE / TẢI FILE ======

        /// <summary>
        /// Flag chặn spam gửi file.
        /// </summary>
        private bool dangGuiFile = false;

        /// <summary>
        /// Flag chặn spam tải file.
        /// </summary>
        private bool dangTaiFile = false;

        /// <summary>
        /// HttpClient dùng chung để tải file (có decompression + user-agent).
        /// </summary>
        private static readonly HttpClient httpClient = TaoHttpClientTaiFile();

        /// <summary>
        /// Tạo HttpClient tải file theo cấu hình ổn định.
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
        /// Click icon gửi file: chọn file rồi gửi (1-1 hoặc nhóm).
        /// </summary>
        private async void PicSendFile_Click(object sender, EventArgs e)
        {
            if (dangGuiFile) return;
            dangGuiFile = true;

            try
            {
                // Chưa chọn cuộc chat
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

                    // Gửi theo chế độ hiện tại
                    if (dangChatNhom)
                    {
                        ChatMessage sent = await boDieuKhienNhanTinNhom.SendGroupAttachmentMessageAsync(idNguoiDangChat, ofd.FileName);
                        if (sent != null)
                        {
                            // listener sẽ tự append
                        }
                    }
                    else
                    {
                        await boDieuKhienNhanTin.SendAttachmentMessageAsync(idNguoiDangChat, ofd.FileName);
                    }
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
        /// Format bytes => B/KB/MB/GB.
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

            return string.Format("{0:0.##} {1}", b, u[i]);
        }

        /// <summary>
        /// Gắn click tải file cho root và tất cả child controls.
        /// </summary>
        private void GanClickDeTaiFile(Control root)
        {
            if (root == null) return;

            root.Click -= BubbleFile_Click;
            root.Click += BubbleFile_Click;

            foreach (Control child in root.Controls)
            {
                GanClickDeTaiFile(child);
            }
        }

        /// <summary>
        /// Tìm ChatMessage từ Tag của control hoặc parent của nó.
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
        /// Click bubble file: mở SaveFileDialog và tải file theo FileUrl.
        /// </summary>
        private async void BubbleFile_Click(object sender, EventArgs e)
        {
            if (dangTaiFile) return;
            dangTaiFile = true;

            try
            {
                // Lấy msg từ Tag
                ChatMessage msg = LayChatMessageTuTag(sender as Control);

                if (msg == null)
                {
                    MessageBox.Show("Không lấy được dữ liệu file (Tag bị thiếu).");
                    return;
                }

                // Không có URL => không tải được
                if (string.IsNullOrWhiteSpace(msg.FileUrl))
                {
                    MessageBox.Show("Tin nhắn này không có URL để tải file.");
                    return;
                }

                using (SaveFileDialog sfd = new SaveFileDialog())
                {
                    // Gợi ý thư mục Downloads
                    string downloads = Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                        "Downloads");
                    if (Directory.Exists(downloads))
                    {
                        sfd.InitialDirectory = downloads;
                    }

                    // Tên file gợi ý
                    sfd.FileName = string.IsNullOrEmpty(msg.FileName) ? "download.bin" : msg.FileName;
                    sfd.OverwritePrompt = true;

                    if (sfd.ShowDialog() != DialogResult.OK)
                    {
                        return;
                    }

                    // Tải và ghi file
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

        #region ====== 14) MỞ FORM KHÁC ======

        /// <summary>
        /// Mở form tìm kiếm bạn bè.
        /// </summary>
        private void btnSearchFriends_Click(object sender, EventArgs e)
        {
            TimKiemBanBe timKiemBanBe = new TimKiemBanBe(idDangNhap, tokenDangNhap);
            timKiemBanBe.Show();
        }

        /// <summary>
        /// Mở form lời mời kết bạn.
        /// </summary>
        private void btnRequest_Click(object sender, EventArgs e)
        {
            FormLoiMoiKetBan formLoiMoiKetBan = new FormLoiMoiKetBan(idDangNhap, tokenDangNhap);
            formLoiMoiKetBan.Show();
        }

        /// <summary>
        /// Mở form quản lý nhóm (chỉ khi đang chat nhóm).
        /// </summary>
        private async void btnQuanLyNhom_Click(object sender, EventArgs e)
        {
            if (!dangChatNhom || string.IsNullOrWhiteSpace(idNguoiDangChat))
            {
                MessageBox.Show("Bạn phải mở 1 nhóm trước.");
                return;
            }

            string gid = idNguoiDangChat;

            GroupInfo g = null;
            if (tatCaNhom != null) tatCaNhom.TryGetValue(gid, out g);

            using (QuanLyNhom f = new QuanLyNhom(tatCaNguoiDung, idNguoiDangChat, idDangNhap, tokenDangNhap))
            {
                if (f.ShowDialog(this) == DialogResult.OK)
                {
                    // Reload danh sách để update tên/last message/member...
                    await LoadUsersAsync();

                    // Update title (nếu đổi tên nhóm)
                    if (!string.IsNullOrWhiteSpace(f.GroupName))
                        lblTenDangNhapGiua.Text = f.GroupName;

                    // Update avatar (nếu đổi avatar)
                    if (!string.IsNullOrWhiteSpace(f.GroupAvatarBase64))
                        picAnhDaiDienGiua.Image = ImageBase64.Base64ToImage(f.GroupAvatarBase64) ?? Properties.Resources.DefaultAvatar;
                    else
                        picAnhDaiDienGiua.Image = Properties.Resources.DefaultAvatar;
                }
            }
        }

        /// <summary>
        /// Mở form thông tin bạn bè (chỉ khi chat 1-1).
        /// </summary>
        private void btnThongTinBanBe_Click(object sender, EventArgs e)
        {
            if (dangChatNhom) return;

            if (string.IsNullOrWhiteSpace(idNguoiDangChat))
            {
                MessageBox.Show("Bạn chưa chọn bạn bè để xem thông tin.");
                return;
            }

            // Lấy user đã cache (nếu có)
            User u = null;
            if (tatCaNguoiDung != null)
            {
                tatCaNguoiDung.TryGetValue(idNguoiDangChat, out u);
            }

            using (ThongTinBanBe f = new ThongTinBanBe(idNguoiDangChat, u, idDangNhap))
            {
                f.ShowDialog(this);
            }
        }

        #endregion

        #region ====== 15) EMOJI ======

        /// <summary>
        /// Mở form emoji và chèn mã emoji vào textbox.
        /// </summary>
        private void picEmoji_Click(object sender, EventArgs e)
        {
            FormEmoji frm = new FormEmoji();

            // Callback nhận emoji từ FormEmoji
            frm.OnEmojiSelected = delegate (string emojiCode)
            {
                // Append theo format :code:
                txtNhapTinNhan.AppendText(string.Format(" :{0}: ", emojiCode));

                // Trả focus về ô nhập
                txtNhapTinNhan.Focus();
                txtNhapTinNhan.SelectionStart = txtNhapTinNhan.Text.Length;
            };

            // Đặt vị trí hiển thị ngay phía trên icon emoji
            Point pt = picEmoji.PointToScreen(Point.Empty);
            frm.StartPosition = FormStartPosition.Manual;
            frm.Location = new Point(pt.X - (frm.Width / 2) + (picEmoji.Width / 2), pt.Y - frm.Height - 10);

            frm.Show();
        }

        #endregion

        #region ====== 16) BATCH ADD (PENDING) ======

        /// <summary>
        /// Đảm bảo timer batch đã được tạo và chạy.
        /// </summary>
        private void EnsureFlushTimer()
        {
            if (_flushTimer != null) return;

            _flushTimer = new System.Windows.Forms.Timer();
            _flushTimer.Interval = 80; // gom tin nhanh trong 80ms
            _flushTimer.Tick += (s, e) => FlushPendingAdds();
            _flushTimer.Start();
        }

        /// <summary>
        /// Thêm tin vào hàng đợi, chờ flush batch.
        /// </summary>
        private void EnqueueIncomingMessage(ChatMessage msg)
        {
            if (msg == null) return;

            lock (_pendingLock)
            {
                _pendingAdds.Add(msg);
            }

            EnsureFlushTimer();
        }

        /// <summary>
        /// Lấy batch tin pending, sort theo Timestamp và render một lượt.
        /// </summary>
        private void FlushPendingAdds()
        {
            if (this.IsDisposed) return;
            if (!IsHandleCreated) return;

            List<ChatMessage> batch = null;

            lock (_pendingLock)
            {
                if (_pendingAdds.Count == 0) return;
                batch = new List<ChatMessage>(_pendingAdds);
                _pendingAdds.Clear();
            }

            // Sort theo thời gian để đảm bảo đúng thứ tự
            batch.Sort(delegate (ChatMessage a, ChatMessage b)
            {
                long ta = a != null ? a.Timestamp : 0;
                long tb = b != null ? b.Timestamp : 0;
                return ta.CompareTo(tb);
            });

            try
            {
                BeginInvoke(new Action(async delegate
                {
                    // Vẽ từng tin (có dedupe)
                    for (int i = 0; i < batch.Count; i++)
                    {
                        ChatMessage msg = batch[i];
                        bool added = await AddMessageBubble(msg);
                        if (added)
                        {
                            InsertRenderedMessageToList(msg);
                        }
                    }

                    ScrollToBottom();
                }));
            }
            catch { }
        }

        #endregion

        #region ====== 17) RESET STATE + DEDUPE ======

        /// <summary>
        /// Reset UI chat + state: pending, dedupe, list rendered.
        /// Gọi mỗi khi đổi cuộc chat để tránh append nhầm / trùng.
        /// </summary>
        private void ClearChatUIAndState()
        {
            // Clear pending batch
            lock (_pendingLock)
            {
                _pendingAdds.Clear();
            }

            // Reset dedupe set
            lock (_drawnLock)
            {
                _drawnMessageIds.Clear();
            }

            // Reset UI
            try
            {
                if (flpKhungChat != null)
                {
                    flpKhungChat.SuspendLayout();
                    try
                    {
                        flpKhungChat.Controls.Clear();
                    }
                    finally
                    {
                        flpKhungChat.ResumeLayout();
                    }
                }
            }
            catch { }

            // Reset list rendered
            danhSachTinNhanDangVe.Clear();
        }

        /// <summary>
        /// Build key để dedupe. Ưu tiên MessageId (Firebase push key).
        /// </summary>
        private static string BuildMessageKey(ChatMessage msg)
        {
            if (msg == null) return string.Empty;

            // Ưu tiên MessageId nếu có
            if (!string.IsNullOrWhiteSpace(msg.MessageId))
            {
                return msg.MessageId.Trim();
            }

            // Fallback: ghép các trường đặc trưng
            return string.Format("{0}|{1}|{2}|{3}|{4}|{5}|{6}|{7}",
                msg.Timestamp,
                msg.SenderId ?? string.Empty,
                msg.ReceiverId ?? string.Empty,
                (msg.MessageType ?? string.Empty).ToLowerInvariant(),
                msg.Text ?? string.Empty,
                msg.FileUrl ?? string.Empty,
                msg.FileName ?? string.Empty,
                msg.FileSize);
        }

        /// <summary>
        /// Đánh dấu tin đã vẽ. Trả false nếu tin đã tồn tại trong set.
        /// </summary>
        private bool TryMarkAsDrawn(ChatMessage msg)
        {
            string key = BuildMessageKey(msg);

            // Không có key => cho qua (best-effort)
            if (string.IsNullOrWhiteSpace(key)) return true;

            lock (_drawnLock)
            {
                if (_drawnMessageIds.Contains(key))
                {
                    return false;
                }

                _drawnMessageIds.Add(key);
                return true;
            }
        }

        #endregion
    }
}
