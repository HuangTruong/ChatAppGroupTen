using ChatApp.Models.Messages;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;

namespace ChatApp.Services.UI
{
    /// <summary>
    /// Vẽ tin nhắn trực tiếp lên pnlKhungChat (Panel) KHÔNG dùng FlowLayoutPanel.
    /// - RenderInitial: load lịch sử 1 lần
    /// - QueueAppend: append realtime (batch bằng Timer)
    /// - Dùng "row panel" để bubble (dock trái/phải) không bị chồng layout
    /// </summary>
    public sealed class ChatRenderer : IDisposable
    {
        #region ====== FIELDS ======

        private readonly Panel _hostPanel;

        /// <summary>
        /// Factory tạo Control bubble từ ChatMessage (ví dụ: MessageBubbles).
        /// ChatRenderer sẽ tự add bubble vào UI theo layout dọc.
        /// </summary>
        private readonly Func<ChatMessage, Control> _bubbleFactory;

        private readonly object _lockObj = new object();
        private readonly List<ChatMessage> _pending = new List<ChatMessage>();
        private readonly HashSet<string> _drawnIds = new HashSet<string>(StringComparer.Ordinal);

        private readonly Timer _flushTimer;

        private string _ownerKey;

        private int _padding = 8;
        private int _spacing = 6;
        private int _nextTop = 8;

        #endregion

        #region ====== PROPERTIES ======

        /// <summary>
        /// Giới hạn số tin giữ trên UI (để tránh nặng dần).
        /// </summary>
        public int MaxUiMessages { get; set; }

        #endregion

        #region ====== CTOR / DISPOSE ======

        public ChatRenderer(Panel hostPanel, Func<ChatMessage, Control> bubbleFactory)
        {
            if (hostPanel == null) throw new ArgumentNullException("hostPanel");
            if (bubbleFactory == null) throw new ArgumentNullException("bubbleFactory");

            _hostPanel = hostPanel;
            _bubbleFactory = bubbleFactory;

            MaxUiMessages = 300;

            _hostPanel.AutoScroll = true;
            _hostPanel.Padding = new Padding(0);
            EnableDoubleBuffered(_hostPanel);

            _hostPanel.Resize -= HostPanel_Resize;
            _hostPanel.Resize += HostPanel_Resize;

            _flushTimer = new Timer();
            _flushTimer.Interval = 60;
            _flushTimer.Tick += FlushTimer_Tick;

            ResetLayoutCursor();
        }

        public void Dispose()
        {
            try
            {
                if (_flushTimer != null)
                {
                    _flushTimer.Stop();
                    _flushTimer.Tick -= FlushTimer_Tick;
                    _flushTimer.Dispose();
                }
            }
            catch { }

            try
            {
                _hostPanel.Resize -= HostPanel_Resize;
            }
            catch { }
        }

        #endregion

        #region ====== PUBLIC API ======

        /// <summary>
        /// Xóa sạch UI chat.
        /// </summary>
        public void Clear()
        {
            RunOnUi(delegate
            {
                lock (_lockObj)
                {
                    _pending.Clear();
                    _drawnIds.Clear();
                    _ownerKey = null;
                }

                try { _flushTimer.Stop(); } catch { }

                try
                {
                    _hostPanel.SuspendLayout();
                    _hostPanel.Controls.Clear();
                    // Reset scroll về đầu để tránh trường hợp AutoScrollPosition cũ làm control mới bị "lệch" khỏi vùng nhìn.
                    try { _hostPanel.AutoScrollPosition = new Point(0, 0); } catch { }
                    ResetLayoutCursor();
                }
                finally
                {
                    _hostPanel.ResumeLayout(true);
                }
            });
        }

        /// <summary>
        /// Render toàn bộ lịch sử (khi mới mở hội thoại).
        /// </summary>
        public void RenderInitial(IList<ChatMessage> messages, string ownerKey)
        {
            RunOnUi(delegate
            {
                _ownerKey = ownerKey;

                lock (_lockObj)
                {
                    _pending.Clear();
                    _drawnIds.Clear();
                }

                _flushTimer.Stop();

                _hostPanel.SuspendLayout();
                try
                {
                    _hostPanel.Controls.Clear();
                    try { _hostPanel.AutoScrollPosition = new Point(0, 0); } catch { }
                    ResetLayoutCursor();

                    if (messages != null)
                    {
                        for (int i = 0; i < messages.Count; i++)
                        {
                            ChatMessage m = messages[i];
                            if (m == null) continue;

                            string id = SafeMessageId(m);
                            if (!string.IsNullOrEmpty(id))
                            {
                                lock (_lockObj)
                                {
                                    _drawnIds.Add(id);
                                }
                            }

                            AddOneMessageRow(m);
                        }
                    }
                }
                finally
                {
                    _hostPanel.ResumeLayout(true);
                }

                TrimIfNeeded();
                ScrollToBottom(true);
            });
        }

