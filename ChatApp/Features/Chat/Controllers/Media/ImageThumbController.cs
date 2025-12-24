using ChatApp.Models.Messages;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ChatApp.Controllers
{
    /// <summary>
    /// Quản lý thumbnail ảnh:
    /// - Placeholder
    /// - Cache thumbnail để tránh decode lại
    /// - Load async, update bubble khi xong
    /// </summary>
    public class ImageThumbController : IDisposable
    {
        #region ====== KHAI BÁO BIẾN ======

        private readonly object _lock = new object();
        private readonly Dictionary<string, Image> _cache = new Dictionary<string, Image>(StringComparer.Ordinal);
        private readonly HashSet<string> _loading = new HashSet<string>(StringComparer.Ordinal);
        private readonly Queue<string> _order = new Queue<string>();

        private Image _placeholder;

        public int MaxCache { get; set; }

        #endregion

        #region ====== HÀM KHỞI TẠO ======

        public ImageThumbController()
        {
            MaxCache = 200;
        }

        #endregion

        #region ====== HÀM CÔNG KHAI ======

        public Image GetOrCreatePlaceholder()
        {
            if (_placeholder != null) return _placeholder;

            try
            {
                Bitmap bmp = new Bitmap(24, 24);
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    g.Clear(Color.FromArgb(235, 235, 235));
                    using (Pen p = new Pen(Color.FromArgb(200, 200, 200)))
                    {
                        g.DrawRectangle(p, 1, 1, bmp.Width - 3, bmp.Height - 3);
                        g.DrawLine(p, 3, bmp.Height - 5, bmp.Width - 5, 5);
                    }
                }
                _placeholder = bmp;
            }
            catch
            {
                _placeholder = null;
            }

            return _placeholder;
        }

        public Image TryGet(string key)
        {
            if (string.IsNullOrWhiteSpace(key)) return null;

            lock (_lock)
            {
                Image img;
                if (_cache.TryGetValue(key, out img)) return img;
            }
            return null;
        }

        public void EnsureThumbLoadedAsync(
            ChatMessage msg,
            Control uiOwner,
            Action<Image> onLoadedSetImage,
            int maxW,
            int maxH)
        {
            if (msg == null) return;
            if (uiOwner == null || uiOwner.IsDisposed) return;
            if (string.IsNullOrWhiteSpace(msg.ImageBase64)) return;

            string key = (msg.MessageId ?? string.Empty).Trim();
            if (string.IsNullOrEmpty(key))
            {
                // Không có MessageId => vẫn load async nhưng không cache
                Task.Run(delegate
                {
                    Image thumb = SafeCreateThumb(msg.ImageBase64, maxW, maxH);
                    try
                    {
                        if (uiOwner.IsDisposed) { SafeDispose(thumb); return; }
                        uiOwner.BeginInvoke((Action)delegate
                        {
                            if (uiOwner.IsDisposed) { SafeDispose(thumb); return; }
                            if (thumb != null && onLoadedSetImage != null) onLoadedSetImage(thumb);
                        });
                    }
                    catch
                    {
                        SafeDispose(thumb);
                    }
                });
                return;
            }

            Image cached = TryGet(key);
            if (cached != null)
            {
                if (onLoadedSetImage != null) onLoadedSetImage(cached);
                return;
            }

            lock (_lock)
            {
                if (_loading.Contains(key)) return;
                _loading.Add(key);
            }

            Task.Run(delegate
            {
                Image thumb = SafeCreateThumb(msg.ImageBase64, maxW, maxH);

                lock (_lock)
                {
                    _loading.Remove(key);
                }

                if (thumb != null)
                {
                    AddToCache(key, thumb);
                }

                try
                {
                    if (uiOwner.IsDisposed) return;
                    uiOwner.BeginInvoke((Action)delegate
                    {
                        if (uiOwner.IsDisposed) return;

                        Image t = TryGet(key);
                        if (t == null) return;

                        if (onLoadedSetImage != null) onLoadedSetImage(t);
                    });
                }
                catch
                {
                    // ignore
                }
            });
        }

        #endregion

        #region ====== BỘ NHỚ ĐỆM (CACHE) ======

        private void AddToCache(string key, Image img)
        {
            if (string.IsNullOrWhiteSpace(key) || img == null) return;

            lock (_lock)
            {
                if (_cache.ContainsKey(key))
                {
                    SafeDispose(img);
                    return;
                }

                _cache[key] = img;
                _order.Enqueue(key);

                while (_order.Count > MaxCache)
                {
                    string oldKey = _order.Dequeue();
                    Image old;
                    if (_cache.TryGetValue(oldKey, out old))
                    {
                        _cache.Remove(oldKey);
                        SafeDispose(old);
                    }
                }
            }
        }

        #endregion

        #region ====== TẠO THUMBNAIL ======

        private static Image SafeCreateThumb(string base64, int maxW, int maxH)
        {
            try
            {
                byte[] bytes = Convert.FromBase64String(base64);
                return CreateThumbFromBytes(bytes, maxW, maxH);
            }
            catch
            {
                return null;
            }
        }

        private static Image CreateThumbFromBytes(byte[] bytes, int maxW, int maxH)
        {
            if (bytes == null || bytes.Length == 0) return null;

            using (MemoryStream ms = new MemoryStream(bytes))
            using (Image img = Image.FromStream(ms))
            {
                int w = img.Width;
                int h = img.Height;
                if (w <= 0 || h <= 0) return null;

                double scale = Math.Min((double)maxW / w, (double)maxH / h);
                if (scale > 1) scale = 1;

                int tw = Math.Max(1, (int)Math.Round(w * scale));
                int th = Math.Max(1, (int)Math.Round(h * scale));

                Bitmap bmp = new Bitmap(tw, th);
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    g.SmoothingMode = SmoothingMode.HighQuality;
                    g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                    g.DrawImage(img, 0, 0, tw, th);
                }
                return bmp;
            }
        }

        private static void SafeDispose(Image img)
        {
            try { if (img != null) img.Dispose(); } catch { }
        }

        #endregion

        #region ====== GIẢI PHÓNG TÀI NGUYÊN ======

        public void Dispose()
        {
            lock (_lock)
            {
                foreach (var kv in _cache)
                {
                    SafeDispose(kv.Value);
                }
                _cache.Clear();
                _order.Clear();
                _loading.Clear();
            }

            SafeDispose(_placeholder);
            _placeholder = null;
        }

        #endregion
    }
}
