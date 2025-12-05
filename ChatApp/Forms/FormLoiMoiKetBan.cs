using ChatApp.Controllers;
using ChatApp.Controls; 
using ChatApp.Models.Users;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ChatApp.Forms
{
    public partial class FormLoiMoiKetBan : Form
    {
        private readonly FriendController _friendController;
        private readonly string _currentLocalId;
        private readonly string _currentToken;

        public FormLoiMoiKetBan(string localId, string token)
        {
            InitializeComponent();

            _currentLocalId = localId;
            _currentToken = token;

            _friendController = new FriendController(_currentLocalId);

            flpView.AutoScroll = true;
            flpView.WrapContents = false;
            flpView.FlowDirection = FlowDirection.TopDown;

            this.Load += async (sender, e) => await LoadFriendRequestsUsingFlowPanel();
        }

        // --- PHƯƠNG THỨC TẢI DANH SÁCH LỜI MỜI ---

        private async Task LoadFriendRequestsUsingFlowPanel()
        {
            try
            {
                flpView.Controls.Clear();

                // 1. LẤY DANH SÁCH LỜI MỜI
                List<User> friendRequests = await _friendController.LoadFriendRequestsAsync();

                // --- XỬ LÝ TRƯỜNG HỢP RỖNG ---
                if (friendRequests == null || friendRequests.Count == 0)
                {
                    Label lblEmpty = new Label();
                    lblEmpty.Text = "Bạn không có lời mời kết bạn nào.";
                    lblEmpty.Dock = DockStyle.Top;
                    lblEmpty.TextAlign = ContentAlignment.MiddleCenter;
                    lblEmpty.ForeColor = Color.Gray;
                    lblEmpty.Height = 50;
                    flpView.Controls.Add(lblEmpty);
                    return;
                }

                // 2. DUYỆT VÀ TẠO CONTROL
                foreach (var user in friendRequests)
                {
                    var requestControl = new FriendRequestItem();

                    // I. Gán dữ liệu cơ bản
                    requestControl.SetUserData(localId: user.LocalId, fullName: user.FullName);

                    // Cài đặt Dock và Sự kiện
                    requestControl.ActionButtonClicked += RequestControl_HandleAction;

                    flpView.Controls.Add(requestControl);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải danh sách lời mời: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // --- PHƯƠNG THỨC XỬ LÝ CHẤP NHẬN HOẶC TỪ CHỐI (Giữ nguyên) ---

        private async void RequestControl_HandleAction(object sender, string requesterId, FriendRequestItem.RequestAction action)
        {
            FriendRequestItem clickedItem = (FriendRequestItem)sender;
            string actionName = (action == FriendRequestItem.RequestAction.Accept) ? "chấp nhận" : "từ chối";

            try
            {
                // Vô hiệu hóa nút tạm thời
                clickedItem.IsActionEnabled = false;

                // 1. Thực hiện hành động (Accept/Reject)
                if (action == FriendRequestItem.RequestAction.Accept)
                {
                    await _friendController.AcceptFriendRequestAsync(requesterId);
                }
                else // Reject
                {
                    await _friendController.RejectFriendRequestAsync(requesterId);
                }

                // 💥 2. XÓA USER CONTROL KHỎI FLOW LAYOUT PANEL
                flpView.Controls.Remove(clickedItem);

                // (Tùy chọn: Kiểm tra nếu danh sách trống thì thêm label "Không có lời mời nào")
                if (flpView.Controls.Count == 0)
                {
                    Label lblEmpty = new Label();
                    lblEmpty.Text = "Bạn không có lời mời kết bạn nào.";
                    lblEmpty.Dock = DockStyle.Top;
                    lblEmpty.TextAlign = ContentAlignment.MiddleCenter;
                    lblEmpty.ForeColor = Color.Gray;
                    lblEmpty.Height = 50;
                    flpView.Controls.Add(lblEmpty);
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi {actionName} lời mời: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                clickedItem.IsActionEnabled = true;
            }
        }
    }
}