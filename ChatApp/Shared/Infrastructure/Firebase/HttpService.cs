using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace ChatApp.Services.Firebase
{
    /// <summary>
    /// Dịch vụ HTTP dùng chung để gọi REST API (Firebase Auth, Realtime DB, v.v.).
    /// Đóng gói các phương thức GET / POST / PUT / PATCH / DELETE với JSON.
    /// </summary>
    public class HttpService
    {
        #region ====== BIẾN THÀNH VIÊN ======

        /// <summary>
        /// HttpClient dùng chung trong HttpService.
        /// </summary>
        private readonly HttpClient _client = new HttpClient();

        #endregion

        #region ====== HÀM DÙNG CHUNG ======

        /// <summary>
        /// Ném Exception nếu HTTP response không thành công (để không bị fail-silent).
        /// </summary>
        private static void EnsureSuccess(HttpResponseMessage res, string body)
        {
            if (res == null)
            {
                throw new Exception("HTTP response null.");
            }

            if (!res.IsSuccessStatusCode)
            {
                string msg = string.Format("HTTP {0} {1}: {2}",
                    (int)res.StatusCode,
                    res.ReasonPhrase,
                    body);

                throw new Exception(msg);
            }
        }

        #endregion

        #region ====== POST (JSON) ======

        /// <summary>
        /// Gửi request POST với body JSON và deserialize phản hồi về kiểu T.
        /// </summary>
        public async Task<T> PostAsync<T>(string url, object data)
        {
            string json = JsonConvert.SerializeObject(data);
            StringContent http = new StringContent(json, Encoding.UTF8, "application/json");

            HttpResponseMessage res = await _client.PostAsync(url, http).ConfigureAwait(false);
            string body = await res.Content.ReadAsStringAsync().ConfigureAwait(false);

            EnsureSuccess(res, body);
            return JsonConvert.DeserializeObject<T>(body);
        }

        #endregion

        #region ====== GET (RAW) ======

        /// <summary>
        /// Gửi request GET và trả về chuỗi raw (không deserialize).
        /// Dùng cho các endpoint trả về string/null hoặc cần tự parse.
        /// </summary>
        public async Task<string> GetRawAsync(string url)
        {
            HttpResponseMessage res = await _client.GetAsync(url).ConfigureAwait(false);
            string body = await res.Content.ReadAsStringAsync().ConfigureAwait(false);

            EnsureSuccess(res, body);
            return body;
        }

        #endregion

        #region ====== GET (JSON) ======

        /// <summary>
        /// Gửi request GET và deserialize JSON phản hồi về kiểu T.
        /// </summary>
        public async Task<T> GetAsync<T>(string url)
        {
            HttpResponseMessage res = await _client.GetAsync(url).ConfigureAwait(false);
            string body = await res.Content.ReadAsStringAsync().ConfigureAwait(false);

            EnsureSuccess(res, body);
            return JsonConvert.DeserializeObject<T>(body);
        }

        #endregion

        #region ====== PUT (JSON) ======

        /// <summary>
        /// Gửi request PUT với body JSON (không cần đọc phản hồi).
        /// Thường dùng để ghi đè node trên Firebase Realtime Database.
        /// </summary>
        public async Task PutAsync(string url, object data)
        {
            string json = JsonConvert.SerializeObject(data);
            StringContent http = new StringContent(json, Encoding.UTF8, "application/json");

            HttpResponseMessage res = await _client.PutAsync(url, http).ConfigureAwait(false);
            string body = await res.Content.ReadAsStringAsync().ConfigureAwait(false);

            EnsureSuccess(res, body);
        }

        #endregion

        #region ====== PATCH (JSON) ======

        /// <summary>
        /// Gửi request PATCH với body JSON.
        /// Thường dùng để cập nhật một phần dữ liệu (update field) trên Firebase.
        /// </summary>
        public async Task PatchAsync(string url, object data)
        {
            string json = JsonConvert.SerializeObject(data);

            HttpMethod method = new HttpMethod("PATCH");
            HttpRequestMessage req = new HttpRequestMessage(method, url);
            req.Content = new StringContent(json, Encoding.UTF8, "application/json");

            HttpResponseMessage res = await _client.SendAsync(req).ConfigureAwait(false);
            string body = await res.Content.ReadAsStringAsync().ConfigureAwait(false);

            EnsureSuccess(res, body);
        }

        #endregion

        #region ====== DELETE ======

        /// <summary>
        /// Gửi request DELETE tới endpoint, thường dùng để xóa node trên Firebase.
        /// </summary>
        public async Task DeleteAsync(string url)
        {
            HttpResponseMessage res = await _client.DeleteAsync(url).ConfigureAwait(false);
            string body = await res.Content.ReadAsStringAsync().ConfigureAwait(false);

            EnsureSuccess(res, body);
        }

        #endregion
    }
}
