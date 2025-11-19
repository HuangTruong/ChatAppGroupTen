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
            this.components = new System.ComponentModel.Container();
            this.pnlBackground = new Guna2GradientPanel();
            this.mainPanel = new Guna2Panel();
            this.tableLayout = new TableLayoutPanel();
            this.leftPanel = new Guna2Panel();
            this.flpSearch = new FlowLayoutPanel();
            this.txtSearch = new Guna2TextBox();
            this.lblSearchTitle = new Label();
            this.rightPanel = new Guna2Panel();
            this.flpMembers = new FlowLayoutPanel();
            this.lblMembersTitle = new Label();
            this.headerPanel = new Guna2Panel();
            this.lblGroupName = new Guna2HtmlLabel();
            this.headerSettingsFlow = new FlowLayoutPanel();
            this.chkAdminOnlyChat = new Guna2CheckBox();
            this.chkRequireApproval = new Guna2CheckBox();
            this.pnlBackground.SuspendLayout();
            this.mainPanel.SuspendLayout();
            this.tableLayout.SuspendLayout();
            this.leftPanel.SuspendLayout();
            this.rightPanel.SuspendLayout();
            this.headerPanel.SuspendLayout();
            this.headerSettingsFlow.SuspendLayout();
            this.SuspendLayout();
            // 
            // Form
            // 
            this.AutoScaleMode = AutoScaleMode.None;
            this.ClientSize = new System.Drawing.Size(900, 540);
            this.Name = "QuanLyThanhVienNhom";
            this.Text = "Quản lý thành viên nhóm";
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            // 
            // pnlBackground
            // 
            this.pnlBackground.Dock = DockStyle.Fill;
            this.pnlBackground.FillColor = System.Drawing.Color.FromArgb(116, 185, 255);
            this.pnlBackground.FillColor2 = System.Drawing.Color.FromArgb(9, 132, 227);
            this.pnlBackground.GradientMode = System.Drawing.Drawing2D.LinearGradientMode.Vertical;
            this.pnlBackground.Padding = new Padding(80, 60, 80, 60);
            this.pnlBackground.Controls.Add(this.mainPanel);
            // 
            // mainPanel (card trắng)
            // 
            this.mainPanel.Dock = DockStyle.Fill;
            this.mainPanel.BorderRadius = 24;
            this.mainPanel.FillColor = System.Drawing.Color.White;
            this.mainPanel.ShadowDecoration.Enabled = true;
            this.mainPanel.ShadowDecoration.Depth = 12;
            this.mainPanel.ShadowDecoration.BorderRadius = 24;
            this.mainPanel.ShadowDecoration.Shadow = new Padding(0, 0, 8, 8);
            this.mainPanel.Controls.Add(this.tableLayout);
            this.mainPanel.Controls.Add(this.headerPanel);
            // 
            // headerPanel
            // 
            this.headerPanel.Dock = DockStyle.Top;
            this.headerPanel.Height = 72;
            this.headerPanel.FillColor = System.Drawing.Color.White;
            this.headerPanel.CustomBorderColor = System.Drawing.Color.FromArgb(220, 230, 245);
            this.headerPanel.CustomBorderThickness = new Padding(0, 0, 0, 1);
            this.headerPanel.Padding = new Padding(20, 16, 20, 8);
            this.headerPanel.Controls.Add(this.headerSettingsFlow);
            this.headerPanel.Controls.Add(this.lblGroupName);
            // 
            // lblGroupName
            // 
            this.lblGroupName.BackColor = System.Drawing.Color.Transparent;
            this.lblGroupName.Text = "Nhóm";
            this.lblGroupName.Location = new System.Drawing.Point(20, 24);
            this.lblGroupName.Font = new System.Drawing.Font("Segoe UI Semibold", 11.5F,
                System.Drawing.FontStyle.Bold);
            // 
            // headerSettingsFlow (chứa 2 checkbox, dock phải)
            // 
            this.headerSettingsFlow.Dock = DockStyle.Right;
            this.headerSettingsFlow.AutoSize = true;
            this.headerSettingsFlow.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            this.headerSettingsFlow.FlowDirection = FlowDirection.LeftToRight;
            this.headerSettingsFlow.WrapContents = false;
            this.headerSettingsFlow.Padding = new Padding(0, 18, 0, 0);
            this.headerSettingsFlow.Controls.Add(this.chkAdminOnlyChat);
            this.headerSettingsFlow.Controls.Add(this.chkRequireApproval);
            // 
            // chkAdminOnlyChat
            // 
            this.chkAdminOnlyChat.Text = "Chỉ vàng & bạc được chat";
            this.chkAdminOnlyChat.AutoSize = true;
            this.chkAdminOnlyChat.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.chkAdminOnlyChat.CheckedState.FillColor = System.Drawing.Color.FromArgb(9, 132, 227);
            this.chkAdminOnlyChat.UncheckedState.FillColor = System.Drawing.Color.Silver;
            this.chkAdminOnlyChat.Margin = new Padding(0, 0, 18, 0);
            // 
            // chkRequireApproval
            // 
            this.chkRequireApproval.Text = "Bật phê duyệt thành viên";
            this.chkRequireApproval.AutoSize = true;
            this.chkRequireApproval.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.chkRequireApproval.CheckedState.FillColor = System.Drawing.Color.FromArgb(9, 132, 227);
            this.chkRequireApproval.UncheckedState.FillColor = System.Drawing.Color.Silver;
            // 
            // tableLayout (2 cột trái/phải)
            // 
            this.tableLayout.Dock = DockStyle.Fill;
            this.tableLayout.ColumnCount = 2;
            this.tableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            this.tableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            this.tableLayout.RowCount = 1;
            this.tableLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            this.tableLayout.Padding = new Padding(16, 12, 16, 16);
            this.tableLayout.Controls.Add(this.leftPanel, 0, 0);
            this.tableLayout.Controls.Add(this.rightPanel, 1, 0);
            // 
            // leftPanel
            // 
            this.leftPanel.Dock = DockStyle.Fill;
            this.leftPanel.BorderRadius = 16;
            this.leftPanel.FillColor = System.Drawing.Color.FromArgb(248, 250, 252);
            this.leftPanel.Padding = new Padding(14);
            this.leftPanel.ShadowDecoration.Enabled = true;
            this.leftPanel.ShadowDecoration.Depth = 4;
            this.leftPanel.ShadowDecoration.Shadow = new Padding(0, 0, 4, 4);
            this.leftPanel.Controls.Add(this.flpSearch);
            this.leftPanel.Controls.Add(this.txtSearch);
            this.leftPanel.Controls.Add(this.lblSearchTitle);
            // 
            // lblSearchTitle
            // 
            this.lblSearchTitle.Text = "Tìm người để thêm vào nhóm";
            this.lblSearchTitle.AutoSize = true;
            this.lblSearchTitle.Font = new System.Drawing.Font("Segoe UI Semibold", 9.5F,
                System.Drawing.FontStyle.Bold);
            this.lblSearchTitle.Dock = DockStyle.Top;
            this.lblSearchTitle.Margin = new Padding(0, 0, 0, 4);
            // 
            // txtSearch
            // 
            this.txtSearch.PlaceholderText = "Nhập tên người dùng...";
            this.txtSearch.BorderRadius = 10;
            this.txtSearch.Dock = DockStyle.Top;
            this.txtSearch.Margin = new Padding(0, 6, 0, 6);
            this.txtSearch.Height = 36;
            this.txtSearch.IconLeftOffset = new System.Drawing.Point(4, 0);
            // 
            // flpSearch
            // 
            this.flpSearch.Dock = DockStyle.Fill;
            this.flpSearch.AutoScroll = true;
            this.flpSearch.FlowDirection = FlowDirection.TopDown;
            this.flpSearch.WrapContents = false;
            this.flpSearch.Margin = new Padding(0, 8, 0, 0);
            // 
            // rightPanel
            // 
            this.rightPanel.Dock = DockStyle.Fill;
            this.rightPanel.BorderRadius = 16;
            this.rightPanel.FillColor = System.Drawing.Color.FromArgb(248, 250, 252);
            this.rightPanel.Padding = new Padding(14);
            this.rightPanel.ShadowDecoration.Enabled = true;
            this.rightPanel.ShadowDecoration.Depth = 4;
            this.rightPanel.ShadowDecoration.Shadow = new Padding(0, 0, 4, 4);
            this.rightPanel.Controls.Add(this.flpMembers);
            this.rightPanel.Controls.Add(this.lblMembersTitle);
            // 
            // lblMembersTitle
            // 
            this.lblMembersTitle.Text = "Thành viên hiện tại (chuột phải để quản lý)";
            this.lblMembersTitle.AutoSize = true;
            this.lblMembersTitle.Font = new System.Drawing.Font("Segoe UI Semibold", 9.5F,
                System.Drawing.FontStyle.Bold);
            this.lblMembersTitle.Dock = DockStyle.Top;
            this.lblMembersTitle.Margin = new Padding(0, 0, 0, 4);
            // 
            // flpMembers
            // 
            this.flpMembers.Dock = DockStyle.Fill;
            this.flpMembers.AutoScroll = true;
            this.flpMembers.FlowDirection = FlowDirection.TopDown;
            this.flpMembers.WrapContents = false;
            this.flpMembers.Margin = new Padding(0, 8, 0, 0);

            // ========= add root control =========
            this.Controls.Add(this.pnlBackground);

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
