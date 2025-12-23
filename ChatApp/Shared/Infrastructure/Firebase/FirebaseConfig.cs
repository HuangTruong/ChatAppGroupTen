using System;

namespace ChatApp.Services.Firebase
{
    /// <summary>
    /// Cấu hình Firebase dùng chung cho toàn bộ ứng dụng:
    /// - ApiKey của Firebase project.
    /// - DatabaseUrl của Realtime Database.
    /// 
    /// Lưu ý:
    /// - Không nên hard-code trong code khi đưa lên repo public.
    ///   Nên đọc từ file cấu hình / biến môi trường nếu dùng production.
    /// </summary>
    public static class FirebaseConfig
    {
        #region ====== FIREBASE CONFIG ======

        /// <summary>
        /// API Key của Firebase project (dùng cho Firebase Auth, v.v.).
        /// </summary>
        public static readonly string ApiKey =
            "AIzaSyAH9rzc7Udd3cEy9RHgymrT_-FAoGqhoRk";

        /// <summary>
        /// URL của Firebase Realtime Database.
        /// </summary>
        public static readonly string DatabaseUrl =
            "https://chatappdemo-a552b-default-rtdb.asia-southeast1.firebasedatabase.app/";

        #endregion
    }
}
