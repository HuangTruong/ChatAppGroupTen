using Guna.UI2.WinForms;

namespace ChatApp
{
    partial class CatDat
    {
        private System.ComponentModel.IContainer components = null;

        private Guna2Panel pnlMain;
        private Guna2Button btnDoiAvatar;
        private Guna2TextBox txtTenDangNhap;
        private Guna2TextBox txtMatKhau;
        private Guna2Button btnDoiMatKhau;
        private Guna2Button btnDoiTenDangNhap;
        private Guna2Button btnDong;
        private System.Windows.Forms.Label lblTitle;
        private Guna2Button btnDoiGioiTinh;
        private Guna2Button btnDoiTenHienThi;
        private Guna2TextBox txtEmail;
        private Guna2TextBox txtGioiTinh;
        private Guna2TextBox txtTenHienThi;
        private Guna2Button btnDoiNgaySinh;
        private Guna2TextBox txtNgaySinh;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            this.pnlMain = new Guna.UI2.WinForms.Guna2Panel();
            this.lblEmail = new Guna.UI2.WinForms.Guna2HtmlLabel();
            this.lblBirthday = new Guna.UI2.WinForms.Guna2HtmlLabel();
            this.lblGender = new Guna.UI2.WinForms.Guna2HtmlLabel();
            this.lblTenHienThi = new Guna.UI2.WinForms.Guna2HtmlLabel();
            this.lblMatKhau = new Guna.UI2.WinForms.Guna2HtmlLabel();
            this.lblTenDangNhap = new Guna.UI2.WinForms.Guna2HtmlLabel();
            this.picAvatar = new Guna.UI2.WinForms.Guna2PictureBox();
            this.btnDong = new Guna.UI2.WinForms.Guna2Button();
            this.txtTenDangNhap = new Guna.UI2.WinForms.Guna2TextBox();
            this.btnDoiTenDangNhap = new Guna.UI2.WinForms.Guna2Button();
            this.txtMatKhau = new Guna.UI2.WinForms.Guna2TextBox();
            this.btnDoiMatKhau = new Guna.UI2.WinForms.Guna2Button();
            this.txtTenHienThi = new Guna.UI2.WinForms.Guna2TextBox();
            this.btnDoiTenHienThi = new Guna.UI2.WinForms.Guna2Button();
            this.txtGioiTinh = new Guna.UI2.WinForms.Guna2TextBox();
            this.btnDoiGioiTinh = new Guna.UI2.WinForms.Guna2Button();
            this.txtNgaySinh = new Guna.UI2.WinForms.Guna2TextBox();
            this.btnDoiNgaySinh = new Guna.UI2.WinForms.Guna2Button();
            this.txtEmail = new Guna.UI2.WinForms.Guna2TextBox();
            this.btnDoiAvatar = new Guna.UI2.WinForms.Guna2Button();
            this.lblTitle = new System.Windows.Forms.Label();
            this.guna2GradientPanel1 = new Guna.UI2.WinForms.Guna2GradientPanel();
            this.pnlMain.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picAvatar)).BeginInit();
            this.guna2GradientPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // pnlMain
            // 
            this.pnlMain.BackColor = System.Drawing.Color.Transparent;
            this.pnlMain.BorderRadius = 18;
            this.pnlMain.Controls.Add(this.lblEmail);
            this.pnlMain.Controls.Add(this.lblBirthday);
            this.pnlMain.Controls.Add(this.lblGender);
            this.pnlMain.Controls.Add(this.lblTenHienThi);
            this.pnlMain.Controls.Add(this.lblMatKhau);
            this.pnlMain.Controls.Add(this.lblTenDangNhap);
            this.pnlMain.Controls.Add(this.picAvatar);
            this.pnlMain.Controls.Add(this.btnDong);
            this.pnlMain.Controls.Add(this.txtTenDangNhap);
            this.pnlMain.Controls.Add(this.btnDoiTenDangNhap);
            this.pnlMain.Controls.Add(this.txtMatKhau);
            this.pnlMain.Controls.Add(this.btnDoiMatKhau);
            this.pnlMain.Controls.Add(this.txtTenHienThi);
            this.pnlMain.Controls.Add(this.btnDoiTenHienThi);
            this.pnlMain.Controls.Add(this.txtGioiTinh);
            this.pnlMain.Controls.Add(this.btnDoiGioiTinh);
            this.pnlMain.Controls.Add(this.txtNgaySinh);
            this.pnlMain.Controls.Add(this.btnDoiNgaySinh);
            this.pnlMain.Controls.Add(this.txtEmail);
            this.pnlMain.Controls.Add(this.btnDoiAvatar);
            this.pnlMain.Controls.Add(this.lblTitle);
            this.pnlMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlMain.FillColor = System.Drawing.Color.White;
            this.pnlMain.Location = new System.Drawing.Point(20, 20);
            this.pnlMain.Name = "pnlMain";
            this.pnlMain.ShadowDecoration.BorderRadius = 18;
            this.pnlMain.ShadowDecoration.Color = System.Drawing.Color.White;
            this.pnlMain.ShadowDecoration.Depth = 8;
            this.pnlMain.ShadowDecoration.Enabled = true;
            this.pnlMain.Size = new System.Drawing.Size(739, 657);
            this.pnlMain.TabIndex = 0;
            // 
            // lblEmail
            // 
            this.lblEmail.BackColor = System.Drawing.Color.Transparent;
            this.lblEmail.Font = new System.Drawing.Font("Segoe UI Semibold", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblEmail.ForeColor = System.Drawing.Color.White;
            this.lblEmail.Location = new System.Drawing.Point(115, 561);
            this.lblEmail.Name = "lblEmail";
            this.lblEmail.Size = new System.Drawing.Size(53, 25);
            this.lblEmail.TabIndex = 27;
            this.lblEmail.Text = "Email :";
            // 
            // lblBirthday
            // 
            this.lblBirthday.BackColor = System.Drawing.Color.Transparent;
            this.lblBirthday.Font = new System.Drawing.Font("Segoe UI Semibold", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblBirthday.ForeColor = System.Drawing.Color.White;
            this.lblBirthday.Location = new System.Drawing.Point(76, 503);
            this.lblBirthday.Name = "lblBirthday";
            this.lblBirthday.Size = new System.Drawing.Size(91, 25);
            this.lblBirthday.TabIndex = 26;
            this.lblBirthday.Text = "Ngày Sinh :";
            // 
            // lblGender
            // 
            this.lblGender.BackColor = System.Drawing.Color.Transparent;
            this.lblGender.Font = new System.Drawing.Font("Segoe UI Semibold", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblGender.ForeColor = System.Drawing.Color.White;
            this.lblGender.Location = new System.Drawing.Point(88, 445);
            this.lblGender.Name = "lblGender";
            this.lblGender.Size = new System.Drawing.Size(80, 25);
            this.lblGender.TabIndex = 25;
            this.lblGender.Text = "Giới Tính :";
            // 
            // lblTenHienThi
            // 
            this.lblTenHienThi.BackColor = System.Drawing.Color.Transparent;
            this.lblTenHienThi.Font = new System.Drawing.Font("Segoe UI Semibold", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTenHienThi.ForeColor = System.Drawing.Color.White;
            this.lblTenHienThi.Location = new System.Drawing.Point(60, 387);
            this.lblTenHienThi.Name = "lblTenHienThi";
            this.lblTenHienThi.Size = new System.Drawing.Size(109, 25);
            this.lblTenHienThi.TabIndex = 24;
            this.lblTenHienThi.Text = "Tên Hiển Thị :";
            // 
            // lblMatKhau
            // 
            this.lblMatKhau.BackColor = System.Drawing.Color.Transparent;
            this.lblMatKhau.Font = new System.Drawing.Font("Segoe UI Semibold", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblMatKhau.ForeColor = System.Drawing.Color.White;
            this.lblMatKhau.Location = new System.Drawing.Point(82, 329);
            this.lblMatKhau.Name = "lblMatKhau";
            this.lblMatKhau.Size = new System.Drawing.Size(87, 25);
            this.lblMatKhau.TabIndex = 23;
            this.lblMatKhau.Text = "Mật Khẩu :";
            // 
            // lblTenDangNhap
            // 
            this.lblTenDangNhap.BackColor = System.Drawing.Color.Transparent;
            this.lblTenDangNhap.Font = new System.Drawing.Font("Segoe UI Semibold", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTenDangNhap.ForeColor = System.Drawing.Color.White;
            this.lblTenDangNhap.Location = new System.Drawing.Point(33, 270);
            this.lblTenDangNhap.Name = "lblTenDangNhap";
            this.lblTenDangNhap.Size = new System.Drawing.Size(133, 25);
            this.lblTenDangNhap.TabIndex = 22;
            this.lblTenDangNhap.Text = "Tên Đăng Nhập :";
            // 
            // picAvatar
            // 
            this.picAvatar.BackColor = System.Drawing.Color.Transparent;
            this.picAvatar.ImageRotate = 0F;
            this.picAvatar.Location = new System.Drawing.Point(317, 78);
            this.picAvatar.Name = "picAvatar";
            this.picAvatar.Size = new System.Drawing.Size(108, 108);
            this.picAvatar.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.picAvatar.TabIndex = 2;
            this.picAvatar.TabStop = false;
            this.picAvatar.UseTransparentBackground = true;
            // 
            // btnDong
            // 
            this.btnDong.BorderRadius = 10;
            this.btnDong.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnDong.FillColor = System.Drawing.Color.White;
            this.btnDong.Font = new System.Drawing.Font("Segoe UI Semibold", 10F, System.Drawing.FontStyle.Bold);
            this.btnDong.ForeColor = System.Drawing.Color.White;
            this.btnDong.Location = new System.Drawing.Point(308, 603);
            this.btnDong.Name = "btnDong";
            this.btnDong.Size = new System.Drawing.Size(117, 44);
            this.btnDong.TabIndex = 21;
            this.btnDong.Text = "Đóng";
            this.btnDong.Click += new System.EventHandler(this.btnDong_Click);
            // 
            // txtTenDangNhap
            // 
            this.txtTenDangNhap.BorderColor = System.Drawing.Color.White;
            this.txtTenDangNhap.BorderRadius = 10;
            this.txtTenDangNhap.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.txtTenDangNhap.DefaultText = "";
            this.txtTenDangNhap.Font = new System.Drawing.Font("Segoe UI Semibold", 10F, System.Drawing.FontStyle.Bold);
            this.txtTenDangNhap.Location = new System.Drawing.Point(191, 262);
            this.txtTenDangNhap.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.txtTenDangNhap.Name = "txtTenDangNhap";
            this.txtTenDangNhap.PlaceholderForeColor = System.Drawing.Color.White;
            this.txtTenDangNhap.PlaceholderText = "Nhập tên đăng nhập mới";
            this.txtTenDangNhap.SelectedText = "";
            this.txtTenDangNhap.Size = new System.Drawing.Size(301, 44);
            this.txtTenDangNhap.TabIndex = 5;
            // 
            // btnDoiTenDangNhap
            // 
            this.btnDoiTenDangNhap.BorderRadius = 10;
            this.btnDoiTenDangNhap.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnDoiTenDangNhap.FillColor = System.Drawing.Color.White;
            this.btnDoiTenDangNhap.Font = new System.Drawing.Font("Segoe UI Semibold", 10F, System.Drawing.FontStyle.Bold);
            this.btnDoiTenDangNhap.ForeColor = System.Drawing.Color.White;
            this.btnDoiTenDangNhap.Location = new System.Drawing.Point(498, 262);
            this.btnDoiTenDangNhap.Name = "btnDoiTenDangNhap";
            this.btnDoiTenDangNhap.Size = new System.Drawing.Size(202, 44);
            this.btnDoiTenDangNhap.TabIndex = 6;
            this.btnDoiTenDangNhap.Text = "Đổi tên đăng nhập";
            this.btnDoiTenDangNhap.Click += new System.EventHandler(this.btnDoiTenDangNhap_Click);
            // 
            // txtMatKhau
            // 
            this.txtMatKhau.BorderColor = System.Drawing.Color.White;
            this.txtMatKhau.BorderRadius = 10;
            this.txtMatKhau.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.txtMatKhau.DefaultText = "";
            this.txtMatKhau.Font = new System.Drawing.Font("Segoe UI Semibold", 10F, System.Drawing.FontStyle.Bold);
            this.txtMatKhau.Location = new System.Drawing.Point(191, 320);
            this.txtMatKhau.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.txtMatKhau.Name = "txtMatKhau";
            this.txtMatKhau.PlaceholderForeColor = System.Drawing.Color.White;
            this.txtMatKhau.PlaceholderText = "Nhập mật khẩu mới";
            this.txtMatKhau.SelectedText = "";
            this.txtMatKhau.Size = new System.Drawing.Size(301, 44);
            this.txtMatKhau.TabIndex = 8;
            this.txtMatKhau.UseSystemPasswordChar = true;
            // 
            // btnDoiMatKhau
            // 
            this.btnDoiMatKhau.BorderRadius = 10;
            this.btnDoiMatKhau.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnDoiMatKhau.FillColor = System.Drawing.Color.White;
            this.btnDoiMatKhau.Font = new System.Drawing.Font("Segoe UI Semibold", 10F, System.Drawing.FontStyle.Bold);
            this.btnDoiMatKhau.ForeColor = System.Drawing.Color.White;
            this.btnDoiMatKhau.Location = new System.Drawing.Point(498, 320);
            this.btnDoiMatKhau.Name = "btnDoiMatKhau";
            this.btnDoiMatKhau.Size = new System.Drawing.Size(202, 44);
            this.btnDoiMatKhau.TabIndex = 9;
            this.btnDoiMatKhau.Text = "Đổi mật khẩu";
            this.btnDoiMatKhau.Click += new System.EventHandler(this.btnDoiMatKhau_Click);
            // 
            // txtTenHienThi
            // 
            this.txtTenHienThi.BorderColor = System.Drawing.Color.White;
            this.txtTenHienThi.BorderRadius = 10;
            this.txtTenHienThi.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.txtTenHienThi.DefaultText = "";
            this.txtTenHienThi.Font = new System.Drawing.Font("Segoe UI Semibold", 10F, System.Drawing.FontStyle.Bold);
            this.txtTenHienThi.Location = new System.Drawing.Point(191, 378);
            this.txtTenHienThi.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.txtTenHienThi.Name = "txtTenHienThi";
            this.txtTenHienThi.PlaceholderForeColor = System.Drawing.Color.White;
            this.txtTenHienThi.PlaceholderText = "Nhập tên hiển thị";
            this.txtTenHienThi.SelectedText = "";
            this.txtTenHienThi.Size = new System.Drawing.Size(301, 44);
            this.txtTenHienThi.TabIndex = 11;
            // 
            // btnDoiTenHienThi
            // 
            this.btnDoiTenHienThi.BorderRadius = 10;
            this.btnDoiTenHienThi.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnDoiTenHienThi.FillColor = System.Drawing.Color.White;
            this.btnDoiTenHienThi.Font = new System.Drawing.Font("Segoe UI Semibold", 10F, System.Drawing.FontStyle.Bold);
            this.btnDoiTenHienThi.ForeColor = System.Drawing.Color.White;
            this.btnDoiTenHienThi.Location = new System.Drawing.Point(498, 378);
            this.btnDoiTenHienThi.Name = "btnDoiTenHienThi";
            this.btnDoiTenHienThi.Size = new System.Drawing.Size(202, 44);
            this.btnDoiTenHienThi.TabIndex = 12;
            this.btnDoiTenHienThi.Text = "Đổi tên hiển thị";
            this.btnDoiTenHienThi.Click += new System.EventHandler(this.btnDoiTenHienThi_Click);
            // 
            // txtGioiTinh
            // 
            this.txtGioiTinh.BorderColor = System.Drawing.Color.White;
            this.txtGioiTinh.BorderRadius = 10;
            this.txtGioiTinh.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.txtGioiTinh.DefaultText = "";
            this.txtGioiTinh.Font = new System.Drawing.Font("Segoe UI Semibold", 10F, System.Drawing.FontStyle.Bold);
            this.txtGioiTinh.Location = new System.Drawing.Point(191, 436);
            this.txtGioiTinh.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.txtGioiTinh.Name = "txtGioiTinh";
            this.txtGioiTinh.PlaceholderForeColor = System.Drawing.Color.White;
            this.txtGioiTinh.PlaceholderText = "Nhập giới tính";
            this.txtGioiTinh.SelectedText = "";
            this.txtGioiTinh.Size = new System.Drawing.Size(301, 44);
            this.txtGioiTinh.TabIndex = 14;
            // 
            // btnDoiGioiTinh
            // 
            this.btnDoiGioiTinh.BorderRadius = 10;
            this.btnDoiGioiTinh.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnDoiGioiTinh.FillColor = System.Drawing.Color.White;
            this.btnDoiGioiTinh.Font = new System.Drawing.Font("Segoe UI Semibold", 10F, System.Drawing.FontStyle.Bold);
            this.btnDoiGioiTinh.ForeColor = System.Drawing.Color.White;
            this.btnDoiGioiTinh.Location = new System.Drawing.Point(498, 436);
            this.btnDoiGioiTinh.Name = "btnDoiGioiTinh";
            this.btnDoiGioiTinh.Size = new System.Drawing.Size(202, 44);
            this.btnDoiGioiTinh.TabIndex = 15;
            this.btnDoiGioiTinh.Text = "Đổi giới tính";
            this.btnDoiGioiTinh.Click += new System.EventHandler(this.btnDoiGioiTinh_Click);
            // 
            // txtNgaySinh
            // 
            this.txtNgaySinh.BorderColor = System.Drawing.Color.White;
            this.txtNgaySinh.BorderRadius = 10;
            this.txtNgaySinh.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.txtNgaySinh.DefaultText = "";
            this.txtNgaySinh.Font = new System.Drawing.Font("Segoe UI Semibold", 10F, System.Drawing.FontStyle.Bold);
            this.txtNgaySinh.Location = new System.Drawing.Point(191, 494);
            this.txtNgaySinh.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.txtNgaySinh.Name = "txtNgaySinh";
            this.txtNgaySinh.PlaceholderForeColor = System.Drawing.Color.White;
            this.txtNgaySinh.PlaceholderText = "dd/MM/yyyy";
            this.txtNgaySinh.SelectedText = "";
            this.txtNgaySinh.Size = new System.Drawing.Size(301, 44);
            this.txtNgaySinh.TabIndex = 17;
            // 
            // btnDoiNgaySinh
            // 
            this.btnDoiNgaySinh.BorderRadius = 10;
            this.btnDoiNgaySinh.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnDoiNgaySinh.FillColor = System.Drawing.Color.White;
            this.btnDoiNgaySinh.Font = new System.Drawing.Font("Segoe UI Semibold", 10F, System.Drawing.FontStyle.Bold);
            this.btnDoiNgaySinh.ForeColor = System.Drawing.Color.White;
            this.btnDoiNgaySinh.Location = new System.Drawing.Point(498, 494);
            this.btnDoiNgaySinh.Name = "btnDoiNgaySinh";
            this.btnDoiNgaySinh.Size = new System.Drawing.Size(202, 44);
            this.btnDoiNgaySinh.TabIndex = 18;
            this.btnDoiNgaySinh.Text = "Đổi ngày sinh";
            this.btnDoiNgaySinh.Click += new System.EventHandler(this.btnDoiNgaySinh_Click);
            // 
            // txtEmail
            // 
            this.txtEmail.BorderColor = System.Drawing.Color.White;
            this.txtEmail.BorderRadius = 10;
            this.txtEmail.Cursor = System.Windows.Forms.Cursors.Arrow;
            this.txtEmail.DefaultText = "";
            this.txtEmail.Enabled = false;
            this.txtEmail.Font = new System.Drawing.Font("Segoe UI Semibold", 10F, System.Drawing.FontStyle.Bold);
            this.txtEmail.Location = new System.Drawing.Point(191, 552);
            this.txtEmail.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.txtEmail.Name = "txtEmail";
            this.txtEmail.PlaceholderForeColor = System.Drawing.Color.White;
            this.txtEmail.PlaceholderText = "Email";
            this.txtEmail.ReadOnly = true;
            this.txtEmail.SelectedText = "";
            this.txtEmail.Size = new System.Drawing.Size(509, 44);
            this.txtEmail.TabIndex = 20;
            // 
            // btnDoiAvatar
            // 
            this.btnDoiAvatar.BorderRadius = 10;
            this.btnDoiAvatar.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnDoiAvatar.FillColor = System.Drawing.Color.White;
            this.btnDoiAvatar.Font = new System.Drawing.Font("Segoe UI Semibold", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnDoiAvatar.ForeColor = System.Drawing.Color.White;
            this.btnDoiAvatar.Location = new System.Drawing.Point(300, 201);
            this.btnDoiAvatar.Name = "btnDoiAvatar";
            this.btnDoiAvatar.Size = new System.Drawing.Size(134, 40);
            this.btnDoiAvatar.TabIndex = 3;
            this.btnDoiAvatar.Text = "Đổi avatar";
            this.btnDoiAvatar.Click += new System.EventHandler(this.btnDoiAvatar_Click);
            // 
            // lblTitle
            // 
            this.lblTitle.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblTitle.Font = new System.Drawing.Font("Verdana", 18F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTitle.ForeColor = System.Drawing.Color.White;
            this.lblTitle.Location = new System.Drawing.Point(0, 0);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(739, 60);
            this.lblTitle.TabIndex = 1;
            this.lblTitle.Text = "CÀI ĐẶT TÀI KHOẢN";
            this.lblTitle.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            // 
            // guna2GradientPanel1
            // 
            this.guna2GradientPanel1.Controls.Add(this.pnlMain);
            this.guna2GradientPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.guna2GradientPanel1.GradientMode = System.Drawing.Drawing2D.LinearGradientMode.Vertical;
            this.guna2GradientPanel1.Location = new System.Drawing.Point(0, 0);
            this.guna2GradientPanel1.Name = "guna2GradientPanel1";
            this.guna2GradientPanel1.Padding = new System.Windows.Forms.Padding(20);
            this.guna2GradientPanel1.Size = new System.Drawing.Size(779, 697);
            this.guna2GradientPanel1.TabIndex = 28;
            // 
            // CatDat
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(245)))), ((int)(((byte)(248)))), ((int)(((byte)(255)))));
            this.ClientSize = new System.Drawing.Size(779, 697);
            this.Controls.Add(this.guna2GradientPanel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "CatDat";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Cài đặt tài khoản";
            this.Load += new System.EventHandler(this.CatDat_Load);
            this.pnlMain.ResumeLayout(false);
            this.pnlMain.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picAvatar)).EndInit();
            this.guna2GradientPanel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private Guna2PictureBox picAvatar;
        private Guna2HtmlLabel lblEmail;
        private Guna2HtmlLabel lblBirthday;
        private Guna2HtmlLabel lblGender;
        private Guna2HtmlLabel lblTenHienThi;
        private Guna2HtmlLabel lblMatKhau;
        private Guna2HtmlLabel lblTenDangNhap;
        private Guna2GradientPanel guna2GradientPanel1;
    }
}
