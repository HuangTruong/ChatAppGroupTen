using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
