using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatApp.Models.Chat
{
    public class TinNhan
    {
        public string id { get; set; }
        public string guiBoi { get; set; }
        public string nhanBoi { get; set; }   // rỗng nếu nhóm
        public string noiDung { get; set; }
        public string thoiGian { get; set; }  // ISO-8601 "o" hoặc Unix ms
        public bool laNhom { get; set; }
    
    }
}
