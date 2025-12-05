namespace ChatApp.Controls
{
    partial class FriendRequestItem
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FriendRequestItem));
            this.pbAvatar = new Guna.UI2.WinForms.Guna2CirclePictureBox();
            this.lblUserName = new System.Windows.Forms.Label();
            this.pbReject = new System.Windows.Forms.PictureBox();
            this.pbAccept = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.pbAvatar)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbReject)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbAccept)).BeginInit();
            this.SuspendLayout();
            // 
            // pbAvatar
            // 
            this.pbAvatar.ImageRotate = 0F;
            this.pbAvatar.Location = new System.Drawing.Point(3, 3);
            this.pbAvatar.Name = "pbAvatar";
            this.pbAvatar.ShadowDecoration.Mode = Guna.UI2.WinForms.Enums.ShadowMode.Circle;
            this.pbAvatar.Size = new System.Drawing.Size(64, 64);
            this.pbAvatar.TabIndex = 0;
            this.pbAvatar.TabStop = false;
            // 
            // lblUserName
            // 
            this.lblUserName.AutoSize = true;
            this.lblUserName.Location = new System.Drawing.Point(73, 34);
            this.lblUserName.Name = "lblUserName";
            this.lblUserName.Size = new System.Drawing.Size(35, 13);
            this.lblUserName.TabIndex = 1;
            this.lblUserName.Text = "label1";
            // 
            // pbReject
            // 
            this.pbReject.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("pbReject.BackgroundImage")));
            this.pbReject.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.pbReject.Location = new System.Drawing.Point(230, 21);
            this.pbReject.Name = "pbReject";
            this.pbReject.Size = new System.Drawing.Size(53, 46);
            this.pbReject.TabIndex = 2;
            this.pbReject.TabStop = false;
            // 
            // pbAccept
            // 
            this.pbAccept.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("pbAccept.BackgroundImage")));
            this.pbAccept.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.pbAccept.Location = new System.Drawing.Point(171, 21);
            this.pbAccept.Name = "pbAccept";
            this.pbAccept.Size = new System.Drawing.Size(53, 46);
            this.pbAccept.TabIndex = 3;
            this.pbAccept.TabStop = false;
            // 
            // FriendRequestItem
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.pbAccept);
            this.Controls.Add(this.pbReject);
            this.Controls.Add(this.lblUserName);
            this.Controls.Add(this.pbAvatar);
            this.Name = "FriendRequestItem";
            this.Size = new System.Drawing.Size(286, 79);
            ((System.ComponentModel.ISupportInitialize)(this.pbAvatar)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbReject)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbAccept)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private Guna.UI2.WinForms.Guna2CirclePictureBox pbAvatar;
        private System.Windows.Forms.Label lblUserName;
        public System.Windows.Forms.PictureBox pbReject;
        public System.Windows.Forms.PictureBox pbAccept;
    }
}
