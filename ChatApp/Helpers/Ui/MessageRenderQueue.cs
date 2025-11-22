using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Windows.Forms;
using ChatApp.Models.Chat;   // TinNhan

namespace ChatApp.Helpers.Ui
{
    /// <summary>
    /// Hàng đợi render tin nhắn theo batch để giảm lag UI:
    /// - Hàng đợi <see cref="TinNhan"/> cần vẽ.
    /// - Định kỳ theo interval dùng <see cref="Timer"/> để flush sang UI.
    /// - Giới hạn số bubble tối đa trong panel để tránh tràn control.
    /// </summary>
    public class MessageRenderQueue : IDisposable
    {
        #region ======== Trường / State ========

        /// <summary>
        /// Hàng đợi thread-safe chứa các tin nhắn chờ render.
        /// </summary>
        private readonly ConcurrentQueue<TinNhan> _queue = new ConcurrentQueue<TinNhan>();


        /// <summary>
        /// Panel (thường là <see cref="FlowLayoutPanel"/>) dùng để chứa các bubble.
        /// </summary>
        private readonly FlowLayoutPanel _panel;
        /// <summary>
        /// Hàm tạo bubble UI từ một <see cref="TinNhan"/>.
        /// </summary>
        private readonly Func<TinNhan, Messages> _bubbleFactory;
        /// <summary>
        /// Số lượng bubble tối đa được giữ lại trên panel.
        /// Nếu vượt, các bubble cũ nhất sẽ bị xóa bớt.
        /// </summary>
        private readonly int _maxBubbles;


        /// <summary>
        /// Timer định kỳ gọi <see cref="Flush"/> để render batch tin nhắn.
        /// </summary>
        private readonly Timer _timer;

        #endregion

        #region ======== Khởi tạo ========

        /// <summary>
        /// Khởi tạo hàng đợi render tin nhắn:
        /// - Gắn panel đích và factory tạo bubble.
        /// - Cấu hình timer với interval (ms) và giới hạn số bubble.
        /// - Tự động Start timer.
        /// </summary>
        /// <param name="panel">         Panel (FlowLayoutPanel) chứa các dòng chat.          </param>
        /// <param name="bubbleFactory"> Hàm tạo bubble UI từ <see cref="TinNhan"/>.          </param>
        /// <param name="intervalMs">    Chu kỳ flush (ms), mặc định 80ms.                    </param>
        /// <param name="maxBubbles">    Số lượng bubble tối đa giữ trên panel, mặc định 300. </param>
        public MessageRenderQueue(
            FlowLayoutPanel panel,
            Func<TinNhan, Messages> bubbleFactory,
            int intervalMs = 80,
            int maxBubbles = 300)
        {
            if (panel == null) throw new ArgumentNullException(nameof(panel));
            if (bubbleFactory == null) throw new ArgumentNullException(nameof(bubbleFactory));

            _panel = panel;
            _bubbleFactory = bubbleFactory;
            _maxBubbles = maxBubbles;

            _timer = new Timer { Interval = intervalMs };
            _timer.Tick += delegate { Flush(); };
            _timer.Start();
        }

        #endregion

        #region ======== API công khai ========

        /// <summary>
        /// Thêm một tin nhắn vào hàng đợi chờ render.
        /// Tin nhắn sẽ được vẽ khi đến chu kỳ <see cref="Flush"/>.
        /// </summary>
        /// <param name="tn">Tin nhắn cần render.</param>
        public void Enqueue(TinNhan tn)
        {
            if (tn == null) return;
            _queue.Enqueue(tn);
        }

        /// <summary>
        /// Xoá toàn bộ tin nhắn đang nằm trong hàng đợi (chưa vẽ ra UI).
        /// Không ảnh hưởng các bubble đã render trên panel.
        /// </summary>
        public void ClearQueue()
        {
            while (_queue.TryDequeue(out _))
            {
                // bỏ qua
            }
        }

        #endregion

        #region ======== Render nội bộ (Flush từng batch) ========

        /// <summary>
        /// Flush một batch tin nhắn từ hàng đợi ra UI:
        /// - Lấy tối đa 50 tin mỗi lần.
        /// - Thêm bubble vào panel theo đúng thứ tự.
        /// - Cắt bớt các bubble quá cũ khi vượt quá <see cref="_maxBubbles"/>.
        /// - Giữ scroll ở cuối nếu trước đó user đang ở cuối.
        /// </summary>
        private void Flush()
        {
            if (_panel.IsDisposed || !_panel.IsHandleCreated)
                return;

            if (_queue.IsEmpty)
                return;

            var batch = new List<TinNhan>(50);
            while (batch.Count < 50 && _queue.TryDequeue(out var tn))
            {
                batch.Add(tn);
            }

            if (batch.Count == 0)
                return;

            // Xác định user đang ở cuối hay không để sau khi thêm bubble thì auto-scroll
            bool oCuoi =
                !_panel.VerticalScroll.Visible ||
                _panel.VerticalScroll.Value >=
                _panel.VerticalScroll.Maximum - _panel.VerticalScroll.LargeChange - 5;

            _panel.SuspendLayout();
            bool oldAuto = _panel.AutoScroll;
            _panel.AutoScroll = false;

            // Thêm batch bubble
            foreach (var tn in batch)
            {
                var row = _bubbleFactory(tn);
                _panel.Controls.Add(row);
            }

            // Cắt bớt nếu quá maxBubbles
            int over = _panel.Controls.Count - _maxBubbles;
            if (over > 0)
            {
                for (int i = 0; i < over; i++)
                {
                    var c = _panel.Controls[0];
                    c.Dispose();
                    _panel.Controls.RemoveAt(0);
                }
            }

            _panel.AutoScroll = oldAuto;
            _panel.ResumeLayout(true);

            // Nếu trước đó ở cuối thì sau khi thêm vẫn giữ ở cuối
            if (oCuoi && _panel.Controls.Count > 0)
            {
                var last = _panel.Controls[_panel.Controls.Count - 1];
                _panel.ScrollControlIntoView(last);
            }
        }

        #endregion

        #region ======== IDisposable ========

        /// <summary>
        /// Giải phóng tài nguyên:
        /// - Dừng <see cref="Timer"/> và dispose nó.
        /// </summary>
        public void Dispose()
        {
            if (_timer != null)
            {
                _timer.Stop();
                _timer.Dispose();
            }
        }

        #endregion
    }
}
