namespace ChatApp.Forms
{
    partial class FormLoiMoiKetBan
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
            this.pnlBackground = new Guna.UI2.WinForms.Guna2GradientPanel();
            this.pnlView = new Guna.UI2.WinForms.Guna2Panel();
            this.pnlBackground.SuspendLayout();
            this.SuspendLayout();
            // 
            // pnlBackground
            // 
            this.pnlBackground.Controls.Add(this.pnlView);
            this.pnlBackground.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlBackground.FillColor = System.Drawing.Color.Transparent;
            this.pnlBackground.FillColor2 = System.Drawing.Color.Transparent;
            this.pnlBackground.GradientMode = System.Drawing.Drawing2D.LinearGradientMode.Vertical;
            this.pnlBackground.Location = new System.Drawing.Point(0, 0);
            this.pnlBackground.Name = "pnlBackground";
            this.pnlBackground.Padding = new System.Windows.Forms.Padding(20);
            this.pnlBackground.Size = new System.Drawing.Size(668, 520);
            this.pnlBackground.TabIndex = 0;
            // 
            // pnlView
            // 
            this.pnlView.BackColor = System.Drawing.Color.Transparent;
            this.pnlView.BorderRadius = 10;
            this.pnlView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlView.FillColor = System.Drawing.Color.Transparent;
            this.pnlView.Location = new System.Drawing.Point(20, 20);
            this.pnlView.Name = "pnlView";
            this.pnlView.Size = new System.Drawing.Size(628, 480);
            this.pnlView.TabIndex = 0;
            // 
            // FormLoiMoiKetBan
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(668, 520);
            this.Controls.Add(this.pnlBackground);
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "FormLoiMoiKetBan";
            this.Text = "FormLoiMoiKetBan";
            this.pnlBackground.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private Guna.UI2.WinForms.Guna2GradientPanel pnlBackground;
        private Guna.UI2.WinForms.Guna2Panel pnlView;
    }
}