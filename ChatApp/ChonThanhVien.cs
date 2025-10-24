using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace ChatApp
{
    public partial class ChonThanhVien : Form
    {
        public List<string> ThanhVienDuocChon { get; private set; } = new List<string>();

        public ChonThanhVien(IEnumerable<string> danhSachBanBe)
        {
            InitializeComponent();
            Text = "Chọn thành viên";
            Width = 300;
            Height = 400;
            StartPosition = FormStartPosition.CenterParent;

            // Tạo danh sách checkbox
            var flp = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true
            };

            foreach (var ten in danhSachBanBe)
            {
                var cb = new CheckBox
                {
                    Text = ten,
                    AutoSize = true,
                    Padding = new Padding(5)
                };
                flp.Controls.Add(cb);
            }

            var btnXacNhan = new Button
            {
                Text = "✅ Xác nhận",
                Dock = DockStyle.Bottom,
                Height = 40
            };
            btnXacNhan.Click += (s, e) =>
            {
                ThanhVienDuocChon = flp.Controls.OfType<CheckBox>()
                    .Where(cb => cb.Checked)
                    .Select(cb => cb.Text)
                    .ToList();
                DialogResult = DialogResult.OK;
                Close();
            };

            Controls.Add(flp);
            Controls.Add(btnXacNhan);
        }
    }
}