        /// <summary>
        /// Append tin mới lên UI (được batch bằng Timer).
        /// </summary>
        public void QueueAppend(ChatMessage msg, string ownerKey)
        {
            if (msg == null) return;

            RunOnUi(delegate
            {
                // Nếu vừa Clear() và chưa RenderInitial(), _ownerKey đang null.
                // SessionController đã lọc đúng ownerKey rồi, nên ở đây có thể "nhận" append đầu tiên
                // để không bị mất tin nhắn khi listener bắn onMessageAdded trước onInitialLoaded.
                if (string.IsNullOrEmpty(_ownerKey))
                {
                    _ownerKey = ownerKey;
                }
                else
                {
                    if (!string.Equals(_ownerKey, ownerKey, StringComparison.Ordinal))
                    {
                        return;
                    }
                }

                string id = SafeMessageId(msg);

                lock (_lockObj)
                {
                    if (!string.IsNullOrEmpty(id) && _drawnIds.Contains(id))
                    {
                        return;
                    }

                    _pending.Add(msg);

                    if (!string.IsNullOrEmpty(id))
                    {
                        _drawnIds.Add(id);
                    }

                    if (!_flushTimer.Enabled)
                    {
                        _flushTimer.Start();
                    }
                }
            });
        }

        #endregion

        #region ====== TIMER FLUSH ======

        private void FlushTimer_Tick(object sender, EventArgs e)
        {
            List<ChatMessage> batch;
            bool shouldScroll;

            lock (_lockObj)
            {
                _flushTimer.Stop();

                if (_pending.Count == 0)
                {
                    return;
                }

                batch = new List<ChatMessage>(_pending);
                _pending.Clear();
            }

            shouldScroll = IsNearBottom();

            _hostPanel.SuspendLayout();
            try
            {
                for (int i = 0; i < batch.Count; i++)
                {
                    AddOneMessageRow(batch[i]);
                }
            }
            finally
            {
                _hostPanel.ResumeLayout(true);
            }

            TrimIfNeeded();

            if (shouldScroll)
            {
                ScrollToBottom(false);
            }
        }

        #endregion

        #region ====== LAYOUT CORE ======

        private void AddOneMessageRow(ChatMessage msg)
        {
            if (msg == null) return;

            Control bubble = null;
            try { bubble = _bubbleFactory(msg); }
            catch { bubble = null; }

            if (bubble == null) return;

            // Row panel dùng layout thủ công: Panel.AutoSize + Dock(Left/Right) thường gây height=0.
            // => Ta đặt row.Height theo bubble.Height và tự canh trái/phải.
            Panel row = new Panel();
            row.BackColor = Color.Transparent;
            row.AutoSize = false;
            row.Margin = new Padding(0);
            row.Padding = new Padding(_padding, 2, _padding, 2);
            row.Tag = msg;

            // Override Dock của bubble (MessageBubbles hay set Dock Left/Right để dùng với FlowLayoutPanel).
            // Với Panel + row, ta tự canh vị trí để bubble không bị co height về 0.
            try { bubble.Dock = DockStyle.None; } catch { }
            bubble.Margin = new Padding(0);

            row.Left = 0;
            row.Top = _nextTop;
            row.Width = _hostPanel.ClientSize.Width;
            row.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;

            // Add bubble trước để có size hợp lệ
            row.Controls.Add(bubble);

            // Set vị trí bubble theo IsMine
            bool isMine = false;
            try { isMine = msg.IsMine; } catch { isMine = false; }

            // Đặt Top theo padding
            bubble.Top = row.Padding.Top;

            if (isMine)
            {
                bubble.Anchor = AnchorStyles.Top | AnchorStyles.Right;
                bubble.Left = Math.Max(row.Padding.Left, row.ClientSize.Width - row.Padding.Right - bubble.Width);
            }
            else
            {
                bubble.Anchor = AnchorStyles.Top | AnchorStyles.Left;
                bubble.Left = row.Padding.Left;
            }

            // Set height theo bubble hiện tại
            int h = bubble.Height + row.Padding.Vertical;
            if (h < 10) h = 10;
            row.Height = h;

            // Nếu bubble đổi height (load thumbnail / update name), resize row + reflow
            bubble.SizeChanged += delegate
            {
                RunOnUi(delegate
                {
                    try
                    {
                        if (row.IsDisposed || bubble.IsDisposed) return;

                        int nh = bubble.Height + row.Padding.Vertical;
                        if (nh < 10) nh = 10;
                        if (row.Height != nh)
                        {
                            row.Height = nh;
                            ReflowAll();
                        }

                        // Re-align when row width changes
                        if (isMine)
                        {
                            bubble.Left = Math.Max(row.Padding.Left, row.ClientSize.Width - row.Padding.Right - bubble.Width);
                        }
                    }
                    catch { }
                });
            };

            // Re-align khi row resize (đổi size form)
            row.Resize += delegate
            {
                try
                {
                    if (bubble.IsDisposed) return;
                    if (isMine)
                    {
                        bubble.Left = Math.Max(row.Padding.Left, row.ClientSize.Width - row.Padding.Right - bubble.Width);
                    }
                }
                catch { }
            };

            _hostPanel.Controls.Add(row);

            _nextTop = row.Bottom + _spacing;
        }

