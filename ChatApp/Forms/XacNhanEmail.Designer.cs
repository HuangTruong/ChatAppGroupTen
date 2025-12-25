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
            this.lblTitle.AutoSize = true;
            this.lblTitle.Font = new System.Drawing.Font("Segoe UI Semibold", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTitle.Location = new System.Drawing.Point(27, 29);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(239, 23);
            this.lblTitle.TabIndex = 0;
            this.lblTitle.Text = "Nhập mã xác nhận đã gửi tới:";
            // 
            // lblEmail
            // 
            this.lblEmail.AutoSize = true;
            this.lblEmail.Font = new System.Drawing.Font("Segoe UI Semibold", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblEmail.Location = new System.Drawing.Point(27, 56);
            this.lblEmail.Name = "lblEmail";
            this.lblEmail.Size = new System.Drawing.Size(169, 23);
            this.lblEmail.TabIndex = 1;
            this.lblEmail.Text = "email@example.com";
            // 
            // txtMa
            // 
            this.txtMa.BorderRadius = 12;
            this.txtMa.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.txtMa.DefaultText = "";
            this.txtMa.Font = new System.Drawing.Font("Segoe UI Semibold", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtMa.Location = new System.Drawing.Point(31, 91);
            this.txtMa.Margin = new System.Windows.Forms.Padding(3, 5, 3, 5);
            this.txtMa.MaxLength = 6;
            this.txtMa.Name = "txtMa";
            this.txtMa.PlaceholderText = "Nhập mã 6 số";
            this.txtMa.SelectedText = "";
            this.txtMa.Size = new System.Drawing.Size(290, 40);
            this.txtMa.TabIndex = 2;
            this.txtMa.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // btnXacNhan
            // 
            this.btnXacNhan.BorderRadius = 10;
            this.btnXacNhan.Font = new System.Drawing.Font("Segoe UI Semibold", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnXacNhan.ForeColor = System.Drawing.Color.White;
            this.btnXacNhan.Location = new System.Drawing.Point(31, 147);
            this.btnXacNhan.Name = "btnXacNhan";
            this.btnXacNhan.Size = new System.Drawing.Size(120, 40);
            this.btnXacNhan.TabIndex = 3;
            this.btnXacNhan.Text = "Xác nhận";
            this.btnXacNhan.Click += new System.EventHandler(this.btnXacNhan_Click);
            // 
            // btnGuiLai
            // 
            this.btnGuiLai.BorderRadius = 10;
            this.btnGuiLai.Font = new System.Drawing.Font("Segoe UI Semibold", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnGuiLai.ForeColor = System.Drawing.Color.White;
            this.btnGuiLai.Location = new System.Drawing.Point(161, 147);
            this.btnGuiLai.Name = "btnGuiLai";
            this.btnGuiLai.Size = new System.Drawing.Size(120, 40);
            this.btnGuiLai.TabIndex = 4;
            this.btnGuiLai.Text = "Gửi lại mã";
            this.btnGuiLai.Click += new System.EventHandler(this.btnGuiLai_Click);
            // 
            // btnHuy
            // 
            this.btnHuy.BorderRadius = 10;
            this.btnHuy.Font = new System.Drawing.Font("Segoe UI Semibold", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnHuy.ForeColor = System.Drawing.Color.White;
            this.btnHuy.Location = new System.Drawing.Point(291, 147);
            this.btnHuy.Name = "btnHuy";
            this.btnHuy.Size = new System.Drawing.Size(80, 40);
            this.btnHuy.TabIndex = 5;
            this.btnHuy.Text = "Hủy";
            this.btnHuy.Click += new System.EventHandler(this.btnHuy_Click);
            // 
            // lblDemNguoc
            // 
            this.lblDemNguoc.AutoSize = true;
            this.lblDemNguoc.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblDemNguoc.Location = new System.Drawing.Point(25, 186);
            this.lblDemNguoc.Name = "lblDemNguoc";
            this.lblDemNguoc.Size = new System.Drawing.Size(0, 20);
            this.lblDemNguoc.TabIndex = 6;
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
            this.Name = "XacNhanEmail";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Xác nhận email";
            this.Load += new System.EventHandler(this.XacNhanEmail_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }
    }
}
