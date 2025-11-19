using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using ChatApp.Models.Chat;
using ChatApp.Models.Users;
using ChatApp.Services.Chat;
using FireSharp.Interfaces;
using Guna.UI2.WinForms;

namespace ChatApp.Controllers
{
    public partial class QuanLyThanhVienNhom : Form
    {
        private readonly IFirebaseClient _firebase;
        private readonly GroupService _groupService;
        private readonly string _groupId;
        private readonly string _currentUser;
        private readonly bool _requireConfirmOnAdd;

        private Nhom _group;
        private bool _isGoldAdmin;
        private bool _isSilverAdmin;

        private bool _suppressSettingsEvents;

        public QuanLyThanhVienNhom(
            IFirebaseClient firebase,
            GroupService groupService,
            string groupId,
            string currentUser,
            bool requireConfirmOnAdd)
        {
            if (firebase == null) throw new ArgumentNullException(nameof(firebase));
            if (groupService == null) throw new ArgumentNullException(nameof(groupService));
            if (string.IsNullOrWhiteSpace(groupId)) throw new ArgumentNullException(nameof(groupId));
            if (string.IsNullOrWhiteSpace(currentUser)) throw new ArgumentNullException(nameof(currentUser));

            _firebase = firebase;
            _groupService = groupService;
            _groupId = groupId;
            _currentUser = currentUser;
            _requireConfirmOnAdd = requireConfirmOnAdd;

            InitializeComponent();

            // Hook sự kiện runtime
            this.Load += async (_, __) =>
            {
                await LoadGroupAsync();
                await ReloadMembersAsync();
            };

            txtSearch.TextChanged += async (_, __) =>
            {
                await SearchUsersAsync(txtSearch.Text);
            };

            chkAdminOnlyChat.CheckedChanged += ChkAdminOnlyChat_CheckedChanged;
            chkRequireApproval.CheckedChanged += ChkRequireApproval_CheckedChanged;
        }

        // ================== LOAD NHÓM & THÀNH VIÊN ==================

        private async Task LoadGroupAsync()
        {
            _group = await _groupService.GetAsync(_groupId);
            if (_group == null)
            {
                lblGroupName.Text = "Nhóm không tồn tại";
                chkAdminOnlyChat.Enabled = false;
                chkRequireApproval.Enabled = false;
                return;
            }

            lblGroupName.Text = "Nhóm: " + (_group.tenNhom ?? _group.id);

            _isGoldAdmin = false;
            _isSilverAdmin = false;

            if (_group.thanhVien != null &&
                _group.thanhVien.TryGetValue(_currentUser, out GroupMemberInfo me) &&
                me != null)
            {
                if (string.Equals(me.Tier, "gold", StringComparison.OrdinalIgnoreCase))
                    _isGoldAdmin = true;
                else if (string.Equals(me.Tier, "silver", StringComparison.OrdinalIgnoreCase))
                    _isSilverAdmin = true;
            }

            _suppressSettingsEvents = true;
            try
            {
                chkAdminOnlyChat.Checked = _group.AdminOnlyChat;
                chkRequireApproval.Checked = _group.RequireApproval;
            }
            finally
            {
                _suppressSettingsEvents = false;
            }

            // Chỉ admin vàng được bật tắt 2 quyền này
            chkAdminOnlyChat.Enabled = _isGoldAdmin;
            chkRequireApproval.Enabled = _isGoldAdmin;
        }

        private async Task ReloadMembersAsync()
        {
            if (_group == null || _group.thanhVien == null)
            {
                flpMembers.Controls.Clear();
                return;
            }

            flpMembers.SuspendLayout();
            flpMembers.Controls.Clear();

            foreach (var kv in _group.thanhVien.OrderBy(k => k.Key, StringComparer.OrdinalIgnoreCase))
            {
                string userName = kv.Key;
                GroupMemberInfo info = kv.Value ?? new GroupMemberInfo();
                Control row = CreateMemberRow(userName, info);
                flpMembers.Controls.Add(row);
            }

            flpMembers.ResumeLayout();
        }

        // ================== ROW THÀNH VIÊN (CARD STYLE) ==================

