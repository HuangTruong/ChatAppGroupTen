using ChatApp.Controllers;
using ChatApp.Services.Chat;
using Guna.UI2.WinForms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI.WebControls;
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
        private readonly FriendService _friendService;
        private readonly NhanTinController _nhanTinController;

        public Conservation(string name, bool laBanBe, bool ketBan, bool moiDen, bool status,FriendService friendService, NhanTinController nhanTinController)
        {
            _name = name;
            _laBanBe = laBanBe;
            _ketBan = ketBan;
            _moiDen = moiDen;
            _status = status;

            _friendService = friendService;
            _nhanTinController = nhanTinController;

            InitializeComponent();
            LoadForm(name, laBanBe, ketBan, moiDen, status);
        }

        public void LoadForm(string name, bool laBanBe, bool ketBan, bool moiDen, bool status)
        {
            lblConservationName.Text = name;
            // Trống Lastname
            picAvatar.Image = Properties.Resources.HoTen;


        }
        public void CreateMenu()
        {
            // Tạo menu
            Guna2ContextMenuStrip menu = new Guna2ContextMenuStrip();

            // Thêm mục "Kết bạn" nếu chưa là bạn và chưa gửi lời mời
            if (_laBanBe && !_ketBan && !_moiDen)
            {
                var ketBan = new ToolStripMenuItem("Kết bạn");
                ketBan.Click += async (s, e) =>
                {
                    await _friendService.GuiLoiMoiAsync(_name);
                    //_view.ShowInfo("Đã gửi lời mời.");
                };
                menu.Items.Add(ketBan);
            }

            // Thêm mục "Chấp nhận kết bạn" nếu có lời mời
            if (_moiDen)
            {
                var chapNhan = new ToolStripMenuItem("Chấp nhận kết bạn");
                chapNhan.Click += async (s, e) =>
                {
                    await _friendService.ChapNhanAsync(_name);
                    //_nhanTinController.view.ShowInfo("Đã trở thành bạn bè.");
                };
                menu.Items.Add(chapNhan);
            }

            // Thêm mục "Huỷ kết bạn" nếu đã là bạn
            if (_laBanBe)
            {
                var huyBan = new ToolStripMenuItem("Huỷ kết bạn");
                huyBan.Click += async (s, e) =>
                {
                    //DialogResult r = _view.ShowConfirm("Huỷ kết bạn với " + _name + "?", "Xác nhận");
                    //if (r == DialogResult.Yes)
                    //{
                        await _friendService.HuyKetBanAsync(_name);
                    //}
                };
                menu.Items.Add(huyBan);
            }

            // Gán menu cho control (ví dụ Guna2Button)
            pnlBackground.ContextMenuStrip = menu;
            picAvatar.ContextMenuStrip = menu;
            lblConservationName.ContextMenuStrip = menu;

            pnlBackground.Click += async delegate { await _nhanTinController.MoChat1_1Async(_name, false); };
        }
    }
}
