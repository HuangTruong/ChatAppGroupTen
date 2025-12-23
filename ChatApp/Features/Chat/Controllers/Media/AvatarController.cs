using ChatApp.Services.Firebase;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ChatApp.Controllers
{
    /// <summary>
    /// Controller quản lý Avatar:
    /// - Lấy avatar từ Firebase (base64)
    /// - Decode -> Image
    /// - Set vào PictureBox an toàn (UI thread)
    /// - Cache để giảm lag
    /// </summary>
    public class AvatarController
    {
        #region ====== FIELDS ======

        private readonly AuthService _authService;

        private readonly object _lock = new object();
        private readonly Dictionary<string, Image> _cache = new Dictionary<string, Image>(StringComparer.Ordinal);

        #endregion

        #region ====== CTOR ======

        public AvatarController()
        {
            _authService = new AuthService();
        }

        #endregion

        #region ====== PUBLIC API ======

        /// <summary>
        /// Load avatar theo localId và set vào PictureBox.
        /// Có cache để giảm decode lại.
        /// </summary>
        public async Task LoadAvatarToPictureBoxAsync(string localId, PictureBox pb, Image placeholder)
        {
            if (pb == null) return;

            // set placeholder trước
            if (placeholder != null)
            {
                SetPictureBoxImageSafe(pb, (Image)placeholder.Clone());
            }

            if (string.IsNullOrWhiteSpace(localId)) return;

            // cache hit
            Image cached = TryGetCache(localId);
            if (cached != null)
            {
                SetPictureBoxImageSafe(pb, (Image)cached.Clone());
                return;
            }

            string base64 = null;
            try
            {
                base64 = await _authService.GetAvatarAsync(localId).ConfigureAwait(false);
            }
            catch
            {
                base64 = null;
            }

            Image img = TryDecodeBase64ToImage(base64);
            if (img == null) return;

            AddCache(localId, img);

            // clone khi set để tránh dispose nhầm cache
            SetPictureBoxImageSafe(pb, (Image)img.Clone());
        }

        #endregion

        #region ====== CACHE ======

        private Image TryGetCache(string key)
        {
            if (string.IsNullOrWhiteSpace(key)) return null;

            lock (_lock)
            {
                Image img;
                if (_cache.TryGetValue(key, out img))
                {
                    return img;
                }
            }
            return null;
        }

        private void AddCache(string key, Image img)
        {
            if (string.IsNullOrWhiteSpace(key)) return;
            if (img == null) return;

            lock (_lock)
            {
                if (_cache.ContainsKey(key))
                {
                    try { img.Dispose(); } catch { }
                    return;
                }
                _cache[key] = img; // giữ 1 bản trong cache
            }
        }

        #endregion

        #region ====== HELPERS ======

        private static Image TryDecodeBase64ToImage(string base64)
        {
            if (string.IsNullOrWhiteSpace(base64)) return null;

            try
            {
                byte[] bytes = Convert.FromBase64String(base64);
                Image img;
                using (MemoryStream ms = new MemoryStream(bytes))
                {
                    using (Image tmp = Image.FromStream(ms))
                    {
                        img = (Image)tmp.Clone();
                    }
                }
                return img;
            }
            catch
            {
                return null;
            }
        }

        private void SetPictureBoxImageSafe(PictureBox pb, Image newImg)
        {
            if (pb == null)
            {
                if (newImg != null) newImg.Dispose();
                return;
            }

            if (pb.InvokeRequired)
            {
                try
                {
                    pb.BeginInvoke(new Action(delegate { SetPictureBoxImageSafe(pb, newImg); }));
                }
                catch
                {
                    try { if (newImg != null) newImg.Dispose(); } catch { }
                }
                return;
            }

            Image old = pb.Image;
            pb.Image = newImg;
            pb.SizeMode = PictureBoxSizeMode.Zoom;

            if (old != null)
            {
                try { old.Dispose(); } catch { }
            }
        }

        #endregion
    }
}
