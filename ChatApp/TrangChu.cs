using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Runtime.CompilerServices.RuntimeHelpers;

namespace ChatApp
{
    public partial class TrangChu : Form
    {
        public TrangChu()
        {
            InitializeComponent();
        }

        // Nhấn vào panel + picturebox + label Nhan Tin
        private void pnlNhanTin_Click(object sender, EventArgs e)
        {
            NhanTin f = new NhanTin();
            f.Show();
        }

        // Gắn sự kiện cho cả panel, picturebox, label
        private void TrangChu_Load(object sender, EventArgs e)
        {
            pnlNhanTin.Click += pnlNhanTin_Click;
            picNhanTin.Click += pnlNhanTin_Click;
            lblNhanTin.Click += pnlNhanTin_Click;
        }
    }
}
