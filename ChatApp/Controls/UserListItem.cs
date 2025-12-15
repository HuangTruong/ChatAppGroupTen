using System;
using System.Drawing;
using System.Windows.Forms;

namespace ChatApp.Controls // Hoặc namespace mà bạn đang sử dụng
{
    public partial class UserListItem : UserControl
    {
        #region ====== SỰ KIỆN VÀ FIELDS NỘI BỘ ======

        // 1. KHAI BÁO SỰ KIỆN TÙY CHỈNH

        /// <summary>
        /// Sự kiện được kích hoạt khi người dùng nhấn vào biểu tượng hành động.
        /// Sự kiện này truyền về LocalId của người dùng đang được hiển thị.
        /// </summary>
        public event EventHandler<string> ActionButtonClicked;

        // 2. FIELDS

        /// <summary>
        /// Lưu trữ LocalId của người dùng hiện tại (là người nhận hành động).
        /// </summary>
        private string _userId;

        #endregion

        #region ====== HÀM KHỞI TẠO (CONSTRUCTOR) ======

        /// <summary>
        /// Khởi tạo UserListItem.
        /// </summary>
        public UserListItem()
        {
            InitializeComponent();

            // Gắn sự kiện Click vào PictureBox hành động
            pbAction.Click += BtnAction_Click;

            // Thiết lập con trỏ tay mặc định
            pbAction.Cursor = Cursors.Hand;
        }

        #endregion

        #region ====== THUỘC TÍNH (PROPERTIES) ======
        /// <summary>
        /// Thuộc tính để bật/tắt nút Action và cung cấp feedback hình ảnh.
        /// Khi vô hiệu hóa (false), con trỏ sẽ trở lại mặc định.
        /// </summary>
        public bool IsActionEnabled
        {
            get => pbAction.Enabled;
            set
            {
                pbAction.Enabled = value;
                // Cung cấp feedback hình ảnh/trạng thái cho người dùng
                pbAction.Cursor = value ? Cursors.Hand : Cursors.Default;
                // Tùy chọn: Thêm hiệu ứng mờ (Opacity) nếu vô hiệu hóa
                pbAction.BackColor = value ? Color.Transparent : Color.LightGray;
            }
        }

        #endregion

        #region ====== PHƯƠNG THỨC VÀ SỰ KIỆN ======

        /// <summary>
        /// Gán LocalId và Tên hiển thị cho Control.
        /// </summary>
        public void SetUserData(string localId, string fullName)
        {
            _userId = localId;
            lblUserName.Text = fullName;
            this.Tag = localId;
        }

        /// <summary>
        /// Xử lý sự kiện Click của PictureBox hành động.
        /// Nếu nút được bật, kích hoạt sự kiện ActionButtonClicked và truyền _userId ra ngoài.
        /// </summary>
        private void BtnAction_Click(object sender, EventArgs e)
        {
            if (IsActionEnabled)
            {
                // Truyền LocalId (_userId) của người dùng này ra ngoài Form chính
                ActionButtonClicked?.Invoke(this, _userId);
            }
        }

        #endregion

    }
}