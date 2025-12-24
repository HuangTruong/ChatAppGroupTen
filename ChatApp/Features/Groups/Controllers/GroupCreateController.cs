using ChatApp.Forms;
using ChatApp.Models.Users;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ChatApp.Controllers
{
    /// <summary>
    /// Controller gom luồng tạo nhóm:
    /// - Mở form TaoNhom để chọn tên nhóm + thành viên
    /// - Gọi CreateGroupAsync để tạo nhóm trên Firebase
    /// - Reload lại danh sách hội thoại (để thấy nhóm mới)
    /// - Tự mở nhóm vừa tạo để bắt đầu chat ngay
    /// </summary>
    public class GroupCreateController
    {
        #region ====== BIẾN THÀNH VIÊN ======

        private readonly string _currentUserId;
        private readonly string _token;

        private readonly NhanTinNhomController _groupController;
        private readonly ConversationListController _conversationListController;

        /// <summary>
        /// Callback mở group theo groupId (thường gọi ChatSessionController.OpenGroupConversationById).
        /// </summary>
        private readonly Action<string> _openGroupById;

        #endregion

        #region ====== HÀM KHỞI TẠO ======

        public GroupCreateController(
            string currentUserId,
            string token,
            NhanTinNhomController groupController,
            ConversationListController conversationListController,
            Action<string> openGroupById)
        {
            _currentUserId = currentUserId;
            _token = token;
            _groupController = groupController;
            _conversationListController = conversationListController;
            _openGroupById = openGroupById;
        }

        #endregion

        #region ====== LUỒNG TẠO NHÓM ======

        public void ShowCreateGroupFlow(IWin32Window owner, Dictionary<string, User> friends)
        {
            if (friends == null || friends.Count == 0)
            {
                MessageBox.Show(owner, "Chưa có danh sách bạn bè để tạo nhóm.", "Thông báo",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // Chạy async để không block UI khi create + reload
            _ = RunCreateGroupAsync(owner, friends);
        }

        private async Task RunCreateGroupAsync(IWin32Window owner, Dictionary<string, User> friends)
        {
            try
            {
                using (TaoNhom f = new TaoNhom(friends, _currentUserId, _token))
                {
                    DialogResult dr = f.ShowDialog(owner);
                    if (dr != DialogResult.OK)
                    {
                        return;
                    }

                    string groupName = f.GroupName;
                    List<string> members = f.SelectedMemberIds;

                    string newGroupId = await _groupController
                        .CreateGroupAsync(_currentUserId, groupName, members)
                        .ConfigureAwait(true);

                    if (_conversationListController != null)
                    {
                        await _conversationListController.ReloadAsync().ConfigureAwait(true);
                    }

                    if (!string.IsNullOrWhiteSpace(newGroupId))
                    {
                        if (_openGroupById != null)
                        {
                            _openGroupById(newGroupId);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(owner, "Tạo nhóm thất bại: " + ex.Message, "Lỗi",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        #endregion
    }
}
