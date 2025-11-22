using System;
using System.Windows.Forms;

namespace ChatApp.Helpers.Ui
{
    /// <summary>
    /// Chứa các extension method hỗ trợ thao tác Focus trên các ô nhập liệu.
    /// </summary>
    public static class FocusExtensions
    {
        #region RefocusToEnd
        /// <summary>
        /// Đặt focus vào control và đưa con trỏ văn bản về cuối.
        /// Hỗ trợ TextBox, RichTextBox và mọi control kế thừa TextBoxBase
        /// (bao gồm cả Guna2TextBox).
        /// </summary>
        /// <param name="control">Control cần đặt lại focus.</param>
        public static void RefocusToEnd(this Control control)
        {
            if (control == null) return;
            if (control.IsDisposed) return;

            // Đưa focus vào control
            control.Focus();

            // Nếu là textbox hoặc kế thừa TextBoxBase
            var tb = control as TextBoxBase;
            if (tb != null)
            {
                tb.SelectionStart = tb.TextLength;
                tb.SelectionLength = 0;
            }
        }
        #endregion
    }
}
