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
            this.pbAvatar = new Guna.UI2.WinForms.Guna2CirclePictureBox();
            this.lblUserName = new System.Windows.Forms.Label();
            this.pbAction = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.pbAvatar)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbAction)).BeginInit();
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
            this.lblUserName.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblUserName.Location = new System.Drawing.Point(73, 31);
            this.lblUserName.Name = "lblUserName";
            this.lblUserName.Size = new System.Drawing.Size(51, 20);
            this.lblUserName.TabIndex = 1;
            this.lblUserName.Text = "label1";
            // 
            // pbAction
            // 
            this.pbAction.Location = new System.Drawing.Point(201, 17);
            this.pbAction.Name = "pbAction";
            this.pbAction.Size = new System.Drawing.Size(34, 34);
            this.pbAction.TabIndex = 2;
            this.pbAction.TabStop = false;
            // 
            // UserListItem
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.pbAction);
            this.Controls.Add(this.lblUserName);
            this.Controls.Add(this.pbAvatar);
            this.Name = "UserListItem";
            this.Size = new System.Drawing.Size(238, 70);
            ((System.ComponentModel.ISupportInitialize)(this.pbAvatar)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbAction)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private Guna.UI2.WinForms.Guna2CirclePictureBox pbAvatar;
        private System.Windows.Forms.Label lblUserName;
        public System.Windows.Forms.PictureBox pbAction;
    }
}
