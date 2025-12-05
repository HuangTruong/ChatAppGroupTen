using System;
using System.Drawing;
using System.Windows.Forms;

namespace ChatApp.Controls // Hoặc namespace mà bạn đang sử dụng
{
    public partial class UserListItem : UserControl
    {
        // 1. KHAI BÁO SỰ KIỆN TÙY CHỈNH
        public event EventHandler<string> ActionButtonClicked;

        // 2. FIELDS
        private string _userId;

        // 3. HÀM KHỞI TẠO
        public UserListItem()
        {
            InitializeComponent();

            // Gắn sự kiện Click vào PictureBox hành động
            pbAction.Click += BtnAction_Click;

            // Thiết lập con trỏ tay mặc định
            pbAction.Cursor = Cursors.Hand;
        }

        // 4. CÁC THUỘC TÍNH PUBLIC (CHO FORM CHÍNH GÁN DỮ LIỆU)

        /// <summary>
        /// Thuộc tính để gán và hiển thị ảnh Avatar.
        /// </summary>
        public Image AvatarImage
        {
            get => pbAvatar.Image;
            set
            {
                pbAvatar.Image = value;
                pbAvatar.SizeMode = PictureBoxSizeMode.Zoom;
            }
        }

        /// <summary>
        /// Thuộc tính để gán và hiển thị ảnh Icon hành động (+ hoặc Tick).
        /// </summary>
        public Image ActionIcon
        {
            get => pbAction.Image;
            set
            {
                pbAction.Image = value;
                pbAction.SizeMode = PictureBoxSizeMode.Zoom;
            }
        }

        /// <summary>
        /// Thuộc tính để bật/tắt nút Action và cung cấp feedback hình ảnh.
        /// </summary>
        public bool IsActionEnabled
        {
            get => pbAction.Enabled;
            set
            {
                pbAction.Enabled = value;
                // Cung cấp feedback hình ảnh/trạng thái cho người dùng
                pbAction.Cursor = value ? Cursors.Hand : Cursors.Default;   
            }
        }

        // 5. PHƯƠNG THỨC SET DỮ LIỆU CƠ BẢN

        /// <summary>
        /// Gán LocalId và Tên hiển thị.
        /// </summary>
        /// <param name="localId">Mã định danh người dùng.</param>
        /// <param name="fullName">Họ tên đầy đủ.</param>
        public void SetUserData(string localId, string fullName)
        {
            _userId = localId;
            lblUserName.Text = fullName;
        }

        // 6. XỬ LÝ SỰ KIỆN CLICK CỦA PICTUREBOX
        private void BtnAction_Click(object sender, EventArgs e)
        {
            if (IsActionEnabled)
            {
                // Truyền LocalId (_userId) của người dùng này ra ngoài Form chính
                ActionButtonClicked?.Invoke(this, _userId);
            }
        }
    }
}