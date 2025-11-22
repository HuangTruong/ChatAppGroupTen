using System;
using System.Drawing;
using System.Windows.Forms;
using ChatApp.Helpers;
using ChatApp.Models.Chat;
//using ChatApp.Controls;
namespace ChatApp.Helpers.Ui
{
    /// <summary>
    /// Factory tạo UI bubble chat cho từng tin nhắn:
    /// - Hiển thị tin nhắn của mình / của người khác.
    /// - Hỗ trợ chat nhóm (hiện tên người gửi).
    /// - Căn canh bong bóng trái/phải theo người gửi.
    /// </summary>
    public static class ChatBubbleFactory
    {
        #region ======== Tạo row bubble chat ========

        /// <summary>
        /// Tạo một hàng (row) chứa bubble chat cho tin nhắn:
        /// - Tự canh padding trái/phải tùy thuộc <paramref name="laCuaToi"/>.
        /// - Nếu là nhóm (<paramref name="laNhom"/>) thì hiện thêm tên người gửi.
        /// - Giới hạn chiều rộng text bằng <paramref name="maxTextWidth"/>.
        /// - Hiển thị thời gian gửi được parse qua <see cref="TimeParser.ToUtc(string)"/>.
        /// </summary>
        /// <param name="tn">Đối tượng tin nhắn <see cref="TinNhan"/> cần render.</param>
        /// <param name="laCuaToi">Cho biết đây có phải tin nhắn của chính mình hay không.</param>
        /// <param name="laNhom">Cho biết đây có phải khung chat nhóm hay không.</param>
        /// <param name="panelWidth">Chiều rộng tổng của row (bằng với panel chứa).</param>
        /// <param name="maxTextWidth">Chiều rộng tối đa cho nội dung text để wrap dòng.</param>
        /// <returns><see cref="Panel"/> đại diện cho một hàng bubble trong khung chat.</returns>
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
            //var bubble = new TinNhanBubble
            //{
            //    LaCuaToi = laCuaToi,
            //    LaNhom = laNhom,
            //    TenNguoiGui = tn.guiBoi ?? "",
            //    NoiDung = tn.noiDung ?? "",
            //    ThoiGianUtc = utc,

            //    // Emoji
            //    LaEmoji = tn.laEmoji,
            //    EmojiKey = tn.emojiKey
            //};

            //bubble.Render();

            //row.Controls.Add(bubble);

            // Căn trái/phải trong row
            //AlignBubbleInRow(row);

            //row.SizeChanged += delegate
            //{
            //    if (row.Width != panelWidth)
            //        row.Width = panelWidth;

            //    AlignBubbleInRow(row);
            //};

            //bubble.SizeChanged += delegate
            //{
            //    AlignBubbleInRow(row);
            //};

            return row;
        }

        #endregion

        #region ======== Căn canh bubble trái/phải trong row ========

        /// <summary>
        /// Căn vị trí bubble trong row:
        /// - Nếu là tin nhắn của mình: canh phải.
        /// - Nếu là tin nhắn của người khác: canh trái.
        /// </summary>
        /// <param name="row">
        /// Panel row chứa bubble, có <see cref="Control.Tag"/> là <c>bool</c> cho biết có phải của mình hay không.
        /// </param>
        public static void AlignBubbleInRow(Panel row)
        {
            if (row == null || row.Controls.Count == 0)
                return;

            var bubble = row.Controls[0];
            bool laCuaToi = row.Tag is bool && (bool)row.Tag;

            if (laCuaToi)
            {
                int x = row.ClientSize.Width - row.Padding.Right - bubble.Width;
                if (x < row.Padding.Left)
                    x = row.Padding.Left;

                bubble.Left = x;
            }
            else
            {
                bubble.Left = row.Padding.Left;
            }
        }

        #endregion
    }
}
