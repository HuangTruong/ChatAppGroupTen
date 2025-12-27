using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace ChatApp.Services.FileHost
{
    /// <summary>
    /// Nhiệm vụ: upload file lên host trung gian và nhận về URL tải
    /// </summary>
    public class FileAttachmentUploader
    {
        // Dùng chung 1 HttpClient để tránh tạo nhiều connection (đỡ lỗi vặt khi upload nhiều lần)
        private static readonly HttpClient httpClient = CreateClient();

        private static HttpClient CreateClient()
        {
            HttpClient c = new HttpClient();

            // Một số host chặn request nếu thiếu User-Agent hoặc UA bất thường
            // Để chắc kèo, set UA kiểu phổ biến.
            c.DefaultRequestHeaders.UserAgent.Clear();
            c.DefaultRequestHeaders.UserAgent.ParseAdd("curl/8.0.1");

            // Accept để host trả dữ liệu bình thường.
            c.DefaultRequestHeaders.Accept.Clear();
            c.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("*/*"));

            return c;
        }

        /// <summary>
        /// Upload file và trả về URL tải.
        /// - Nếu upload fail: throw Exception để UI bắt và hiện MessageBox
        /// </summary>
        public async Task<string> UploadAsync(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
                throw new Exception("File không tồn tại.");

            // multipart/form-data để gửi file
            using (MultipartFormDataContent form = new MultipartFormDataContent())
            using (FileStream fs = File.OpenRead(filePath))
            {
                // Catbox yêu cầu field reqtype=fileupload để biết bạn đang upload file
                form.Add(new StringContent("fileupload"), "reqtype");

                // Field chứa file phải đúng tên "fileToUpload" theo API của họ
                form.Add(new StreamContent(fs), "fileToUpload", Path.GetFileName(filePath));

                // Gọi API upload
                HttpResponseMessage resp = await httpClient.PostAsync("https://catbox.moe/user/api.php", form);

                // Catbox trả về plain text
                string body = (await resp.Content.ReadAsStringAsync()).Trim();

                // HTTP fail -> báo lỗi
                if (!resp.IsSuccessStatusCode)
                    throw new Exception("Upload fail: " + body);

                // Thành công thì body phải là URL (http/https). Nếu không phải, coi như lỗi
                if (!body.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                    throw new Exception("Upload fail: " + body);

                return body;
            }
        }
    }
}
