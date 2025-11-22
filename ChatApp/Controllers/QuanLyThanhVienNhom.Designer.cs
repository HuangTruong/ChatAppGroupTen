using System.Windows.Forms;
using Guna.UI2.WinForms;

namespace ChatApp.Controllers
{
    partial class QuanLyThanhVienNhom
    {
        private System.ComponentModel.IContainer components = null;

        private Guna2GradientPanel pnlBackground;
        private Guna2Panel mainPanel;
        private Guna2Panel headerPanel;
        private Guna2HtmlLabel lblGroupName;
        private FlowLayoutPanel headerSettingsFlow;
        private Guna2CheckBox chkAdminOnlyChat;
        private Guna2CheckBox chkRequireApproval;

        private TableLayoutPanel tableLayout;
        private Guna2Panel leftPanel;
        private Label lblSearchTitle;
        private Guna2TextBox txtSearch;
        private FlowLayoutPanel flpSearch;

        private Guna2Panel rightPanel;
        private Label lblMembersTitle;
        private FlowLayoutPanel flpMembers;

        /// <summary>
        ///  Required designer method.
        /// </summary>
        private void InitializeComponent()
        {
            this.pnlBackground = new Guna.UI2.WinForms.Guna2GradientPanel();
            this.mainPanel = new Guna.UI2.WinForms.Guna2Panel();
            this.tableLayout = new System.Windows.Forms.TableLayoutPanel();
            this.leftPanel = new Guna.UI2.WinForms.Guna2Panel();
            this.flpSearch = new System.Windows.Forms.FlowLayoutPanel();
            this.txtSearch = new Guna.UI2.WinForms.Guna2TextBox();
            this.lblSearchTitle = new System.Windows.Forms.Label();
            this.rightPanel = new Guna.UI2.WinForms.Guna2Panel();
            this.flpMembers = new System.Windows.Forms.FlowLayoutPanel();
            this.lblMembersTitle = new System.Windows.Forms.Label();
            this.headerPanel = new Guna.UI2.WinForms.Guna2Panel();
            this.headerSettingsFlow = new System.Windows.Forms.FlowLayoutPanel();
            this.chkAdminOnlyChat = new Guna.UI2.WinForms.Guna2CheckBox();
            this.chkRequireApproval = new Guna.UI2.WinForms.Guna2CheckBox();
            this.lblGroupName = new Guna.UI2.WinForms.Guna2HtmlLabel();
            this.pnlBackground.SuspendLayout();
            this.mainPanel.SuspendLayout();
            this.tableLayout.SuspendLayout();
            this.leftPanel.SuspendLayout();
            this.rightPanel.SuspendLayout();
            this.headerPanel.SuspendLayout();
            this.headerSettingsFlow.SuspendLayout();
            this.SuspendLayout();
            // 
            // pnlBackground
            // 
            this.pnlBackground.Controls.Add(this.mainPanel);
            this.pnlBackground.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlBackground.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(116)))), ((int)(((byte)(185)))), ((int)(((byte)(255)))));
            this.pnlBackground.FillColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(9)))), ((int)(((byte)(132)))), ((int)(((byte)(227)))));
            this.pnlBackground.GradientMode = System.Drawing.Drawing2D.LinearGradientMode.Vertical;
            this.pnlBackground.Location = new System.Drawing.Point(0, 0);
            this.pnlBackground.Name = "pnlBackground";
            this.pnlBackground.Padding = new System.Windows.Forms.Padding(80, 60, 80, 60);
            this.pnlBackground.Size = new System.Drawing.Size(900, 540);
            this.pnlBackground.TabIndex = 0;
            // 
            // mainPanel
            // 
            this.mainPanel.BackColor = System.Drawing.Color.Transparent;
            this.mainPanel.BorderRadius = 24;
            this.mainPanel.Controls.Add(this.tableLayout);
            this.mainPanel.Controls.Add(this.headerPanel);
            this.mainPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.mainPanel.FillColor = System.Drawing.Color.White;
            this.mainPanel.Location = new System.Drawing.Point(80, 60);
            this.mainPanel.Name = "mainPanel";
            this.mainPanel.ShadowDecoration.BorderRadius = 24;
            this.mainPanel.ShadowDecoration.Depth = 12;
            this.mainPanel.ShadowDecoration.Enabled = true;
            this.mainPanel.ShadowDecoration.Shadow = new System.Windows.Forms.Padding(0, 0, 8, 8);
            this.mainPanel.Size = new System.Drawing.Size(740, 420);
            this.mainPanel.TabIndex = 0;
            // 
            // tableLayout
            // 
            this.tableLayout.ColumnCount = 2;
            this.tableLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayout.Controls.Add(this.leftPanel, 0, 0);
            this.tableLayout.Controls.Add(this.rightPanel, 1, 0);
            this.tableLayout.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayout.Location = new System.Drawing.Point(0, 72);
            this.tableLayout.Name = "tableLayout";
            this.tableLayout.Padding = new System.Windows.Forms.Padding(16, 12, 16, 16);
            this.tableLayout.RowCount = 1;
            this.tableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayout.Size = new System.Drawing.Size(740, 348);
            this.tableLayout.TabIndex = 0;
            // 
            // leftPanel
            // 
            this.leftPanel.BackColor = System.Drawing.Color.Transparent;
            this.leftPanel.BorderRadius = 16;
            this.leftPanel.Controls.Add(this.flpSearch);
            this.leftPanel.Controls.Add(this.txtSearch);
            this.leftPanel.Controls.Add(this.lblSearchTitle);
            this.leftPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.leftPanel.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(248)))), ((int)(((byte)(250)))), ((int)(((byte)(252)))));
            this.leftPanel.Location = new System.Drawing.Point(19, 15);
            this.leftPanel.Name = "leftPanel";
            this.leftPanel.Padding = new System.Windows.Forms.Padding(14);
            this.leftPanel.ShadowDecoration.Depth = 4;
            this.leftPanel.ShadowDecoration.Enabled = true;
            this.leftPanel.ShadowDecoration.Shadow = new System.Windows.Forms.Padding(0, 0, 4, 4);
            this.leftPanel.Size = new System.Drawing.Size(348, 314);
            this.leftPanel.TabIndex = 0;
            // 
            // flpSearch
            // 
            this.flpSearch.AutoScroll = true;
            this.flpSearch.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flpSearch.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.flpSearch.Location = new System.Drawing.Point(14, 71);
            this.flpSearch.Margin = new System.Windows.Forms.Padding(0, 8, 0, 0);
            this.flpSearch.Name = "flpSearch";
            this.flpSearch.Size = new System.Drawing.Size(320, 229);
            this.flpSearch.TabIndex = 0;
            this.flpSearch.WrapContents = false;
            // 
            // txtSearch
            // 
            this.txtSearch.BorderRadius = 10;
            this.txtSearch.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.txtSearch.DefaultText = "";
            this.txtSearch.Dock = System.Windows.Forms.DockStyle.Top;
            this.txtSearch.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.txtSearch.IconLeftOffset = new System.Drawing.Point(4, 0);
            this.txtSearch.Location = new System.Drawing.Point(14, 35);
            this.txtSearch.Margin = new System.Windows.Forms.Padding(0, 6, 0, 6);
            this.txtSearch.Name = "txtSearch";
            this.txtSearch.PlaceholderText = "Nhập tên người dùng...";
            this.txtSearch.SelectedText = "";
            this.txtSearch.Size = new System.Drawing.Size(320, 36);
            this.txtSearch.TabIndex = 1;
            // 
            // lblSearchTitle
            // 
            this.lblSearchTitle.AutoSize = true;
            this.lblSearchTitle.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblSearchTitle.Font = new System.Drawing.Font("Segoe UI Semibold", 9.5F, System.Drawing.FontStyle.Bold);
            this.lblSearchTitle.Location = new System.Drawing.Point(14, 14);
            this.lblSearchTitle.Margin = new System.Windows.Forms.Padding(0, 0, 0, 4);
            this.lblSearchTitle.Name = "lblSearchTitle";
            this.lblSearchTitle.Size = new System.Drawing.Size(225, 21);
            this.lblSearchTitle.TabIndex = 2;
            this.lblSearchTitle.Text = "Tìm người để thêm vào nhóm";
            // 
            // rightPanel
            // 
            this.rightPanel.BackColor = System.Drawing.Color.Transparent;
            this.rightPanel.BorderRadius = 16;
            this.rightPanel.Controls.Add(this.flpMembers);
            this.rightPanel.Controls.Add(this.lblMembersTitle);
            this.rightPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rightPanel.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(248)))), ((int)(((byte)(250)))), ((int)(((byte)(252)))));
            this.rightPanel.Location = new System.Drawing.Point(373, 15);
            this.rightPanel.Name = "rightPanel";
            this.rightPanel.Padding = new System.Windows.Forms.Padding(14);
            this.rightPanel.ShadowDecoration.Depth = 4;
            this.rightPanel.ShadowDecoration.Enabled = true;
            this.rightPanel.ShadowDecoration.Shadow = new System.Windows.Forms.Padding(0, 0, 4, 4);
            this.rightPanel.Size = new System.Drawing.Size(348, 314);
            this.rightPanel.TabIndex = 1;
            // 
            // flpMembers
            // 
            this.flpMembers.AutoScroll = true;
            this.flpMembers.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flpMembers.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.flpMembers.Location = new System.Drawing.Point(14, 35);
            this.flpMembers.Margin = new System.Windows.Forms.Padding(0, 8, 0, 0);
            this.flpMembers.Name = "flpMembers";
            this.flpMembers.Size = new System.Drawing.Size(320, 265);
            this.flpMembers.TabIndex = 0;
            this.flpMembers.WrapContents = false;
            // 
            // lblMembersTitle
            // 
            this.lblMembersTitle.AutoSize = true;
            this.lblMembersTitle.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblMembersTitle.Font = new System.Drawing.Font("Segoe UI Semibold", 9.5F, System.Drawing.FontStyle.Bold);
            this.lblMembersTitle.Location = new System.Drawing.Point(14, 14);
            this.lblMembersTitle.Margin = new System.Windows.Forms.Padding(0, 0, 0, 4);
            this.lblMembersTitle.Name = "lblMembersTitle";
            this.lblMembersTitle.Size = new System.Drawing.Size(315, 21);
            this.lblMembersTitle.TabIndex = 1;
            this.lblMembersTitle.Text = "Thành viên hiện tại (chuột phải để quản lý)";
            // 
            // headerPanel
            // 
            this.headerPanel.Controls.Add(this.headerSettingsFlow);
            this.headerPanel.Controls.Add(this.lblGroupName);
            this.headerPanel.CustomBorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(230)))), ((int)(((byte)(245)))));
            this.headerPanel.CustomBorderThickness = new System.Windows.Forms.Padding(0, 0, 0, 1);
            this.headerPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.headerPanel.FillColor = System.Drawing.Color.White;
            this.headerPanel.Location = new System.Drawing.Point(0, 0);
            this.headerPanel.Name = "headerPanel";
            this.headerPanel.Padding = new System.Windows.Forms.Padding(20, 16, 20, 8);
            this.headerPanel.Size = new System.Drawing.Size(740, 72);
            this.headerPanel.TabIndex = 1;
            // 
            // headerSettingsFlow
            // 
            this.headerSettingsFlow.AutoSize = true;
            this.headerSettingsFlow.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.headerSettingsFlow.Controls.Add(this.chkAdminOnlyChat);
            this.headerSettingsFlow.Controls.Add(this.chkRequireApproval);
            this.headerSettingsFlow.Dock = System.Windows.Forms.DockStyle.Right;
            this.headerSettingsFlow.Location = new System.Drawing.Point(311, 16);
            this.headerSettingsFlow.Name = "headerSettingsFlow";
            this.headerSettingsFlow.Padding = new System.Windows.Forms.Padding(0, 18, 0, 0);
            this.headerSettingsFlow.Size = new System.Drawing.Size(409, 48);
            this.headerSettingsFlow.TabIndex = 0;
            this.headerSettingsFlow.WrapContents = false;
            // 
            // chkAdminOnlyChat
            // 
            this.chkAdminOnlyChat.AutoSize = true;
            this.chkAdminOnlyChat.CheckedState.BorderRadius = 0;
            this.chkAdminOnlyChat.CheckedState.BorderThickness = 0;
            this.chkAdminOnlyChat.CheckedState.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(9)))), ((int)(((byte)(132)))), ((int)(((byte)(227)))));
            this.chkAdminOnlyChat.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.chkAdminOnlyChat.Location = new System.Drawing.Point(0, 18);
            this.chkAdminOnlyChat.Margin = new System.Windows.Forms.Padding(0, 0, 18, 0);
            this.chkAdminOnlyChat.Name = "chkAdminOnlyChat";
            this.chkAdminOnlyChat.Size = new System.Drawing.Size(190, 24);
            this.chkAdminOnlyChat.TabIndex = 0;
            this.chkAdminOnlyChat.Text = "Chỉ vàng & bạc được chat";
            this.chkAdminOnlyChat.UncheckedState.BorderRadius = 0;
            this.chkAdminOnlyChat.UncheckedState.BorderThickness = 0;
            this.chkAdminOnlyChat.UncheckedState.FillColor = System.Drawing.Color.Silver;
            // 
            // chkRequireApproval
            // 
            this.chkRequireApproval.AutoSize = true;
            this.chkRequireApproval.CheckedState.BorderRadius = 0;
            this.chkRequireApproval.CheckedState.BorderThickness = 0;
            this.chkRequireApproval.CheckedState.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(9)))), ((int)(((byte)(132)))), ((int)(((byte)(227)))));
            this.chkRequireApproval.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.chkRequireApproval.Location = new System.Drawing.Point(211, 21);
            this.chkRequireApproval.Name = "chkRequireApproval";
            this.chkRequireApproval.Size = new System.Drawing.Size(195, 24);
            this.chkRequireApproval.TabIndex = 1;
            this.chkRequireApproval.Text = "Bật phê duyệt thành viên";
            this.chkRequireApproval.UncheckedState.BorderRadius = 0;
            this.chkRequireApproval.UncheckedState.BorderThickness = 0;
            this.chkRequireApproval.UncheckedState.FillColor = System.Drawing.Color.Silver;
            // 
            // lblGroupName
            // 
            this.lblGroupName.BackColor = System.Drawing.Color.Transparent;
            this.lblGroupName.Font = new System.Drawing.Font("Segoe UI Semibold", 11.5F, System.Drawing.FontStyle.Bold);
            this.lblGroupName.Location = new System.Drawing.Point(20, 24);
            this.lblGroupName.Name = "lblGroupName";
            this.lblGroupName.Size = new System.Drawing.Size(57, 27);
            this.lblGroupName.TabIndex = 1;
            this.lblGroupName.Text = "Nhóm";
            // 
            // QuanLyThanhVienNhom
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.ClientSize = new System.Drawing.Size(900, 540);
            this.Controls.Add(this.pnlBackground);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Name = "QuanLyThanhVienNhom";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Quản lý thành viên nhóm";
            this.pnlBackground.ResumeLayout(false);
            this.mainPanel.ResumeLayout(false);
            this.tableLayout.ResumeLayout(false);
            this.leftPanel.ResumeLayout(false);
            this.leftPanel.PerformLayout();
            this.rightPanel.ResumeLayout(false);
            this.rightPanel.PerformLayout();
            this.headerPanel.ResumeLayout(false);
            this.headerPanel.PerformLayout();
            this.headerSettingsFlow.ResumeLayout(false);
            this.headerSettingsFlow.PerformLayout();
            this.ResumeLayout(false);

        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
