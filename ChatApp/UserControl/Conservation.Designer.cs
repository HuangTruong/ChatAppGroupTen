namespace ChatApp
{
    partial class Conservation
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
            this.pnlBackground = new Guna.UI2.WinForms.Guna2Panel();
            this.picAvatar = new Guna.UI2.WinForms.Guna2CirclePictureBox();
            this.flpText = new System.Windows.Forms.FlowLayoutPanel();
            this.lblConservationName = new Guna.UI2.WinForms.Guna2HtmlLabel();
            this.lblLastMessage = new Guna.UI2.WinForms.Guna2HtmlLabel();
            this.pnlBackground.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picAvatar)).BeginInit();
            this.flpText.SuspendLayout();
            this.SuspendLayout();
            // 
            // pnlBackground
            // 
            this.pnlBackground.Controls.Add(this.flpText);
            this.pnlBackground.Controls.Add(this.picAvatar);
            this.pnlBackground.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlBackground.Location = new System.Drawing.Point(0, 0);
            this.pnlBackground.Name = "pnlBackground";
            this.pnlBackground.Size = new System.Drawing.Size(344, 75);
            this.pnlBackground.TabIndex = 0;
            // 
            // picAvatar
            // 
            this.picAvatar.ImageRotate = 0F;
            this.picAvatar.Location = new System.Drawing.Point(3, 3);
            this.picAvatar.Name = "picAvatar";
            this.picAvatar.ShadowDecoration.Mode = Guna.UI2.WinForms.Enums.ShadowMode.Circle;
            this.picAvatar.Size = new System.Drawing.Size(64, 64);
            this.picAvatar.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.picAvatar.TabIndex = 0;
            this.picAvatar.TabStop = false;
            // 
            // flpText
            // 
            this.flpText.AutoSize = true;
            this.flpText.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.flpText.Controls.Add(this.lblConservationName);
            this.flpText.Controls.Add(this.lblLastMessage);
            this.flpText.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.flpText.Location = new System.Drawing.Point(86, 19);
            this.flpText.Name = "flpText";
            this.flpText.Size = new System.Drawing.Size(104, 48);
            this.flpText.TabIndex = 1;
            this.flpText.WrapContents = false;
            // 
            // lblConservationName
            // 
            this.lblConservationName.BackColor = System.Drawing.Color.Transparent;
            this.lblConservationName.Location = new System.Drawing.Point(3, 3);
            this.lblConservationName.Name = "lblConservationName";
            this.lblConservationName.Size = new System.Drawing.Size(50, 18);
            this.lblConservationName.TabIndex = 0;
            this.lblConservationName.Text = "{Name}";
            // 
            // lblLastMessage
            // 
            this.lblLastMessage.BackColor = System.Drawing.Color.Transparent;
            this.lblLastMessage.Location = new System.Drawing.Point(3, 27);
            this.lblLastMessage.Name = "lblLastMessage";
            this.lblLastMessage.Size = new System.Drawing.Size(98, 18);
            this.lblLastMessage.TabIndex = 1;
            this.lblLastMessage.Text = "{Last Message}";
            // 
            // Conservation
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.pnlBackground);
            this.Name = "Conservation";
            this.Size = new System.Drawing.Size(344, 75);
            this.pnlBackground.ResumeLayout(false);
            this.pnlBackground.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picAvatar)).EndInit();
            this.flpText.ResumeLayout(false);
            this.flpText.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private Guna.UI2.WinForms.Guna2Panel pnlBackground;
        private Guna.UI2.WinForms.Guna2CirclePictureBox picAvatar;
        private System.Windows.Forms.FlowLayoutPanel flpText;
        private Guna.UI2.WinForms.Guna2HtmlLabel lblConservationName;
        private Guna.UI2.WinForms.Guna2HtmlLabel lblLastMessage;
    }
}
