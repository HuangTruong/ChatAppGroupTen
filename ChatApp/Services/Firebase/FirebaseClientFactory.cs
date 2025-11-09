using FireSharp;
using FireSharp.Config;
using FireSharp.Interfaces;
using System.Configuration;

namespace ChatApp.Services.Firebase
{
    public static class FirebaseClientFactory
    {
        // Đọc cấu hình từ App.config
        private static readonly string _basePath = ConfigurationManager.AppSettings["FirebaseBasePath"];
        private static readonly string _authSecret = ConfigurationManager.AppSettings["FirebaseAuthSecret"];

        // tạo ra đối tượng IFirebaseClient với các cấu hình (BasePath, AuthSecret) được đọc tự động từ file App.config.
        public static IFirebaseClient Create()
        {
            var cfg = new FirebaseConfig { 
                BasePath = _basePath, AuthSecret = _authSecret 
            };
            return new FirebaseClient(cfg);
        }
    }
}
