using System.Collections.Generic;

namespace ChatApp.Models.Chat
{
    public class Nhom
    {
        public string id { get; set; }
        public string tenNhom { get; set; }
        public string taoBoi { get; set; }
        public Dictionary<string, bool> thanhVien { get; set; } = new Dictionary<string, bool>();
    }
}
