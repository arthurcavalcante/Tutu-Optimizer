using System;
using System.Drawing;
using System.Windows.Forms;

namespace TutusOptimizer
{
    public partial class MainForm
    {
        private void BuildSidebarContent()
        {
            sidebarPanel.Controls.Clear();
            navButtons.Clear();
            Panel brand = new Panel { Dock = DockStyle.Top, Height = 72, BackColor = theme.Surface, Padding = new Padding(18, 14, 10, 10) };
            Label name = new Label { Name = "SidebarBrand", Text = "TUTU'S OPTIMIZER", Dock = DockStyle.Top, Height = 24,
                Font = new Font("Segoe UI", 10.5f, FontStyle.Bold), ForeColor = theme.TextPrimary };
            Label subtitle = new Label { Text = "Monitoramento e otimização", Dock = DockStyle.Top, Height = 22,
                Font = new Font("Segoe UI", 8), ForeColor = theme.TextSecondary };
            brand.Controls.Add(subtitle);
            brand.Controls.Add(name);
            sidebarPanel.Controls.Add(brand);
            navigationScroll = new ModernScrollPanel(theme) { Dock = DockStyle.Fill };
            sidebarPanel.Controls.Add(navigationScroll);
            navigationScroll.BringToFront();
            Panel container = navigationScroll.Content;
            string[] names = { "Visão geral", "Saúde e sensores", "Sistema e CPU", "Privacidade", "Rede e ping", "Jogos (IFEO)",
                "Periféricos", "Limpeza de disco", "Serviços e apps", "Console de logs", "Personalização", "Discos e saúde", "Benchmark e Estresse" };
            string[] icons = { "⌂", "▥", "▣", "◈", "◎", "♧", "◉", "◇", "⚙", "≡", "◐", "▤", "↗" };
            string[] groups = { "MONITORAMENTO", "OTIMIZAÇÕES", "APLICATIVO" };
            int[][] order = { new int[] { 0, 1, 11, 12 }, new int[] { 2, 3, 4, 5, 6, 7, 8 }, new int[] { 10, 9 } };
            for (int i = 0; i < names.Length; i++) navButtons.Add(null);
            int y = 4;
            for (int group = 0; group < groups.Length; group++)
            {
                Label caption = new Label { Text = groups[group], Location = new Point(18, y), Size = new Size(container.Width - 28, 22),
                    Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right, ForeColor = theme.TextSecondary,
                    Font = new Font("Segoe UI", 7.5f, FontStyle.Bold), TextAlign = ContentAlignment.MiddleLeft };
                container.Controls.Add(caption);
                y += 24;
                foreach (int page in order[group])
                {
                    int index = page;
                    Button button = new AnimatedButton { Text = names[index], AccessibleName = names[index], Location = new Point(8, y),
                        Size = new Size(container.Width - 16, 32), Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                        FlatStyle = FlatStyle.Flat, TextAlign = ContentAlignment.MiddleLeft, Padding = new Padding(34, 0, 4, 0),
                        Font = new Font("Segoe UI", 9.25f, index == currentTabIndex ? FontStyle.Bold : FontStyle.Regular),
                        ForeColor = index == currentTabIndex ? theme.NavActiveText : theme.TextSecondary,
                        BackColor = index == currentTabIndex ? theme.NavActiveBg : theme.Surface,
                        Cursor = Cursors.Hand, UseMnemonic = false, Tag = index == currentTabIndex, TabIndex = group * 20 + y };
                    button.FlatAppearance.BorderSize = 0;
                    button.Paint += (s, e) =>
                    {
                        bool active = (bool)button.Tag;
                        using (Font iconFont = new Font("Segoe UI Symbol", 10))
                            TextRenderer.DrawText(e.Graphics, icons[index], iconFont, new Rectangle(button.Text.Length == 0 ? (button.Width - 20) / 2 : 10, 0, 20, button.Height),
                                active ? theme.Accent : theme.TextSecondary, TextFormatFlags.VerticalCenter | TextFormatFlags.HorizontalCenter | TextFormatFlags.NoPrefix);
                        if (active) using (Brush brush = new SolidBrush(theme.Accent)) e.Graphics.FillRectangle(brush, 0, 7, 3, button.Height - 14);
                    };
                    button.Click += (s, e) => SelectTab(index);
                    button.MouseEnter += (s, e) => button.BackColor = index == currentTabIndex ? theme.NavActiveBg : theme.NavHoverBg;
                    button.MouseLeave += (s, e) => button.BackColor = index == currentTabIndex ? theme.NavActiveBg : theme.Surface;
                    navButtons[index] = button;
                    container.Controls.Add(button);
                    y += 34;
                }
                y += 8;
            }
            navigationScroll.SetContentHeight(y + 8);
            navigationScroll.HookWheelRecursive(container);
        }
    }
}
