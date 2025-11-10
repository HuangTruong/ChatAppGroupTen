namespace ChatApp
{
    partial class XacNhanEmail
    {
        private System.ComponentModel.IContainer components = null;

        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Label lblEmail;
        private Guna.UI2.WinForms.Guna2TextBox txtMa;
        private Guna.UI2.WinForms.Guna2Button btnXacNhan;
        private Guna.UI2.WinForms.Guna2Button btnGuiLai;
        private Guna.UI2.WinForms.Guna2Button btnHuy;
        private System.Windows.Forms.Label lblDemNguoc;
        private System.Windows.Forms.Timer timerCooldown;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.lblTitle = new System.Windows.Forms.Label();
            this.lblEmail = new System.Windows.Forms.Label();
            this.txtMa = new Guna.UI2.WinForms.Guna2TextBox();
            this.btnXacNhan = new Guna.UI2.WinForms.Guna2Button();
            this.btnGuiLai = new Guna.UI2.WinForms.Guna2Button();
            this.btnHuy = new Guna.UI2.WinForms.Guna2Button();
            this.lblDemNguoc = new System.Windows.Forms.Label();
            this.timerCooldown = new System.Windows.Forms.Timer(this.components);
            this.SuspendLayout();
            // 
            // lblTitle
            // 
            this.lblTitle.Text = "Nhập mã xác nhận đã gửi tới:";
            this.lblTitle.AutoSize = true;
            this.lblTitle.Font = new System.Drawing.Font("Segoe UI Semibold", 10.5F);
            this.lblTitle.Location = new System.Drawing.Point(24, 18);
            // 
            // lblEmail
            // 
            this.lblEmail.Text = "email@example.com";
            this.lblEmail.AutoSize = true;
            this.lblEmail.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.lblEmail.Location = new System.Drawing.Point(24, 45);
            // 
            // txtMa
            // 
            this.txtMa.BorderRadius = 12;
            this.txtMa.PlaceholderText = "Nhập mã 6 số";
            this.txtMa.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.txtMa.Location = new System.Drawing.Point(28, 80);
            this.txtMa.Size = new System.Drawing.Size(290, 40);
            this.txtMa.MaxLength = 6;
            this.txtMa.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // btnXacNhan
            // 
            this.btnXacNhan.Text = "Xác nhận";
            this.btnXacNhan.BorderRadius = 10;
            this.btnXacNhan.Location = new System.Drawing.Point(28, 136);
            this.btnXacNhan.Size = new System.Drawing.Size(120, 40);
            this.btnXacNhan.Click += new System.EventHandler(this.btnXacNhan_Click);
            // 
            // btnGuiLai
            // 
            this.btnGuiLai.Text = "Gửi lại mã";
            this.btnGuiLai.BorderRadius = 10;
            this.btnGuiLai.Location = new System.Drawing.Point(158, 136);
            this.btnGuiLai.Size = new System.Drawing.Size(120, 40);
            this.btnGuiLai.Click += new System.EventHandler(this.btnGuiLai_Click);
            // 
            // btnHuy
            // 
            this.btnHuy.Text = "Hủy";
            this.btnHuy.BorderRadius = 10;
            this.btnHuy.Location = new System.Drawing.Point(288, 136);
            this.btnHuy.Size = new System.Drawing.Size(80, 40);
            this.btnHuy.Click += new System.EventHandler(this.btnHuy_Click);
            // 
            // lblDemNguoc
            // 
            this.lblDemNguoc.Text = "";
            this.lblDemNguoc.AutoSize = true;
            this.lblDemNguoc.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblDemNguoc.Location = new System.Drawing.Point(25, 186);
            // 
            // timerCooldown
            // 
            this.timerCooldown.Tick += new System.EventHandler(this.timerCooldown_Tick);
            // 
            // XacNhanEmail
            // 
            this.ClientSize = new System.Drawing.Size(396, 226);
            this.Controls.Add(this.lblTitle);
            this.Controls.Add(this.lblEmail);
            this.Controls.Add(this.txtMa);
            this.Controls.Add(this.btnXacNhan);
            this.Controls.Add(this.btnGuiLai);
            this.Controls.Add(this.btnHuy);
            this.Controls.Add(this.lblDemNguoc);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Xác nhận email";
            this.Load += new System.EventHandler(this.XacNhanEmail_Load);
            this.ResumeLayout(false);
            this.PerformLayout();
        }
    }
}
