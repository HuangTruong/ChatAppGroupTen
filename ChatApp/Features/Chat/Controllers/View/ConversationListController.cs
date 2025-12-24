using ChatApp.Controls;
using ChatApp.Models.Groups;
using ChatApp.Models.Users;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ChatApp.Controllers
{
    /// <summary>
    /// Controller hiển thị danh sách hội thoại ở cột trái:
    /// - Gồm: Nhóm (Groups) + Bạn bè (Friends)
    /// - Render thành list dọc trên Panel (tự tính Location/Width, KHÔNG dùng Dock để tránh lỗi layout WinForms)
    /// - Hỗ trợ ReloadAsync(): tải dữ liệu + sắp xếp + vẽ lại UI
    /// - Mỗi item gắn Tag:
    ///   + Nhóm: "GROUP:{groupId}"
    ///   + Bạn: "{userId}"
    /// </summary>
    public class ConversationListController : IDisposable
    {
        #region ====== HẰNG SỐ GIAO DIỆN ======

        private const int ITEM_HEIGHT = 76;
        private const int ITEM_GAP = 6;
        private const int MIN_ITEM_WIDTH = 100;

        #endregion

        #region ====== BIẾN THÀNH VIÊN ======

        private readonly Panel _panel;
        private readonly Func<Task<Dictionary<string, User>>> _loadFriendsAsync;
        private readonly Func<Task<Dictionary<string, GroupInfo>>> _loadGroupsAsync;
        private readonly EventHandler _onItemClicked;

        private bool _disposed;

        #endregion

        #region ====== THUỘC TÍNH CÔNG KHAI ======

        /// <summary>
        /// Prefix để phân biệt nhóm trong Tag (vd: "GROUP:")
        /// </summary>
        public string GroupTagPrefix { get; set; }

        /// <summary>
        /// Cache bạn bè (key = userId).
        /// </summary>
        public Dictionary<string, User> Friends { get; private set; }

        /// <summary>
        /// Cache nhóm (key = groupId).
        /// </summary>
        public Dictionary<string, GroupInfo> Groups { get; private set; }

        #endregion

        #region ====== HÀM KHỞI TẠO ======

        public ConversationListController(
            Panel panel,
            Func<Task<Dictionary<string, User>>> loadFriendsAsync,
            Func<Task<Dictionary<string, GroupInfo>>> loadGroupsAsync,
            EventHandler onItemClicked,
            string groupTagPrefix)
        {
            if (panel == null) throw new ArgumentNullException(nameof(panel));

            _panel = panel;
            _loadFriendsAsync = loadFriendsAsync;
            _loadGroupsAsync = loadGroupsAsync;
            _onItemClicked = onItemClicked;

            GroupTagPrefix = string.IsNullOrWhiteSpace(groupTagPrefix) ? "GROUP:" : groupTagPrefix;

            Friends = new Dictionary<string, User>(StringComparer.Ordinal);
            Groups = new Dictionary<string, GroupInfo>(StringComparer.Ordinal);

            _panel.AutoScroll = true;

            _panel.Resize -= Panel_Resize;
            _panel.Resize += Panel_Resize;
        }

        #endregion

        #region ====== API CHÍNH: TẢI LẠI & VẼ LẠI ======

        public async Task ReloadAsync()
        {
            EnsureNotDisposed();

            await Task.WhenAll(
                SafeLoadFriendsAsync(),
                SafeLoadGroupsAsync()
            ).ConfigureAwait(false);

            List<ListItem> items = BuildItems();

            await RunOnUiThreadAsync(delegate
            {
                Render_UI(items);
            }).ConfigureAwait(false);
        }

        #endregion

        #region ====== TẢI DỮ LIỆU AN TOÀN (TRY/CATCH) ======

        private async Task SafeLoadFriendsAsync()
        {
            try { await LoadFriendsInternalAsync().ConfigureAwait(false); }
            catch { Friends = new Dictionary<string, User>(StringComparer.Ordinal); }
        }

        private async Task SafeLoadGroupsAsync()
        {
            try { await LoadGroupsInternalAsync().ConfigureAwait(false); }
            catch { Groups = new Dictionary<string, GroupInfo>(StringComparer.Ordinal); }
        }

        private async Task LoadFriendsInternalAsync()
        {
            Friends = new Dictionary<string, User>(StringComparer.Ordinal);
            if (_loadFriendsAsync == null) return;

            var data = await _loadFriendsAsync().ConfigureAwait(false);
            if (data == null) return;

            foreach (var kv in data)
            {
                if (string.IsNullOrWhiteSpace(kv.Key)) continue;
                if (kv.Value != null) kv.Value.LocalId = kv.Key;
                Friends[kv.Key] = kv.Value;
            }
        }

        private async Task LoadGroupsInternalAsync()
        {
            Groups = new Dictionary<string, GroupInfo>(StringComparer.Ordinal);
            if (_loadGroupsAsync == null) return;

            var data = await _loadGroupsAsync().ConfigureAwait(false);
            if (data == null) return;

            foreach (var kv in data)
            {
                if (string.IsNullOrWhiteSpace(kv.Key) || kv.Value == null) continue;
                Groups[kv.Key] = kv.Value;
            }
        }

        #endregion

        #region ====== TẠO DANH SÁCH ITEM ĐỂ RENDER ======

        private sealed class ListItem
        {
            public int Kind; // 0 = group, 1 = friend
            public string Tag;
            public string Title;
            public string AvatarId;
            public long SortKey;
        }

        private List<ListItem> BuildItems()
        {
            var result = new List<ListItem>();

            // Groups (ưu tiên lên trước theo LastMessageAt giảm dần)
            if (Groups != null)
            {
                foreach (var kv in Groups)
                {
                    var g = kv.Value;
                    if (g == null) continue;

                    result.Add(new ListItem
                    {
                        Kind = 0,
                        Tag = GroupTagPrefix + kv.Key,
                        Title = string.IsNullOrWhiteSpace(g.Name) ? "Nhóm" : g.Name,
                        AvatarId = kv.Key,
                        SortKey = g.LastMessageAt
                    });
                }
            }

            // Friends (xếp theo tên A-Z)
            if (Friends != null)
            {
                foreach (var kv in Friends)
                {
                    var u = kv.Value;
                    if (u == null) continue;

                    result.Add(new ListItem
                    {
                        Kind = 1,
                        Tag = kv.Key,
                        Title = GetUserFullName(u),
                        AvatarId = kv.Key,
                        SortKey = 0
                    });
                }
            }

            var groups = result.Where(x => x.Kind == 0).OrderByDescending(x => x.SortKey).ToList();
            var friends = result.Where(x => x.Kind == 1)
                                .OrderBy(x => x.Title, StringComparer.CurrentCultureIgnoreCase)
                                .ToList();

            var ordered = new List<ListItem>();
            ordered.AddRange(groups);
            ordered.AddRange(friends);
            return ordered;
        }

        #endregion

        #region ====== VẼ UI (RENDER) ======

        private void Render_UI(List<ListItem> items)
        {
            if (_panel == null || _panel.IsDisposed || !_panel.IsHandleCreated)
                return;

            _panel.SuspendLayout();
            try
            {
                ClearPanel_UI();

                if (items == null || items.Count == 0)
                {
                    _panel.AutoScrollMinSize = Size.Empty;
                    return;
                }

                int y = 0;
                int w = CalcItemWidth();

                foreach (var m in items)
                {
                    var item = new Conversations
                    {
                        Tag = m.Tag,
                        AutoSize = false,
                        Width = w,
                        Height = ITEM_HEIGHT,
                        Location = new Point(0, y),
                        Margin = new Padding(0),
                        Cursor = Cursors.Hand
                    };

                    if (_onItemClicked != null)
                    {
                        item.ItemClicked -= _onItemClicked;
                        item.ItemClicked += _onItemClicked;
                    }

                    item.SetInfo(m.Title ?? string.Empty, m.AvatarId ?? string.Empty);

                    _panel.Controls.Add(item);
                    y += ITEM_HEIGHT + ITEM_GAP;
                }

                _panel.AutoScrollMinSize = new Size(0, Math.Max(0, y));
            }
            finally
            {
                _panel.ResumeLayout(true);
            }
        }

        private void ClearPanel_UI()
        {
            for (int i = _panel.Controls.Count - 1; i >= 0; i--)
            {
                try { _panel.Controls[i].Dispose(); } catch { }
            }
            _panel.Controls.Clear();
        }

        #endregion

        #region ====== TÍNH KÍCH THƯỚC & XỬ LÝ RESIZE ======

        private int CalcItemWidth()
        {
            if (_panel == null || _panel.IsDisposed || !_panel.IsHandleCreated)
                return MIN_ITEM_WIDTH;

            int w = _panel.ClientSize.Width;
            if (w <= 0) w = _panel.Width;

            w -= SystemInformation.VerticalScrollBarWidth;
            if (w < MIN_ITEM_WIDTH) w = MIN_ITEM_WIDTH;

            return w;
        }

        private void Panel_Resize(object sender, EventArgs e)
        {
            if (_disposed) return;
            if (_panel == null || _panel.IsDisposed || !_panel.IsHandleCreated) return;

            RunOnUiThread(delegate
            {
                int w = CalcItemWidth();
                foreach (Control c in _panel.Controls)
                {
                    if (c == null || c.IsDisposed) continue;
                    c.Width = w;
                    c.Left = 0;
                }
            });
        }

        #endregion

        #region ====== HỖ TRỢ CHẠY TRÊN UI THREAD ======

        private void RunOnUiThread(Action action)
        {
            if (action == null || _panel == null || _panel.IsDisposed) return;

            if (_panel.InvokeRequired)
            {
                try { _panel.BeginInvoke((MethodInvoker)(() => { try { action(); } catch { } })); }
                catch { }
                return;
            }

            action();
        }

        private Task RunOnUiThreadAsync(Action action)
        {
            var tcs = new TaskCompletionSource<object>();

            RunOnUiThread(delegate
            {
                try { action(); tcs.SetResult(null); }
                catch (Exception ex) { tcs.SetException(ex); }
            });

            return tcs.Task;
        }

        #endregion

        #region ====== HÀM TIỆN ÍCH ======

        private void EnsureNotDisposed()
        {
            if (_disposed) throw new ObjectDisposedException(nameof(ConversationListController));
        }

        private static string GetUserFullName(User user)
        {
            if (user == null) return "Người dùng";
            if (!string.IsNullOrWhiteSpace(user.FullName)) return user.FullName.Trim();
            if (!string.IsNullOrWhiteSpace(user.DisplayName)) return user.DisplayName.Trim();
            if (!string.IsNullOrWhiteSpace(user.Email))
            {
                int at = user.Email.IndexOf('@');
                return at > 0 ? user.Email.Substring(0, at) : user.Email;
            }
            return "Người dùng";
        }

        #endregion

        #region ====== GIẢI PHÓNG TÀI NGUYÊN ======

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            try { if (_panel != null) _panel.Resize -= Panel_Resize; } catch { }
        }

        #endregion
    }
}
