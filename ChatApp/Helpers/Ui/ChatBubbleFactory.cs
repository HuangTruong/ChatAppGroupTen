using ChatApp.Helpers;
using ChatApp.Models.Chat;     // ✅ THÊM DÒNG NÀY
using System;
using System.Drawing;
using System.Windows.Forms;
using ChatApp.Controls;

namespace ChatApp.Helpers.Ui
{
    public static class ChatBubbleFactory
    {
        public static Panel CreateRow(
            TinNhan tn,
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

            // Padding trái/phải để căn bong bóng
            row.Padding = laCuaToi
                ? new Padding(60, 2, 8, 8)
                : new Padding(8, 2, 60, 8);

            // Parse thời gian từ string trong TinNhan
            DateTime utc;
            try
            {
                utc = TimeParser.ToUtc(tn.thoiGian);
            }
            catch
            {
                utc = DateTime.UtcNow;
            }

            // ==== Tạo TinNhanBubble & map dữ liệu ====
            var bubble = new TinNhanBubble
            {
                LaCuaToi = laCuaToi,
                LaNhom = laNhom,
                TenNguoiGui = tn.guiBoi ?? "",
                NoiDung = tn.noiDung ?? "",
                ThoiGianUtc = utc,

                //Emoji
                LaEmoji = tn.laEmoji,
                EmojiKey = tn.emojiKey,

                // File
                LaFile = tn.laFile,
                TenFile = tn.tenFile,
                KichThuoc = tn.kichThuoc,
                FileUrl = tn.fileUrl,
        };

            bubble.Render();

            row.Controls.Add(bubble);

            // Căn trái/phải trong row
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
