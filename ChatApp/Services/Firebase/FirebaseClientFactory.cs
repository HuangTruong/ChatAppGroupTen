using FireSharp;
using FireSharp.Config;
using FireSharp.Interfaces;
using System;
using System.Configuration;

namespace ChatApp.Services.Firebase
{
    #region FirebaseClientFactory
    /// <summary>
    /// Factory tạo đối tượng <see cref="IFirebaseClient"/> dựa trên cấu hình trong App.config.
    /// </summary>
    public static class FirebaseClientFactory
    {
        #region Fields
        /// <summary>
        /// BasePath để kết nối Firebase (Realtime Database URL).
        /// </summary>
        private static readonly string _basePath = ConfigurationManager.AppSettings["FirebaseBasePath"];

        /// <summary>
        /// AuthSecret (Database Secret) của Firebase.
        /// </summary>
        private static readonly string _authSecret = ConfigurationManager.AppSettings["FirebaseAuthSecret"];
        #endregion

        #region Create()
        /// <summary>
        /// Tạo mới một <see cref="IFirebaseClient"/> sử dụng cấu hình từ App.config.
        /// </summary>
        /// <returns>
        /// Đối tượng FirebaseClient đã cấu hình sẵn.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// Ném ra nếu thiếu cấu hình BasePath hoặc AuthSecret.
        /// </exception>
        public static IFirebaseClient Create()
        {
            if (string.IsNullOrWhiteSpace(_basePath))
                throw new InvalidOperationException("FirebaseBasePath không được cấu hình trong App.config.");

            if (string.IsNullOrWhiteSpace(_authSecret))
                throw new InvalidOperationException("FirebaseAuthSecret không được cấu hình trong App.config.");

            var cfg = new FirebaseConfig
            {
                BasePath = _basePath,
                AuthSecret = _authSecret
            };

            return new FirebaseClient(cfg);
        }
        #endregion
    }
    #endregion
}
