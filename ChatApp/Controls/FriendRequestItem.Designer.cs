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
            this.pbAccept = new System.Windows.Forms.PictureBox();
            this.flpUserName = new System.Windows.Forms.FlowLayoutPanel();
            this.lblUserName = new Guna.UI2.WinForms.Guna2HtmlLabel();
            this.pnlFrame = new Guna.UI2.WinForms.Guna2Panel();
            this.pnlAction = new Guna.UI2.WinForms.Guna2Panel();
            this.pnlPaddingAction = new Guna.UI2.WinForms.Guna2Panel();
            this.pbReject = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.pbAvatar)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbAccept)).BeginInit();
            this.flpUserName.SuspendLayout();
            this.pnlFrame.SuspendLayout();
            this.pnlAction.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbReject)).BeginInit();
            this.SuspendLayout();
            // 
            // pbAvatar
            // 
            this.pbAvatar.Dock = System.Windows.Forms.DockStyle.Left;
            this.pbAvatar.ImageRotate = 0F;
            this.pbAvatar.Location = new System.Drawing.Point(5, 5);
            this.pbAvatar.Margin = new System.Windows.Forms.Padding(4);
            this.pbAvatar.Name = "pbAvatar";
            this.pbAvatar.ShadowDecoration.Mode = Guna.UI2.WinForms.Enums.ShadowMode.Circle;
            this.pbAvatar.Size = new System.Drawing.Size(69, 69);
            this.pbAvatar.TabIndex = 0;
            this.pbAvatar.TabStop = false;
            // 
            // pbAccept
            // 
            this.pbAccept.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("pbAccept.BackgroundImage")));
            this.pbAccept.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.pbAccept.Dock = System.Windows.Forms.DockStyle.Right;
            this.pbAccept.Location = new System.Drawing.Point(24, 0);
            this.pbAccept.Margin = new System.Windows.Forms.Padding(4);
            this.pbAccept.Name = "pbAccept";
            this.pbAccept.Padding = new System.Windows.Forms.Padding(3);
            this.pbAccept.Size = new System.Drawing.Size(57, 69);
            this.pbAccept.TabIndex = 3;
            this.pbAccept.TabStop = false;
            // 
            // flpUserName
            // 
            this.flpUserName.AutoSize = true;
            this.flpUserName.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.flpUserName.Controls.Add(this.lblUserName);
            this.flpUserName.Dock = System.Windows.Forms.DockStyle.Left;
            this.flpUserName.Location = new System.Drawing.Point(74, 5);
            this.flpUserName.Name = "flpUserName";
            this.flpUserName.Padding = new System.Windows.Forms.Padding(10);
            this.flpUserName.Size = new System.Drawing.Size(121, 69);
            this.flpUserName.TabIndex = 4;
            // 
            // lblUserName
            // 
            this.lblUserName.BackColor = System.Drawing.Color.Transparent;
            this.lblUserName.Dock = System.Windows.Forms.DockStyle.Left;
            this.lblUserName.Font = new System.Drawing.Font("Segoe UI Semibold", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblUserName.Location = new System.Drawing.Point(13, 13);
            this.lblUserName.Name = "lblUserName";
            this.lblUserName.Size = new System.Drawing.Size(95, 25);
            this.lblUserName.TabIndex = 2;
            this.lblUserName.Text = "{UserName}";
            // 
            // pnlFrame
            // 
            this.pnlFrame.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(39)))), ((int)(((byte)(164)))), ((int)(((byte)(242)))));
            this.pnlFrame.Controls.Add(this.pnlAction);
            this.pnlFrame.Controls.Add(this.flpUserName);
            this.pnlFrame.Controls.Add(this.pbAvatar);
            this.pnlFrame.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlFrame.Location = new System.Drawing.Point(10, 10);
            this.pnlFrame.Name = "pnlFrame";
            this.pnlFrame.Padding = new System.Windows.Forms.Padding(5);
            this.pnlFrame.Size = new System.Drawing.Size(382, 79);
            this.pnlFrame.TabIndex = 6;
            // 
            // pnlAction
            // 
            this.pnlAction.Controls.Add(this.pbAccept);
            this.pnlAction.Controls.Add(this.pnlPaddingAction);
            this.pnlAction.Controls.Add(this.pbReject);
            this.pnlAction.Dock = System.Windows.Forms.DockStyle.Right;
            this.pnlAction.Location = new System.Drawing.Point(229, 5);
            this.pnlAction.Name = "pnlAction";
            this.pnlAction.Size = new System.Drawing.Size(148, 69);
            this.pnlAction.TabIndex = 6;
            // 
            // pnlPaddingAction
            // 
            this.pnlPaddingAction.Dock = System.Windows.Forms.DockStyle.Right;
            this.pnlPaddingAction.Location = new System.Drawing.Point(81, 0);
            this.pnlPaddingAction.Name = "pnlPaddingAction";
            this.pnlPaddingAction.Size = new System.Drawing.Size(10, 69);
            this.pnlPaddingAction.TabIndex = 7;
            // 
            // pbReject
            // 
            this.pbReject.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("pbReject.BackgroundImage")));
            this.pbReject.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.pbReject.Dock = System.Windows.Forms.DockStyle.Right;
            this.pbReject.Location = new System.Drawing.Point(91, 0);
            this.pbReject.Margin = new System.Windows.Forms.Padding(4);
            this.pbReject.Name = "pbReject";
            this.pbReject.Padding = new System.Windows.Forms.Padding(3);
            this.pbReject.Size = new System.Drawing.Size(57, 69);
            this.pbReject.TabIndex = 2;
            this.pbReject.TabStop = false;
            // 
            // FriendRequestItem
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.pnlFrame);
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "FriendRequestItem";
            this.Padding = new System.Windows.Forms.Padding(10, 10, 10, 0);
            this.Size = new System.Drawing.Size(402, 89);
            ((System.ComponentModel.ISupportInitialize)(this.pbAvatar)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbAccept)).EndInit();
            this.flpUserName.ResumeLayout(false);
            this.flpUserName.PerformLayout();
            this.pnlFrame.ResumeLayout(false);
            this.pnlFrame.PerformLayout();
            this.pnlAction.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pbReject)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private Guna.UI2.WinForms.Guna2CirclePictureBox pbAvatar;
        public System.Windows.Forms.PictureBox pbAccept;
        private System.Windows.Forms.FlowLayoutPanel flpUserName;
        private Guna.UI2.WinForms.Guna2HtmlLabel lblUserName;
        private Guna.UI2.WinForms.Guna2Panel pnlFrame;
        private Guna.UI2.WinForms.Guna2Panel pnlAction;
        public System.Windows.Forms.PictureBox pbReject;
        private Guna.UI2.WinForms.Guna2Panel pnlPaddingAction;
    }
}
