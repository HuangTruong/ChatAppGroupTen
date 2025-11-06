namespace ChatApp
{
    partial class CuocTroChuyen
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
            this.picAnhDaiDien = new Guna.UI2.WinForms.Guna2CirclePictureBox();
            this.lblTen = new Guna.UI2.WinForms.Guna2HtmlLabel();
            this.lblTinNhanCuoiCung = new Guna.UI2.WinForms.Guna2HtmlLabel();
            this.guna2Panel1 = new Guna.UI2.WinForms.Guna2Panel();
            ((System.ComponentModel.ISupportInitialize)(this.picAnhDaiDien)).BeginInit();
            this.SuspendLayout();
            // 
            // picAnhDaiDien
            // 
            this.picAnhDaiDien.ImageRotate = 0F;
            this.picAnhDaiDien.Location = new System.Drawing.Point(10, 7);
            this.picAnhDaiDien.Name = "picAnhDaiDien";
            this.picAnhDaiDien.ShadowDecoration.Mode = Guna.UI2.WinForms.Enums.ShadowMode.Circle;
            this.picAnhDaiDien.Size = new System.Drawing.Size(45, 45);
            this.picAnhDaiDien.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.picAnhDaiDien.TabIndex = 0;
            this.picAnhDaiDien.TabStop = false;
            // 
            // lblTen
            // 
            this.lblTen.AutoSize = false;
            this.lblTen.BackColor = System.Drawing.Color.Transparent;
            this.lblTen.Font = new System.Drawing.Font("Segoe UI", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTen.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(52)))), ((int)(((byte)(54)))));
            this.lblTen.Location = new System.Drawing.Point(65, 8);
            this.lblTen.Name = "lblTen";
            this.lblTen.Size = new System.Drawing.Size(200, 20);
            this.lblTen.TabIndex = 2;
            this.lblTen.Text = "guna2HtmlLabel2";
            // 
            // lblTinNhanCuoiCung
            // 
            this.lblTinNhanCuoiCung.AutoSize = false;
            this.lblTinNhanCuoiCung.BackColor = System.Drawing.Color.Transparent;
            this.lblTinNhanCuoiCung.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTinNhanCuoiCung.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(99)))), ((int)(((byte)(110)))), ((int)(((byte)(114)))));
            this.lblTinNhanCuoiCung.Location = new System.Drawing.Point(65, 30);
            this.lblTinNhanCuoiCung.Name = "lblTinNhanCuoiCung";
            this.lblTinNhanCuoiCung.Size = new System.Drawing.Size(200, 20);
            this.lblTinNhanCuoiCung.TabIndex = 3;
            this.lblTinNhanCuoiCung.Text = "guna2HtmlLabel3";
            // 
            // guna2Panel1
            // 
            this.guna2Panel1.BackColor = System.Drawing.Color.LightGray;
            this.guna2Panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.guna2Panel1.Location = new System.Drawing.Point(8, 54);
            this.guna2Panel1.Name = "guna2Panel1";
            this.guna2Panel1.Size = new System.Drawing.Size(284, 1);
            this.guna2Panel1.TabIndex = 4;
            // 
            // CuocTroChuyen
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.Controls.Add(this.guna2Panel1);
            this.Controls.Add(this.lblTinNhanCuoiCung);
            this.Controls.Add(this.lblTen);
            this.Controls.Add(this.picAnhDaiDien);
            this.Name = "CuocTroChuyen";
            this.Padding = new System.Windows.Forms.Padding(8, 5, 8, 5);
            this.Size = new System.Drawing.Size(300, 60);
            ((System.ComponentModel.ISupportInitialize)(this.picAnhDaiDien)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private Guna.UI2.WinForms.Guna2CirclePictureBox picAnhDaiDien;
        private Guna.UI2.WinForms.Guna2HtmlLabel lblTen;
        private Guna.UI2.WinForms.Guna2HtmlLabel lblTinNhanCuoiCung;
        private Guna.UI2.WinForms.Guna2Panel guna2Panel1;
    }
}
