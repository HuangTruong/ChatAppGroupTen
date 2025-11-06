using System.Windows.Forms;

namespace ChatApp.Helpers.Ui
{
    public static class FocusExtensions
    {
        public static void Refocus(this TextBoxBase tb)
        {
            if (tb == null || tb.IsDisposed) return;
            tb.Focus();
            tb.SelectionStart = tb.TextLength;
            tb.SelectionLength = 0;
        }
    }
}
