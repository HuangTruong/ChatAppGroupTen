using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ChatApp
{
    public partial class Conservation : UserControl
    {
        private readonly string _name;
        private readonly bool _laBanBe;
        private readonly bool _ketBan;
        private readonly bool _moiDen;
        private readonly bool _status;

        public Conservation(string name, bool laBanBe, bool ketBan, bool moiDen, bool status)
        {
            _name = name;
            _laBanBe = laBanBe;
            _ketBan = ketBan;
            _moiDen = moiDen;
            _status = status;

            InitializeComponent();
            LoadForm(name, laBanBe, ketBan, moiDen, status);
        }

        public void LoadForm(string name, bool laBanBe, bool ketBan, bool moiDen, bool status)
        {
            lblConservationName.Text = name;
            // Trống Lastname
            picAvatar.Image = Properties.Resources.HoTen;


        }
    }
}