        private Control CreateMemberRow(string userName, GroupMemberInfo info)
        {
            int panelWidth = Math.Max(flpMembers.ClientSize.Width, 260);

            var card = new Panel
            {
                Width = panelWidth - 8,
                Height = 70,
                Margin = new Padding(4, 3, 4, 3),
                BackColor = Color.FromArgb(245, 248, 252),
                Cursor = Cursors.Hand,
                Tag = userName
            };

            card.MouseEnter += (s, e) => card.BackColor = Color.FromArgb(232, 240, 255);
            card.MouseLeave += (s, e) => card.BackColor = Color.FromArgb(245, 248, 252);

            // Avatar tròn
            var avatar = new Panel
            {
                Width = 40,
                Height = 40,
                Left = 10,
                Top = 15
            };

            Color avatarColor;
            if (string.Equals(info.Tier, "gold", StringComparison.OrdinalIgnoreCase))
                avatarColor = Color.FromArgb(255, 193, 7);   // vàng
            else if (string.Equals(info.Tier, "silver", StringComparison.OrdinalIgnoreCase))
                avatarColor = Color.FromArgb(148, 163, 184); // bạc
            else
                avatarColor = Color.FromArgb(90, 160, 255);  // xanh

            avatar.BackColor = avatarColor;

            var gp = new System.Drawing.Drawing2D.GraphicsPath();
            gp.AddEllipse(0, 0, avatar.Width - 1, avatar.Height - 1);
            avatar.Region = new Region(gp);

            var lblInitial = new Label
            {
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 11f, FontStyle.Bold),
                Text = (userName ?? "?").Trim().Length > 0 ? userName.Trim()[0].ToString().ToUpper() : "?"
            };
            avatar.Controls.Add(lblInitial);

            // Vùng text
            var textArea = new Panel
            {
                Left = 60,
                Top = 10,
                Height = 50,
                Width = card.Width - 60 - 120,
                BackColor = Color.Transparent
            };
            textArea.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;

            var lblName = new Label
            {
                AutoSize = false,
                Left = 0,
                Top = 0,
                Width = textArea.Width,
                Font = new Font("Segoe UI", 10.0f, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 30, 30),
                Text = userName,
                AutoEllipsis = true
            };
            lblName.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;

            string roleText;
            if (string.Equals(info.Tier, "gold", StringComparison.OrdinalIgnoreCase))
                roleText = "Vàng · quản trị cao nhất";
            else if (string.Equals(info.Tier, "silver", StringComparison.OrdinalIgnoreCase))
                roleText = "Bạc · quản trị viên";
            else
                roleText = "Thành viên";

            if (info.MutedUntil > DateTimeOffset.UtcNow.ToUnixTimeSeconds())
                roleText += " · đang bị khoá chat";

            var lblRole = new Label
            {
                AutoSize = false,
                Left = 0,
                Top = 24,
                Width = textArea.Width,
                Font = new Font("Segoe UI", 8.5f, FontStyle.Italic),
                ForeColor = Color.DimGray,
                AutoEllipsis = true,
                Text = roleText
            };
            lblRole.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;

            textArea.Controls.Add(lblName);
            textArea.Controls.Add(lblRole);

            // Nút hành động nhanh: Xoá
            var btnRemove = new Guna2Button
            {
                Width = 90,
                Height = 28,
                Top = 21,
                BorderRadius = 12,
                FillColor = Color.FromArgb(239, 68, 68),
                HoverState = { FillColor = Color.FromArgb(248, 113, 113) },
                Font = new Font("Segoe UI", 9f),
                ForeColor = Color.White,
                Text = "Xoá",
                Cursor = Cursors.Hand
            };
            btnRemove.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnRemove.Left = card.Width - btnRemove.Width - 12;

            // Resize cho card
            card.Resize += (s, e) =>
            {
                textArea.Width = card.Width - 60 - 120;
                lblName.Width = textArea.Width;
                lblRole.Width = textArea.Width;
                btnRemove.Left = card.Width - btnRemove.Width - 12;
            };

