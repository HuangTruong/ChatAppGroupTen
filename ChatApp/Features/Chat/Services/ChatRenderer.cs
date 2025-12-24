using ChatApp.Models.Messages;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;

namespace ChatApp.Services.UI
{
    /// <summary>
    /// ChatRenderer: Vẽ tin nhắn trực tiếp lên Panel (pnlKhungChat).
    ///
    /// Ý tưởng chính:
    /// - Panel sẽ chứa nhiều "row panel" xếp dọc.
    /// - Mỗi row panel chứa 1 bubble (MessageBubbles hoặc control tương tự).
    /// - Bubble được canh trái/phải theo IsMine và KHÔNG dùng Dock để tránh lỗi WinForms (height=0/chồng layout).
    ///
    /// Luồng chính:
    /// - RenderInitial(): vẽ toàn bộ lịch sử 1 lần khi mở hội thoại.
    /// - QueueAppend(): nhận tin mới realtime -> đưa vào hàng đợi _pending.
    /// - Timer (Flush): gom nhiều tin và vẽ 1 lần để UI mượt hơn.
    /// </summary>
    public sealed class ChatRenderer : IDisposable
    {
        #region ====== BIẾN THÀNH VIÊN ======

        private readonly Panel _hostPanel;

        /// <summary>
        /// Hàm tạo bubble Control từ ChatMessage (ví dụ: MessageBubbles).
        /// ChatRenderer chỉ gọi factory và tự add + layout theo dạng list dọc.
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

        #region ====== THUỘC TÍNH ======

        /// <summary>
        /// Giới hạn số tin giữ trên UI (để tránh nặng dần).
        /// </summary>
        public int MaxUiMessages { get; set; }

        #endregion

        #region ====== KHỞI TẠO / GIẢI PHÓNG ======

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

        #region ====== API CÔNG KHAI ======

        /// <summary>
        /// Xóa toàn bộ UI chat + reset trạng thái nội bộ.
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

                    // Reset scroll để tránh "lệch" vùng nhìn sau khi clear
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
        /// Vẽ toàn bộ lịch sử (khi mới mở hội thoại).
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

