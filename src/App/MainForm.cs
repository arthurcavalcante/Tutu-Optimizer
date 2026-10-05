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
    public partial class MainForm : Form
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
        private RealTimeSnapshot latestSnapshot;
        private AppSettings settings;

        private Dictionary<OptimizationItem, CheckBox> tweakCheckBoxes = new Dictionary<OptimizationItem, CheckBox>();
        private Dictionary<GameItem, CheckBox> gameCheckBoxes = new Dictionary<GameItem, CheckBox>();

        private BackgroundWorker worker;
        private System.Windows.Forms.Timer liveMonitorTimer;
        private int currentTabIndex = 0;

        // Dicionário com explicações amigáveis e didáticas para leigos de cada otimização
        private Dictionary<string, string> tweakBeginnerExplanations = new Dictionary<string, string>();
        private Dictionary<string, string> tweakSafetyInfo = new Dictionary<string, string>();

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
        private ModernScrollPanel gamesScrollPanel;

        // Medidores em Tempo Real no Dashboard
        private LiveUsageGauge gaugeCpu;
        private LiveUsageGauge gaugeRam;
        private LiveUsageGauge gaugeHealth;
        private Label lblLiveProcesses;
        private Label lblLiveUptime;
        private Label lblLiveHealthText;

        private NotifyIcon trayIcon;
        private ModernScrollPanel navigationScroll;
        private ContextMenuStrip trayMenu;
        private ToolTip toolTip;

        #endregion

        public MainForm()
        {
            this.FormBorderStyle = FormBorderStyle.None;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Size = new Size(1160, 750);
            this.MinimumSize = new Size(760, 560);
            this.Padding = new Padding(8);
            this.DoubleBuffered = true;
            this.Font = new Font("Segoe UI", 9f);
            this.SetStyle(ControlStyles.ResizeRedraw, true);

            settings = SettingsStore.Load(DetectSystemDarkMode());
            UiMotion.AppEnabled = settings.AnimationsEnabled;
            theme = settings.DarkMode ? ThemePalette.DarkTheme() : ThemePalette.LightTheme();
            ApplySavedAccent();
            this.BackColor = theme.Bg;

            try
            {
                this.Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
            }
            catch { }

            InitBeginnerExplanations();

            allTweaks = TweakEngine.BuildCatalog();
            allGames = TweakEngine.GetGamesCatalog();
            sysInfo = TweakEngine.GetSystemInfo();
            hwData = TweakEngine.GetHardwareSensors();
            latestSnapshot = TweakEngine.GetRealTimeSnapshot();

            toolTip = new ToolTip();
            toolTip.AutoPopDelay = 6000;
            toolTip.InitialDelay = 200;

            InitializeTrayIcon();
            BuildBaseLayout();
            SelectTab(0);
            UpdateSelectedCount();

            worker = new BackgroundWorker();
            worker.WorkerReportsProgress = true;
            worker.DoWork += Worker_DoWork;
            worker.ProgressChanged += Worker_ProgressChanged;
            worker.RunWorkerCompleted += Worker_RunWorkerCompleted;

            // Timer de Monitoramento em Tempo Real (2000ms = 2s)
            liveMonitorTimer = new System.Windows.Forms.Timer();
            liveMonitorTimer.Interval = settings.RefreshIntervalMs;
            liveMonitorTimer.Tick += LiveMonitorTimer_Tick;
            liveMonitorTimer.Start();
        }

        private void InitBeginnerExplanations()
        {
            tweakBeginnerExplanations["sys_power_plan"] = "Coloca o processador na potência máxima. Impede que o Windows reduza a velocidade da máquina durante seus jogos para economizar energia na tomada.";
            tweakSafetyInfo["sys_power_plan"] = "100% Seguro. Testado pela Microsoft para máximo desempenho em computadores desktop e notebooks plugados na tomada.";

            tweakBeginnerExplanations["sys_cpu_priority"] = "Faz o processador dar atenção total para o jogo ou programa que você está olhando na tela, impedindo que coisas em segundo plano causem pequenas travadas (stutters).";
            tweakSafetyInfo["sys_cpu_priority"] = "Muito Seguro. Melhora muito a estabilidade da mira e taxa de quadros (FPS).";

            tweakBeginnerExplanations["sys_multimedia_responsiveness"] = "O Windows reserva por padrão 20% da velocidade do processador para tarefas do sistema. Essa opção libera 100% da força para os seus jogos rodarem livres.";
            tweakSafetyInfo["sys_multimedia_responsiveness"] = "Totalmente Seguro e Recomendado.";

            tweakBeginnerExplanations["sys_menu_delays"] = "O Windows coloca uma animaçãozinha lenta de 0,4 segundos toda vez que você clica em menus. Essa opção remove essa espera, deixando a navegação instantânea.";
            tweakSafetyInfo["sys_menu_delays"] = "100% Seguro. Afeta apenas a rapidez visual dos menus.";

            tweakBeginnerExplanations["sys_nvme_tweaks"] = "Configura o disco SSD moderno para ler e gravar arquivos e carregar mapas de jogos na velocidade máxima possível.";
            tweakSafetyInfo["sys_nvme_tweaks"] = "Seguro para SSDs NVMe, SSDs SATA e HDDs.";

            tweakBeginnerExplanations["sys_disable_hibernate"] = "A hibernação cria um arquivo gigante no seu disco que rouba de 8 GB a 32 GB de espaço livre. Desativar isso libera muitos gigabytes imediatamente no disco C:.";
            tweakSafetyInfo["sys_disable_hibernate"] = "Seguro. Pode ser reativado a qualquer momento com o botão Reverter.";

            tweakBeginnerExplanations["sys_sticky_keys"] = "Sabe quando você está jogando, aperta Shift 5 vezes seguidas e surge uma janela chata com bip travando o jogo? Essa opção desativa essa janela de acessibilidade.";
            tweakSafetyInfo["sys_sticky_keys"] = "100% Seguro e Essencial para quem joga.";

            tweakBeginnerExplanations["sys_alt_tab"] = "Faz a troca entre a janela do jogo e o navegador (Alt+Tab) ser imediata, sem travar o vídeo ou exibir miniaturas pesadas.";
            tweakSafetyInfo["sys_alt_tab"] = "100% Seguro para multitarefa ágil.";

            tweakBeginnerExplanations["sys_explorer_tweaks"] = "Faz as pastas abrirem direto em 'Este Computador' e desliga animações lentas na barra de tarefas.";
            tweakSafetyInfo["sys_explorer_tweaks"] = "Seguro e muito prático para o dia a dia.";

            tweakBeginnerExplanations["sys_hyperv"] = "Desativa a virtualização profunda se você não usa máquinas virtuais corporativas, diminuindo a latência e micro-travamentos em jogos competitivos.";
            tweakSafetyInfo["sys_hyperv"] = "Opcional. Se você usa WSL ou emulador avançado com Hyper-V, pode manter desmarcado.";

            tweakBeginnerExplanations["priv_disable_telemetry"] = "Impede o Windows de ficar vigiando o que você faz e enviando relatórios para os servidores da Microsoft pela internet.";
            tweakSafetyInfo["priv_disable_telemetry"] = "Excelente para sua privacidade e poupa conexão de internet.";

            tweakBeginnerExplanations["priv_ceip_tasks"] = "Desativa tarefas automáticas que rodam sozinhas em segundo plano consumindo CPU e disco para compilar relatórios para a Microsoft.";
            tweakSafetyInfo["priv_ceip_tasks"] = "Totalmente seguro e poupa ciclos do processador.";

            tweakBeginnerExplanations["priv_disable_error_reporting"] = "Impede o Windows de ficar congelado mostrando 'Verificando uma solução para o problema...' quando algum programa fecha inesperadamente.";
            tweakSafetyInfo["priv_disable_error_reporting"] = "Seguro. Poupa tempo e memória RAM.";

            tweakBeginnerExplanations["priv_disable_cortana"] = "Desliga a assistente de voz Cortana que consome memória RAM e áudio em segundo plano mesmo sem você usá-la.";
            tweakSafetyInfo["priv_disable_cortana"] = "100% Seguro e Recomendado.";

            tweakBeginnerExplanations["priv_disable_feedback"] = "Bloqueia aqueles pedidos chatos do Windows perguntando 'Qual a probabilidade de você recomendar o Windows?'.";
            tweakSafetyInfo["priv_disable_feedback"] = "100% Seguro.";

            tweakBeginnerExplanations["priv_disable_edge_telemetry"] = "Evita que o navegador Edge gaste sua internet e processador enviando diagnósticos em segundo plano mesmo se você usa Chrome ou Opera.";
            tweakSafetyInfo["priv_disable_edge_telemetry"] = "Muito Seguro e Recomendado.";

            tweakBeginnerExplanations["net_tcp_low_latency"] = "O Windows costuma segurar pacotes de internet para enviar em blocos. Essa opção obriga a enviar na hora (TCPNoDelay), diminuindo seu Ping em jogos online (CS2, Valorant, FiveM, Fortnite).";
            tweakSafetyInfo["net_tcp_low_latency"] = "Seguro e padrão em qualquer setup competitivo de eSports.";

            tweakBeginnerExplanations["net_qos_bandwidth"] = "Por padrão, o Windows reserva até 20% da velocidade da sua internet para downloads do sistema. Essa opção libera essa reserva 100% para você.";
            tweakSafetyInfo["net_qos_bandwidth"] = "Seguro. Melhora estabilidade da velocidade de download e navegação.";

            tweakBeginnerExplanations["net_disable_adapter_power_saving"] = "Impede que sua placa de rede ou Wi-Fi entre em modo de economia de energia no meio de uma partida online.";
            tweakSafetyInfo["net_disable_adapter_power_saving"] = "Seguro e altamente recomendado.";

            tweakBeginnerExplanations["net_cloudflare_dns"] = "O DNS é a lista telefônica da internet. O DNS da sua operadora costuma ser lento; o Cloudflare (1.1.1.1) responde quase que instantaneamente, abrindo sites e achando servidores de jogos muito mais rápido.";
            tweakSafetyInfo["net_cloudflare_dns"] = "100% Seguro. Pode voltar para o automático pelo botão Reverter a qualquer hora.";

            tweakBeginnerExplanations["net_clean_delivery_service"] = "Impede que o seu computador compartilhe arquivos de atualizações pela internet com computadores desconhecidos, economizando sua taxa de Upload.";
            tweakSafetyInfo["net_clean_delivery_service"] = "Excelente para quem tem internet instável.";

            tweakBeginnerExplanations["game_disable_xbox_dvr"] = "A Xbox Game Bar fica filmando sua tela em segundo plano caso você aperte um atalho. Se você não usa isso, desligar aumenta o FPS e acaba com pequenos engasgos nos jogos.";
            tweakSafetyInfo["game_disable_xbox_dvr"] = "Recomendado para todo gamer.";

            tweakBeginnerExplanations["game_disable_xbox_services"] = "Desativa serviços da Xbox que rodam sozinhos para quem joga na Steam, Epic Games, Riot Games, etc.";
            tweakSafetyInfo["game_disable_xbox_services"] = "Seguro. Não afeta jogos normais da Steam ou Epic.";

            tweakBeginnerExplanations["periph_keyboard_latency"] = "Faz as teclas do teclado responderem no milissegundo exato em que são pressionadas, melhorando a agilidade nos comandos.";
            tweakSafetyInfo["periph_keyboard_latency"] = "100% Seguro.";

            tweakBeginnerExplanations["periph_mouse_raw_input"] = "No Windows comum, se você mover o mouse rápido, a mira voa mais longe. Desativar a aceleração faz a mira ser 1:1, desenvolvendo memória muscular pura para jogos de tiro.";
            tweakSafetyInfo["periph_mouse_raw_input"] = "Fundamental para jogadores de jogos de tiro (FPS).";

            tweakBeginnerExplanations["periph_gpu_hags"] = "Permite que sua placa de vídeo controle diretamente a própria memória de vídeo, deixando a taxa de quadros (FPS) mais lisa e sem engasgos.";
            tweakSafetyInfo["periph_gpu_hags"] = "Recurso oficial de alto desempenho da Microsoft e fabricantes de GPU.";

            tweakBeginnerExplanations["periph_ssd_tweaks"] = "Tira tarefas legadas do Windows dos anos 90 que desgastavam o disco sem necessidade, aumentando a vida útil e a velocidade do seu SSD.";
            tweakSafetyInfo["periph_ssd_tweaks"] = "Recomendado para SSDs e HDDs.";

            tweakBeginnerExplanations["periph_memory_management"] = "Obriga os arquivos essenciais do sistema a ficarem na memória RAM (que é ultra rápida) em vez de irem para o disco lento.";
            tweakSafetyInfo["periph_memory_management"] = "Excelente para computadores com 8 GB de memória RAM ou mais.";

            tweakBeginnerExplanations["svc_maps_broker"] = "Desativa o gerenciador de mapas offline do Windows que consome memória mesmo que você nunca abra o aplicativo Mapas.";
            tweakSafetyInfo["svc_maps_broker"] = "100% Seguro.";

            tweakBeginnerExplanations["svc_telemetry_diagtrack"] = "Desliga o serviço de rastreamento de diagnósticos em tempo real do Windows permanentemente.";
            tweakSafetyInfo["svc_telemetry_diagtrack"] = "Totalmente seguro e libera processamento.";

            tweakBeginnerExplanations["svc_useless_legacy"] = "Desativa serviços inúteis da época do Windows XP que ninguém mais usa: Fax, Registro Remoto, Modo de Loja e Arquivos Offline.";
            tweakSafetyInfo["svc_useless_legacy"] = "100% Seguro para computadores de uso doméstico e jogos.";

            tweakBeginnerExplanations["svc_sensors"] = "Desativa sensores de rotação de tela e luminosidade em computadores desktop de mesa que usam monitor comum.";
            tweakSafetyInfo["svc_sensors"] = "Seguro para computadores de mesa.";

            tweakBeginnerExplanations["svc_sysmain"] = "O SysMain tenta pré-carregar programas na memória. Em SSDs modernos ele é desnecessário e só causa lentidão e uso de 100% de disco.";
            tweakSafetyInfo["svc_sysmain"] = "Altamente recomendado para quem usa SSD.";

            tweakBeginnerExplanations["svc_print_spooler"] = "Desativa a fila de impressão. Apenas marque se você NÃO tiver impressora conectada ao computador.";
            tweakSafetyInfo["svc_print_spooler"] = "Opcional. Não ative se você imprime documentos no computador.";

            tweakBeginnerExplanations["svc_windows_search"] = "Desativa a indexação contínua de arquivos no disco para quem não usa a barra de pesquisa do Windows para ler conteúdos dentro de documentos.";
            tweakSafetyInfo["svc_windows_search"] = "Opcional. Economiza muita leitura e gravação no SSD.";
            tweakBeginnerExplanations["sys_game_mode"] = "Ativa o Modo Jogo oficial da Microsoft: enquanto você joga, o Windows prioriza 100% do processador e da placa de vídeo para o jogo e bloqueia instalações automáticas de atualizações.";
            tweakSafetyInfo["sys_game_mode"] = "100% Seguro e Recomendado para todos os jogadores.";

            tweakBeginnerExplanations["sys_visual_effects"] = "Desativa efeitos visuais bonitos mas pesados do Windows (sombras, animações de janelas) que gastam processamento da placa de vídeo desnecessariamente.";
            tweakSafetyInfo["sys_visual_effects"] = "Seguro. O computador fica mais rápido visivelmente, especialmente em máquinas mais antigas.";

            tweakBeginnerExplanations["net_dns_cache_ttl"] = "Faz o computador lembrar por mais tempo dos endereços de sites e servidores de jogos já visitados, evitando consultas repetidas ao DNS e abrindo conexões mais rápido.";
            tweakSafetyInfo["net_dns_cache_ttl"] = "Seguro. Melhora tempo de resposta da rede.";

            tweakBeginnerExplanations["periph_gpu_preemption"] = "Configura como a placa de vídeo divide o trabalho entre diferentes tarefas, reduzindo engasgos (stutters) e microparadas de FPS durante os jogos.";
            tweakSafetyInfo["periph_gpu_preemption"] = "Seguro. Recomendado para NVIDIA e AMD.";

            tweakBeginnerExplanations["svc_diagnostic_policy"] = "Desativa o serviço de Diagnóstico do Windows que fica analisando falhas e gerando relatórios em segundo plano, consumindo disco e CPU desnecessariamente.";
            tweakSafetyInfo["svc_diagnostic_policy"] = "Seguro para uso diário e em jogos. Não interfere com o funcionamento normal do sistema.";

            tweakBeginnerExplanations["priv_disable_location"] = "Impede que aplicativos e o Windows rastreiem sua localização física sem a sua permissão.";
            tweakSafetyInfo["priv_disable_location"] = "100% Seguro. Protege sua privacidade.";
        }

        private bool monitorPending;
        private async void LiveMonitorTimer_Tick(object sender, EventArgs e)
        {
            if (monitorPending || IsDisposed || !Visible) return;
            monitorPending = true;
            try
            {
                RealTimeSnapshot snapshot = await System.Threading.Tasks.Task.Run(() => TweakEngine.GetRealTimeSnapshot());
                if (IsDisposed || Disposing) return;
                latestSnapshot = snapshot;

                if (currentTabIndex == 0 && gaugeCpu != null && gaugeRam != null && gaugeHealth != null)
                {
                    // Atualiza Medidores em Tempo Real no Dashboard
                    Color cpuCol = latestSnapshot.CpuUsagePercent > 80 ? theme.AccentRed : (latestSnapshot.CpuUsagePercent > 50 ? theme.AccentAmber : theme.AccentGreen);
                    gaugeCpu.SetValue(latestSnapshot.CpuUsagePercent, string.Format("{0:0}% em uso", latestSnapshot.CpuUsagePercent), cpuCol);

                    Color ramCol = latestSnapshot.RamUsagePercent > 85 ? theme.AccentRed : (latestSnapshot.RamUsagePercent > 70 ? theme.AccentAmber : theme.Accent);
                    gaugeRam.SetValue(latestSnapshot.RamUsagePercent, string.Format("{0:0} GB / {1:0} GB", latestSnapshot.RamUsedMb / 1024.0, latestSnapshot.RamTotalMb / 1024.0), ramCol);

                    Color healthCol = latestSnapshot.HealthScore >= 80 ? theme.AccentGreen : (latestSnapshot.HealthScore >= 60 ? theme.AccentCyan : (latestSnapshot.HealthScore >= 40 ? theme.AccentAmber : theme.AccentRed));
                    gaugeHealth.SetValue(latestSnapshot.HealthScore, latestSnapshot.HealthLabel, healthCol);

                    if (lblLiveProcesses != null)
                        lblLiveProcesses.Text = string.Format("Processos Ativos: {0}", latestSnapshot.ActiveProcessCount);

                    if (lblLiveUptime != null)
                    {
                        long hours = latestSnapshot.UptimeSeconds / 3600;
                        long mins = (latestSnapshot.UptimeSeconds % 3600) / 60;
                        lblLiveUptime.Text = string.Format("Tempo Ligado: {0}h {1:00}m", hours, mins);
                    }

                    if (lblLiveHealthText != null)
                    {
                        lblLiveHealthText.Text = string.Format("Índice de carga: {0}/100 • {1}", latestSnapshot.HealthScore, latestSnapshot.HealthLabel);
                        lblLiveHealthText.ForeColor = healthCol;
                    }
                }
            }
            catch { }
            finally { monitorPending = false; }
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
                            return Convert.ToInt32(val) == 0;
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
            StopPageTransition();
            this.SuspendLayout();
            while (this.Controls.Count > 0) this.Controls[0].Dispose();
            tweakCheckBoxes.Clear();
            gameCheckBoxes.Clear();

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
            lblLogo.Text = "⚡ TUTU'S OPTIMIZER PRO";
            lblLogo.Font = new Font("Segoe UI", 10.5f, FontStyle.Bold);
            lblLogo.ForeColor = theme.Accent;
            lblLogo.AutoSize = true;
            lblLogo.Location = new Point(14, 12);
            lblLogo.MouseDown += TitleBar_MouseDown;
            lblLogo.DoubleClick += (s, e) => ToggleMaximizeWindow();

            lblSubLogo = new Label();
            lblSubLogo.Text = string.Format("2026 • Windows 10 & 11 • Monitor ao Vivo • Modo {0}", theme.IsDark ? "Escuro" : "Claro");
            lblSubLogo.Font = new Font("Segoe UI", 8.5f);
            lblSubLogo.ForeColor = theme.TextSecondary;
            lblSubLogo.AutoSize = true;
            lblSubLogo.Location = new Point(220, 14);
            lblSubLogo.MouseDown += TitleBar_MouseDown;
            lblSubLogo.DoubleClick += (s, e) => ToggleMaximizeWindow();

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
            sidebarPanel.Width = 220;
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
            UpdateResponsiveShell();
        }

        private void TitleBar_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                if (e.Clicks > 1) return;
                if (WindowState == FormWindowState.Maximized)
                {
                    Point cursor = Cursor.Position;
                    double ratio = Math.Max(0, Math.Min(1, (double)(cursor.X - Left) / Math.Max(1, Width)));
                    ToggleMaximizeWindow();
                    Location = new Point(cursor.X - (int)(Width * ratio), cursor.Y - 22);
                }
                ReleaseCapture();
                SendMessage(this.Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
            }
        }

        #endregion

        #region Helper Centralizado de Botões Secundários Elegantes

        private Button CreateSubtleButton(string text, EventHandler onClick, int width, int height)
        {
            Button btn = new AnimatedButton();
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

        private void BuildFooterContent()
        {
            footerPanel.Controls.Clear();

            Panel btnPanel = new Panel();
            btnPanel.Dock = DockStyle.Right;
            btnPanel.Width = 400;
            btnPanel.BackColor = Color.Transparent;

            btnApply = new AnimatedButton();
            btnApply.Text = "🚀 APLICAR";
            btnApply.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
            btnApply.ForeColor = ContrastText(theme.Accent);
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
            lblStatus.AutoEllipsis = true;

            lblSelectedCount = new Label();
            lblSelectedCount.Text = "0 selecionados";
            lblSelectedCount.Font = new Font("Segoe UI", 8.5f);
            lblSelectedCount.ForeColor = theme.Accent;
            lblSelectedCount.Dock = DockStyle.Top;
            lblSelectedCount.Height = 18;
            lblSelectedCount.AutoEllipsis = true;

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
            if (tabViews == null || index < 0 || index >= tabViews.Length) return;
            StopPageTransition();
            Bitmap before = UiMotion.Enabled && Visible && currentTabIndex != index ? CapturePage(tabViews[currentTabIndex]) : null;
            currentTabIndex = index;
            if (navigationScroll != null && index >= 0 && index < navButtons.Count)
            {
                int current = navigationScroll.ScrollBar.Value;
                int top = navButtons[index].Top;
                int bottom = navButtons[index].Bottom;
                if (index == 0 || index == 1 || index == 11 || index == 12) navigationScroll.ScrollBar.Value = 0;
                else if (top < current) navigationScroll.ScrollBar.Value = top;
                else if (bottom > current + navigationScroll.Height) navigationScroll.ScrollBar.Value = bottom - navigationScroll.Height;
            }
            for (int i = 0; i < navButtons.Count; i++)
            {
                bool active = (i == index);
                navButtons[i].Tag = active;
                ((AnimatedButton)navButtons[i]).SetRestingColor(active ? theme.NavActiveBg : theme.Surface);
                navButtons[i].ForeColor = active ? theme.NavActiveText : theme.TextSecondary;
                navButtons[i].Font = new Font("Segoe UI", 9.25f, active ? FontStyle.Bold : FontStyle.Regular);
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
            StartPageTransition(before, tabViews[index]);
        }

        #endregion

        #region Criação das Abas de Conteúdo

        private void BuildTabViews()
        {
            contentPanel.Controls.Clear();
            tabViews = new Panel[13];

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
            tabViews[10] = CreateSettingsTab();
            tabViews[11] = CreateDisksTab();
            tabViews[12] = CreateBenchmarkTab();

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
        // ABA 0 — DASHBOARD COM MONITOR EM TEMPO REAL
        // ─────────────────────────────────────────────────────────────────────
        #endregion


    }
}
