using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatApp.Models.Themes
{
    /// <summary>
    /// Lưu trạng thái giao diện của người dùng
    /// </summary>
    public class ThemeSetting
    {
        /// <summary>
        /// true = Dark Mode, false = Light Mode
        /// </summary>
        public bool IsDarkMode { get; set; }
    }
}
