using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Microsoft.Win32;

namespace TutusOptimizer
{
    #region Modelos e Paletas de Cores

    // Definição de item de ação rápida (compatível com C# 5)
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

    // Gerenciador central de paletas de cores (Modo Escuro / Modo Claro perfeitamente sincronizados com o Windows)
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

        // Botões secundários
        public Color BtnSecBg;
        public Color BtnSecBorder;
        public Color BtnSecText;
        public Color BtnSecHover;

        // Navegação lateral
        public Color NavActiveBg;
        public Color NavActiveText;
        public Color NavHoverBg;

        public static ThemePalette LightTheme()
        {
            ThemePalette p = new ThemePalette();
            p.IsDark = false;
            p.Bg = Color.FromArgb(248, 250, 252);           // Slate 50 suave
            p.Surface = Color.FromArgb(255, 255, 255);      // Branco puro
            p.Card = Color.FromArgb(255, 255, 255);         // Cards brancos
            p.CardHover = Color.FromArgb(241, 245, 249);    // Slate 100 suave
            p.Border = Color.FromArgb(226, 232, 240);       // Slate 200 sutil
            p.TextPrimary = Color.FromArgb(15, 23, 42);      // Slate 900 preto legível
            p.TextSecondary = Color.FromArgb(71, 85, 105);   // Slate 600 cinza médio
            p.Accent = Color.FromArgb(79, 70, 229);         // Indigo 600 vibrante
            p.AccentHover = Color.FromArgb(67, 56, 202);
            p.AccentGreen = Color.FromArgb(22, 163, 74);    // Verde 600
            p.AccentAmber = Color.FromArgb(217, 119, 6);    // Âmbar 600
            p.AccentRed = Color.FromArgb(220, 38, 38);      // Vermelho 600
            p.AccentCyan = Color.FromArgb(2, 132, 199);     // Azul 600

            p.BtnSecBg = Color.FromArgb(241, 245, 249);     // Slate 100
            p.BtnSecBorder = Color.FromArgb(203, 213, 225); // Slate 300
            p.BtnSecText = Color.FromArgb(30, 41, 59);      // Slate 800 legível
            p.BtnSecHover = Color.FromArgb(226, 232, 240);  // Slate 200

            p.NavActiveBg = Color.FromArgb(238, 242, 255);  // Indigo 50
            p.NavActiveText = Color.FromArgb(67, 56, 202);  // Indigo 700 bold
            p.NavHoverBg = Color.FromArgb(248, 250, 252);
            return p;
        }

