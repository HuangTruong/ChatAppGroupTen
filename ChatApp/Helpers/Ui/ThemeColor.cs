using System.Drawing;

namespace ChatApp.Helpers.UI
{
    public static class ThemeColors
    {
        #region DAY MODE
        // =====================
        // 🌤 DAY MODE
        // =====================
        // Background / Surface
        public static readonly Color WindowBackground = ColorTranslator.FromHtml("#E8F3FF");
        public static readonly Color Surface = ColorTranslator.FromHtml("#FDFEFF");
        public static readonly Color SurfaceAlt = ColorTranslator.FromHtml("#F4F9FF");
        public static readonly Color SurfaceMuted = ColorTranslator.FromHtml("#DCE9F5");
        public static readonly Color SidebarBackground = ColorTranslator.FromHtml("#E9F2FA");

        // Text
        public static readonly Color TextPrimary = ColorTranslator.FromHtml("#0F1C2E");
        public static readonly Color TextSecondary = ColorTranslator.FromHtml("#3A4B63");
        public static readonly Color TextMuted = ColorTranslator.FromHtml("#6A7F99");
        public static readonly Color TextOnAccent = ColorTranslator.FromHtml("#FFFFFF");

        // Accent
        public static readonly Color AccentPrimary = ColorTranslator.FromHtml("#1D7CF2");
        public static readonly Color AccentHover = ColorTranslator.FromHtml("#1664C8");
        public static readonly Color AccentPressed = ColorTranslator.FromHtml("#0F4FA3");
        public static readonly Color AccentSoftBackground = ColorTranslator.FromHtml("#E1EDFF");
        public static readonly Color AccentBorder = ColorTranslator.FromHtml("#93C5FD");

        // Border
        public static readonly Color BorderSoft = ColorTranslator.FromHtml("#D5E2F0");
        public static readonly Color BorderStrong = ColorTranslator.FromHtml("#AFC6DB");

        // List Item
        public static readonly Color ListItem = ColorTranslator.FromHtml("#F7FAFF");
        public static readonly Color ListItemHover = ColorTranslator.FromHtml("#EAF2FF");
        public static readonly Color ListItemSelected = ColorTranslator.FromHtml("#D3E5FF");

        // Guna2 Controls - Day
        public static readonly Color Guna2ButtonFill = ColorTranslator.FromHtml("#1D7CF2");
        public static readonly Color Guna2ButtonHover = ColorTranslator.FromHtml("#1664C8");
        public static readonly Color Guna2ButtonPressed = ColorTranslator.FromHtml("#0F4FA3");
        public static readonly Color Guna2ButtonDisabled = ColorTranslator.FromHtml("#BBD4F7");
        public static readonly Color Guna2ButtonBorder = ColorTranslator.FromHtml("#93C5FD");

        public static readonly Color Guna2TextBoxBack = ColorTranslator.FromHtml("#FFFFFF");
        public static readonly Color Guna2TextBoxBorder = ColorTranslator.FromHtml("#C7D9EB");
        public static readonly Color Guna2TextBoxHoverBorder = ColorTranslator.FromHtml("#1D7CF2");
        public static readonly Color Guna2TextBoxFocusBorder = ColorTranslator.FromHtml("#1D7CF2");
        public static readonly Color Guna2TextBoxPlaceholder = ColorTranslator.FromHtml("#8CA1B8");

        public static readonly Color Guna2ComboBoxBack = ColorTranslator.FromHtml("#FFFFFF");
        public static readonly Color Guna2ComboBoxDropdownBack = ColorTranslator.FromHtml("#F6FAFF");

        public static readonly Color Guna2PanelFill = ColorTranslator.FromHtml("#FFFFFF");
        public static readonly Color Guna2PanelBorder = ColorTranslator.FromHtml("#D5E2F0");

        public static readonly Color Guna2ProgressBar1 = ColorTranslator.FromHtml("#1D7CF2");
        public static readonly Color Guna2ProgressBar2 = ColorTranslator.FromHtml("#67A9FF");
        public static readonly Color Guna2ProgressBarTrack = ColorTranslator.FromHtml("#DCE8F8");

