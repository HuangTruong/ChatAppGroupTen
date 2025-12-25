namespace ChatApp.Forms
{
    partial class FormEmoji
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.flpEmojis = new System.Windows.Forms.FlowLayoutPanel();
            this.SuspendLayout();
            // 
            // flpEmojis
            // 
            this.flpEmojis.AutoScroll = true;
            this.flpEmojis.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flpEmojis.Location = new System.Drawing.Point(0, 0);
            this.flpEmojis.Name = "flpEmojis";
            this.flpEmojis.Size = new System.Drawing.Size(450, 267);
            this.flpEmojis.TabIndex = 0;
            // 
            // FormEmoji
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(450, 267);
            this.Controls.Add(this.flpEmojis);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "FormEmoji";
            this.ShowInTaskbar = false;
            this.Text = "FormEmoji";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.FlowLayoutPanel flpEmojis;
    }
}