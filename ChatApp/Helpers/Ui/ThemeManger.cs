using System;
using System.Drawing;
using System.Windows.Forms;
using Guna.UI2.WinForms;
using ChatApp.Helpers.UI;
using ChatApp;

namespace ChatApp.Helpers.UI
{
    /// <summary>
    /// ThemeManager áp dụng Day/Night cho toàn bộ Form và các control con.
    /// Dễ hiểu – cơ bản – không logic phức tạp.
    /// </summary>
    public static class ThemeManager
    {
        /// <summary>
        /// Chế độ hiện tại: true = Dark, false = Light
        /// </summary>
        public static bool IsDarkMode = false;

        #region ===== APPLY THEME PUBLIC APIs =====

        /// <summary>
        /// Áp dụng giao diện Day Mode cho một Form.
        /// </summary>
        public static void ApplyDayTheme(Form form)
        {
            IsDarkMode = false;
            ApplyThemeToControl(form, false);
        }

        /// <summary>
        /// Áp dụng giao diện Night Mode cho một Form.
        /// </summary>
        public static void ApplyNightTheme(Form form)
        {
            IsDarkMode = true;
            ApplyThemeToControl(form, true);
        }

        #endregion



        #region ===== CORE THEME ENGINE =====

        /// <summary>
        /// Hàm đệ quy áp dụng theme cho control và các control con.
        /// </summary>
        private static void ApplyThemeToControl(Control ctrl, bool dark)
        {
            // Nếu là UserControl Messages thì dùng theme riêng của nó và không theme tiếp generic
            // Đổi màu messages(bubble)
            Messages msg = ctrl as Messages;
            if (msg != null)
            {
                msg.ApplyTheme(dark);
                return;
            }

            // Form background
            if (ctrl is Form)
            {
                ctrl.BackColor = dark
                    ? ThemeColors.DarkWindowBackground
                    : ThemeColors.WindowBackground;
            }
            else
            {
                ctrl.BackColor = dark
                    ? ThemeColors.DarkSurface
                    : ThemeColors.Surface;
            }

            // Text color
            ctrl.ForeColor = dark
                ? ThemeColors.DarkTextPrimary
                : ThemeColors.TextPrimary;

            // ---- GUNA2 CONTROLS ----
            ApplyGuna2Theme(ctrl, dark);

            // ---- DUYỆT CONTROL CON ----
            foreach (Control child in ctrl.Controls)
            {
                ApplyThemeToControl(child, dark);
            }
        }

        #endregion



        #region ===== THEMING FOR GUNA2 CONTROLS =====

        /// <summary>
        /// Áp dụng theme cho các control Guna2 phổ biến.
        /// </summary>
        private static void ApplyGuna2Theme(Control ctrl, bool dark)
        {
            //// Messages
            //Messages mes = ctrl as Messages;
            //if (mes != null)
            //{
            //    mes.
            //}

            // Label
            Label lbl = ctrl as Label;
            if (lbl != null)
            {
                lbl.BackColor = Color.Transparent;
            }

            // Button
            Guna2Button btn = ctrl as Guna2Button;
            if (btn != null)
            {
                if (dark)
                {
                    btn.FillColor = ThemeColors.DarkGuna2ButtonFill;
                    btn.ForeColor = ThemeColors.DarkTextOnAccent;
                    btn.HoverState.FillColor = ThemeColors.DarkGuna2ButtonHover;
                    btn.PressedColor = ThemeColors.DarkGuna2ButtonPressed;
                }
                else
                {
                    btn.FillColor = ThemeColors.Guna2ButtonFill;
                    btn.ForeColor = ThemeColors.TextOnAccent;
                    btn.HoverState.FillColor = ThemeColors.Guna2ButtonHover;
                    btn.PressedColor = ThemeColors.Guna2ButtonPressed;
                }
                return;
            }

            // TextBox
            Guna2TextBox tb = ctrl as Guna2TextBox;
            if (tb != null)
            {
                if (dark)
                {
                    tb.FillColor = ThemeColors.DarkGuna2TextBoxBack;
                    tb.BorderColor = ThemeColors.DarkGuna2TextBoxBorder;
                    tb.PlaceholderForeColor = ThemeColors.DarkGuna2TextBoxPlaceholder;
                }
                else
                {
                    tb.FillColor = ThemeColors.Guna2TextBoxBack;
                    tb.BorderColor = ThemeColors.Guna2TextBoxBorder;
                    tb.PlaceholderForeColor = ThemeColors.Guna2TextBoxPlaceholder;
                }
                return;
            }

            // ComboBox
            Guna2ComboBox cb = ctrl as Guna2ComboBox;
            if (cb != null)
            {
                if (dark)
                {
                    cb.FillColor = ThemeColors.DarkGuna2ComboBoxBack;
                    cb.ForeColor = ThemeColors.DarkTextPrimary;
                }
                else
                {
                    cb.FillColor = ThemeColors.Guna2ComboBoxBack;
                    cb.ForeColor = ThemeColors.TextPrimary;
                }
                return;
            }

            // Grandient Panel
            Guna2GradientPanel gpn = ctrl as Guna2GradientPanel;
            if (gpn != null)
            {
                if (dark)
                {
                    gpn.FillColor = ThemeColors.SkyNightStart;
                    gpn.FillColor2 = ThemeColors.SkyNightEnd;
                }
                else
                {
                    gpn.FillColor = ThemeColors.SkyMorningStart;
                    gpn.FillColor2 = ThemeColors.SkyMorningEnd;
                }
                return;
            }

            // Panel
            Guna2Panel pn = ctrl as Guna2Panel;
            if (pn != null)
            {
                pn.BackColor = Color.Transparent;
                pn.BorderRadius = 20;
                // shadow
                pn.ShadowDecoration.BorderRadius = 20;
                pn.ShadowDecoration.Enabled = true;
                pn.ShadowDecoration.Depth = 12;
                pn.ShadowDecoration.Color = dark ? ThemeColors.NightSoftBlueShadow : ThemeColors.DayPrimaryShadow;
                pn.FillColor = dark
                    ? ThemeColors.DarkGuna2PanelFill
                    : ThemeColors.Guna2PanelFill;
                
                return;
            }
        }

        #endregion
    }
}
