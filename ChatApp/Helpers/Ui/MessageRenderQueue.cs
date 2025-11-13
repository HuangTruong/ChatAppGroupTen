using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

using ChatApp.Models.Chat;   // 🔥 Thêm dòng này

namespace ChatApp.Helpers.Ui
{
    public class MessageRenderQueue : IDisposable
    {
        private readonly ConcurrentQueue<TinNhan> _queue =
            new ConcurrentQueue<TinNhan>();

        private readonly FlowLayoutPanel _panel;
        private readonly Func<TinNhan, Panel> _bubbleFactory;

        private readonly int _maxBubbles;
        private readonly Timer _timer;

        public MessageRenderQueue(
            FlowLayoutPanel panel,
            Func<TinNhan, Panel> bubbleFactory,
            int intervalMs = 80,
            int maxBubbles = 300)
        {
            _panel = panel ?? throw new ArgumentNullException(nameof(panel));
            _bubbleFactory = bubbleFactory ?? throw new ArgumentNullException(nameof(bubbleFactory));
            _maxBubbles = maxBubbles;

            _timer = new Timer { Interval = intervalMs };
            _timer.Tick += (s, e) => Flush();
            _timer.Start();
        }

        public void Enqueue(TinNhan tn)
        {
            if (tn == null) return;
            _queue.Enqueue(tn);
        }

        public void ClearQueue()
        {
            while (_queue.TryDequeue(out _)) { }
        }

        private void Flush()
        {
            if (_panel.IsDisposed || !_panel.IsHandleCreated) return;
            if (_queue.IsEmpty) return;

            var batch = new List<TinNhan>(50);
            while (batch.Count < 50 && _queue.TryDequeue(out var tn))
                batch.Add(tn);

            if (batch.Count == 0) return;

            bool oCuoi =
                !_panel.VerticalScroll.Visible ||
                _panel.VerticalScroll.Value >=
                _panel.VerticalScroll.Maximum - _panel.VerticalScroll.LargeChange - 5;

            _panel.SuspendLayout();
            bool oldAuto = _panel.AutoScroll;
            _panel.AutoScroll = false;

            foreach (var tn in batch)
            {
                var row = _bubbleFactory(tn);
                _panel.Controls.Add(row);
            }

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

            if (oCuoi && _panel.Controls.Count > 0)
            {
                var last = _panel.Controls[_panel.Controls.Count - 1];
                _panel.ScrollControlIntoView(last);
            }
        }

        public void Dispose()
        {
            _timer?.Stop();
            _timer?.Dispose();
        }
    }
}
