using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Microsoft.Win32;
using System.Net;
using System.Net.NetworkInformation;

namespace TutusOptimizer
{
    public sealed class ThemedComboBox : ComboBox
    {
        private readonly ThemePalette palette;
        public ThemedComboBox(ThemePalette theme)
        {
            palette = theme;
            DrawMode = DrawMode.OwnerDrawFixed;
            FlatStyle = FlatStyle.Flat;
            DropDownStyle = ComboBoxStyle.DropDownList;
            BackColor = theme.Surface;
            ForeColor = theme.TextPrimary;
            Font = new Font("Segoe UI", 9);
            ItemHeight = 22;
        }
        protected override void OnDrawItem(DrawItemEventArgs e)
        {
            bool selected = (e.State & DrawItemState.Selected) != 0 && (e.State & DrawItemState.ComboBoxEdit) == 0;
            Color background = selected ? palette.NavActiveBg : palette.Surface;
            Color foreground = Enabled ? (selected ? palette.NavActiveText : palette.TextPrimary) : palette.TextSecondary;
            using (Brush brush = new SolidBrush(background)) e.Graphics.FillRectangle(brush, e.Bounds);
            if (e.Index >= 0 && e.Index < Items.Count)
                TextRenderer.DrawText(e.Graphics, GetItemText(Items[e.Index]), Font,
                    new Rectangle(e.Bounds.X + 6, e.Bounds.Y, Math.Max(0, e.Bounds.Width - 12), e.Bounds.Height), foreground,
                    TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);
            if ((e.State & DrawItemState.Focus) != 0) e.DrawFocusRectangle();
        }
    }

    #region Modelos e Paletas de Cores

    internal class ActionCardDef
    {
        public string Title;
        public string Desc;
        public string BtnText;
        public Color AccentColor;
        public EventHandler ClickHandler;

        public ActionCardDef(string title, string desc, string btnText, Color accent, EventHandler handler)
        {
            this.Title = title;
            this.Desc = desc;
            this.BtnText = btnText;
            this.AccentColor = accent;
            this.ClickHandler = handler;
        }
    }

    #endregion

    #region Controles Customizados (Botões Vetoriais, ScrollBar e Medidores)

    public enum WindowButtonType
    {
        Minimize,
        Maximize,
        Close
    }

    public class WindowControlButton : Control
    {
        private WindowButtonType btnType;
        private bool isHovered = false;
        private bool isPressed = false;
        private bool isMaximized = false;
        private ThemePalette theme;

        public WindowControlButton(WindowButtonType type, ThemePalette currentTheme)
        {
            this.btnType = type;
            this.theme = currentTheme;
            this.Size = new Size(46, 44);
            this.BackColor = currentTheme.Surface;
            this.DoubleBuffered = true;
            this.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer, true);
            this.Cursor = Cursors.Hand;
        }

        public void SetMaximized(bool max)
        {
            this.isMaximized = max;
            this.Invalidate();
        }

        public void UpdateTheme(ThemePalette newTheme)
        {
            this.theme = newTheme;
            this.BackColor = newTheme.Surface;
            this.Invalidate();
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            isHovered = true;
            Invalidate();
            base.OnMouseEnter(e);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            isHovered = false;
            isPressed = false;
            Invalidate();
            base.OnMouseLeave(e);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                isPressed = true;
                Invalidate();
            }
            base.OnMouseDown(e);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            isPressed = false;
            Invalidate();
            base.OnMouseUp(e);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            Color bg = theme.Surface;
            if (isHovered)
            {
                if (btnType == WindowButtonType.Close)
                    bg = Color.FromArgb(232, 17, 35);
                else
                    bg = theme.IsDark ? Color.FromArgb(45, 52, 65) : Color.FromArgb(241, 245, 249);
            }
            if (isPressed)
            {
                if (btnType == WindowButtonType.Close)
                    bg = Color.FromArgb(241, 112, 122);
                else
                    bg = theme.IsDark ? Color.FromArgb(55, 65, 81) : Color.FromArgb(226, 232, 240);
            }

            using (SolidBrush b = new SolidBrush(bg))
            {
                g.FillRectangle(b, ClientRectangle);
            }

            Color iconColor = (isHovered && btnType == WindowButtonType.Close)
                ? Color.White
                : (isHovered ? theme.TextPrimary : theme.TextSecondary);

            int cx = Width / 2;
            int cy = Height / 2;

