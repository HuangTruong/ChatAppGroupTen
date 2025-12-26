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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Conversations));
            this.pnlBackground = new Guna.UI2.WinForms.Guna2Panel();
            this.picCancelRequest = new System.Windows.Forms.PictureBox();
            this.lblDisplayName = new Guna.UI2.WinForms.Guna2HtmlLabel();
            this.picAvatar = new Guna.UI2.WinForms.Guna2CirclePictureBox();
            this.pnlBackground.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picCancelRequest)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.picAvatar)).BeginInit();
            this.SuspendLayout();
            // 
            // pnlBackground
            // 
            this.pnlBackground.BackColor = System.Drawing.Color.DodgerBlue;
            this.pnlBackground.Controls.Add(this.picCancelRequest);
            this.pnlBackground.Controls.Add(this.lblDisplayName);
            this.pnlBackground.Controls.Add(this.picAvatar);
            this.pnlBackground.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlBackground.Location = new System.Drawing.Point(4, 4);
            this.pnlBackground.Margin = new System.Windows.Forms.Padding(2);
            this.pnlBackground.Name = "pnlBackground";
            this.pnlBackground.Padding = new System.Windows.Forms.Padding(4);
            this.pnlBackground.Size = new System.Drawing.Size(1043, 54);
            this.pnlBackground.TabIndex = 0;
            // 
            // picCancelRequest
            // 
            this.picCancelRequest.Dock = System.Windows.Forms.DockStyle.Right;
            this.picCancelRequest.Image = ((System.Drawing.Image)(resources.GetObject("picCancelRequest.Image")));
            this.picCancelRequest.Location = new System.Drawing.Point(1008, 4);
            this.picCancelRequest.Name = "picCancelRequest";
            this.picCancelRequest.Size = new System.Drawing.Size(31, 46);
            this.picCancelRequest.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.picCancelRequest.TabIndex = 2;
            this.picCancelRequest.TabStop = false;
            // 
            // lblDisplayName
            // 
            this.lblDisplayName.AutoSize = false;
            this.lblDisplayName.BackColor = System.Drawing.Color.Transparent;
            this.lblDisplayName.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblDisplayName.Location = new System.Drawing.Point(46, 4);
            this.lblDisplayName.Margin = new System.Windows.Forms.Padding(2);
            this.lblDisplayName.Name = "lblDisplayName";
            this.lblDisplayName.Padding = new System.Windows.Forms.Padding(4, 0, 0, 0);
            this.lblDisplayName.Size = new System.Drawing.Size(993, 46);
            this.lblDisplayName.TabIndex = 1;
            this.lblDisplayName.Text = "guna2HtmlLabel1";
            this.lblDisplayName.TextAlignment = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // picAvatar
            // 
            this.picAvatar.Dock = System.Windows.Forms.DockStyle.Left;
            this.picAvatar.ImageRotate = 0F;
            this.picAvatar.Location = new System.Drawing.Point(4, 4);
            this.picAvatar.Margin = new System.Windows.Forms.Padding(2);
            this.picAvatar.Name = "picAvatar";
            this.picAvatar.ShadowDecoration.Mode = Guna.UI2.WinForms.Enums.ShadowMode.Circle;
            this.picAvatar.Size = new System.Drawing.Size(42, 46);
            this.picAvatar.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.picAvatar.TabIndex = 0;
            this.picAvatar.TabStop = false;
            // 
            // Conversations
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.pnlBackground);
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "Conversations";
            this.Padding = new System.Windows.Forms.Padding(4);
            this.Dock = System.Windows.Forms.DockStyle.Top;
            this.Size = new System.Drawing.Size(1051, 62);
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
