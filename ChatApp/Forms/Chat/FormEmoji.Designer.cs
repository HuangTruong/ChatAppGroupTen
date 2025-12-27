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
            this.guna2Panel1 = new Guna.UI2.WinForms.Guna2Panel();
            this.guna2Panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // flpEmojis
            // 
            this.flpEmojis.AutoScroll = true;
            this.flpEmojis.BackColor = System.Drawing.Color.White;
            this.flpEmojis.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flpEmojis.Location = new System.Drawing.Point(5, 5);
            this.flpEmojis.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.flpEmojis.Name = "flpEmojis";
            this.flpEmojis.Size = new System.Drawing.Size(306, 117);
            this.flpEmojis.TabIndex = 0;
            // 
            // guna2Panel1
            // 
            this.guna2Panel1.BorderRadius = 10;
            this.guna2Panel1.Controls.Add(this.flpEmojis);
            this.guna2Panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.guna2Panel1.FillColor = System.Drawing.Color.White;
            this.guna2Panel1.Location = new System.Drawing.Point(3, 3);
            this.guna2Panel1.Name = "guna2Panel1";
            this.guna2Panel1.Padding = new System.Windows.Forms.Padding(5);
            this.guna2Panel1.Size = new System.Drawing.Size(316, 127);
            this.guna2Panel1.TabIndex = 1;
            // 
            // FormEmoji
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.DodgerBlue;
            this.ClientSize = new System.Drawing.Size(322, 133);
            this.Controls.Add(this.guna2Panel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.Name = "FormEmoji";
            this.Padding = new System.Windows.Forms.Padding(3);
            this.ShowInTaskbar = false;
            this.Text = "FormEmoji";
            this.guna2Panel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.FlowLayoutPanel flpEmojis;
        private Guna.UI2.WinForms.Guna2Panel guna2Panel1;
    }
}