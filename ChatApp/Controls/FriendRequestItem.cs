using System;
using System.Drawing;
using System.Windows.Forms;

namespace ChatApp.Controls
{
    public partial class FriendRequestItem : UserControl
    {
        private string _requesterId;

        /// <summary>
        /// Định nghĩa 2 loại hành động mà Control này có thể kích hoạt (Accept hoặc Reject).
        /// </summary>
        public enum RequestAction
        {
            Accept,
            Reject
        }

        /// <summary>
        /// Định nghĩa mẫu ủy quyền (delegate) cho sự kiện ActionButtonClicked.
        /// Sự kiện này truyền ID người gửi lời mời và hành động được chọn.
        /// </summary>
        public delegate void ActionButtonClickedEventHandler(object sender, string requesterId, RequestAction action);

        /// <summary>
        /// Sự kiện được kích hoạt khi người dùng nhấn nút Chấp nhận hoặc Từ chối.
        /// </summary>
        public event ActionButtonClickedEventHandler ActionButtonClicked;

        public FriendRequestItem()
        {
            InitializeComponent();

            // Đặt sự kiện Click cho 2 PictureBox
            pbAccept.Click += ActionButton_Click;
            pbReject.Click += ActionButton_Click;

            // Đặt thuộc tính Cursor thành Hand cho các nút hành động
            pbAccept.Cursor = Cursors.Hand;
            pbReject.Cursor = Cursors.Hand;
        }

        #region ====== PUBLIC PROPERTIES VÀ SET DATA ======

        /// <summary>
        /// Gán dữ liệu cơ bản cho Control (ID người gửi và Tên hiển thị).
        /// </summary>
        public void SetUserData(string localId, string fullName)
        {
            _requesterId = localId;
            lblUserName.Text = fullName;
        }

        /// <summary>
        /// Bật/Tắt khả năng tương tác của các nút hành động (Accept/Reject).
        /// Dùng để ngăn chặn double click hoặc thao tác khi đang xử lý.
        /// </summary>
        public bool IsActionEnabled
        {
            set
            {
                pbAccept.Enabled = value;
                pbReject.Enabled = value;
                pbAccept.Cursor = value ? Cursors.Hand : Cursors.Default;
                pbReject.Cursor = value ? Cursors.Hand : Cursors.Default;
            }
        }

        #endregion

        #region ====== XỬ LÝ SỰ KIỆN CLICK CHUNG ======

        /// <summary>
        /// Xử lý sự kiện click chung cho cả nút Chấp nhận và Từ chối.
        /// </summary>
        private void ActionButton_Click(object sender, EventArgs e)
        {
            // Kiểm tra xem sự kiện có được gán ở Form chính không
            if (ActionButtonClicked == null || !(sender is PictureBox)) return;

            // Xác định hành động (Accept/Reject)
            RequestAction action;
            if (sender == pbAccept)
            {
                action = RequestAction.Accept;
            }
            else if (sender == pbReject)
            {
                action = RequestAction.Reject;
            }
            else
            {
                return;
            }

            // Truyền ID người gửi (_requesterId) và hành động ra ngoài Form chính
            ActionButtonClicked(this, _requesterId, action);
        }

        #endregion
    }
}