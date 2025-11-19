using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ChatApp.Helpers
{
    public static class ChatInputHandler
    {
        public static void HandleKeyDown(KeyEventArgs e, Guna.UI2.WinForms.Guna2CircleButton btnGui)
        {
            // Enter thường: gửi; Shift+Enter hoặc Ctrl+Enter: xuống dòng
            if (e.KeyCode == Keys.Enter && !e.Shift && !e.Control)
            {
                e.SuppressKeyPress = true;   // không chèn newline, không beep
                if (btnGui.Enabled) btnGui.PerformClick();
            }
        }
    }
}
