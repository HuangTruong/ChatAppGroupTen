namespace ChatApp.Controls
{
    partial class UserListItem
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
            this.pbAvatar = new Guna.UI2.WinForms.Guna2CirclePictureBox();
            this.pbAction = new System.Windows.Forms.PictureBox();
            this.pnlFrame.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbAvatar)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbAction)).BeginInit();
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
            this.lblUserName.Size = new System.Drawing.Size(140, 69);
            this.lblUserName.TabIndex = 2;
            this.lblUserName.Text = "{UserName}";
            this.lblUserName.TextAlignment = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // pnlFrame
            // 
            this.pnlFrame.BackColor = System.Drawing.Color.Transparent;
            this.pnlFrame.BorderRadius = 10;
            this.pnlFrame.Controls.Add(this.lblUserName);
            this.pnlFrame.Controls.Add(this.pbAvatar);
            this.pnlFrame.Controls.Add(this.pbAction);
            this.pnlFrame.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlFrame.FillColor = System.Drawing.Color.White;
            this.pnlFrame.Location = new System.Drawing.Point(10, 10);
            this.pnlFrame.Name = "pnlFrame";
            this.pnlFrame.Padding = new System.Windows.Forms.Padding(5);
            this.pnlFrame.Size = new System.Drawing.Size(276, 79);
            this.pnlFrame.TabIndex = 4;
            // 
            // pbAvatar
            // 
            this.pbAvatar.BackColor = System.Drawing.Color.Transparent;
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
            // pbAction
            // 
            this.pbAction.Cursor = System.Windows.Forms.Cursors.Hand;
            this.pbAction.Dock = System.Windows.Forms.DockStyle.Right;
            this.pbAction.Image = global::ChatApp.Properties.Resources.Add;
            this.pbAction.Location = new System.Drawing.Point(214, 5);
            this.pbAction.Margin = new System.Windows.Forms.Padding(4);
            this.pbAction.Name = "pbAction";
            this.pbAction.Size = new System.Drawing.Size(57, 69);
            this.pbAction.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pbAction.TabIndex = 2;
            this.pbAction.TabStop = false;
            // 
            // UserListItem
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Transparent;
            this.Controls.Add(this.pnlFrame);
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "UserListItem";
            this.Padding = new System.Windows.Forms.Padding(10, 10, 10, 0);
            this.Size = new System.Drawing.Size(296, 89);
            this.pnlFrame.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pbAvatar)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbAction)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private Guna.UI2.WinForms.Guna2CirclePictureBox pbAvatar;
        public System.Windows.Forms.PictureBox pbAction;
        private Guna.UI2.WinForms.Guna2HtmlLabel lblUserName;
        private Guna.UI2.WinForms.Guna2Panel pnlFrame;
    }
}
