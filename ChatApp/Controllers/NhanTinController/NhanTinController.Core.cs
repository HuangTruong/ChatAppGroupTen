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
    // View interface
    public interface INhanTinView
    {
        FlowLayoutPanel DanhSachChatPanel { get; }
        FlowLayoutPanel KhungChatPanel { get; }
        Guna2TextBox TxtNhapTin { get; }
        Label LblTieuDeGiua { get; }
        Label LblTenDangNhapPhai { get; }
        Label LblTyping { get; }

        string CurrentSearchKeyword { get; }

        void ShowInfo(string message);
        DialogResult ShowConfirm(string message, string title);
    }

    // CORE – field, ctor, Init, Dispose
    public partial class NhanTinController : IDisposable
    {
        private readonly INhanTinView _view;
        private readonly string _tenNguoiDung;  // username trong Firebase

        private readonly IFirebaseClient _firebase;
        private readonly AuthService _authService;
        private readonly ChatService _chatService;
        private readonly GroupService _groupService;
        private readonly FriendService _friendService;
        private readonly StatusService _statusService;
        private readonly TypingService _typingService;

        private readonly MessageRenderQueue _renderQueue;

        private string _tenDoiPhuong = string.Empty;
        private string _groupId = string.Empty;
        private bool _isGroupChat = false;

        // Lưu id tin nhắn từng cuộc trò chuyện
        private readonly Dictionary<string, HashSet<string>> _idsTheoDoanChat =
            new Dictionary<string, HashSet<string>>();
        private readonly Dictionary<string, List<string>> _thuTuTheoDoanChat =
            new Dictionary<string, List<string>>();

        // Snapshot friend state
        private readonly HashSet<string> _lastBanBe =
            new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private readonly HashSet<string> _lastDaMoi =
            new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private readonly HashSet<string> _lastMoiDen =
            new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        // Cache trạng thái online/offline: key = id trong node status
        private readonly Dictionary<string, string> _statusCache =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        // FireSharp streaming (realtime)
        private EventStreamResponse _friendsStream;
        private EventStreamResponse _pendingForMeStream;
        private EventStreamResponse _pendingAllStream;
        private EventStreamResponse _statusStream;

        // Stream realtime cho cuộc chat 1-1 hiện tại
        private EventStreamResponse _chatStream;
        private bool _isSyncingChatRealtime = false;

        // marshal về UI thread
        private readonly SynchronizationContext _uiContext;

        private bool _isBuildingUserList = false;
        private string _currentSearchKeyword = string.Empty;

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

            _view.KhungChatPanel.FlowDirection = FlowDirection.TopDown;
            _view.KhungChatPanel.WrapContents = false;
            _view.KhungChatPanel.AutoScroll = true;
            _view.KhungChatPanel.EnableDoubleBuffer();

            _renderQueue = new MessageRenderQueue(
                _view.KhungChatPanel,
                CreateBubbleForCurrentContext,
                80,
                300);

            _view.TxtNhapTin.KeyDown += TxtNhapTin_KeyDown;

            _uiContext = SynchronizationContext.Current ?? new SynchronizationContext();
        }

        // ================== INIT ==================

        public async Task InitAsync()
        {
            await _authService.UpdateStatusAsync(_tenNguoiDung, "online");

            await RefreshFriendStatesSnapshotAsync();
            await RefreshStatusCacheAsync();

            await TaiDanhSachNguoiDungAsync();
            await TaiDanhSachNhomAsync();

            await SetupRealtimeListenersAsync();
        }

        // ================== STATUS & DISPOSE ==================

        public async Task SetOfflineAsync()
        {
            await _authService.UpdateStatusAsync(_tenNguoiDung, "offline");
        }

        public void Dispose()
        {
            _renderQueue?.Dispose();
            _friendsStream?.Dispose();
            _pendingForMeStream?.Dispose();
            _pendingAllStream?.Dispose();
            _statusStream?.Dispose();
            _chatStream?.Dispose();
        }
    }
}
