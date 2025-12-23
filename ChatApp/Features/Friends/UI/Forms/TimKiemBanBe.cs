using ChatApp.Controllers;
using ChatApp.Controls;
using ChatApp.Models.Users;
using ChatApp.Services.Firebase;
using ChatApp.Services.UI;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TrackBar; // Nên xóa nếu không dùng

namespace ChatApp.Forms
{
    /// <summary>
    /// Form cho phép người dùng tìm kiếm và gửi lời mời kết bạn đến các user khác trong hệ thống.
    /// </summary>
    public partial class TimKiemBanBe : Form
    {
        #region ====== THUỘC TÍNH NỘI BỘ======

        private readonly FriendController _friendController;
        private readonly string _currentLocalId;
        private readonly string _currentToken;

        /// <summary>
        /// Dịch vụ để cập nhật chế độ ngày đêm (dark/light).
        /// </summary>
        private readonly ThemeService _themeService = new ThemeService();

        #endregion

        public TimKiemBanBe(string localId, string token)
        {
            InitializeComponent();

            // 1. Lưu ID và Token
            _currentLocalId = localId;
            _currentToken = token;

            // 2. Khởi tạo Controller (Cần đảm bảo FriendController đã được truyền đúng dependencies)
            _friendController = new FriendController(_currentLocalId);

            // 3. Load dữ liệu khi Form được hiển thị
            this.Load += async (sender, e) => await LoadAllUsersUsingFlowPanel();
        }

        #region ====== PHƯƠNG THỨC TẢI DỮ LIỆU VÀ HIỂN THỊ ======

        /// <summary>
        /// Tải tất cả user (trừ người dùng hiện tại) và hiển thị chúng lên FlowLayoutPanel.
        /// </summary>
        private async Task LoadAllUsersUsingFlowPanel()
        {
            try
            {
                pnlView.Controls.Clear();

                List<User> userList = await _friendController.LoadAllUsersForDisplayAsync();

                foreach (var user in userList)
                {
                    var userControl = new UserListItem();

                    userControl.SetUserData(
                        localId: user.LocalId,
                        fullName: user.FullName
                    );

                    userControl.ActionButtonClicked += UserControl_SendRequest;

                    userControl.Dock = DockStyle.Top;

                    pnlView.Controls.Add(userControl);
                    pnlView.Controls.SetChildIndex(userControl, 0); // Để cho thứ tự tin nhắn không bị ngược

                }

                // Load chế độ ngày đêm
                bool isDark = await _themeService.GetThemeAsync(_currentLocalId);
                ThemeManager.ApplyTheme(this, isDark);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải danh sách người dùng: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        #endregion

        #region ====== XỬ LÝ SỰ KIỆN HÀNH ĐỘNG ======

        /// <summary>
        /// Xử lý sự kiện nhấn nút Gửi lời mời từ UserListItem.
        /// </summary>
        private async void UserControl_SendRequest(object sender, string receiverId)
        {
            // Ép kiểu sender về UserListItem để tương tác với control đó
            UserListItem clickedItem = (UserListItem)sender;

            try
            {
                clickedItem.IsActionEnabled = false;

                // Gửi lời mời qua Controller.
                await _friendController.SendRequestAsync(receiverId);

                MessageBox.Show("Đã gửi lời mời kết bạn thành công!", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);

                // Tải lại danh sách để cập nhật trạng thái người dùng.
                // Hiện tại, việc tải lại toàn bộ form sẽ xóa item đã gửi đi.
                await LoadAllUsersUsingFlowPanel();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi gửi lời mời: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                clickedItem.IsActionEnabled = true;
            }
        }

        #endregion
    }
}