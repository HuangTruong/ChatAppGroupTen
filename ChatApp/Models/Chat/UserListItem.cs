using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatApp.Models.Chat
{
    public class UserListItem
    {
        public string TenHienThi { get; set; }
        public bool LaBanBe { get; set; }
        public bool DaGuiLoiMoi { get; set; }
        public bool MoiKetBanChoMinh { get; set; }
        public bool Online { get; set; }
    }
}
