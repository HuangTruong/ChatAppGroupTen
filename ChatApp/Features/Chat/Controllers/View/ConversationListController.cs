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
    /// Controller quản lý hiển thị danh sách cuộc trò chuyện (friends + groups)
    /// lên Panel (pnlDanhSachChat) theo dạng mỗi item 1 dòng.
    /// </summary>
    public class ConversationListController : IDisposable
    {
        #region ====== FIELDS ======

        private readonly Panel _panel;
        private readonly Func<Task<Dictionary<string, User>>> _loadFriendsAsync;
        private readonly Func<Task<Dictionary<string, GroupInfo>>> _loadGroupsAsync;
        private readonly EventHandler _onItemClicked;

        private bool _isHookedResize;

        #endregion

        #region ====== PROPERTIES ======

        /// <summary>
        /// Prefix tag để phân biệt item nhóm trong danh sách.
        /// </summary>
        public string GroupTagPrefix { get; set; }

        /// <summary>
        /// Danh sách bạn bè đã load (key = userId/safeId).
        /// </summary>
        public Dictionary<string, User> Friends { get; private set; }

        /// <summary>
        /// Danh sách nhóm đã load (key = groupId).
        /// </summary>
        public Dictionary<string, GroupInfo> Groups { get; private set; }

        #endregion

        #region ====== CTOR ======

        /// <summary>
        /// Khởi tạo controller cho danh sách conversation.
        /// </summary>
        /// <param name="panel">Panel dùng để chứa item Conversations (pnlDanhSachChat)</param>
        /// <param name="loadFriendsAsync">Hàm load friend users</param>
        /// <param name="loadGroupsAsync">Hàm load groups</param>
        /// <param name="onItemClicked">Handler click item (UserItem_Click)</param>
        /// <param name="groupTagPrefix">Prefix tag nhóm, ví dụ "GROUP:"</param>
        public ConversationListController(
            Panel panel,
            Func<Task<Dictionary<string, User>>> loadFriendsAsync,
            Func<Task<Dictionary<string, GroupInfo>>> loadGroupsAsync,
            EventHandler onItemClicked,
            string groupTagPrefix)
        {
            _panel = panel;
            _loadFriendsAsync = loadFriendsAsync;
            _loadGroupsAsync = loadGroupsAsync;
            _onItemClicked = onItemClicked;

            GroupTagPrefix = string.IsNullOrWhiteSpace(groupTagPrefix) ? "GROUP:" : groupTagPrefix;

            Friends = new Dictionary<string, User>(StringComparer.Ordinal);
            Groups = new Dictionary<string, GroupInfo>(StringComparer.Ordinal);

            HookPanelResize();
        }

        #endregion

        #region ====== PUBLIC API ======

        /// <summary>
        /// Reload toàn bộ danh sách: Groups trước, Friends sau.
        /// (Đảm bảo mọi thao tác UI đều chạy trên UI thread)
        /// </summary>
        public async Task ReloadAsync()
        {
            EnsurePanelValid();

            // 1) Load data ở background thread (KHÔNG đụng UI)
            await LoadGroupsInternalAsync().ConfigureAwait(false);
            await LoadFriendsInternalAsync().ConfigureAwait(false);

            // 2) Render UI trên UI thread
            await RunOnUiThreadAsync(delegate
            {
                EnsurePanelValid();

                _panel.AutoScroll = true;

                _panel.SuspendLayout();
                try
                {
                    _panel.Controls.Clear();

                    AddGroupItemsToPanel_UI();
                    AddFriendItemsToPanel_UI();
                }
                finally
                {
                    _panel.ResumeLayout(true);
                }
            }).ConfigureAwait(false);
        }

        /// <summary>
        /// Lấy User theo userId đã cache.
        /// </summary>
        public bool TryGetFriend(string userId, out User user)
        {
            user = null;
            if (string.IsNullOrWhiteSpace(userId)) return false;
            return Friends != null && Friends.TryGetValue(userId, out user);
        }

        /// <summary>
        /// Lấy Group theo groupId đã cache.
        /// </summary>
        public bool TryGetGroup(string groupId, out GroupInfo group)
        {
            group = null;
            if (string.IsNullOrWhiteSpace(groupId)) return false;
            return Groups != null && Groups.TryGetValue(groupId, out group);
        }

        #endregion

        #region ====== INTERNAL LOAD ======

        private async Task LoadFriendsInternalAsync()
        {
            Friends = new Dictionary<string, User>(StringComparer.Ordinal);

            if (_loadFriendsAsync == null) return;

            Dictionary<string, User> data = await _loadFriendsAsync().ConfigureAwait(false);
            if (data == null) return;

            foreach (KeyValuePair<string, User> kv in data)
            {
                if (string.IsNullOrWhiteSpace(kv.Key)) continue;

                User u = kv.Value;
                if (u != null) u.LocalId = kv.Key;

                Friends[kv.Key] = u;
            }
        }

        private async Task LoadGroupsInternalAsync()
        {
            Groups = new Dictionary<string, GroupInfo>(StringComparer.Ordinal);

            if (_loadGroupsAsync == null) return;

            Dictionary<string, GroupInfo> data = await _loadGroupsAsync().ConfigureAwait(false);
            if (data == null) return;

            foreach (KeyValuePair<string, GroupInfo> kv in data)
            {
                if (string.IsNullOrWhiteSpace(kv.Key)) continue;
                if (kv.Value == null) continue;

                Groups[kv.Key] = kv.Value;
            }
        }

        #endregion

        #region ====== BUILD ITEMS (UI THREAD ONLY) ======

        /// <summary>
        /// CHỈ gọi trên UI thread.
        /// </summary>
        private void AddGroupItemsToPanel_UI()
        {
            if (Groups == null || Groups.Count == 0) return;

            // Sort group theo LastMessageAt giảm dần
            List<GroupInfo> list = Groups.Values.ToList();
            list.Sort(delegate (GroupInfo a, GroupInfo b)
            {
                long ta = a != null ? a.LastMessageAt : 0;
                long tb = b != null ? b.LastMessageAt : 0;
                return tb.CompareTo(ta);
            });

            for (int i = 0; i < list.Count; i++)
            {
                GroupInfo g = list[i];
                if (g == null) continue;

                Conversations item = CreateBaseItem_UI();
                item.Tag = GroupTagPrefix + g.GroupId;

                string title = string.IsNullOrWhiteSpace(g.Name) ? ("Nhóm " + g.GroupId) : g.Name;

                string subtitle = g.LastMessage;
                if (string.IsNullOrWhiteSpace(subtitle))
                {
                    subtitle = g.MemberCount > 0 ? (g.MemberCount + " thành viên") : "Nhóm chat";
                }

                item.SetInfo(title, subtitle);

                AddToTop_UI(item);
            }
        }

        /// <summary>
        /// CHỈ gọi trên UI thread.
        /// </summary>
        private void AddFriendItemsToPanel_UI()
        {
            if (Friends == null || Friends.Count == 0) return;

            foreach (KeyValuePair<string, User> kv in Friends)
            {
                string userId = kv.Key;
                User user = kv.Value;

                Conversations item = CreateBaseItem_UI();
                item.Tag = userId;

                item.SetInfo(GetUserFullName(user), GetUserSubtitle(user, userId));

                AddToTop_UI(item);
            }
        }

        /// <summary>
        /// Tạo item Conversations (CHỈ UI thread).
        /// </summary>
        private Conversations CreateBaseItem_UI()
        {
            Conversations item = new Conversations();

            // Click event (đảm bảo không bị gắn nhiều lần)
            if (_onItemClicked != null)
            {
                item.ItemClicked -= _onItemClicked;
                item.ItemClicked += _onItemClicked;
            }

            // Quan trọng: Panel + Dock Top => mỗi item 1 dòng (vertical stack)
            item.Dock = DockStyle.Top;

            // Nếu Panel có Padding, để margin 0 cho đẹp
            item.Margin = new Padding(0);

            // Để panel tự stretch ngang
            item.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
            item.Width = GetPanelClientWidth();

            item.Cursor = Cursors.Hand;

            return item;
        }

        /// <summary>
        /// Add item vào Panel theo đúng thứ tự hiển thị top-to-bottom.
        /// Lưu ý: Dock Top + Controls.Add sẽ xếp ngược, nên phải BringToFront.
        /// (CHỈ UI thread)
        /// </summary>
        private void AddToTop_UI(Control c)
        {
            _panel.Controls.Add(c);
            c.BringToFront();
        }

        #endregion

        #region ====== UI THREAD HELPERS ======

        /// <summary>
        /// Chạy action trên UI thread của _panel. Nếu đang ở background thread sẽ BeginInvoke.
        /// </summary>
        private void RunOnUiThread(Action action)
        {
            if (action == null) return;
            if (_panel == null) return;
            if (_panel.IsDisposed) return;

            if (_panel.InvokeRequired)
            {
                try
                {
                    _panel.BeginInvoke((MethodInvoker)delegate
                    {
                        try { action(); } catch { /* ignore */ }
                    });
                }
                catch
                {
                    // ignore
                }
                return;
            }

            action();
        }

        /// <summary>
        /// Chạy action trên UI thread và trả về Task để await.
        /// </summary>
        private Task RunOnUiThreadAsync(Action action)
        {
            TaskCompletionSource<object> tcs = new TaskCompletionSource<object>();

            RunOnUiThread(delegate
            {
                try
                {
                    action();
                    tcs.SetResult(null);
                }
                catch (Exception ex)
                {
                    tcs.SetException(ex);
                }
            });

            return tcs.Task;
        }

        #endregion

        #region ====== HELPERS ======

        private void EnsurePanelValid()
        {
            if (_panel == null)
            {
                throw new InvalidOperationException("ConversationListController: panel is null.");
            }
        }

        private int GetPanelClientWidth()
        {
            int w = _panel.ClientSize.Width;
            if (w <= 0) w = _panel.Width;
            if (w <= 0) w = 320;
            return w;
        }

        private static string GetUserFullName(User user)
        {
            if (user == null) return "Người dùng";

            string ten = user.FullName;

            if (string.IsNullOrWhiteSpace(ten))
            {
                ten = user.DisplayName;
            }

            if (string.IsNullOrWhiteSpace(ten) && !string.IsNullOrWhiteSpace(user.Email))
            {
                string email = user.Email.Trim();
                int at = email.IndexOf('@');
                ten = (at > 0) ? email.Substring(0, at) : email;
            }

            if (string.IsNullOrWhiteSpace(ten)) return "Người dùng";

            return ten.Trim();
        }

        private static string GetUserSubtitle(User user, string userId)
        {
            if (user != null && !string.IsNullOrWhiteSpace(user.Email))
            {
                return user.Email.Trim();
            }
            return string.IsNullOrWhiteSpace(userId) ? string.Empty : userId;
        }

        private void HookPanelResize()
        {
            if (_panel == null) return;
            if (_isHookedResize) return;

            _panel.SizeChanged += Panel_SizeChanged;
            _isHookedResize = true;
        }

        private void Panel_SizeChanged(object sender, EventArgs e)
        {
            // Nếu event bị gọi từ thread khác (hiếm nhưng có thể), marshal về UI thread
            if (_panel != null && _panel.InvokeRequired)
            {
                try
                {
                    _panel.BeginInvoke((MethodInvoker)delegate
                    {
                        try { Panel_SizeChanged(sender, e); } catch { /* ignore */ }
                    });
                }
                catch
                {
                    // ignore
                }
                return;
            }

            try
            {
                int w = GetPanelClientWidth();
                foreach (Control c in _panel.Controls)
                {
                    if (c == null) continue;
                    c.Width = w; // Dock Top đã đủ, nhưng set thêm để chắc chắn
                }
            }
            catch
            {
                // ignore
            }
        }

        #endregion

        #region ====== DISPOSE ======

        public void Dispose()
        {
            try
            {
                if (_panel != null && _isHookedResize)
                {
                    _panel.SizeChanged -= Panel_SizeChanged;
                    _isHookedResize = false;
                }
            }
            catch
            {
                // ignore
            }
        }

        #endregion
    }
}
