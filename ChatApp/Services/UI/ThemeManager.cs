using ChatApp.Controls;
using ChatApp.Services.UI;
using Guna.UI2.WinForms;
using System;
using System.Drawing;
using System.Windows.Forms;
using static ChatApp.Services.UI.ThemeColor;

namespace ChatApp.Services.UI
{
    /// <summary>
    /// Quản lý giao diện Light / Dark Mode cho toàn bộ ứng dụng
    /// </summary>
    public static class ThemeManager
    {
        #region ======= PROPERTIES =======

        /// <summary>
        /// Trạng thái hiện tại của giao diện (Dark / Light)
        /// </summary>
        public static bool IsDark { get; private set; }

        #endregion

        #region ======= PUBLIC METHODS =======

        /// <summary>
        /// Áp dụng giao diện cho Form và toàn bộ control con
        /// </summary>
        /// <param name="form">Form cần áp dụng theme</param>
        /// <param name="isDark">true: Dark Mode, false: Light Mode</param>
        public static void ApplyTheme(Form form, bool isDark)
        {
            IsDark = isDark;

            // Màu nền Form
            form.BackColor = isDark ? Dark.Background : Light.Background;

            // Duyệt tất cả control trên Form
            foreach (Control ctrl in form.Controls)
            {
                ApplyControlTheme(ctrl, isDark);
            }
        }

        /// <summary>
        /// Chuyển đổi Light / Dark Mode
        /// </summary>
        /// <param name="form">Form cần toggle</param>
        public static void ToggleTheme(Form form)
        {
            IsDark = !IsDark;
            ApplyTheme(form, IsDark);
        }

        #endregion

        #region ======= PRIVATE METHODS =======

