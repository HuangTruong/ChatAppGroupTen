using ChatApp.Models.Messages;
using ChatApp.Services.UI;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace ChatApp.Controllers
{
    /// <summary>
    /// Controller quản lý phần UI render chat:
    /// - Setup ChatRenderer (render trực tiếp lên Panel host)
    /// - Clear / RenderInitial / QueueAppend
    /// </summary>
    public class ChatViewController : IDisposable
    {
        #region ====== KHAI BÁO BIẾN ======

        private readonly Panel _hostPanel;

        /// <summary>
        /// Hàm tạo bubble Control từ ChatMessage (ví dụ: MessageBubbles).
        /// Renderer sẽ tự add bubble vào pnlKhungChat theo layout dọc.
        /// </summary>
        private readonly Func<ChatMessage, Control> _bubbleFactory;

        private ChatRenderer _renderer;

        #endregion

        #region ====== THUỘC TÍNH ======

        /// <summary>
        /// Giới hạn số tin giữ trên UI để tránh nặng dần.
        /// </summary>
        public int MaxUiMessages { get; set; }

        #endregion

        #region ====== HÀM KHỞI TẠO ======

        public ChatViewController(Panel hostPanel, Func<ChatMessage, Control> bubbleFactory)
        {
            _hostPanel = hostPanel;
            _bubbleFactory = bubbleFactory;

            MaxUiMessages = 300;
        }

        #endregion

        #region ====== KHỞI TẠO RENDERER ======

        public void Initialize()
        {
            try
            {
                DisposeRenderer();

                _renderer = new ChatRenderer(_hostPanel, _bubbleFactory);
                _renderer.MaxUiMessages = MaxUiMessages;
            }
            catch
            {
                // ignore
            }
        }

        #endregion

        #region ====== API RENDER CHAT ======

        public void Clear()
        {
            try
            {
                if (_renderer != null)
                {
                    _renderer.Clear();
                    return;
                }

                if (_hostPanel != null)
                {
                    _hostPanel.SuspendLayout();
                    try { _hostPanel.Controls.Clear(); }
                    finally { _hostPanel.ResumeLayout(); }
                }
            }
            catch
            {
                // ignore
            }
        }

        public void RenderInitial(IList<ChatMessage> messages, string ownerKey)
        {
            if (_renderer == null) return;
            _renderer.RenderInitial(messages, ownerKey);
        }

        public void QueueAppend(ChatMessage msg, string ownerKey)
        {
            if (_renderer == null) return;
            if (msg == null) return;

            _renderer.QueueAppend(msg, ownerKey);
        }

        #endregion

        #region ====== GIẢI PHÓNG TÀI NGUYÊN ======

        private void DisposeRenderer()
        {
            try
            {
                if (_renderer != null)
                {
                    _renderer.Dispose();
                    _renderer = null;
                }
            }
            catch { }
        }

        public void Dispose()
        {
            try { DisposeRenderer(); } catch { }
        }

        #endregion
    }
}
