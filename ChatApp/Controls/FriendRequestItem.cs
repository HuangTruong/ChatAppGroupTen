using System;
using System.Drawing;
using System.Windows.Forms;

namespace ChatApp.Controls
{
    public partial class FriendRequestItem : UserControl
    {
        // Trường private để lưu ID người gửi lời mời
        private string _requesterId;

        // Định nghĩa 2 loại hành động mà Control này có thể kích hoạt
        public enum RequestAction
        {
            Accept,
            Reject
        }

        public delegate void ActionButtonClickedEventHandler(object sender, string requesterId, RequestAction action);
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
        /// Gán dữ liệu cơ bản cho Control.
        /// </summary>
        public void SetUserData(string localId, string fullName)
        {
            _requesterId = localId;
            lblUserName.Text = fullName;
        }

        /// <summary>
        /// Gán hình ảnh Avatar (PictureBox: pbAvatar).
        /// </summary>
        public Image AvatarImage
        {
            get => pbAvatar.Image;
            set => pbAvatar.Image = value;
        }

        /// <summary>
        /// Bật/Tắt khả năng tương tác của các nút hành động.
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

        #region ====== XỬ LÝ SỰ KIỆN CLICK ======

        /// <summary>
        /// Xử lý sự kiện click chung cho cả nút Chấp nhận và Từ chối.
        /// </summary>
        private void ActionButton_Click(object sender, EventArgs e)
        {
            if (ActionButtonClicked == null) return;

            // Xác định nút nào được nhấn
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

            // Truyền ID từ trường private
            ActionButtonClicked(this, _requesterId, action);
        }

        #endregion
    }
}