        public static readonly Color Guna2ToggleCheckedFill = ColorTranslator.FromHtml("#1D7CF2");
        public static readonly Color Guna2ToggleCheckedThumb = ColorTranslator.FromHtml("#FFFFFF");
        public static readonly Color Guna2ToggleUncheckedFill = ColorTranslator.FromHtml("#C7D8E6");
        public static readonly Color Guna2ToggleUncheckedThumb = ColorTranslator.FromHtml("#F4F9FF");

        // =====================
        // ✅ STATUS / SEMANTIC (DAY)
        // =====================
        public static readonly Color StatusSuccessBackground = ColorTranslator.FromHtml("#E6F6EC");
        public static readonly Color StatusSuccessBorder = ColorTranslator.FromHtml("#7ACB9A");
        public static readonly Color StatusSuccessText = ColorTranslator.FromHtml("#155F3C");

        public static readonly Color StatusWarningBackground = ColorTranslator.FromHtml("#FFF7E6");
        public static readonly Color StatusWarningBorder = ColorTranslator.FromHtml("#FACC6B");
        public static readonly Color StatusWarningText = ColorTranslator.FromHtml("#7A4B0E");

        public static readonly Color StatusDangerBackground = ColorTranslator.FromHtml("#FDECEC");
        public static readonly Color StatusDangerBorder = ColorTranslator.FromHtml("#F28B82");
        public static readonly Color StatusDangerText = ColorTranslator.FromHtml("#8B1F28");

        // =====================
        // 💬 CHAT BUBBLE / BADGE (DAY)
        // =====================
        // Tin nhắn của tôi
        public static readonly Color MyMessageBackground = ColorTranslator.FromHtml("#D3E5FF"); // gần ListItemSelected
        public static readonly Color MyMessageBorder = ColorTranslator.FromHtml("#93C5FD");
        public static readonly Color MyMessageText = ColorTranslator.FromHtml("#0F1C2E");

        // Tin nhắn của bạn
        public static readonly Color FriendMessageBackground = ColorTranslator.FromHtml("#FFFFFF");
        public static readonly Color FriendMessageBorder = ColorTranslator.FromHtml("#D5E2F0");
        public static readonly Color FriendMessageText = ColorTranslator.FromHtml("#0F1C2E");

        // Tin nhắn hệ thống (joined, left, v.v.)
        public static readonly Color SystemMessageBackground = ColorTranslator.FromHtml("#EFF4FF");
        public static readonly Color SystemMessageText = ColorTranslator.FromHtml("#3A4B63");

        // Badge trạng thái online
        public static readonly Color StatusOnlineDot = ColorTranslator.FromHtml("#22C55E");
        public static readonly Color StatusIdleDot = ColorTranslator.FromHtml("#FACC15");
        public static readonly Color StatusOfflineDot = ColorTranslator.FromHtml("#94A3B8");

        // =====================
        // 🧭 SCROLLBAR / OVERLAY (DAY)
        // =====================
        public static readonly Color ScrollbarTrack = ColorTranslator.FromHtml("#D6E2F3");
        public static readonly Color ScrollbarThumb = ColorTranslator.FromHtml("#A9BDD9");

        // Overlay khi mở dialog / popup
        public static readonly Color OverlayStrong =
            Color.FromArgb(160, 11, 18, 32); // mờ, xanh đen nhẹ
        // =====================
        // SHADOW
        // =====================
        public static readonly Color DayPrimaryShadow = Color.FromArgb(46, 29, 124, 242);  // rgba(29,124,242,0.18)
        public static readonly Color DayAmbientShadow = Color.FromArgb(18, 0, 0, 0);       // rgba(0,0,0,0.07)

        #endregion

        #region NIGHT MODE
        // =====================
        // 🌙 NIGHT MODE
        // =====================
        // Background / Surface
        public static readonly Color DarkWindowBackground = ColorTranslator.FromHtml("#0B1220");
        public static readonly Color DarkSurface = ColorTranslator.FromHtml("#111A2C");
        public static readonly Color DarkSurfaceAlt = ColorTranslator.FromHtml("#152033");
        public static readonly Color DarkSurfaceMuted = ColorTranslator.FromHtml("#1D2A40");
        public static readonly Color DarkSidebarBackground = ColorTranslator.FromHtml("#101A27");

