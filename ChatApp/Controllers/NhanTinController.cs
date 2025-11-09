using ChatApp.Helpers.Ui;
using ChatApp.Helpers;
using ChatApp.Models.Chat;
using ChatApp.Services.Auth;
using ChatApp.Services.Chat;
using ChatApp.Services.Firebase;
using ChatApp.Services.Status;
using ChatApp.Models.Users;
using FireSharp.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ChatApp.Controllers
{
    // Giao diện View để form NhanTin kế thừa
    public interface INhanTinView
    {
        FlowLayoutPanel DanhSachChatPanel { get; }
        FlowLayoutPanel KhungChatPanel { get; }
        TextBox TxtNhapTin { get; }
        Label LblTieuDeGiua { get; }
        Label LblTenDangNhapPhai { get; }
        Label LblTyping { get; }

        void ShowInfo(string message);
        DialogResult ShowConfirm(string message, string title);
    }

    public class NhanTinController : IDisposable
    {
        private readonly INhanTinView _view;
        private readonly string _tenHienTai;

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

        // Lưu thứ tự tin nhắn từng cuộc trò chuyện
        private readonly Dictionary<string, List<string>> _thuTuTheoDoanChat =
            new Dictionary<string, List<string>>();

        // Khởi tạo controller
        public NhanTinController(INhanTinView view, string tenHienTai)
        {
            if (view == null) throw new ArgumentNullException("view");
            if (string.IsNullOrEmpty(tenHienTai)) throw new ArgumentNullException("tenHienTai");

            _view = view;
            _tenHienTai = tenHienTai;

            // Tạo dịch vụ Firebase và các service cần dùng
            _firebase = FirebaseClientFactory.Create();
            _authService = new AuthService(_firebase);
            _chatService = new ChatService(_firebase);
            _groupService = new GroupService(_firebase);
            _friendService = new FriendService(_firebase, _tenHienTai);
            _statusService = new StatusService(_firebase);
            _typingService = new TypingService(_firebase);

            _view.LblTenDangNhapPhai.Text = _tenHienTai;

            // Thiết lập khung chat
            _view.KhungChatPanel.FlowDirection = FlowDirection.TopDown;
            _view.KhungChatPanel.WrapContents = false;
            _view.KhungChatPanel.AutoScroll = true;
            _view.KhungChatPanel.EnableDoubleBuffer();

            // Hàng đợi render bong bóng chat
            _renderQueue = new MessageRenderQueue(
                _view.KhungChatPanel,
                CreateBubbleForCurrentContext,
                80,
                300);

            // Bắt sự kiện nhấn phím trong ô nhập
            _view.TxtNhapTin.KeyDown += TxtNhapTin_KeyDown;
        }

        // Xử lý Enter trong ô nhập
        private void TxtNhapTin_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter && !e.Shift && !e.Control)
            {
                e.SuppressKeyPress = true;
                GuiTinNhanHienTaiAsync();
            }
        }

        // Gọi khi form load
        public async Task InitAsync()
        {
            await _statusService.UpdateAsync(_tenHienTai, "online");
            await TaiDanhSachNguoiDungAsync();
            await TaiDanhSachNhomAsync();
        }

        // =============== DANH SÁCH NGƯỜI DÙNG ===============

        // Tải danh sách người dùng và tạo nút tương ứng
        public async Task TaiDanhSachNguoiDungAsync()
        {
            var res = await _firebase.GetAsync("users");
            Dictionary<string, User> data = res.ResultAs<Dictionary<string, User>>();

            var friendStates = await _friendService.LoadFriendStatesAsync();
            HashSet<string> banBe = friendStates.BanBe;
            HashSet<string> daMoi = friendStates.DaMoi;
            HashSet<string> moiDen = friendStates.MoiDen;

            Dictionary<string, string> status = await _statusService.GetAllAsync();

            // Xoá các nút user cũ
            List<Control> xoaUser = new List<Control>();
            foreach (Control c in _view.DanhSachChatPanel.Controls)
            {
                if (c is Button btn && btn.Tag is string tag && !tag.StartsWith("group:"))
                    xoaUser.Add(btn);
            }
            foreach (Control c in xoaUser)
            {
                _view.DanhSachChatPanel.Controls.Remove(c);
                c.Dispose();
            }

            if (data == null || data.Count == 0)
            {
                if (_view.DanhSachChatPanel.Controls.Count == 0)
                {
                    Label lbl = new Label();
                    lbl.Text = "Không có người dùng nào.";
                    lbl.AutoSize = true;
                    _view.DanhSachChatPanel.Controls.Add(lbl);
                }
                return;
            }

            // Tạo nút người dùng
            foreach (var kv in data)
            {
                User u = kv.Value;
                if (u == null || u.Ten.Equals(_tenHienTai, StringComparison.OrdinalIgnoreCase))
                    continue;

                UserListItem item = new UserListItem
                {
                    TenHienThi = u.Ten,
                    LaBanBe = banBe.Contains(u.Ten),
                    DaGuiLoiMoi = daMoi.Contains(u.Ten),
                    MoiKetBanChoMinh = moiDen.Contains(u.Ten),
                    Online = status.TryGetValue(u.Ten.Replace('.', '_'), out var st) && st == "online"
                };

                Button btn = TaoNutUser(item);
                _view.DanhSachChatPanel.Controls.Add(btn);
            }
        }

        // Tạo nút người dùng với menu chuột phải
        private Button TaoNutUser(UserListItem item)
        {
            string prefix = item.Online ? "(online) " : "(offline) ";
            string state = item.LaBanBe ? " (Bạn bè)" :
                           item.DaGuiLoiMoi ? " (Đã mời)" :
                           item.MoiKetBanChoMinh ? " (Mời bạn)" : "";

            Button btn = new Button
            {
                Text = prefix + item.TenHienThi + state,
                Tag = item.TenHienThi,
                Width = _view.DanhSachChatPanel.Width - 25,
                Height = 40,
                TextAlign = System.Drawing.ContentAlignment.MiddleLeft,
                BackColor = System.Drawing.Color.WhiteSmoke,
                FlatStyle = FlatStyle.Flat
            };

            ContextMenuStrip menu = new ContextMenuStrip();

            // Tuỳ trạng thái hiển thị menu phù hợp
            if (!item.LaBanBe && !item.DaGuiLoiMoi && !item.MoiKetBanChoMinh)
                menu.Items.Add("Kết bạn", null, async delegate { await _friendService.GuiLoiMoiAsync(item.TenHienThi); await TaiDanhSachNguoiDungAsync(); _view.ShowInfo("Đã gửi lời mời."); });

            if (item.MoiKetBanChoMinh)
                menu.Items.Add("Chấp nhận kết bạn", null, async delegate { await _friendService.ChapNhanAsync(item.TenHienThi); await TaiDanhSachNguoiDungAsync(); _view.ShowInfo("Đã trở thành bạn bè."); });

            if (item.LaBanBe)
                menu.Items.Add("Huỷ kết bạn", null, async delegate
                {
                    DialogResult r = _view.ShowConfirm($"Huỷ kết bạn với {item.TenHienThi}?", "Xác nhận");
                    if (r == DialogResult.Yes)
                    {
                        await _friendService.HuyKetBanAsync(item.TenHienThi);
                        await TaiDanhSachNguoiDungAsync();
                    }
                });

            btn.ContextMenuStrip = menu;

            // Mở chat khi click
            btn.Click += async delegate { await MoChat1_1Async(item.TenHienThi); };

            return btn;
        }

        // =============== DANH SÁCH NHÓM ===============

        // Tải danh sách nhóm
        public async Task TaiDanhSachNhomAsync()
        {
            Dictionary<string, Nhom> data = await _groupService.GetAllAsync();

            // Xoá nút nhóm cũ
            List<Control> xoaNhom = new List<Control>();
            foreach (Control c in _view.DanhSachChatPanel.Controls)
            {
                if (c is Button btn && btn.Tag is string tag && tag.StartsWith("group:"))
                    xoaNhom.Add(btn);
            }
            foreach (Control c in xoaNhom)
            {
                _view.DanhSachChatPanel.Controls.Remove(c);
                c.Dispose();
            }

            if (data == null || data.Count == 0)
                return;

            // Tạo nút nhóm
            foreach (var kv in data)
            {
                string id = kv.Key;
                Nhom nhom = kv.Value;
                if (nhom?.thanhVien == null || !nhom.thanhVien.ContainsKey(_tenHienTai))
                    continue;

                string tenHienThi = string.IsNullOrEmpty(nhom.tenNhom) ? id : nhom.tenNhom;

                Button btn = new Button
                {
                    Text = "[Nhóm] " + tenHienThi,
                    Tag = "group:" + id,
                    Width = _view.DanhSachChatPanel.Width - 25,
                    Height = 40,
                    TextAlign = System.Drawing.ContentAlignment.MiddleLeft,
                    BackColor = System.Drawing.Color.LightYellow,
                    FlatStyle = FlatStyle.Flat
                };

                ContextMenuStrip menu = new ContextMenuStrip();
                menu.Items.Add("Thêm thành viên", null, async delegate { _view.ShowInfo("Chức năng đang phát triển."); await Task.CompletedTask; });

                if (!string.IsNullOrEmpty(nhom.taoBoi) && nhom.taoBoi.Equals(_tenHienTai, StringComparison.OrdinalIgnoreCase))
                    menu.Items.Add("Xoá nhóm", null, async delegate
                    {
                        DialogResult r = _view.ShowConfirm($"Xoá nhóm \"{tenHienThi}\"?", "Xác nhận");
                        if (r == DialogResult.Yes)
                        {
                            await _groupService.DeleteGroupAsync(id);
                            await TaiDanhSachNhomAsync();
                        }
                    });

                btn.ContextMenuStrip = menu;
                btn.Click += async delegate { await MoChatNhomAsync(id, tenHienThi); };
                _view.DanhSachChatPanel.Controls.Add(btn);
            }
        }

        // =============== MỞ CUỘC TRÒ CHUYỆN ===============

        // Mở chat 1-1
        public async Task MoChat1_1Async(string tenDoiPhuong)
        {
            _isGroupChat = false;
            _groupId = "";
            _tenDoiPhuong = tenDoiPhuong;

            _view.LblTieuDeGiua.Text = tenDoiPhuong;
            _view.KhungChatPanel.Controls.Clear();
            _renderQueue.ClearQueue();

            string cid = _chatService.BuildCid(_tenHienTai, tenDoiPhuong);
            _idsTheoDoanChat[cid] = new HashSet<string>();
            _thuTuTheoDoanChat[cid] = new List<string>();

            List<TinNhan> ds = await _chatService.LoadDirectAsync(_tenHienTai, tenDoiPhuong);
            foreach (TinNhan tn in ds)
            {
                if (_idsTheoDoanChat[cid].Add(tn.id))
                {
                    _thuTuTheoDoanChat[cid].Add(tn.id);
                    _renderQueue.Enqueue(tn);
                }
            }
        }

        // Mở chat nhóm
        public async Task MoChatNhomAsync(string groupId, string tenNhom)
        {
            _isGroupChat = true;
            _groupId = groupId;
            _tenDoiPhuong = "";

            _view.LblTieuDeGiua.Text = tenNhom;
            _view.KhungChatPanel.Controls.Clear();
            _renderQueue.ClearQueue();

            _idsTheoDoanChat[groupId] = new HashSet<string>();
            _thuTuTheoDoanChat[groupId] = new List<string>();

            List<TinNhan> ds = await _groupService.LoadGroupAsync(groupId);
            foreach (TinNhan tn in ds)
            {
                if (_idsTheoDoanChat[groupId].Add(tn.id))
                {
                    _thuTuTheoDoanChat[groupId].Add(tn.id);
                    _renderQueue.Enqueue(tn);
                }
            }
        }

        // =============== GỬI TIN NHẮN ===============

        // Gửi tin nhắn hiện tại
        public async Task GuiTinNhanHienTaiAsync()
        {
            string text = _view.TxtNhapTin.Text.Trim();
            if (string.IsNullOrEmpty(text))
            {
                _view.ShowInfo("Nhập nội dung trước khi gửi.");
                return;
            }

            TinNhan tn;

            // Gửi nhóm
            if (_isGroupChat)
            {
                if (string.IsNullOrEmpty(_groupId))
                {
                    _view.ShowInfo("Chọn nhóm trước khi gửi.");
                    return;
                }

                tn = await _groupService.SendGroupAsync(_groupId, _tenHienTai, text);

                if (!_idsTheoDoanChat.ContainsKey(_groupId))
                    _idsTheoDoanChat[_groupId] = new HashSet<string>();
                if (_idsTheoDoanChat[_groupId].Add(tn.id))
                    _thuTuTheoDoanChat[_groupId].Add(tn.id);
            }
            // Gửi 1-1
            else
            {
                if (string.IsNullOrEmpty(_tenDoiPhuong))
                {
                    _view.ShowInfo("Chọn người cần trò chuyện.");
                    return;
                }

                tn = await _chatService.SendDirectAsync(_tenHienTai, _tenDoiPhuong, text);
                string cid = _chatService.BuildCid(_tenHienTai, _tenDoiPhuong);

                if (!_idsTheoDoanChat.ContainsKey(cid))
                    _idsTheoDoanChat[cid] = new HashSet<string>();
                if (_idsTheoDoanChat[cid].Add(tn.id))
                    _thuTuTheoDoanChat[cid].Add(tn.id);
            }

            _renderQueue.Enqueue(tn);
            _view.TxtNhapTin.Clear();
            _view.TxtNhapTin.RefocusToEnd();
        }

        // =============== TRẠNG THÁI NGƯỜI DÙNG ===============

        public async Task SetOfflineAsync()
        {
            await _statusService.UpdateAsync(_tenHienTai, "offline");
        }

        // Giải phóng tài nguyên
        public void Dispose()
        {
            _renderQueue?.Dispose();
        }

        // =============== TẠO BONG BÓNG CHAT ===============

        private Panel CreateBubbleForCurrentContext(TinNhan tn)
        {
            if (tn == null) return new Panel();

            bool laCuaToi = tn.guiBoi.Equals(_tenHienTai, StringComparison.OrdinalIgnoreCase);
            bool laNhom = _isGroupChat;

            int panelWidth = _view.KhungChatPanel.ClientSize.Width;
            if (panelWidth <= 0)
                panelWidth = Math.Max(_view.KhungChatPanel.Width, 400);

            int maxTextWidth = Math.Max(panelWidth - 120, 220);

            Panel row = ChatBubbleFactory.CreateRow(tn, laCuaToi, laNhom, panelWidth, maxTextWidth);

            ContextMenuStrip menu = new ContextMenuStrip();
            menu.Items.Add("Sao chép", null, delegate
            {
                try { Clipboard.SetText(tn.noiDung ?? ""); } catch { }
            });

            row.ContextMenuStrip = menu;
            return row;
        }
    }
}
