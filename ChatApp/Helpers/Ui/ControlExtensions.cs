using System.Reflection;
using System.Windows.Forms;

namespace ChatApp.Helpers.Ui
{
    public static class ControlExtensions
    {
        // Bật double-buffer cho Panel/FlowLayoutPanel để giảm flicker
        public static void EnableDoubleBuffer(this Panel pnl)
        {

            var prop = typeof(Panel).GetProperty("DoubleBuffered",
                BindingFlags.Instance | BindingFlags.NonPublic);
            prop?.SetValue(pnl, true, null);
        }
    }
}
