using ChatApp.Models.Messages;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ChatApp.Controllers
{
    /// <summary>
    /// Controller tải file từ tin nhắn "file".
    /// (Tách khỏi NhanTin.cs để form không chứa HttpClient/IO)
    /// </summary>
    public class FileDownloadController : IDisposable
    {
        #region ====== FIELDS ======

        private readonly SemaphoreSlim _gate = new SemaphoreSlim(1, 1);

        private static readonly HttpClient _httpClient = CreateHttpClient();

        #endregion

        #region ====== DOWNLOAD ======

        public async Task DownloadFromMessageAsync(ChatMessage msg, IWin32Window owner)
        {
            if (msg == null) return;

            if (!string.Equals(msg.MessageType, "file", StringComparison.OrdinalIgnoreCase))
                return;

            if (string.IsNullOrWhiteSpace(msg.FileUrl))
            {
                MessageBox.Show(owner, "Tin nhắn file không có URL để tải.", "Thông báo",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            await _gate.WaitAsync().ConfigureAwait(true);
            try
            {
                using (SaveFileDialog sfd = new SaveFileDialog())
                {
                    string downloads = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
                    if (Directory.Exists(downloads)) sfd.InitialDirectory = downloads;

                    sfd.FileName = string.IsNullOrEmpty(msg.FileName) ? "download.bin" : msg.FileName;
                    sfd.OverwritePrompt = true;

                    if (sfd.ShowDialog(owner) != DialogResult.OK) return;

                    using (HttpResponseMessage resp = await _httpClient.GetAsync(msg.FileUrl.Trim(), HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(true))
                    {
                        resp.EnsureSuccessStatusCode();

                        using (Stream net = await resp.Content.ReadAsStreamAsync().ConfigureAwait(true))
                        using (FileStream fs = new FileStream(sfd.FileName, FileMode.Create, FileAccess.Write, FileShare.None))
                        {
                            await net.CopyToAsync(fs).ConfigureAwait(true);
                        }
                    }

                    MessageBox.Show(owner, "Tải xong:\n" + sfd.FileName, "Hoàn tất",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (IOException)
            {
                MessageBox.Show(owner,
                    "Không ghi được file vì file đang được mở/đang bị khóa.\nĐóng file đó hoặc chọn tên khác rồi tải lại.",
                    "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show(owner, "Tải file lỗi:\n" + ex.ToString(), "Lỗi",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                _gate.Release();
            }
        }

        #endregion

        #region ====== HTTP CLIENT ======

        private static HttpClient CreateHttpClient()
        {
            HttpClientHandler handler = new HttpClientHandler();
            handler.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            handler.UseProxy = true;
            handler.Proxy = WebRequest.DefaultWebProxy;

            HttpClient c = new HttpClient(handler);
            c.Timeout = TimeSpan.FromMinutes(5);
            c.DefaultRequestHeaders.UserAgent.Clear();
            c.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0");
            return c;
        }

        #endregion

        #region ====== DISPOSE ======

        public void Dispose()
        {
            try { _gate.Dispose(); } catch { }
        }

        #endregion
    }
}
