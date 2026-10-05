using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace TutusOptimizer
{
    public static class UiMotion
    {
        public static bool AppEnabled = true;
        [DllImport("user32.dll", EntryPoint = "SystemParametersInfoW")]
        private static extern bool ReadSystemPreference(uint action, uint parameter, out int value, uint flags);
        public static bool Enabled
        {
            get
            {
                if (!AppEnabled || SystemInformation.HighContrast || SystemInformation.TerminalServerSession) return false;
                int enabled;
                return ReadSystemPreference(0x1042, 0, out enabled, 0) && enabled != 0;
            }
        }
        public static double Ease(double progress)
        { progress = Math.Max(0, Math.Min(1, progress)); return 1 - Math.Pow(1 - progress, 3); }
        public static Color Blend(Color from, Color to, double progress)
        {
            progress = Math.Max(0, Math.Min(1, progress));
            return Color.FromArgb((int)(from.A + (to.A - from.A) * progress), (int)(from.R + (to.R - from.R) * progress),
                (int)(from.G + (to.G - from.G) * progress), (int)(from.B + (to.B - from.B) * progress));
        }
    }

    public sealed class UiTween : IDisposable
    {
        private readonly Timer timer = new Timer { Interval = 15 };
        private readonly Stopwatch elapsed = new Stopwatch();
        private Action<double> frame;
        private Action finished;
        private int duration;
        private bool disposed;
        public UiTween() { timer.Tick += Tick; }
        public void Run(int milliseconds, Action<double> update, Action completed = null)
        {
            Stop();
            if (disposed) return;
            if (!UiMotion.Enabled || milliseconds <= 0) { update(1); if (completed != null) completed(); return; }
            duration = milliseconds; frame = update; finished = completed;
            elapsed.Restart();
            update(0);
            if (!disposed && frame != null) timer.Start();
        }
        private void Tick(object sender, EventArgs e)
        {
            if (frame == null) { Stop(); return; }
            double progress = UiMotion.Enabled ? Math.Min(1, elapsed.Elapsed.TotalMilliseconds / duration) : 1;
            Action<double> update = frame;
            Action completed = finished;
            if (progress >= 1) Stop();
            update(UiMotion.Ease(progress));
            if (progress >= 1 && completed != null) completed();
        }
        public void Stop() { timer.Stop(); elapsed.Stop(); frame = null; finished = null; }
        public void Dispose() { if (disposed) return; Stop(); disposed = true; timer.Dispose(); }
    }

    public class AnimatedButton : Button
    {
        private readonly UiTween motion = new UiTween();
        private Color resting;
        private bool hovered;
        public AnimatedButton()
        { resting = BackColor; SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint, true); }
        public void SetRestingColor(Color color) { motion.Stop(); resting = color; BackColor = color; }
        private void Animate(Color from, Color to)
        {
            if (IsDisposed) return;
            BackColor = from;
            motion.Run(120, progress => { if (!IsDisposed) BackColor = UiMotion.Blend(from, to, progress); });
        }
        protected override void OnMouseEnter(EventArgs e)
        {
            hovered = true;
            resting = BackColor;
            base.OnMouseEnter(e);
            Color target = BackColor != resting ? BackColor : UiMotion.Blend(resting, ForeColor, 0.08);
            Animate(resting, target);
        }
        protected override void OnMouseLeave(EventArgs e)
        {
            hovered = false;
            Color from = BackColor;
            base.OnMouseLeave(e);
            Color target = BackColor != from ? BackColor : resting;
            Animate(from, target);
        }
        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (!hovered) resting = BackColor;
            base.OnMouseDown(e);
            if (e.Button == MouseButtons.Left) Animate(BackColor, UiMotion.Blend(BackColor, Color.Black, 0.12));
        }
        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            if (!IsDisposed) Animate(BackColor, hovered ? UiMotion.Blend(resting, ForeColor, 0.08) : resting);
        }
        protected override void Dispose(bool disposing)
        { if (disposing) motion.Dispose(); base.Dispose(disposing); }
    }

    public sealed class PageTransitionOverlay : Control
    {
        private Bitmap previous, next;
        private readonly UiTween motion = new UiTween();
        private double progress;
        public PageTransitionOverlay(Bitmap before, Bitmap after, Color background)
        {
            previous = before; next = after; BackColor = background;
            Dock = DockStyle.Fill; TabStop = false;
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);
        }
        public void Start(Action completed)
        { motion.Run(180, value => { progress = value; Invalidate(); }, completed); }
        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.Clear(BackColor);
            if (previous != null) e.Graphics.DrawImageUnscaled(previous, 0, 0);
            if (next == null) return;
            using (ImageAttributes attributes = new ImageAttributes())
            {
                ColorMatrix alpha = new ColorMatrix(); alpha.Matrix33 = (float)progress;
                attributes.SetColorMatrix(alpha);
                int offset = (int)Math.Round((1 - progress) * 10);
                e.Graphics.DrawImage(next, new Rectangle(offset, 0, next.Width, next.Height), 0, 0, next.Width, next.Height, GraphicsUnit.Pixel, attributes);
            }
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing) { motion.Dispose(); if (previous != null) previous.Dispose(); if (next != null) next.Dispose(); previous = null; next = null; }
            base.Dispose(disposing);
        }
    }
}
