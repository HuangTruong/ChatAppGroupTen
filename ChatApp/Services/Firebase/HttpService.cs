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
        #region ====== FIELDS ======

        /// <summary>
        /// HttpClient dùng chung trong HttpService.
        /// </summary>
        private readonly HttpClient _client = new HttpClient();

        #endregion

        #region ====== POST (JSON) ======

        /// <summary>
        /// Gửi request POST với body JSON và deserialize phản hồi về kiểu T.
        /// </summary>
        /// <typeparam name="T">Kiểu dữ liệu mong muốn cho phản hồi.</typeparam>
        /// <param name="url">Địa chỉ endpoint.</param>
        /// <param name="data">Đối tượng sẽ được serialize sang JSON.</param>
        /// <returns>Đối tượng kiểu T đọc được từ JSON phản hồi.</returns>
        public async Task<T> PostAsync<T>(string url, object data)
        {
            var json = JsonConvert.SerializeObject(data);
            var http = new StringContent(json, Encoding.UTF8, "application/json");

            var res = await _client.PostAsync(url, http).ConfigureAwait(false);
            var body = await res.Content.ReadAsStringAsync().ConfigureAwait(false);

            return JsonConvert.DeserializeObject<T>(body);
        }

        #endregion

        #region ====== GET (JSON) ======

        /// <summary>
        /// Gửi request GET và deserialize JSON phản hồi về kiểu T.
        /// </summary>
        /// <typeparam name="T">Kiểu dữ liệu mong muốn cho phản hồi.</typeparam>
        /// <param name="url">Địa chỉ endpoint.</param>
        /// <returns>Đối tượng kiểu T đọc được từ JSON phản hồi.</returns>
        public async Task<T> GetAsync<T>(string url)
        {
            var res = await _client.GetAsync(url).ConfigureAwait(false);
            var body = await res.Content.ReadAsStringAsync().ConfigureAwait(false);

            return JsonConvert.DeserializeObject<T>(body);
        }

        #endregion

        #region ====== PUT (JSON) ======

        /// <summary>
        /// Gửi request PUT với body JSON (không cần đọc phản hồi).
        /// Thường dùng để ghi đè node trên Firebase Realtime Database.
        /// </summary>
        /// <param name="url">Địa chỉ endpoint.</param>
        /// <param name="data">Đối tượng sẽ được serialize sang JSON.</param>
        public async Task PutAsync(string url, object data)
        {
            var json = JsonConvert.SerializeObject(data);
            var http = new StringContent(json, Encoding.UTF8, "application/json");

            await _client.PutAsync(url, http).ConfigureAwait(false);
        }

        #endregion

        #region ====== PATCH (JSON) ======

        /// <summary>
        /// Gửi request PATCH với body JSON (không cần đọc phản hồi).
        /// Thường dùng để cập nhật một phần dữ liệu (update field) trên Firebase.
        /// </summary>
        /// <param name="url">Địa chỉ endpoint.</param>
        /// <param name="data">Đối tượng sẽ được serialize sang JSON.</param>
        public async Task PatchAsync(string url, object data)
        {
            var json = JsonConvert.SerializeObject(data);

            var method = new HttpMethod("PATCH");
            var req = new HttpRequestMessage(method, url)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };

            await _client.SendAsync(req).ConfigureAwait(false);
        }

        //// Nếu project target .NET hỗ trợ PatchAsync sẵn có thể dùng phiên bản này:
        ////public async Task PatchAsync(string url, object data)
        ////{
        ////    var json = JsonConvert.SerializeObject(data);
        ////    var http = new StringContent(json, Encoding.UTF8, "application/json");
        ////    await _client.PatchAsync(url, http);
        ////}

        #endregion

        #region ====== DELETE ======

        /// <summary>
        /// Gửi request DELETE tới endpoint, thường dùng để xóa node trên Firebase.
        /// </summary>
        /// <param name="url">Địa chỉ endpoint.</param>
        public async Task DeleteAsync(string url)
        {
            await _client.DeleteAsync(url).ConfigureAwait(false);
        }

        #endregion
    }
}
