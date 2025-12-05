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
            this.flpView = new System.Windows.Forms.FlowLayoutPanel();
            this.SuspendLayout();
            // 
            // flpView
            // 
            this.flpView.AutoScroll = true;
            this.flpView.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.flpView.Location = new System.Drawing.Point(12, 12);
            this.flpView.Name = "flpView";
            this.flpView.Size = new System.Drawing.Size(596, 426);
            this.flpView.TabIndex = 0;
            this.flpView.WrapContents = false;
            // 
            // FormLoiMoiKetBan
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(620, 450);
            this.Controls.Add(this.flpView);
            this.Name = "FormLoiMoiKetBan";
            this.Text = "FormLoiMoiKetBan";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.FlowLayoutPanel flpView;
    }
}