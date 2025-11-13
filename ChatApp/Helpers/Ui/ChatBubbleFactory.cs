using ChatApp.Helpers;
using ChatApp.Models.Chat;     // ✅ THÊM DÒNG NÀY
using System;
using System.Drawing;
using System.Windows.Forms;

namespace ChatApp.Helpers.Ui
{
    public static class ChatBubbleFactory
    {
        public static Panel CreateRow(
            TinNhan tn,                 // ✅ Lúc này trỏ đúng ChatApp.Models.Chat.TinNhan
            bool laCuaToi,
            bool laNhom,
            int panelWidth,
            int maxTextWidth)
        {
            var row = new Panel
            {
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Width = panelWidth,
                Margin = new Padding(0, 2, 0, 2),
                Tag = laCuaToi
            };

            row.Padding = laCuaToi
                ? new Padding(60, 2, 8, 8)
                : new Padding(8, 2, 60, 8);

            var bubble = new Panel
            {
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Padding = new Padding(10, 6, 10, 6),
                BackColor = laCuaToi
                    ? Color.FromArgb(222, 242, 255)
                    : Color.White
            };

            var stack = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.TopDown,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Margin = Padding.Empty,
                Padding = Padding.Empty
            };

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

            var text = tn.noiDung ?? "";
            var lblMsg = new Label
            {
                AutoSize = true,
                Text = string.IsNullOrEmpty(text) ? " " : text,
                Font = new Font("Segoe UI", 10f),
                ForeColor = Color.Black,
                Margin = new Padding(0, 0, 0, 4),
                UseMnemonic = false
            };

            int cap = maxTextWidth - bubble.Padding.Horizontal;
            if (cap < 50) cap = 50;
            lblMsg.MaximumSize = new Size(cap, 0);

            var lblTime = new Label
            {
                AutoSize = true,
                Text = TimeParser.ToUtc(tn.thoiGian).ToLocalTime()
                    .ToString("HH:mm dd/MM/yyyy"),
                Font = new Font("Segoe UI", 8.5f, FontStyle.Italic),
                ForeColor = Color.DimGray
            };

            stack.Controls.Add(lblMsg);
            stack.Controls.Add(lblTime);

            bubble.Controls.Add(stack);
            row.Controls.Add(bubble);

            AlignBubbleInRow(row);

            row.SizeChanged += (s, e) =>
            {
                if (row.Width != panelWidth)
                    row.Width = panelWidth;
                AlignBubbleInRow(row);
            };
            bubble.SizeChanged += (s, e) => AlignBubbleInRow(row);

            return row;
        }

        public static void AlignBubbleInRow(Panel row)
        {
            if (row == null || row.Controls.Count == 0) return;

            var bubble = row.Controls[0];
            bool laCuaToi = row.Tag is bool b && b;

            if (laCuaToi)
            {
                int x = row.ClientSize.Width - row.Padding.Right - bubble.Width;
                if (x < row.Padding.Left) x = row.Padding.Left;
                bubble.Left = x;
            }
            else
            {
                bubble.Left = row.Padding.Left;
            }
        }
    }
}
