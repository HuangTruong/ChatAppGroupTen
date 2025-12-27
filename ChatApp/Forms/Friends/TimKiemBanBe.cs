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

                // 1. Load danh sách users
                List<User> userList = await _friendController.LoadAllUsersForDisplayAsync();

                // 2. Load danh sách ID đã gửi lời mời
                var outgoingRequestIds = await _friendController.GetOutgoingRequestIdsAsync();

                // 3. Load theme một lần
                bool isDark = await _themeService.GetThemeAsync(_currentLocalId);
                ThemeManager.ApplyTheme(this, isDark);

                foreach (var user in userList)
                {
                    var userControl = new UserListItem();

                    userControl.SetUserData(
                        localId: user.LocalId,
                        DisplayName: user.DisplayName
                    );

                    // Kiểm tra xem đã gửi lời mời chưa
                    bool hasOutgoingRequest = outgoingRequestIds.Contains(user.LocalId);
                    
                    // Set mode: nếu đã gửi thì hiển thị mode "Cancel", chưa thì "Send"
                    userControl.SetActionMode(hasOutgoingRequest ? UserListItem.ActionMode.Cancel : UserListItem.ActionMode.Send);

                    // Gán event handler
                    if (hasOutgoingRequest)
                    {
                        userControl.ActionButtonClicked -= UserControl_SendRequest;
                        userControl.ActionButtonClicked += UserControl_CancelRequest;
                    }
                    else
                    {
                        userControl.ActionButtonClicked -= UserControl_CancelRequest; // Đảm bảo gỡ bỏ nếu có
                        userControl.ActionButtonClicked += UserControl_SendRequest;
                    }

                    userControl.Dock = DockStyle.Top;

                    pnlView.Controls.Add(userControl);
                    pnlView.Controls.SetChildIndex(userControl, 0);
                }
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

                // Chuyển sang mode Cancel và đổi event handler
                clickedItem.SetActionMode(UserListItem.ActionMode.Cancel);
                clickedItem.ActionButtonClicked -= UserControl_SendRequest;
                clickedItem.ActionButtonClicked += UserControl_CancelRequest;
                clickedItem.IsActionEnabled = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi gửi lời mời: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                clickedItem.IsActionEnabled = true;
            }
        }

        /// <summary>
        /// Xử lý sự kiện nhấn nút Hủy lời mời từ UserListItem.
        /// </summary>
        private async void UserControl_CancelRequest(object sender, string receiverId)
        {
            UserListItem clickedItem = (UserListItem)sender;

            try
            {
                clickedItem.IsActionEnabled = false;

                // Hủy lời mời qua Controller.
                await _friendController.CancelFriendRequestAsync(receiverId);

                MessageBox.Show("Đã hủy lời mời kết bạn thành công!", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);

                // Chuyển sang mode Send và đổi event handler
                clickedItem.SetActionMode(UserListItem.ActionMode.Send);
                clickedItem.ActionButtonClicked -= UserControl_CancelRequest;
                clickedItem.ActionButtonClicked += UserControl_SendRequest;
                clickedItem.IsActionEnabled = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi hủy lời mời: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                clickedItem.IsActionEnabled = true;
            }
        }

        #endregion
    }
}