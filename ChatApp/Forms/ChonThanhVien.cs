using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace ChatApp
{
    /// <summary>
    /// Form cho phép người dùng chọn nhiều thành viên từ danh sách bạn bè.
    /// Mỗi bạn bè được hiển thị bằng một CheckBox.
    /// </summary>
    public partial class ChonThanhVien : Form
    {
        #region ======== Thuộc tính ========

        /// <summary>
        /// Danh sách tên thành viên được người dùng chọn.
        /// </summary>
        public List<string> ThanhVienDuocChon { get; private set; } = new List<string>();

        #endregion

        #region ======== Khởi tạo Form ========

        /// <summary>
        /// Khởi tạo form chọn thành viên với danh sách bạn bè cho trước.
        /// </summary>
        /// <param name="danhSachBanBe">Danh sách tên bạn bè để hiển thị cho người dùng chọn.</param>
        public ChonThanhVien(IEnumerable<string> danhSachBanBe)
        {
            InitializeComponent();

            // Thiết lập cơ bản cho form
            this.Text = "Chọn thành viên";
            this.Width = 300;
            this.Height = 400;
            this.StartPosition = FormStartPosition.CenterParent;

            // Tạo FlowLayoutPanel chứa các CheckBox
            FlowLayoutPanel flp = new FlowLayoutPanel();
            flp.Dock = DockStyle.Fill;
            flp.AutoScroll = true;

            // Tạo CheckBox cho từng tên bạn bè
            foreach (string ten in danhSachBanBe)
            {
                CheckBox cb = new CheckBox();
                cb.Text = ten;
                cb.AutoSize = true;
                cb.Padding = new Padding(5);

                flp.Controls.Add(cb);
            }

            // Nút Xác nhận ở dưới
            Button btnXacNhan = new Button();
            btnXacNhan.Text = "✅ Xác nhận";
            btnXacNhan.Dock = DockStyle.Bottom;
            btnXacNhan.Height = 40;

            // Sự kiện click nút Xác nhận
            btnXacNhan.Click += delegate (object sender, EventArgs e)
            {
                // Lọc những checkbox được tick và lấy Text làm tên thành viên
                ThanhVienDuocChon = flp.Controls
                    .OfType<CheckBox>()
                    .Where(delegate (CheckBox cb) { return cb.Checked; })
                    .Select(delegate (CheckBox cb) { return cb.Text; })
                    .ToList();

                this.DialogResult = DialogResult.OK;
                this.Close();
            };

            // Thêm control vào form
            this.Controls.Add(flp);
            this.Controls.Add(btnXacNhan);
        }

        #endregion
    }
}
