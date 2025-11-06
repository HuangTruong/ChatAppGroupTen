using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ChatApp
{
    public partial class TinNhanCuaToi : UserControl
    {
        public TinNhanCuaToi()
        {
            InitializeComponent();
        }

        //public UC_ChatRight(string message, Image avatar)
        //{
        //    InitializeComponent();
        //    lblMessage.Text = message;
        //    picAvatar.Image = avatar;
        //}

        //public string Message
        //{
        //    get => lblMessage.Text;
        //    set => lblMessage.Text = value;
        //}

        //public Image Avatar
        //{
        //    get => picAvatar.Image;
        //    set => picAvatar.Image = value;
        //}

        //private void UC_ChatRight_Load(object sender, EventArgs e)
        //{
        //    pnlBubble.MaximumSize = new Size(250, 0);
        //    pnlBubble.AutoSize = true;

        //    // Canh phải toàn bộ bong bóng
        //    pnlBubble.Left = this.Width - pnlBubble.Width - picAvatar.Width - 20;
        //}
    }
}