            using (Pen pen = new Pen(iconColor, 1.4f))
            {
                if (btnType == WindowButtonType.Minimize)
                {
                    g.DrawLine(pen, cx - 5, cy, cx + 5, cy);
                }
                else if (btnType == WindowButtonType.Maximize)
                {
                    if (!isMaximized)
                    {
                        g.DrawRectangle(pen, cx - 5, cy - 5, 10, 10);
                    }
                    else
                    {
                        g.DrawRectangle(pen, cx - 3, cy - 5, 8, 8);
                        using (SolidBrush clearBrush = new SolidBrush(bg))
                        {
                            g.FillRectangle(clearBrush, cx - 5, cy - 3, 8, 8);
                        }
                        g.DrawRectangle(pen, cx - 5, cy - 3, 8, 8);
                    }
                }
                else if (btnType == WindowButtonType.Close)
                {
                    g.DrawLine(pen, cx - 4, cy - 4, cx + 4, cy + 4);
                    g.DrawLine(pen, cx + 4, cy - 4, cx - 4, cy + 4);
                }
            }
        }
    }

    public class ModernScrollBar : Control
    {
        private int _minimum = 0;
        private int _maximum = 100;
        private int _value = 0;
        private int _viewSize = 20;

        private bool _isHovered = false;
        private bool _isDragging = false;
        private int _dragStartY = 0;
        private int _dragStartVal = 0;

        private ThemePalette _theme;

        public event EventHandler ValueChanged;

        public ModernScrollBar(ThemePalette theme)
        {
            this._theme = theme;
            this.Width = 10;
            this.BackColor = theme.Bg;
            this.DoubleBuffered = true;
            this.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer, true);
            this.Cursor = Cursors.Default;
        }

        public void UpdateTheme(ThemePalette newTheme)
        {
            this._theme = newTheme;
            this.BackColor = newTheme.Bg;
            this.Invalidate();
        }

        public int Minimum
        {
            get { return _minimum; }
            set { _minimum = value; Invalidate(); }
        }

        public int Maximum
        {
            get { return _maximum; }
            set { _maximum = Math.Max(_minimum, value); Invalidate(); }
        }

        public int ViewSize
        {
            get { return _viewSize; }
            set { _viewSize = Math.Max(1, value); Invalidate(); }
        }

        public int Value
        {
            get { return _value; }
            set
            {
                int maxVal = Math.Max(0, _maximum - _viewSize);
                int newVal = Math.Max(_minimum, Math.Min(maxVal, value));
                if (newVal != _value)
                {
                    _value = newVal;
                    Invalidate();
                    if (ValueChanged != null)
                    {
                        ValueChanged(this, EventArgs.Empty);
                    }
                }
            }
        }

        private Rectangle GetThumbRect()
        {
            int trackHeight = Height;
            if (trackHeight <= 0 || _maximum <= 0) return Rectangle.Empty;

            int maxScroll = Math.Max(0, _maximum - _viewSize);
            if (maxScroll == 0) return Rectangle.Empty;

            int thumbHeight = Math.Max(32, (int)((float)_viewSize / _maximum * trackHeight));
            int availableTrack = trackHeight - thumbHeight;
            int thumbY = (int)((float)_value / maxScroll * availableTrack);

            int thumbWidth = 6;
            int thumbX = (Width - thumbWidth) / 2;

            return new Rectangle(thumbX, thumbY, thumbWidth, thumbHeight);
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            _isHovered = true;
            Invalidate();
            base.OnMouseEnter(e);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            _isHovered = false;
            Invalidate();
            base.OnMouseLeave(e);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                Rectangle thumb = GetThumbRect();
                if (thumb.Contains(e.Location))
                {
                    _isDragging = true;
                    _dragStartY = e.Y;
                    _dragStartVal = _value;
                }
                else
                {
                    if (e.Y < thumb.Y)
                        Value -= _viewSize;
                    else
                        Value += _viewSize;
                }
            }
            base.OnMouseDown(e);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (_isDragging)
            {
                int deltaY = e.Y - _dragStartY;
                int trackHeight = Height;
                Rectangle thumb = GetThumbRect();
                int availableTrack = trackHeight - thumb.Height;

                if (availableTrack > 0)
                {
                    int maxScroll = Math.Max(0, _maximum - _viewSize);
                    int deltaVal = (int)((float)deltaY / availableTrack * maxScroll);
                    Value = _dragStartVal + deltaVal;
                }
            }
            base.OnMouseMove(e);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            _isDragging = false;
            Invalidate();
            base.OnMouseUp(e);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            g.Clear(_theme.Bg);

            int maxScroll = Math.Max(0, _maximum - _viewSize);
            if (maxScroll <= 0) return;

            int trackW = 4;
            int trackX = (Width - trackW) / 2;
            Color trackColor = _theme.IsDark ? Color.FromArgb(22, 27, 34) : Color.FromArgb(241, 245, 249);
            using (SolidBrush trkB = new SolidBrush(trackColor))
            {
                using (GraphicsPath trkPath = new GraphicsPath())
                {
                    trkPath.AddArc(trackX, 4, trackW, trackW, 180, 180);
                    trkPath.AddArc(trackX, Height - trackW - 4, trackW, trackW, 0, 180);
                    trkPath.CloseFigure();
                    g.FillPath(trkB, trkPath);
                }
            }

            Rectangle thumb = GetThumbRect();
            if (thumb.Width <= 0 || thumb.Height <= 0) return;

            Color thumbColor;
            if (_isDragging)
            {
                thumbColor = _theme.IsDark ? Color.FromArgb(139, 148, 158) : Color.FromArgb(100, 116, 139);
            }
            else if (_isHovered)
            {
                thumbColor = _theme.IsDark ? Color.FromArgb(110, 118, 129) : Color.FromArgb(148, 163, 184);
            }
            else
            {
                thumbColor = _theme.IsDark ? Color.FromArgb(72, 79, 88) : Color.FromArgb(203, 213, 225);
            }

            using (GraphicsPath path = new GraphicsPath())
            {
                int rad = thumb.Width / 2;
                if (rad < 1) rad = 1;
                path.AddArc(thumb.X, thumb.Y, rad * 2, rad * 2, 180, 90);
                path.AddArc(thumb.Right - rad * 2, thumb.Y, rad * 2, rad * 2, 270, 90);
                path.AddArc(thumb.Right - rad * 2, thumb.Bottom - rad * 2, rad * 2, rad * 2, 0, 90);
                path.AddArc(thumb.X, thumb.Bottom - rad * 2, rad * 2, rad * 2, 90, 90);
                path.CloseFigure();

                using (SolidBrush b = new SolidBrush(thumbColor))
                {
                    g.FillPath(b, path);
                }
            }
        }
    }

    public class ModernScrollPanel : Panel
    {
        private readonly UiTween scrollMotion = new UiTween();
        private bool wheelAnimating, settingScrollFrame;
        private int wheelTarget;
        private Panel _viewport;
        private Panel _content;
        private ModernScrollBar _scrollBar;
        private ThemePalette _theme;

        public Panel Content
        {
            get { return _content; }
        }

        public ModernScrollBar ScrollBar
        {
            get { return _scrollBar; }
        }

        public ModernScrollPanel(ThemePalette theme)
        {
            this._theme = theme;
            this.Size = new Size(760, 500);
            this.DoubleBuffered = true;
            this.BackColor = theme.Bg;

            _scrollBar = new ModernScrollBar(theme);
            _scrollBar.Dock = DockStyle.Right;
            _scrollBar.Width = 10;
            _scrollBar.ValueChanged += (s, e) =>
            {
                if (!settingScrollFrame) { scrollMotion.Stop(); wheelAnimating = false; wheelTarget = _scrollBar.Value; }
                _content.Top = -_scrollBar.Value;
            };
            _scrollBar.MouseDown += (s, e) => { scrollMotion.Stop(); wheelAnimating = false; };

            _viewport = new Panel();
            _viewport.Dock = DockStyle.Fill;
            _viewport.BackColor = theme.Bg;

            _content = new Panel();
            _content.Location = new Point(0, 0);
            _content.Width = Math.Max(100, this.Width - 12);
            _content.BackColor = theme.Bg;

            _viewport.Controls.Add(_content);

            this.Controls.Add(_viewport);
            this.Controls.Add(_scrollBar);

            _viewport.Resize += (s, e) =>
            {
                _content.Width = _viewport.Width;
                UpdateScrollMetrics();
            };

            this.MouseWheel += OnWheel;
            _viewport.MouseWheel += OnWheel;
            _content.MouseWheel += OnWheel;
        }

        public void HookWheelRecursive(Control parent)
        {
            foreach (Control c in parent.Controls)
            {
                c.MouseWheel -= OnWheel;
                c.MouseWheel += OnWheel;
                if (c.HasChildren)
                {
                    HookWheelRecursive(c);
                }
            }
        }

        private void OnWheel(object sender, MouseEventArgs e)
        {
            int delta = (int)Math.Round(-e.Delta / 120.0 * 60);
            int maximum = Math.Max(0, _content.Height - _viewport.Height);
            wheelTarget = Math.Max(0, Math.Min(maximum, (wheelAnimating ? wheelTarget : _scrollBar.Value) + delta));
            int from = _scrollBar.Value;
            int target = wheelTarget;
            wheelAnimating = true;
            scrollMotion.Run(140, progress =>
            {
                if (IsDisposed) return;
                settingScrollFrame = true;
                _scrollBar.Value = (int)Math.Round(from + (target - from) * progress);
                settingScrollFrame = false;
                wheelAnimating = progress < 1;
            });
        }

        public void SetContentHeight(int totalHeight)
        {
            scrollMotion.Stop(); wheelAnimating = false;
            _content.Height = totalHeight;
            UpdateScrollMetrics();
        }

        public void UpdateScrollMetrics()
        {
            int vH = _viewport.Height;
            int cH = _content.Height;

            _scrollBar.Maximum = cH;
            _scrollBar.ViewSize = vH;
            _scrollBar.Visible = (cH > vH);
            _scrollBar.Value = _scrollBar.Value;
            _content.Top = -_scrollBar.Value;

            if (cH <= vH)
            {
                _scrollBar.Value = 0;
                _content.Top = 0;
            }
        }

        public void UpdateTheme(ThemePalette newTheme)
        {
            this._theme = newTheme;
            this.BackColor = newTheme.Bg;
            _viewport.BackColor = newTheme.Bg;
            _content.BackColor = newTheme.Bg;
            _scrollBar.UpdateTheme(newTheme);
        }
        protected override void Dispose(bool disposing)
        { if (disposing) scrollMotion.Dispose(); base.Dispose(disposing); }
    }

    // Medidor Circular Moderno de Uso e Saúde em Tempo Real
    public class LiveUsageGauge : Control
    {
        private readonly UiTween valueMotion = new UiTween();
        private double _percent = 0;
        private string _title = "CPU";
        private string _sub = "0%";
        private Color _accent = Color.FromArgb(99, 102, 241);
        private ThemePalette _theme;

        public LiveUsageGauge(string title, ThemePalette theme)
        {
            this._title = title;
            this._theme = theme;
            this.Size = new Size(130, 110);
            this.DoubleBuffered = true;
            this.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer, true);
        }

        public void SetValue(double percent, string subText, Color accentColor)
        {
            double target = Math.Max(0, Math.Min(100, percent));
            double from = _percent;
            this._sub = subText;
            this._accent = accentColor;
            if (!Visible) { valueMotion.Stop(); _percent = target; Invalidate(); return; }
            valueMotion.Run(220, progress => { if (!IsDisposed) { _percent = from + (target - from) * progress; Invalidate(); } });
        }

        public void UpdateTheme(ThemePalette newTheme)
        {
            this._theme = newTheme;
            this.Invalidate();
        }

        protected override void Dispose(bool disposing)
        { if (disposing) valueMotion.Dispose(); base.Dispose(disposing); }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            // Fundo
            using (SolidBrush bgB = new SolidBrush(_theme.Card))
            {
                g.FillRectangle(bgB, ClientRectangle);
            }

            // Borda do card
            using (Pen borderP = new Pen(_theme.Border, 1f))
            {
                g.DrawRectangle(borderP, 0, 0, Width - 1, Height - 1);
            }

            // Título superior
            using (SolidBrush tB = new SolidBrush(_theme.TextSecondary))
            {
                using (Font tF = new Font("Segoe UI", 7.5f, FontStyle.Bold))
                {
                    g.DrawString(_title.ToUpper(), tF, tB, new PointF(10, 8));
                }
            }

            // Arco circular de porcentagem
            int arcSize = 56;
            int arcX = (Width - arcSize) / 2;
            int arcY = 28;
            Rectangle arcRect = new Rectangle(arcX, arcY, arcSize, arcSize);

            // Trilha de fundo do arco
            Color trackC = _theme.IsDark ? Color.FromArgb(45, 52, 65) : Color.FromArgb(226, 232, 240);
            using (Pen trackPen = new Pen(trackC, 5.5f))
            {
                trackPen.StartCap = LineCap.Round;
                trackPen.EndCap = LineCap.Round;
                g.DrawArc(trackPen, arcRect, 135, 270);
            }

            // Arco preenchido proporcional à porcentagem
            float sweep = (float)(_percent / 100.0 * 270.0);
            if (sweep > 0)
            {
                using (Pen valPen = new Pen(_accent, 5.5f))
                {
                    valPen.StartCap = LineCap.Round;
                    valPen.EndCap = LineCap.Round;
                    g.DrawArc(valPen, arcRect, 135, Math.Max(2, sweep));
                }
            }

            // Texto percentual no centro do círculo
            string pctStr = string.Format("{0:0}%", _percent);
            using (SolidBrush numB = new SolidBrush(_theme.TextPrimary))
            {
                using (Font numF = new Font("Segoe UI", 10.5f, FontStyle.Bold))
                {
                    SizeF sz = g.MeasureString(pctStr, numF);
                    g.DrawString(pctStr, numF, numB, arcX + (arcSize - sz.Width) / 2, arcY + (arcSize - sz.Height) / 2);
                }
            }

            // Subtexto descritivo na base
            using (SolidBrush subB = new SolidBrush(_theme.TextSecondary))
            {
                using (Font subF = new Font("Segoe UI", 7.5f))
                {
                    SizeF ssz = g.MeasureString(_sub, subF);
                    g.DrawString(_sub, subF, subB, (Width - ssz.Width) / 2, Height - 18);
                }
            }
        }
    }

    #endregion

}
