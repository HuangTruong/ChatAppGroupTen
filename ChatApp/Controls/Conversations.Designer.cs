namespace ChatApp.Controls
{
    partial class Conversations
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
            this.lblDisplayName = new Guna.UI2.WinForms.Guna2HtmlLabel();
            this.picCancelRequest = new System.Windows.Forms.PictureBox();
            this.picAvatar = new Guna.UI2.WinForms.Guna2CirclePictureBox();
            this.pnlBackground.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picCancelRequest)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.picAvatar)).BeginInit();
            this.SuspendLayout();
            // 
            // pnlBackground
            // 
            this.pnlBackground.BackColor = System.Drawing.Color.DodgerBlue;
            this.pnlBackground.Controls.Add(this.lblDisplayName);
            this.pnlBackground.Controls.Add(this.picCancelRequest);
            this.pnlBackground.Controls.Add(this.picAvatar);
            this.pnlBackground.Cursor = System.Windows.Forms.Cursors.Hand;
            this.pnlBackground.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlBackground.Location = new System.Drawing.Point(5, 5);
            this.pnlBackground.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.pnlBackground.Name = "pnlBackground";
            this.pnlBackground.Padding = new System.Windows.Forms.Padding(5);
            this.pnlBackground.Size = new System.Drawing.Size(291, 66);
            this.pnlBackground.TabIndex = 0;
            // 
            // lblDisplayName
            // 
            this.lblDisplayName.AutoSize = false;
            this.lblDisplayName.BackColor = System.Drawing.Color.Transparent;
            this.lblDisplayName.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblDisplayName.Location = new System.Drawing.Point(61, 5);
            this.lblDisplayName.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.lblDisplayName.Name = "lblDisplayName";
            this.lblDisplayName.Padding = new System.Windows.Forms.Padding(5, 0, 0, 0);
            this.lblDisplayName.Size = new System.Drawing.Size(169, 56);
            this.lblDisplayName.TabIndex = 1;
            this.lblDisplayName.Text = "guna2HtmlLabel1";
            this.lblDisplayName.TextAlignment = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // picCancelRequest
            // 
            this.picCancelRequest.Cursor = System.Windows.Forms.Cursors.Hand;
            this.picCancelRequest.Dock = System.Windows.Forms.DockStyle.Right;
            this.picCancelRequest.Image = global::ChatApp.Properties.Resources.Cross;
            this.picCancelRequest.Location = new System.Drawing.Point(230, 5);
            this.picCancelRequest.Margin = new System.Windows.Forms.Padding(4);
            this.picCancelRequest.Name = "picCancelRequest";
            this.picCancelRequest.Size = new System.Drawing.Size(56, 56);
            this.picCancelRequest.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.picCancelRequest.TabIndex = 2;
            this.picCancelRequest.TabStop = false;
            // 
            // picAvatar
            // 
            this.picAvatar.Dock = System.Windows.Forms.DockStyle.Left;
            this.picAvatar.ImageRotate = 0F;
            this.picAvatar.Location = new System.Drawing.Point(5, 5);
            this.picAvatar.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.picAvatar.Name = "picAvatar";
            this.picAvatar.ShadowDecoration.Mode = Guna.UI2.WinForms.Enums.ShadowMode.Circle;
            this.picAvatar.Size = new System.Drawing.Size(56, 56);
            this.picAvatar.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.picAvatar.TabIndex = 0;
            this.picAvatar.TabStop = false;
            // 
            // Conversations
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.pnlBackground);
            this.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.Name = "Conversations";
            this.Padding = new System.Windows.Forms.Padding(5);
            this.Size = new System.Drawing.Size(301, 76);
            this.Tag = "Conversations";
            this.pnlBackground.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.picCancelRequest)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.picAvatar)).EndInit();
            this.ResumeLayout(false);

        }
        #endregion

        private Guna.UI2.WinForms.Guna2Panel pnlBackground;
        private Guna.UI2.WinForms.Guna2CirclePictureBox picAvatar;
        private Guna.UI2.WinForms.Guna2HtmlLabel lblDisplayName;
        public System.Windows.Forms.PictureBox picCancelRequest;
    }
}
