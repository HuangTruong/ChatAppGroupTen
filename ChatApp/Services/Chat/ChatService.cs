using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ChatApp.Helpers;
using ChatApp.Models.Chat;
using ChatApp.Services.Firebase;
using FireSharp.Response;

namespace ChatApp.Services.Chat
{
    public class ChatService
    {
        private static string Cid(string a, string b)
            => string.Compare(a, b, StringComparison.OrdinalIgnoreCase) < 0
               ? $"{a}_{b}" : $"{b}_{a}";

        public async Task<string> SendDirectAsync(string from, string to, string text)
        {
            var client = FirebaseClientFactory.Create();
            var cid = Cid(from, to);
            var tn = new TinNhan { guiBoi = from, nhanBoi = to, noiDung = text, thoiGian = DateTime.UtcNow.ToString("o") };
            PushResponse p = await client.PushAsync($"cuocTroChuyen/{cid}/", tn);
            tn.id = p.Result.name;
            await client.SetAsync($"cuocTroChuyen/{cid}/{tn.id}", tn);
            return tn.id;
        }

        public async Task<List<TinNhan>> LoadDirectAsync(string a, string b)
        {
            var client = FirebaseClientFactory.Create();
            var res = await client.GetAsync($"cuocTroChuyen/{Cid(a, b)}");
            var dict = res.ResultAs<Dictionary<string, TinNhan>>() ?? new Dictionary<string, TinNhan>();
            return dict.Values.OrderBy(t => TimeParser.ToUtc(t?.thoiGian)).ToList();
        }
    }
}
