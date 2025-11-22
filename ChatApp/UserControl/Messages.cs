using ChatApp.Helpers;
using ChatApp.Helpers.UI;
using ChatApp.Models.Chat;
using Guna.UI2.WinForms;
using System;
using System.Diagnostics.Eventing.Reader;
using System.Drawing;
using System.Windows.Forms;

namespace ChatApp
{
    public partial class Messages : UserControl
    {
        public Messages()
        {
            InitializeComponent();
        }

        public Messages(TinNhan tn, bool laCuaToi, bool laNhom, int panelWidth, int maxTextWidth)
        {
            InitializeComponent();
            LoadForm(tn, laCuaToi, laNhom, panelWidth, maxTextWidth);
        }

        

        public void LoadForm(TinNhan tn, bool laCuaToi, bool laNhom, int panelWidth, int maxTextWidth)
        {
            this.Width = panelWidth;
            pnlBackground.Tag = laCuaToi;

            //------------------------------
            // Avatar
            //------------------------------
            picAvatar.Image = Properties.Resources.HoTen;
            picAvatar.SizeMode = PictureBoxSizeMode.Zoom;

            //------------------------------
            // Text content
            //------------------------------
            lblMessage.Text = tn.noiDung;
            lblMessage.MaximumSize = new Size(maxTextWidth, 0); // Cho phép xuống dòng

            //------------------------------
            // Time
            //------------------------------
            lblTime.Text = TimeParser.ToUtc(tn.thoiGian)
                                     .ToLocalTime()
                                     .ToString("HH:mm dd/MM/yyyy");

            //------------------------------
            // Bubble color
            //------------------------------
            pnlMessages.Padding = new Padding(10, 6, 10, 6);

            // Áp dụng theme hiện tại (Day/Night)
            ApplyTheme(ThemeManager.IsDarkMode);

            //------------------------------
            // Căn ban đầu (trước khi resize)
            //------------------------------
            AlignBubbleInRow(pnlBackground);

            //------------------------------
            // Tự căn lại khi resize các control
            //------------------------------
            pnlBackground.SizeChanged += (s, e) => AlignBubbleInRow(pnlBackground);
            pnlMessages.SizeChanged += (s, e) => AlignBubbleInRow(pnlBackground);
        }

        #region Căn chỉnh bubble, avatar
        //====================================================================
        // Căn chỉnh bubble và avatar trong 1 dòng – Chuẩn Messenger
        //====================================================================
        public static void AlignBubbleInRow(Panel row)
        {
            if (row == null) return;

            var bubble = row.Controls["pnlMessages"];
            var avatar = row.Controls["picAvatar"];
            if (bubble == null || avatar == null) return;

            bool laCuaToi = row.Tag is bool && (bool)row.Tag;

            if (laCuaToi)
            {
                // Avatar bên phải
                avatar.Left = row.ClientSize.Width - avatar.Width - 8;

                // Bubble nằm bên trái avatar
                bubble.Left = avatar.Left - bubble.Width - 10;
            }
            else
            {
                // Avatar bên trái
                avatar.Left = 8;

                // Bubble nằm bên phải avatar
                bubble.Left = avatar.Right + 8;
            }

            // Tự cập nhật chiều cao dòng
            row.Height = Math.Max(avatar.Bottom, bubble.Bottom) + 10;
        }
        #endregion

        #region ===== THEME SUPPORT =====

        /// <summary>
        /// Áp dụng Day/Night theme cho bubble tin nhắn.
        /// </summary>
        /// <param name="dark">true = Night Mode, false = Day Mode.</param>
        public void ApplyTheme(bool dark)
        {
            bool laCuaToi = false;

            // Lấy cờ từ Tag của pnlBackground (đã gán trong LoadForm).
            if (pnlBackground.Tag is bool)
            {
                laCuaToi = (bool)pnlBackground.Tag;
            }

            // Nền của UserControl và background giữ trong suốt / đồng bộ.
            this.BackColor = Color.Transparent;
            pnlBackground.BackColor = Color.Transparent;

            if (dark)
            {
                // Bubble của mình: dùng màu accent tối (giống button)
                pnlMessages.FillColor = laCuaToi
                    ? ThemeColors.DarkMyMessageBackground
                    : ThemeColors.DarkFriendMessageBackground;

                lblMessage.ForeColor = laCuaToi
                    ? ThemeColors.DarkMyMessageText
                    : ThemeColors.DarkMyMessageText;

                lblTime.ForeColor = laCuaToi
                    ? ThemeColors.DarkMyMessageText
                    : ThemeColors.DarkMyMessageText;
            }
            else
            {
                // Day Mode
                pnlMessages.FillColor = laCuaToi
                    ? ThemeColors.MyMessageBackground
                    : ThemeColors.FriendMessageBackground;

                lblMessage.ForeColor = laCuaToi
                    ? ThemeColors.MyMessageText
                    : ThemeColors.MyMessageText;

                lblTime.ForeColor = laCuaToi
                    ? ThemeColors.MyMessageText
                    : ThemeColors.MyMessageText;
            }
        }

        #endregion

    }
}
