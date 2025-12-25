namespace ChatApp.Controls
{
    partial class MessageBubbles
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
            this.flpBubble = new System.Windows.Forms.FlowLayoutPanel();
            this.lblDisplayName = new Guna.UI2.WinForms.Guna2HtmlLabel();
            this.pnlBubble = new Guna.UI2.WinForms.Guna2Panel();
            this.flpMessageContent = new System.Windows.Forms.FlowLayoutPanel();
            this.lblTime = new Guna.UI2.WinForms.Guna2HtmlLabel();
            this.picAvatar = new Guna.UI2.WinForms.Guna2CirclePictureBox();
            this.pnlBackGround = new Guna.UI2.WinForms.Guna2Panel();
            this.pnlAvatar = new Guna.UI2.WinForms.Guna2Panel();
            this.flpBubble.SuspendLayout();
            this.pnlBubble.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picAvatar)).BeginInit();
            this.pnlBackGround.SuspendLayout();
            this.pnlAvatar.SuspendLayout();
            this.SuspendLayout();
            // 
            // flpBubble
            // 
            this.flpBubble.AutoSize = true;
            this.flpBubble.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.flpBubble.Controls.Add(this.lblDisplayName);
            this.flpBubble.Controls.Add(this.pnlBubble);
            this.flpBubble.Controls.Add(this.lblTime);
            this.flpBubble.Dock = System.Windows.Forms.DockStyle.Left;
            this.flpBubble.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.flpBubble.Location = new System.Drawing.Point(52, 0);
            this.flpBubble.Margin = new System.Windows.Forms.Padding(2);
            this.flpBubble.Name = "flpBubble";
            this.flpBubble.Size = new System.Drawing.Size(77, 76);
            this.flpBubble.TabIndex = 1;
            // 
            // lblDisplayName
            // 
            this.lblDisplayName.BackColor = System.Drawing.Color.Transparent;
            this.lblDisplayName.Location = new System.Drawing.Point(2, 2);
            this.lblDisplayName.Margin = new System.Windows.Forms.Padding(2);
            this.lblDisplayName.Name = "lblDisplayName";
            this.lblDisplayName.Size = new System.Drawing.Size(73, 15);
            this.lblDisplayName.TabIndex = 0;
            this.lblDisplayName.Text = "{DisplayName}";
            // 
            // pnlBubble
            // 
            this.pnlBubble.AutoSize = true;
            this.pnlBubble.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.pnlBubble.Controls.Add(this.flpMessageContent);
            this.pnlBubble.Location = new System.Drawing.Point(2, 21);
            this.pnlBubble.Margin = new System.Windows.Forms.Padding(2);
            this.pnlBubble.Name = "pnlBubble";
            this.pnlBubble.Size = new System.Drawing.Size(0, 0);
            this.pnlBubble.TabIndex = 1;
            // 
            // flpMessageContent
            // 
            this.flpMessageContent.AutoSize = true;
            this.flpMessageContent.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.flpMessageContent.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flpMessageContent.Location = new System.Drawing.Point(0, 0);
            this.flpMessageContent.Name = "flpMessageContent";
            this.flpMessageContent.Size = new System.Drawing.Size(0, 0);
            this.flpMessageContent.TabIndex = 1;
            // 
            // lblTime
            // 
            this.lblTime.BackColor = System.Drawing.Color.Transparent;
            this.lblTime.Location = new System.Drawing.Point(2, 25);
            this.lblTime.Margin = new System.Windows.Forms.Padding(2);
            this.lblTime.Name = "lblTime";
            this.lblTime.Size = new System.Drawing.Size(34, 15);
            this.lblTime.TabIndex = 2;
            this.lblTime.Text = "{Time}";
            // 
            // picAvatar
            // 
            this.picAvatar.ImageRotate = 0F;
            this.picAvatar.Location = new System.Drawing.Point(2, 2);
            this.picAvatar.Margin = new System.Windows.Forms.Padding(2);
            this.picAvatar.Name = "picAvatar";
            this.picAvatar.ShadowDecoration.Mode = Guna.UI2.WinForms.Enums.ShadowMode.Circle;
            this.picAvatar.Size = new System.Drawing.Size(48, 52);
            this.picAvatar.TabIndex = 0;
            this.picAvatar.TabStop = false;
            // 
            // pnlBackGround
            // 
            this.pnlBackGround.AutoSize = true;
            this.pnlBackGround.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.pnlBackGround.Controls.Add(this.flpBubble);
            this.pnlBackGround.Controls.Add(this.pnlAvatar);
            this.pnlBackGround.Dock = System.Windows.Forms.DockStyle.Left;
            this.pnlBackGround.Location = new System.Drawing.Point(0, 0);
            this.pnlBackGround.Margin = new System.Windows.Forms.Padding(2);
            this.pnlBackGround.Name = "pnlBackGround";
            this.pnlBackGround.Size = new System.Drawing.Size(129, 76);
            this.pnlBackGround.TabIndex = 0;
            // 
            // pnlAvatar
            // 
            this.pnlAvatar.AutoSize = true;
            this.pnlAvatar.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.pnlAvatar.Controls.Add(this.picAvatar);
            this.pnlAvatar.Dock = System.Windows.Forms.DockStyle.Left;
            this.pnlAvatar.Location = new System.Drawing.Point(0, 0);
            this.pnlAvatar.Margin = new System.Windows.Forms.Padding(2);
            this.pnlAvatar.Name = "pnlAvatar";
            this.pnlAvatar.Size = new System.Drawing.Size(52, 76);
            this.pnlAvatar.TabIndex = 2;
            // 
            // MessageBubbles
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.pnlBackGround);
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "MessageBubbles";
            this.Size = new System.Drawing.Size(296, 76);
            this.flpBubble.ResumeLayout(false);
            this.flpBubble.PerformLayout();
            this.pnlBubble.ResumeLayout(false);
            this.pnlBubble.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picAvatar)).EndInit();
            this.pnlBackGround.ResumeLayout(false);
            this.pnlBackGround.PerformLayout();
            this.pnlAvatar.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private Guna.UI2.WinForms.Guna2CirclePictureBox picAvatar;
        private System.Windows.Forms.FlowLayoutPanel flpBubble;
        private Guna.UI2.WinForms.Guna2HtmlLabel lblDisplayName;
        private Guna.UI2.WinForms.Guna2Panel pnlBubble;
        private Guna.UI2.WinForms.Guna2HtmlLabel lblTime;
        private Guna.UI2.WinForms.Guna2Panel pnlBackGround;
        private Guna.UI2.WinForms.Guna2Panel pnlAvatar;
        private System.Windows.Forms.FlowLayoutPanel flpMessageContent;
    }
}
