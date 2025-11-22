using ChatApp.Helpers.Ui;
using ChatApp.Models.Chat;
using ChatApp.Models.Users;
using ChatApp.Services.Auth;
using ChatApp.Services.Chat;
using ChatApp.Services.Firebase;
using ChatApp.Services.Status;
using FireSharp.Interfaces;
using FireSharp.Response;
using Guna.UI2.WinForms;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ChatApp.Controllers
{
    #region ======== INhanTinView – Interface cho View nhắn tin ========

    /// <summary>
    /// Interface đại diện cho View nhắn tin (Form / UserControl màn hình Chat):
    /// - Cung cấp các control chính (danh sách chat, khung hội thoại, ô nhập, label tiêu đề...).
    /// - Cung cấp các hàm hiển thị thông báo / xác nhận cho người dùng.
    /// - Expose từ khóa tìm kiếm hiện tại để controller có thể lọc / build danh sách.
    /// </summary>
    public interface INhanTinView
    {
        /// <summary>
        /// Panel chứa danh sách các đoạn chat (bạn bè, nhóm, người lạ...).
        /// </summary>
        FlowLayoutPanel DanhSachChatPanel { get; }

        /// <summary>
        /// Panel chứa các bubble tin nhắn của cuộc hội thoại hiện tại.
        /// </summary>
        FlowLayoutPanel KhungChatPanel { get; }

        /// <summary>
        /// Ô nhập nội dung tin nhắn.
        /// </summary>
        Guna2TextBox TxtNhapTin { get; }

        /// <summary>
        /// Label tiêu đề giữa (hiển thị tên đối phương / tên nhóm).
        /// </summary>
        Label LblTieuDeGiua { get; }

        /// <summary>
        /// Label hiển thị tên đăng nhập ở panel bên phải.
        /// </summary>
        Label LblTenDangNhapPhai { get; }

        /// <summary>
        /// Label hiển thị trạng thái "đang nhập..." (typing indicator).
        /// </summary>
        Label LblTyping { get; }


        /// <summary>
        /// Từ khóa tìm kiếm hiện tại để lọc danh sách người dùng / đoạn chat.
        /// </summary>  
        string CurrentSearchKeyword { get; }
        /// <summary>
        /// Hiển thị thông báo đơn giản cho người dùng (MessageBox dạng thông tin).
        /// </summary>
        /// <param name="message">Nội dung thông báo.</param>
        void ShowInfo(string message);
        /// <summary>
        /// Hiển thị dialog xác nhận (OK/Cancel, Yes/No...).
        /// </summary>
        /// <param name="message">Nội dung câu hỏi.</param>
        /// <param name="title">Tiêu đề hộp thoại.</param>
        /// <returns>Kết quả người dùng chọn.</returns>
        DialogResult ShowConfirm(string message, string title);
    }

    #endregion

    #region ======== NhanTinController – Core (field, ctor, init, dispose) ========

    /// <summary>
    /// Controller xử lý toàn bộ logic màn hình nhắn tin:
    /// - Quản lý danh sách bạn bè, nhóm, lời mời kết bạn và trạng thái online/offline.
    /// - Quản lý state cuộc trò chuyện hiện tại (1-1 hoặc nhóm, ID đoạn chat, thứ tự tin nhắn).
    /// - Khởi tạo các service Firebase (Auth, Chat, Group, Friend, Status, Typing).
    /// - Thiết lập luồng render tin nhắn và lắng nghe sự kiện realtime từ Firebase.
    /// - Hỗ trợ cập nhật trạng thái online/offline khi người dùng vào/ra màn hình chat.
    /// </summary>
    public partial class NhanTinController : IDisposable
    {
        #region ======== Trường / Services / State chính ========

        /// <summary>
        /// View nhắn tin (Form/UserControl) được controller điều khiển.
        /// </summary>
        private readonly INhanTinView _view;

        /// <summary>
        /// Tên người dùng hiện tại (username trong Firebase).
        /// </summary>
        private readonly string _tenNguoiDung;

        /// <summary>
        /// Client Firebase dùng chung cho các service.
        /// </summary>
        private readonly IFirebaseClient _firebase;

        /// <summary>
        /// Service xử lý xác thực và trạng thái online/offline.
        /// </summary>
        private readonly AuthService _authService;

        /// <summary>
        /// Service xử lý gửi/nhận tin nhắn (1-1 và group).
        /// </summary>
        private readonly ChatService _chatService;

        /// <summary>
        /// Service xử lý thông tin nhóm (thành viên, quyền, gửi tin nhóm...).
        /// </summary>
        private readonly GroupService _groupService;

        /// <summary>
        /// Service xử lý bạn bè và lời mời kết bạn.
        /// </summary>
        private readonly FriendService _friendService;

        /// <summary>
        /// Service xử lý trạng thái online/offline của người dùng.
        /// </summary>
        private readonly StatusService _statusService;

        /// <summary>
        /// Service xử lý trạng thái "đang nhập..." (typing).
        /// </summary>
        private readonly TypingService _typingService;

        /// <summary>
        /// Hàng đợi render tin nhắn để vẽ bubble mượt hơn, tránh giật lag UI.
        /// </summary>
        private readonly MessageRenderQueue _renderQueue;

        /// <summary>
        /// Tên đối phương trong cuộc trò chuyện 1-1 hiện tại.
        /// </summary>
        private string _tenDoiPhuong = string.Empty;

        /// <summary>
        /// ID nhóm đang chat (nếu là chat nhóm), rỗng nếu đang ở chat 1-1.
        /// </summary>
        private string _groupId = string.Empty;

        /// <summary>
        /// Cờ cho biết đang ở chế độ chat nhóm hay không.
        /// </summary>
        private bool _isGroupChat = false;

        /// <summary>
        /// Lưu danh sách ID tin nhắn theo từng đoạn chat (key = CID / groupId).
        /// Dùng để tránh thêm trùng tin nhắn khi sync realtime.
        /// </summary>
        private readonly Dictionary<string, HashSet<string>> _idsTheoDoanChat =
            new Dictionary<string, HashSet<string>>();

        /// <summary>
        /// Lưu thứ tự ID tin nhắn theo từng đoạn chat (key = CID / groupId).
        /// </summary>
        private readonly Dictionary<string, List<string>> _thuTuTheoDoanChat =
            new Dictionary<string, List<string>>();

        /// <summary>
        /// Snapshot danh sách bạn bè lần gần nhất (dùng để so sánh thay đổi).
        /// </summary>
        private readonly HashSet<string> _lastBanBe =
            new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Snapshot danh sách những người mình đã gửi lời mời kết bạn.
        /// </summary>
        private readonly HashSet<string> _lastDaMoi =
            new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Snapshot danh sách lời mời kết bạn gửi đến mình.
        /// </summary>
        private readonly HashSet<string> _lastMoiDen =
            new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Cache trạng thái online/offline:
        /// key = id trong node status, value = trạng thái hiện tại.
        /// </summary>
        private readonly Dictionary<string, string> _statusCache =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Stream realtime danh sách bạn bè từ Firebase.
        /// </summary>
        private EventStreamResponse _friendsStream;

        /// <summary>
        /// Stream realtime các yêu cầu kết bạn gửi đến mình.
        /// </summary>
        private EventStreamResponse _pendingForMeStream;

        /// <summary>
        /// Stream realtime các yêu cầu kết bạn mình đã gửi.
        /// </summary>
        private EventStreamResponse _pendingAllStream;

        /// <summary>
        /// Stream realtime trạng thái online/offline.
        /// </summary>
        private EventStreamResponse _statusStream;

        /// <summary>
        /// Stream realtime cho cuộc trò chuyện 1-1 hiện tại.
        /// </summary>
        private EventStreamResponse _chatStream;

        /// <summary>
        /// Cờ tránh sync chồng chéo khi cập nhật tin nhắn realtime.
        /// </summary>
        private bool _isSyncingChatRealtime = false;

        /// <summary>
        /// SynchronizationContext dùng để marshal callback về đúng UI thread.
        /// </summary>
        private readonly SynchronizationContext _uiContext;

        /// <summary>
        /// Cờ cho biết hệ thống đang build lại danh sách người dùng (để tránh double-build).
        /// </summary>
        private bool _isBuildingUserList = false;

        /// <summary>
        /// Từ khóa tìm kiếm hiện tại (cache lại từ View).
        /// </summary>
        private string _currentSearchKeyword = string.Empty;

        #endregion

        #region ======== Constructor ========

        /// <summary>
        /// Khởi tạo controller nhắn tin:
        /// - Gán view và username, kiểm tra null/empty.
        /// - Tạo Firebase client và các service liên quan (Auth, Chat, Group, Friend, Status, Typing).
        /// - Cấu hình lại <see cref="INhanTinView.KhungChatPanel"/> (FlowDirection, AutoScroll, DoubleBuffer).
        /// - Khởi tạo <see cref="MessageRenderQueue"/> để render bubble mượt.
        /// - Đăng ký event KeyDown cho ô nhập tin.
        /// - Lấy <see cref="SynchronizationContext.Current"/> phục vụ cho callback realtime.
        /// </summary>
        /// <param name="view">View màn hình nhắn tin.</param>
        /// <param name="tenNguoiDung">Tên người dùng hiện tại (username trong Firebase).</param>
        public NhanTinController(INhanTinView view, string tenNguoiDung)
        {
            _view = view ?? throw new ArgumentNullException(nameof(view));
            _tenNguoiDung = !string.IsNullOrWhiteSpace(tenNguoiDung)
                ? tenNguoiDung
                : throw new ArgumentNullException(nameof(tenNguoiDung));

            _firebase = FirebaseClientFactory.Create();
            _authService = new AuthService(_firebase);
            _chatService = new ChatService(_firebase);
            _groupService = new GroupService(_firebase);
            _friendService = new FriendService(_firebase, _tenNguoiDung);
            _statusService = new StatusService(_firebase);
            _typingService = new TypingService(_firebase);

            _view.LblTenDangNhapPhai.Text = _tenNguoiDung;

            _view.KhungChatPanel.EnableDoubleBuffer();

            _renderQueue = new MessageRenderQueue(_view.KhungChatPanel, CreateBubbleForCurrentContext, 80, 300);

            _view.TxtNhapTin.KeyDown += TxtNhapTin_KeyDown;  // Xử lý phím Enter

            _uiContext = SynchronizationContext.Current ?? new SynchronizationContext();
        }

        #endregion

        #region ======== INIT – Khởi tạo dữ liệu & realtime ========

        /// <summary>
        /// Khởi tạo controller sau khi tạo xong:
        /// - Cập nhật trạng thái người dùng thành "online".
        /// - Refresh snapshot bạn bè và cache trạng thái online/offline.
        /// - Tải danh sách người dùng (bạn bè, người lạ, lời mời...) và danh sách nhóm.
        /// - Thiết lập các listener realtime từ Firebase (bạn bè, pending, status...).
        /// </summary>
        public async Task InitAsync()
        {
            await _authService.UpdateStatusAsync(_tenNguoiDung, "online");

            await RefreshFriendStatesSnapshotAsync();
            await RefreshStatusCacheAsync();

            await TaiDanhSachNguoiDungAsync();
            await TaiDanhSachNhomAsync();

            await SetupRealtimeListenersAsync();
        }

        #endregion

        #region ======== STATUS & DISPOSE ========

        /// <summary>
        /// Cập nhật trạng thái người dùng thành "offline" khi thoát màn hình chat / ứng dụng.
        /// </summary>
        public async Task SetOfflineAsync()
        {
            await _authService.UpdateStatusAsync(_tenNguoiDung, "offline");
        }

        /// <summary>
        /// Giải phóng tài nguyên:
        /// - Hủy hàng đợi render.
        /// - Hủy toàn bộ stream realtime (bạn bè, pending, status, chat).
        /// </summary>
        public void Dispose()
        {
            _renderQueue?.Dispose();
            _friendsStream?.Dispose();
            _pendingForMeStream?.Dispose();
            _pendingAllStream?.Dispose();
            _statusStream?.Dispose();
            _chatStream?.Dispose();
        }

        #endregion
    }

    #endregion
}
