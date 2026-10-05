using System.Drawing;
namespace TutusOptimizer
{
    public class ThemePalette
    {
        public bool IsDark;
        public Color Bg;
        public Color Surface;
        public Color Card;
        public Color CardHover;
        public Color Border;
        public Color TextPrimary;
        public Color TextSecondary;
        public Color Accent;
        public Color AccentHover;
        public Color AccentGreen;
        public Color AccentAmber;
        public Color AccentRed;
        public Color AccentCyan;

        public Color BtnSecBg;
        public Color BtnSecBorder;
        public Color BtnSecText;
        public Color BtnSecHover;

        public Color NavActiveBg;
        public Color NavActiveText;
        public Color NavHoverBg;

        public static ThemePalette LightTheme()
        {
            ThemePalette p = new ThemePalette();
            p.IsDark = false;
            p.Bg = Color.FromArgb(248, 250, 252);
            p.Surface = Color.FromArgb(255, 255, 255);
            p.Card = Color.FromArgb(255, 255, 255);
            p.CardHover = Color.FromArgb(241, 245, 249);
            p.Border = Color.FromArgb(226, 232, 240);
            p.TextPrimary = Color.FromArgb(15, 23, 42);
            p.TextSecondary = Color.FromArgb(71, 85, 105);
            p.Accent = Color.FromArgb(79, 70, 229);
            p.AccentHover = Color.FromArgb(67, 56, 202);
            p.AccentGreen = Color.FromArgb(22, 163, 74);
            p.AccentAmber = Color.FromArgb(217, 119, 6);
            p.AccentRed = Color.FromArgb(220, 38, 38);
            p.AccentCyan = Color.FromArgb(2, 132, 199);

            p.BtnSecBg = Color.FromArgb(241, 245, 249);
            p.BtnSecBorder = Color.FromArgb(203, 213, 225);
            p.BtnSecText = Color.FromArgb(30, 41, 59);
            p.BtnSecHover = Color.FromArgb(226, 232, 240);

            p.NavActiveBg = Color.FromArgb(238, 242, 255);
            p.NavActiveText = Color.FromArgb(67, 56, 202);
            p.NavHoverBg = Color.FromArgb(248, 250, 252);
            return p;
        }

        public static ThemePalette DarkTheme()
        {
            ThemePalette p = new ThemePalette();
            p.IsDark = true;
            p.Bg = Color.FromArgb(13, 17, 23);
            p.Surface = Color.FromArgb(22, 27, 34);
            p.Card = Color.FromArgb(30, 37, 48);
            p.CardHover = Color.FromArgb(38, 47, 61);
            p.Border = Color.FromArgb(48, 54, 61);
            p.TextPrimary = Color.FromArgb(240, 246, 252);
            p.TextSecondary = Color.FromArgb(139, 148, 158);
            p.Accent = Color.FromArgb(99, 102, 241);
            p.AccentHover = Color.FromArgb(79, 70, 229);
            p.AccentGreen = Color.FromArgb(34, 197, 94);
            p.AccentAmber = Color.FromArgb(245, 158, 11);
            p.AccentRed = Color.FromArgb(239, 68, 68);
            p.AccentCyan = Color.FromArgb(56, 189, 248);

            p.BtnSecBg = Color.FromArgb(33, 38, 45);
            p.BtnSecBorder = Color.FromArgb(48, 54, 61);
            p.BtnSecText = Color.FromArgb(240, 246, 252);
            p.BtnSecHover = Color.FromArgb(48, 54, 61);

            p.NavActiveBg = Color.FromArgb(30, 37, 48);
            p.NavActiveText = Color.FromArgb(129, 140, 248);
            p.NavHoverBg = Color.FromArgb(30, 37, 48);
            return p;
        }
    }

}
