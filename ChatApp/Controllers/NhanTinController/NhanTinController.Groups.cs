using ChatApp.Models.Chat;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ChatApp.Controllers
{
    /// <summary>
    /// Phần xử lý các tính năng liên quan đến nhóm trong <see cref="NhanTinController"/>:
    /// - Tạo nhóm mới (nhập tên, thêm thành viên).
    /// - Tải và hiển thị danh sách nhóm ở panel bên trái.
    /// - Mở cửa sổ chat nhóm (load lịch sử, đăng ký realtime).
    /// - Đồng bộ tin nhắn nhóm khi có sự kiện realtime từ Firebase.
    /// - Hộp thoại nhập đơn giản cho thao tác tạo nhóm / thêm thành viên.
    /// </summary>
    public partial class NhanTinController
    {
        #region ======== NHÓM – TẠO NHÓM MỚI ========

        /// <summary>
        /// Xử lý khi người dùng bấm nút "Tạo nhóm":
        /// - Bước 1: hỏi tên nhóm (có thể bỏ trống để dùng tên mặc định).
        /// - Bước 2: hỏi danh sách thành viên (tên cách nhau bởi dấu phẩy).
        /// - Bước 3: gọi <see cref="GroupService.CreateGroupAsync(string, string, System.Collections.Generic.IEnumerable{string})"/> để tạo nhóm.
        /// - Bước 4: tải lại danh sách nhóm bên trái.
        /// - Bước 5: tự động mở cửa sổ chat nhóm vừa tạo.
        /// </summary>
        public async Task HandleCreateGroupClickedAsync()
        {
            // 1. Nhập tên nhóm
            string tenNhom = PromptForInput(
                "Tạo nhóm mới",
                "Nhập tên nhóm (có thể để trống, sẽ dùng tên mặc định):");

            // Người dùng bấm Huỷ
            if (tenNhom == null)
                return;

            // 2. Chọn thành viên (tên cách nhau bởi dấu phẩy)
            string rawMembers = PromptForInput(
                "Thêm thành viên",
                "Nhập TÊN người dùng, cách nhau bởi dấu phẩy.\n" +
                "VD: Minh Hoàng, Lê Minh Hoàng, abc123");

            IEnumerable<string> members = null;
            if (!string.IsNullOrWhiteSpace(rawMembers))
            {
                members = rawMembers
                    .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(m => m.Trim())
                    .Where(m => !string.IsNullOrWhiteSpace(m)
                                && !m.Equals(_tenNguoiDung, StringComparison.OrdinalIgnoreCase));
            }

            try
            {
                // 3. Tạo nhóm trong Firebase
                string groupId = await _groupService.CreateGroupAsync(
                    tenNhom,
                    _tenNguoiDung,
                    members);

                _view.ShowInfo("Tạo nhóm thành công!");

                // 4. Reload danh sách nhóm bên trái
                await TaiDanhSachNhomAsync();

                // 5. Mở luôn cửa sổ chat nhóm mới tạo
                await MoChatNhomAsync(groupId, tenNhom);
            }
            catch (Exception ex)
            {
                _view.ShowInfo("Không thể tạo nhóm: " + ex.Message);
            }
        }

        #endregion

        #region ======== NHÓM – TẢI DANH SÁCH NHÓM ========

        /// <summary>
        /// Tải danh sách nhóm mà người dùng hiện tại là thành viên:
        /// - Xóa toàn bộ nút nhóm cũ trong <see cref="INhanTinView.DanhSachChatPanel"/>.
        /// - Lấy toàn bộ nhóm từ <see cref="GroupService.GetAllAsync"/>.
        /// - Lọc các nhóm mà <c>_tenNguoiDung</c> là thành viên.
        /// - Tạo <see cref="Button"/> cho từng nhóm, kèm context menu:
        ///   + Rời nhóm (mọi thành viên).
        ///   + Quản lý thành viên (admin).
        ///   + Xóa nhóm (chỉ người tạo nhóm).
        /// - Click vào nút nhóm sẽ mở chat nhóm tương ứng.
        /// </summary>
        public async Task TaiDanhSachNhomAsync()
        {
            Dictionary<string, Nhom> data = await _groupService.GetAllAsync();

            // Xoá nút nhóm cũ
            List<Control> xoaNhom = new List<Control>();
            foreach (Control c in _view.DanhSachChatPanel.Controls)
            {
                if (c is Button btn &&
                    btn.Tag is string tag &&
                    tag.StartsWith("group:", StringComparison.Ordinal))
                {
                    xoaNhom.Add(btn);
                }
            }
            foreach (Control c in xoaNhom)
            {
                _view.DanhSachChatPanel.Controls.Remove(c);
                c.Dispose();
            }

            if (data == null || data.Count == 0)
                return;

            foreach (var kv in data)
            {
                string id = kv.Key;
                Nhom nhom = kv.Value;
                if (nhom == null || nhom.thanhVien == null || !nhom.thanhVien.ContainsKey(_tenNguoiDung))
                    continue;

                string tenHienThi = string.IsNullOrEmpty(nhom.tenNhom) ? id : nhom.tenNhom;

                GroupMemberInfo myInfo = nhom.thanhVien.TryGetValue(_tenNguoiDung, out var info) ? info : null;
                bool laAdmin = myInfo != null && myInfo.IsAdmin;
                string tier = myInfo != null ? (myInfo.Tier ?? "member") : "member";

                bool isGold = string.Equals(tier, "gold", StringComparison.OrdinalIgnoreCase);
                bool isSilver = string.Equals(tier, "silver", StringComparison.OrdinalIgnoreCase);

                string tierIcon =
                    isGold ? "★ " :
                    isSilver ? "☆ " : string.Empty;

                Color backColor =
                    isGold ? Color.FromArgb(255, 249, 196) :      // vàng nhạt
                    isSilver ? Color.FromArgb(224, 242, 241) :    // xanh nhạt
                    Color.LightYellow;

                Button btn = new Button
                {
                    Text = tierIcon + "[Nhóm] " + tenHienThi,
                    Tag = "group:" + id,
                    Width = _view.DanhSachChatPanel.Width - 25,
                    Height = 40,
                    TextAlign = ContentAlignment.MiddleLeft,
                    BackColor = backColor,
                    FlatStyle = FlatStyle.Flat
                };

                // Context menu quản lý nhóm
                ContextMenuStrip menu = new ContextMenuStrip();

                // Ai cũng có thể rời nhóm
                menu.Items.Add("Rời nhóm", null, async (_, __) =>
                {
                    DialogResult r = _view.ShowConfirm(
                        "Bạn chắc chắn muốn rời khỏi nhóm \"" + tenHienThi + "\"?",
                        "Xác nhận");
                    if (r != DialogResult.Yes) return;

                    try
                    {
                        await _groupService.RemoveMemberAsync(id, _tenNguoiDung);
                        _view.ShowInfo("Đã rời nhóm.");
                        await TaiDanhSachNhomAsync();
                    }
                    catch (Exception ex)
                    {
                        _view.ShowInfo("Không thể rời nhóm: " + ex.Message);
                    }
                });

                if (laAdmin)
                {
                    menu.Items.Add(new ToolStripSeparator());

                    // Mở form quản lý thành viên (tìm kiếm, thêm, xoá, mute, cấp quyền...)
                    menu.Items.Add("Quản lý thành viên...", null, (_, __) =>
                    {
                        using (var form = new QuanLyThanhVienNhom(
                            _firebase,
                            _groupService,
                            id,
                            _tenNguoiDung,
                            requireConfirmOnAdd: true))
                        {
                            form.ShowDialog();
                        }

                        // Sau khi đóng form, reload lại danh sách nhóm
                        _ = TaiDanhSachNhomAsync();
                    });

                    menu.Items.Add(new ToolStripSeparator());

                    // Xoá nhóm chỉ dành cho người tạo
                    if (!string.IsNullOrEmpty(nhom.taoBoi) &&
                        nhom.taoBoi.Equals(_tenNguoiDung, StringComparison.OrdinalIgnoreCase))
                    {
                        menu.Items.Add("Xoá nhóm", null, async (_, __) =>
                        {
                            DialogResult r = _view.ShowConfirm(
                                "Xoá nhóm \"" + tenHienThi + "\"?",
                                "Xác nhận");
                            if (r == DialogResult.Yes)
                            {
                                await _groupService.DeleteGroupAsync(id);
                                await TaiDanhSachNhomAsync();
                            }
                        });
                    }
                }

                btn.ContextMenuStrip = menu;
                btn.Click += async (_, __) =>
                {
                    // Khi mở chat nhóm, tắt stream 1-1 để đỡ tốn tài nguyên
                    _chatStream?.Dispose();
                    _chatStream = null;
                    await MoChatNhomAsync(id, tenHienThi);
                };

                _view.DanhSachChatPanel.Controls.Add(btn);
            }
        }

        #endregion

        #region ======== MỞ CHAT NHÓM (CÓ REALTIME) ========

        /// <summary>
        /// Mở cuộc trò chuyện nhóm:
        /// - Đặt chế độ về chat nhóm, reset tên đối phương 1-1.
        /// - Cập nhật tiêu đề UI theo tên nhóm.
        /// - Xóa khung chat hiện tại và clear hàng đợi render.
        /// - Reset cấu trúc lưu ID / thứ tự tin nhắn cho groupId.
        /// - Load lịch sử tin nhắn ban đầu.
        /// - Đăng ký lắng nghe realtime cho nhóm.
        /// </summary>
        /// <param name="groupId">ID nhóm trong Firebase.</param>
        /// <param name="tenNhom">Tên hiển thị của nhóm.</param>
        public async Task MoChatNhomAsync(string groupId, string tenNhom)
        {
            _isGroupChat = true;
            _groupId = groupId;
            _tenDoiPhuong = string.Empty;

            _view.LblTieuDeGiua.Text = tenNhom;
            _view.KhungChatPanel.Controls.Clear();
            _renderQueue.ClearQueue();

            _idsTheoDoanChat[groupId] = new HashSet<string>();
            _thuTuTheoDoanChat[groupId] = new List<string>();

            // Load lịch sử ban đầu
            List<TinNhan> ds = await _groupService.LoadGroupAsync(groupId);
            foreach (TinNhan tn in ds)
            {
                if (_idsTheoDoanChat[groupId].Add(tn.id))
                {
                    _thuTuTheoDoanChat[groupId].Add(tn.id);
                    _renderQueue.Enqueue(tn);
                }
            }

            // Realtime cho nhóm
            await SubscribeGroupChatRealtimeAsync(groupId);
        }

        /// <summary>
        /// Đăng ký lắng nghe realtime cho cuộc trò chuyện nhóm:
        /// - Hủy stream cũ (nếu có).
        /// - Lấy path tin nhắn nhóm từ <see cref="GroupService.GetGroupMessagesPath(string)"/>.
        /// - Mỗi khi có sự kiện, sẽ đồng bộ tin nhắn bằng <see cref="SyncGroupChatMessagesAsync(string)"/>.
        /// </summary>
        /// <param name="groupId">ID nhóm cần đăng ký realtime.</param>
        private async Task SubscribeGroupChatRealtimeAsync(string groupId)
        {
            _chatStream?.Dispose();
            _chatStream = null;

            string path = _groupService.GetGroupMessagesPath(groupId);

            _chatStream = await _firebase.OnAsync(
                path,
                (sender, args, context) =>
                {
                    _uiContext.Post(async _ =>
                    {
                        await SyncGroupChatMessagesAsync(groupId);
                    }, null);
                });
        }

        /// <summary>
        /// Đồng bộ tin nhắn nhóm khi nhận sự kiện realtime:
        /// - Chỉ xử lý nếu đang ở chế độ chat nhóm và đúng groupId hiện tại.
        /// - Dùng cờ <see cref="_isSyncingChatRealtime"/> tránh chạy chồng chéo.
        /// - Gọi <see cref="GroupService.LoadGroupAsync(string)"/> lấy danh sách tin nhắn.
        /// - Chỉ enqueue render các tin nhắn mới (ID chưa tồn tại trong <see cref="_idsTheoDoanChat"/>).
        /// </summary>
        /// <param name="groupId">ID nhóm cần sync tin nhắn.</param>
        private async Task SyncGroupChatMessagesAsync(string groupId)
        {
            if (!_isGroupChat) return;
            if (!string.Equals(groupId, _groupId, StringComparison.OrdinalIgnoreCase))
                return;

            if (_isSyncingChatRealtime) return;
            _isSyncingChatRealtime = true;

            try
            {
                List<TinNhan> ds = await _groupService.LoadGroupAsync(groupId);

                if (!_idsTheoDoanChat.ContainsKey(groupId))
                {
                    _idsTheoDoanChat[groupId] = new HashSet<string>();
                    _thuTuTheoDoanChat[groupId] = new List<string>();
                }

                var known = _idsTheoDoanChat[groupId];
                var order = _thuTuTheoDoanChat[groupId];

                foreach (TinNhan tn in ds)
                {
                    if (tn == null || string.IsNullOrEmpty(tn.id)) continue;
                    if (known.Add(tn.id))
                    {
                        order.Add(tn.id);
                        _renderQueue.Enqueue(tn);
                    }
                }
            }
            catch
            {
                // ignore, chỉ tránh crash UI
            }
            finally
            {
                _isSyncingChatRealtime = false;
            }
        }

        #endregion

        #region ======== INPUTBOX ĐƠN GIẢN – NHẬP TÊN NHÓM / THÀNH VIÊN ========

        /// <summary>
        /// Hộp thoại nhập text đơn giản (InputBox):
        /// - Hiển thị form modal với tiêu đề, nội dung câu hỏi và ô nhập text.
        /// - Có nút OK và Huỷ.
        /// - Nếu người dùng bấm OK: trả về chuỗi text nhập.
        /// - Nếu người dùng bấm Huỷ hoặc đóng form: trả về <c>null</c>.
        /// </summary>
        /// <param name="title">Tiêu đề form.</param>
        /// <param name="message">Nội dung hướng dẫn / câu hỏi.</param>
        /// <returns>Chuỗi người dùng nhập hoặc <c>null</c> nếu huỷ.</returns>
        private string PromptForInput(string title, string message)
        {
            using (var form = new Form())
            using (var label = new Label())
            using (var textBox = new TextBox())
            using (var buttonOk = new Button())
            using (var buttonCancel = new Button())
            {
                form.Text = title;
                label.Text = message;
                textBox.Width = 220;

                buttonOk.Text = "OK";
                buttonCancel.Text = "Huỷ";
                buttonOk.DialogResult = DialogResult.OK;
                buttonCancel.DialogResult = DialogResult.Cancel;

                label.SetBounds(9, 10, 372, 13);
                textBox.SetBounds(12, 30, 372, 20);
                buttonOk.SetBounds(228, 60, 75, 23);
                buttonCancel.SetBounds(309, 60, 75, 23);

                label.AutoSize = true;
                form.ClientSize = new Size(396, 95);
                form.Controls.AddRange(new Control[] { label, textBox, buttonOk, buttonCancel });
                form.FormBorderStyle = FormBorderStyle.FixedDialog;
                form.StartPosition = FormStartPosition.CenterParent;
                form.MinimizeBox = false;
                form.MaximizeBox = false;
                form.AcceptButton = buttonOk;
                form.CancelButton = buttonCancel;

                DialogResult result = form.ShowDialog();
                return result == DialogResult.OK ? textBox.Text : null;
            }
        }

        #endregion
    }
}
