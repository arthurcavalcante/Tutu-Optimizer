using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace TutusOptimizer
{
    internal static class ResponsiveLayout
    {
        internal static int TextHeight(Label label, int width, int minimum)
        {
            return Math.Max(minimum, TextRenderer.MeasureText(label.Text, label.Font,
                new Size(Math.Max(1, width), int.MaxValue), TextFormatFlags.WordBreak | TextFormatFlags.NoPrefix).Height + 2);
        }

        // Preserve the original gaps while allowing cards to grow when text wraps.
        internal static void Stack(ModernScrollPanel scroll)
        {
            List<Control> controls = new List<Control>();
            foreach (Control control in scroll.Content.Controls) controls.Add(control);
            controls.Sort((a, b) => a.Top.CompareTo(b.Top));
            int[] gaps = new int[controls.Count];
            int bottom = 0;
            for (int i = 0; i < controls.Count; i++)
            { gaps[i] = Math.Max(0, controls[i].Top - bottom); bottom = controls[i].Bottom; }
            bool arranging = false;
            Action arrange = () =>
            {
                if (arranging || scroll.IsDisposed) return;
                arranging = true;
                try
                {
                    int y = 0;
                    for (int i = 0; i < controls.Count; i++)
                    { y += gaps[i]; controls[i].Top = y; y += controls[i].Height; }
                    scroll.SetContentHeight(y + 24);
                }
                finally { arranging = false; }
            };
            foreach (Control control in controls) control.SizeChanged += (s, e) => arrange();
            arrange();
        }

        internal static void Grid(Panel parent, TableLayoutPanel table, Control[] cards, int maxColumns, int minimumWidth, int rowHeight)
        {
            int lastColumns = -1;
            bool arranging = false;
            Action arrange = () =>
            {
                if (arranging) return;
                int columns = Math.Max(1, Math.Min(maxColumns, parent.ClientSize.Width / minimumWidth));
                int rows = (cards.Length + columns - 1) / columns;
                if (columns == lastColumns) return;
                arranging = true;
                table.SuspendLayout();
                try
                {
                    table.Controls.Clear(); table.ColumnStyles.Clear(); table.RowStyles.Clear();
                    table.ColumnCount = columns; table.RowCount = rows;
                    for (int c = 0; c < columns; c++) table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f / columns));
                    for (int r = 0; r < rows; r++) table.RowStyles.Add(new RowStyle(SizeType.Absolute, rowHeight));
                    for (int i = 0; i < cards.Length; i++) table.Controls.Add(cards[i], i % columns, i / columns);
                    lastColumns = columns;
                    parent.Height = rows * rowHeight + parent.Padding.Vertical;
                }
                finally { table.ResumeLayout(true); arranging = false; }
            };
            parent.Resize += (s, e) => arrange();
        }

        internal static void Toolbar(Panel parent, Label title, params Control[] actions)
        {
            bool arranging = false;
            Action arrange = () =>
            {
                if (arranging) return;
                arranging = true;
                try
                {
                    int actionsWidth = 0, actionHeight = 0;
                    foreach (Control action in actions) { actionsWidth += action.Width + 8; actionHeight = Math.Max(actionHeight, action.Height); }
                    bool stacked = parent.Width < actionsWidth + 470;
                    title.AutoSize = false;
                    title.Location = new Point(4, 8);
                    title.Width = Math.Max(1, parent.Width - (stacked ? 12 : actionsWidth + 16));
                    title.Height = TextHeight(title, title.Width, 26);
                    int x = stacked ? 4 : parent.Width - actionsWidth;
                    foreach (Control action in actions)
                    { action.Anchor = AnchorStyles.Top | AnchorStyles.Left; action.Location = new Point(x, stacked ? title.Bottom + 8 : 8); x += action.Width + 8; }
                    parent.Height = Math.Max(title.Bottom, (stacked ? title.Bottom + 8 : 8) + actionHeight) + 12;
                }
                finally { arranging = false; }
            };
            parent.Resize += (s, e) => arrange();
        }
    }
}
