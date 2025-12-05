namespace ChatApp.Forms
{
    partial class frmFriendsList
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmFriendsList));
            this.pnlUserInfo = new System.Windows.Forms.Panel();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.guna2CirclePictureBox1 = new Guna.UI2.WinForms.Guna2CirclePictureBox();
            this.pnlGroupChat = new System.Windows.Forms.Panel();
            this.pnlGroupChatList = new System.Windows.Forms.Panel();
            this.pnlGroupChatHeader = new System.Windows.Forms.Panel();
            this.picGroupChatToggle = new System.Windows.Forms.PictureBox();
            this.picCreateGroup = new System.Windows.Forms.PictureBox();
            this.label6 = new System.Windows.Forms.Label();
            this.flpFriendListContainer = new System.Windows.Forms.FlowLayoutPanel();
            this.pnlFriendsHeader = new System.Windows.Forms.Panel();
            this.picSearch = new System.Windows.Forms.PictureBox();
            this.picAddFriends = new System.Windows.Forms.PictureBox();
            this.label3 = new System.Windows.Forms.Label();
            this.pnlOnlineHeader = new System.Windows.Forms.Panel();
            this.lblOnline = new System.Windows.Forms.Label();
            this.picOnlineToggle = new System.Windows.Forms.PictureBox();
            this.pnlOnlineList = new System.Windows.Forms.Panel();
            this.pnlOfflineHeader = new System.Windows.Forms.Panel();
            this.lblOffline = new System.Windows.Forms.Label();
            this.picOfflineToggle = new System.Windows.Forms.PictureBox();
            this.pnlOfflineList = new System.Windows.Forms.Panel();
            this.pnlUserInfo.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.guna2CirclePictureBox1)).BeginInit();
            this.pnlGroupChat.SuspendLayout();
            this.pnlGroupChatHeader.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picGroupChatToggle)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.picCreateGroup)).BeginInit();
            this.flpFriendListContainer.SuspendLayout();
            this.pnlFriendsHeader.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picSearch)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.picAddFriends)).BeginInit();
            this.pnlOnlineHeader.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picOnlineToggle)).BeginInit();
            this.pnlOfflineHeader.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picOfflineToggle)).BeginInit();
            this.SuspendLayout();
            // 
            // pnlUserInfo
            // 
            this.pnlUserInfo.Controls.Add(this.label2);
            this.pnlUserInfo.Controls.Add(this.label1);
            this.pnlUserInfo.Controls.Add(this.guna2CirclePictureBox1);
            this.pnlUserInfo.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlUserInfo.Location = new System.Drawing.Point(0, 0);
            this.pnlUserInfo.Name = "pnlUserInfo";
            this.pnlUserInfo.Size = new System.Drawing.Size(617, 86);
            this.pnlUserInfo.TabIndex = 0;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(86, 43);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(37, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "Online";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(82, 21);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(158, 20);
            this.label1.TabIndex = 1;
            this.label1.Text = "Trần Phước Hoàng";
            // 
            // guna2CirclePictureBox1
            // 
            this.guna2CirclePictureBox1.Image = ((System.Drawing.Image)(resources.GetObject("guna2CirclePictureBox1.Image")));
            this.guna2CirclePictureBox1.ImageRotate = 0F;
            this.guna2CirclePictureBox1.Location = new System.Drawing.Point(12, 12);
            this.guna2CirclePictureBox1.Name = "guna2CirclePictureBox1";
            this.guna2CirclePictureBox1.ShadowDecoration.Mode = Guna.UI2.WinForms.Enums.ShadowMode.Circle;
            this.guna2CirclePictureBox1.Size = new System.Drawing.Size(64, 64);
            this.guna2CirclePictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.guna2CirclePictureBox1.TabIndex = 0;
            this.guna2CirclePictureBox1.TabStop = false;
            // 
            // pnlGroupChat
            // 
            this.pnlGroupChat.Controls.Add(this.pnlGroupChatList);
            this.pnlGroupChat.Controls.Add(this.pnlGroupChatHeader);
            this.pnlGroupChat.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlGroupChat.Location = new System.Drawing.Point(0, 350);
            this.pnlGroupChat.Name = "pnlGroupChat";
            this.pnlGroupChat.Size = new System.Drawing.Size(617, 100);
            this.pnlGroupChat.TabIndex = 1;
            // 
            // pnlGroupChatList
            // 
            this.pnlGroupChatList.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlGroupChatList.Location = new System.Drawing.Point(0, 28);
            this.pnlGroupChatList.Name = "pnlGroupChatList";
            this.pnlGroupChatList.Size = new System.Drawing.Size(617, 72);
            this.pnlGroupChatList.TabIndex = 2;
            this.pnlGroupChatList.Visible = false;
            // 
            // pnlGroupChatHeader
            // 
            this.pnlGroupChatHeader.Controls.Add(this.picGroupChatToggle);
            this.pnlGroupChatHeader.Controls.Add(this.picCreateGroup);
            this.pnlGroupChatHeader.Controls.Add(this.label6);
            this.pnlGroupChatHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlGroupChatHeader.Location = new System.Drawing.Point(0, 0);
            this.pnlGroupChatHeader.Name = "pnlGroupChatHeader";
            this.pnlGroupChatHeader.Size = new System.Drawing.Size(617, 28);
            this.pnlGroupChatHeader.TabIndex = 1;
            // 
            // picGroupChatToggle
            // 
            this.picGroupChatToggle.Image = global::ChatApp.Properties.Resources.CaiDat;
            this.picGroupChatToggle.Location = new System.Drawing.Point(6, 3);
            this.picGroupChatToggle.Name = "picGroupChatToggle";
            this.picGroupChatToggle.Size = new System.Drawing.Size(29, 17);
            this.picGroupChatToggle.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.picGroupChatToggle.TabIndex = 5;
            this.picGroupChatToggle.TabStop = false;
            this.picGroupChatToggle.Click += new System.EventHandler(this.pnlGroupChatHeader_Click);
            // 
            // picCreateGroup
            // 
            this.picCreateGroup.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.picCreateGroup.Image = global::ChatApp.Properties.Resources.CaiDat;
            this.picCreateGroup.Location = new System.Drawing.Point(567, 6);
            this.picCreateGroup.Name = "picCreateGroup";
            this.picCreateGroup.Size = new System.Drawing.Size(35, 17);
            this.picCreateGroup.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.picCreateGroup.TabIndex = 2;
            this.picCreateGroup.TabStop = false;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label6.Location = new System.Drawing.Point(41, 0);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(102, 20);
            this.label6.TabIndex = 1;
            this.label6.Text = "Group Chat";
            // 
            // flpFriendListContainer
            // 
            this.flpFriendListContainer.AutoScroll = true;
            this.flpFriendListContainer.Controls.Add(this.pnlFriendsHeader);
            this.flpFriendListContainer.Controls.Add(this.pnlOnlineHeader);
            this.flpFriendListContainer.Controls.Add(this.pnlOnlineList);
            this.flpFriendListContainer.Controls.Add(this.pnlOfflineHeader);
            this.flpFriendListContainer.Controls.Add(this.pnlOfflineList);
            this.flpFriendListContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flpFriendListContainer.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.flpFriendListContainer.Location = new System.Drawing.Point(0, 86);
            this.flpFriendListContainer.Name = "flpFriendListContainer";
            this.flpFriendListContainer.Size = new System.Drawing.Size(617, 264);
            this.flpFriendListContainer.TabIndex = 2;
            this.flpFriendListContainer.WrapContents = false;
            // 
            // pnlFriendsHeader
            // 
            this.pnlFriendsHeader.Controls.Add(this.picSearch);
            this.pnlFriendsHeader.Controls.Add(this.picAddFriends);
            this.pnlFriendsHeader.Controls.Add(this.label3);
            this.pnlFriendsHeader.Location = new System.Drawing.Point(3, 3);
            this.pnlFriendsHeader.Name = "pnlFriendsHeader";
            this.pnlFriendsHeader.Size = new System.Drawing.Size(611, 27);
            this.pnlFriendsHeader.TabIndex = 0;
            // 
            // picSearch
            // 
            this.picSearch.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.picSearch.Image = global::ChatApp.Properties.Resources.CaiDat;
            this.picSearch.Location = new System.Drawing.Point(523, 3);
            this.picSearch.Name = "picSearch";
            this.picSearch.Size = new System.Drawing.Size(35, 17);
            this.picSearch.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.picSearch.TabIndex = 5;
            this.picSearch.TabStop = false;
            // 
            // picAddFriends
            // 
            this.picAddFriends.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.picAddFriends.Image = global::ChatApp.Properties.Resources.CaiDat;
            this.picAddFriends.Location = new System.Drawing.Point(564, 3);
            this.picAddFriends.Name = "picAddFriends";
            this.picAddFriends.Size = new System.Drawing.Size(35, 17);
            this.picAddFriends.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.picAddFriends.TabIndex = 4;
            this.picAddFriends.TabStop = false;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Dock = System.Windows.Forms.DockStyle.Left;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(0, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(69, 20);
            this.label3.TabIndex = 1;
            this.label3.Text = "Friends";
            // 
            // pnlOnlineHeader
            // 
            this.pnlOnlineHeader.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pnlOnlineHeader.Controls.Add(this.lblOnline);
            this.pnlOnlineHeader.Controls.Add(this.picOnlineToggle);
            this.pnlOnlineHeader.Location = new System.Drawing.Point(3, 36);
            this.pnlOnlineHeader.Name = "pnlOnlineHeader";
            this.pnlOnlineHeader.Size = new System.Drawing.Size(611, 25);
            this.pnlOnlineHeader.TabIndex = 1;
            // 
            // lblOnline
            // 
            this.lblOnline.AutoSize = true;
            this.lblOnline.Location = new System.Drawing.Point(38, 7);
            this.lblOnline.Name = "lblOnline";
            this.lblOnline.Size = new System.Drawing.Size(90, 13);
            this.lblOnline.TabIndex = 5;
            this.lblOnline.Text = "Online Friends (X)";
            // 
            // picOnlineToggle
            // 
            this.picOnlineToggle.Image = global::ChatApp.Properties.Resources.CaiDat;
            this.picOnlineToggle.Location = new System.Drawing.Point(3, 3);
            this.picOnlineToggle.Name = "picOnlineToggle";
            this.picOnlineToggle.Size = new System.Drawing.Size(29, 17);
            this.picOnlineToggle.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.picOnlineToggle.TabIndex = 4;
            this.picOnlineToggle.TabStop = false;
            this.picOnlineToggle.Click += new System.EventHandler(this.pnlOnlineHeader_Click);
            // 
            // pnlOnlineList
            // 
            this.pnlOnlineList.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pnlOnlineList.Location = new System.Drawing.Point(3, 67);
            this.pnlOnlineList.Name = "pnlOnlineList";
            this.pnlOnlineList.Size = new System.Drawing.Size(611, 42);
            this.pnlOnlineList.TabIndex = 2;
            this.pnlOnlineList.Visible = false;
            // 
            // pnlOfflineHeader
            // 
            this.pnlOfflineHeader.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pnlOfflineHeader.Controls.Add(this.lblOffline);
            this.pnlOfflineHeader.Controls.Add(this.picOfflineToggle);
            this.pnlOfflineHeader.Location = new System.Drawing.Point(3, 115);
            this.pnlOfflineHeader.Name = "pnlOfflineHeader";
            this.pnlOfflineHeader.Size = new System.Drawing.Size(611, 25);
            this.pnlOfflineHeader.TabIndex = 3;
            // 
            // lblOffline
            // 
            this.lblOffline.AutoSize = true;
            this.lblOffline.Location = new System.Drawing.Point(38, 7);
            this.lblOffline.Name = "lblOffline";
            this.lblOffline.Size = new System.Drawing.Size(90, 13);
            this.lblOffline.TabIndex = 5;
            this.lblOffline.Text = "Offline Friends (X)";
            // 
            // picOfflineToggle
            // 
            this.picOfflineToggle.Image = global::ChatApp.Properties.Resources.CaiDat;
            this.picOfflineToggle.Location = new System.Drawing.Point(3, 3);
            this.picOfflineToggle.Name = "picOfflineToggle";
            this.picOfflineToggle.Size = new System.Drawing.Size(29, 17);
            this.picOfflineToggle.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.picOfflineToggle.TabIndex = 4;
            this.picOfflineToggle.TabStop = false;
            this.picOfflineToggle.Click += new System.EventHandler(this.pnlOfflineHeader_Click);
            // 
            // pnlOfflineList
            // 
            this.pnlOfflineList.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pnlOfflineList.Location = new System.Drawing.Point(3, 146);
            this.pnlOfflineList.Name = "pnlOfflineList";
            this.pnlOfflineList.Size = new System.Drawing.Size(611, 42);
            this.pnlOfflineList.TabIndex = 4;
            this.pnlOfflineList.Visible = false;
            // 
            // frmFriendsList
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(617, 450);
            this.Controls.Add(this.flpFriendListContainer);
            this.Controls.Add(this.pnlGroupChat);
            this.Controls.Add(this.pnlUserInfo);
            this.Name = "frmFriendsList";
            this.pnlUserInfo.ResumeLayout(false);
            this.pnlUserInfo.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.guna2CirclePictureBox1)).EndInit();
            this.pnlGroupChat.ResumeLayout(false);
            this.pnlGroupChatHeader.ResumeLayout(false);
            this.pnlGroupChatHeader.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picGroupChatToggle)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.picCreateGroup)).EndInit();
            this.flpFriendListContainer.ResumeLayout(false);
            this.pnlFriendsHeader.ResumeLayout(false);
            this.pnlFriendsHeader.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picSearch)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.picAddFriends)).EndInit();
            this.pnlOnlineHeader.ResumeLayout(false);
            this.pnlOnlineHeader.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picOnlineToggle)).EndInit();
            this.pnlOfflineHeader.ResumeLayout(false);
            this.pnlOfflineHeader.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picOfflineToggle)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel pnlUserInfo;
        private System.Windows.Forms.Panel pnlGroupChat;
        private System.Windows.Forms.FlowLayoutPanel flpFriendListContainer;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private Guna.UI2.WinForms.Guna2CirclePictureBox guna2CirclePictureBox1;
        private System.Windows.Forms.Panel pnlFriendsHeader;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Panel pnlOnlineHeader;
        private System.Windows.Forms.Label lblOnline;
        private System.Windows.Forms.PictureBox picOnlineToggle;
        private System.Windows.Forms.Panel pnlOnlineList;
        private System.Windows.Forms.Panel pnlOfflineHeader;
        private System.Windows.Forms.Label lblOffline;
        private System.Windows.Forms.PictureBox picOfflineToggle;
        private System.Windows.Forms.Panel pnlOfflineList;
        private System.Windows.Forms.Panel pnlGroupChatHeader;
        private System.Windows.Forms.PictureBox picCreateGroup;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.PictureBox picGroupChatToggle;
        private System.Windows.Forms.Panel pnlGroupChatList;
        private System.Windows.Forms.PictureBox picSearch;
        private System.Windows.Forms.PictureBox picAddFriends;
    }
}