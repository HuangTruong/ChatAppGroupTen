using ChatApp.Models.Users;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ChatApp.Models.Chat;

namespace ChatApp.Controllers
{
    public partial class NhanTinController
    {
        // ================== HELPER: CHECK ONLINE/OFFLINE ==================

        private bool IsUserOnlineByName(string displayName)
        {
            if (string.IsNullOrWhiteSpace(displayName))
                return false;

            string trimmed = displayName.Trim();

            if (_statusCache.TryGetValue(trimmed, out var st1) &&
                string.Equals(st1, "online", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            string k2 = trimmed
                .Replace(".", "_")
                .Replace("@", "_at_");

            if (_statusCache.TryGetValue(k2, out var st2) &&
                string.Equals(st2, "online", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            string k3 = k2.Replace(" ", "_");
            if (_statusCache.TryGetValue(k3, out var st3) &&
                string.Equals(st3, "online", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            foreach (var kv in _statusCache)
            {
                if (string.Equals(kv.Key, trimmed, StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(kv.Value, "online", StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        // ================== REALTIME LISTENERS (friend, status) ==================

        private async Task SetupRealtimeListenersAsync()
        {
            _friendsStream = await _firebase.OnAsync(
                $"friends/{_tenNguoiDung}",
                (sender, args, context) =>
                {
                    _uiContext.Post(async _ => { await OnFriendStateChangedAsync(); }, null);
                });

            _pendingForMeStream = await _firebase.OnAsync(
                $"friendRequests/pending/{_tenNguoiDung}",
                (sender, args, context) =>
                {
                    _uiContext.Post(async _ => { await OnFriendStateChangedAsync(); }, null);
                });

            _pendingAllStream = await _firebase.OnAsync(
                "friendRequests/pending",
                (sender, args, context) =>
                {
                    _uiContext.Post(async _ => { await OnFriendStateChangedAsync(); }, null);
                });

            _statusStream = await _firebase.OnAsync(
                "status",
                (sender, args, context) =>
                {
                    _uiContext.Post(async _ => { await OnStatusChangedAsync(); }, null);
                });
        }

        private async Task OnFriendStateChangedAsync()
        {
            await RefreshFriendStatesSnapshotAsync();
            await RebuildUserListAccordingToCurrentSearchAsync();
        }

        private async Task OnStatusChangedAsync()
        {
            await RefreshStatusCacheAsync();
            UpdateFriendOnlineFlags();
        }

        private async Task RefreshFriendStatesSnapshotAsync()
        {
            var states = await _friendService.LoadFriendStatesAsync();
            _lastBanBe.Clear();
            _lastDaMoi.Clear();
            _lastMoiDen.Clear();

            if (states.BanBe != null)
                foreach (var b in states.BanBe) _lastBanBe.Add(b);
            if (states.DaMoi != null)
                foreach (var x in states.DaMoi) _lastDaMoi.Add(x);
            if (states.MoiDen != null)
                foreach (var x in states.MoiDen) _lastMoiDen.Add(x);
        }

        private async Task RefreshStatusCacheAsync()
        {
            var dict = await _statusService.GetAllAsync();
            _statusCache.Clear();
            if (dict != null)
                foreach (var kv in dict) _statusCache[kv.Key] = kv.Value;
        }

        private async Task RebuildUserListAccordingToCurrentSearchAsync()
        {
            if (_isBuildingUserList) return;

            _isBuildingUserList = true;
            try
            {
                string keyword = (_currentSearchKeyword ?? string.Empty).Trim();
                if (string.IsNullOrEmpty(keyword))
                    await TaiDanhSachNguoiDungAsync();
                else
                    await SearchUsersAsync(keyword);
            }
            finally
            {
                _isBuildingUserList = false;
            }
        }

        public async Task HandleSearchTextChangedAsync(string text)
        {
            _currentSearchKeyword = text ?? string.Empty;
            await RebuildUserListAccordingToCurrentSearchAsync();
        }

        // ================== CLEAR USER UI (KHÔNG ĐỤNG NHÓM) ==================

        private void ClearUserListButKeepGroups()
        {
            var toRemove = new List<Control>();

            foreach (Control c in _view.DanhSachChatPanel.Controls)
            {
                if (c is Button btn && btn.Tag is string tag && tag.StartsWith("group:", StringComparison.Ordinal))
                    continue;

                toRemove.Add(c);
            }

            foreach (var c in toRemove)
            {
                _view.DanhSachChatPanel.Controls.Remove(c);
                c.Dispose();
            }
        }

        // ================== UPDATE ONLINE/OFFLINE TRÊN NÚT BẠN BÈ ==================

        private void UpdateFriendOnlineFlags()
        {
            foreach (Control c in _view.DanhSachChatPanel.Controls)
            {
                if (c is Button btn && btn.Tag is string tag && !tag.StartsWith("group:", StringComparison.Ordinal))
                {
                    string userName = tag;
                    bool online = IsUserOnlineByName(userName);
                    string prefix = online ? "(online) " : "(offline) ";
                    string suffix = " (Bạn bè)";
                    btn.Text = prefix + userName + suffix;
                }
            }
        }

        // ================== DANH SÁCH NGƯỜI DÙNG ==================

        public async Task TaiDanhSachNguoiDungAsync()
        {
            var banBe = _lastBanBe;
            var moiDen = _lastMoiDen;

            var res = await _firebase.GetAsync("users");
            Dictionary<string, User> data = res.ResultAs<Dictionary<string, User>>();

            ClearUserListButKeepGroups();

            bool coNhom = _view.DanhSachChatPanel.Controls
                .OfType<Button>()
                .Any(b => b.Tag is string t && t.StartsWith("group:", StringComparison.Ordinal));

            if ((data == null || data.Count == 0) && banBe.Count == 0 && moiDen.Count == 0)
            {
                if (!coNhom)
                {
                    Label lbl = new Label
                    {
                        Text = "Bạn chưa có bạn bè nào.",
                        AutoSize = true
                    };
                    _view.DanhSachChatPanel.Controls.Add(lbl);
                }
                return;
            }

            // Lời mời kết bạn đến mình
            if (moiDen.Count > 0 && data != null)
            {
                var lblInviteHeader = new Label
                {
                    Text = "Lời mời kết bạn:",
                    Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                    ForeColor = Color.FromArgb(0, 120, 215),
                    AutoSize = true,
                    Margin = new Padding(4, 4, 4, 2),
                    Tag = "invite_header"
                };
                _view.DanhSachChatPanel.Controls.Add(lblInviteHeader);

                var groupsInvite = data.Values
                    .Where(u => u != null && moiDen.Contains(u.Ten))
                    .GroupBy(u => u.Ten, StringComparer.OrdinalIgnoreCase);

                foreach (var g in groupsInvite)
                {
                    var u = g.First();
                    var card = CreateSearchUserRow(
                        u,
                        laBanBe: false,
                        daGuiLoiMoi: false,
                        moiDen: true,
                        normKey: NormalizeForSearch(u.Ten),
                        rawKeyword: string.Empty);
                    _view.DanhSachChatPanel.Controls.Add(card);
                }

                _view.DanhSachChatPanel.Controls.Add(new Label
                {
                    Text = "",
                    AutoSize = true,
                    Margin = new Padding(4, 0, 4, 0),
                    Tag = "invite_spacer"
                });
            }

            // Bạn bè
            if (banBe.Count == 0)
            {
                Label lbl = new Label
                {
                    Text = moiDen.Count > 0
                        ? "Bạn chưa có bạn bè nào, nhưng đã có lời mời kết bạn."
                        : "Bạn chưa có bạn bè nào.",
                    AutoSize = true,
                    Tag = "no_friend_label"
                };
                _view.DanhSachChatPanel.Controls.Add(lbl);
                return;
            }

            if (data == null || data.Count == 0)
                return;

            foreach (string tenBan in banBe)
            {
                if (tenBan.Equals(_tenNguoiDung, StringComparison.OrdinalIgnoreCase))
                    continue;

                User u = data.Values.FirstOrDefault(x =>
                    x != null && x.Ten.Equals(tenBan, StringComparison.OrdinalIgnoreCase));

                if (u == null)
                    continue;

                bool online = IsUserOnlineByName(u.Ten);

                var item = new UserListItem
                {
                    TenHienThi = u.Ten,
                    LaBanBe = true,
                    DaGuiLoiMoi = false,
                    MoiKetBanChoMinh = moiDen.Contains(u.Ten),
                    Online = online
                };

                Button btn = TaoNutUser(item);
                _view.DanhSachChatPanel.Controls.Add(btn);
            }
        }

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
                TextAlign = ContentAlignment.MiddleLeft,
                BackColor = Color.WhiteSmoke,
                FlatStyle = FlatStyle.Flat
            };

            ContextMenuStrip menu = new ContextMenuStrip();

            if (!item.LaBanBe && !item.DaGuiLoiMoi && !item.MoiKetBanChoMinh)
                menu.Items.Add("Kết bạn", null, async delegate
                {
                    await _friendService.GuiLoiMoiAsync(item.TenHienThi);
                    _view.ShowInfo("Đã gửi lời mời.");
                });

            if (item.MoiKetBanChoMinh)
                menu.Items.Add("Chấp nhận kết bạn", null, async delegate
                {
                    await _friendService.ChapNhanAsync(item.TenHienThi);
                    _view.ShowInfo("Đã trở thành bạn bè.");
                });

            if (item.LaBanBe)
                menu.Items.Add("Huỷ kết bạn", null, async delegate
                {
                    DialogResult r = _view.ShowConfirm($"Huỷ kết bạn với {item.TenHienThi}?", "Xác nhận");
                    if (r == DialogResult.Yes)
                    {
                        await _friendService.HuyKetBanAsync(item.TenHienThi);
                    }
                });

            btn.ContextMenuStrip = menu;
            btn.Click += async delegate { await MoChat1_1Async(item.TenHienThi, false); };

            return btn;
        }

        // ================== SEARCH USER ==================

        private static string NormalizeForSearch(string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return string.Empty;

            string formD = s.Normalize(NormalizationForm.FormD);
            var chars = formD
                .Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                .ToArray();

            return new string(chars)
                .ToLowerInvariant()
                .Replace(" ", "");
        }

        public async Task SearchUsersAsync(string keyword)
        {
            keyword = (keyword ?? string.Empty).Trim();
            string normKey = NormalizeForSearch(keyword);

            if (normKey.Length == 0)
            {
                await TaiDanhSachNguoiDungAsync();
                return;
            }

            var banBe = _lastBanBe;
            var daMoi = _lastDaMoi;
            var moiDen = _lastMoiDen;

            var res = await _firebase.GetAsync("users");
            Dictionary<string, User> data = res.ResultAs<Dictionary<string, User>>()
                                            ?? new Dictionary<string, User>();

            ClearUserListButKeepGroups();

            _view.DanhSachChatPanel.SuspendLayout();

            var groups = data.Values
                .Where(u => u != null && !string.IsNullOrWhiteSpace(u.Ten))
                .GroupBy(u => u.Ten, StringComparer.OrdinalIgnoreCase);

            foreach (var g in groups)
            {
                User u = g.First();
                string normName = NormalizeForSearch(u.Ten);

                if (!normName.Contains(normKey))
                    continue;

                if (u.Ten.Equals(_tenNguoiDung, StringComparison.OrdinalIgnoreCase))
                    continue;

                bool laBanBe = banBe.Contains(u.Ten);
                bool daGui = daMoi.Contains(u.Ten);
                bool moiToi = moiDen.Contains(u.Ten);

                Control row = CreateSearchUserRow(u, laBanBe, daGui, moiToi, normKey, keyword);
                _view.DanhSachChatPanel.Controls.Add(row);
            }

            _view.DanhSachChatPanel.ResumeLayout();
        }

        private Control CreateSearchUserRow(
            User u,
            bool laBanBe,
            bool daGuiLoiMoi,
            bool moiDen,
            string normKey,
            string rawKeyword)
        {
            int panelWidth = Math.Max(_view.DanhSachChatPanel.ClientSize.Width, 260);

            var card = new Panel
            {
                Width = panelWidth - 8,
                Height = 70,
                Margin = new Padding(4, 3, 4, 3),
                BackColor = Color.FromArgb(245, 248, 252),
                Cursor = Cursors.Hand,
                Tag = u.Ten
            };

            card.MouseEnter += (s, e) => card.BackColor = Color.FromArgb(232, 240, 255);
            card.MouseLeave += (s, e) => card.BackColor = Color.FromArgb(245, 248, 252);

            var avatar = new Panel
            {
                Width = 40,
                Height = 40,
                Left = 10,
                Top = 15,
                BackColor = Color.FromArgb(90, 160, 255)
            };
            var gp = new System.Drawing.Drawing2D.GraphicsPath();
            gp.AddEllipse(0, 0, avatar.Width - 1, avatar.Height - 1);
            avatar.Region = new Region(gp);

            var lblInitial = new Label
            {
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 11f, FontStyle.Bold),
                Text = (u.Ten ?? "?").Trim().Length > 0 ? u.Ten.Trim()[0].ToString().ToUpper() : "?"
            };
            avatar.Controls.Add(lblInitial);

            var textArea = new Panel
            {
                Left = 60,
                Top = 10,
                Height = 50,
                Width = card.Width - 60 - 100,
                BackColor = Color.Transparent
            };
            textArea.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;

            var lblName = new Label
            {
                AutoSize = false,
                Left = 0,
                Top = 0,
                Width = textArea.Width,
                Font = new Font("Segoe UI", 10.5f, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 30, 30),
                Text = u.Ten,
                AutoEllipsis = true
            };
            lblName.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;

            string statusText;
            Color statusColor;

            if (laBanBe)
            {
                statusText = "Bạn bè · Nhấn để chat";
                statusColor = Color.FromArgb(0, 128, 0);
            }
            else if (daGuiLoiMoi)
            {
                statusText = "Đã gửi lời mời · Nhấn để huỷ";
                statusColor = Color.FromArgb(200, 140, 0);
            }
            else if (moiDen)
            {
                statusText = "Đã mời bạn · Nhấn 'Chấp nhận'";
                statusColor = Color.FromArgb(0, 120, 215);
            }
            else
            {
                statusText = "Người lạ · Bấm + để kết bạn hoặc nhấn để chat";
                statusColor = Color.FromArgb(100, 100, 100);
            }

            var lblStatus = new Label
            {
                AutoSize = false,
                Left = 0,
                Top = 24,
                Width = textArea.Width,
                Font = new Font("Segoe UI", 8.5f, FontStyle.Italic),
                ForeColor = statusColor,
                AutoEllipsis = true,
                Text = statusText
            };
            lblStatus.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;

            textArea.Controls.Add(lblName);
            textArea.Controls.Add(lblStatus);

            var btnAction = new Button
            {
                Width = 90,
                Height = 28,
                Top = 21,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.White,
                Font = new Font("Segoe UI", 9f),
                Cursor = Cursors.Hand
            };
            btnAction.FlatAppearance.BorderSize = 0;
            btnAction.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnAction.Left = card.Width - btnAction.Width - 10;

            card.Resize += (s, e) =>
            {
                textArea.Width = card.Width - 60 - 100;
                lblName.Width = textArea.Width;
                lblStatus.Width = textArea.Width;
                btnAction.Left = card.Width - btnAction.Width - 10;
            };

            if (laBanBe)
            {
                btnAction.Text = "Chat";
                btnAction.ForeColor = Color.White;
                btnAction.BackColor = Color.FromArgb(0, 120, 215);

                btnAction.Click += async (s, e) => { await MoChat1_1Async(u.Ten, false); };
                card.Click += async (s, e) => { await MoChat1_1Async(u.Ten, false); };
            }
            else if (daGuiLoiMoi)
            {
                btnAction.Text = "Đã gửi";
                btnAction.ForeColor = Color.White;
                btnAction.BackColor = Color.FromArgb(200, 140, 0);

                btnAction.Click += async (s, e) =>
                {
                    await _friendService.HuyLoiMoiAsync(u.Ten);
                    _view.ShowInfo("Đã huỷ lời mời kết bạn.");
                };

                card.Click += async (s, e) => { await MoChat1_1Async(u.Ten, true); };
            }
            else if (moiDen)
            {
                btnAction.Text = "Chấp nhận";
                btnAction.ForeColor = Color.White;
                btnAction.BackColor = Color.FromArgb(0, 160, 80);

                btnAction.Click += async (s, e) =>
                {
                    await _friendService.ChapNhanAsync(u.Ten);
                    _view.ShowInfo("Đã chấp nhận lời mời kết bạn!");
                };

                card.Click += async (s, e) => { await MoChat1_1Async(u.Ten, true); };
            }
            else
            {
                btnAction.Text = "➕ Kết bạn";
                btnAction.ForeColor = Color.FromArgb(0, 120, 215);

                btnAction.Click += async (s, e) =>
                {
                    await _friendService.GuiLoiMoiAsync(u.Ten);
                    _view.ShowInfo("Đã gửi lời mời kết bạn!");
                };

                card.Click += async (s, e) => { await MoChat1_1Async(u.Ten, true); };
            }

            foreach (Control child in new Control[] { avatar, textArea, lblName, lblStatus })
            {
                child.MouseEnter += (s, e) => card.BackColor = Color.FromArgb(232, 240, 255);
                child.MouseLeave += (s, e) => card.BackColor = Color.FromArgb(245, 248, 252);

                child.Click += async (s, e) =>
                {
                    if (laBanBe)
                        await MoChat1_1Async(u.Ten, false);
                    else
                        await MoChat1_1Async(u.Ten, true);
                };
            }

            card.Controls.Add(avatar);
            card.Controls.Add(textArea);
            card.Controls.Add(btnAction);

            return card;
        }
    }
}
