using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ChatApp.Helpers.Ui
{
    // Hàng đợi vẽ tin nhắn theo lô (batch), tránh lag UI khi hiển thị nhiều tin
    public class MessageRenderQueue : IDisposable
    {
        private readonly ConcurrentQueue<TinNhan> _queue = new ConcurrentQueue<TinNhan>(); // hàng đợi thread-safe
        private readonly FlowLayoutPanel _panel;          // nơi hiển thị tin nhắn
        private readonly Func<TinNhan, Panel> _bubbleFactory; // hàm tạo bong bóng chat
        private readonly int _maxBubbles;                 // số bong bóng tối đa giữ lại
        private readonly Timer _timer;                    // timer định kỳ để flush

        // Khởi tạo hàng đợi vẽ
        public MessageRenderQueue(
            FlowLayoutPanel panel,
            Func<TinNhan, Panel> bubbleFactory,
            int intervalMs = 80,
            int maxBubbles = 300)
        {
            _panel = panel ?? throw new ArgumentNullException(nameof(panel));
            _bubbleFactory = bubbleFactory ?? throw new ArgumentNullException(nameof(bubbleFactory));
            _maxBubbles = maxBubbles;

            _timer = new Timer { Interval = intervalMs }; // flush mỗi 80ms
            _timer.Tick += (s, e) => Flush();
            _timer.Start();
        }

        // Thêm 1 tin nhắn vào hàng đợi
        public void Enqueue(TinNhan tn)
        {
            if (tn == null) return;
            _queue.Enqueue(tn);
        }

        // Xoá toàn bộ tin trong hàng đợi
        public void ClearQueue()
        {
            while (_queue.TryDequeue(out _)) { }
        }

        // Vẽ các tin trong hàng đợi ra panel
        private void Flush()
        {
            if (_panel.IsDisposed || !_panel.IsHandleCreated) return;
            if (_queue.IsEmpty) return;

            // Lấy tối đa 50 tin mỗi lần
            var batch = new List<TinNhan>(50);
            while (batch.Count < 50 && _queue.TryDequeue(out var tn))
                batch.Add(tn);

            if (batch.Count == 0) return;

            // Kiểm tra xem đang cuộn ở cuối không
            bool oCuoi =
                !_panel.VerticalScroll.Visible ||
                _panel.VerticalScroll.Value >=
                _panel.VerticalScroll.Maximum - _panel.VerticalScroll.LargeChange - 5;

            _panel.SuspendLayout();
            bool oldAuto = _panel.AutoScroll;
            _panel.AutoScroll = false;

            // Tạo và thêm bong bóng cho từng tin
            foreach (var tn in batch)
            {
                var row = _bubbleFactory(tn);
                _panel.Controls.Add(row);
            }

            // Giới hạn số bong bóng, xoá bớt nếu quá nhiều
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

            // Nếu đang ở cuối, tự động cuộn đến tin mới nhất
            if (oCuoi && _panel.Controls.Count > 0)
            {
                var last = _panel.Controls[_panel.Controls.Count - 1];
                _panel.ScrollControlIntoView(last);
            }
        }

        // Giải phóng tài nguyên timer
        public void Dispose()
        {
            _timer?.Stop();
            _timer?.Dispose();
        }
    }
}