        // Text
        public static readonly Color DarkTextPrimary = ColorTranslator.FromHtml("#F3F6FB");
        public static readonly Color DarkTextSecondary = ColorTranslator.FromHtml("#C7D2E3");
        public static readonly Color DarkTextMuted = ColorTranslator.FromHtml("#97A6BB");
        public static readonly Color DarkTextOnAccent = ColorTranslator.FromHtml("#FFFFFF");

        // Accent
        public static readonly Color DarkAccentPrimary = ColorTranslator.FromHtml("#4C9CFF");
        public static readonly Color DarkAccentHover = ColorTranslator.FromHtml("#3C82DB");
        public static readonly Color DarkAccentPressed = ColorTranslator.FromHtml("#2F6AB8");
        public static readonly Color DarkAccentSoftBackground = ColorTranslator.FromHtml("#1A2F4D");
        public static readonly Color DarkAccentBorder = ColorTranslator.FromHtml("#4C9CFF");

        // Border
        public static readonly Color DarkBorderSoft = ColorTranslator.FromHtml("#1C2A40");
        public static readonly Color DarkBorderStrong = ColorTranslator.FromHtml("#2C3E57");

        // List Item
        public static readonly Color DarkListItem = ColorTranslator.FromHtml("#131E31");
        public static readonly Color DarkListItemHover = ColorTranslator.FromHtml("#1B2A41");
        public static readonly Color DarkListItemSelected = ColorTranslator.FromHtml("#244063");

        // Guna2 Controls - Night
        public static readonly Color DarkGuna2ButtonFill = ColorTranslator.FromHtml("#4C9CFF");
        public static readonly Color DarkGuna2ButtonHover = ColorTranslator.FromHtml("#3C82DB");
        public static readonly Color DarkGuna2ButtonPressed = ColorTranslator.FromHtml("#2F6AB8");
        public static readonly Color DarkGuna2ButtonDisabled = ColorTranslator.FromHtml("#2A3A54");

        public static readonly Color DarkGuna2TextBoxBack = ColorTranslator.FromHtml("#111A2C");
        public static readonly Color DarkGuna2TextBoxBorder = ColorTranslator.FromHtml("#2C3E57");
        public static readonly Color DarkGuna2TextBoxHoverBorder = ColorTranslator.FromHtml("#4C9CFF");
        public static readonly Color DarkGuna2TextBoxFocusBorder = ColorTranslator.FromHtml("#4C9CFF");
        public static readonly Color DarkGuna2TextBoxPlaceholder = ColorTranslator.FromHtml("#8393AB");

        public static readonly Color DarkGuna2ComboBoxBack = ColorTranslator.FromHtml("#111A2C");
        public static readonly Color DarkGuna2ComboBoxDropdownBack = ColorTranslator.FromHtml("#152033");

        public static readonly Color DarkGuna2PanelFill = ColorTranslator.FromHtml("#111A2C");
        public static readonly Color DarkGuna2ProgressBar1 = ColorTranslator.FromHtml("#4C9CFF");
        public static readonly Color DarkGuna2ProgressBar2 = ColorTranslator.FromHtml("#8CBDFF");
        public static readonly Color DarkGuna2ProgressBarTrack = ColorTranslator.FromHtml("#1A263C");

        public static readonly Color DarkGuna2ToggleCheckedFill = ColorTranslator.FromHtml("#4C9CFF");
        public static readonly Color DarkGuna2ToggleCheckedThumb = ColorTranslator.FromHtml("#FFFFFF");
        public static readonly Color DarkGuna2ToggleUncheckedFill = ColorTranslator.FromHtml("#2C3E57");
        public static readonly Color DarkGuna2ToggleUncheckedThumb = ColorTranslator.FromHtml("#1A263A");

        // =====================
        // ✅ STATUS / SEMANTIC (NIGHT)
        // =====================
        public static readonly Color DarkStatusSuccessBackground = ColorTranslator.FromHtml("#102A22");
        public static readonly Color DarkStatusSuccessBorder = ColorTranslator.FromHtml("#34D399");
        public static readonly Color DarkStatusSuccessText = ColorTranslator.FromHtml("#CFFAFE");

