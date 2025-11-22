using ChatApp.Helpers.Ui;
using ChatApp.Models.Chat;
using ChatApp;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ChatApp.Controllers
{
    /// <summary>
    /// Controller xử lý logic nhắn tin (phím Enter, chat 1-1 realtime, gửi tin nhắn, render bubble).
    /// </summary>
    /// <remarks>
    /// - Lắng nghe phím Enter trong ô nhập tin để gửi nhanh.
    /// - Mở cuộc trò chuyện 1-1, load lịch sử và đăng ký lắng nghe realtime từ Firebase.
    /// - Gửi tin nhắn cho cuộc trò chuyện 1-1 hoặc nhóm (kèm kiểm tra quyền trong nhóm).
    /// - Tạo bubble chat UI tương ứng với bối cảnh hiện tại (nhóm / 1-1, của mình / của người khác).
    /// - Không thay đổi state/logic sẵn có, chỉ bổ sung tài liệu và trình bày.
    /// </remarks>
    public partial class NhanTinController
    {
        #region ======== XỬ LÝ PHÍM ENTER ========

        /// <summary>
        /// Xử lý sự kiện khi người dùng nhấn phím trong ô nhập tin:
        /// - Nếu nhấn Enter mà không giữ Shift/Control thì sẽ gửi tin nhắn hiện tại.
        /// - Ngăn không cho xuống dòng khi gửi bằng Enter.
        /// </summary>
        /// <param name="sender">Ô nhập tin nhắn (TextBox / RichTextBox).</param>
        /// <param name="e">Thông tin sự kiện phím được nhấn.</param>
        private async void TxtNhapTin_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter && !e.Shift && !e.Control)
            {
                e.SuppressKeyPress = true;  // Lệnh này chặn Textbox ko xuống dòng.
                await GuiTinNhanHienTaiAsync();
            }
        }

        #endregion

        #region ======== MỞ CHAT 1-1 (CÓ REALTIME) ========

        /// <summary>
        /// Mở cuộc trò chuyện 1-1 với một người dùng khác:
        /// - Đặt chế độ về chat 1-1, reset thông tin nhóm.
        /// - Cập nhật tiêu đề UI (người lạ / tên đối phương).
        /// - Xóa khung chat hiện tại và reset hàng đợi render.
        /// - Tạo CID cho đoạn chat 1-1 và chuẩn bị cấu trúc lưu ID / thứ tự tin nhắn.
        /// - Load lịch sử tin nhắn ban đầu.
        /// - Đăng ký lắng nghe realtime cho cuộc trò chuyện này.
        /// </summary>
        /// <param name="tenDoiPhuong">Tên tài khoản / hiển thị của người đối phương.</param>
        /// <param name="laNguoiLa">
        /// true nếu cuộc trò chuyện là với người lạ (chưa trong danh sách bạn), 
        /// false nếu là bạn bè / người quen.
        /// </param>
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

            await SubscribeDirectChatRealtimeAsync(tenDoiPhuong);
        }

        /// <summary>
        /// Đăng ký stream realtime trên Firebase cho cuộc trò chuyện 1-1 hiện tại:
        /// - Hủy stream cũ (nếu có).
        /// - Tạo lắng nghe mới trên đường dẫn cuộc trò chuyện.
        /// - Mỗi khi có sự kiện thay đổi, sẽ đồng bộ lại danh sách tin nhắn.
        /// </summary>
        /// <param name="tenDoiPhuong">Tên tài khoản / hiển thị của người đối phương.</param>
        private async Task SubscribeDirectChatRealtimeAsync(string tenDoiPhuong)
        {
            // Huỷ stream cũ (nếu có)
            _chatStream?.Dispose();
            _chatStream = null;

            // Tạo lắng nghe mới
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

        /// <summary>
        /// Đồng bộ tin nhắn cho cuộc trò chuyện 1-1 khi nhận sự kiện realtime từ Firebase:
        /// - Bỏ qua nếu đang ở chế độ chat nhóm hoặc đã chuyển sang cuộc trò chuyện khác.
        /// - Tránh chạy trùng bằng cờ <c>_isSyncingChatRealtime</c>.
        /// - Reload toàn bộ danh sách tin nhắn và chỉ enqueue các tin nhắn mới (chưa biết ID).
        /// - Không làm crash UI nếu có lỗi, chỉ bỏ qua.
        /// </summary>
        /// <param name="tenDoiPhuong">Tên tài khoản / hiển thị của người đối phương hiện tại.</param>
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
                // Không làm crash UI
            }
            finally
            {
                _isSyncingChatRealtime = false;
            }
        }

        #endregion

        #region ======== GỬI TIN NHẮN ========

        /// <summary>
        /// Gửi tin nhắn hiện tại trong ô nhập:
        /// - Lấy nội dung từ <c>_view.TxtNhapTin</c>, trim và kiểm tra rỗng.
        /// - Nếu là chat nhóm:
        ///   + Kiểm tra đã chọn nhóm chưa.
        ///   + Kiểm tra thành viên còn trong nhóm không.
        ///   + Kiểm tra trạng thái mute (MutedUntil).
        ///   + Kiểm tra chế độ chỉ admin được chat (AdminOnlyChat).
        ///   + Gửi tin nhắn nhóm, cập nhật danh sách ID và thứ tự tin nhắn, enqueue render.
        /// - Nếu là chat 1-1:
        ///   + Kiểm tra đã chọn người trò chuyện chưa.
        ///   + Gửi tin nhắn 1-1, cập nhật danh sách ID / thứ tự, nhưng không render ngay
        ///     để tránh nhân đôi với luồng realtime.
        /// - Sau khi gửi: xóa ô nhập, đặt lại caret và focus.
        /// </summary>
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
                if (string.IsNullOrEmpty(_groupId))
                {
                    _view.ShowInfo("Chọn nhóm trước khi gửi.");
                    return;
                }

                var group = await _groupService.GetAsync(_groupId);
                if (group == null || group.thanhVien == null ||
                    !group.thanhVien.TryGetValue(_tenNguoiDung, out var myInfo))
                {
                    _view.ShowInfo("Bạn không còn là thành viên của nhóm này.");
                    return;
                }

                long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

                if (myInfo.MutedUntil > now)
                {
                    var until = DateTimeOffset.FromUnixTimeSeconds(myInfo.MutedUntil).ToLocalTime();
                    _view.ShowInfo($"Bạn đang bị cấm chat đến {until:HH:mm dd/MM/yyyy}.");
                    return;
                }

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

                _renderQueue.Enqueue(tn);
            }
            else
            {
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

                // Không render ngay để tránh nhân đôi
            }

            _view.TxtNhapTin.Clear();
            _view.TxtNhapTin.SelectionStart = _view.TxtNhapTin.Text.Length;
            _view.TxtNhapTin.Focus();
        }

        // ================== GỬI EMOJI ==================
        public async Task GuiEmojiAsync(string emojiKey)
        {
            if (string.IsNullOrWhiteSpace(emojiKey))
                return;

            if (_isGroupChat)
            {
                if (string.IsNullOrEmpty(_groupId))
                {
                    _view.ShowInfo("Chọn nhóm trước khi gửi emoji.");
                    return;
                }

                await _groupService.SendGroupEmojiAsync(_groupId, _tenNguoiDung, emojiKey);
            }
            else
            {
                if (string.IsNullOrEmpty(_tenDoiPhuong))
                {
                    _view.ShowInfo("Chọn người cần trò chuyện.");
                    return;
                }

                await _chatService.SendDirectEmojiAsync(_tenNguoiDung, _tenDoiPhuong, emojiKey);
            }
        }

        // ================== BUBBLE CHAT ==================

        /// <summary>
        /// Tạo bubble chat UI cho tin nhắn dựa vào ngữ cảnh hiện tại:
        /// - Xác định tin nhắn là của mình hay của người khác (canh phải/trái).
        /// - Xác định đang ở chat nhóm hay chat 1-1 (có/không hiển thị tên người gửi).
        /// - Tính toán max width hợp lý theo kích thước panel khung chat.
        /// - Gọi <see cref="ChatBubbleFactory.CreateRow(TinNhan, bool, bool, int, int)"/> để tạo Panel.
        /// - Gắn context menu cho phép sao chép nội dung tin nhắn.
        /// </summary>
        /// <param name="tn">Đối tượng tin nhắn cần tạo bubble.</param>
        /// <returns>
        /// <see cref="Panel"/> chứa bubble chat tương ứng. 
        /// Nếu <paramref name="tn"/> null thì trả về panel rỗng.
        /// </returns>
        private Messages CreateBubbleForCurrentContext(TinNhan tn)
        {
            if (tn == null) return new Messages();

            bool laCuaToi = tn.guiBoi.Equals(_tenNguoiDung, StringComparison.OrdinalIgnoreCase);
            bool laNhom = _isGroupChat;

            int panelWidth = _view.KhungChatPanel.ClientSize.Width;
            if (panelWidth <= 0)
                panelWidth = Math.Max(_view.KhungChatPanel.Width, 400);

            int maxTextWidth = Math.Max(panelWidth - 120, 220);

            //Panel row = ChatBubbleFactory.CreateRow(tn, laCuaToi, laNhom, panelWidth, maxTextWidth);
            Messages row = new Messages(tn, laCuaToi, laNhom, panelWidth, maxTextWidth);

            #region Nhấn phải chuột vào tin nhắn hiện menu copy tin nhắn
            ContextMenuStrip menu = new ContextMenuStrip();
            menu.Items.Add("Sao chép", null, delegate
            {
                try { Clipboard.SetText(tn.noiDung ?? ""); } catch { }
            });
            #endregion

            row.ContextMenuStrip = menu;
            return row;
        }

        #endregion
    }
}
