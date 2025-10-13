namespace ChatApp
{
    partial class TinNhanCuaToi
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.picAnhDaiDien = new Guna.UI2.WinForms.Guna2CirclePictureBox();
            this.pnlKhungTinNhan = new Guna.UI2.WinForms.Guna2Panel();
            this.lblTinNhan = new Guna.UI2.WinForms.Guna2HtmlLabel();
            ((System.ComponentModel.ISupportInitialize)(this.picAnhDaiDien)).BeginInit();
            this.pnlKhungTinNhan.SuspendLayout();
            this.SuspendLayout();
            // 
            // picAnhDaiDien
            // 
            this.picAnhDaiDien.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.picAnhDaiDien.ImageRotate = 0F;
            this.picAnhDaiDien.Location = new System.Drawing.Point(345, 10);
            this.picAnhDaiDien.Name = "picAnhDaiDien";
            this.picAnhDaiDien.ShadowDecoration.Mode = Guna.UI2.WinForms.Enums.ShadowMode.Circle;
            this.picAnhDaiDien.Size = new System.Drawing.Size(45, 45);
            this.picAnhDaiDien.TabIndex = 1;
            this.picAnhDaiDien.TabStop = false;
            // 
            // pnlKhungTinNhan
            // 
            this.pnlKhungTinNhan.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.pnlKhungTinNhan.AutoSize = true;
            this.pnlKhungTinNhan.BorderRadius = 12;
            this.pnlKhungTinNhan.Controls.Add(this.lblTinNhan);
            this.pnlKhungTinNhan.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(255)))));
            this.pnlKhungTinNhan.Location = new System.Drawing.Point(41, 19);
            this.pnlKhungTinNhan.Name = "pnlKhungTinNhan";
            this.pnlKhungTinNhan.Size = new System.Drawing.Size(298, 47);
            this.pnlKhungTinNhan.TabIndex = 2;
            // 
            // lblTinNhan
            // 
            this.lblTinNhan.BackColor = System.Drawing.Color.Transparent;
            this.lblTinNhan.Font = new System.Drawing.Font("Segoe UI", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTinNhan.ForeColor = System.Drawing.Color.White;
            this.lblTinNhan.Location = new System.Drawing.Point(135, 9);
            this.lblTinNhan.MaximumSize = new System.Drawing.Size(250, 0);
            this.lblTinNhan.Name = "lblTinNhan";
            this.lblTinNhan.Padding = new System.Windows.Forms.Padding(8, 5, 8, 5);
            this.lblTinNhan.Size = new System.Drawing.Size(153, 35);
            this.lblTinNhan.TabIndex = 0;
            this.lblTinNhan.Text = "guna2HtmlLabel1";
            // 
            // TinNhanCuaToi
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Transparent;
            this.Controls.Add(this.pnlKhungTinNhan);
            this.Controls.Add(this.picAnhDaiDien);
            this.Name = "TinNhanCuaToi";
            this.Size = new System.Drawing.Size(400, 65);
            ((System.ComponentModel.ISupportInitialize)(this.picAnhDaiDien)).EndInit();
            this.pnlKhungTinNhan.ResumeLayout(false);
            this.pnlKhungTinNhan.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private Guna.UI2.WinForms.Guna2CirclePictureBox picAnhDaiDien;
        private Guna.UI2.WinForms.Guna2Panel pnlKhungTinNhan;
        private Guna.UI2.WinForms.Guna2HtmlLabel lblTinNhan;
    }
}
