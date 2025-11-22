using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ChatApp.Helpers;
using FireSharp.Interfaces;

namespace ChatApp.Services.Status
{
    /// <summary>
    /// Service quản lý trạng thái online/offline (và các trạng thái khác) của người dùng trên Firebase.
    /// </summary>
    /// <remarks>
    /// - Lưu trạng thái vào node <c>status/&lt;safeKey&gt;</c>, trong đó <c>safeKey</c> được chuẩn hoá bằng <see cref="KeySanitizer.SafeKey(string)"/>.
    /// - Cho phép cập nhật trạng thái của một user bất kỳ.
    /// - Cho phép lấy toàn bộ bảng trạng thái để hiển thị online/offline trong UI.
    /// </remarks>
    public class StatusService
    {
        #region ======== Trường / Thuộc tính ========

        /// <summary>
        /// Client Firebase dùng để thao tác với dữ liệu trạng thái.
        /// </summary>
        private readonly IFirebaseClient _firebase;

        #endregion

        #region ======== Khởi tạo ========

        /// <summary>
        /// Khởi tạo <see cref="StatusService"/> với client Firebase đã cấu hình.
        /// </summary>
        /// <param name="firebase">Client Firebase dùng để đọc/ghi node <c>status</c>.</param>
        /// <exception cref="ArgumentNullException">
        /// Ném ra nếu <paramref name="firebase"/> là <c>null</c>.
        /// </exception>
        public StatusService(IFirebaseClient firebase)
        {
            if (firebase == null) throw new ArgumentNullException("firebase");
            _firebase = firebase;
        }

        #endregion

        #region ======== Cập nhật trạng thái người dùng ========

        /// <summary>
        /// Cập nhật trạng thái (online/offline/typing...) cho người dùng.
        /// </summary>
        /// <param name="tenHienThi">
        /// Tên hiển thị của người dùng. Sẽ được chuẩn hoá bằng <see cref="KeySanitizer.SafeKey(string)"/> khi lưu.
        /// </param>
        /// <param name="trangThai">
        /// Chuỗi trạng thái cần lưu (ví dụ: <c>"online"</c>, <c>"offline"</c>, <c>"typing"</c>, ...).
        /// </param>
        /// <remarks>
        /// - Nếu <paramref name="tenHienThi"/> rỗng hoặc chỉ toàn khoảng trắng, hàm sẽ return ngay.
        /// - Dữ liệu được lưu vào node: <c>status/&lt;safeKey&gt;</c>.
        /// </remarks>
        public async Task UpdateAsync(string tenHienThi, string trangThai)
        {
            if (string.IsNullOrWhiteSpace(tenHienThi))
                return;

            string key = KeySanitizer.SafeKey(tenHienThi);
            await _firebase.SetAsync("status/" + key, trangThai);
        }

        #endregion

        #region ======== Lấy toàn bộ bảng trạng thái ========

        /// <summary>
        /// Lấy danh sách trạng thái của tất cả người dùng trong node <c>status</c>.
        /// </summary>
        /// <returns>
        /// Dictionary với:
        /// - Key: khoá đã được chuẩn hoá (safe key) khi lưu.
        /// - Value: trạng thái tương ứng (ví dụ: <c>"online"</c>, <c>"offline"</c>, ...).
        /// Trả về dictionary rỗng nếu không có dữ liệu.
        /// </returns>
        public async Task<Dictionary<string, string>> GetAllAsync()
        {
            var res = await _firebase.GetAsync("status");
            var data = res.ResultAs<Dictionary<string, string>>();
            return data ?? new Dictionary<string, string>();
        }

        #endregion
    }
}
