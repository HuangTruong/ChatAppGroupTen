using ChatApp.Helpers.Ui;
using ChatApp.Models.Chat;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ChatApp.Controllers
{
    public partial class NhanTinController
    {
        // ================== SỰ KIỆN PHÍM ENTER ==================

        private async void TxtNhapTin_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter && !e.Shift && !e.Control)
            {
                e.SuppressKeyPress = true;
                await GuiTinNhanHienTaiAsync();
            }
        }

        // ================== MỞ CHAT 1-1 (CÓ REALTIME) ==================

        public async Task MoChat1_1Async(string tenDoiPhuong, bool laNguoiLa = false)
        {
            _isGroupChat = false;
            _groupId = "";
            _tenDoiPhuong = tenDoiPhuong;

            _view.LblTieuDeGiua.Text = laNguoiLa ? "Người lạ" : tenDoiPhuong;

            _view.KhungChatPanel.Controls.Clear();
            _renderQueue.ClearQueue();

            string cid = _chatService.BuildCid(_tenNguoiDung, tenDoiPhuong);
            _idsTheoDoanChat[cid] = new HashSet<string>();
            _thuTuTheoDoanChat[cid] = new List<string>();

            // Load lịch sử ban đầu
            List<TinNhan> ds = await _chatService.LoadDirectAsync(_tenNguoiDung, tenDoiPhuong);
            foreach (TinNhan tn in ds)
            {
                if (_idsTheoDoanChat[cid].Add(tn.id))
                {
                    _thuTuTheoDoanChat[cid].Add(tn.id);
                    _renderQueue.Enqueue(tn);
                }
            }

            // Đăng ký realtime cho cuộc trò chuyện này
            await SubscribeDirectChatRealtimeAsync(tenDoiPhuong);
        }

        // Đăng ký stream realtime cho 1-1
        private async Task SubscribeDirectChatRealtimeAsync(string tenDoiPhuong)
        {
            _chatStream?.Dispose();
            _chatStream = null;

            string cid = _chatService.BuildCid(_tenNguoiDung, tenDoiPhuong);
            string path = $"cuocTroChuyen/{cid}";

            _chatStream = await _firebase.OnAsync(
                path,
                (sender, args, context) =>
                {
                    _uiContext.Post(async _ =>
                    {
                        await SyncDirectChatMessagesAsync(tenDoiPhuong);
                    }, null);
                });
        }

        // Đồng bộ tin nhắn mới từ Firebase vào UI (chỉ add những id chưa có)
        private async Task SyncDirectChatMessagesAsync(string tenDoiPhuong)
        {
            if (_isGroupChat) return;
            if (!string.Equals(tenDoiPhuong, _tenDoiPhuong, StringComparison.OrdinalIgnoreCase))
                return;

            if (_isSyncingChatRealtime) return;
            _isSyncingChatRealtime = true;

            try
            {
                List<TinNhan> ds = await _chatService.LoadDirectAsync(_tenNguoiDung, tenDoiPhuong);
                string cid = _chatService.BuildCid(_tenNguoiDung, tenDoiPhuong);

                if (!_idsTheoDoanChat.ContainsKey(cid))
                {
                    _idsTheoDoanChat[cid] = new HashSet<string>();
                    _thuTuTheoDoanChat[cid] = new List<string>();
                }

                var known = _idsTheoDoanChat[cid];
                var order = _thuTuTheoDoanChat[cid];

                foreach (TinNhan tn in ds)
                {
                    if (tn == null || string.IsNullOrEmpty(tn.id))
                        continue;

                    if (known.Add(tn.id))
                    {
                        order.Add(tn.id);
                        _renderQueue.Enqueue(tn);
                    }
                }
            }
            catch
            {
                // có thể log lỗi, nhưng không crash UI
            }
            finally
            {
                _isSyncingChatRealtime = false;
            }
        }

        // ================== GỬI TIN NHẮN ==================

        public async Task GuiTinNhanHienTaiAsync()
        {
            string text = _view.TxtNhapTin.Text.Trim();
            if (string.IsNullOrEmpty(text))
            {
                _view.ShowInfo("Nhập nội dung trước khi gửi.");
                return;
            }

            TinNhan tn;

            if (_isGroupChat)
            {
                // === GỬI CHO NHÓM ===
                if (string.IsNullOrEmpty(_groupId))
                {
                    _view.ShowInfo("Chọn nhóm trước khi gửi.");
                    return;
                }

                // Lấy thông tin nhóm + quyền hiện tại của mình
                var group = await _groupService.GetAsync(_groupId);
                if (group == null || group.thanhVien == null ||
                    !group.thanhVien.TryGetValue(_tenNguoiDung, out var myInfo))
                {
                    _view.ShowInfo("Bạn không còn là thành viên của nhóm này.");
                    return;
                }

                long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

                // Bị cấm chat?
                if (myInfo.MutedUntil > now)
                {
                    var until = DateTimeOffset.FromUnixTimeSeconds(myInfo.MutedUntil).ToLocalTime();
                    _view.ShowInfo($"Bạn đang bị cấm chat đến {until:HH:mm dd/MM/yyyy}.");
                    return;
                }

                // Nhóm chỉ cho admin vàng/bạc chat?
                if (group.AdminOnlyChat)
                {
                    string tier = myInfo.Tier ?? "member";
                    bool isGold = string.Equals(tier, "gold", StringComparison.OrdinalIgnoreCase);
                    bool isSilver = string.Equals(tier, "silver", StringComparison.OrdinalIgnoreCase);

                    if (!isGold && !isSilver)
                    {
                        _view.ShowInfo("Hiện nhóm chỉ cho phép quản trị viên gửi tin nhắn.");
                        return;
                    }
                }

                tn = await _groupService.SendGroupAsync(_groupId, _tenNguoiDung, text);

                if (!_idsTheoDoanChat.ContainsKey(_groupId))
                    _idsTheoDoanChat[_groupId] = new HashSet<string>();

                if (_idsTheoDoanChat[_groupId].Add(tn.id))
                    _thuTuTheoDoanChat[_groupId].Add(tn.id);

                // Đã có realtime cho nhóm, nhưng vẫn render ngay:
                // lần sync tiếp theo sẽ bỏ qua vì id đã được Add vào known.
                _renderQueue.Enqueue(tn);
            }
            else
            {
                // === GỬI 1-1 ===
                if (string.IsNullOrEmpty(_tenDoiPhuong))
                {
                    _view.ShowInfo("Chọn người cần trò chuyện.");
                    return;
                }

                tn = await _chatService.SendDirectAsync(_tenNguoiDung, _tenDoiPhuong, text);
                string cid = _chatService.BuildCid(_tenNguoiDung, _tenDoiPhuong);

                if (!_idsTheoDoanChat.ContainsKey(cid))
                    _idsTheoDoanChat[cid] = new HashSet<string>();

                if (_idsTheoDoanChat[cid].Add(tn.id))
                    _thuTuTheoDoanChat[cid].Add(tn.id);

                // KHÔNG render ở đây nữa để tránh bị nhân đôi.
                // Tin nhắn sẽ xuất hiện khi Firebase bắn event,
                // và SyncDirectChatMessagesAsync enqueue vào queue.
            }

            _view.TxtNhapTin.Clear();
            _view.TxtNhapTin.SelectionStart = _view.TxtNhapTin.Text.Length;
            _view.TxtNhapTin.SelectionLength = 0;
            _view.TxtNhapTin.Focus();
        }

        // ================== BUBBLE CHAT ==================

        private Panel CreateBubbleForCurrentContext(TinNhan tn)
        {
            if (tn == null) return new Panel();

            bool laCuaToi = tn.guiBoi.Equals(_tenNguoiDung, StringComparison.OrdinalIgnoreCase);
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
