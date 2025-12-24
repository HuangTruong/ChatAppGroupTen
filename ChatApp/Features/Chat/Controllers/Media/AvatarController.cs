using ChatApp.Services.Firebase;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ChatApp.Controllers
{
    /// <summary>
    /// Controller quản lý Avatar (ảnh đại diện):
    /// - Luôn hiển thị ảnh mặc định: Resources/default_avatar.png
    /// - Nếu Firebase có avatar (base64) thì decode và thay thế
    /// - Set vào PictureBox an toàn (UI thread)
    /// - Cache để giảm lag (không decode lại nhiều lần)
    /// </summary>
    public class AvatarController
    {
        #region ====== BIẾN THÀNH VIÊN ======

        private readonly AuthService _authService;

        private readonly object _lockObj = new object();
        private readonly Dictionary<string, Image> _cache = new Dictionary<string, Image>(StringComparer.Ordinal);

        private Image _defaultAvatar;
        private bool _defaultAvatarTried;

        private const string DEFAULT_AVATAR_FILE_RELATIVE = "Resources\\default_avatar.png";

        #endregion

        #region ====== KHỞI TẠO ======

        public AvatarController()
        {
            _authService = new AuthService();
        }

        #endregion

        #region ====== API CÔNG KHAI ======

        /// <summary>
        /// Load avatar theo localId và set vào PictureBox.
        /// Nếu không có avatar trên Firebase => giữ ảnh mặc định (default_avatar.png).
        /// </summary>
        public Task LoadAvatarToPictureBoxAsync(string localId, PictureBox pb)
        {
            return LoadAvatarToPictureBoxAsync(localId, pb, null);
        }

        /// <summary>
        /// (Giữ tương thích) Tham số placeholder bị bỏ qua.
        /// AvatarController sẽ tự dùng ảnh mặc định default_avatar.png.
        /// </summary>
        public async Task LoadAvatarToPictureBoxAsync(string localId, PictureBox pb, Image placeholder)
        {
            if (pb == null) return;

            // 1) Luôn set ảnh mặc định trước (không còn placeholder "vẽ vòng tròn" nữa)
            Image def = EnsureDefaultAvatar();
            if (def != null)
            {
                SetPictureBoxImageSafe(pb, CloneImageSafe(def));
            }

            // 2) Nếu không có localId thì dừng ở ảnh mặc định
            if (string.IsNullOrWhiteSpace(localId)) return;

            // 3) Cache hit
            Image cached = TryGetCache(localId);
            if (cached != null)
            {
                SetPictureBoxImageSafe(pb, CloneImageSafe(cached));
                return;
            }

            // 4) Lấy base64 từ Firebase (nếu có thì thay thế ảnh mặc định)
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
            if (img == null)
            {
                // Không có avatar hoặc decode lỗi => giữ default
                return;
            }

            AddCache(localId, img);

            // Clone khi set để tránh dispose nhầm cache
            SetPictureBoxImageSafe(pb, CloneImageSafe(img));
        }

        #endregion

        #region ====== CACHE AVATAR THEO USER ======

        private Image TryGetCache(string key)
        {
            if (string.IsNullOrWhiteSpace(key)) return null;

            lock (_lockObj)
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

            lock (_lockObj)
            {
                if (_cache.ContainsKey(key))
                {
                    SafeDispose(img);
                    return;
                }

                _cache[key] = img; // giữ 1 bản trong cache
            }
        }

        #endregion

        #region ====== ẢNH MẶC ĐỊNH (DEFAULT AVATAR) ======

        /// <summary>
        /// Load 1 lần ảnh mặc định (ưu tiên Resource nhúng, fallback ra file Resources/default_avatar.png).
        /// Nếu không load được thì trả null (khi đó PictureBox giữ nguyên ảnh hiện tại).
        /// </summary>
        private Image EnsureDefaultAvatar()
        {
            lock (_lockObj)
            {
                if (_defaultAvatar != null) return _defaultAvatar;
                if (_defaultAvatarTried) return null;
                _defaultAvatarTried = true;
            }

            Image loaded = null;

            // 1) Ưu tiên: ảnh nhúng trong Properties.Resources (nếu bạn Add vào Resources.resx)
            loaded = TryLoadDefaultFromEmbeddedResources();

            // 2) Fallback: đọc file từ output (Resources/default_avatar.png)
            if (loaded == null)
            {
                loaded = TryLoadDefaultFromFile();
            }

            lock (_lockObj)
            {
                _defaultAvatar = loaded;
            }

            return _defaultAvatar;
        }

        private static Image TryLoadDefaultFromEmbeddedResources()
        {
            try
            {
                Assembly asm = typeof(AvatarController).Assembly;
                Type resType = null;

                // Tìm type kiểu "...Properties.Resources"
                Type[] types = asm.GetTypes();
                for (int i = 0; i < types.Length; i++)
                {
                    Type t = types[i];
                    if (t == null) continue;

                    string full = t.FullName;
                    if (string.IsNullOrEmpty(full)) continue;

                    if (full.EndsWith(".Properties.Resources", StringComparison.Ordinal))
                    {
                        resType = t;
                        break;
                    }
                }

                if (resType == null) return null;

                // Tìm property tên "default_avatar" hoặc property nào có chứa "default_avatar"
                PropertyInfo prop = resType.GetProperty("default_avatar", BindingFlags.Public | BindingFlags.Static);

                if (prop == null)
                {
                    PropertyInfo[] props = resType.GetProperties(BindingFlags.Public | BindingFlags.Static);
                    for (int i = 0; i < props.Length; i++)
                    {
                        PropertyInfo p = props[i];
                        if (p == null) continue;
                        if (p.Name != null && p.Name.IndexOf("default_avatar", StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            prop = p;
                            break;
                        }
                    }
                }

                if (prop == null) return null;

                object val = prop.GetValue(null, null);
                Image img = val as Image;
                if (img == null) return null;

                return CloneImageSafe(img);
            }
            catch
            {
                return null;
            }
        }

        private static Image TryLoadDefaultFromFile()
        {
            try
            {
                string path = Path.Combine(Application.StartupPath, DEFAULT_AVATAR_FILE_RELATIVE);
                if (!File.Exists(path)) return null;

                byte[] bytes = File.ReadAllBytes(path);
                using (MemoryStream ms = new MemoryStream(bytes))
                using (Image tmp = Image.FromStream(ms))
                {
                    return (Image)tmp.Clone();
                }
            }
            catch
            {
                return null;
            }
        }

        private static Image CloneImageSafe(Image img)
        {
            if (img == null) return null;

            try { return (Image)img.Clone(); }
            catch { return img; }
        }

        #endregion

        #region ====== GIẢI MÃ BASE64 ======

        private static Image TryDecodeBase64ToImage(string base64)
        {
            if (string.IsNullOrWhiteSpace(base64)) return null;

            try
            {
                byte[] bytes = Convert.FromBase64String(base64);

                using (MemoryStream ms = new MemoryStream(bytes))
                using (Image tmp = Image.FromStream(ms))
                {
                    return (Image)tmp.Clone();
                }
            }
            catch
            {
                return null;
            }
        }

        #endregion

        #region ====== SET ẢNH AN TOÀN TRÊN UI THREAD ======

        private void SetPictureBoxImageSafe(PictureBox pb, Image newImg)
        {
            if (pb == null)
            {
                SafeDispose(newImg);
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
                    SafeDispose(newImg);
                }
                return;
            }

            Image old = pb.Image;
            pb.Image = newImg;
            pb.SizeMode = PictureBoxSizeMode.Zoom;

            SafeDispose(old);
        }

        private static void SafeDispose(Image img)
        {
            try
            {
                if (img != null) img.Dispose();
            }
            catch { }
        }

        #endregion
    }
}
