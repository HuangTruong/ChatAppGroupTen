using ChatApp.Helpers;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace ChatApp.Helpers.Ui
{
    // Tạo bong bóng chat (bubble) từ tin nhắn
    public static class ChatBubbleFactory
    {
        // Tạo 1 dòng chat (Panel) chứa bong bóng tin nhắn
        public static Panel CreateRow(
            TinNhan tn,
            bool laCuaToi,
            bool laNhom,
            int panelWidth,
            int maxTextWidth)
        {
            // Panel dòng chat
            var row = new Panel
            {
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Width = panelWidth,
                Margin = new Padding(0, 2, 0, 2),
                Tag = laCuaToi
            };

            // Căn lề theo người gửi
            row.Padding = laCuaToi
                ? new Padding(60, 2, 8, 8)
                : new Padding(8, 2, 60, 8);

            // Bong bóng chứa nội dung
            var bubble = new Panel
            {
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Padding = new Padding(10, 6, 10, 6),
                BackColor = laCuaToi
                    ? Color.FromArgb(222, 242, 255)
                    : Color.White
            };

            // Stack sắp xếp tên, nội dung, thời gian theo chiều dọc
            var stack = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.TopDown,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Margin = Padding.Empty,
                Padding = Padding.Empty
            };

            // Nếu là nhóm thì hiện tên người gửi
            if (laNhom)
            {
                var lblSender = new Label
                {
                    AutoSize = true,
                    Text = tn.guiBoi ?? "",
                    Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                    ForeColor = Color.DimGray,
                    Margin = new Padding(0, 0, 0, 2)
                };
                stack.Controls.Add(lblSender);
            }

            // Nội dung tin nhắn
            var text = tn.noiDung ?? "";
            var lblMsg = new Label
            {
                AutoSize = true,
                Text = text.Length == 0 ? " " : text,
                Font = new Font("Segoe UI", 10f),
                ForeColor = Color.Black,
                Margin = new Padding(0, 0, 0, 4),
                UseMnemonic = false
            };

            // Giới hạn độ rộng tối đa
            int cap = maxTextWidth - bubble.Padding.Horizontal;
            if (cap < 50) cap = 50;
            lblMsg.MaximumSize = new Size(cap, 0);

            // Thời gian gửi
            var lblTime = new Label
            {
                AutoSize = true,
                Text = TimeParser.ToUtc(tn.thoiGian).ToLocalTime()
                    .ToString("HH:mm dd/MM/yyyy"),
                Font = new Font("Segoe UI", 8.5f, FontStyle.Italic),
                ForeColor = Color.DimGray
            };

            // Thêm nội dung và thời gian vào stack
            stack.Controls.Add(lblMsg);
            stack.Controls.Add(lblTime);

            // Gắn stack vào bubble và bubble vào row
            bubble.Controls.Add(stack);
            row.Controls.Add(bubble);

            // Căn trái/phải bong bóng
            AlignBubbleInRow(row);

            // Tự căn lại khi resize
            row.SizeChanged += (s, e) =>
            {
                if (row.Width != panelWidth)
                    row.Width = panelWidth;
                AlignBubbleInRow(row);
            };
            bubble.SizeChanged += (s, e) => AlignBubbleInRow(row);

            return row;
        }

        // Căn trái hoặc phải cho bong bóng trong dòng chat
        public static void AlignBubbleInRow(Panel row)
        {
            if (row == null || row.Controls.Count == 0) return;

            var bubble = row.Controls[0];
            bool laCuaToi = row.Tag is bool b && b;

            if (laCuaToi)
            {
                int x = row.ClientSize.Width - row.Padding.Right - bubble.Width;
                if (x < row.Padding.Left) x = row.Padding.Left;
                bubble.Left = x; // căn phải
            }
            else
            {
                bubble.Left = row.Padding.Left; // căn trái
            }
        }
    }
}
