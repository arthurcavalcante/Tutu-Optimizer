using System;
using System.Drawing;
using System.Windows.Forms;

namespace TutusOptimizer
{
    public partial class MainForm
    {
        private bool responsiveShellPending;
        private void UpdateResponsiveShell()
        {
            if (responsiveShellPending || sidebarPanel == null || sidebarPanel.IsDisposed || contentPanel == null) return;
            responsiveShellPending = true;
            SuspendLayout();
            try
            {
                bool compact = ClientSize.Width < 1000;
                sidebarPanel.Width = compact ? 64 : 220;
                contentPanel.Padding = compact ? new Padding(12, 12, 12, 12) : new Padding(18, 14, 18, 14);
                foreach (Button button in navButtons)
                {
                    if (button == null || button.IsDisposed) continue;
                    button.Text = compact ? "" : button.AccessibleName;
                    toolTip.SetToolTip(button, button.AccessibleName);
                    button.Invalidate();
                }
                if (navigationScroll != null && !navigationScroll.IsDisposed)
                {
                    int[][] groups = { new[] { 0, 1, 11, 12 }, new[] { 2, 3, 4, 5, 6, 7, 8 }, new[] { 10, 9 } };
                    int y = 4, groupIndex = 0;
                    foreach (Control control in navigationScroll.Content.Controls)
                    {
                        if (!(control is Label)) continue;
                        control.Visible = !compact;
                        control.Top = y;
                        y += compact ? 4 : 24;
                        foreach (int index in groups[groupIndex++]) { navButtons[index].Top = y; y += 34; }
                        y += 8;
                    }
                    navigationScroll.SetContentHeight(y + 8);
                }
                foreach (Control control in sidebarPanel.Controls)
                {
                    Panel brand = control as Panel;
                    if (brand == null || brand is ModernScrollPanel) continue;
                    brand.Height = compact ? 56 : 72;
                    brand.Padding = compact ? new Padding(8, 14, 4, 10) : new Padding(18, 14, 10, 10);
                    foreach (Control label in brand.Controls)
                    {
                        if (label.Name == "SidebarBrand") label.Text = compact ? "TUTU" : "TUTU'S OPTIMIZER";
                        else label.Visible = !compact;
                    }
                }
                if (lblSubLogo != null && !lblSubLogo.IsDisposed)
                {
                    lblSubLogo.AutoSize = false;
                    lblSubLogo.AutoEllipsis = true;
                    lblSubLogo.SetBounds(lblLogo.Right + 16, 14, Math.Max(0, ClientSize.Width - lblLogo.Right - 172), 22);
                }
            }
            finally { ResumeLayout(true); responsiveShellPending = false; }
        }
    }
}