        /// <summary>
        /// Áp dụng theme cho từng control (đệ quy)
        /// </summary>
        private static void ApplyControlTheme(Control ctrl, bool isDark)
        {
            #region ===== Custom Controls =====

            // Control danh sách hội thoại
            if (ctrl is Conversations conv)
            {
                conv.ApplyTheme(isDark);
                return;
            }

            // Bong bóng tin nhắn
            if (ctrl is MessageBubbles bubble)
            {
                bubble.ApplyTheme(isDark);
                return;
            }

            #endregion

            #region ===== Label =====

            // Label chuẩn WinForms
            if (ctrl is Label lbl)
            {
                lbl.BackColor = Color.Transparent;
                lbl.ForeColor = isDark
                    ? ColorTranslator.FromHtml("#E5E7EB")
                    : ColorTranslator.FromHtml("#0F172A");
            }

            // Label của Guna2
            if (ctrl is Guna2HtmlLabel gLbl)
            {
                gLbl.BackColor = Color.Transparent;
                gLbl.ForeColor = isDark
                    ? ColorTranslator.FromHtml("#E5E7EB")
                    : ColorTranslator.FromHtml("#0F172A");
            }

            #endregion

            #region ===== CheckBox =====
            if (ctrl is Guna2CheckBox gCb)
            {
                gCb.AutoSize = true;

                if (isDark)
                {
                    // ===== DARK MODE =====
                    gCb.ForeColor = Dark.TextPrimary;

                    gCb.CheckedState.BorderColor = Dark.ButtonPrimary;
                    gCb.CheckedState.FillColor = Dark.ButtonPrimary;

                    gCb.UncheckedState.BorderColor = Dark.Border;
                    gCb.UncheckedState.FillColor = Color.Transparent;
                }
                else
                {
                    // ===== LIGHT MODE =====
                    gCb.ForeColor = Light.TextPrimary;

                    gCb.CheckedState.BorderColor = Shared.Primary;
                    gCb.CheckedState.FillColor = Shared.Primary;

                    gCb.UncheckedState.BorderColor = Light.Border;
                    gCb.UncheckedState.FillColor = Color.Transparent;
                }
            }
            #endregion

            #region ===== Button =====

            if (ctrl is Guna2Button gBtn)
            {
                // Bo góc + đổ bóng
                gBtn.BorderRadius = 20;
                gBtn.BorderThickness = 0;

                gBtn.ShadowDecoration.Enabled = true;
                gBtn.ShadowDecoration.Depth = 8;
                gBtn.ShadowDecoration.BorderRadius = 20;
                gBtn.ShadowDecoration.Color = ColorTranslator.FromHtml("#93C5FD");

                if (isDark)
                {
                    gBtn.FillColor = ColorTranslator.FromHtml("#2563EB");
                    gBtn.ForeColor = Color.White;
                    gBtn.HoverState.FillColor = ColorTranslator.FromHtml("#3B82F6");
                    gBtn.PressedColor = ColorTranslator.FromHtml("#1E40AF");
                }
                else
                {
                    gBtn.FillColor = ColorTranslator.FromHtml("#3B82F6");
                    gBtn.ForeColor = Color.White;
                    gBtn.HoverState.FillColor = ColorTranslator.FromHtml("#2563EB");
                    gBtn.PressedColor = ColorTranslator.FromHtml("#1D4ED8");
                }
            }

            #endregion

            #region ===== Panel =====

            // Panel thường
            if (ctrl is Panel pnl)
            {
                pnl.BackColor = Color.Transparent;
            }

            // Panel Guna2
            if (ctrl is Guna2Panel gPnl)
            {
                // Bỏ qua panel Conversations
                if (gPnl.Tag?.ToString() != "Conversations")
                {
                    gPnl.BackColor = Color.Transparent;
                    gPnl.BorderThickness = 1;

                    gPnl.ShadowDecoration.Enabled = true;
                    gPnl.ShadowDecoration.Depth = 10;

                    if (isDark)
                    {
                        gPnl.FillColor = ColorTranslator.FromHtml("#0F172A");
                        gPnl.BorderColor = ColorTranslator.FromHtml("#1E3A8A");
                        gPnl.ShadowDecoration.Color = ColorTranslator.FromHtml("#1D4ED8");
                    }
                    else
                    {
                        gPnl.FillColor = ColorTranslator.FromHtml("#E0F2FE");
                        gPnl.BorderColor = ColorTranslator.FromHtml("#BAE6FD");
                        gPnl.ShadowDecoration.Color = ColorTranslator.FromHtml("#93C5FD");
                    }
                }
            }

            #endregion

            #region ===== TextBox =====

            if (ctrl is Guna2TextBox gTxt)
            {
                gTxt.BorderRadius = 20;
                gTxt.BorderThickness = 1;

                if (isDark)
                {
                    gTxt.FillColor = ColorTranslator.FromHtml("#020617");
                    gTxt.ForeColor = ColorTranslator.FromHtml("#E5E7EB");
                    gTxt.PlaceholderForeColor = ColorTranslator.FromHtml("#64748B");
                    gTxt.BorderColor = ColorTranslator.FromHtml("#1E3A8A");

                    gTxt.FocusedState.BorderColor = ColorTranslator.FromHtml("#38BDF8");
                    gTxt.HoverState.BorderColor = ColorTranslator.FromHtml("#60A5FA");
                }
                else
                {
                    gTxt.FillColor = ColorTranslator.FromHtml("#F8FAFC");
                    gTxt.ForeColor = ColorTranslator.FromHtml("#0F172A");
                    gTxt.PlaceholderForeColor = ColorTranslator.FromHtml("#94A3B8");
                    gTxt.BorderColor = ColorTranslator.FromHtml("#93C5FD");

                    gTxt.FocusedState.BorderColor = ColorTranslator.FromHtml("#3B82F6");
                    gTxt.HoverState.BorderColor = ColorTranslator.FromHtml("#60A5FA");
                }
            }

            #endregion

            #region ===== Gradient Panel =====

            if (ctrl is Guna2GradientPanel gpnl)
            {
                gpnl.FillColor = isDark ? Dark.PanelGradientStart : Light.PanelGradientStart;
                gpnl.FillColor2 = isDark ? Dark.PanelGradientEnd : Light.PanelGradientEnd;
            }

            #endregion

            #region ===== Recursive Apply =====

            // Áp dụng tiếp cho control con
            foreach (Control child in ctrl.Controls)
            {
                ApplyControlTheme(child, isDark);
            }

            #endregion
        }

        #endregion
    }
}
