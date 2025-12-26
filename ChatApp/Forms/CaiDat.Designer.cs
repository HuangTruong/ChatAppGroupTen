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
            this.guna2HtmlLabel1 = new Guna.UI2.WinForms.Guna2HtmlLabel();
            this.guna2HtmlLabel2 = new Guna.UI2.WinForms.Guna2HtmlLabel();
            this.guna2HtmlLabel3 = new Guna.UI2.WinForms.Guna2HtmlLabel();
            this.guna2HtmlLabel4 = new Guna.UI2.WinForms.Guna2HtmlLabel();
            this.guna2HtmlLabel5 = new Guna.UI2.WinForms.Guna2HtmlLabel();
            this.guna2HtmlLabel6 = new Guna.UI2.WinForms.Guna2HtmlLabel();
            this.pnlMain.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picAvatar)).BeginInit();
            this.SuspendLayout();
            // 
            // pnlMain
            // 
            this.pnlMain.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pnlMain.BackColor = System.Drawing.Color.Transparent;
            this.pnlMain.BorderRadius = 18;
            this.pnlMain.Controls.Add(this.guna2HtmlLabel6);
            this.pnlMain.Controls.Add(this.guna2HtmlLabel5);
            this.pnlMain.Controls.Add(this.guna2HtmlLabel4);
            this.pnlMain.Controls.Add(this.guna2HtmlLabel3);
            this.pnlMain.Controls.Add(this.guna2HtmlLabel2);
            this.pnlMain.Controls.Add(this.guna2HtmlLabel1);
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
            this.pnlMain.FillColor = System.Drawing.Color.White;
            this.pnlMain.Location = new System.Drawing.Point(18, 18);
            this.pnlMain.Name = "pnlMain";
            this.pnlMain.ShadowDecoration.BorderRadius = 18;
            this.pnlMain.ShadowDecoration.Depth = 8;
            this.pnlMain.ShadowDecoration.Enabled = true;
            this.pnlMain.Size = new System.Drawing.Size(743, 661);
            this.pnlMain.TabIndex = 0;
            // 
            // picAvatar
            // 
            this.picAvatar.BackColor = System.Drawing.Color.Transparent;
            this.picAvatar.BorderRadius = 0;
            this.picAvatar.FillColor = System.Drawing.Color.Silver;
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
            this.btnDong.BorderRadius = 12;
            this.btnDong.Font = new System.Drawing.Font("Segoe UI Semibold", 10F, System.Drawing.FontStyle.Bold);
            this.btnDong.ForeColor = System.Drawing.Color.White;
            this.btnDong.Location = new System.Drawing.Point(291, 602);
            this.btnDong.Name = "btnDong";
            this.btnDong.Size = new System.Drawing.Size(160, 44);
            this.btnDong.TabIndex = 21;
            this.btnDong.Text = "Đóng";
            this.btnDong.Click += new System.EventHandler(this.btnDong_Click);
            // 
            // txtTenDangNhap
            // 
            this.txtTenDangNhap.BorderRadius = 10;
            this.txtTenDangNhap.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.txtTenDangNhap.DefaultText = "";
            this.txtTenDangNhap.Font = new System.Drawing.Font("Segoe UI Semibold", 10F, System.Drawing.FontStyle.Bold);
            this.txtTenDangNhap.Location = new System.Drawing.Point(166, 262);
            this.txtTenDangNhap.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.txtTenDangNhap.Name = "txtTenDangNhap";
            this.txtTenDangNhap.PlaceholderText = "Nhập tên đăng nhập mới";
            this.txtTenDangNhap.SelectedText = "";
            this.txtTenDangNhap.Size = new System.Drawing.Size(320, 44);
            this.txtTenDangNhap.TabIndex = 5;
            // 
            // btnDoiTenDangNhap
            // 
            this.btnDoiTenDangNhap.BorderRadius = 10;
            this.btnDoiTenDangNhap.Font = new System.Drawing.Font("Segoe UI Semibold", 10F, System.Drawing.FontStyle.Bold);
            this.btnDoiTenDangNhap.ForeColor = System.Drawing.Color.White;
            this.btnDoiTenDangNhap.Location = new System.Drawing.Point(500, 262);
            this.btnDoiTenDangNhap.Name = "btnDoiTenDangNhap";
            this.btnDoiTenDangNhap.Size = new System.Drawing.Size(200, 44);
            this.btnDoiTenDangNhap.TabIndex = 6;
            this.btnDoiTenDangNhap.Text = "Đổi tên đăng nhập";
            this.btnDoiTenDangNhap.Click += new System.EventHandler(this.btnDoiTenDangNhap_Click);
            // 
            // txtMatKhau
            // 
            this.txtMatKhau.BorderRadius = 10;
            this.txtMatKhau.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.txtMatKhau.DefaultText = "";
            this.txtMatKhau.Font = new System.Drawing.Font("Segoe UI Semibold", 10F, System.Drawing.FontStyle.Bold);
            this.txtMatKhau.Location = new System.Drawing.Point(166, 320);
            this.txtMatKhau.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.txtMatKhau.Name = "txtMatKhau";
            this.txtMatKhau.PlaceholderText = "Nhập mật khẩu mới";
            this.txtMatKhau.SelectedText = "";
            this.txtMatKhau.Size = new System.Drawing.Size(320, 44);
            this.txtMatKhau.TabIndex = 8;
            this.txtMatKhau.UseSystemPasswordChar = true;
            // 
            // btnDoiMatKhau
            // 
            this.btnDoiMatKhau.BorderRadius = 10;
            this.btnDoiMatKhau.Font = new System.Drawing.Font("Segoe UI Semibold", 10F, System.Drawing.FontStyle.Bold);
            this.btnDoiMatKhau.ForeColor = System.Drawing.Color.White;
            this.btnDoiMatKhau.Location = new System.Drawing.Point(500, 320);
            this.btnDoiMatKhau.Name = "btnDoiMatKhau";
            this.btnDoiMatKhau.Size = new System.Drawing.Size(200, 44);
            this.btnDoiMatKhau.TabIndex = 9;
            this.btnDoiMatKhau.Text = "Đổi mật khẩu";
            this.btnDoiMatKhau.Click += new System.EventHandler(this.btnDoiMatKhau_Click);
            // 
            // txtTenHienThi
            // 
            this.txtTenHienThi.BorderRadius = 10;
            this.txtTenHienThi.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.txtTenHienThi.DefaultText = "";
            this.txtTenHienThi.Font = new System.Drawing.Font("Segoe UI Semibold", 10F, System.Drawing.FontStyle.Bold);
            this.txtTenHienThi.Location = new System.Drawing.Point(166, 378);
            this.txtTenHienThi.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.txtTenHienThi.Name = "txtTenHienThi";
            this.txtTenHienThi.PlaceholderText = "Nhập tên hiển thị";
            this.txtTenHienThi.SelectedText = "";
            this.txtTenHienThi.Size = new System.Drawing.Size(320, 44);
            this.txtTenHienThi.TabIndex = 11;
            // 
            // btnDoiTenHienThi
            // 
            this.btnDoiTenHienThi.BorderRadius = 10;
            this.btnDoiTenHienThi.Font = new System.Drawing.Font("Segoe UI Semibold", 10F, System.Drawing.FontStyle.Bold);
            this.btnDoiTenHienThi.ForeColor = System.Drawing.Color.White;
            this.btnDoiTenHienThi.Location = new System.Drawing.Point(500, 378);
            this.btnDoiTenHienThi.Name = "btnDoiTenHienThi";
            this.btnDoiTenHienThi.Size = new System.Drawing.Size(200, 44);
            this.btnDoiTenHienThi.TabIndex = 12;
            this.btnDoiTenHienThi.Text = "Đổi tên hiển thị";
            this.btnDoiTenHienThi.Click += new System.EventHandler(this.btnDoiTenHienThi_Click);
            // 
            // txtGioiTinh
            // 
            this.txtGioiTinh.BorderRadius = 10;
            this.txtGioiTinh.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.txtGioiTinh.DefaultText = "";
            this.txtGioiTinh.Font = new System.Drawing.Font("Segoe UI Semibold", 10F, System.Drawing.FontStyle.Bold);
            this.txtGioiTinh.Location = new System.Drawing.Point(166, 436);
            this.txtGioiTinh.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.txtGioiTinh.Name = "txtGioiTinh";
            this.txtGioiTinh.PlaceholderText = "Nhập giới tính";
            this.txtGioiTinh.SelectedText = "";
            this.txtGioiTinh.Size = new System.Drawing.Size(320, 44);
            this.txtGioiTinh.TabIndex = 14;
            // 
            // btnDoiGioiTinh
            // 
            this.btnDoiGioiTinh.BorderRadius = 10;
            this.btnDoiGioiTinh.Font = new System.Drawing.Font("Segoe UI Semibold", 10F, System.Drawing.FontStyle.Bold);
            this.btnDoiGioiTinh.ForeColor = System.Drawing.Color.White;
            this.btnDoiGioiTinh.Location = new System.Drawing.Point(500, 436);
            this.btnDoiGioiTinh.Name = "btnDoiGioiTinh";
            this.btnDoiGioiTinh.Size = new System.Drawing.Size(200, 44);
            this.btnDoiGioiTinh.TabIndex = 15;
            this.btnDoiGioiTinh.Text = "Đổi giới tính";
            this.btnDoiGioiTinh.Click += new System.EventHandler(this.btnDoiGioiTinh_Click);
            // 
            // txtNgaySinh
            // 
            this.txtNgaySinh.BorderRadius = 10;
            this.txtNgaySinh.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.txtNgaySinh.DefaultText = "";
            this.txtNgaySinh.Font = new System.Drawing.Font("Segoe UI Semibold", 10F, System.Drawing.FontStyle.Bold);
            this.txtNgaySinh.Location = new System.Drawing.Point(166, 494);
            this.txtNgaySinh.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.txtNgaySinh.Name = "txtNgaySinh";
            this.txtNgaySinh.PlaceholderText = "dd/MM/yyyy";
            this.txtNgaySinh.SelectedText = "";
            this.txtNgaySinh.Size = new System.Drawing.Size(320, 44);
            this.txtNgaySinh.TabIndex = 17;
            // 
            // btnDoiNgaySinh
            // 
            this.btnDoiNgaySinh.BorderRadius = 10;
            this.btnDoiNgaySinh.Font = new System.Drawing.Font("Segoe UI Semibold", 10F, System.Drawing.FontStyle.Bold);
            this.btnDoiNgaySinh.ForeColor = System.Drawing.Color.White;
            this.btnDoiNgaySinh.Location = new System.Drawing.Point(500, 494);
            this.btnDoiNgaySinh.Name = "btnDoiNgaySinh";
            this.btnDoiNgaySinh.Size = new System.Drawing.Size(200, 44);
            this.btnDoiNgaySinh.TabIndex = 18;
            this.btnDoiNgaySinh.Text = "Đổi ngày sinh";
            this.btnDoiNgaySinh.Click += new System.EventHandler(this.btnDoiNgaySinh_Click);
            // 
            // txtEmail
            // 
            this.txtEmail.BorderRadius = 10;
            this.txtEmail.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.txtEmail.DefaultText = "";
            this.txtEmail.Font = new System.Drawing.Font("Segoe UI Semibold", 10F, System.Drawing.FontStyle.Bold);
            this.txtEmail.Location = new System.Drawing.Point(166, 552);
            this.txtEmail.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.txtEmail.Name = "txtEmail";
            this.txtEmail.PlaceholderText = "";
            this.txtEmail.ReadOnly = true;
            this.txtEmail.SelectedText = "";
            this.txtEmail.Size = new System.Drawing.Size(534, 44);
            this.txtEmail.TabIndex = 20;
            // 
            // btnDoiAvatar
            // 
            this.btnDoiAvatar.BorderRadius = 10;
            this.btnDoiAvatar.Font = new System.Drawing.Font("Segoe UI Semibold", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnDoiAvatar.ForeColor = System.Drawing.Color.White;
            this.btnDoiAvatar.Location = new System.Drawing.Point(291, 196);
            this.btnDoiAvatar.Name = "btnDoiAvatar";
            this.btnDoiAvatar.Size = new System.Drawing.Size(160, 40);
            this.btnDoiAvatar.TabIndex = 3;
            this.btnDoiAvatar.Text = "Đổi avatar";
            this.btnDoiAvatar.Click += new System.EventHandler(this.btnDoiAvatar_Click);
            // 
            // lblTitle
            // 
            this.lblTitle.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblTitle.Font = new System.Drawing.Font("Segoe UI", 16.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTitle.Location = new System.Drawing.Point(0, 0);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(743, 60);
            this.lblTitle.TabIndex = 1;
            this.lblTitle.Text = "CÀI ĐẶT TÀI KHOẢN";
            this.lblTitle.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // guna2HtmlLabel1
            // 
            this.guna2HtmlLabel1.BackColor = System.Drawing.Color.Transparent;
            this.guna2HtmlLabel1.Font = new System.Drawing.Font("Segoe UI", 10.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.guna2HtmlLabel1.Location = new System.Drawing.Point(33, 270);
            this.guna2HtmlLabel1.Name = "guna2HtmlLabel1";
            this.guna2HtmlLabel1.Size = new System.Drawing.Size(127, 27);
            this.guna2HtmlLabel1.TabIndex = 22;
            this.guna2HtmlLabel1.Text = "Tên Đăng Nhập";
            // 
            // guna2HtmlLabel2
            // 
            this.guna2HtmlLabel2.BackColor = System.Drawing.Color.Transparent;
            this.guna2HtmlLabel2.Font = new System.Drawing.Font("Segoe UI", 10.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.guna2HtmlLabel2.Location = new System.Drawing.Point(82, 329);
            this.guna2HtmlLabel2.Name = "guna2HtmlLabel2";
            this.guna2HtmlLabel2.Size = new System.Drawing.Size(78, 27);
            this.guna2HtmlLabel2.TabIndex = 23;
            this.guna2HtmlLabel2.Text = "Mật Khẩu";
            // 
            // guna2HtmlLabel3
            // 
            this.guna2HtmlLabel3.BackColor = System.Drawing.Color.Transparent;
            this.guna2HtmlLabel3.Font = new System.Drawing.Font("Segoe UI", 10.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.guna2HtmlLabel3.Location = new System.Drawing.Point(60, 387);
            this.guna2HtmlLabel3.Name = "guna2HtmlLabel3";
            this.guna2HtmlLabel3.Size = new System.Drawing.Size(100, 27);
            this.guna2HtmlLabel3.TabIndex = 24;
            this.guna2HtmlLabel3.Text = "Tên Hiển Thị";
            // 
            // guna2HtmlLabel4
            // 
            this.guna2HtmlLabel4.BackColor = System.Drawing.Color.Transparent;
            this.guna2HtmlLabel4.Font = new System.Drawing.Font("Segoe UI", 10.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.guna2HtmlLabel4.Location = new System.Drawing.Point(88, 445);
            this.guna2HtmlLabel4.Name = "guna2HtmlLabel4";
            this.guna2HtmlLabel4.Size = new System.Drawing.Size(72, 27);
            this.guna2HtmlLabel4.TabIndex = 25;
            this.guna2HtmlLabel4.Text = "Giới Tính";
            // 
            // guna2HtmlLabel5
            // 
            this.guna2HtmlLabel5.BackColor = System.Drawing.Color.Transparent;
            this.guna2HtmlLabel5.Font = new System.Drawing.Font("Segoe UI", 10.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.guna2HtmlLabel5.Location = new System.Drawing.Point(76, 503);
            this.guna2HtmlLabel5.Name = "guna2HtmlLabel5";
            this.guna2HtmlLabel5.Size = new System.Drawing.Size(84, 27);
            this.guna2HtmlLabel5.TabIndex = 26;
            this.guna2HtmlLabel5.Text = "Ngày Sinh";
            // 
            // guna2HtmlLabel6
            // 
            this.guna2HtmlLabel6.BackColor = System.Drawing.Color.Transparent;
            this.guna2HtmlLabel6.Font = new System.Drawing.Font("Segoe UI", 10.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.guna2HtmlLabel6.Location = new System.Drawing.Point(115, 561);
            this.guna2HtmlLabel6.Name = "guna2HtmlLabel6";
            this.guna2HtmlLabel6.Size = new System.Drawing.Size(45, 27);
            this.guna2HtmlLabel6.TabIndex = 27;
            this.guna2HtmlLabel6.Text = "Email";
            // 
            // CatDat
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(245)))), ((int)(((byte)(248)))), ((int)(((byte)(255)))));
            this.ClientSize = new System.Drawing.Size(779, 697);
            this.Controls.Add(this.pnlMain);
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
            this.ResumeLayout(false);

        }

        #endregion

        private Guna2PictureBox picAvatar;
        private Guna2HtmlLabel guna2HtmlLabel6;
        private Guna2HtmlLabel guna2HtmlLabel5;
        private Guna2HtmlLabel guna2HtmlLabel4;
        private Guna2HtmlLabel guna2HtmlLabel3;
        private Guna2HtmlLabel guna2HtmlLabel2;
        private Guna2HtmlLabel guna2HtmlLabel1;
    }
}
