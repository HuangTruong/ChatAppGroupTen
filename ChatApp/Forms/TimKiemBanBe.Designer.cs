namespace ChatApp.Forms
{
    partial class TimKiemBanBe
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
            this.pnlBackGround = new Guna.UI2.WinForms.Guna2GradientPanel();
            this.pnlView = new Guna.UI2.WinForms.Guna2Panel();
            this.pnlBackGround.SuspendLayout();
            this.SuspendLayout();
            // 
            // pnlBackGround
            // 
            this.pnlBackGround.Controls.Add(this.pnlView);
            this.pnlBackGround.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlBackGround.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(116)))), ((int)(((byte)(185)))), ((int)(((byte)(255)))));
            this.pnlBackGround.FillColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(9)))), ((int)(((byte)(132)))), ((int)(((byte)(227)))));
            this.pnlBackGround.GradientMode = System.Drawing.Drawing2D.LinearGradientMode.Vertical;
            this.pnlBackGround.Location = new System.Drawing.Point(0, 0);
            this.pnlBackGround.Name = "pnlBackGround";
            this.pnlBackGround.Padding = new System.Windows.Forms.Padding(20);
            this.pnlBackGround.Size = new System.Drawing.Size(628, 554);
            this.pnlBackGround.TabIndex = 1;
            // 
            // pnlView
            // 
            this.pnlView.BackColor = System.Drawing.Color.White;
            this.pnlView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlView.Location = new System.Drawing.Point(20, 20);
            this.pnlView.Name = "pnlView";
            this.pnlView.Size = new System.Drawing.Size(588, 514);
            this.pnlView.TabIndex = 1;
            // 
            // TimKiemBanBe
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(628, 554);
            this.Controls.Add(this.pnlBackGround);
            this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.Name = "TimKiemBanBe";
            this.Text = "TimKiemBanBe";
            this.pnlBackGround.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        private Guna.UI2.WinForms.Guna2GradientPanel pnlBackGround;
        private Guna.UI2.WinForms.Guna2Panel pnlView;
    }
}