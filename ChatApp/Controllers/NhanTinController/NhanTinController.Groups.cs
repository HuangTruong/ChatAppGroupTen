using ChatApp.Models.Chat;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ChatApp.Controllers
{
    public partial class NhanTinController
    {
        // ================== NHÓM ==================

        public async Task HandleCreateGroupClickedAsync()
        {
            // 1. Nhập tên nhóm
            string tenNhom = PromptForInput(
                "Tạo nhóm mới",
                "Nhập tên nhóm (có thể để trống, sẽ dùng tên mặc định):");

            if (tenNhom == null)   // user bấm Huỷ
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

        public async Task TaiDanhSachNhomAsync()
        {
            Dictionary<string, Nhom> data = await _groupService.GetAllAsync();

            // Xoá nút nhóm cũ
            List<Control> xoaNhom = new List<Control>();
            foreach (Control c in _view.DanhSachChatPanel.Controls)
            {
                if (c is Button btn && btn.Tag is string tag && tag.StartsWith("group:", StringComparison.Ordinal))
                    xoaNhom.Add(btn);
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
                if (nhom?.thanhVien == null || !nhom.thanhVien.ContainsKey(_tenNguoiDung))
                    continue;

                string tenHienThi = string.IsNullOrEmpty(nhom.tenNhom) ? id : nhom.tenNhom;

                GroupMemberInfo myInfo = nhom.thanhVien.TryGetValue(_tenNguoiDung, out var info) ? info : null;
                bool laAdmin = myInfo?.IsAdmin == true;
                string tier = myInfo?.Tier ?? "member";

                bool isGold = string.Equals(tier, "gold", StringComparison.OrdinalIgnoreCase);
                bool isSilver = string.Equals(tier, "silver", StringComparison.OrdinalIgnoreCase);

                string tierIcon =
                    isGold ? "★ " :
                    isSilver ? "☆ " : "";

                Color backColor =
                    isGold ? Color.FromArgb(255, 249, 196) :      // vàng nhạt
                    isSilver ? Color.FromArgb(224, 242, 241) :    // xanh nhạt
                    Color.LightYellow;

                Button btn = new Button
                {
                    Text = $"{tierIcon}[Nhóm] {tenHienThi}",
                    Tag = "group:" + id,
                    Width = _view.DanhSachChatPanel.Width - 25,
                    Height = 40,
                    TextAlign = ContentAlignment.MiddleLeft,
                    BackColor = backColor,
                    FlatStyle = FlatStyle.Flat
                };

                // Context menu quản lý nhóm
                ContextMenuStrip menu = new ContextMenuStrip();

                // ai cũng có thể rời nhóm
                menu.Items.Add("Rời nhóm", null, async (_, __) =>
                {
                    var r = _view.ShowConfirm($"Bạn chắc chắn muốn rời khỏi nhóm \"{tenHienThi}\"?", "Xác nhận");
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

                    // 🔹 MỞ FORM QUẢN LÝ THÀNH VIÊN (tìm kiếm, thêm, xoá, mute, cấp quyền...)
                    menu.Items.Add("Quản lý thành viên...", null, (_, __) =>
                    {
                        using (var form = new QuanLyThanhVienNhom(
                            _firebase,
                            _groupService,
                            id,
                            _tenNguoiDung,
                            requireConfirmOnAdd: true      // quản lý sau này thì hỏi confirm
                        ))
                        {
                            form.ShowDialog();
                        }

                        // Sau khi đóng form, reload lại danh sách nhóm (phòng khi có xoá / rời / đổi quyền)
                        _ = TaiDanhSachNhomAsync();
                    });

                    menu.Items.Add(new ToolStripSeparator());

                    // Xoá nhóm chỉ dành cho người tạo
                    if (!string.IsNullOrEmpty(nhom.taoBoi) &&
                        nhom.taoBoi.Equals(_tenNguoiDung, StringComparison.OrdinalIgnoreCase))
                    {
                        menu.Items.Add("Xoá nhóm", null, async (_, __) =>
                        {
                            DialogResult r = _view.ShowConfirm($"Xoá nhóm \"{tenHienThi}\"?", "Xác nhận");
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

        // ================== MỞ CHAT NHÓM (CÓ REALTIME) ==================

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

        // Đăng ký realtime cho chat nhóm
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

        // Đồng bộ tin nhắn nhóm (chỉ add id mới)
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

        // ====== InputBox đơn giản để nhập tên user ======

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

                return form.ShowDialog() == DialogResult.OK ? textBox.Text : null;
            }
        }
    }
}
