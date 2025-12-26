namespace ChatApp
{
    partial class DangKy
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
            this.btnBack = new Guna.UI2.WinForms.Guna2Button();
            this.btnRegister = new Guna.UI2.WinForms.Guna2Button();
            this.dtpBirthday = new Guna.UI2.WinForms.Guna2DateTimePicker();
            this.cbbGender = new Guna.UI2.WinForms.Guna2ComboBox();
            this.lblTittle = new System.Windows.Forms.Label();
            this.pnlLogin = new Guna.UI2.WinForms.Guna2Panel();
            this.txtDisplayName = new Guna.UI2.WinForms.Guna2TextBox();
            this.txtConfirmPassword = new Guna.UI2.WinForms.Guna2TextBox();
            this.txtEmail = new Guna.UI2.WinForms.Guna2TextBox();
            this.txtUserName = new Guna.UI2.WinForms.Guna2TextBox();
            this.txtPassword = new Guna.UI2.WinForms.Guna2TextBox();
            this.pnlBackground = new Guna.UI2.WinForms.Guna2GradientPanel();
            this.pnlLogin.SuspendLayout();
            this.pnlBackground.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnBack
            // 
            this.btnBack.BackColor = System.Drawing.Color.Transparent;
            this.btnBack.BorderRadius = 10;
            this.btnBack.DisabledState.BorderColor = System.Drawing.Color.DarkGray;
            this.btnBack.DisabledState.CustomBorderColor = System.Drawing.Color.DarkGray;
            this.btnBack.DisabledState.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(169)))), ((int)(((byte)(169)))), ((int)(((byte)(169)))));
            this.btnBack.DisabledState.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(141)))), ((int)(((byte)(141)))), ((int)(((byte)(141)))));
            this.btnBack.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(52)))), ((int)(((byte)(54)))));
            this.btnBack.Font = new System.Drawing.Font("Segoe UI Semibold", 10.2F, System.Drawing.FontStyle.Bold);
            this.btnBack.ForeColor = System.Drawing.Color.White;
            this.btnBack.HoverState.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(99)))), ((int)(((byte)(110)))), ((int)(((byte)(114)))));
            this.btnBack.HoverState.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(99)))), ((int)(((byte)(110)))), ((int)(((byte)(114)))));
            this.btnBack.Location = new System.Drawing.Point(204, 357);
            this.btnBack.Name = "btnBack";
            this.btnBack.ShadowDecoration.BorderRadius = 10;
            this.btnBack.ShadowDecoration.Color = System.Drawing.Color.FromArgb(((int)(((byte)(176)))), ((int)(((byte)(190)))), ((int)(((byte)(197)))));
            this.btnBack.ShadowDecoration.Depth = 10;
            this.btnBack.ShadowDecoration.Enabled = true;
            this.btnBack.Size = new System.Drawing.Size(150, 45);
            this.btnBack.TabIndex = 20;
            this.btnBack.Text = "Quay Lại";
            this.btnBack.Click += new System.EventHandler(this.btnQuayLaiDangNhap_Click);
            // 
            // btnRegister
            // 
            this.btnRegister.BackColor = System.Drawing.Color.Transparent;
            this.btnRegister.BorderRadius = 10;
            this.btnRegister.DisabledState.BorderColor = System.Drawing.Color.DarkGray;
            this.btnRegister.DisabledState.CustomBorderColor = System.Drawing.Color.DarkGray;
            this.btnRegister.DisabledState.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(169)))), ((int)(((byte)(169)))), ((int)(((byte)(169)))));
            this.btnRegister.DisabledState.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(141)))), ((int)(((byte)(141)))), ((int)(((byte)(141)))));
            this.btnRegister.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(9)))), ((int)(((byte)(132)))), ((int)(((byte)(227)))));
            this.btnRegister.Font = new System.Drawing.Font("Segoe UI Semibold", 10.2F, System.Drawing.FontStyle.Bold);
            this.btnRegister.ForeColor = System.Drawing.Color.White;
            this.btnRegister.HoverState.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(116)))), ((int)(((byte)(185)))), ((int)(((byte)(255)))));
            this.btnRegister.Location = new System.Drawing.Point(48, 357);
            this.btnRegister.Name = "btnRegister";
            this.btnRegister.ShadowDecoration.BorderRadius = 10;
            this.btnRegister.ShadowDecoration.Color = System.Drawing.Color.FromArgb(((int)(((byte)(176)))), ((int)(((byte)(190)))), ((int)(((byte)(197)))));
            this.btnRegister.ShadowDecoration.Depth = 10;
            this.btnRegister.ShadowDecoration.Enabled = true;
            this.btnRegister.Size = new System.Drawing.Size(150, 45);
            this.btnRegister.TabIndex = 19;
            this.btnRegister.Text = "Đăng Ký";
            this.btnRegister.Click += new System.EventHandler(this.btnDangKy_Click);
            // 
            // dtpBirthday
            // 
            this.dtpBirthday.BackColor = System.Drawing.Color.Transparent;
            this.dtpBirthday.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(218)))), ((int)(((byte)(220)))), ((int)(((byte)(224)))));
            this.dtpBirthday.BorderRadius = 15;
            this.dtpBirthday.Checked = true;
            this.dtpBirthday.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(245)))), ((int)(((byte)(248)))), ((int)(((byte)(255)))));
            this.dtpBirthday.FocusedColor = System.Drawing.Color.FromArgb(((int)(((byte)(33)))), ((int)(((byte)(147)))), ((int)(((byte)(176)))));
            this.dtpBirthday.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.dtpBirthday.ForeColor = System.Drawing.Color.Black;
            this.dtpBirthday.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            this.dtpBirthday.HoverState.BorderColor = System.Drawing.Color.Transparent;
            this.dtpBirthday.HoverState.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(235)))), ((int)(((byte)(240)))), ((int)(((byte)(255)))));
            this.dtpBirthday.Location = new System.Drawing.Point(65, 316);
            this.dtpBirthday.MaxDate = new System.DateTime(9998, 12, 31, 0, 0, 0, 0);
            this.dtpBirthday.MinDate = new System.DateTime(1753, 1, 1, 0, 0, 0, 0);
            this.dtpBirthday.Name = "dtpBirthday";
            this.dtpBirthday.Size = new System.Drawing.Size(270, 35);
            this.dtpBirthday.TabIndex = 18;
            this.dtpBirthday.Value = new System.DateTime(2025, 10, 9, 13, 41, 15, 896);
            // 
            // cbbGender
            // 
            this.cbbGender.BackColor = System.Drawing.Color.Transparent;
            this.cbbGender.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(218)))), ((int)(((byte)(220)))), ((int)(((byte)(224)))));
            this.cbbGender.BorderRadius = 15;
            this.cbbGender.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.cbbGender.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbbGender.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(245)))), ((int)(((byte)(248)))), ((int)(((byte)(255)))));
            this.cbbGender.FocusedColor = System.Drawing.Color.FromArgb(((int)(((byte)(33)))), ((int)(((byte)(147)))), ((int)(((byte)(176)))));
            this.cbbGender.FocusedState.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(33)))), ((int)(((byte)(147)))), ((int)(((byte)(176)))));
            this.cbbGender.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.cbbGender.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(68)))), ((int)(((byte)(88)))), ((int)(((byte)(112)))));
            this.cbbGender.HoverState.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(235)))), ((int)(((byte)(240)))), ((int)(((byte)(255)))));
            this.cbbGender.ItemHeight = 30;
            this.cbbGender.Items.AddRange(new object[] {
            "Giới Tính",
            "Nam",
            "Nữ",
            "Khác"});
            this.cbbGender.Location = new System.Drawing.Point(65, 269);
            this.cbbGender.Name = "cbbGender";
            this.cbbGender.Size = new System.Drawing.Size(270, 36);
            this.cbbGender.StartIndex = 0;
            this.cbbGender.TabIndex = 17;
            // 
            // lblTittle
            // 
            this.lblTittle.AutoSize = true;
            this.lblTittle.Font = new System.Drawing.Font("Segoe UI", 13.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTittle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(52)))), ((int)(((byte)(54)))));
            this.lblTittle.Location = new System.Drawing.Point(82, 18);
            this.lblTittle.Name = "lblTittle";
            this.lblTittle.Size = new System.Drawing.Size(240, 31);
            this.lblTittle.TabIndex = 7;
            this.lblTittle.Text = "ĐĂNG KÝ HỆ THỐNG";
            this.lblTittle.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // pnlLogin
            // 
            this.pnlLogin.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.pnlLogin.BackColor = System.Drawing.Color.Transparent;
            this.pnlLogin.BorderRadius = 20;
            this.pnlLogin.Controls.Add(this.dtpBirthday);
            this.pnlLogin.Controls.Add(this.cbbGender);
            this.pnlLogin.Controls.Add(this.txtDisplayName);
            this.pnlLogin.Controls.Add(this.txtConfirmPassword);
            this.pnlLogin.Controls.Add(this.txtEmail);
            this.pnlLogin.Controls.Add(this.txtUserName);
            this.pnlLogin.Controls.Add(this.btnBack);
            this.pnlLogin.Controls.Add(this.txtPassword);
            this.pnlLogin.Controls.Add(this.btnRegister);
            this.pnlLogin.Controls.Add(this.lblTittle);
            this.pnlLogin.FillColor = System.Drawing.Color.White;
            this.pnlLogin.Location = new System.Drawing.Point(192, 21);
            this.pnlLogin.Name = "pnlLogin";
            this.pnlLogin.ShadowDecoration.BorderRadius = 20;
            this.pnlLogin.ShadowDecoration.Color = System.Drawing.Color.FromArgb(((int)(((byte)(176)))), ((int)(((byte)(190)))), ((int)(((byte)(197)))));
            this.pnlLogin.ShadowDecoration.Depth = 20;
            this.pnlLogin.ShadowDecoration.Enabled = true;
            this.pnlLogin.Size = new System.Drawing.Size(400, 420);
            this.pnlLogin.TabIndex = 21;
            // 
            // txtDisplayName
            // 
            this.txtDisplayName.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(218)))), ((int)(((byte)(220)))), ((int)(((byte)(224)))));
            this.txtDisplayName.BorderRadius = 15;
            this.txtDisplayName.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.txtDisplayName.DefaultText = "";
            this.txtDisplayName.DisabledState.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(208)))), ((int)(((byte)(208)))), ((int)(((byte)(208)))));
            this.txtDisplayName.DisabledState.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(226)))), ((int)(((byte)(226)))), ((int)(((byte)(226)))));
            this.txtDisplayName.DisabledState.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(138)))), ((int)(((byte)(138)))), ((int)(((byte)(138)))));
            this.txtDisplayName.DisabledState.PlaceholderForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(138)))), ((int)(((byte)(138)))), ((int)(((byte)(138)))));
            this.txtDisplayName.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(245)))), ((int)(((byte)(248)))), ((int)(((byte)(255)))));
            this.txtDisplayName.FocusedState.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(33)))), ((int)(((byte)(147)))), ((int)(((byte)(176)))));
            this.txtDisplayName.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.txtDisplayName.HoverState.BorderColor = System.Drawing.Color.Transparent;
            this.txtDisplayName.HoverState.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(235)))), ((int)(((byte)(240)))), ((int)(((byte)(255)))));
            this.txtDisplayName.IconLeft = global::ChatApp.Properties.Resources.HoTen;
            this.txtDisplayName.Location = new System.Drawing.Point(65, 101);
            this.txtDisplayName.Margin = new System.Windows.Forms.Padding(3, 5, 3, 5);
            this.txtDisplayName.Name = "txtDisplayName";
            this.txtDisplayName.PlaceholderText = "Họ Tên";
            this.txtDisplayName.SelectedText = "";
            this.txtDisplayName.Size = new System.Drawing.Size(270, 40);
            this.txtDisplayName.TabIndex = 10;
            // 
            // txtConfirmPassword
            // 
            this.txtConfirmPassword.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(218)))), ((int)(((byte)(220)))), ((int)(((byte)(224)))));
            this.txtConfirmPassword.BorderRadius = 15;
            this.txtConfirmPassword.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.txtConfirmPassword.DefaultText = "";
            this.txtConfirmPassword.DisabledState.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(208)))), ((int)(((byte)(208)))), ((int)(((byte)(208)))));
            this.txtConfirmPassword.DisabledState.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(226)))), ((int)(((byte)(226)))), ((int)(((byte)(226)))));
            this.txtConfirmPassword.DisabledState.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(138)))), ((int)(((byte)(138)))), ((int)(((byte)(138)))));
            this.txtConfirmPassword.DisabledState.PlaceholderForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(138)))), ((int)(((byte)(138)))), ((int)(((byte)(138)))));
            this.txtConfirmPassword.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(245)))), ((int)(((byte)(248)))), ((int)(((byte)(255)))));
            this.txtConfirmPassword.FocusedState.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(33)))), ((int)(((byte)(147)))), ((int)(((byte)(176)))));
            this.txtConfirmPassword.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.txtConfirmPassword.HoverState.BorderColor = System.Drawing.Color.Transparent;
            this.txtConfirmPassword.HoverState.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(235)))), ((int)(((byte)(240)))), ((int)(((byte)(255)))));
            this.txtConfirmPassword.IconLeft = global::ChatApp.Properties.Resources.MatKhau;
            this.txtConfirmPassword.IconRight = global::ChatApp.Properties.Resources.AnMatKhau;
            this.txtConfirmPassword.IconRightCursor = System.Windows.Forms.Cursors.Hand;
            this.txtConfirmPassword.Location = new System.Drawing.Point(65, 185);
            this.txtConfirmPassword.Margin = new System.Windows.Forms.Padding(3, 5, 3, 5);
            this.txtConfirmPassword.Name = "txtConfirmPassword";
            this.txtConfirmPassword.PasswordChar = '●';
            this.txtConfirmPassword.PlaceholderText = "Xác Nhận Mật Khẩu";
            this.txtConfirmPassword.SelectedText = "";
            this.txtConfirmPassword.Size = new System.Drawing.Size(270, 40);
            this.txtConfirmPassword.TabIndex = 12;
            // 
            // txtEmail
            // 
            this.txtEmail.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(218)))), ((int)(((byte)(220)))), ((int)(((byte)(224)))));
            this.txtEmail.BorderRadius = 15;
            this.txtEmail.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.txtEmail.DefaultText = "";
            this.txtEmail.DisabledState.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(208)))), ((int)(((byte)(208)))), ((int)(((byte)(208)))));
            this.txtEmail.DisabledState.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(226)))), ((int)(((byte)(226)))), ((int)(((byte)(226)))));
            this.txtEmail.DisabledState.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(138)))), ((int)(((byte)(138)))), ((int)(((byte)(138)))));
            this.txtEmail.DisabledState.PlaceholderForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(138)))), ((int)(((byte)(138)))), ((int)(((byte)(138)))));
            this.txtEmail.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(245)))), ((int)(((byte)(248)))), ((int)(((byte)(255)))));
            this.txtEmail.FocusedState.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(33)))), ((int)(((byte)(147)))), ((int)(((byte)(176)))));
            this.txtEmail.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.txtEmail.HoverState.BorderColor = System.Drawing.Color.Transparent;
            this.txtEmail.HoverState.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(235)))), ((int)(((byte)(240)))), ((int)(((byte)(255)))));
            this.txtEmail.IconLeft = global::ChatApp.Properties.Resources.Email;
            this.txtEmail.Location = new System.Drawing.Point(65, 227);
            this.txtEmail.Name = "txtEmail";
            this.txtEmail.PlaceholderText = "Email";
            this.txtEmail.SelectedText = "";
            this.txtEmail.Size = new System.Drawing.Size(270, 40);
            this.txtEmail.TabIndex = 13;
            // 
            // txtUserName
            // 
            this.txtUserName.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(218)))), ((int)(((byte)(220)))), ((int)(((byte)(224)))));
            this.txtUserName.BorderRadius = 15;
            this.txtUserName.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.txtUserName.DefaultText = "";
            this.txtUserName.DisabledState.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(208)))), ((int)(((byte)(208)))), ((int)(((byte)(208)))));
            this.txtUserName.DisabledState.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(226)))), ((int)(((byte)(226)))), ((int)(((byte)(226)))));
            this.txtUserName.DisabledState.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(138)))), ((int)(((byte)(138)))), ((int)(((byte)(138)))));
            this.txtUserName.DisabledState.PlaceholderForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(138)))), ((int)(((byte)(138)))), ((int)(((byte)(138)))));
            this.txtUserName.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(245)))), ((int)(((byte)(248)))), ((int)(((byte)(255)))));
            this.txtUserName.FocusedState.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(33)))), ((int)(((byte)(147)))), ((int)(((byte)(176)))));
            this.txtUserName.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.txtUserName.HoverState.BorderColor = System.Drawing.Color.Transparent;
            this.txtUserName.HoverState.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(235)))), ((int)(((byte)(240)))), ((int)(((byte)(255)))));
            this.txtUserName.IconLeft = global::ChatApp.Properties.Resources.TenDangNhap;
            this.txtUserName.Location = new System.Drawing.Point(65, 58);
            this.txtUserName.Margin = new System.Windows.Forms.Padding(3, 5, 3, 5);
            this.txtUserName.Name = "txtUserName";
            this.txtUserName.PlaceholderText = "Tên Đăng Nhập";
            this.txtUserName.SelectedText = "";
            this.txtUserName.Size = new System.Drawing.Size(270, 40);
            this.txtUserName.TabIndex = 9;
            // 
            // txtPassword
            // 
            this.txtPassword.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(218)))), ((int)(((byte)(220)))), ((int)(((byte)(224)))));
            this.txtPassword.BorderRadius = 15;
            this.txtPassword.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.txtPassword.DefaultText = "";
            this.txtPassword.DisabledState.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(208)))), ((int)(((byte)(208)))), ((int)(((byte)(208)))));
            this.txtPassword.DisabledState.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(226)))), ((int)(((byte)(226)))), ((int)(((byte)(226)))));
            this.txtPassword.DisabledState.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(138)))), ((int)(((byte)(138)))), ((int)(((byte)(138)))));
            this.txtPassword.DisabledState.PlaceholderForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(138)))), ((int)(((byte)(138)))), ((int)(((byte)(138)))));
            this.txtPassword.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(245)))), ((int)(((byte)(248)))), ((int)(((byte)(255)))));
            this.txtPassword.FocusedState.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(33)))), ((int)(((byte)(147)))), ((int)(((byte)(176)))));
            this.txtPassword.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.txtPassword.HoverState.BorderColor = System.Drawing.Color.Transparent;
            this.txtPassword.HoverState.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(235)))), ((int)(((byte)(240)))), ((int)(((byte)(255)))));
            this.txtPassword.IconLeft = global::ChatApp.Properties.Resources.MatKhau;
            this.txtPassword.IconRight = global::ChatApp.Properties.Resources.AnMatKhau;
            this.txtPassword.IconRightCursor = System.Windows.Forms.Cursors.Hand;
            this.txtPassword.Location = new System.Drawing.Point(65, 143);
            this.txtPassword.Margin = new System.Windows.Forms.Padding(3, 5, 3, 5);
            this.txtPassword.Name = "txtPassword";
            this.txtPassword.PasswordChar = '●';
            this.txtPassword.PlaceholderText = "Mật Khẩu";
            this.txtPassword.SelectedText = "";
            this.txtPassword.Size = new System.Drawing.Size(270, 40);
            this.txtPassword.TabIndex = 11;
            this.txtPassword.IconRightClick += new System.EventHandler(this.txtMatKhau_IconRightClick);
            // 
            // pnlBackground
            // 
            this.pnlBackground.Controls.Add(this.pnlLogin);
            this.pnlBackground.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlBackground.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(116)))), ((int)(((byte)(185)))), ((int)(((byte)(255)))));
            this.pnlBackground.FillColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(9)))), ((int)(((byte)(132)))), ((int)(((byte)(227)))));
            this.pnlBackground.GradientMode = System.Drawing.Drawing2D.LinearGradientMode.Vertical;
            this.pnlBackground.Location = new System.Drawing.Point(0, 0);
            this.pnlBackground.Name = "pnlBackground";
            this.pnlBackground.Size = new System.Drawing.Size(782, 453);
            this.pnlBackground.TabIndex = 22;
            // 
            // DangKy
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(94)))), ((int)(((byte)(148)))), ((int)(((byte)(255)))));
            this.ClientSize = new System.Drawing.Size(782, 453);
            this.Controls.Add(this.pnlBackground);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Name = "DangKy";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Đăng Ký";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.DangKy_FormClosed);
            this.pnlLogin.ResumeLayout(false);
            this.pnlLogin.PerformLayout();
            this.pnlBackground.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        private Guna.UI2.WinForms.Guna2TextBox txtEmail;
        private Guna.UI2.WinForms.Guna2TextBox txtConfirmPassword;
        private Guna.UI2.WinForms.Guna2TextBox txtPassword;
        private Guna.UI2.WinForms.Guna2TextBox txtDisplayName;
        private Guna.UI2.WinForms.Guna2TextBox txtUserName;
        private System.Windows.Forms.Label lblTittle;
        private Guna.UI2.WinForms.Guna2DateTimePicker dtpBirthday;
        private Guna.UI2.WinForms.Guna2ComboBox cbbGender;
        private Guna.UI2.WinForms.Guna2Button btnBack;
        private Guna.UI2.WinForms.Guna2Button btnRegister;
        private Guna.UI2.WinForms.Guna2Panel pnlLogin;
        private Guna.UI2.WinForms.Guna2GradientPanel pnlBackground;
    }
}