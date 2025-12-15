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
            this.flpText = new System.Windows.Forms.FlowLayoutPanel();
            this.lblDisplayName = new Guna.UI2.WinForms.Guna2HtmlLabel();
            this.picAvatar = new Guna.UI2.WinForms.Guna2CirclePictureBox();
            this.pnlBackground.SuspendLayout();
            this.flpText.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picAvatar)).BeginInit();
            this.SuspendLayout();
            // 
            // pnlBackground
            // 
            this.pnlBackground.BackColor = System.Drawing.Color.DodgerBlue;
            this.pnlBackground.Controls.Add(this.flpText);
            this.pnlBackground.Controls.Add(this.picAvatar);
            this.pnlBackground.Location = new System.Drawing.Point(3, 3);
            this.pnlBackground.Name = "pnlBackground";
            this.pnlBackground.Size = new System.Drawing.Size(285, 73);
            this.pnlBackground.TabIndex = 0;
            // 
            // flpText
            // 
            this.flpText.AutoSize = true;
            this.flpText.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.flpText.Controls.Add(this.lblDisplayName);
            this.flpText.Location = new System.Drawing.Point(85, 3);
            this.flpText.Name = "flpText";
            this.flpText.Size = new System.Drawing.Size(127, 31);
            this.flpText.TabIndex = 1;
            // 
            // lblDisplayName
            // 
            this.lblDisplayName.BackColor = System.Drawing.Color.Transparent;
            this.lblDisplayName.Font = new System.Drawing.Font("Segoe UI Semibold", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblDisplayName.Location = new System.Drawing.Point(3, 3);
            this.lblDisplayName.Name = "lblDisplayName";
            this.lblDisplayName.Size = new System.Drawing.Size(121, 25);
            this.lblDisplayName.TabIndex = 0;
            this.lblDisplayName.Text = "{Display Name}";
            // 
            // picAvatar
            // 
            this.picAvatar.ImageRotate = 0F;
            this.picAvatar.Location = new System.Drawing.Point(3, 3);
            this.picAvatar.Name = "picAvatar";
            this.picAvatar.ShadowDecoration.Mode = Guna.UI2.WinForms.Enums.ShadowMode.Circle;
            this.picAvatar.Size = new System.Drawing.Size(64, 64);
            this.picAvatar.TabIndex = 0;
            this.picAvatar.TabStop = false;
            // 
            // Conversations
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.pnlBackground);
            this.Name = "Conversations";
            this.Size = new System.Drawing.Size(294, 79);
            this.Tag = "Conversations";
            this.pnlBackground.ResumeLayout(false);
            this.pnlBackground.PerformLayout();
            this.flpText.ResumeLayout(false);
            this.flpText.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picAvatar)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private Guna.UI2.WinForms.Guna2Panel pnlBackground;
        private System.Windows.Forms.FlowLayoutPanel flpText;
        private Guna.UI2.WinForms.Guna2HtmlLabel lblDisplayName;
        private Guna.UI2.WinForms.Guna2CirclePictureBox picAvatar;
    }
}