        public static readonly Color DarkStatusWarningBackground = ColorTranslator.FromHtml("#3B2A12");
        public static readonly Color DarkStatusWarningBorder = ColorTranslator.FromHtml("#FACC6B");
        public static readonly Color DarkStatusWarningText = ColorTranslator.FromHtml("#FEF3C7");

        public static readonly Color DarkStatusDangerBackground = ColorTranslator.FromHtml("#3B181C");
        public static readonly Color DarkStatusDangerBorder = ColorTranslator.FromHtml("#F97373");
        public static readonly Color DarkStatusDangerText = ColorTranslator.FromHtml("#FECACA");

        // =====================
        // 💬 CHAT BUBBLE / BADGE (NIGHT)
        // =====================
        public static readonly Color DarkMyMessageBackground = ColorTranslator.FromHtml("#244063");
        public static readonly Color DarkMyMessageBorder = ColorTranslator.FromHtml("#4C9CFF");
        public static readonly Color DarkMyMessageText = ColorTranslator.FromHtml("#F3F6FB");

        public static readonly Color DarkFriendMessageBackground = ColorTranslator.FromHtml("#131E31");
        public static readonly Color DarkFriendMessageBorder = ColorTranslator.FromHtml("#1C2A40");
        public static readonly Color DarkFriendMessageText = ColorTranslator.FromHtml("#F3F6FB");

        public static readonly Color DarkSystemMessageBackground = ColorTranslator.FromHtml("#18253A");
        public static readonly Color DarkSystemMessageText = ColorTranslator.FromHtml("#C7D2E3");

        public static readonly Color DarkStatusOnlineDot = ColorTranslator.FromHtml("#22C55E");
        public static readonly Color DarkStatusIdleDot = ColorTranslator.FromHtml("#FACC15");
        public static readonly Color DarkStatusOfflineDot = ColorTranslator.FromHtml("#6B7280");

        // =====================
        // 🧭 SCROLLBAR / OVERLAY (NIGHT)
        // =====================
        public static readonly Color DarkScrollbarTrack = ColorTranslator.FromHtml("#111827");
        public static readonly Color DarkScrollbarThumb = ColorTranslator.FromHtml("#374151");

        public static readonly Color DarkOverlayStrong =
            Color.FromArgb(180, 3, 7, 18);
        // =====================
        // SHADOW
        // =====================
        public static readonly Color NightPrimaryShadow = Color.FromArgb(107, 0, 0, 0);    // rgba(0,0,0,0.42)
        public static readonly Color NightAmbientShadow = Color.FromArgb(56, 0, 0, 0);     // rgba(0,0,0,0.22)
        #endregion

        #region OPTIONAL GRADIENTS
        // =====================
        // 🎨 Optional Gradients
        // =====================
        public static readonly Color SkyMorningStart = ColorTranslator.FromHtml("#D9EEFF");
        public static readonly Color SkyMorningEnd = ColorTranslator.FromHtml("#A7D3FF");
        public static readonly Color SkyNoonStart = ColorTranslator.FromHtml("#E8F3FF");
        public static readonly Color SkyNoonEnd = ColorTranslator.FromHtml("#C3E2FF");
        public static readonly Color SkySunsetStart = ColorTranslator.FromHtml("#8AB6FF");
        public static readonly Color SkySunsetEnd = ColorTranslator.FromHtml("#4C8CFF");
        public static readonly Color SkyNightStart = ColorTranslator.FromHtml("#0B1220");
        public static readonly Color SkyNightEnd = ColorTranslator.FromHtml("#152033");

        // === NIGHT MODE (BRIGHT / GLOW) ===
        public static readonly Color NightSoftBlueShadow = Color.FromArgb(51, 29, 124, 242); // blue glow
        public static readonly Color NightSoftWhiteShadow = Color.FromArgb(25, 255, 255, 255); // white glow
        public static readonly Color NightSoftAmbientGlow = Color.FromArgb(30, 80, 140, 255);  // ambient blue
        #endregion
    }
}