        public static ThemePalette DarkTheme()
        {
            ThemePalette p = new ThemePalette();
            p.IsDark = true;
            p.Bg = Color.FromArgb(13, 17, 23);              // Slate dark 900 (GitHub/Windows 11 Dark)
            p.Surface = Color.FromArgb(22, 27, 34);         // Surface dark elegante
            p.Card = Color.FromArgb(30, 37, 48);            // Card escuro moderno
            p.CardHover = Color.FromArgb(38, 47, 61);      // Card hover sutil
            p.Border = Color.FromArgb(48, 54, 61);          // Bordas discretas
            p.TextPrimary = Color.FromArgb(240, 246, 252);  // Branco suave de alto contraste
            p.TextSecondary = Color.FromArgb(139, 148, 158);// Cinza médio nítido
            p.Accent = Color.FromArgb(99, 102, 241);        // Indigo 500 vibrante
            p.AccentHover = Color.FromArgb(79, 70, 229);
            p.AccentGreen = Color.FromArgb(34, 197, 94);    // Verde esmeralda
            p.AccentAmber = Color.FromArgb(245, 158, 11);   // Âmbar brilhante
            p.AccentRed = Color.FromArgb(239, 68, 68);      // Vermelho
            p.AccentCyan = Color.FromArgb(56, 189, 248);    // Ciano

            p.BtnSecBg = Color.FromArgb(33, 38, 45);        // Slate escuro 800
            p.BtnSecBorder = Color.FromArgb(48, 54, 61);    // Borda cinza sutil
            p.BtnSecText = Color.FromArgb(240, 246, 252);   // Branco nítido (não amarelo!)
            p.BtnSecHover = Color.FromArgb(48, 54, 61);     // Hover elevado

            p.NavActiveBg = Color.FromArgb(30, 37, 48);
            p.NavActiveText = Color.FromArgb(129, 140, 248); // Indigo claro bold
            p.NavHoverBg = Color.FromArgb(30, 37, 48);
            return p;
        }
    }

    #endregion

    #region Controles Customizados (Botões Vetoriais de Janela e ScrollBar Moderna)

    public enum WindowButtonType
    {
        Minimize,
        Maximize,
        Close
    }

    // Botões de Janela Estilo Windows 11 Fluent (Vetor GDI+ Nítido - SEM FUNDO BRANCO NO MODO ESCURO)
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
            this.BackColor = currentTheme.Surface; // NUNCA branco por padrão!
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

            // Fundo: Sempre respeita theme.Surface no repouso (zero retângulo branco no Dark Mode!)
            Color bg = theme.Surface;
            if (isHovered)
            {
                if (btnType == WindowButtonType.Close)
                    bg = Color.FromArgb(232, 17, 35); // Vermelho clássico Windows 11
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

    // Barra de Rolagem Estilizada, Minimalista e Suave (Zero preto, zero artefatos)
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

            // Fundo da barra sempre limpo com a cor exata da página (zero transparência com falha!)
            g.Clear(_theme.Bg);

            int maxScroll = Math.Max(0, _maximum - _viewSize);
            if (maxScroll <= 0) return;

            // Trilha central sutil
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

            // Cores do Thumb: cinza suave e elegante em ambos os temas
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

    // Painel de Rolagem Suave com Viewport Robusta e Responsiva
    public class ModernScrollPanel : Panel
    {
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
            this.DoubleBuffered = true;
            this.BackColor = theme.Bg;

            _scrollBar = new ModernScrollBar(theme);
            _scrollBar.Dock = DockStyle.Right;
            _scrollBar.Width = 10;
            _scrollBar.ValueChanged += (s, e) =>
            {
                _content.Top = -_scrollBar.Value;
            };

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
            int delta = -e.Delta / 120 * 60;
            _scrollBar.Value += delta;
        }

        public void SetContentHeight(int totalHeight)
        {
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
    }

    #endregion

    public class MainForm : Form
    {
        #region Win32 API Window Dragging & Theming

        [DllImport("user32.dll")]
        private static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

        [DllImport("user32.dll")]
        private static extern bool ReleaseCapture();

        [DllImport("uxtheme.dll", ExactSpelling = true, CharSet = CharSet.Unicode)]
        private static extern int SetWindowTheme(IntPtr hWnd, string pszSubAppName, string pszSubIdList);

        [DllImport("dwmapi.dll")]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

        private const int WM_NCLBUTTONDOWN = 0xA1;
        private const int HT_CAPTION = 0x2;
        private const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;
        private const int DWMWA_USE_IMMERSIVE_DARK_MODE_OLD = 19;

        #endregion

        #region Estado

        private ThemePalette theme;
        private List<OptimizationItem> allTweaks;
        private List<GameItem> allGames;
        private SystemInfo sysInfo;
        private HardwareMonitorData hwData;

        private Dictionary<OptimizationItem, CheckBox> tweakCheckBoxes = new Dictionary<OptimizationItem, CheckBox>();
        private Dictionary<GameItem, CheckBox> gameCheckBoxes = new Dictionary<GameItem, CheckBox>();

        private BackgroundWorker worker;
        private int currentTabIndex = 0;

        #endregion

        #region Componentes de Interface

        private Panel titleBar;
        private Label lblLogo;
        private Label lblSubLogo;

        private WindowControlButton btnMinimize;
        private WindowControlButton btnMaximize;
        private WindowControlButton btnClose;

        private Panel sidebarPanel;
        private Panel contentPanel;
        private Panel footerPanel;

        private List<Button> navButtons = new List<Button>();
        private Panel[] tabViews;

        private Label lblStatus;
        private Label lblSelectedCount;
        private ProgressBar progressBar;
        private Button btnApply;
        private Button btnRevert;
        private Button btnRecommended;

        private RichTextBox logBox;
        private TextBox txtGameSearch;
        private FlowLayoutPanel flowGames;

        private NotifyIcon trayIcon;
        private ContextMenuStrip trayMenu;
        private ToolTip toolTip;

        #endregion

        public MainForm()
        {
            this.FormBorderStyle = FormBorderStyle.None;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Size = new Size(1120, 720);
            this.MinimumSize = new Size(880, 580);
            this.DoubleBuffered = true;
            this.SetStyle(ControlStyles.ResizeRedraw, true);

            // Detecta e sincroniza automaticamente com o tema nativo do Windows do usuário
            bool isSystemDark = DetectSystemDarkMode();
            theme = isSystemDark ? ThemePalette.DarkTheme() : ThemePalette.LightTheme();
            this.BackColor = theme.Bg;

            try
            {
                if (File.Exists("app.ico"))
                {
                    this.Icon = new Icon("app.ico");
                }
            }
            catch { }

            allTweaks = TweakEngine.BuildCatalog();
            allGames = TweakEngine.GetGamesCatalog();
            sysInfo = TweakEngine.GetSystemInfo();
            hwData = TweakEngine.GetHardwareSensors();

            toolTip = new ToolTip();
            toolTip.AutoPopDelay = 5000;
            toolTip.InitialDelay = 300;

            InitializeTrayIcon();
            BuildBaseLayout();
            SelectTab(0);
            UpdateSelectedCount();

            worker = new BackgroundWorker();
            worker.WorkerReportsProgress = true;
            worker.DoWork += Worker_DoWork;
            worker.ProgressChanged += Worker_ProgressChanged;
            worker.RunWorkerCompleted += Worker_RunWorkerCompleted;
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            ApplyImmersiveDarkMode(this.Handle, theme.IsDark);
        }

        private static void ApplyImmersiveDarkMode(IntPtr handle, bool enabled)
        {
            try
            {
                int useDark = enabled ? 1 : 0;
                if (DwmSetWindowAttribute(handle, DWMWA_USE_IMMERSIVE_DARK_MODE, ref useDark, sizeof(int)) != 0)
                {
                    DwmSetWindowAttribute(handle, DWMWA_USE_IMMERSIVE_DARK_MODE_OLD, ref useDark, sizeof(int));
                }
            }
            catch { }
        }

        #region Detecção de Tema do Sistema

        private static bool DetectSystemDarkMode()
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize"))
                {
                    if (key != null)
                    {
                        object val = key.GetValue("AppsUseLightTheme");
                        if (val != null)
                        {
                            return Convert.ToInt32(val) == 0; // 0 = Dark Mode, 1 = Light Mode
                        }
                    }
                }
            }
            catch { }
            return false;
        }

        #endregion

        #region Estrutura de Layout Base

        private void BuildBaseLayout()
        {
            this.SuspendLayout();
            this.Controls.Clear();

            // 1. BARRA DE TÍTULO
            titleBar = new Panel();
            titleBar.Dock = DockStyle.Top;
            titleBar.Height = 44;
            titleBar.BackColor = theme.Surface;
            titleBar.MouseDown += TitleBar_MouseDown;
            titleBar.DoubleClick += (s, e) => ToggleMaximizeWindow();
            titleBar.Paint += (s, e) =>
            {
                using (Pen p = new Pen(theme.Border, 1f))
                {
                    e.Graphics.DrawLine(p, 0, titleBar.Height - 1, titleBar.Width, titleBar.Height - 1);
                }
            };

            lblLogo = new Label();
            lblLogo.Text = "⚡ TUTU'S OPTIMIZER";
            lblLogo.Font = new Font("Segoe UI", 10.5f, FontStyle.Bold);
            lblLogo.ForeColor = theme.Accent;
            lblLogo.AutoSize = true;
            lblLogo.Location = new Point(14, 12);
            lblLogo.MouseDown += TitleBar_MouseDown;
            lblLogo.DoubleClick += (s, e) => ToggleMaximizeWindow();

            lblSubLogo = new Label();
            lblSubLogo.Text = string.Format("2026 • Windows 10 & 11 • Modo {0}", theme.IsDark ? "Escuro" : "Claro");
            lblSubLogo.Font = new Font("Segoe UI", 8.5f);
            lblSubLogo.ForeColor = theme.TextSecondary;
            lblSubLogo.AutoSize = true;
            lblSubLogo.Location = new Point(190, 14);
            lblSubLogo.MouseDown += TitleBar_MouseDown;
            lblSubLogo.DoubleClick += (s, e) => ToggleMaximizeWindow();

            // Botões de Janela
            Panel titleControls = new Panel();
            titleControls.Dock = DockStyle.Right;
            titleControls.Width = 140;
            titleControls.BackColor = theme.Surface;

            btnMinimize = new WindowControlButton(WindowButtonType.Minimize, theme);
            btnMinimize.Location = new Point(2, 0);
            btnMinimize.Click += (s, e) => this.WindowState = FormWindowState.Minimized;
            toolTip.SetToolTip(btnMinimize, "Minimizar Janela");

            btnMaximize = new WindowControlButton(WindowButtonType.Maximize, theme);
            btnMaximize.Location = new Point(48, 0);
            btnMaximize.Click += (s, e) => ToggleMaximizeWindow();
            toolTip.SetToolTip(btnMaximize, "Maximizar / Restaurar Janela");

            btnClose = new WindowControlButton(WindowButtonType.Close, theme);
            btnClose.Location = new Point(94, 0);
            btnClose.Click += (s, e) => this.Close();
            toolTip.SetToolTip(btnClose, "Fechar Aplicativo");

            titleControls.Controls.Add(btnMinimize);
            titleControls.Controls.Add(btnMaximize);
            titleControls.Controls.Add(btnClose);

            titleBar.Controls.Add(lblLogo);
            titleBar.Controls.Add(lblSubLogo);
            titleBar.Controls.Add(titleControls);

            // 2. RODAPÉ (FOOTER)
            footerPanel = new Panel();
            footerPanel.Dock = DockStyle.Bottom;
            footerPanel.Height = 60;
            footerPanel.BackColor = theme.Surface;
            footerPanel.Padding = new Padding(16, 8, 16, 8);
            footerPanel.Paint += (s, e) =>
            {
                using (Pen p = new Pen(theme.Border, 1f))
                {
                    e.Graphics.DrawLine(p, 0, 0, footerPanel.Width, 0);
                }
            };
            BuildFooterContent();

            // 3. BARRA LATERAL (SIDEBAR)
            sidebarPanel = new Panel();
            sidebarPanel.Dock = DockStyle.Left;
            sidebarPanel.Width = 210;
            sidebarPanel.BackColor = theme.Surface;
            sidebarPanel.Paint += (s, e) =>
            {
                using (Pen p = new Pen(theme.Border, 1f))
                {
                    e.Graphics.DrawLine(p, sidebarPanel.Width - 1, 0, sidebarPanel.Width - 1, sidebarPanel.Height);
                }
            };
            BuildSidebarContent();

            // 4. CONTEÚDO PRINCIPAL (DOCK=FILL)
            contentPanel = new Panel();
            contentPanel.Dock = DockStyle.Fill;
            contentPanel.BackColor = theme.Bg;
            contentPanel.Padding = new Padding(18, 14, 18, 14);

            this.Controls.Add(contentPanel);
            this.Controls.Add(sidebarPanel);
            this.Controls.Add(footerPanel);
            this.Controls.Add(titleBar);

            BuildTabViews();

            this.ResumeLayout(true);
        }

        private void TitleBar_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(this.Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
            }
        }

        #endregion

        #region Helper Centralizado de Botões Secundários Elegantes

        // Botões secundários com tema sincronizado (Dark/Light sem bordas roxas e sem textos amarelos)
        private Button CreateSubtleButton(string text, EventHandler onClick, int width, int height)
        {
            Button btn = new Button();
            btn.Text = text;
            btn.Font = new Font("Segoe UI", 8.5f, FontStyle.Bold);
            btn.Size = new Size(width, height);
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 1;
            btn.FlatAppearance.BorderColor = theme.BtnSecBorder;
            btn.BackColor = theme.BtnSecBg;
            btn.ForeColor = theme.BtnSecText;
            btn.Cursor = Cursors.Hand;
            if (onClick != null) btn.Click += onClick;

            btn.MouseEnter += (s, e) =>
            {
                btn.BackColor = theme.BtnSecHover;
                btn.ForeColor = theme.IsDark ? Color.White : Color.FromArgb(15, 23, 42);
            };
            btn.MouseLeave += (s, e) =>
            {
                btn.BackColor = theme.BtnSecBg;
                btn.ForeColor = theme.BtnSecText;
            };
            return btn;
        }

        #endregion

        #region Sidebar & Footer

        private void BuildSidebarContent()
        {
            sidebarPanel.Controls.Clear();
            navButtons.Clear();

            Panel brandPanel = new Panel();
            brandPanel.Dock = DockStyle.Top;
            brandPanel.Height = 56;
            brandPanel.BackColor = theme.Surface;
            brandPanel.Padding = new Padding(16, 12, 16, 8);

            Label lblB1 = new Label();
            lblB1.Text = "TUTU'S";
            lblB1.Font = new Font("Segoe UI", 12f, FontStyle.Bold);
            lblB1.ForeColor = theme.TextPrimary;
            lblB1.Dock = DockStyle.Top;
            lblB1.Height = 22;

            Label lblB2 = new Label();
            lblB2.Text = "OPTIMIZER SUITE";
            lblB2.Font = new Font("Segoe UI", 7.5f, FontStyle.Bold);
            lblB2.ForeColor = theme.Accent;
            lblB2.Dock = DockStyle.Top;
            lblB2.Height = 16;

            brandPanel.Controls.Add(lblB2);
            brandPanel.Controls.Add(lblB1);
            sidebarPanel.Controls.Add(brandPanel);

            Panel sep = new Panel();
            sep.Dock = DockStyle.Top;
            sep.Height = 1;
            sep.BackColor = theme.Border;
            sidebarPanel.Controls.Add(sep);

            Panel navContainer = new Panel();
            navContainer.Dock = DockStyle.Fill;
            navContainer.AutoScroll = true;
            sidebarPanel.Controls.Add(navContainer);
            navContainer.BringToFront();

            string[] tabNames = new string[] {
                "⚡  Início (Geral)",
                "📈  Saúde & Sensores",
                "💻  Sistema & CPU",
                "🛡️  Privacidade",
                "🌐  Rede & Ping",
                "🎮  Jogos (IFEO)",
                "🖱️  Periféricos",
                "🧹  Limpeza & Disco",
                "⚙️  Serviços & Apps",
                "📜  Console de Logs"
            };

            for (int i = tabNames.Length - 1; i >= 0; i--)
            {
                int idx = i;
                Button btn = new Button();
                btn.Text = "  " + tabNames[i];
                btn.Dock = DockStyle.Top;
                btn.Height = 42;
                btn.FlatStyle = FlatStyle.Flat;
                btn.FlatAppearance.BorderSize = 0;
                btn.TextAlign = ContentAlignment.MiddleLeft;
                btn.Font = new Font("Segoe UI", 9.5f, (idx == currentTabIndex) ? FontStyle.Bold : FontStyle.Regular);
                btn.ForeColor = (idx == currentTabIndex) ? theme.NavActiveText : theme.TextSecondary;
                btn.BackColor = (idx == currentTabIndex) ? theme.NavActiveBg : theme.Surface;
                btn.Cursor = Cursors.Hand;
                btn.Padding = new Padding(12, 0, 0, 0);
                btn.Tag = (idx == currentTabIndex);

                btn.Paint += (s, e) =>
                {
                    Button b = (Button)s;
                    if (b.Tag != null && (bool)b.Tag == true)
                    {
                        using (SolidBrush barBrush = new SolidBrush(theme.Accent))
                        {
                            e.Graphics.FillRectangle(barBrush, 0, 6, 3, b.Height - 12);
                        }
                    }
                };

                btn.Click += (s, e) => SelectTab(idx);
                btn.MouseEnter += (s, e) =>
                {
                    if (currentTabIndex != idx)
                        btn.BackColor = theme.NavHoverBg;
                };
                btn.MouseLeave += (s, e) =>
                {
                    if (currentTabIndex != idx)
                        btn.BackColor = theme.Surface;
                };

                navButtons.Insert(0, btn);
                navContainer.Controls.Add(btn);
            }
        }

        private void BuildFooterContent()
        {
            footerPanel.Controls.Clear();

            Panel btnPanel = new Panel();
            btnPanel.Dock = DockStyle.Right;
            btnPanel.Width = 400;
            btnPanel.BackColor = Color.Transparent;

            btnApply = new Button();
            btnApply.Text = "🚀 APLICAR";
            btnApply.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
            btnApply.ForeColor = Color.White;
            btnApply.BackColor = theme.Accent;
            btnApply.Size = new Size(125, 40);
            btnApply.Location = new Point(270, 2);
            btnApply.FlatStyle = FlatStyle.Flat;
            btnApply.FlatAppearance.BorderSize = 0;
            btnApply.Cursor = Cursors.Hand;
            btnApply.Click += (s, e) => RunOptimization(false);

            btnRevert = CreateSubtleButton("↩️ Reverter", (s, e) => RunOptimization(true), 110, 40);
            btnRevert.Location = new Point(152, 2);

            btnRecommended = CreateSubtleButton("⭐ Recomendados", (s, e) => SelectRecommendedTweaks(), 140, 40);
            btnRecommended.Location = new Point(4, 2);

            btnPanel.Controls.Add(btnRecommended);
            btnPanel.Controls.Add(btnRevert);
            btnPanel.Controls.Add(btnApply);

            Panel statusPanel = new Panel();
            statusPanel.Dock = DockStyle.Fill;
            statusPanel.BackColor = Color.Transparent;

            lblStatus = new Label();
            lblStatus.Text = "Pronto para otimizar.";
            lblStatus.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
            lblStatus.ForeColor = theme.TextPrimary;
            lblStatus.Dock = DockStyle.Top;
            lblStatus.Height = 20;

            lblSelectedCount = new Label();
            lblSelectedCount.Text = "0 selecionados";
            lblSelectedCount.Font = new Font("Segoe UI", 8.5f);
            lblSelectedCount.ForeColor = theme.Accent;
            lblSelectedCount.Dock = DockStyle.Top;
            lblSelectedCount.Height = 18;

            progressBar = new ProgressBar();
            progressBar.Dock = DockStyle.Bottom;
            progressBar.Height = 6;
            progressBar.Visible = false;

            statusPanel.Controls.Add(lblSelectedCount);
            statusPanel.Controls.Add(lblStatus);
            statusPanel.Controls.Add(progressBar);

            footerPanel.Controls.Add(statusPanel);
            footerPanel.Controls.Add(btnPanel);
        }

        private void SelectTab(int index)
        {
            currentTabIndex = index;
            for (int i = 0; i < navButtons.Count; i++)
            {
                bool active = (i == index);
                navButtons[i].Tag = active;
                navButtons[i].BackColor = active ? theme.NavActiveBg : theme.Surface;
                navButtons[i].ForeColor = active ? theme.NavActiveText : theme.TextSecondary;
                navButtons[i].Font = new Font("Segoe UI", 9.5f, active ? FontStyle.Bold : FontStyle.Regular);
                navButtons[i].Invalidate();
            }

            for (int i = 0; i < tabViews.Length; i++)
            {
                if (tabViews[i] != null)
                {
                    tabViews[i].Visible = (i == index);
                    if (i == index)
                    {
                        tabViews[i].BringToFront();
                    }
                }
            }
        }

        #endregion

        #region Criação das Abas de Conteúdo

        private void BuildTabViews()
        {
            contentPanel.Controls.Clear();
            tabViews = new Panel[10];

            tabViews[0] = CreateDashboardTab();
            tabViews[1] = CreateSensorsTab();
            tabViews[2] = CreateCategoryTab("Sistema");
            tabViews[3] = CreateCategoryTab("Privacidade");
            tabViews[4] = CreateCategoryTab("Rede");
            tabViews[5] = CreateGamesTab();
            tabViews[6] = CreateCategoryTab("Periféricos");
            tabViews[7] = CreateMaintenanceTab();
            tabViews[8] = CreateCategoryTab("Serviços");
            tabViews[9] = CreateLogsTab();

            for (int i = 0; i < tabViews.Length; i++)
            {
                if (tabViews[i] != null)
                {
                    tabViews[i].Dock = DockStyle.Fill;
                    tabViews[i].Visible = (i == currentTabIndex);
                    contentPanel.Controls.Add(tabViews[i]);
                }
            }
        }

        // ─────────────────────────────────────────────────────────────────────
        // ABA 0 — DASHBOARD
        // ─────────────────────────────────────────────────────────────────────
        private Panel CreateDashboardTab()
        {
            ModernScrollPanel scrollPanel = new ModernScrollPanel(theme);
            scrollPanel.Dock = DockStyle.Fill;
            Panel container = scrollPanel.Content;

            int y = 0;

            Panel banner = MakePageBanner("⚡ Painel Geral de Otimização",
                "Otimize seu Windows 10 e 11 com 1 clique para máxima taxa de quadros (FPS), menor latência e resposta instantânea.");
            banner.Location = new Point(0, y);
            banner.Width = container.Width;
            banner.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            container.Controls.Add(banner);
            y += banner.Height + 10;

            Panel secStats = MakeSectionHeader("💻 INFORMAÇÕES DO COMPUTADOR");
            secStats.Location = new Point(0, y);
            secStats.Width = container.Width;
            secStats.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            container.Controls.Add(secStats);
            y += secStats.Height + 4;

            Panel statsRow = MakeStatCardRow(new Panel[] {
                MakeStatCard("PROCESSADOR", sysInfo.CpuName, theme.Accent),
                MakeStatCard("MEMÓRIA RAM", string.Format("{0} MB Livres de {1} MB", sysInfo.FreeMemoryMb, sysInfo.TotalMemoryMb), theme.AccentGreen),
                MakeStatCard("SISTEMA OPERACIONAL", sysInfo.OsName, theme.AccentCyan),
                MakeStatCard("DISCO C: DISPONÍVEL", string.Format("{0} GB Livres", sysInfo.FreeDiskSpaceGb), theme.AccentAmber)
            });
            statsRow.Location = new Point(0, y);
            statsRow.Width = container.Width;
            statsRow.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            container.Controls.Add(statsRow);
            y += statsRow.Height + 12;

            Panel secHw = MakeSectionHeader("📈 SAÚDE DO HARDWARE — SSD, CPU E GPU");
            secHw.Location = new Point(0, y);
            secHw.Width = container.Width;
            secHw.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            container.Controls.Add(secHw);
            y += secHw.Height + 4;

            string diskHealth = "🟢 Saudável";
            string diskSummary = "Operação Normal";
            if (hwData != null && hwData.Disks != null && hwData.Disks.Count > 0)
            {
                DiskHealthInfo d = hwData.Disks[0];
                diskHealth = d.HealthStatus;
                string t = d.TemperatureC > 0 ? " • " + d.TemperatureC + " °C" : "";
                diskSummary = d.Model + " • " + d.MediaType + t;
            }

            string cpuHealth = "🟢 Normal / Estável";
            string cpuSummary = sysInfo.CpuName;
            if (hwData != null && hwData.Cpu != null)
            {
                cpuHealth = hwData.Cpu.StatusText;
                cpuSummary = hwData.Cpu.Name + " • " + hwData.Cpu.Cores + "C / " + hwData.Cpu.Threads + "T";
            }

            string gpuHealth = "🟢 Normal / Estável";
            string gpuSummary = "Driver OK";
            if (hwData != null && hwData.Gpu != null)
            {
                gpuHealth = hwData.Gpu.StatusText;
                gpuSummary = hwData.Gpu.Name + (hwData.Gpu.VramMb > 0 ? " • " + hwData.Gpu.VramMb + " MB" : "");
            }

            Panel hwRow = MakeStatCardRow(new Panel[] {
                MakeSensorMiniCard("💾 SAÚDE DO SSD", diskHealth, diskSummary, theme.AccentAmber),
                MakeSensorMiniCard("🌡️ PROCESSADOR (CPU)", cpuHealth, cpuSummary, theme.Accent),
                MakeSensorMiniCard("🎮 PLACA DE VÍDEO (GPU)", gpuHealth, gpuSummary, theme.AccentCyan)
            });
            hwRow.Location = new Point(0, y);
            hwRow.Width = container.Width;
            hwRow.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            container.Controls.Add(hwRow);
            y += hwRow.Height + 12;

            Panel secActions = MakeSectionHeader("⚡ AÇÕES RÁPIDAS DE 1-CLIQUE");
            secActions.Location = new Point(0, y);
            secActions.Width = container.Width;
            secActions.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            container.Controls.Add(secActions);
            y += secActions.Height + 4;

            Panel actionsGrid = MakeActionGrid(new ActionCardDef[] {
                new ActionCardDef(
                    "⚡ Otimização Rápida Recomendada",
                    "Aplica automaticamente todos os ajustes seguros de CPU, energia, rede e privacidade.",
                    "Otimizar Agora", theme.Accent,
                    (s, e) => { SelectRecommendedTweaks(); RunOptimization(false); }
                ),
                new ActionCardDef(
                    "🛡️ Criar Ponto de Restauração",
                    "Cria um ponto de restauração no Windows agora para segurança antes de otimizar.",
                    "Criar Ponto", theme.AccentCyan,
                    (s, e) => RunStandalone("Ponto de Restauração", (log) => TweakEngine.CreateSystemRestorePoint(log))
                ),
                new ActionCardDef(
                    "🧠 Esvaziar Memória RAM Instantaneamente",
                    "Limpa caches e working sets de processos sem fechar seus programas e navegadores abertos.",
                    "Esvaziar RAM", theme.AccentGreen,
                    (s, e) => RunStandalone("Limpeza de RAM", (log) => TweakEngine.EmptyRamMemory(log))
                ),
                new ActionCardDef(
                    "🧹 Limpeza Profunda de Disco (%TEMP%)",
                    "Remove arquivos temporários, lixo do Windows, logs e instaladores antigos acumulados.",
                    "Limpar Lixo", theme.AccentAmber,
                    (s, e) => RunStandalone("Limpeza de Disco", (log) => TweakEngine.CleanAllTemporaryFiles(log))
                )
            });
            actionsGrid.Location = new Point(0, y);
            actionsGrid.Width = container.Width;
            actionsGrid.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            container.Controls.Add(actionsGrid);
            y += actionsGrid.Height + 16;

            scrollPanel.SetContentHeight(y + 20);
            scrollPanel.HookWheelRecursive(container);
            return scrollPanel;
        }

        // ─────────────────────────────────────────────────────────────────────
        // ABA 1 — SAÚDE & SENSORES
        // ─────────────────────────────────────────────────────────────────────
        private Panel CreateSensorsTab()
        {
            ModernScrollPanel scrollPanel = new ModernScrollPanel(theme);
            scrollPanel.Dock = DockStyle.Fill;
            Panel container = scrollPanel.Content;

            int y = 0;

            Panel topBar = new Panel();
            topBar.Location = new Point(0, y);
            topBar.Height = 50;
            topBar.Width = container.Width;
            topBar.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            topBar.BackColor = Color.Transparent;

            Label lblT = new Label();
            lblT.Text = "📈 Saúde do Hardware & Sensores Térmicos";
            lblT.Font = new Font("Segoe UI", 12f, FontStyle.Bold);
            lblT.ForeColor = theme.TextPrimary;
            lblT.Location = new Point(4, 12);
            lblT.AutoSize = true;

            Button btnRefresh = CreateSubtleButton("⟳  Atualizar Sensores", null, 160, 32);
            btnRefresh.Location = new Point(topBar.Width - 170, 8);
            btnRefresh.Anchor = AnchorStyles.Top | AnchorStyles.Right;

            Label lblUpdated = new Label();
            lblUpdated.Text = "Última leitura: " + (hwData != null ? hwData.LastUpdated.ToString("HH:mm:ss") : "N/A");
            lblUpdated.Font = new Font("Segoe UI", 8.5f);
            lblUpdated.ForeColor = theme.TextSecondary;
            lblUpdated.Location = new Point(topBar.Width - 360, 16);
            lblUpdated.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            lblUpdated.AutoSize = true;

            btnRefresh.Click += (s, e) =>
            {
                btnRefresh.Enabled = false;
                btnRefresh.Text = "⏳ Lendo...";
                BackgroundWorker hw = new BackgroundWorker();
                hw.DoWork += (ws, we) => { we.Result = TweakEngine.GetHardwareSensors(); };
                hw.RunWorkerCompleted += (ws, we) =>
                {
                    if (we.Result != null) hwData = (HardwareMonitorData)we.Result;
                    lblUpdated.Text = "Última leitura: " + DateTime.Now.ToString("HH:mm:ss");
                    btnRefresh.Enabled = true;
                    btnRefresh.Text = "⟳  Atualizar Sensores";

                    Panel newTab = CreateSensorsTab();
                    newTab.Dock = DockStyle.Fill;
                    newTab.Visible = true;
                    Panel old = tabViews[1];
                    tabViews[1] = newTab;
                    contentPanel.Controls.Add(newTab);
                    newTab.BringToFront();
                    if (old != null) contentPanel.Controls.Remove(old);
                };
                hw.RunWorkerAsync();
            };

            topBar.Controls.Add(lblT);
            topBar.Controls.Add(lblUpdated);
            topBar.Controls.Add(btnRefresh);
            container.Controls.Add(topBar);
            y += topBar.Height + 10;

            // 1. Armazenamento SSD
            Panel secSSD = MakeSectionHeader("💾 ARMAZENAMENTO (SSD / NVMe / HDD)");
            secSSD.Location = new Point(0, y);
            secSSD.Width = container.Width;
            secSSD.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            container.Controls.Add(secSSD);
            y += secSSD.Height + 4;

            string dModel = "Não detectado", dType = "—", dHealth = "—", dTemp = "Não exposto";
            if (hwData != null && hwData.Disks != null && hwData.Disks.Count > 0)
            {
                DiskHealthInfo d = hwData.Disks[0];
                dModel = d.Model;
                dType = d.MediaType;
                dHealth = d.HealthStatus;
                dTemp = d.TemperatureC > 0 ? d.TemperatureC + " °C" : "Não exposto pelo driver";
            }

            Panel diskCards = MakeStatCardRow(new Panel[] {
                MakeStatCard("MODELO", dModel, theme.AccentAmber),
                MakeStatCard("TIPO DE MÍDIA", dType, theme.AccentAmber),
                MakeStatCard("SAÚDE (SMART)", dHealth, theme.AccentGreen),
                MakeStatCard("TEMPERATURA SSD", dTemp, theme.AccentAmber)
            });
            diskCards.Location = new Point(0, y);
            diskCards.Width = container.Width;
            diskCards.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            container.Controls.Add(diskCards);
            y += diskCards.Height + 12;

            // 2. Processador CPU
            Panel secCPU = MakeSectionHeader("🌡️ PROCESSADOR (CPU)");
            secCPU.Location = new Point(0, y);
            secCPU.Width = container.Width;
            secCPU.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            container.Controls.Add(secCPU);
            y += secCPU.Height + 4;

            string cpuName = sysInfo.CpuName, cpuCores = "—", cpuLoad = "—", cpuTemp = "Não exposto (ACPI)", cpuStatus = "Normal";
            if (hwData != null && hwData.Cpu != null)
            {
                cpuName = hwData.Cpu.Name;
                cpuCores = hwData.Cpu.Cores + " Núcleos / " + hwData.Cpu.Threads + " Threads";
                cpuLoad = hwData.Cpu.LoadPercent + "% de Uso";
                cpuTemp = hwData.Cpu.TemperatureC > 0 ? hwData.Cpu.TemperatureC + " °C" : "Sensor ACPI não exposto";
                cpuStatus = hwData.Cpu.StatusText;
            }

            Panel cpuCards = MakeStatCardRow(new Panel[] {
                MakeStatCard("PROCESSADOR", cpuName, theme.Accent),
                MakeStatCard("NÚCLEOS / THREADS", cpuCores, theme.Accent),
                MakeStatCard("CARGA ATUAL", cpuLoad, theme.AccentGreen),
                MakeStatCard("TEMPERATURA CPU", cpuTemp, theme.Accent)
            });
            cpuCards.Location = new Point(0, y);
            cpuCards.Width = container.Width;
            cpuCards.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            container.Controls.Add(cpuCards);
            y += cpuCards.Height + 8;

            Panel cpuNote = MakeNoteCard("Status Térmico: " + cpuStatus,
                "A leitura de temperatura requer sensor ACPI exposto pela placa-mãe. Em desktops sem ACPI exposto pelo BIOS, o status é monitorado pela carga percentual.");
            cpuNote.Location = new Point(0, y);
            cpuNote.Width = container.Width;
            cpuNote.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            container.Controls.Add(cpuNote);
            y += cpuNote.Height + 12;

            // 3. Placa de Vídeo GPU
            Panel secGPU = MakeSectionHeader("🎮 PLACA DE VÍDEO (GPU)");
            secGPU.Location = new Point(0, y);
            secGPU.Width = container.Width;
            secGPU.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            container.Controls.Add(secGPU);
            y += secGPU.Height + 4;

            string gpuName = "Não detectado", gpuVram = "—", gpuDriver = "—", gpuTemp = "Requer nvidia-smi", gpuStatus = "Normal";
            if (hwData != null && hwData.Gpu != null)
            {
                gpuName = hwData.Gpu.Name;
                gpuVram = hwData.Gpu.VramMb > 0 ? hwData.Gpu.VramMb + " MB VRAM" : "N/A";
                gpuDriver = hwData.Gpu.DriverVersion;
                gpuTemp = hwData.Gpu.TemperatureC > 0 ? hwData.Gpu.TemperatureC + " °C" : "Requer nvidia-smi";
                gpuStatus = hwData.Gpu.StatusText;
            }

            Panel gpuCards = MakeStatCardRow(new Panel[] {
                MakeStatCard("PLACA DE VÍDEO", gpuName, theme.AccentCyan),
                MakeStatCard("MEMÓRIA VRAM", gpuVram, theme.AccentCyan),
                MakeStatCard("VERSÃO DO DRIVER", gpuDriver, theme.AccentCyan),
                MakeStatCard("TEMPERATURA GPU", gpuTemp, theme.AccentCyan)
            });
            gpuCards.Location = new Point(0, y);
            gpuCards.Width = container.Width;
            gpuCards.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            container.Controls.Add(gpuCards);
            y += gpuCards.Height + 8;

            Panel gpuNote = MakeNoteCard("Status Operacional: " + gpuStatus,
                "A leitura de temperatura da GPU requer nvidia-smi (NVIDIA) configurado no PATH do sistema. Placas AMD e Intel utilizam a verificação de driver operacional.");
            gpuNote.Location = new Point(0, y);
            gpuNote.Width = container.Width;
            gpuNote.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            container.Controls.Add(gpuNote);
            y += gpuNote.Height + 16;

            scrollPanel.SetContentHeight(y + 20);
            scrollPanel.HookWheelRecursive(container);
            return scrollPanel;
        }

        // ─────────────────────────────────────────────────────────────────────
        // ABA DE CATEGORIA (Tweaks)
        // ─────────────────────────────────────────────────────────────────────
        private Panel CreateCategoryTab(string categoryName)
        {
            Panel root = new Panel();
            root.Dock = DockStyle.Fill;
            root.BackColor = Color.Transparent;

            Panel topBar = new Panel();
            topBar.Dock = DockStyle.Top;
            topBar.Height = 48;
            topBar.BackColor = Color.Transparent;

            Label lblT = new Label();
            lblT.Text = "Otimizações de " + categoryName;
            lblT.Font = new Font("Segoe UI", 11.5f, FontStyle.Bold);
            lblT.ForeColor = theme.TextPrimary;
            lblT.Location = new Point(4, 12);
            lblT.AutoSize = true;

            Button btnSelectAll = CreateSubtleButton("Marcar Todos", (s, e) => ToggleCategory(categoryName, true), 110, 30);
            btnSelectAll.Location = new Point(topBar.Width - 240, 8);
            btnSelectAll.Anchor = AnchorStyles.Top | AnchorStyles.Right;

            Button btnDeselectAll = CreateSubtleButton("Desmarcar Todos", (s, e) => ToggleCategory(categoryName, false), 120, 30);
            btnDeselectAll.Location = new Point(topBar.Width - 124, 8);
            btnDeselectAll.Anchor = AnchorStyles.Top | AnchorStyles.Right;

            topBar.Controls.Add(lblT);
            topBar.Controls.Add(btnSelectAll);
            topBar.Controls.Add(btnDeselectAll);
            root.Controls.Add(topBar);

            ModernScrollPanel scrollPanel = new ModernScrollPanel(theme);
            scrollPanel.Dock = DockStyle.Fill;
            Panel listContainer = scrollPanel.Content;

            int y = 4;
            foreach (OptimizationItem item in allTweaks)
            {
                if (item.Category == categoryName)
                {
                    Panel card = CreateTweakCard(item);
                    card.Location = new Point(4, y);
                    card.Width = listContainer.Width - 12;
                    card.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
                    listContainer.Controls.Add(card);
                    y += card.Height + 8;
                }
            }

            scrollPanel.SetContentHeight(y + 16);
            scrollPanel.HookWheelRecursive(listContainer);
            root.Controls.Add(scrollPanel);
            scrollPanel.BringToFront();

            return root;
        }

        private Panel CreateTweakCard(OptimizationItem item)
        {
            Panel card = new Panel();
            card.Height = 64;
            card.BackColor = theme.Card;
            card.Padding = new Padding(12, 8, 12, 8);
            card.Paint += (s, e) =>
            {
                using (Pen p = new Pen(theme.Border, 1f))
                {
                    e.Graphics.DrawRectangle(p, 0, 0, card.Width - 1, card.Height - 1);
                }
            };

            CheckBox chk = new CheckBox();
            chk.Checked = item.IsSelected;
            chk.Location = new Point(14, 22);
            chk.Size = new Size(18, 18);
            chk.Cursor = Cursors.Hand;
            chk.CheckedChanged += (s, e) =>
            {
                item.IsSelected = chk.Checked;
                UpdateSelectedCount();
            };
            tweakCheckBoxes[item] = chk;

            Label lblT = new Label();
            lblT.Text = item.Title;
            lblT.Font = new Font("Segoe UI", 9.5f, FontStyle.Bold);
            lblT.ForeColor = theme.TextPrimary;
            lblT.Location = new Point(42, 10);
            lblT.AutoSize = true;

            Label lblD = new Label();
            lblD.Text = item.Description;
            lblD.Font = new Font("Segoe UI", 8.5f);
            lblD.ForeColor = theme.TextSecondary;
            lblD.Location = new Point(43, 34);
            lblD.AutoSize = true;

            string badgeText = "RECOMENDADO";
            Color badgeColor = theme.AccentGreen;
            if (item.Safety == SafetyLevel.Optional)
            {
                badgeText = "OPCIONAL";
                badgeColor = theme.Accent;
            }
            else if (item.Safety == SafetyLevel.Advanced)
            {
                badgeText = "AVANÇADO";
                badgeColor = theme.AccentAmber;
            }

            Label lblBadge = new Label();
            lblBadge.Text = badgeText;
            lblBadge.Font = new Font("Segoe UI", 7.5f, FontStyle.Bold);
            lblBadge.ForeColor = badgeColor;
            lblBadge.Location = new Point(card.Width - 120, 22);
            lblBadge.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            lblBadge.AutoSize = true;

            card.Controls.Add(chk);
            card.Controls.Add(lblT);
            card.Controls.Add(lblD);
            card.Controls.Add(lblBadge);

            card.Click += (s, e) => chk.Checked = !chk.Checked;
            lblT.Click += (s, e) => chk.Checked = !chk.Checked;
            lblD.Click += (s, e) => chk.Checked = !chk.Checked;

            return card;
        }

        // ─────────────────────────────────────────────────────────────────────
        // ABA 5 — JOGOS (IFEO)
        // ─────────────────────────────────────────────────────────────────────
        private Panel CreateGamesTab()
        {
            Panel root = new Panel();
            root.Dock = DockStyle.Fill;
            root.BackColor = Color.Transparent;

            Panel topBar = new Panel();
            topBar.Dock = DockStyle.Top;
            topBar.Height = 74;
            topBar.BackColor = Color.Transparent;

            Label lblT = new Label();
            lblT.Text = "🎮 Catálogo de Otimização de Jogos (Prioridade de CPU/GPU)";
            lblT.Font = new Font("Segoe UI", 11.5f, FontStyle.Bold);
            lblT.ForeColor = theme.TextPrimary;
            lblT.Location = new Point(4, 6);
            lblT.AutoSize = true;

            Label lblSub = new Label();
            lblSub.Text = "Aplica prioridade alta (IFEO) no registro para jogos selecionados, eliminando micro-travamentos e estabilizando FPS.";
            lblSub.Font = new Font("Segoe UI", 8.5f);
            lblSub.ForeColor = theme.TextSecondary;
            lblSub.Location = new Point(6, 28);
            lblSub.AutoSize = true;

            txtGameSearch = new TextBox();
            txtGameSearch.Font = new Font("Segoe UI", 9f);
            txtGameSearch.ForeColor = theme.TextPrimary;
            txtGameSearch.BackColor = theme.Card;
            txtGameSearch.BorderStyle = BorderStyle.FixedSingle;
            txtGameSearch.Size = new Size(180, 26);
            txtGameSearch.Location = new Point(6, 48);
            txtGameSearch.TextChanged += (s, e) => FilterGames(txtGameSearch.Text);

            Button btnSelectAll = CreateSubtleButton("Marcar Todos", (s, e) => ToggleAllGames(true), 100, 26);
            btnSelectAll.Location = new Point(194, 48);

            Button btnDeselectAll = CreateSubtleButton("Desmarcar Todos", (s, e) => ToggleAllGames(false), 120, 26);
            btnDeselectAll.Location = new Point(300, 48);

            Button btnAdd = CreateSubtleButton("+ Adicionar Outro Jogo (.exe)", (s, e) => AddCustomGame(), 190, 26);
            btnAdd.Location = new Point(426, 48);

            topBar.Controls.Add(lblT);
            topBar.Controls.Add(lblSub);
            topBar.Controls.Add(txtGameSearch);
            topBar.Controls.Add(btnSelectAll);
            topBar.Controls.Add(btnDeselectAll);
            topBar.Controls.Add(btnAdd);
            root.Controls.Add(topBar);

            ModernScrollPanel scrollPanel = new ModernScrollPanel(theme);
            scrollPanel.Dock = DockStyle.Fill;
            Panel container = scrollPanel.Content;

            flowGames = new FlowLayoutPanel();
            flowGames.Location = new Point(0, 0);
            flowGames.Width = container.Width;
            flowGames.AutoSize = true;
            flowGames.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            flowGames.WrapContents = true;
            flowGames.Padding = new Padding(4);
            flowGames.BackColor = Color.Transparent;

            container.Controls.Add(flowGames);
            root.Controls.Add(scrollPanel);
            scrollPanel.BringToFront();

            PopulateGamesGrid("");
            scrollPanel.SetContentHeight(flowGames.Height + 40);
            scrollPanel.HookWheelRecursive(container);
            return root;
        }

        private void PopulateGamesGrid(string search)
        {
            flowGames.SuspendLayout();
            flowGames.Controls.Clear();
            string filter = search.Trim().ToLower();

            foreach (GameItem g in allGames)
            {
                if (string.IsNullOrEmpty(filter) || g.Name.ToLower().Contains(filter) || g.ExeNames[0].ToLower().Contains(filter))
                {
                    Panel tile = new Panel();
                    tile.Size = new Size(240, 56);
                    tile.BackColor = theme.Card;
                    tile.Margin = new Padding(4);
                    tile.Padding = new Padding(8);
                    tile.Paint += (s, e) =>
                    {
                        using (Pen p = new Pen(theme.Border, 1f))
                        {
                            e.Graphics.DrawRectangle(p, 0, 0, tile.Width - 1, tile.Height - 1);
                        }
                    };

                    CheckBox chk = new CheckBox();
                    chk.Checked = g.IsSelected;
                    chk.Location = new Point(10, 18);
                    chk.Size = new Size(18, 18);
                    chk.Cursor = Cursors.Hand;
                    chk.CheckedChanged += (s, e) => g.IsSelected = chk.Checked;
                    gameCheckBoxes[g] = chk;

                    Label lName = new Label();
                    lName.Text = g.Name;
                    lName.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
                    lName.ForeColor = theme.TextPrimary;
                    lName.Location = new Point(34, 8);
                    lName.AutoSize = true;

                    Label lExe = new Label();
                    lExe.Text = g.ExeNames[0];
                    lExe.Font = new Font("Segoe UI", 7.5f);
                    lExe.ForeColor = theme.TextSecondary;
                    lExe.Location = new Point(35, 28);
                    lExe.AutoSize = true;

                    tile.Controls.Add(chk);
                    tile.Controls.Add(lName);
                    tile.Controls.Add(lExe);

                    tile.Click += (s, e) => chk.Checked = !chk.Checked;
                    lName.Click += (s, e) => chk.Checked = !chk.Checked;
                    lExe.Click += (s, e) => chk.Checked = !chk.Checked;

                    flowGames.Controls.Add(tile);
                }
            }
            flowGames.ResumeLayout();
        }

        private void FilterGames(string s)
        {
            PopulateGamesGrid(s);
        }

        private void ToggleAllGames(bool c)
        {
            foreach (GameItem g in allGames)
            {
                g.IsSelected = c;
                if (gameCheckBoxes.ContainsKey(g))
                    gameCheckBoxes[g].Checked = c;
            }
        }

        private void AddCustomGame()
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = "Executáveis (*.exe)|*.exe";
                ofd.Title = "Selecione o executável do jogo";
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    string file = Path.GetFileName(ofd.FileName);
                    string name = Path.GetFileNameWithoutExtension(file);
                    allGames.Insert(0, new GameItem(name, file, true));
                    PopulateGamesGrid(txtGameSearch.Text);
                    MessageBox.Show(this, "Jogo \"" + name + "\" adicionado à lista com sucesso!", "Adicionado", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        // ─────────────────────────────────────────────────────────────────────
        // ABA 7 — LIMPEZA & DISCO
        // ─────────────────────────────────────────────────────────────────────
        private Panel CreateMaintenanceTab()
        {
            ModernScrollPanel scrollPanel = new ModernScrollPanel(theme);
            scrollPanel.Dock = DockStyle.Fill;
            Panel container = scrollPanel.Content;

            int y = 0;

            Panel banner = MakePageBanner("🧹 Limpeza Profunda & Manutenção do Sistema",
                "Ferramentas completas de manutenção preventiva para limpar gigabytes de arquivos temporários, reparar integridade e purgar memória.");
            banner.Location = new Point(0, y);
            banner.Width = container.Width;
            banner.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            container.Controls.Add(banner);
            y += banner.Height + 10;

            Panel actionsGrid = MakeActionGrid(new ActionCardDef[] {
                new ActionCardDef(
                    "🧹 Limpar Arquivos Temporários (%TEMP%)",
                    "Remove pastas temporárias de usuários, Windows, cache Prefetch e relatórios de erro.",
                    "Executar Limpeza", theme.AccentAmber,
                    (s, e) => RunStandalone("Limpeza de Temporários", (log) => TweakEngine.CleanAllTemporaryFiles(log))
                ),
                new ActionCardDef(
                    "🖼️ Resetar Cache de Miniaturas e Ícones",
                    "Corrige ícones corrompidos e invisíveis e compacta a base IconCache.db do Explorer.",
                    "Resetar Cache", theme.Accent,
                    (s, e) => RunStandalone("Reset de Ícones", (log) => TweakEngine.ResetIconAndThumbCache(log))
                ),
                new ActionCardDef(
                    "🌐 Limpar Cache DNS e Rede (Winsock)",
                    "Redefine o cache de rede e resolve problemas de lentidão, perda de pacotes e DNS corrompido.",
                    "Limpar Rede", theme.AccentCyan,
                    (s, e) => RunStandalone("Reset de Rede", (log) => TweakEngine.FlushDnsAndWinsock(log))
                ),
                new ActionCardDef(
                    "🧠 Purgar Memória RAM",
                    "Esvazia a memória em espera e working sets dos programas sem reiniciar o computador.",
                    "Purgar RAM", theme.AccentGreen,
                    (s, e) => RunStandalone("Limpeza de RAM", (log) => TweakEngine.EmptyRamMemory(log))
                ),
                new ActionCardDef(
                    "🛠️ Reparar Arquivos do Sistema (SFC / DISM)",
                    "Varredura completa por arquivos corrompidos no Windows e reparo automatizado via imagem oficial.",
                    "Iniciar Reparo", theme.AccentAmber,
                    (s, e) => RunStandalone("Reparo de Arquivos", (log) => TweakEngine.RunSystemFileCheck(log))
                ),
                new ActionCardDef(
                    "🗑️ Remover Bloatwares do Windows (UWP)",
                    "Desinstala com segurança aplicativos inúteis pré-instalados pela Microsoft (Cortana, Mapas, Dicas, etc.).",
                    "Remover Bloatware", theme.AccentRed,
                    (s, e) => RunStandalone("Remoção de Bloatwares", (log) => TweakEngine.RemoveBloatwareApps(log))
                )
            });
            actionsGrid.Location = new Point(0, y);
            actionsGrid.Width = container.Width;
            actionsGrid.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            container.Controls.Add(actionsGrid);
            y += actionsGrid.Height + 16;

            scrollPanel.SetContentHeight(y + 20);
            scrollPanel.HookWheelRecursive(container);
            return scrollPanel;
        }

        // ─────────────────────────────────────────────────────────────────────
        // ABA 9 — CONSOLE DE LOGS
        // ─────────────────────────────────────────────────────────────────────
        private Panel CreateLogsTab()
        {
            Panel root = new Panel();
            root.Dock = DockStyle.Fill;
            root.BackColor = Color.Transparent;

            Panel topBar = new Panel();
            topBar.Dock = DockStyle.Top;
            topBar.Height = 46;
            topBar.BackColor = Color.Transparent;

            Label lblT = new Label();
            lblT.Text = "📜 Console de Execução e Logs em Tempo Real";
            lblT.Font = new Font("Segoe UI", 11.5f, FontStyle.Bold);
            lblT.ForeColor = theme.TextPrimary;
            lblT.Location = new Point(4, 10);
            lblT.AutoSize = true;

            Button btnClear = CreateSubtleButton("Limpar Log", (s, e) => logBox.Clear(), 95, 28);
            btnClear.Location = new Point(topBar.Width - 230, 8);
            btnClear.Anchor = AnchorStyles.Top | AnchorStyles.Right;

            Button btnSave = CreateSubtleButton("Salvar em Arquivo", (s, e) => SaveLogToFile(), 125, 28);
            btnSave.Location = new Point(topBar.Width - 128, 8);
            btnSave.Anchor = AnchorStyles.Top | AnchorStyles.Right;

            topBar.Controls.Add(lblT);
            topBar.Controls.Add(btnClear);
            topBar.Controls.Add(btnSave);
            root.Controls.Add(topBar);

            // Painel para acomodar o RichTextBox com a scrollbar escura/clara integrada
            Panel logContainer = new Panel();
            logContainer.Dock = DockStyle.Fill;
            logContainer.BackColor = theme.Surface;

            logBox = new RichTextBox();
            logBox.Dock = DockStyle.Fill;
            logBox.BackColor = theme.Surface;
            logBox.ForeColor = theme.TextPrimary;
            logBox.Font = new Font("Consolas", 9.5f);
            logBox.BorderStyle = BorderStyle.None;
            logBox.ReadOnly = true;
            logBox.ScrollBars = RichTextBoxScrollBars.Vertical;

            // Aplica tema nativo na scrollbar do RichTextBox (Dark Mode nativo do Windows 10/11)
            logBox.HandleCreated += (s, e) =>
            {
                SetWindowTheme(logBox.Handle, theme.IsDark ? "DarkMode_Explorer" : "Explorer", null);
            };

            logContainer.Controls.Add(logBox);
            root.Controls.Add(logContainer);
            logContainer.BringToFront();

            AppendLog("Tutu's Optimizer 2026 inicializado com sucesso.", theme.Accent);
            AppendLog("Tema do sistema detectado: " + (theme.IsDark ? "Modo Escuro (Dark)" : "Modo Claro (Light)"), theme.TextSecondary);
            AppendLog("Sistema operacional detectado: " + sysInfo.OsName, theme.TextSecondary);
            AppendLog("Privilégios de Administrador ativos.", theme.AccentGreen);

            return root;
        }

        #endregion

        #region Helpers de Criação de Widgets Responsivos

        private Panel MakePageBanner(string title, string subtitle)
        {
            Panel p = new Panel();
            p.Height = 72;
            p.BackColor = theme.Card;
            p.Padding = new Padding(16, 12, 16, 12);
            p.Paint += (s, e) =>
            {
                using (Pen pen = new Pen(theme.Border, 1f))
                {
                    e.Graphics.DrawRectangle(pen, 0, 0, p.Width - 1, p.Height - 1);
                }
            };

            Label l1 = new Label();
            l1.Text = title;
            l1.Font = new Font("Segoe UI", 12f, FontStyle.Bold);
            l1.ForeColor = theme.TextPrimary;
            l1.Dock = DockStyle.Top;
            l1.Height = 24;

            Label l2 = new Label();
            l2.Text = subtitle;
            l2.Font = new Font("Segoe UI", 8.5f);
            l2.ForeColor = theme.TextSecondary;
            l2.Dock = DockStyle.Top;
            l2.Height = 20;

            p.Controls.Add(l2);
            p.Controls.Add(l1);
            return p;
        }

        private Panel MakeSectionHeader(string title)
        {
            Panel p = new Panel();
            p.Height = 30;
            p.BackColor = Color.Transparent;
            p.Padding = new Padding(4, 6, 4, 2);

            Label l = new Label();
            l.Text = title;
            l.Font = new Font("Segoe UI", 8f, FontStyle.Bold);
            l.ForeColor = theme.Accent;
            l.Dock = DockStyle.Fill;

            p.Controls.Add(l);
            return p;
        }

        private Panel MakeStatCardRow(Panel[] cards)
        {
            Panel p = new Panel();
            p.Height = 88;
            p.BackColor = Color.Transparent;
            p.Padding = new Padding(0, 2, 0, 6);

            TableLayoutPanel tbl = new TableLayoutPanel();
            tbl.Dock = DockStyle.Fill;
            tbl.RowCount = 1;
            tbl.ColumnCount = cards.Length;

            float pct = 100f / cards.Length;
            for (int i = 0; i < cards.Length; i++)
            {
                tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, pct));
                tbl.Controls.Add(cards[i], i, 0);
            }
            tbl.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));

            p.Controls.Add(tbl);
            return p;
        }

        private Panel MakeStatCard(string title, string value, Color accent)
        {
            Panel card = new Panel();
            card.Dock = DockStyle.Fill;
            card.BackColor = theme.Card;
            card.Margin = new Padding(4);
            card.Padding = new Padding(12, 10, 12, 10);
            card.Paint += (s, e) =>
            {
                using (Pen pen = new Pen(theme.Border, 1f))
                {
                    e.Graphics.DrawRectangle(pen, 0, 0, card.Width - 1, card.Height - 1);
                }
            };

            Label lT = new Label();
            lT.Text = title;
            lT.Font = new Font("Segoe UI", 7.5f, FontStyle.Bold);
            lT.ForeColor = accent;
            lT.Dock = DockStyle.Top;
            lT.Height = 18;

            Label lV = new Label();
            lV.Text = value;
            lV.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
            lV.ForeColor = theme.TextPrimary;
            lV.Dock = DockStyle.Fill;

            card.Controls.Add(lV);
            card.Controls.Add(lT);
            return card;
        }

        private Panel MakeSensorMiniCard(string title, string status, string summary, Color accent)
        {
            Panel card = new Panel();
            card.Dock = DockStyle.Fill;
            card.BackColor = theme.Card;
            card.Margin = new Padding(4);
            card.Padding = new Padding(12, 8, 12, 8);
            card.Paint += (s, e) =>
            {
                using (Pen pen = new Pen(theme.Border, 1f))
                {
                    e.Graphics.DrawRectangle(pen, 0, 0, card.Width - 1, card.Height - 1);
                }
            };

            Label lT = new Label();
            lT.Text = title;
            lT.Font = new Font("Segoe UI", 7.5f, FontStyle.Bold);
            lT.ForeColor = accent;
            lT.Dock = DockStyle.Top;
            lT.Height = 17;

            Label lS = new Label();
            lS.Text = status;
            lS.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
            lS.ForeColor = theme.TextPrimary;
            lS.Dock = DockStyle.Top;
            lS.Height = 22;

            Label lD = new Label();
            lD.Text = summary;
            lD.Font = new Font("Segoe UI", 7.5f);
            lD.ForeColor = theme.TextSecondary;
            lD.Dock = DockStyle.Fill;

            card.Controls.Add(lD);
            card.Controls.Add(lS);
            card.Controls.Add(lT);
            return card;
        }

        private Panel MakeNoteCard(string header, string noteText)
        {
            Panel card = new Panel();
            card.Height = 52;
            card.BackColor = theme.Card;
            card.Margin = new Padding(4, 2, 4, 8);
            card.Padding = new Padding(12, 6, 12, 6);
            card.Paint += (s, e) =>
            {
                using (Pen pen = new Pen(theme.Border, 1f))
                {
                    e.Graphics.DrawRectangle(pen, 0, 0, card.Width - 1, card.Height - 1);
                }
            };

            Label lH = new Label();
            lH.Text = header;
            lH.Font = new Font("Segoe UI", 8.5f, FontStyle.Bold);
            lH.ForeColor = theme.TextPrimary;
            lH.Dock = DockStyle.Top;
            lH.Height = 20;

            Label lN = new Label();
            lN.Text = noteText;
            lN.Font = new Font("Segoe UI", 7.5f);
            lN.ForeColor = theme.TextSecondary;
            lN.Dock = DockStyle.Fill;

            card.Controls.Add(lN);
            card.Controls.Add(lH);
            return card;
        }

        private Panel MakeActionGrid(ActionCardDef[] actions)
        {
            int rows = (int)Math.Ceiling(actions.Length / 2.0);
            int height = rows * 130 + 8;

            Panel p = new Panel();
            p.Height = height;
            p.BackColor = Color.Transparent;
            p.Padding = new Padding(0, 2, 0, 6);

            TableLayoutPanel tbl = new TableLayoutPanel();
            tbl.Dock = DockStyle.Fill;
            tbl.ColumnCount = 2;
            tbl.RowCount = rows;
            tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
            tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));

            for (int r = 0; r < rows; r++)
                tbl.RowStyles.Add(new RowStyle(SizeType.Percent, 100f / rows));

            for (int i = 0; i < actions.Length; i++)
            {
                tbl.Controls.Add(MakeActionCard(actions[i]), i % 2, i / 2);
            }

            p.Controls.Add(tbl);
            return p;
        }

        private Panel MakeActionCard(ActionCardDef item)
        {
            Panel card = new Panel();
            card.Dock = DockStyle.Fill;
            card.BackColor = theme.Card;
            card.Margin = new Padding(4);
            card.Padding = new Padding(14, 10, 14, 10);
            card.Paint += (s, e) =>
            {
                using (Pen pen = new Pen(theme.Border, 1f))
                {
                    e.Graphics.DrawRectangle(pen, 0, 0, card.Width - 1, card.Height - 1);
                }
            };

            Label lT = new Label();
            lT.Text = item.Title;
            lT.Font = new Font("Segoe UI", 9.5f, FontStyle.Bold);
            lT.ForeColor = theme.TextPrimary;
            lT.Dock = DockStyle.Top;
            lT.Height = 22;

            Label lD = new Label();
            lD.Text = item.Desc;
            lD.Font = new Font("Segoe UI", 8.5f);
            lD.ForeColor = theme.TextSecondary;
            lD.Dock = DockStyle.Fill;

            Button btn = new Button();
            btn.Text = item.BtnText;
            btn.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
            btn.ForeColor = Color.White;
            btn.BackColor = item.AccentColor;
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            btn.Dock = DockStyle.Bottom;
            btn.Height = 32;
            btn.Cursor = Cursors.Hand;
            btn.Click += item.ClickHandler;

            card.Controls.Add(lD);
            card.Controls.Add(btn);
            card.Controls.Add(lT);
            return card;
        }

        #endregion

        #region Seleção e Execução de Otimizações

        private void ToggleCategory(string category, bool check)
        {
            foreach (OptimizationItem item in allTweaks)
            {
                if (item.Category == category)
                {
                    item.IsSelected = check;
                    if (tweakCheckBoxes.ContainsKey(item))
                        tweakCheckBoxes[item].Checked = check;
                }
            }
            UpdateSelectedCount();
        }

        private void SelectRecommendedTweaks()
        {
            foreach (OptimizationItem item in allTweaks)
            {
                bool rec = (item.Safety == SafetyLevel.Recommended);
                item.IsSelected = rec;
                if (tweakCheckBoxes.ContainsKey(item))
                    tweakCheckBoxes[item].Checked = rec;
            }
            UpdateSelectedCount();
            AppendLog("Otimizações recomendadas selecionadas automaticamente.", theme.AccentGreen);
        }

        private void UpdateSelectedCount()
        {
            int count = 0;
            foreach (OptimizationItem item in allTweaks)
            {
                if (item.IsSelected) count++;
            }
            lblSelectedCount.Text = string.Format("{0} otimizações selecionadas", count);
        }

        private void AppendLog(string message, Color color)
        {
            if (logBox == null) return;
            if (logBox.InvokeRequired)
            {
                logBox.Invoke(new Action(() => AppendLog(message, color)));
                return;
            }

            string timestamp = DateTime.Now.ToString("HH:mm:ss");
            logBox.SelectionStart = logBox.TextLength;
            logBox.SelectionLength = 0;

            logBox.SelectionColor = theme.TextSecondary;
            logBox.AppendText("[" + timestamp + "] ");

            logBox.SelectionColor = color;
            logBox.AppendText(message + Environment.NewLine);
            logBox.ScrollToCaret();
        }

        private void SaveLogToFile()
        {
            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                sfd.Filter = "Arquivo de Texto (*.txt)|*.txt";
                sfd.FileName = "TutusOptimizer_Log_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".txt";
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    File.WriteAllText(sfd.FileName, logBox.Text);
                    MessageBox.Show(this, "Log salvo com sucesso!", "Sucesso", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        private void RunStandalone(string name, Action<Action<string>> action)
        {
            SelectTab(9);
            AppendLog("==================================================", theme.Accent);
            AppendLog("Iniciando: " + name, theme.Accent);
            AppendLog("==================================================", theme.Accent);

            BackgroundWorker w = new BackgroundWorker();
            w.DoWork += (s, e) =>
            {
                try
                {
                    action((msg) => AppendLog("  ⚡ " + msg, theme.TextSecondary));
                }
                catch (Exception ex)
                {
                    AppendLog("[ERRO]: " + ex.Message, theme.AccentRed);
                }
            };
            w.RunWorkerCompleted += (s, e) =>
            {
                AppendLog(name + " finalizado!", theme.AccentGreen);
                MessageBox.Show(this, name + " finalizado com sucesso!", "Concluído", MessageBoxButtons.OK, MessageBoxIcon.Information);
            };
            w.RunWorkerAsync();
        }

        private void RunOptimization(bool isRevert)
        {
            if (worker.IsBusy) return;

            List<OptimizationItem> targets = new List<OptimizationItem>();
            foreach (OptimizationItem item in allTweaks)
            {
                if (item.IsSelected)
                {
                    if (!isRevert && item.ApplyAction != null) targets.Add(item);
                    if (isRevert && item.CanRevert && item.RevertAction != null) targets.Add(item);
                }
            }

            List<GameItem> selectedGames = new List<GameItem>();
            foreach (GameItem g in allGames)
            {
                if (g.IsSelected) selectedGames.Add(g);
            }

            if (targets.Count == 0 && selectedGames.Count == 0)
            {
                MessageBox.Show(this, "Nenhuma otimização selecionada. Por favor, marque ao menos um item.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            btnApply.Enabled = false;
            btnRevert.Enabled = false;
            btnRecommended.Enabled = false;
            progressBar.Value = 0;
            progressBar.Visible = true;
            lblStatus.Text = isRevert ? "Revertendo otimizações..." : "Aplicando otimizações...";

            SelectTab(9);
            worker.RunWorkerAsync(new object[] { targets, selectedGames, isRevert });
        }

        private void Worker_DoWork(object sender, DoWorkEventArgs e)
        {
            object[] args = (object[])e.Argument;
            List<OptimizationItem> targets = (List<OptimizationItem>)args[0];
            List<GameItem> games = (List<GameItem>)args[1];
            bool isRevert = (bool)args[2];

            int totalSteps = targets.Count + (games.Count > 0 ? 1 : 0);
            int currentStep = 0;

            AppendLog("==================================================", theme.Accent);
            AppendLog(isRevert ? "INICIANDO PROCESSO DE REVERSÃO..." : "INICIANDO APLICAÇÃO DE OTIMIZAÇÕES...", theme.Accent);
            AppendLog("==================================================", theme.Accent);

            for (int i = 0; i < targets.Count; i++)
            {
                OptimizationItem item = targets[i];
                currentStep++;
                int percent = (int)((float)currentStep / totalSteps * 100);

                worker.ReportProgress(percent, item.Title);

                try
                {
                    if (isRevert)
                    {
                        item.RevertAction((msg, p) => AppendLog("  ↩️ " + msg, theme.TextSecondary));
                        AppendLog("[REVERTIDO] " + item.Title, theme.AccentAmber);
                    }
                    else
                    {
                        item.ApplyAction((msg, p) => AppendLog("  ⚡ " + msg, theme.TextSecondary));
                        AppendLog("[APLICADO] " + item.Title, theme.AccentGreen);
                    }
                }
                catch (Exception ex)
                {
                    AppendLog("[ERRO em " + item.Title + "]: " + ex.Message, theme.AccentRed);
                }
            }

            if (games.Count > 0)
            {
                currentStep++;
                worker.ReportProgress(100, "Configurando jogos...");
                AppendLog(string.Format("Configurando prioridade IFEO para {0} jogos...", games.Count), theme.AccentCyan);

                foreach (GameItem g in games)
                {
                    foreach (string exe in g.ExeNames)
                    {
                        TweakEngine.SetGamePriority(exe, !isRevert);
                    }
                    AppendLog(string.Format("  🎮 Jogo {0}: {1}", g.Name, isRevert ? "Prioridade padrão" : "Alta Prioridade (High)"), theme.AccentGreen);
                }
            }

            AppendLog("==================================================", theme.Accent);
            AppendLog(isRevert ? "REVERSÃO CONCLUÍDA COM SUCESSO!" : "TODAS AS OTIMIZAÇÕES FORAM APLICADAS COM SUCESSO!", theme.AccentGreen);
            AppendLog("==================================================", theme.Accent);

            e.Result = isRevert;
        }

        private void Worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            progressBar.Value = Math.Min(100, Math.Max(0, e.ProgressPercentage));
            lblStatus.Text = e.UserState != null ? e.UserState.ToString() : "Progresso...";
        }

        private void Worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            progressBar.Visible = false;
            btnApply.Enabled = true;
            btnRevert.Enabled = true;
            btnRecommended.Enabled = true;

            bool isRevert = e.Result is bool ? (bool)e.Result : false;
            lblStatus.Text = isRevert ? "Reversão concluída!" : "Otimizações aplicadas com sucesso!";

            MessageBox.Show(
                this,
                isRevert ? "As alterações foram revertidas com sucesso!" : "Otimizações aplicadas com sucesso!\nSeu sistema agora está configurado para máxima performance.",
                "Tutu's Optimizer 2026",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information
            );
        }

        #endregion

        #region Gerenciamento de Janela e Tray

        public void ToggleMaximizeWindow()
        {
            if (this.WindowState == FormWindowState.Maximized)
            {
                this.WindowState = FormWindowState.Normal;
            }
            else
            {
                this.MaximizedBounds = Screen.FromHandle(this.Handle).WorkingArea;
                this.WindowState = FormWindowState.Maximized;
            }
            UpdateMaximizeButton();
        }

        private void UpdateMaximizeButton()
        {
            if (btnMaximize != null)
            {
                btnMaximize.SetMaximized(this.WindowState == FormWindowState.Maximized);
            }
        }

        public void RestoreAndOpen()
        {
            this.Show();
            if (this.WindowState == FormWindowState.Minimized)
            {
                this.WindowState = FormWindowState.Normal;
            }
            this.BringToFront();
            this.Activate();
            UpdateMaximizeButton();
        }

        private void InitializeTrayIcon()
        {
            try
            {
                trayMenu = new ContextMenuStrip();
                trayMenu.BackColor = theme.Surface;
                trayMenu.ForeColor = theme.TextPrimary;

                ToolStripMenuItem iOpen = new ToolStripMenuItem("🚀 Abrir Janela");
                iOpen.Click += (s, e) => RestoreAndOpen();

                ToolStripMenuItem iExpand = new ToolStripMenuItem("⛶ Expandir / Restaurar");
                iExpand.Click += (s, e) => ToggleMaximizeWindow();

                ToolStripMenuItem iMin = new ToolStripMenuItem("— Minimizar");
                iMin.Click += (s, e) => this.WindowState = FormWindowState.Minimized;

                ToolStripMenuItem iOpt = new ToolStripMenuItem("⚡ Otimização Rápida");
                iOpt.Click += (s, e) =>
                {
                    RestoreAndOpen();
                    SelectRecommendedTweaks();
                    RunOptimization(false);
                };

                ToolStripMenuItem iRam = new ToolStripMenuItem("🧠 Esvaziar RAM");
                iRam.Click += (s, e) =>
                {
                    RestoreAndOpen();
                    RunStandalone("Limpeza de RAM", (log) => TweakEngine.EmptyRamMemory(log));
                };

                ToolStripMenuItem iClose = new ToolStripMenuItem("✕ Fechar");
                iClose.ForeColor = theme.AccentRed;
                iClose.Click += (s, e) => this.Close();

                trayMenu.Items.Add(iOpen);
                trayMenu.Items.Add(iExpand);
                trayMenu.Items.Add(iMin);
                trayMenu.Items.Add(new ToolStripSeparator());
                trayMenu.Items.Add(iOpt);
                trayMenu.Items.Add(iRam);
                trayMenu.Items.Add(new ToolStripSeparator());
                trayMenu.Items.Add(iClose);

                trayIcon = new NotifyIcon();
                trayIcon.Text = "Tutu's Windows Optimizer 2026";
                if (this.Icon != null)
                {
                    trayIcon.Icon = this.Icon;
                }
                trayIcon.ContextMenuStrip = trayMenu;
                trayIcon.Visible = true;
                trayIcon.DoubleClick += (s, e) => RestoreAndOpen();
            }
            catch { }
        }

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.Style |= 0x00020000; // WS_MINIMIZEBOX
                cp.ClassStyle |= 0x8;    // CS_DBLCLKS
                return cp;
            }
        }

        protected override void WndProc(ref Message m)
        {
            const int WM_NCHITTEST = 0x84;
            const int HTCLIENT = 1;
            const int HTLEFT = 10;
            const int HTRIGHT = 11;
            const int HTTOP = 12;
            const int HTTOPLEFT = 13;
            const int HTTOPRIGHT = 14;
            const int HTBOTTOM = 15;
            const int HTBOTTOMLEFT = 16;
            const int HTBOTTOMRIGHT = 17;

            if (m.Msg == WM_NCHITTEST && this.WindowState != FormWindowState.Maximized)
            {
                base.WndProc(ref m);
                if ((int)m.Result == HTCLIENT)
                {
                    Point p = this.PointToClient(new Point(m.LParam.ToInt32()));
                    int border = 8;

                    if (p.X <= border && p.Y <= border)
                        m.Result = (IntPtr)HTTOPLEFT;
                    else if (p.X >= this.ClientSize.Width - border && p.Y <= border)
                        m.Result = (IntPtr)HTTOPRIGHT;
                    else if (p.X <= border && p.Y >= this.ClientSize.Height - border)
                        m.Result = (IntPtr)HTBOTTOMLEFT;
                    else if (p.X >= this.ClientSize.Width - border && p.Y >= this.ClientSize.Height - border)
                        m.Result = (IntPtr)HTBOTTOMRIGHT;
                    else if (p.X <= border)
                        m.Result = (IntPtr)HTLEFT;
                    else if (p.X >= this.ClientSize.Width - border)
                        m.Result = (IntPtr)HTRIGHT;
                    else if (p.Y <= border)
                        m.Result = (IntPtr)HTTOP;
                    else if (p.Y >= this.ClientSize.Height - border)
                        m.Result = (IntPtr)HTBOTTOM;
                }
                return;
            }
            base.WndProc(ref m);
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            UpdateMaximizeButton();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (trayIcon != null)
            {
                trayIcon.Visible = false;
                trayIcon.Dispose();
            }
            base.OnFormClosing(e);
        }

        #endregion
    }
}
