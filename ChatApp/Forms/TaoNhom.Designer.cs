namespace ChatApp.Forms
{
    partial class TaoNhom
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
            this.pnlBackground = new Guna.UI2.WinForms.Guna2GradientPanel();
            this.pnlView = new Guna.UI2.WinForms.Guna2Panel();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.btnTao = new Guna.UI2.WinForms.Guna2Button();
            this.btnHuy = new Guna.UI2.WinForms.Guna2Button();
            this.txtTenNhom = new Guna.UI2.WinForms.Guna2TextBox();
            this.pnlMembers = new Guna.UI2.WinForms.Guna2Panel();
            this.pnlBackground.SuspendLayout();
            this.pnlView.SuspendLayout();
            this.flowLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // pnlBackground
            // 
            this.pnlBackground.Controls.Add(this.pnlView);
            this.pnlBackground.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlBackground.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(116)))), ((int)(((byte)(185)))), ((int)(((byte)(255)))));
            this.pnlBackground.FillColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(9)))), ((int)(((byte)(132)))), ((int)(((byte)(227)))));
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
            this.pnlView.BorderRadius = 10;
            this.pnlView.Controls.Add(this.pnlMembers);
            this.pnlView.Controls.Add(this.flowLayoutPanel1);
            this.pnlView.Controls.Add(this.txtTenNhom);
            this.pnlView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlView.FillColor = System.Drawing.Color.White;
            this.pnlView.Location = new System.Drawing.Point(20, 20);
            this.pnlView.Name = "pnlView";
            this.pnlView.Padding = new System.Windows.Forms.Padding(10);
            this.pnlView.Size = new System.Drawing.Size(628, 480);
            this.pnlView.TabIndex = 0;
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.Controls.Add(this.btnTao);
            this.flowLayoutPanel1.Controls.Add(this.btnHuy);
            this.flowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.flowLayoutPanel1.Location = new System.Drawing.Point(10, 422);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(608, 48);
            this.flowLayoutPanel1.TabIndex = 1;
            // 
            // btnTao
            // 
            this.btnTao.BorderRadius = 10;
            this.btnTao.DisabledState.BorderColor = System.Drawing.Color.DarkGray;
            this.btnTao.DisabledState.CustomBorderColor = System.Drawing.Color.DarkGray;
            this.btnTao.DisabledState.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(169)))), ((int)(((byte)(169)))), ((int)(((byte)(169)))));
            this.btnTao.DisabledState.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(141)))), ((int)(((byte)(141)))), ((int)(((byte)(141)))));
            this.btnTao.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnTao.ForeColor = System.Drawing.Color.White;
            this.btnTao.Location = new System.Drawing.Point(3, 3);
            this.btnTao.Name = "btnTao";
            this.btnTao.Size = new System.Drawing.Size(95, 45);
            this.btnTao.TabIndex = 0;
            this.btnTao.Text = "Tạo";
            this.btnTao.Click += new System.EventHandler(this.btnTao_Click);
            // 
            // btnHuy
            // 
            this.btnHuy.BorderRadius = 10;
            this.btnHuy.DisabledState.BorderColor = System.Drawing.Color.DarkGray;
            this.btnHuy.DisabledState.CustomBorderColor = System.Drawing.Color.DarkGray;
            this.btnHuy.DisabledState.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(169)))), ((int)(((byte)(169)))), ((int)(((byte)(169)))));
            this.btnHuy.DisabledState.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(141)))), ((int)(((byte)(141)))), ((int)(((byte)(141)))));
            this.btnHuy.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnHuy.ForeColor = System.Drawing.Color.White;
            this.btnHuy.Location = new System.Drawing.Point(104, 3);
            this.btnHuy.Name = "btnHuy";
            this.btnHuy.Size = new System.Drawing.Size(95, 45);
            this.btnHuy.TabIndex = 1;
            this.btnHuy.Text = "Huỷ";
            this.btnHuy.Click += new System.EventHandler(this.btnHuy_Click);
            // 
            // txtTenNhom
            // 
            this.txtTenNhom.BorderRadius = 10;
            this.txtTenNhom.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.txtTenNhom.DefaultText = "";
            this.txtTenNhom.DisabledState.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(208)))), ((int)(((byte)(208)))), ((int)(((byte)(208)))));
            this.txtTenNhom.DisabledState.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(226)))), ((int)(((byte)(226)))), ((int)(((byte)(226)))));
            this.txtTenNhom.DisabledState.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(138)))), ((int)(((byte)(138)))), ((int)(((byte)(138)))));
            this.txtTenNhom.DisabledState.PlaceholderForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(138)))), ((int)(((byte)(138)))), ((int)(((byte)(138)))));
            this.txtTenNhom.Dock = System.Windows.Forms.DockStyle.Top;
            this.txtTenNhom.FocusedState.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(94)))), ((int)(((byte)(148)))), ((int)(((byte)(255)))));
            this.txtTenNhom.Font = new System.Drawing.Font("Segoe UI Semibold", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtTenNhom.HoverState.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(94)))), ((int)(((byte)(148)))), ((int)(((byte)(255)))));
            this.txtTenNhom.Location = new System.Drawing.Point(10, 10);
            this.txtTenNhom.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.txtTenNhom.Name = "txtTenNhom";
            this.txtTenNhom.PlaceholderText = "Nhập Tên Nhóm";
            this.txtTenNhom.SelectedText = "";
            this.txtTenNhom.Size = new System.Drawing.Size(608, 48);
            this.txtTenNhom.TabIndex = 0;
            // 
            // pnlMembers
            // 
            this.pnlMembers.AutoScroll = true;
            this.pnlMembers.BackColor = System.Drawing.Color.Transparent;
            this.pnlMembers.BorderRadius = 10;
            this.pnlMembers.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlMembers.FillColor = System.Drawing.Color.White;
            this.pnlMembers.Location = new System.Drawing.Point(10, 58);
            this.pnlMembers.Name = "pnlMembers";
            this.pnlMembers.Padding = new System.Windows.Forms.Padding(10);
            this.pnlMembers.Size = new System.Drawing.Size(608, 364);
            this.pnlMembers.TabIndex = 2;
            // 
            // TaoNhom
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(668, 520);
            this.Controls.Add(this.pnlBackground);
            this.Name = "TaoNhom";
            this.Text = "TaoNhom";
            this.pnlBackground.ResumeLayout(false);
            this.pnlView.ResumeLayout(false);
            this.flowLayoutPanel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private Guna.UI2.WinForms.Guna2GradientPanel pnlBackground;
        private Guna.UI2.WinForms.Guna2Panel pnlView;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private Guna.UI2.WinForms.Guna2Button btnTao;
        private Guna.UI2.WinForms.Guna2TextBox txtTenNhom;
        private Guna.UI2.WinForms.Guna2Button btnHuy;
        private Guna.UI2.WinForms.Guna2Panel pnlMembers;
    }
}