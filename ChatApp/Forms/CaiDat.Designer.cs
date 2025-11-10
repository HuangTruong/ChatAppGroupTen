using Guna.UI2.WinForms;

namespace ChatApp
{
    partial class CatDat
    {
        private System.ComponentModel.IContainer components = null;

        private Guna2Panel pnlMain;
        private Guna2TextBox txtTenDangNhap;
        private Guna2TextBox txtEmail;
        private Guna2Button btnDoiMatKhau;
        private Guna2Button btnDoiEmail;
        private Guna2Button btnDong;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Label lblTenDangNhap;
        private System.Windows.Forms.Label lblEmail;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            this.pnlMain = new Guna2Panel();
            this.lblTitle = new System.Windows.Forms.Label();
            this.lblTenDangNhap = new System.Windows.Forms.Label();
            this.lblEmail = new System.Windows.Forms.Label();
            this.txtTenDangNhap = new Guna2TextBox();
            this.txtEmail = new Guna2TextBox();
            this.btnDoiMatKhau = new Guna2Button();
            this.btnDoiEmail = new Guna2Button();
            this.btnDong = new Guna2Button();
            this.pnlMain.SuspendLayout();
            this.SuspendLayout();
            // 
            // pnlMain
            // 
            this.pnlMain.BorderRadius = 16;
            this.pnlMain.FillColor = System.Drawing.Color.White;
            this.pnlMain.ShadowDecoration.BorderRadius = 16;
            this.pnlMain.ShadowDecoration.Enabled = true;
            this.pnlMain.Controls.Add(this.lblTitle);
            this.pnlMain.Controls.Add(this.lblTenDangNhap);
            this.pnlMain.Controls.Add(this.lblEmail);
            this.pnlMain.Controls.Add(this.txtTenDangNhap);
            this.pnlMain.Controls.Add(this.txtEmail);
            this.pnlMain.Controls.Add(this.btnDoiMatKhau);
            this.pnlMain.Controls.Add(this.btnDoiEmail);
            this.pnlMain.Controls.Add(this.btnDong);
            this.pnlMain.Location = new System.Drawing.Point(20, 20);
            this.pnlMain.Name = "pnlMain";
            this.pnlMain.Size = new System.Drawing.Size(440, 260);
            this.pnlMain.TabIndex = 0;
            // 
            // lblTitle
            // 
            this.lblTitle.Text = "CÀI ĐẶT TÀI KHOẢN";
            this.lblTitle.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this.lblTitle.AutoSize = true;
            this.lblTitle.Location = new System.Drawing.Point(130, 15);
            // 
            // lblTenDangNhap
            // 
            this.lblTenDangNhap.Text = "Tên đăng nhập:";
            this.lblTenDangNhap.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblTenDangNhap.AutoSize = true;
            this.lblTenDangNhap.Location = new System.Drawing.Point(30, 55);
            // 
            // lblEmail
            // 
            this.lblEmail.Text = "Email:";
            this.lblEmail.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblEmail.AutoSize = true;
            this.lblEmail.Location = new System.Drawing.Point(30, 105);
            // 
            // txtTenDangNhap
            // 
            this.txtTenDangNhap.BorderRadius = 10;
            this.txtTenDangNhap.Location = new System.Drawing.Point(150, 48);
            this.txtTenDangNhap.Size = new System.Drawing.Size(250, 32);
            this.txtTenDangNhap.PlaceholderText = "";
            this.txtTenDangNhap.ReadOnly = true;
            // 
            // txtEmail
            // 
            this.txtEmail.BorderRadius = 10;
            this.txtEmail.Location = new System.Drawing.Point(150, 98);
            this.txtEmail.Size = new System.Drawing.Size(250, 32);
            this.txtEmail.PlaceholderText = "";
            this.txtEmail.ReadOnly = true;
            // 
            // btnDoiMatKhau
            // 
            this.btnDoiMatKhau.Text = "Đổi mật khẩu";
            this.btnDoiMatKhau.BorderRadius = 10;
            this.btnDoiMatKhau.Location = new System.Drawing.Point(33, 160);
            this.btnDoiMatKhau.Size = new System.Drawing.Size(130, 40);
            this.btnDoiMatKhau.Click += new System.EventHandler(this.btnDoiMatKhau_Click);
            // 
            // btnDoiEmail
            // 
            this.btnDoiEmail.Text = "Đổi email";
            this.btnDoiEmail.BorderRadius = 10;
            this.btnDoiEmail.Location = new System.Drawing.Point(169, 160);
            this.btnDoiEmail.Size = new System.Drawing.Size(130, 40);
            this.btnDoiEmail.Click += new System.EventHandler(this.btnDoiEmail_Click);
            // 
            // btnDong
            // 
            this.btnDong.Text = "Đóng";
            this.btnDong.BorderRadius = 10;
            this.btnDong.Location = new System.Drawing.Point(305, 160);
            this.btnDong.Size = new System.Drawing.Size(95, 40);
            this.btnDong.Click += new System.EventHandler(this.btnDong_Click);
            // 
            // CatDat
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(245, 248, 255);
            this.ClientSize = new System.Drawing.Size(480, 300);
            this.Controls.Add(this.pnlMain);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Name = "CatDat";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Cài đặt tài khoản";
            this.Load += new System.EventHandler(this.CatDat_Load);
            this.pnlMain.ResumeLayout(false);
            this.pnlMain.PerformLayout();
            this.ResumeLayout(false);
        }

        #endregion
    }
}