        private void ResetLayoutCursor()
        {
            _nextTop = _padding;
        }

        private void ReflowAll()
        {
            int y = _padding;

            for (int i = 0; i < _hostPanel.Controls.Count; i++)
            {
                Control row = _hostPanel.Controls[i];
                if (row == null) continue;

                row.Left = 0;
                row.Top = y;
                row.Width = _hostPanel.ClientSize.Width;

                y = row.Bottom + _spacing;
            }

            _nextTop = y;
        }

        private void TrimIfNeeded()
        {
            try
            {
                int max = MaxUiMessages;
                if (max <= 0) return;

                int count = _hostPanel.Controls.Count;
                if (count <= max) return;

                int removeCount = count - max;
                if (removeCount <= 0) return;

                _hostPanel.SuspendLayout();
                try
                {
                    for (int i = 0; i < removeCount; i++)
                    {
                        Control row = _hostPanel.Controls[0];
                        _hostPanel.Controls.RemoveAt(0);

                        ChatMessage m = row != null ? (row.Tag as ChatMessage) : null;
                        string id = SafeMessageId(m);

                        if (!string.IsNullOrEmpty(id))
                        {
                            lock (_lockObj)
                            {
                                _drawnIds.Remove(id);
                            }
                        }

                        try { row.Dispose(); } catch { }
                    }

                    // Sau khi remove -> reflow để không bị hở khoảng trống
                    ReflowAll();
                }
                finally
                {
                    _hostPanel.ResumeLayout(true);
                }
            }
            catch { }
        }

        #endregion

        #region ====== SCROLL HELPERS ======

        private bool IsNearBottom()
        {
            try
            {
                if (_hostPanel.Controls.Count == 0) return true;
                if (_hostPanel.VerticalScroll == null) return true;
                if (!_hostPanel.VerticalScroll.Visible) return true;

                int value = _hostPanel.VerticalScroll.Value;
                int large = _hostPanel.VerticalScroll.LargeChange;
                int max = _hostPanel.VerticalScroll.Maximum;

                return (value + large) >= (max - 40);
            }
            catch
            {
                return true;
            }
        }

        private void ScrollToBottom(bool force)
        {
            try
            {
                if (!force && !IsNearBottom()) return;
                if (_hostPanel.Controls.Count == 0) return;

                Control last = _hostPanel.Controls[_hostPanel.Controls.Count - 1];
                if (last != null)
                {
                    _hostPanel.ScrollControlIntoView(last);
                }
            }
            catch { }
        }

        #endregion

        #region ====== EVENTS ======

        private void HostPanel_Resize(object sender, EventArgs e)
        {
            // Khi resize panel, cần reflow lại width của row
            RunOnUi(delegate
            {
                try
                {
                    _hostPanel.SuspendLayout();
                    ReflowAll();
                }
                finally
                {
                    _hostPanel.ResumeLayout(true);
                }
            });
        }

        #endregion

        #region ====== UTILS ======

        private void RunOnUi(Action action)
        {
            if (action == null) return;

            try
            {
                if (_hostPanel.IsDisposed) return;

                if (_hostPanel.InvokeRequired)
                {
                    _hostPanel.BeginInvoke(action);
                    return;
                }

                action();
            }
            catch { }
        }

        private static string SafeMessageId(ChatMessage msg)
        {
            try { return msg != null ? (msg.MessageId ?? string.Empty) : string.Empty; }
            catch { return string.Empty; }
        }

        private static void EnableDoubleBuffered(Control control)
        {
            if (control == null) return;

            try
            {
                PropertyInfo p = control.GetType().GetProperty(
                    "DoubleBuffered",
                    BindingFlags.Instance | BindingFlags.NonPublic);

                if (p != null)
                {
                    p.SetValue(control, true, null);
                }
            }
            catch { }
        }

        #endregion
    }
}
