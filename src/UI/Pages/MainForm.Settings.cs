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
    public partial class MainForm
    {
        private Panel CreateSettingsTab()
        {
            ModernScrollPanel scrollPanel = new ModernScrollPanel(theme);
            scrollPanel.Dock = DockStyle.Fill;
            Panel container = scrollPanel.Content;

            int y = 0;

            Panel banner = MakePageBanner("🛠️ Configurações & Personalização Avançada",
                "Escolha o tema, a cor de destaque e o comportamento do aplicativo. Suas preferências são salvas automaticamente.");
            banner.Location = new Point(0, y);
            banner.Width = container.Width;
            banner.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            container.Controls.Add(banner);
            y += banner.Height + 10;

            // 1. PALETA DE CORES DE DESTAQUE
            Panel secPalette = MakeSectionHeader("🎨 PALETA DE CORES DE DESTAQUE (ACCENT COLOR)");
            secPalette.Location = new Point(0, y);
            secPalette.Width = container.Width;
            secPalette.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            container.Controls.Add(secPalette);
            y += secPalette.Height + 4;

            Panel paletteCard = new Panel();
            paletteCard.Location = new Point(4, y);
            paletteCard.Size = new Size(container.Width - 8, 128);
            paletteCard.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            paletteCard.BackColor = theme.Card;
            paletteCard.Padding = new Padding(12);
            paletteCard.Paint += (s, e) =>
            {
                using (Pen p = new Pen(theme.Border, 1f))
                    e.Graphics.DrawRectangle(p, 0, 0, paletteCard.Width - 1, paletteCard.Height - 1);
            };

            Label lblPalDesc = new Label();
            lblPalDesc.Text = "Escolha a cor principal dos botões, medidores e destaques visuais do aplicativo:";
            lblPalDesc.Font = new Font("Segoe UI", 8.5f);
            lblPalDesc.ForeColor = theme.TextSecondary;
            lblPalDesc.Location = new Point(14, 10);
            lblPalDesc.AutoSize = true;
            lblPalDesc.UseMnemonic = false;

            FlowLayoutPanel colorFlow = new FlowLayoutPanel();
            colorFlow.Location = new Point(10, 34);
            colorFlow.Size = new Size(paletteCard.Width - 20, 82);
            colorFlow.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            colorFlow.BackColor = Color.Transparent;
            colorFlow.WrapContents = true;

            Color[] accents = new Color[] {
                Color.FromArgb(99, 102, 241),   // Violeta / Índigo
                Color.FromArgb(14, 165, 233),   // Azul Ciano Neon
                Color.FromArgb(16, 185, 129),   // Verde Esmeralda
                Color.FromArgb(245, 158, 11),   // Âmbar / Laranja Gamer
                Color.FromArgb(239, 68, 68),    // Vermelho Crimson
                Color.FromArgb(236, 72, 153)    // Rosa Magenta
            };

            string[] colorNames = new string[] {
                "Índigo", "Ciano", "Verde", "Âmbar", "Vermelho", "Magenta"
            };

            for (int ci = 0; ci < accents.Length; ci++)
            {
                Color curCol = accents[ci];
                string cName = colorNames[ci];
                Button btnC = new AnimatedButton();
                btnC.Text = "   " + cName;
                btnC.Font = new Font("Segoe UI", 8.5f, FontStyle.Bold);
                btnC.Size = new Size(100, 32);
                btnC.FlatStyle = FlatStyle.Flat;
                btnC.FlatAppearance.BorderSize = 1;
                btnC.FlatAppearance.BorderColor = (theme.Accent.ToArgb() == curCol.ToArgb()) ? curCol : theme.Border;
                btnC.BackColor = theme.Card;
                btnC.ForeColor = theme.TextPrimary;
                btnC.Cursor = Cursors.Hand;
                btnC.Margin = new Padding(0, 0, 8, 8);

                btnC.Paint += (s, e) =>
                {
                    using (SolidBrush dotBrush = new SolidBrush(curCol))
                    {
                        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                        e.Graphics.FillEllipse(dotBrush, 8, 10, 12, 12);
                    }
                };

                btnC.Click += (s, e) =>
                {
                    if (!CanChangeAppearance()) return;
                    theme.Accent = curCol;
                    theme.AccentHover = Color.FromArgb(Math.Max(0, curCol.R - 20), Math.Max(0, curCol.G - 20), Math.Max(0, curCol.B - 20));
                    theme.NavActiveText = curCol;
                    settings.AccentHex = ColorTranslator.ToHtml(curCol);
                    SavePreferences();
                    BuildBaseLayout();
                    SelectTab(10);
                };

                colorFlow.Controls.Add(btnC);
            }

            paletteCard.Controls.Add(lblPalDesc);
            paletteCard.Controls.Add(colorFlow);
            container.Controls.Add(paletteCard);
            y += paletteCard.Height + 12;

            // 2. CONFIGURAÇÕES VISUAIS E DE TEMA
            Panel secGeneral = MakeSectionHeader("🌓 PREFERÊNCIAS DE TEMA E COMPORTAMENTO");
            secGeneral.Location = new Point(0, y);
            secGeneral.Width = container.Width;
            secGeneral.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            container.Controls.Add(secGeneral);
            y += secGeneral.Height + 4;

            Panel optCard = new Panel();
            optCard.Location = new Point(4, y);
            optCard.Size = new Size(container.Width - 8, 250);
            optCard.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            optCard.BackColor = theme.Card;
            optCard.Padding = new Padding(14);
            optCard.Paint += (s, e) =>
            {
                using (Pen p = new Pen(theme.Border, 1f))
                {
                    e.Graphics.DrawRectangle(p, 0, 0, optCard.Width - 1, optCard.Height - 1);
                }
            };

            CheckBox chkDark = new CheckBox();
            chkDark.Text = "Modo Escuro (Dark Mode Fluent)";
            chkDark.Checked = theme.IsDark;
            chkDark.Font = new Font("Segoe UI", 9.5f, FontStyle.Bold);
            chkDark.ForeColor = theme.TextPrimary;
            chkDark.Location = new Point(14, 14);
            chkDark.AutoSize = true;
            chkDark.CheckedChanged += (s, e) =>
            {
                if (chkDark.Checked == settings.DarkMode) return;
                if (!CanChangeAppearance()) { chkDark.Checked = settings.DarkMode; return; }
                theme = chkDark.Checked ? ThemePalette.DarkTheme() : ThemePalette.LightTheme();
                ApplySavedAccent();
                settings.DarkMode = chkDark.Checked;
                SavePreferences();
                this.BackColor = theme.Bg;
                ApplyImmersiveDarkMode(this.Handle, theme.IsDark);
                BuildBaseLayout();
                SelectTab(10);
            };

            CheckBox chkAutoRestore = new CheckBox();
            chkAutoRestore.Text = "Criar Ponto de Restauração automaticamente antes de qualquer otimização grande";
            chkAutoRestore.Checked = settings.AutoCreateRestorePoint;
            chkAutoRestore.Font = new Font("Segoe UI", 9f);
            chkAutoRestore.ForeColor = theme.TextPrimary;
            chkAutoRestore.Location = new Point(14, 44);
            chkAutoRestore.AutoSize = false;
            chkAutoRestore.Size = new Size(optCard.Width - 28, 40);
            chkAutoRestore.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            chkAutoRestore.CheckedChanged += (s, e) => { settings.AutoCreateRestorePoint = chkAutoRestore.Checked; SavePreferences(); };

            CheckBox chkTray = new CheckBox();
            chkTray.Text = "Minimizar para a bandeja do sistema (System Tray) ao fechar o aplicativo";
            chkTray.Checked = settings.MinimizeToTray;
            chkTray.Font = new Font("Segoe UI", 9f);
            chkTray.ForeColor = theme.TextPrimary;
            chkTray.Location = new Point(14, 90);
            chkTray.AutoSize = false;
            chkTray.Size = new Size(optCard.Width - 28, 40);
            chkTray.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            chkTray.CheckedChanged += (s, e) => { settings.MinimizeToTray = chkTray.Checked; SavePreferences(); };

            Label lblInterval = new Label();
            lblInterval.Text = "Taxa de Atualização do Monitor ao Vivo:";
            lblInterval.Font = new Font("Segoe UI", 9f);
            lblInterval.ForeColor = theme.TextSecondary;
            lblInterval.Location = new Point(14, 140);
            lblInterval.AutoSize = true;

            ComboBox cmbInterval = new ThemedComboBox(theme);
            cmbInterval.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbInterval.Items.AddRange(new object[] { "1 segundo (Ultra Rápido)", "2 segundos (Recomendado)", "5 segundos (Econômico)" });
            cmbInterval.SelectedIndex = (settings.RefreshIntervalMs == 1000) ? 0 : (settings.RefreshIntervalMs == 5000 ? 2 : 1);
            cmbInterval.Location = new Point(14, 168);
            cmbInterval.Size = new Size(200, 26);
            cmbInterval.BackColor = theme.Surface;
            cmbInterval.ForeColor = theme.TextPrimary;
            cmbInterval.SelectedIndexChanged += (s, e) =>
            {
                if (cmbInterval.SelectedIndex == 0) liveMonitorTimer.Interval = 1000;
                else if (cmbInterval.SelectedIndex == 1) liveMonitorTimer.Interval = 2000;
                else liveMonitorTimer.Interval = 5000;
                settings.RefreshIntervalMs = liveMonitorTimer.Interval;
                SavePreferences();
            };

            optCard.Controls.Add(chkDark);
            optCard.Controls.Add(chkAutoRestore);
            optCard.Controls.Add(chkTray);
            optCard.Controls.Add(lblInterval);
            optCard.Controls.Add(cmbInterval);
            CheckBox animations = new CheckBox { Text = "Animações suaves (respeita a preferência do Windows)", Checked = settings.AnimationsEnabled,
                Location = new Point(14, 210), Size = new Size(optCard.Width - 28, 28), Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                ForeColor = theme.TextPrimary, Font = new Font("Segoe UI", 9) };
            animations.CheckedChanged += (s, e) =>
            {
                settings.AnimationsEnabled = animations.Checked;
                UiMotion.AppEnabled = animations.Checked;
                if (!animations.Checked) StopPageTransition();
                SavePreferences();
            };
            optCard.Controls.Add(animations);

            container.Controls.Add(optCard);
            y += optCard.Height + 12;

            // 3. PREFERÊNCIA DE SERVIDOR DNS
            Panel secDns = MakeSectionHeader("🌐 PROVEDOR DNS");
            secDns.Location = new Point(0, y);
            secDns.Width = container.Width;
            secDns.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            container.Controls.Add(secDns);
            y += secDns.Height + 4;

            Panel dnsCard = new Panel();
            dnsCard.Location = new Point(4, y);
            dnsCard.Size = new Size(container.Width - 8, 128);
            dnsCard.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            dnsCard.BackColor = theme.Card;
            dnsCard.Padding = new Padding(14);
            dnsCard.Paint += (s, e) =>
            {
                using (Pen p = new Pen(theme.Border, 1f))
                    e.Graphics.DrawRectangle(p, 0, 0, dnsCard.Width - 1, dnsCard.Height - 1);
            };

            Label lblDnsDesc = new Label();
            lblDnsDesc.Text = "Escolha o provedor DNS para resolução de nomes da conexão ativa:";
            lblDnsDesc.Font = new Font("Segoe UI", 8.5f);
            lblDnsDesc.ForeColor = theme.TextSecondary;
            lblDnsDesc.Location = new Point(14, 10);
            lblDnsDesc.AutoSize = true;
            lblDnsDesc.UseMnemonic = false;

            ComboBox cmbDns = new ThemedComboBox(theme);
            cmbDns.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbDns.Font = new Font("Segoe UI", 9f);
            cmbDns.BackColor = theme.Surface;
            cmbDns.ForeColor = theme.TextPrimary;
            cmbDns.Items.AddRange(new object[] {
                "Cloudflare DNS (1.1.1.1 / 1.0.0.1) — Resolução de nomes",
                "Google Public DNS (8.8.8.8 / 8.8.4.4) — Alta Confiabilidade",
                "Quad9 DNS (9.9.9.9) — Bloqueio de Ameaças / Malware",
                "OpenDNS Home (208.67.222.222) — Proteção Familiar"
            });
            cmbDns.SelectedIndex = 0;
            cmbDns.Location = new Point(16, 40);
            cmbDns.Size = new Size(380, 26);

            Label lblDnsFeedback = new Label();
            lblDnsFeedback.Font = new Font("Segoe UI", 8.5f, FontStyle.Bold);
            lblDnsFeedback.ForeColor = theme.AccentGreen;
            lblDnsFeedback.Location = new Point(16, 84);
            lblDnsFeedback.AutoSize = true;
            lblDnsFeedback.UseMnemonic = false;

            Button btnApplyDns = new AnimatedButton();
            btnApplyDns.Text = "Aplicar DNS Agora";
            btnApplyDns.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
            btnApplyDns.ForeColor = ContrastText(theme.Accent);
            btnApplyDns.BackColor = theme.Accent;
            btnApplyDns.FlatStyle = FlatStyle.Flat;
            btnApplyDns.FlatAppearance.BorderSize = 0;
            btnApplyDns.Size = new Size(140, 28);
            btnApplyDns.Location = new Point(406, 39);
            btnApplyDns.Cursor = Cursors.Hand;
            btnApplyDns.Click += (s, e) =>
            {
                string pDns = "1.1.1.1";
                string sDns = "1.0.0.1";
                if (cmbDns.SelectedIndex == 1) { pDns = "8.8.8.8"; sDns = "8.8.4.4"; }
                else if (cmbDns.SelectedIndex == 2) { pDns = "9.9.9.9"; sDns = "149.112.112.112"; }
                else if (cmbDns.SelectedIndex == 3) { pDns = "208.67.222.222"; sDns = "208.67.220.220"; }

                try
                {
                    if (!CanChangeAppearance()) return;
                    TweakEngine.SetDns(pDns, sDns, (msg) => AppendLog(msg, theme.AccentGreen));
                    lblDnsFeedback.Text = "🟢 DNS (" + pDns + ") aplicado!";
                    lblDnsFeedback.ForeColor = theme.AccentGreen;
                }
                catch (Exception ex)
                {
                    lblDnsFeedback.Text = "🔴 Erro: " + ex.Message;
                    lblDnsFeedback.ForeColor = theme.AccentRed;
                }
            };

            dnsCard.Controls.Add(lblDnsDesc);
            dnsCard.Controls.Add(cmbDns);
            dnsCard.Controls.Add(btnApplyDns);
            dnsCard.Controls.Add(lblDnsFeedback);
            bool arrangingDns = false;
            dnsCard.Resize += (s, e) => {
                if (arrangingDns) return;
                arrangingDns = true;
                try {
                    lblDnsDesc.AutoSize = false;
                    lblDnsDesc.Width = Math.Max(1, dnsCard.Width - 28);
                    lblDnsDesc.Height = ResponsiveLayout.TextHeight(lblDnsDesc, lblDnsDesc.Width, 24);
                    bool stacked = dnsCard.Width < 570;
                    cmbDns.SetBounds(16, lblDnsDesc.Bottom + 8, Math.Max(1, dnsCard.Width - (stacked ? 32 : 184)), 26);
                    btnApplyDns.Location = stacked ? new Point(16, cmbDns.Bottom + 8) : new Point(dnsCard.Width - 156, cmbDns.Top);
                    lblDnsFeedback.AutoSize = false;
                    lblDnsFeedback.SetBounds(16, Math.Max(cmbDns.Bottom, btnApplyDns.Bottom) + 10, Math.Max(1, dnsCard.Width - 32), 28);
                    dnsCard.Height = lblDnsFeedback.Bottom + 12;
                } finally { arrangingDns = false; }
            };
            container.Controls.Add(dnsCard);
            y += dnsCard.Height + 12;

            scrollPanel.SetContentHeight(y + 20);
            ResponsiveLayout.Stack(scrollPanel);
            scrollPanel.HookWheelRecursive(container);
            return scrollPanel;
        }
    }
}
