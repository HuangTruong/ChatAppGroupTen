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
        public bool laEmoji { get; set; }
        public string emojiKey { get; set; }
        public bool laFile { get; set; } = false; // true nếu là tin nhắn file
        public string tenFile { get; set; }  // tên file hiển thị
        public long kichThuoc { get; set; }  // dung lượng (bytes) – có thể =0 nếu không biết
        public string fileUrl { get; set; }
    }
}