                try { _flushTimer.Stop(); } catch { }

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
        /// Append tin mới lên UI (thực tế: đưa vào queue, rồi Timer sẽ vẽ theo batch).
        /// </summary>
        public void QueueAppend(ChatMessage msg, string ownerKey)
        {
            if (msg == null) return;

            RunOnUi(delegate
            {
                // Nếu chưa có _ownerKey (vừa Clear và chưa RenderInitial),
                // ta nhận ownerKey đầu tiên để không mất tin nhắn.
                if (string.IsNullOrEmpty(_ownerKey))
                {
                    _ownerKey = ownerKey;
                }
                else
                {
                    bool sameOwner = string.Equals(_ownerKey, ownerKey, StringComparison.Ordinal);
                    if (!sameOwner)
                    {
                        return;
                    }
                }

                string id = SafeMessageId(msg);

                lock (_lockObj)
                {
                    if (!string.IsNullOrEmpty(id))
                    {
                        bool existed = _drawnIds.Contains(id);
                        if (existed)
                        {
                            return;
                        }
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

        #region ====== TIMER: VẼ THEO BATCH ======

        private void FlushTimer_Tick(object sender, EventArgs e)
        {
            List<ChatMessage> batch = null;

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

            bool shouldScroll = IsNearBottom();

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

        #region ====== LAYOUT: THÊM 1 DÒNG TIN NHẮN ======

        private void AddOneMessageRow(ChatMessage msg)
        {
            if (msg == null) return;

            Control bubble = null;
            try
            {
                bubble = _bubbleFactory(msg);
            }
            catch
            {
                bubble = null;
            }

            if (bubble == null) return;

            // Row panel chứa bubble để tránh việc bubble Dock trái/phải làm chồng layout.
            Panel row = new Panel();
            row.BackColor = Color.Transparent;
            row.AutoSize = false;
            row.Margin = new Padding(0);
            row.Padding = new Padding(_padding, 2, _padding, 2);
            row.Tag = msg;

            // Tránh bubble tự Dock (cách cũ thường Dock Left/Right với FlowLayoutPanel)
            try { bubble.Dock = DockStyle.None; } catch { }
            bubble.Margin = new Padding(0);

            // Đặt row theo "con trỏ" layout hiện tại
            row.Left = 0;
            row.Top = _nextTop;
            row.Width = _hostPanel.ClientSize.Width;
            row.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;

            // Add bubble vào row trước để bubble có kích thước
            row.Controls.Add(bubble);

            bool isMine = false;
            try { isMine = msg.IsMine; } catch { isMine = false; }

            // Canh top theo padding của row
            bubble.Top = row.Padding.Top;

            if (isMine)
            {
                bubble.Anchor = AnchorStyles.Top | AnchorStyles.Right;
                bubble.Left = CalcRightAlignedLeft(row, bubble);
            }
            else
            {
                bubble.Anchor = AnchorStyles.Top | AnchorStyles.Left;
                bubble.Left = row.Padding.Left;
            }

            // Chiều cao row phụ thuộc chiều cao bubble
            int rowHeight = bubble.Height + row.Padding.Vertical;
            if (rowHeight < 10) rowHeight = 10;
            row.Height = rowHeight;

            // Nếu bubble đổi height (load thumbnail / update name) => resize row + reflow
            bubble.SizeChanged += delegate
            {
                RunOnUi(delegate
                {
                    try
                    {
                        if (row.IsDisposed) return;
                        if (bubble.IsDisposed) return;

                        int newRowHeight = bubble.Height + row.Padding.Vertical;
                        if (newRowHeight < 10) newRowHeight = 10;

                        if (row.Height != newRowHeight)
                        {
                            row.Height = newRowHeight;
                            ReflowAll();
                        }

                        if (isMine)
                        {
                            bubble.Left = CalcRightAlignedLeft(row, bubble);
                        }
                    }
                    catch { }
                });
            };

            // Khi row đổi width (do resize form) => canh lại bubble nếu là tin của mình
            row.Resize += delegate
            {
                try
                {
                    if (bubble.IsDisposed) return;

                    if (isMine)
                    {
                        bubble.Left = CalcRightAlignedLeft(row, bubble);
                    }
                }
                catch { }
            };

            _hostPanel.Controls.Add(row);

            _nextTop = row.Bottom + _spacing;
        }

        private int CalcRightAlignedLeft(Panel row, Control bubble)
        {
            int left = row.ClientSize.Width - row.Padding.Right - bubble.Width;
            if (left < row.Padding.Left) left = row.Padding.Left;
            return left;
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

                        ChatMessage m = null;
                        if (row != null)
                        {
                            m = row.Tag as ChatMessage;
                        }

                        string id = SafeMessageId(m);
                        if (!string.IsNullOrEmpty(id))
                        {
                            lock (_lockObj)
                            {
                                _drawnIds.Remove(id);
                            }
                        }

                        try { if (row != null) row.Dispose(); } catch { }
                    }

                    // Xóa bớt => sắp xếp lại cho không bị hở khoảng trống
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

        #region ====== SCROLL: HỖ TRỢ CUỘN ======

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

                // còn cách đáy <= 40px thì coi như "gần đáy"
                if ((value + large) >= (max - 40))
                {
                    return true;
                }

                return false;
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
                if (!force)
                {
                    bool nearBottom = IsNearBottom();
                    if (!nearBottom) return;
                }

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

        #region ====== SỰ KIỆN ======

        private void HostPanel_Resize(object sender, EventArgs e)
        {
            // Resize panel => reflow lại vị trí + width các row
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

        #region ====== HÀM TIỆN ÍCH ======

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
            try
            {
                if (msg == null) return string.Empty;
                if (msg.MessageId == null) return string.Empty;
                return msg.MessageId;
            }
            catch
            {
                return string.Empty;
            }
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
