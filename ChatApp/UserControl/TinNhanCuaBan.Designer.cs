namespace ChatApp
{
    partial class TinNhanCuaBan
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
            this.guna2CirclePictureBox1 = new Guna.UI2.WinForms.Guna2CirclePictureBox();
            this.pnlKhungTinNhan = new Guna.UI2.WinForms.Guna2Panel();
            this.lblTinNhan = new Guna.UI2.WinForms.Guna2HtmlLabel();
            ((System.ComponentModel.ISupportInitialize)(this.guna2CirclePictureBox1)).BeginInit();
            this.pnlKhungTinNhan.SuspendLayout();
            this.SuspendLayout();
            // 
            // guna2CirclePictureBox1
            // 
            this.guna2CirclePictureBox1.ImageRotate = 0F;
            this.guna2CirclePictureBox1.Location = new System.Drawing.Point(10, 10);
            this.guna2CirclePictureBox1.Name = "guna2CirclePictureBox1";
            this.guna2CirclePictureBox1.ShadowDecoration.Mode = Guna.UI2.WinForms.Enums.ShadowMode.Circle;
            this.guna2CirclePictureBox1.Size = new System.Drawing.Size(45, 45);
            this.guna2CirclePictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.guna2CirclePictureBox1.TabIndex = 0;
            this.guna2CirclePictureBox1.TabStop = false;
            // 
            // pnlKhungTinNhan
            // 
            this.pnlKhungTinNhan.AutoSize = true;
            this.pnlKhungTinNhan.BackColor = System.Drawing.Color.Transparent;
            this.pnlKhungTinNhan.BorderRadius = 12;
            this.pnlKhungTinNhan.Controls.Add(this.lblTinNhan);
            this.pnlKhungTinNhan.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(235)))), ((int)(((byte)(235)))), ((int)(((byte)(235)))));
            this.pnlKhungTinNhan.Location = new System.Drawing.Point(60, 10);
            this.pnlKhungTinNhan.Name = "pnlKhungTinNhan";
            this.pnlKhungTinNhan.Size = new System.Drawing.Size(319, 41);
            this.pnlKhungTinNhan.TabIndex = 1;
            // 
            // lblTinNhan
            // 
            this.lblTinNhan.BackColor = System.Drawing.Color.Transparent;
            this.lblTinNhan.Font = new System.Drawing.Font("Segoe UI", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTinNhan.ForeColor = System.Drawing.Color.Black;
            this.lblTinNhan.Location = new System.Drawing.Point(28, 3);
            this.lblTinNhan.MaximumSize = new System.Drawing.Size(250, 0);
            this.lblTinNhan.Name = "lblTinNhan";
            this.lblTinNhan.Padding = new System.Windows.Forms.Padding(8, 5, 8, 5);
            this.lblTinNhan.Size = new System.Drawing.Size(153, 35);
            this.lblTinNhan.TabIndex = 0;
            this.lblTinNhan.Text = "guna2HtmlLabel1";
            // 
            // TinNhanCuaBan
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Transparent;
            this.Controls.Add(this.pnlKhungTinNhan);
            this.Controls.Add(this.guna2CirclePictureBox1);
            this.Name = "TinNhanCuaBan";
            this.Size = new System.Drawing.Size(400, 65);
            ((System.ComponentModel.ISupportInitialize)(this.guna2CirclePictureBox1)).EndInit();
            this.pnlKhungTinNhan.ResumeLayout(false);
            this.pnlKhungTinNhan.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private Guna.UI2.WinForms.Guna2CirclePictureBox guna2CirclePictureBox1;
        private Guna.UI2.WinForms.Guna2Panel pnlKhungTinNhan;
        private Guna.UI2.WinForms.Guna2HtmlLabel lblTinNhan;
    }
}
