using ChatApp.Controls;
using ChatApp.Models.Users;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using ChatApp.Controllers;


namespace ChatApp.Forms
{
    public partial class TimKiemBanBe : Form
    {
        private readonly FriendController _friendController;
        private readonly string _currentLocalId;
        private readonly string _currentToken;

        // Khởi tạo Form: Bắt buộc nhận localId và token
        public TimKiemBanBe(string localId, string token)
        {
            InitializeComponent();

            // 1. Lưu ID và Token
            _currentLocalId = localId;
            _currentToken = token;

            // 2. Khởi tạo Controller
            _friendController = new FriendController(_currentLocalId);

            // 3. Cài đặt các thuộc tính của FlowLayoutPanel
            flpView.AutoScroll = true;
            flpView.WrapContents = false; // Đảm bảo các item xếp theo chiều dọc

            // 4. Load dữ liệu khi Form được hiển thị
            this.Load += async (sender, e) => await LoadAllUsersUsingFlowPanel();
        }

        private async Task LoadAllUsersUsingFlowPanel()
        {
            try
            {
                flpView.Controls.Clear();

                // =======================================================
                // 1. TẢI ẢNH ICON (+) CHỈ MỘT LẦN
                // =======================================================
                string path = Path.Combine(Application.StartupPath, @"Resources\plus.png");
                path = Path.GetFullPath(path);

                Image plusIcon = null;
                if (File.Exists(path))
                {
                    plusIcon = Image.FromFile(path);
                }
                else
                {
                    MessageBox.Show("Không tìm thấy ảnh: " + path, "Lỗi Tải Ảnh", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

                // 2. Lấy danh sách người dùng
                List<User> userList = await _friendController.LoadAllUsersForDisplayAsync();

                // 3. Duyệt và tạo Control
                foreach (var user in userList)
                {
                    var userControl = new UserListItem();

                    // I. GÁN DỮ LIỆU CƠ BẢN
                    userControl.SetUserData(
                        localId: user.LocalId,
                        fullName: user.FullName
                    );

                    // II. GÁN ICON HÀNH ĐỘNG (+)
                    userControl.ActionIcon = plusIcon;

                    // Cài đặt Dock và Sự kiện
                    userControl.ActionButtonClicked += UserControl_SendRequest;

                    flpView.Controls.Add(userControl);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải danh sách người dùng: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Xử lý sự kiện nhấn nút Gửi lời mời từ UserListItem.
        /// </summary>
        private async void UserControl_SendRequest(object sender, string receiverId)
        {
            UserListItem clickedItem = (UserListItem)sender;

            try
            {
                // Vô hiệu hóa nút tạm thời
                clickedItem.IsActionEnabled = false;

                // Gửi lời mời qua Controller
                await _friendController.SendRequestAsync(receiverId);

                MessageBox.Show("Đã gửi lời mời kết bạn!", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);

                // Tải lại danh sách để cập nhật trạng thái người dùng
                await LoadAllUsersUsingFlowPanel();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi gửi lời mời: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                // Bật lại nút nếu thất bại
                clickedItem.IsActionEnabled = true;
            }
        }
    }
}