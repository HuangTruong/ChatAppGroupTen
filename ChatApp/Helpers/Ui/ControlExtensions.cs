using System.Reflection;
using System.Windows.Forms;

namespace ChatApp.Helpers.Ui
{
    /// <summary>
    /// Cung cấp phương thức mở rộng (extension methods)
    /// cho các control WinForms để cải thiện hiệu suất UI.
    /// </summary>
    public static class ControlExtensions
    {
        #region EnableDoubleBuffer
        /// <summary>
        /// Bật DoubleBuffered cho <see cref="Panel"/> hoặc <see cref="FlowLayoutPanel"/>
        /// giúp giảm hiện tượng flicker (giựt màn hình) khi scroll hoặc cập nhật nhiều controls.
        ///
        /// Do thuộc tính <c>DoubleBuffered</c> là protected, ta phải truy cập qua Reflection.
        /// </summary>
        /// <param name="pnl">Control Panel cần bật double-buffer.</param>
        public static void EnableDoubleBuffer(this Panel pnl)
        {
            if (pnl == null) return;

            var prop = typeof(Panel).GetProperty(
                "DoubleBuffered",
                BindingFlags.Instance | BindingFlags.NonPublic);

            if (prop != null)
            {
                prop.SetValue(pnl, true, null);
            }
        }
        #endregion
    }
}
