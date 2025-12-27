namespace ChatApp.Forms
{
    partial class QuanLyNhom
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(QuanLyNhom));
            this.pnlBackground = new Guna.UI2.WinForms.Guna2GradientPanel();
            this.pnlView = new Guna.UI2.WinForms.Guna2Panel();
            this.pnlMembers = new Guna.UI2.WinForms.Guna2Panel();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.btnHuy = new Guna.UI2.WinForms.Guna2Button();
            this.btnDoiAvatar = new Guna.UI2.WinForms.Guna2Button();
            this.btnDoiTenNhom = new Guna.UI2.WinForms.Guna2Button();
            this.btnTao = new Guna.UI2.WinForms.Guna2Button();
            this.pnlHeader = new Guna.UI2.WinForms.Guna2Panel();
            this.lblHint = new System.Windows.Forms.Label();
            this.txtTenNhom = new Guna.UI2.WinForms.Guna2TextBox();
            this.picAvatarPreview = new Guna.UI2.WinForms.Guna2CirclePictureBox();
            this.pnlBackground.SuspendLayout();
            this.pnlView.SuspendLayout();
            this.flowLayoutPanel1.SuspendLayout();
            this.pnlHeader.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picAvatarPreview)).BeginInit();
            this.SuspendLayout();
            // 
            // pnlBackground
            // 
            this.pnlBackground.Controls.Add(this.pnlView);
            this.pnlBackground.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlBackground.FillColor = System.Drawing.Color.White;
            this.pnlBackground.FillColor2 = System.Drawing.Color.White;
            this.pnlBackground.GradientMode = System.Drawing.Drawing2D.LinearGradientMode.Vertical;
            this.pnlBackground.Location = new System.Drawing.Point(0, 0);
            this.pnlBackground.Name = "pnlBackground";
            this.pnlBackground.Padding = new System.Windows.Forms.Padding(20);
            this.pnlBackground.Size = new System.Drawing.Size(668, 520);
            this.pnlBackground.TabIndex = 1;
            // 
            // pnlView
            // 
            this.pnlView.BackColor = System.Drawing.Color.Transparent;
            this.pnlView.BorderRadius = 14;
            this.pnlView.Controls.Add(this.pnlMembers);
            this.pnlView.Controls.Add(this.flowLayoutPanel1);
            this.pnlView.Controls.Add(this.pnlHeader);
            this.pnlView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlView.FillColor = System.Drawing.Color.White;
            this.pnlView.Location = new System.Drawing.Point(20, 20);
            this.pnlView.Name = "pnlView";
            this.pnlView.Padding = new System.Windows.Forms.Padding(12);
            this.pnlView.Size = new System.Drawing.Size(628, 480);
            this.pnlView.TabIndex = 0;
            // 
            // pnlMembers
            // 
            this.pnlMembers.AutoScroll = true;
            this.pnlMembers.BackColor = System.Drawing.Color.Transparent;
            this.pnlMembers.BorderRadius = 12;
            this.pnlMembers.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlMembers.FillColor = System.Drawing.Color.White;
            this.pnlMembers.Location = new System.Drawing.Point(12, 118);
            this.pnlMembers.Name = "pnlMembers";
            this.pnlMembers.Padding = new System.Windows.Forms.Padding(10);
            this.pnlMembers.Size = new System.Drawing.Size(604, 294);
            this.pnlMembers.TabIndex = 2;
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.Controls.Add(this.btnHuy);
            this.flowLayoutPanel1.Controls.Add(this.btnDoiAvatar);
            this.flowLayoutPanel1.Controls.Add(this.btnDoiTenNhom);
            this.flowLayoutPanel1.Controls.Add(this.btnTao);
            this.flowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.flowLayoutPanel1.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            this.flowLayoutPanel1.Location = new System.Drawing.Point(12, 412);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Padding = new System.Windows.Forms.Padding(0, 6, 0, 0);
            this.flowLayoutPanel1.Size = new System.Drawing.Size(604, 56);
            this.flowLayoutPanel1.TabIndex = 1;
            this.flowLayoutPanel1.WrapContents = false;
            // 
            // btnHuy
            // 
            this.btnHuy.BorderRadius = 12;
            this.btnHuy.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnHuy.DisabledState.BorderColor = System.Drawing.Color.DarkGray;
            this.btnHuy.DisabledState.CustomBorderColor = System.Drawing.Color.DarkGray;
            this.btnHuy.DisabledState.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(169)))), ((int)(((byte)(169)))), ((int)(((byte)(169)))));
            this.btnHuy.DisabledState.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(141)))), ((int)(((byte)(141)))), ((int)(((byte)(141)))));
            this.btnHuy.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold);
            this.btnHuy.ForeColor = System.Drawing.Color.White;
            this.btnHuy.Location = new System.Drawing.Point(488, 9);
            this.btnHuy.Margin = new System.Windows.Forms.Padding(10, 3, 3, 3);
            this.btnHuy.Name = "btnHuy";
            this.btnHuy.Size = new System.Drawing.Size(113, 42);
            this.btnHuy.TabIndex = 1;
            this.btnHuy.Text = "Đóng";
            // 
            // btnDoiAvatar
            // 
            this.btnDoiAvatar.BorderRadius = 12;
            this.btnDoiAvatar.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnDoiAvatar.DisabledState.BorderColor = System.Drawing.Color.DarkGray;
            this.btnDoiAvatar.DisabledState.CustomBorderColor = System.Drawing.Color.DarkGray;
            this.btnDoiAvatar.DisabledState.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(169)))), ((int)(((byte)(169)))), ((int)(((byte)(169)))));
            this.btnDoiAvatar.DisabledState.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(141)))), ((int)(((byte)(141)))), ((int)(((byte)(141)))));
            this.btnDoiAvatar.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold);
            this.btnDoiAvatar.ForeColor = System.Drawing.Color.White;
            this.btnDoiAvatar.Location = new System.Drawing.Point(347, 9);
            this.btnDoiAvatar.Margin = new System.Windows.Forms.Padding(10, 3, 3, 3);
            this.btnDoiAvatar.Name = "btnDoiAvatar";
            this.btnDoiAvatar.Size = new System.Drawing.Size(128, 42);
            this.btnDoiAvatar.TabIndex = 3;
            this.btnDoiAvatar.Text = "Đổi avatar";
            this.btnDoiAvatar.Click += new System.EventHandler(this.btnDoiAvatar_Click);
            // 
            // btnDoiTenNhom
            // 
            this.btnDoiTenNhom.BorderRadius = 12;
            this.btnDoiTenNhom.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnDoiTenNhom.DisabledState.BorderColor = System.Drawing.Color.DarkGray;
            this.btnDoiTenNhom.DisabledState.CustomBorderColor = System.Drawing.Color.DarkGray;
            this.btnDoiTenNhom.DisabledState.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(169)))), ((int)(((byte)(169)))), ((int)(((byte)(169)))));
            this.btnDoiTenNhom.DisabledState.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(141)))), ((int)(((byte)(141)))), ((int)(((byte)(141)))));
            this.btnDoiTenNhom.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold);
            this.btnDoiTenNhom.ForeColor = System.Drawing.Color.White;
            this.btnDoiTenNhom.Location = new System.Drawing.Point(198, 9);
            this.btnDoiTenNhom.Margin = new System.Windows.Forms.Padding(10, 3, 3, 3);
            this.btnDoiTenNhom.Name = "btnDoiTenNhom";
            this.btnDoiTenNhom.Size = new System.Drawing.Size(136, 42);
            this.btnDoiTenNhom.TabIndex = 2;
            this.btnDoiTenNhom.Text = "Đổi tên";
            this.btnDoiTenNhom.Click += new System.EventHandler(this.btnDoiTenNhom_Click);
            // 
            // btnTao
            // 
            this.btnTao.BorderRadius = 12;
            this.btnTao.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnTao.DisabledState.BorderColor = System.Drawing.Color.DarkGray;
            this.btnTao.DisabledState.CustomBorderColor = System.Drawing.Color.DarkGray;
            this.btnTao.DisabledState.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(169)))), ((int)(((byte)(169)))), ((int)(((byte)(169)))));
            this.btnTao.DisabledState.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(141)))), ((int)(((byte)(141)))), ((int)(((byte)(141)))));
            this.btnTao.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold);
            this.btnTao.ForeColor = System.Drawing.Color.White;
            this.btnTao.Location = new System.Drawing.Point(13, 9);
            this.btnTao.Margin = new System.Windows.Forms.Padding(10, 3, 3, 3);
            this.btnTao.Name = "btnTao";
            this.btnTao.Size = new System.Drawing.Size(172, 42);
            this.btnTao.TabIndex = 0;
            this.btnTao.Text = "Thêm thành viên";
            this.btnTao.Click += new System.EventHandler(this.btnTao_Click);
            // 
            // pnlHeader
            // 
            this.pnlHeader.BackColor = System.Drawing.Color.Transparent;
            this.pnlHeader.BorderRadius = 12;
            this.pnlHeader.Controls.Add(this.lblHint);
            this.pnlHeader.Controls.Add(this.txtTenNhom);
            this.pnlHeader.Controls.Add(this.picAvatarPreview);
            this.pnlHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlHeader.FillColor = System.Drawing.Color.White;
            this.pnlHeader.Location = new System.Drawing.Point(12, 12);
            this.pnlHeader.Name = "pnlHeader";
            this.pnlHeader.Padding = new System.Windows.Forms.Padding(10);
            this.pnlHeader.Size = new System.Drawing.Size(604, 106);
            this.pnlHeader.TabIndex = 3;
            // 
            // lblHint
            // 
            this.lblHint.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblHint.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblHint.ForeColor = System.Drawing.Color.DimGray;
            this.lblHint.Location = new System.Drawing.Point(96, 60);
            this.lblHint.Name = "lblHint";
            this.lblHint.Padding = new System.Windows.Forms.Padding(8, 6, 0, 0);
            this.lblHint.Size = new System.Drawing.Size(498, 36);
            this.lblHint.TabIndex = 2;
            this.lblHint.Text = "Tick người cần thêm • Có thể đổi tên/đổi avatar bên dưới";
            // 
            // txtTenNhom
            // 
            this.txtTenNhom.BorderRadius = 12;
            this.txtTenNhom.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.txtTenNhom.DefaultText = "";
            this.txtTenNhom.DisabledState.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(208)))), ((int)(((byte)(208)))), ((int)(((byte)(208)))));
            this.txtTenNhom.DisabledState.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(226)))), ((int)(((byte)(226)))), ((int)(((byte)(226)))));
            this.txtTenNhom.DisabledState.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(138)))), ((int)(((byte)(138)))), ((int)(((byte)(138)))));
            this.txtTenNhom.DisabledState.PlaceholderForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(138)))), ((int)(((byte)(138)))), ((int)(((byte)(138)))));
            this.txtTenNhom.Dock = System.Windows.Forms.DockStyle.Top;
            this.txtTenNhom.FocusedState.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(94)))), ((int)(((byte)(148)))), ((int)(((byte)(255)))));
            this.txtTenNhom.Font = new System.Drawing.Font("Segoe UI Semibold", 10.2F, System.Drawing.FontStyle.Bold);
            this.txtTenNhom.HoverState.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(94)))), ((int)(((byte)(148)))), ((int)(((byte)(255)))));
            this.txtTenNhom.Location = new System.Drawing.Point(96, 10);
            this.txtTenNhom.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.txtTenNhom.Name = "txtTenNhom";
            this.txtTenNhom.PlaceholderText = "Nhập tên nhóm...";
            this.txtTenNhom.SelectedText = "";
            this.txtTenNhom.Size = new System.Drawing.Size(498, 50);
            this.txtTenNhom.TabIndex = 1;
            // 
            // picAvatarPreview
            // 
            this.picAvatarPreview.BackColor = System.Drawing.Color.Transparent;
            this.picAvatarPreview.Dock = System.Windows.Forms.DockStyle.Left;
            this.picAvatarPreview.FillColor = System.Drawing.Color.DarkGray;
            this.picAvatarPreview.ImageRotate = 0F;
            this.picAvatarPreview.Location = new System.Drawing.Point(10, 10);
            this.picAvatarPreview.Name = "picAvatarPreview";
            this.picAvatarPreview.ShadowDecoration.Mode = Guna.UI2.WinForms.Enums.ShadowMode.Circle;
            this.picAvatarPreview.Size = new System.Drawing.Size(86, 86);
            this.picAvatarPreview.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.picAvatarPreview.TabIndex = 0;
            this.picAvatarPreview.TabStop = false;
            // 
            // QuanLyNhom
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(668, 520);
            this.Controls.Add(this.pnlBackground);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "QuanLyNhom";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "QUẢN LÝ NHÓM";
            this.pnlBackground.ResumeLayout(false);
            this.pnlView.ResumeLayout(false);
            this.flowLayoutPanel1.ResumeLayout(false);
            this.pnlHeader.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.picAvatarPreview)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private Guna.UI2.WinForms.Guna2GradientPanel pnlBackground;
        private Guna.UI2.WinForms.Guna2Panel pnlView;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private Guna.UI2.WinForms.Guna2Button btnTao;
        private Guna.UI2.WinForms.Guna2Button btnHuy;
        private Guna.UI2.WinForms.Guna2Panel pnlMembers;
        private Guna.UI2.WinForms.Guna2Button btnDoiTenNhom;
        private Guna.UI2.WinForms.Guna2Button btnDoiAvatar;

        private Guna.UI2.WinForms.Guna2Panel pnlHeader;
        private Guna.UI2.WinForms.Guna2CirclePictureBox picAvatarPreview;
        private Guna.UI2.WinForms.Guna2TextBox txtTenNhom;
        private System.Windows.Forms.Label lblHint;
    }
}
