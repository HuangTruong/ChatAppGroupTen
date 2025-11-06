using FireSharp;
using FireSharp.Config;
using FireSharp.Interfaces;
using System.Configuration;

namespace ChatApp.Services.Firebase
{
    public static class FirebaseClientFactory
    {
        // TODO: => đọc từ App.config / user-secrets / env var
        private static readonly string _basePath = ConfigurationManager.AppSettings["FirebaseBasePath"];
        private static readonly string _authSecret = ConfigurationManager.AppSettings["FirebaseAuthSecret"];

        public static IFirebaseClient Create()
        {
            var cfg = new FirebaseConfig { 
                BasePath = _basePath, AuthSecret = _authSecret 
            };
            return new FirebaseClient(cfg);
        }
    }
}
