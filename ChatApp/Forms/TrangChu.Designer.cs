using System.Drawing;
using System.Windows.Forms;

namespace ChatApp
{
    partial class TrangChu
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TrangChu));
            this.pnlBackground = new Guna.UI2.WinForms.Guna2GradientPanel();
            this.pnlFooter = new Guna.UI2.WinForms.Guna2Panel();
            this.pnlBody = new Guna.UI2.WinForms.Guna2Panel();
            this.pnlCaiDat = new Guna.UI2.WinForms.Guna2Panel();
            this.lblCaiDat = new System.Windows.Forms.Label();
            this.picCaiDat = new Guna.UI2.WinForms.Guna2PictureBox();
            this.pnlNhanTin = new Guna.UI2.WinForms.Guna2Panel();
            this.lblNhanTin = new System.Windows.Forms.Label();
            this.picNhanTin = new Guna.UI2.WinForms.Guna2PictureBox();
            this.pnlDangXuat = new Guna.UI2.WinForms.Guna2Panel();
            this.lblDangXuat = new System.Windows.Forms.Label();
            this.picDangXuat = new Guna.UI2.WinForms.Guna2PictureBox();
            this.pnlHeader = new Guna.UI2.WinForms.Guna2Panel();
            this.lblTenApp = new System.Windows.Forms.Label();
            this.picLogo = new Guna.UI2.WinForms.Guna2PictureBox();
            this.lblTenDangNhap = new System.Windows.Forms.Label();
            this.picAnhDaiDien = new Guna.UI2.WinForms.Guna2PictureBox();
            this.sepHeader = new Guna.UI2.WinForms.Guna2Separator();
            this.picDayNight = new Guna.UI2.WinForms.Guna2CirclePictureBox();
            this.pnlBackground.SuspendLayout();
            this.pnlBody.SuspendLayout();
            this.pnlCaiDat.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picCaiDat)).BeginInit();
            this.pnlNhanTin.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picNhanTin)).BeginInit();
            this.pnlDangXuat.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picDangXuat)).BeginInit();
            this.pnlHeader.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picLogo)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.picAnhDaiDien)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.picDayNight)).BeginInit();
            this.SuspendLayout();
            // 
            // pnlBackground
            // 
            this.pnlBackground.Controls.Add(this.pnlFooter);
            this.pnlBackground.Controls.Add(this.pnlBody);
            this.pnlBackground.Controls.Add(this.pnlHeader);
            this.pnlBackground.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlBackground.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(116)))), ((int)(((byte)(185)))), ((int)(((byte)(255)))));
            this.pnlBackground.FillColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(9)))), ((int)(((byte)(132)))), ((int)(((byte)(227)))));
            this.pnlBackground.GradientMode = System.Drawing.Drawing2D.LinearGradientMode.Vertical;
            this.pnlBackground.Location = new System.Drawing.Point(0, 0);
            this.pnlBackground.Name = "pnlBackground";
            this.pnlBackground.Size = new System.Drawing.Size(1181, 654);
            this.pnlBackground.TabIndex = 1;
            // 
            // pnlFooter
            // 
            this.pnlFooter.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(187)))), ((int)(((byte)(223)))), ((int)(((byte)(255)))));
            this.pnlFooter.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlFooter.Location = new System.Drawing.Point(0, 565);
            this.pnlFooter.Name = "pnlFooter";
            this.pnlFooter.Size = new System.Drawing.Size(1181, 89);
            this.pnlFooter.TabIndex = 5;
            // 
            // pnlBody
            // 
            this.pnlBody.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.pnlBody.BackColor = System.Drawing.Color.Transparent;
            this.pnlBody.BorderRadius = 20;
            this.pnlBody.Controls.Add(this.pnlCaiDat);
            this.pnlBody.Controls.Add(this.pnlNhanTin);
            this.pnlBody.Controls.Add(this.pnlDangXuat);
            this.pnlBody.FillColor = System.Drawing.Color.White;
            this.pnlBody.Location = new System.Drawing.Point(325, 170);
            this.pnlBody.Name = "pnlBody";
            this.pnlBody.ShadowDecoration.BorderRadius = 20;
            this.pnlBody.ShadowDecoration.Color = System.Drawing.Color.FromArgb(((int)(((byte)(176)))), ((int)(((byte)(190)))), ((int)(((byte)(197)))));
            this.pnlBody.ShadowDecoration.Depth = 25;
            this.pnlBody.ShadowDecoration.Enabled = true;
            this.pnlBody.Size = new System.Drawing.Size(525, 316);
            this.pnlBody.TabIndex = 4;
            // 
            // pnlCaiDat
            // 
            this.pnlCaiDat.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.pnlCaiDat.BackColor = System.Drawing.Color.Transparent;
            this.pnlCaiDat.Controls.Add(this.lblCaiDat);
            this.pnlCaiDat.Controls.Add(this.picCaiDat);
            this.pnlCaiDat.Location = new System.Drawing.Point(232, 99);
            this.pnlCaiDat.Name = "pnlCaiDat";
            this.pnlCaiDat.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.pnlCaiDat.Size = new System.Drawing.Size(68, 89);
            this.pnlCaiDat.TabIndex = 2;
            // 
            // lblCaiDat
            // 
            this.lblCaiDat.AutoSize = true;
            this.lblCaiDat.Font = new System.Drawing.Font("Segoe UI Semibold", 7.8F, System.Drawing.FontStyle.Bold);
            this.lblCaiDat.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(52)))), ((int)(((byte)(54)))));
            this.lblCaiDat.Location = new System.Drawing.Point(9, 63);
            this.lblCaiDat.Name = "lblCaiDat";
            this.lblCaiDat.Size = new System.Drawing.Size(51, 17);
            this.lblCaiDat.TabIndex = 2;
            this.lblCaiDat.Text = "Cài Đặt";
            // 
            // picCaiDat
            // 
            this.picCaiDat.Dock = System.Windows.Forms.DockStyle.Top;
            this.picCaiDat.Image = ((System.Drawing.Image)(resources.GetObject("picCaiDat.Image")));
            this.picCaiDat.ImageRotate = 0F;
            this.picCaiDat.Location = new System.Drawing.Point(0, 0);
            this.picCaiDat.Name = "picCaiDat";
            this.picCaiDat.Size = new System.Drawing.Size(68, 60);
            this.picCaiDat.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.picCaiDat.TabIndex = 0;
            this.picCaiDat.TabStop = false;
            this.picCaiDat.Click += new System.EventHandler(this.picCaiDat_Click);
            // 
            // pnlNhanTin
            // 
            this.pnlNhanTin.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.pnlNhanTin.BackColor = System.Drawing.Color.Transparent;
            this.pnlNhanTin.Controls.Add(this.lblNhanTin);
            this.pnlNhanTin.Controls.Add(this.picNhanTin);
            this.pnlNhanTin.Location = new System.Drawing.Point(123, 99);
            this.pnlNhanTin.Name = "pnlNhanTin";
            this.pnlNhanTin.Size = new System.Drawing.Size(69, 89);
            this.pnlNhanTin.TabIndex = 0;
            this.pnlNhanTin.Click += new System.EventHandler(this.pnlNhanTin_Click);
            // 
            // lblNhanTin
            // 
            this.lblNhanTin.AutoSize = true;
            this.lblNhanTin.Font = new System.Drawing.Font("Segoe UI Semibold", 7.8F, System.Drawing.FontStyle.Bold);
            this.lblNhanTin.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(52)))), ((int)(((byte)(54)))));
            this.lblNhanTin.Location = new System.Drawing.Point(3, 63);
            this.lblNhanTin.Name = "lblNhanTin";
            this.lblNhanTin.Size = new System.Drawing.Size(63, 17);
            this.lblNhanTin.TabIndex = 1;
            this.lblNhanTin.Text = "Nhắn Tin";
            // 
            // picNhanTin
            // 
            this.picNhanTin.Dock = System.Windows.Forms.DockStyle.Top;
            this.picNhanTin.Image = ((System.Drawing.Image)(resources.GetObject("picNhanTin.Image")));
            this.picNhanTin.ImageRotate = 0F;
            this.picNhanTin.Location = new System.Drawing.Point(0, 0);
            this.picNhanTin.Name = "picNhanTin";
            this.picNhanTin.Size = new System.Drawing.Size(69, 60);
            this.picNhanTin.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.picNhanTin.TabIndex = 0;
            this.picNhanTin.TabStop = false;
            // 
            // pnlDangXuat
            // 
            this.pnlDangXuat.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.pnlDangXuat.BackColor = System.Drawing.Color.Transparent;
            this.pnlDangXuat.Controls.Add(this.lblDangXuat);
            this.pnlDangXuat.Controls.Add(this.picDangXuat);
            this.pnlDangXuat.Location = new System.Drawing.Point(335, 99);
            this.pnlDangXuat.Name = "pnlDangXuat";
            this.pnlDangXuat.Size = new System.Drawing.Size(79, 89);
            this.pnlDangXuat.TabIndex = 1;
            // 
            // lblDangXuat
            // 
            this.lblDangXuat.AutoSize = true;
            this.lblDangXuat.Font = new System.Drawing.Font("Segoe UI Semibold", 7.8F, System.Drawing.FontStyle.Bold);
            this.lblDangXuat.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(52)))), ((int)(((byte)(54)))));
            this.lblDangXuat.Location = new System.Drawing.Point(3, 63);
            this.lblDangXuat.Name = "lblDangXuat";
            this.lblDangXuat.Size = new System.Drawing.Size(72, 17);
            this.lblDangXuat.TabIndex = 3;
            this.lblDangXuat.Text = "Đăng Xuất";
            // 
            // picDangXuat
            // 
            this.picDangXuat.Dock = System.Windows.Forms.DockStyle.Top;
            this.picDangXuat.Image = ((System.Drawing.Image)(resources.GetObject("picDangXuat.Image")));
            this.picDangXuat.ImageRotate = 0F;
            this.picDangXuat.Location = new System.Drawing.Point(0, 0);
            this.picDangXuat.Name = "picDangXuat";
            this.picDangXuat.Size = new System.Drawing.Size(79, 60);
            this.picDangXuat.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.picDangXuat.TabIndex = 0;
            this.picDangXuat.TabStop = false;
            this.picDangXuat.Click += new System.EventHandler(this.picDangXuat_Click);
            // 
            // pnlHeader
            // 
            this.pnlHeader.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.pnlHeader.Controls.Add(this.picDayNight);
            this.pnlHeader.Controls.Add(this.lblTenApp);
            this.pnlHeader.Controls.Add(this.picLogo);
            this.pnlHeader.Controls.Add(this.lblTenDangNhap);
            this.pnlHeader.Controls.Add(this.picAnhDaiDien);
            this.pnlHeader.Controls.Add(this.sepHeader);
            this.pnlHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlHeader.Location = new System.Drawing.Point(0, 0);
            this.pnlHeader.Name = "pnlHeader";
            this.pnlHeader.Size = new System.Drawing.Size(1181, 89);
            this.pnlHeader.TabIndex = 3;
            // 
            // lblTenApp
            // 
            this.lblTenApp.AutoSize = true;
            this.lblTenApp.BackColor = System.Drawing.Color.Transparent;
            this.lblTenApp.Font = new System.Drawing.Font("Mistral", 32F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTenApp.ForeColor = System.Drawing.Color.White;
            this.lblTenApp.Location = new System.Drawing.Point(80, 17);
            this.lblTenApp.Name = "lblTenApp";
            this.lblTenApp.Size = new System.Drawing.Size(177, 65);
            this.lblTenApp.TabIndex = 6;
            this.lblTenApp.Text = "H4 Chat";
            // 
            // picLogo
            // 
            this.picLogo.BackColor = System.Drawing.Color.Transparent;
            this.picLogo.BorderRadius = 14;
            this.picLogo.Image = ((System.Drawing.Image)(resources.GetObject("picLogo.Image")));
            this.picLogo.ImageRotate = 0F;
            this.picLogo.Location = new System.Drawing.Point(3, 12);
            this.picLogo.Name = "picLogo";
            this.picLogo.Size = new System.Drawing.Size(80, 66);
            this.picLogo.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.picLogo.TabIndex = 2;
            this.picLogo.TabStop = false;
            // 
            // lblTenDangNhap
            // 
            this.lblTenDangNhap.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblTenDangNhap.AutoSize = true;
            this.lblTenDangNhap.Font = new System.Drawing.Font("Segoe UI Semibold", 10.2F, System.Drawing.FontStyle.Bold);
            this.lblTenDangNhap.ForeColor = System.Drawing.Color.White;
            this.lblTenDangNhap.Location = new System.Drawing.Point(513, 40);
            this.lblTenDangNhap.Name = "lblTenDangNhap";
            this.lblTenDangNhap.Size = new System.Drawing.Size(0, 23);
            this.lblTenDangNhap.TabIndex = 1;
            // 
            // picAnhDaiDien
            // 
            this.picAnhDaiDien.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.picAnhDaiDien.BackColor = System.Drawing.Color.Transparent;
            this.picAnhDaiDien.BorderRadius = 14;
            this.picAnhDaiDien.Image = ((System.Drawing.Image)(resources.GetObject("picAnhDaiDien.Image")));
            this.picAnhDaiDien.ImageRotate = 0F;
            this.picAnhDaiDien.Location = new System.Drawing.Point(1099, 8);
            this.picAnhDaiDien.Name = "picAnhDaiDien";
            this.picAnhDaiDien.Size = new System.Drawing.Size(70, 70);
            this.picAnhDaiDien.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.picAnhDaiDien.TabIndex = 0;
            this.picAnhDaiDien.TabStop = false;
            // 
            // sepHeader
            // 
            this.sepHeader.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.sepHeader.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(25)))));
            this.sepHeader.Location = new System.Drawing.Point(0, 88);
            this.sepHeader.Name = "sepHeader";
            this.sepHeader.Size = new System.Drawing.Size(1181, 1);
            this.sepHeader.TabIndex = 8;
            // 
            // picDayNight
            // 
            this.picDayNight.ImageRotate = 0F;
            this.picDayNight.Location = new System.Drawing.Point(1017, 12);
            this.picDayNight.Name = "picDayNight";
            this.picDayNight.ShadowDecoration.Mode = Guna.UI2.WinForms.Enums.ShadowMode.Circle;
            this.picDayNight.Size = new System.Drawing.Size(64, 64);
            this.picDayNight.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.picDayNight.TabIndex = 9;
            this.picDayNight.TabStop = false;
            this.picDayNight.Click += new System.EventHandler(this.picDayNight_Click);
            // 
            // TrangChu
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.ClientSize = new System.Drawing.Size(1181, 654);
            this.Controls.Add(this.pnlBackground);
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "TrangChu";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Load += new System.EventHandler(this.TrangChu_Load);
            this.pnlBackground.ResumeLayout(false);
            this.pnlBody.ResumeLayout(false);
            this.pnlCaiDat.ResumeLayout(false);
            this.pnlCaiDat.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picCaiDat)).EndInit();
            this.pnlNhanTin.ResumeLayout(false);
            this.pnlNhanTin.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picNhanTin)).EndInit();
            this.pnlDangXuat.ResumeLayout(false);
            this.pnlDangXuat.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picDangXuat)).EndInit();
            this.pnlHeader.ResumeLayout(false);
            this.pnlHeader.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picLogo)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.picAnhDaiDien)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.picDayNight)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private Guna.UI2.WinForms.Guna2GradientPanel pnlBackground;
        private Guna.UI2.WinForms.Guna2Panel pnlNhanTin;
        private Guna.UI2.WinForms.Guna2PictureBox picNhanTin;
        private Guna.UI2.WinForms.Guna2Panel pnlDangXuat;
        private Guna.UI2.WinForms.Guna2PictureBox picDangXuat;
        private Guna.UI2.WinForms.Guna2Panel pnlCaiDat;
        private Guna.UI2.WinForms.Guna2PictureBox picCaiDat;
        private System.Windows.Forms.Label lblCaiDat;
        private System.Windows.Forms.Label lblDangXuat;
        private System.Windows.Forms.Label lblNhanTin;
        private Guna.UI2.WinForms.Guna2Panel pnlBody;
        private Guna.UI2.WinForms.Guna2Panel pnlHeader;
        private Guna.UI2.WinForms.Guna2Panel pnlFooter;
        private System.Windows.Forms.Label lblTenApp;
        private Guna.UI2.WinForms.Guna2PictureBox picLogo;
        private System.Windows.Forms.Label lblTenDangNhap;
        private Guna.UI2.WinForms.Guna2PictureBox picAnhDaiDien;
        private Guna.UI2.WinForms.Guna2Separator sepHeader;
        private Guna.UI2.WinForms.Guna2CirclePictureBox picDayNight;
    }
}
