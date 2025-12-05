using System;

using System.Collections.Generic;

using System.ComponentModel;

using System.Data;

using System.Drawing;

using System.Linq;

using System.Text;

using System.Threading.Tasks;

using System.Windows.Forms;



namespace ChatApp.Forms

{

    public partial class frmFriendsList : Form

    {

        // ===============================================

        // I. KHAI BÁO BIẾN VÀ HẰNG SỐ

        // ===============================================

        // Khai báo biến trạng thái cho chức năng mở/thu gọn (Accordion)

        private bool isOnlineListExpanded = true;

        private bool isOfflineListExpanded = false;

        private bool isGroupChatExpanded = false;



        // Hằng số kích thước

        private const int GROUP_CHAT_HEADER_HEIGHT = 28;

        private const int FRIEND_ITEM_HEIGHT = 40;

        private const int GROUP_CHAT_EXPANDED_HEIGHT = 200;



        // SỐ LƯỢNG MỤC MỚI THEO YÊU CẦU CỦA BẠN

        private const int NUM_ONLINE_FRIENDS = 10; // Đã cập nhật: 10

        private const int NUM_OFFLINE_FRIENDS = 5;  // Đã cập nhật: 5

        private const int NUM_GROUP_CHATS = 4;    // Đã cập nhật: 4



        public frmFriendsList()

        {

            InitializeComponent();



            // 1. Thiết lập trạng thái ban đầu Group Chat

            pnlGroupChat.Height = GROUP_CHAT_HEADER_HEIGHT;

            pnlGroupChatList.Visible = false;



            // 2. Tải dữ liệu mẫu

            LoadSampleData();



            UpdateListHeight();

        }



        // ===============================================

        // II. CLASS LỒNG THAY THẾ CHO CUSTOM CONTROL (KHÔNG CÓ STATUS)

        // ===============================================



        public class FriendListItemControl : Panel

        {

            private PictureBox picAvatar;

            private Label lblUserName;



            public FriendListItemControl()

            {

                // Thiết lập Panel chính (mục bạn bè)

                this.Height = FRIEND_ITEM_HEIGHT;

                this.BackColor = Color.FromArgb(40, 40, 40);

                this.Dock = DockStyle.Top;

                this.Padding = new Padding(5);



                // 1. Avatar

                picAvatar = new PictureBox

                {

                    Location = new Point(5, (FRIEND_ITEM_HEIGHT - 30) / 2),

                    Size = new Size(30, 30),

                    SizeMode = PictureBoxSizeMode.Zoom,

                    BackColor = Color.FromArgb(30, 30, 30)

                };

                this.Controls.Add(picAvatar);



                // 2. Tên người dùng

                lblUserName = new Label

                {

                    Location = new Point(45, (FRIEND_ITEM_HEIGHT - 18) / 2),

                    AutoSize = true,

                    Font = new Font("Microsoft Sans Serif", 9.75F, FontStyle.Regular),

                    ForeColor = Color.White

                };

                this.Controls.Add(lblUserName);

            }



            // Thuộc tính để gán dữ liệu

            public Image Avatar

            {

                get { return picAvatar.Image; }

                set { picAvatar.Image = value; }

            }

            public string UserName

            {

                get { return lblUserName.Text; }

                set { lblUserName.Text = value; }

            }

        }



        // ===============================================

        // III. LOGIC ACCORDION (MỞ/THU GỌN)

        // ===============================================



        private void pnlOnlineHeader_Click(object sender, EventArgs e)

        {

            isOnlineListExpanded = !isOnlineListExpanded;

            pnlOnlineList.Visible = isOnlineListExpanded;

            UpdateListHeight();

        }



        private void pnlOfflineHeader_Click(object sender, EventArgs e)

        {

            isOfflineListExpanded = !isOfflineListExpanded;

            pnlOfflineList.Visible = isOfflineListExpanded;

            UpdateListHeight();

        }



        private void pnlGroupChatHeader_Click(object sender, EventArgs e)

