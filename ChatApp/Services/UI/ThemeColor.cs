using System.Drawing;

namespace ChatApp.Services.UI
{
    /// <summary>
    /// Modern Sky UI Color System – Light & Dark Mode
    /// </summary>
    public static class ThemeColor
    {
        // =====================================================
        // ==================== SHARED COLORS ==================
        // =====================================================
        public static class Shared
        {
            public static Color Primary = ColorTranslator.FromHtml("#3B82F6");
            public static Color PrimaryHover = ColorTranslator.FromHtml("#2563EB");
            public static Color PrimaryLight = ColorTranslator.FromHtml("#93C5FD");
            public static Color PrimarySoft = ColorTranslator.FromHtml("#ECF3FF");

            public static Color Success = ColorTranslator.FromHtml("#22C55E");
            public static Color Warning = ColorTranslator.FromHtml("#FACC15");
            public static Color Danger = ColorTranslator.FromHtml("#EF4444");
            public static Color Info = ColorTranslator.FromHtml("#38BDF8");
        }

        // =====================================================
        // ===================== LIGHT MODE ====================
        // =====================================================
        public static class Light
        {
            public static Color Background = ColorTranslator.FromHtml("#F6FAFF");
            public static Color Surface = ColorTranslator.FromHtml("#FFFFFF");
            public static Color SurfaceSoft = Shared.PrimarySoft;
            public static Color Border = ColorTranslator.FromHtml("#D6E4FF");

            public static Color TextPrimary = ColorTranslator.FromHtml("#0F172A");
            public static Color TextSecondary = ColorTranslator.FromHtml("#475569");

            public static Color ButtonPrimary = Shared.Primary;
            public static Color ButtonPrimaryHover = Shared.PrimaryHover;
            public static Color ButtonSecondary = Shared.PrimaryLight;

            public static Color ToggleOn = Shared.Primary;
            public static Color ToggleOff = ColorTranslator.FromHtml("#DDEEFF");

            public static Color CardBackground = Surface;
            public static Color CardBorder = Border;
            public static Color PanelBackground = SurfaceSoft;
            public static Color HeaderBackground = Shared.Primary;

            // Gradient Panel trực tiếp
            public static Color PanelGradientStart = ColorTranslator.FromHtml("#87CEEB");
            public static Color PanelGradientEnd = ColorTranslator.FromHtml("#ADD8E6");
            public static Color ButtonPrimaryGradientStart = Shared.PrimaryLight;
            public static Color ButtonPrimaryGradientEnd = Shared.Primary;
        }

        // =====================================================
        // ===================== DARK MODE =====================
        // =====================================================
        public static class Dark
        {
            public static Color Background = ColorTranslator.FromHtml("#020617");
            public static Color Surface = Background;
            public static Color SurfaceElev = ColorTranslator.FromHtml("#0F172A");
            public static Color Border = ColorTranslator.FromHtml("#1E293B");

            public static Color TextPrimary = ColorTranslator.FromHtml("#E5E7EB");
            public static Color TextSecondary = ColorTranslator.FromHtml("#94A3B8");

            public static Color ButtonPrimary = ColorTranslator.FromHtml("#60A5FA");
            public static Color ButtonPrimaryHover = Shared.Primary;
            public static Color ButtonSecondary = ColorTranslator.FromHtml("#1E3A8A");

            public static Color ToggleOn = ButtonPrimary;
            public static Color ToggleOff = ColorTranslator.FromHtml("#5C6B7F");

            public static Color CardBackground = SurfaceElev;
            public static Color CardBorder = Border;
            public static Color PanelBackground = Surface;
            public static Color HeaderBackground = Shared.PrimaryHover;

            // Gradient Panel trực tiếp
            public static Color PanelGradientStart = ColorTranslator.FromHtml("#191970");
            public static Color PanelGradientEnd = ColorTranslator.FromHtml("#4B0082");
            public static Color ButtonPrimaryGradientStart = Shared.Primary;
            public static Color ButtonPrimaryGradientEnd = ButtonPrimary;
        }
    }
}
