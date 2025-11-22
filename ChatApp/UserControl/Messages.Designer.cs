namespace ChatApp
{
    partial class Messages
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
            this.picAvatar = new Guna.UI2.WinForms.Guna2CirclePictureBox();
            this.pnlMessages = new Guna.UI2.WinForms.Guna2Panel();
            this.flpText = new System.Windows.Forms.FlowLayoutPanel();
            this.lblMessage = new Guna.UI2.WinForms.Guna2HtmlLabel();
            this.lblTime = new Guna.UI2.WinForms.Guna2HtmlLabel();
            this.pnlBackground = new Guna.UI2.WinForms.Guna2Panel();
            ((System.ComponentModel.ISupportInitialize)(this.picAvatar)).BeginInit();
            this.pnlMessages.SuspendLayout();
            this.flpText.SuspendLayout();
            this.pnlBackground.SuspendLayout();
            this.SuspendLayout();
            // 
            // picAvatar
            // 
            this.picAvatar.ImageRotate = 0F;
            this.picAvatar.Location = new System.Drawing.Point(15, 14);
            this.picAvatar.Name = "picAvatar";
            this.picAvatar.ShadowDecoration.Mode = Guna.UI2.WinForms.Enums.ShadowMode.Circle;
            this.picAvatar.Size = new System.Drawing.Size(64, 64);
            this.picAvatar.TabIndex = 0;
            this.picAvatar.TabStop = false;
            // 
            // pnlMessages
            // 
            this.pnlMessages.AutoSize = true;
            this.pnlMessages.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.pnlMessages.BackColor = System.Drawing.Color.Transparent;
            this.pnlMessages.BorderColor = System.Drawing.Color.Black;
            this.pnlMessages.BorderRadius = 20;
            this.pnlMessages.Controls.Add(this.flpText);
            this.pnlMessages.FillColor = System.Drawing.Color.DodgerBlue;
            this.pnlMessages.Location = new System.Drawing.Point(85, 14);
            this.pnlMessages.Name = "pnlMessages";
            this.pnlMessages.ShadowDecoration.BorderRadius = 20;
            this.pnlMessages.Size = new System.Drawing.Size(90, 56);
            this.pnlMessages.TabIndex = 1;
            // 
            // flpText
            // 
            this.flpText.AutoSize = true;
            this.flpText.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.flpText.Controls.Add(this.lblMessage);
            this.flpText.Controls.Add(this.lblTime);
            this.flpText.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.flpText.Location = new System.Drawing.Point(11, 5);
            this.flpText.Name = "flpText";
            this.flpText.Size = new System.Drawing.Size(76, 48);
            this.flpText.TabIndex = 2;
            // 
            // lblMessage
            // 
            this.lblMessage.BackColor = System.Drawing.Color.Transparent;
            this.lblMessage.Location = new System.Drawing.Point(3, 3);
            this.lblMessage.Name = "lblMessage";
            this.lblMessage.Size = new System.Drawing.Size(70, 18);
            this.lblMessage.TabIndex = 0;
            this.lblMessage.Text = "{Message}";
            // 
            // lblTime
            // 
            this.lblTime.BackColor = System.Drawing.Color.Transparent;
            this.lblTime.Location = new System.Drawing.Point(3, 27);
            this.lblTime.Name = "lblTime";
            this.lblTime.Size = new System.Drawing.Size(44, 18);
            this.lblTime.TabIndex = 1;
            this.lblTime.Text = "{Time}";
            // 
            // pnlBackground
            // 
            this.pnlBackground.Controls.Add(this.picAvatar);
            this.pnlBackground.Controls.Add(this.pnlMessages);
            this.pnlBackground.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlBackground.FillColor = System.Drawing.Color.Transparent;
            this.pnlBackground.Location = new System.Drawing.Point(0, 0);
            this.pnlBackground.Name = "pnlBackground";
            this.pnlBackground.Size = new System.Drawing.Size(313, 92);
            this.pnlBackground.TabIndex = 2;
            // 
            // Messages
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.pnlBackground);
            this.Name = "Messages";
            this.Size = new System.Drawing.Size(313, 92);
            ((System.ComponentModel.ISupportInitialize)(this.picAvatar)).EndInit();
            this.pnlMessages.ResumeLayout(false);
            this.pnlMessages.PerformLayout();
            this.flpText.ResumeLayout(false);
            this.flpText.PerformLayout();
            this.pnlBackground.ResumeLayout(false);
            this.pnlBackground.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private Guna.UI2.WinForms.Guna2CirclePictureBox picAvatar;
        private Guna.UI2.WinForms.Guna2Panel pnlMessages;
        private Guna.UI2.WinForms.Guna2Panel pnlBackground;
        private Guna.UI2.WinForms.Guna2HtmlLabel lblTime;
        private Guna.UI2.WinForms.Guna2HtmlLabel lblMessage;
        private System.Windows.Forms.FlowLayoutPanel flpText;
    }
}
