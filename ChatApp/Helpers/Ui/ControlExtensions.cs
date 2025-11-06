using System.Reflection;
using System.Windows.Forms;

namespace ChatApp.Helpers.Ui
{
    public static class ControlExtensions
    {
        public static void EnableDoubleBuffer(this Panel panel)
        {
            var prop = typeof(Panel).GetProperty("DoubleBuffered",
                BindingFlags.Instance | BindingFlags.NonPublic);
            prop?.SetValue(panel, true, null);
        }
    }
}
