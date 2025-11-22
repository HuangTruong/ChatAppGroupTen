using System;
using System.Windows.Forms;

namespace ChatApp.Helpers
{
    /// <summary>
    /// Helper xử lý phím Enter trong ô nhập chat:
    /// - Enter thường: gửi tin nhắn (kích hoạt nút gửi).
    /// - Shift+Enter hoặc Ctrl+Enter: xuống dòng, không gửi.
    /// </summary>
    public static class ChatInputHandler
    {
        #region ======== Xử lý phím Enter trong ô nhập ========

        /// <summary>
        /// Hàm xử lý sự kiện KeyDown từ ô nhập tin nhắn:
        /// - Nếu nhấn Enter (không giữ Shift/Ctrl): chặn newline và chạy nút gửi.
        /// - Nếu giữ Shift hoặc Ctrl: cho phép xuống dòng bình thường.
        /// </summary>
        /// <param name="e">Đối tượng <see cref="KeyEventArgs"/> của sự kiện KeyDown.</param>
        /// <param name="btnGui">Nút gửi tin nhắn sẽ được kích hoạt khi Enter.</param>
        public static void HandleKeyDown(KeyEventArgs e, Guna.UI2.WinForms.Guna2CircleButton btnGui)
        {
            // Enter thường -> gửi; Shift+Enter hoặc Ctrl+Enter -> xuống dòng
            if (e.KeyCode == Keys.Enter && !e.Shift && !e.Control)
            {
                e.SuppressKeyPress = true; // Không chèn newline, không beep hệ thống

                if (btnGui != null && btnGui.Enabled)
                    btnGui.PerformClick();
            }
        }

        #endregion
    }
}