        {

            isGroupChatExpanded = !isGroupChatExpanded;



            if (isGroupChatExpanded)

            {

                pnlGroupChat.Height = GROUP_CHAT_EXPANDED_HEIGHT;

                pnlGroupChatList.Visible = true;

            }

            else

            {

                pnlGroupChat.Height = GROUP_CHAT_HEADER_HEIGHT;

                pnlGroupChatList.Visible = false;

            }

        }



        // ===============================================

        // IV. TẢI DỮ LIỆU MẪU (SỐ LƯỢNG MỚI)

        // ===============================================



        private void LoadSampleData()

        {

            pnlOnlineList.AutoScroll = true;

            pnlOfflineList.AutoScroll = true;

            pnlGroupChatList.AutoScroll = true;



            // --- ONLINE FRIENDS (10 MỤC) ---

            for (int i = 1; i <= NUM_ONLINE_FRIENDS; i++)

            {

                FriendListItemControl online = new FriendListItemControl();

                online.UserName = $"Online_User_{i}";

                online.Avatar = Properties.Resources.CaiDat;

                pnlOnlineList.Controls.Add(online);

                pnlOnlineList.Controls.SetChildIndex(online, 0);

            }



            // --- OFFLINE FRIENDS (5 MỤC) ---

            for (int i = 1; i <= NUM_OFFLINE_FRIENDS; i++)

            {

                FriendListItemControl offline = new FriendListItemControl();

                offline.UserName = $"Offline_User_{i}";

                offline.Avatar = Properties.Resources.CaiDat;

                pnlOfflineList.Controls.Add(offline);

                pnlOfflineList.Controls.SetChildIndex(offline, 0);

            }



            // --- GROUP CHAT (4 MỤC) ---

            for (int i = 1; i <= NUM_GROUP_CHATS; i++)

            {

                FriendListItemControl groupChat = new FriendListItemControl();

                groupChat.UserName = $"Nhóm Chiến Thuật {i}";

                groupChat.Avatar = Properties.Resources.CaiDat;

                pnlGroupChatList.Controls.Add(groupChat);

                pnlGroupChatList.Controls.SetChildIndex(groupChat, 0);

            }

        }



        // Đảm bảo rằng bạn có hằng số này: private const int FRIEND_ITEM_HEIGHT = 40;



        private void UpdateListHeight()

        {

            // Giả định bạn có hằng số này: private const int FRIEND_ITEM_HEIGHT = 40;



            // --- 1. Xử lý Danh sách Online (pnlOnlineList) ---

            int totalOnlineHeight = 0;

            // Tính tổng chiều cao của tất cả các mục bạn bè Online

            foreach (Control control in pnlOnlineList.Controls)

            {

                totalOnlineHeight += FRIEND_ITEM_HEIGHT;

            }



            // Đặt chiều cao của pnlOnlineList: Giãn nở nếu mở, về 0 nếu thu gọn

            pnlOnlineList.Height = isOnlineListExpanded ? totalOnlineHeight : 0;



            // --- 2. Xử lý Danh sách Offline (pnlOfflineList) ---

            int totalOfflineHeight = 0;

            // Tính tổng chiều cao của tất cả các mục bạn bè Offline

            foreach (Control control in pnlOfflineList.Controls)

            {

                totalOfflineHeight += FRIEND_ITEM_HEIGHT;

            }



            // Đặt chiều cao của pnlOfflineList: Giãn nở nếu mở, về 0 nếu thu gọn

            pnlOfflineList.Height = isOfflineListExpanded ? totalOfflineHeight : 0;



            // --- 3. Buộc Bố cục Cập nhật ---

            // Sử dụng tên FlowLayoutPanel chính xác của bạn: flowLayoutPanel1 hoặc flpFriendListContainer

            // Tôi sẽ dùng flowLayoutPanel1 như mặc định, bạn hãy thay đổi nếu cần.

            flpFriendListContainer.PerformLayout();

        }

    }

}