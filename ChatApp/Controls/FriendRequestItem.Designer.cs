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
            this.lblUserName = new Guna.UI2.WinForms.Guna2HtmlLabel();
            this.pnlFrame = new Guna.UI2.WinForms.Guna2Panel();
            this.pbAccept = new System.Windows.Forms.PictureBox();
            this.pbReject = new System.Windows.Forms.PictureBox();
            this.pbAvatar = new Guna.UI2.WinForms.Guna2CirclePictureBox();
            this.pnlFrame.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbAccept)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbReject)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbAvatar)).BeginInit();
            this.SuspendLayout();
            // 
            // lblUserName
            // 
            this.lblUserName.AutoSize = false;
            this.lblUserName.BackColor = System.Drawing.Color.Transparent;
            this.lblUserName.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblUserName.Font = new System.Drawing.Font("Segoe UI Semibold", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblUserName.Location = new System.Drawing.Point(74, 5);
            this.lblUserName.Name = "lblUserName";
            this.lblUserName.Padding = new System.Windows.Forms.Padding(5, 0, 0, 0);
            this.lblUserName.Size = new System.Drawing.Size(132, 69);
            this.lblUserName.TabIndex = 2;
            this.lblUserName.Text = "{UserName}";
            this.lblUserName.TextAlignment = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // pnlFrame
            // 
            this.pnlFrame.BackColor = System.Drawing.Color.Transparent;
            this.pnlFrame.BorderRadius = 10;
            this.pnlFrame.Controls.Add(this.lblUserName);
            this.pnlFrame.Controls.Add(this.pbAccept);
            this.pnlFrame.Controls.Add(this.pbReject);
            this.pnlFrame.Controls.Add(this.pbAvatar);
            this.pnlFrame.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlFrame.FillColor = System.Drawing.Color.White;
            this.pnlFrame.Location = new System.Drawing.Point(10, 10);
            this.pnlFrame.Name = "pnlFrame";
            this.pnlFrame.Padding = new System.Windows.Forms.Padding(5);
            this.pnlFrame.Size = new System.Drawing.Size(325, 79);
            this.pnlFrame.TabIndex = 6;
            // 
            // pbAccept
            // 
            this.pbAccept.BackgroundImage = global::ChatApp.Properties.Resources.Tick;
            this.pbAccept.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.pbAccept.Dock = System.Windows.Forms.DockStyle.Right;
            this.pbAccept.Location = new System.Drawing.Point(206, 5);
            this.pbAccept.Margin = new System.Windows.Forms.Padding(4);
            this.pbAccept.Name = "pbAccept";
            this.pbAccept.Padding = new System.Windows.Forms.Padding(3);
            this.pbAccept.Size = new System.Drawing.Size(57, 69);
            this.pbAccept.TabIndex = 3;
            this.pbAccept.TabStop = false;
            // 
            // pbReject
            // 
            this.pbReject.BackgroundImage = global::ChatApp.Properties.Resources.Cross;
            this.pbReject.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.pbReject.Dock = System.Windows.Forms.DockStyle.Right;
            this.pbReject.Location = new System.Drawing.Point(263, 5);
            this.pbReject.Margin = new System.Windows.Forms.Padding(4);
            this.pbReject.Name = "pbReject";
            this.pbReject.Padding = new System.Windows.Forms.Padding(3);
            this.pbReject.Size = new System.Drawing.Size(57, 69);
            this.pbReject.TabIndex = 2;
            this.pbReject.TabStop = false;
            // 
            // pbAvatar
            // 
            this.pbAvatar.Dock = System.Windows.Forms.DockStyle.Left;
            this.pbAvatar.FillColor = System.Drawing.Color.Silver;
            this.pbAvatar.ImageRotate = 0F;
            this.pbAvatar.Location = new System.Drawing.Point(5, 5);
            this.pbAvatar.Margin = new System.Windows.Forms.Padding(4);
            this.pbAvatar.Name = "pbAvatar";
            this.pbAvatar.ShadowDecoration.Mode = Guna.UI2.WinForms.Enums.ShadowMode.Circle;
            this.pbAvatar.Size = new System.Drawing.Size(69, 69);
            this.pbAvatar.TabIndex = 0;
            this.pbAvatar.TabStop = false;
            // 
            // FriendRequestItem
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Transparent;
            this.Controls.Add(this.pnlFrame);
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "FriendRequestItem";
            this.Padding = new System.Windows.Forms.Padding(10, 10, 10, 0);
            this.Size = new System.Drawing.Size(345, 89);
            this.pnlFrame.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pbAccept)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbReject)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbAvatar)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private Guna.UI2.WinForms.Guna2CirclePictureBox pbAvatar;
        public System.Windows.Forms.PictureBox pbAccept;
        private Guna.UI2.WinForms.Guna2HtmlLabel lblUserName;
        private Guna.UI2.WinForms.Guna2Panel pnlFrame;
        public System.Windows.Forms.PictureBox pbReject;
    }
}
