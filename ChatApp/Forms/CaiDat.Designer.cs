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

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            this.pnlMain = new Guna.UI2.WinForms.Guna2Panel();
            this.btnDoiAvatar = new Guna.UI2.WinForms.Guna2Button();
            this.lblTitle = new System.Windows.Forms.Label();
            this.txtTenDangNhap = new Guna.UI2.WinForms.Guna2TextBox();
            this.txtMatKhau = new Guna.UI2.WinForms.Guna2TextBox();
            this.btnDoiMatKhau = new Guna.UI2.WinForms.Guna2Button();
            this.btnDoiTenDangNhap = new Guna.UI2.WinForms.Guna2Button();
            this.btnDong = new Guna.UI2.WinForms.Guna2Button();
            this.picAvatar = new Guna.UI2.WinForms.Guna2PictureBox();
            this.pnlMain.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picAvatar)).BeginInit();
            this.SuspendLayout();
            // 
            // pnlMain
            // 
            this.pnlMain.BackColor = System.Drawing.Color.Transparent;
            this.pnlMain.BorderRadius = 16;
            this.pnlMain.Controls.Add(this.picAvatar);
            this.pnlMain.Controls.Add(this.btnDoiAvatar);
            this.pnlMain.Controls.Add(this.lblTitle);
            this.pnlMain.Controls.Add(this.txtTenDangNhap);
            this.pnlMain.Controls.Add(this.txtMatKhau);
            this.pnlMain.Controls.Add(this.btnDoiMatKhau);
            this.pnlMain.Controls.Add(this.btnDoiTenDangNhap);
            this.pnlMain.Controls.Add(this.btnDong);
            this.pnlMain.FillColor = System.Drawing.Color.White;
            this.pnlMain.Location = new System.Drawing.Point(20, 20);
            this.pnlMain.Name = "pnlMain";
            this.pnlMain.ShadowDecoration.BorderRadius = 16;
            this.pnlMain.ShadowDecoration.Enabled = true;
            this.pnlMain.Size = new System.Drawing.Size(747, 388);
            this.pnlMain.TabIndex = 0;
            // 
            // btnDoiAvatar
            // 
            this.btnDoiAvatar.BorderRadius = 10;
            this.btnDoiAvatar.Font = new System.Drawing.Font("Segoe UI Semibold", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnDoiAvatar.ForeColor = System.Drawing.Color.White;
            this.btnDoiAvatar.Location = new System.Drawing.Point(304, 165);
            this.btnDoiAvatar.Name = "btnDoiAvatar";
            this.btnDoiAvatar.Size = new System.Drawing.Size(143, 40);
            this.btnDoiAvatar.TabIndex = 1;
            this.btnDoiAvatar.Text = "Đổi avatar";
            this.btnDoiAvatar.Click += new System.EventHandler(this.btnDoiAvatar_Click);
            // 
            // lblTitle
            // 
            this.lblTitle.AutoSize = true;
            this.lblTitle.Font = new System.Drawing.Font("Segoe UI", 16.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTitle.Location = new System.Drawing.Point(241, 34);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(289, 38);
            this.lblTitle.TabIndex = 2;
            this.lblTitle.Text = "CÀI ĐẶT TÀI KHOẢN";
            // 
            // txtTenDangNhap
            // 
            this.txtTenDangNhap.BorderRadius = 10;
            this.txtTenDangNhap.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.txtTenDangNhap.DefaultText = "";
            this.txtTenDangNhap.Font = new System.Drawing.Font("Segoe UI Semibold", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtTenDangNhap.Location = new System.Drawing.Point(134, 230);
            this.txtTenDangNhap.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.txtTenDangNhap.Name = "txtTenDangNhap";
            this.txtTenDangNhap.PlaceholderText = "Nhập tên đăng nhập mới";
            this.txtTenDangNhap.ReadOnly = true;
            this.txtTenDangNhap.SelectedText = "";
            this.txtTenDangNhap.Size = new System.Drawing.Size(269, 42);
            this.txtTenDangNhap.TabIndex = 2;
            // 
            // txtMatKhau
            // 
            this.txtMatKhau.BorderRadius = 10;
            this.txtMatKhau.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.txtMatKhau.DefaultText = "";
            this.txtMatKhau.Font = new System.Drawing.Font("Segoe UI Semibold", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtMatKhau.Location = new System.Drawing.Point(134, 280);
            this.txtMatKhau.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.txtMatKhau.Name = "txtMatKhau";
            this.txtMatKhau.PlaceholderText = "Nhập mật khẩu mới";
            this.txtMatKhau.SelectedText = "";
            this.txtMatKhau.Size = new System.Drawing.Size(269, 42);
            this.txtMatKhau.TabIndex = 3;
            // 
            // btnDoiMatKhau
            // 
            this.btnDoiMatKhau.BorderRadius = 10;
            this.btnDoiMatKhau.Font = new System.Drawing.Font("Segoe UI Semibold", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnDoiMatKhau.ForeColor = System.Drawing.Color.White;
            this.btnDoiMatKhau.Location = new System.Drawing.Point(409, 279);
            this.btnDoiMatKhau.Name = "btnDoiMatKhau";
            this.btnDoiMatKhau.Size = new System.Drawing.Size(218, 43);
            this.btnDoiMatKhau.TabIndex = 6;
            this.btnDoiMatKhau.Text = "Đổi mật khẩu";
            this.btnDoiMatKhau.Click += new System.EventHandler(this.btnDoiMatKhau_Click);
            // 
            // btnDoiTenDangNhap
            // 
            this.btnDoiTenDangNhap.BorderRadius = 10;
            this.btnDoiTenDangNhap.Font = new System.Drawing.Font("Segoe UI Semibold", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnDoiTenDangNhap.ForeColor = System.Drawing.Color.White;
            this.btnDoiTenDangNhap.Location = new System.Drawing.Point(409, 229);
            this.btnDoiTenDangNhap.Name = "btnDoiTenDangNhap";
            this.btnDoiTenDangNhap.Size = new System.Drawing.Size(218, 43);
            this.btnDoiTenDangNhap.TabIndex = 7;
            this.btnDoiTenDangNhap.Text = "Đổi tên đăng nhập";
            this.btnDoiTenDangNhap.Click += new System.EventHandler(this.btnDoiTenDangNhap_Click);
            // 
            // btnDong
            // 
            this.btnDong.BorderRadius = 10;
            this.btnDong.Font = new System.Drawing.Font("Segoe UI Semibold", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnDong.ForeColor = System.Drawing.Color.White;
            this.btnDong.Location = new System.Drawing.Point(302, 328);
            this.btnDong.Name = "btnDong";
            this.btnDong.Size = new System.Drawing.Size(145, 40);
            this.btnDong.TabIndex = 8;
            this.btnDong.Text = "Đóng";
            this.btnDong.Click += new System.EventHandler(this.btnDong_Click);
            // 
            // picAvatar
            // 
            this.picAvatar.BorderRadius = 30;
            this.picAvatar.FillColor = System.Drawing.Color.Silver;
            this.picAvatar.ImageRotate = 0F;
            this.picAvatar.Location = new System.Drawing.Point(332, 75);
            this.picAvatar.Name = "picAvatar";
            this.picAvatar.Size = new System.Drawing.Size(87, 85);
            this.picAvatar.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.picAvatar.TabIndex = 9;
            this.picAvatar.TabStop = false;
            // 
            // CatDat
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(245)))), ((int)(((byte)(248)))), ((int)(((byte)(255)))));
            this.ClientSize = new System.Drawing.Size(779, 422);
            this.Controls.Add(this.pnlMain);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
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
    }
}
