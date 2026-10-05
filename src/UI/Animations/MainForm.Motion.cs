using System;
using System.Drawing;
using System.Windows.Forms;

namespace TutusOptimizer
{
    public partial class MainForm
    {
        private PageTransitionOverlay pageTransition;
        private void StopPageTransition()
        {
            if (pageTransition == null) return;
            PageTransitionOverlay current = pageTransition;
            pageTransition = null;
            current.Dispose();
        }
        private Bitmap CapturePage(Panel page)
        {
            if (page == null || page.IsDisposed || page.Width <= 0 || page.Height <= 0) return null;
            Bitmap bitmap = new Bitmap(page.Width, page.Height);
            try { page.DrawToBitmap(bitmap, new Rectangle(Point.Empty, page.Size)); return bitmap; }
            catch { bitmap.Dispose(); return null; }
        }
        private void StartPageTransition(Bitmap before, Panel page)
        {
            if (before == null) return;
            Bitmap after = CapturePage(page);
            if (after == null) { before.Dispose(); return; }
            PageTransitionOverlay overlay = new PageTransitionOverlay(before, after, theme.Bg);
            pageTransition = overlay;
            contentPanel.Controls.Add(overlay);
            overlay.BringToFront();
            overlay.Start(() =>
            {
                if (object.ReferenceEquals(pageTransition, overlay)) pageTransition = null;
                overlay.Dispose();
            });
        }
    }
}
