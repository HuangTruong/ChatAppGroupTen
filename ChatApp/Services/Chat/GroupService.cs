using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ChatApp.Models.Chat;
using ChatApp.Services.Firebase;
using ChatApp.Helpers;
using FireSharp.Response;

namespace ChatApp.Services.Chat
{
    public class GroupService
    {
        public async Task<Nhom> GetAsync(string id)
        {
            var client = FirebaseClientFactory.Create();
            var res = await client.GetAsync($"nhom/{id}");
            return res.Body == "null" ? null : res.ResultAs<Nhom>();
        }

        public async Task AddMembersAsync(string id, IEnumerable<string> members)
        {
            var client = FirebaseClientFactory.Create();
            foreach (var m in members)
                await client.SetAsync($"nhom/{id}/thanhVien/{KeySanitizer.SafeKey(m)}", true);
        }

        public async Task DeleteAsync(string id)
        {
            var client = FirebaseClientFactory.Create();
            await client.DeleteAsync($"nhom/{id}");
            await client.DeleteAsync($"cuocTroChuyenNhom/{id}");
        }

        public async Task<string> SendGroupAsync(string id, TinNhan tn)
        {
            var client = FirebaseClientFactory.Create();
            PushResponse p = await client.PushAsync($"cuocTroChuyenNhom/{id}/", tn);
            tn.id = p.Result.name;
            await client.SetAsync($"cuocTroChuyenNhom/{id}/{tn.id}", tn);
            return tn.id;
        }
    }
}
