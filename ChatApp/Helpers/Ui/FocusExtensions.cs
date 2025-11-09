using System;
using System.Windows.Forms;

namespace ChatApp.Helpers.Ui
{
    public static class FocusExtensions
    {
        // Đặt lại con trỏ về cuối ô nhập
        public static void RefocusToEnd(this Control control)
        {
            if (control == null || control.IsDisposed)
                return;

            control.Focus();

            // Nếu là TextBox / RichTextBox / Guna2TextBox (thường kế thừa TextBoxBase)
            var tb = control as TextBoxBase;
            if (tb != null)
            {
                tb.SelectionStart = tb.TextLength;
                tb.SelectionLength = 0;
            }
        }
    }
}
