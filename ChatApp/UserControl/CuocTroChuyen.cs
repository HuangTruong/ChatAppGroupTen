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
    public partial class CuocTroChuyen : UserControl
    {
        public CuocTroChuyen()
        {
            InitializeComponent();
        }

        //// Constructor có tham số
        //public UC_UserItem(Image avatar, string name, string lastMessage)
        //{
        //    InitializeComponent();
        //    picAvatar.Image = avatar;
        //    lblName.Text = name;
        //    lblLastMessage.Text = lastMessage;
        //}

        //// ====== Thuộc tính công khai ======
        //public Image Avatar
        //{
        //    get => picAvatar.Image;
        //    set => picAvatar.Image = value;
        //}

        //public string UserName
        //{
        //    get => lblName.Text;
        //    set => lblName.Text = value;
        //}

        //public string LastMessage
        //{
        //    get => lblLastMessage.Text;
        //    set => lblLastMessage.Text = value;
        //}

        //// Sự kiện click chọn người chat
        //public event EventHandler OnSelected;

        //private void UC_UserItem_Click(object sender, EventArgs e)
        //{
        //    OnSelected?.Invoke(this, e);
        //}

        //// Tạo hiệu ứng hover
        //private void UC_UserItem_MouseEnter(object sender, EventArgs e)
        //{
        //    this.BackColor = Color.FromArgb(230, 240, 255);
        //}

        //private void UC_UserItem_MouseLeave(object sender, EventArgs e)
        //{
        //    this.BackColor = Color.White;
        //}
    }
}