            // Logic xoá: click card hoặc click nút
            async Task RemoveMemberAsync()
            {
                if (!_isGoldAdmin && !_isSilverAdmin)
                {
                    MessageBox.Show("Bạn không có quyền xoá thành viên.", "Thông báo",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                if (string.Equals(userName, _currentUser, StringComparison.OrdinalIgnoreCase))
                {
                    MessageBox.Show("Nếu muốn rời nhóm, hãy dùng chức năng 'Rời nhóm' ở ngoài.",
                        "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                if (MessageBox.Show($"Xoá {userName} khỏi nhóm ?", "Xác nhận",
                        MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                    return;

                await _groupService.RemoveMemberAsync(_groupId, userName);
                await LoadGroupAsync();
                await ReloadMembersAsync();
            }

            btnRemove.Click += async (s, e) => await RemoveMemberAsync();
            card.Click += async (s, e) => await RemoveMemberAsync();

            // Context menu chi tiết (mute / set quyền / nhượng quyền)
            var menu = new ContextMenuStrip();

            if (_isGoldAdmin || _isSilverAdmin)
            {
                menu.Items.Add("Xoá khỏi nhóm", null, async (_, __) => await RemoveMemberAsync());

                menu.Items.Add("Cấm chat 10 phút", null, async (_, __) =>
                {
                    await _groupService.MuteMemberAsync(_groupId, userName, TimeSpan.FromMinutes(10));
                    await LoadGroupAsync();
                    await ReloadMembersAsync();
                });

                menu.Items.Add("Bỏ cấm chat", null, async (_, __) =>
                {
                    await _groupService.UnmuteMemberAsync(_groupId, userName);
                    await LoadGroupAsync();
                    await ReloadMembersAsync();
                });

                menu.Items.Add(new ToolStripSeparator());
            }

            if (_isGoldAdmin)
            {
                menu.Items.Add("Đặt làm thành viên thường", null, async (_, __) =>
                {
                    await _groupService.UpdateMemberRoleAsync(_groupId, userName, false, "member");
                    await LoadGroupAsync();
                    await ReloadMembersAsync();
                });

                menu.Items.Add("Cấp quyền bạc", null, async (_, __) =>
                {
                    await _groupService.UpdateMemberRoleAsync(_groupId, userName, true, "silver");
                    await LoadGroupAsync();
                    await ReloadMembersAsync();
                });

                menu.Items.Add("Cấp quyền vàng", null, async (_, __) =>
                {
                    await _groupService.UpdateMemberRoleAsync(_groupId, userName, true, "gold");
                    await LoadGroupAsync();
                    await ReloadMembersAsync();
                });

                if (!string.Equals(userName, _currentUser, StringComparison.OrdinalIgnoreCase))
                {
                    menu.Items.Add("Nhượng quyền chủ nhóm cho " + userName, null, async (_, __) =>
                    {
                        if (MessageBox.Show("Nhượng quyền chủ nhóm cho " + userName + " ?",
                                "Xác nhận",
                                MessageBoxButtons.YesNo,
                                MessageBoxIcon.Question) != DialogResult.Yes)
                            return;

                        await _groupService.TransferOwnershipAsync(_groupId, _currentUser, userName);
                        await LoadGroupAsync();
                        await ReloadMembersAsync();
                    });
                }
            }

            card.ContextMenuStrip = menu;

            // Đảm bảo hover/card-click cho các child
            foreach (Control child in new Control[] { avatar, textArea, lblName, lblRole })
            {
                child.MouseEnter += (s, e) => card.BackColor = Color.FromArgb(232, 240, 255);
                child.MouseLeave += (s, e) => card.BackColor = Color.FromArgb(245, 248, 252);
                child.Click += async (s, e) => await RemoveMemberAsync();
            }

            card.Controls.Add(avatar);
            card.Controls.Add(textArea);
            card.Controls.Add(btnRemove);

            return card;
        }

        // ================== SEARCH USER & THÊM THÀNH VIÊN (CARD STYLE) ==================

        private static string Normalize(string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return string.Empty;
            s = s.Trim().ToLowerInvariant();
            return s.Replace(" ", "");
        }

        private async Task SearchUsersAsync(string keyword)
        {
            flpSearch.Controls.Clear();

            keyword = (keyword ?? string.Empty).Trim();
            string normKey = Normalize(keyword);
            if (normKey.Length == 0 || _group == null)
                return;

            var res = await _firebase.GetAsync("users");
            var dict = res.ResultAs<Dictionary<string, User>>()
                       ?? new Dictionary<string, User>();

            var users = dict.Values
                .Where(u => u != null && !string.IsNullOrWhiteSpace(u.Ten))
                .GroupBy(u => u.Ten, StringComparer.OrdinalIgnoreCase)
                .Select(g => g.First());

            foreach (var u in users)
            {
                string normName = Normalize(u.Ten);
                if (!normName.Contains(normKey))
                    continue;

                bool isMember = _group.thanhVien != null && _group.thanhVien.ContainsKey(u.Ten);
                Control row = CreateSearchRow(u.Ten, isMember);
                flpSearch.Controls.Add(row);
            }
        }

        private Control CreateSearchRow(string userName, bool isMember)
        {
            int panelWidth = Math.Max(flpSearch.ClientSize.Width, 260);

            var card = new Panel
            {
                Width = panelWidth - 8,
                Height = 64,
                Margin = new Padding(4, 3, 4, 3),
                BackColor = Color.FromArgb(245, 248, 252)
            };

            card.MouseEnter += (s, e) => card.BackColor = Color.FromArgb(232, 240, 255);
            card.MouseLeave += (s, e) => card.BackColor = Color.FromArgb(245, 248, 252);

            // avatar
            var avatar = new Panel
            {
                Width = 36,
                Height = 36,
                Left = 10,
                Top = 14,
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
                Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                Text = (userName ?? "?").Trim().Length > 0 ? userName.Trim()[0].ToString().ToUpper() : "?"
            };
            avatar.Controls.Add(lblInitial);

            // text area
            var textArea = new Panel
            {
                Left = 56,
                Top = 8,
                Height = 48,
                Width = card.Width - 56 - 110,
                BackColor = Color.Transparent
            };
            textArea.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;

            var lblName = new Label
            {
                AutoSize = false,
                Left = 0,
                Top = 0,
                Width = textArea.Width,
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 30, 30),
                Text = userName,
                AutoEllipsis = true
            };
            lblName.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;

            var lblStatus = new Label
            {
                AutoSize = false,
                Left = 0,
                Top = 22,
                Width = textArea.Width,
                Font = new Font("Segoe UI", 8.5f, FontStyle.Italic),
                ForeColor = isMember ? Color.FromArgb(34, 197, 94) : Color.FromArgb(100, 116, 139),
                AutoEllipsis = true,
                Text = isMember ? "Đã trong nhóm" : "Người dùng · bấm Thêm để mời vào nhóm"
            };
            lblStatus.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;

            textArea.Controls.Add(lblName);
            textArea.Controls.Add(lblStatus);

            // nút Thêm
            var btn = new Guna2Button
            {
                Width = 90,
                Height = 28,
                Top = 18,
                BorderRadius = 12,
                Font = new Font("Segoe UI", 8.5F),
                Cursor = Cursors.Hand,
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            btn.Left = card.Width - btn.Width - 10;

            if (isMember)
            {
                btn.Text = "Đã thêm";
                btn.Enabled = false;
                btn.FillColor = Color.FromArgb(148, 163, 184);
                btn.ForeColor = Color.White;
            }
            else
            {
                btn.Text = "Thêm";
                btn.FillColor = Color.FromArgb(9, 132, 227);
                btn.HoverState.FillColor = Color.FromArgb(116, 185, 255);
                btn.ForeColor = Color.White;

                btn.Click += async delegate
                {
                    if (_requireConfirmOnAdd)
                    {
                        if (MessageBox.Show($"Thêm {userName} vào nhóm?",
                                "Xác nhận",
                                MessageBoxButtons.YesNo,
                                MessageBoxIcon.Question) != DialogResult.Yes)
                            return;
                    }

                    await _groupService.AddMemberAsync(_groupId, userName, false, "member");
                    await LoadGroupAsync();
                    await ReloadMembersAsync();
                    await SearchUsersAsync(txtSearch.Text);
                };
            }

            // resize
            card.Resize += (s, e) =>
            {
                textArea.Width = card.Width - 56 - 110;
                lblName.Width = textArea.Width;
                lblStatus.Width = textArea.Width;
                btn.Left = card.Width - btn.Width - 10;
            };

            card.Controls.Add(avatar);
            card.Controls.Add(textArea);
            card.Controls.Add(btn);

            // đảm bảo hover cho child
            foreach (Control child in new Control[] { avatar, textArea, lblName, lblStatus })
            {
                child.MouseEnter += (s, e) => card.BackColor = Color.FromArgb(232, 240, 255);
                child.MouseLeave += (s, e) => card.BackColor = Color.FromArgb(245, 248, 252);
            }

            return card;
        }

        // ================== SETTINGS (VÀNG BẬT/TẮT) ==================

        private async void ChkAdminOnlyChat_CheckedChanged(object sender, EventArgs e)
        {
            if (_suppressSettingsEvents) return;

            if (!_isGoldAdmin)
            {
                _suppressSettingsEvents = true;
                chkAdminOnlyChat.Checked = !chkAdminOnlyChat.Checked;
                _suppressSettingsEvents = false;
                return;
            }

            await _groupService.SetAdminOnlyChatAsync(_groupId, chkAdminOnlyChat.Checked);
        }

        private async void ChkRequireApproval_CheckedChanged(object sender, EventArgs e)
        {
            if (_suppressSettingsEvents) return;

            if (!_isGoldAdmin)
            {
                _suppressSettingsEvents = true;
                chkRequireApproval.Checked = !chkRequireApproval.Checked;
                _suppressSettingsEvents = false;
                return;
            }

            await _groupService.SetRequireApprovalAsync(_groupId, chkRequireApproval.Checked);
        }
    }
}
