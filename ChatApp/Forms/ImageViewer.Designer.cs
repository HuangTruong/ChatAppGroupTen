namespace ChatApp.Forms
{
    partial class ImageViewer
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ImageViewer));
            this.pictureImage = new Guna.UI2.WinForms.Guna2PictureBox();
            this.btnDownload = new Guna.UI2.WinForms.Guna2Button();
            this.panelBottom = new Guna.UI2.WinForms.Guna2Panel();
            this.pnlBackground = new Guna.UI2.WinForms.Guna2GradientPanel();
            ((System.ComponentModel.ISupportInitialize)(this.pictureImage)).BeginInit();
            this.panelBottom.SuspendLayout();
            this.pnlBackground.SuspendLayout();
            this.SuspendLayout();
            // 
            // pictureImage
            // 
            this.pictureImage.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pictureImage.ImageRotate = 0F;
            this.pictureImage.Location = new System.Drawing.Point(20, 20);
            this.pictureImage.Name = "pictureImage";
            this.pictureImage.Size = new System.Drawing.Size(722, 546);
            this.pictureImage.TabIndex = 0;
            this.pictureImage.TabStop = false;
            // 
            // btnDownload
            // 
            this.btnDownload.BorderRadius = 10;
            this.btnDownload.DisabledState.BorderColor = System.Drawing.Color.DarkGray;
            this.btnDownload.DisabledState.CustomBorderColor = System.Drawing.Color.DarkGray;
            this.btnDownload.DisabledState.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(169)))), ((int)(((byte)(169)))), ((int)(((byte)(169)))));
            this.btnDownload.DisabledState.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(141)))), ((int)(((byte)(141)))), ((int)(((byte)(141)))));
            this.btnDownload.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnDownload.ForeColor = System.Drawing.Color.White;
            this.btnDownload.Location = new System.Drawing.Point(311, 13);
            this.btnDownload.Name = "btnDownload";
            this.btnDownload.Size = new System.Drawing.Size(110, 45);
            this.btnDownload.TabIndex = 1;
            this.btnDownload.Text = "Tải Ảnh Về";
            // 
            // panelBottom
            // 
            this.panelBottom.BackColor = System.Drawing.Color.White;
            this.panelBottom.Controls.Add(this.btnDownload);
            this.panelBottom.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panelBottom.Location = new System.Drawing.Point(20, 566);
            this.panelBottom.Name = "panelBottom";
            this.panelBottom.Padding = new System.Windows.Forms.Padding(10);
            this.panelBottom.Size = new System.Drawing.Size(722, 67);
            this.panelBottom.TabIndex = 2;
            // 
            // pnlBackground
            // 
            this.pnlBackground.Controls.Add(this.pictureImage);
            this.pnlBackground.Controls.Add(this.panelBottom);
            this.pnlBackground.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlBackground.FillColor = System.Drawing.Color.Cyan;
            this.pnlBackground.FillColor2 = System.Drawing.Color.Blue;
            this.pnlBackground.GradientMode = System.Drawing.Drawing2D.LinearGradientMode.Vertical;
            this.pnlBackground.Location = new System.Drawing.Point(0, 0);
            this.pnlBackground.Name = "pnlBackground";
            this.pnlBackground.Padding = new System.Windows.Forms.Padding(20);
            this.pnlBackground.Size = new System.Drawing.Size(762, 653);
            this.pnlBackground.TabIndex = 3;
            // 
            // ImageViewer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(762, 653);
            this.Controls.Add(this.pnlBackground);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "ImageViewer";
            this.Text = "HÌNH ẢNH";
            ((System.ComponentModel.ISupportInitialize)(this.pictureImage)).EndInit();
            this.panelBottom.ResumeLayout(false);
            this.pnlBackground.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private Guna.UI2.WinForms.Guna2PictureBox pictureImage;
        private Guna.UI2.WinForms.Guna2Button btnDownload;
        private Guna.UI2.WinForms.Guna2Panel panelBottom;
        private Guna.UI2.WinForms.Guna2GradientPanel pnlBackground;
    }
}