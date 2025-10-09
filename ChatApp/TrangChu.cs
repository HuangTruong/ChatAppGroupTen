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

            txtTimKiem.PlaceholderText = "Tìm kiếm bạn bè...";
            txtTimKiem.PlaceholderForeColor = Color.Gray;
            txtTimKiem.Font = new Font("Segoe UI", 10);

            txtNhanTin.PlaceholderText = "Write a message...";
            txtNhanTin.PlaceholderForeColor = Color.Gray;
            txtNhanTin.Font = new Font("Segoe UI", 10);

            btnTimKiem.ImageAlign = ContentAlignment.MiddleCenter;
        }
    }
}
