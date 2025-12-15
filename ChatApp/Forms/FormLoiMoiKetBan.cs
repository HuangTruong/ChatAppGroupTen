using ChatApp.Controllers;
using ChatApp.Controls;
using ChatApp.Models.Users;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Web.UI;
using System.Windows.Forms;

namespace ChatApp.Forms
{
    public partial class FormLoiMoiKetBan : Form
    {
        #region ====== THUỘC TÍNH NỘI BỘ======

        private readonly FriendController _friendController;
        private readonly string _currentLocalId;
        private readonly string _currentToken;

        #endregion

        public FormLoiMoiKetBan(string localId, string token)
        {
            InitializeComponent();

            _currentLocalId = localId;
            _currentToken = token;

            _friendController = new FriendController(_currentLocalId);

            this.Load += async (sender, e) => await LoadFriendRequestsUsingFlowPanel();
        }

        #region ====== TẢI DỮ LIỆU VÀ HIỂN THỊ ======

        /// <summary>
        /// Tải danh sách lời mời kết bạn đang chờ.
        /// </summary>
        private async Task LoadFriendRequestsUsingFlowPanel()
        {
            try
            {
                pnlView.Controls.Clear();

                // 1. LẤY DANH SÁCH LỜI MỜI (trả về List<User> Profile của người gửi)
                List<User> friendRequests = await _friendController.LoadFriendRequestsAsync();

                // --- XỬ LÝ TRƯỜNG HỢP RỖNG ---
                if (friendRequests == null || friendRequests.Count == 0)
                {
                    DisplayEmptyMessage();
                    return;
                }

                // 2. DUYỆT VÀ TẠO CONTROL
                foreach (var user in friendRequests)
                {
                    var requestControl = new FriendRequestItem();

                    // Gán dữ liệu cơ bản (User Profile)
                    requestControl.SetUserData(localId: user.LocalId, fullName: user.FullName);

                    // Cài đặt Sự kiện (Sự kiện nhấn nút Accept/Reject)
                    requestControl.ActionButtonClicked += RequestControl_HandleAction;
                    
                    requestControl.Dock = DockStyle.Top;

                    pnlView.Controls.Add(requestControl);
                    pnlView.Controls.SetChildIndex(requestControl, 0); // Để cho thứ tự tin nhắn không bị ngược
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải danh sách lời mời: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        #endregion

        #region ====== XỬ LÝ HÀNH ĐỘNG (ACCEPT/REJECT) ======

        /// <summary>
        /// Xử lý sự kiện nhấn nút Chấp nhận hoặc Từ chối từ FriendRequestItem.
        /// </summary>
        private async void RequestControl_HandleAction(object sender, string requesterId, FriendRequestItem.RequestAction action)
        {
            FriendRequestItem clickedItem = (FriendRequestItem)sender;
            string actionName = (action == FriendRequestItem.RequestAction.Accept) ? "chấp nhận" : "từ chối";

            try
            {
                // Vô hiệu hóa nút tạm thời.
                clickedItem.IsActionEnabled = false;

                // 1. Thực hiện hành động (Accept/Reject) thông qua Controller
                if (action == FriendRequestItem.RequestAction.Accept)
                {
                    await _friendController.AcceptFriendRequestAsync(requesterId);
                }
                else // Reject
                {
                    await _friendController.RejectFriendRequestAsync(requesterId);
                }

                // 2. XÓA USER CONTROL KHỎI FLOW LAYOUT PANEL sau khi xử lý thành công
                pnlView.Controls.Remove(clickedItem);

                // 3. Kiểm tra và hiển thị Label rỗng nếu đây là lời mời cuối cùng
                if (pnlView.Controls.Count == 0)
                {
                    DisplayEmptyMessage();
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi {actionName} lời mời: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);

                // Bật lại nút nếu thất bại để người dùng có thể thử lại
                clickedItem.IsActionEnabled = true;
            }
        }

        #endregion

        #region ====== CÁC PHƯƠNG THỨC HỖ TRỢ HIỂN THỊ ======

        /// <summary>
        /// Tạo và hiển thị Label thông báo khi danh sách lời mời rỗng.
        /// </summary>
        private void DisplayEmptyMessage()
        {
            // Luôn xóa controls cũ trước khi thêm thông báo rỗng
            pnlView.Controls.Clear();

            Label lblEmpty = new Label();
            lblEmpty.Text = "Bạn không có lời mời kết bạn nào.";
            // 💥 Quan trọng: Cần set Width bằng với FlowLayoutPanel để căn giữa được
            lblEmpty.Width = pnlView.ClientSize.Width;

            lblEmpty.TextAlign = ContentAlignment.MiddleCenter;
            lblEmpty.ForeColor = Color.Gray;
            lblEmpty.Height = 50;

            pnlView.Controls.Add(lblEmpty);
        }

        #endregion

    }
}