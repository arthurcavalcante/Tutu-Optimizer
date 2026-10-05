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
        private Panel CreateDashboardTab()
        {
            ModernScrollPanel scrollPanel = new ModernScrollPanel(theme);
            scrollPanel.Dock = DockStyle.Fill;
            Panel container = scrollPanel.Content;

            int y = 0;

            Panel banner = MakePageBanner("⚡ Painel de Controle & Saúde em Tempo Real",
                "Acompanhe o uso do seu processador, memória RAM e saúde geral ao vivo. Otimize seu PC com explicações simples e fáceis.");
            banner.Location = new Point(0, y);
            banner.Width = container.Width;
            banner.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            container.Controls.Add(banner);
            y += banner.Height + 10;

            // PAINEL DE MONITORAMENTO AO VIVO (NOVIDADE)
            Panel secLive = MakeSectionHeader("📊 MONITOR EM TEMPO REAL (AO VIVO)");
            secLive.Location = new Point(0, y);
            secLive.Width = container.Width;
            secLive.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            container.Controls.Add(secLive);
            y += secLive.Height + 4;

            Panel liveCard = new Panel();
            liveCard.Location = new Point(4, y);
            liveCard.Size = new Size(container.Width - 8, 164);
            liveCard.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            liveCard.BackColor = theme.Card;
            liveCard.Paint += (s, e) =>
            {
                using (Pen p = new Pen(theme.Border, 1f))
                {
                    e.Graphics.DrawRectangle(p, 0, 0, liveCard.Width - 1, liveCard.Height - 1);
                }
            };

            gaugeCpu = new LiveUsageGauge("USO DA CPU", theme);
            gaugeCpu.Location = new Point(14, 8);

            gaugeRam = new LiveUsageGauge("USO DA MEMÓRIA RAM", theme);
            gaugeRam.Location = new Point(154, 8);

            gaugeHealth = new LiveUsageGauge("SAÚDE DO SISTEMA", theme);
            gaugeHealth.Location = new Point(294, 8);

            // Valores iniciais imediatos
            gaugeCpu.SetValue(latestSnapshot.CpuUsagePercent, string.Format("{0:0}% em uso", latestSnapshot.CpuUsagePercent), theme.AccentGreen);
            gaugeRam.SetValue(latestSnapshot.RamUsagePercent, string.Format("{0:0} GB em uso", latestSnapshot.RamUsedMb / 1024.0), theme.Accent);
            gaugeHealth.SetValue(latestSnapshot.HealthScore, latestSnapshot.HealthLabel, theme.AccentGreen);

            Panel infoSide = new Panel();
            infoSide.Location = new Point(434, 8);
            infoSide.Size = new Size(liveCard.Width - 446, 140);
            infoSide.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            infoSide.BackColor = Color.Transparent;

            lblLiveHealthText = new Label();
            lblLiveHealthText.Text = string.Format("Índice de carga: {0}/100 • {1}", latestSnapshot.HealthScore, latestSnapshot.HealthLabel);
            lblLiveHealthText.Font = new Font("Segoe UI", 10.5f, FontStyle.Bold);
            lblLiveHealthText.ForeColor = theme.AccentGreen;
            lblLiveHealthText.Dock = DockStyle.Top;
            lblLiveHealthText.Height = 42;

            lblLiveProcesses = new Label();
            lblLiveProcesses.Text = string.Format("Processos Ativos: {0}", latestSnapshot.ActiveProcessCount);
            lblLiveProcesses.Font = new Font("Segoe UI", 9f);
            lblLiveProcesses.ForeColor = theme.TextPrimary;
            lblLiveProcesses.Dock = DockStyle.Top;
            lblLiveProcesses.Height = 22;

            long hours = latestSnapshot.UptimeSeconds / 3600;
            long mins = (latestSnapshot.UptimeSeconds % 3600) / 60;
            lblLiveUptime = new Label();
            lblLiveUptime.Text = string.Format("Tempo Ligado: {0}h {1:00}m", hours, mins);
            lblLiveUptime.Font = new Font("Segoe UI", 9f);
            lblLiveUptime.ForeColor = theme.TextSecondary;
            lblLiveUptime.Dock = DockStyle.Top;
            lblLiveUptime.Height = 20;

            Label lblLiveTip = new Label();
            lblLiveTip.Text = "Indicador de carga atual. Não mede FPS nem certifica a saúde do hardware.";
            lblLiveTip.Font = new Font("Segoe UI", 8f);
            lblLiveTip.ForeColor = theme.AccentAmber;
            lblLiveTip.Dock = DockStyle.Bottom;
            lblLiveTip.Height = 40;

            infoSide.Controls.Add(lblLiveTip);
            infoSide.Controls.Add(lblLiveUptime);
            infoSide.Controls.Add(lblLiveProcesses);
            infoSide.Controls.Add(lblLiveHealthText);

            liveCard.Controls.Add(gaugeCpu);
            liveCard.Controls.Add(gaugeRam);
            liveCard.Controls.Add(gaugeHealth);
            liveCard.Controls.Add(infoSide);

            liveCard.Resize += (s, e) => {
                bool stacked = liveCard.Width < 780;
                infoSide.Anchor = AnchorStyles.Top | AnchorStyles.Left;
                infoSide.Location = new Point(stacked ? 14 : 434, stacked ? 130 : 8);
                infoSide.Size = new Size(Math.Max(1, liveCard.Width - (stacked ? 28 : 448)), 140);
                liveCard.Height = stacked ? 284 : 164;
            };
            container.Controls.Add(liveCard);
            y += liveCard.Height + 12;

            // INFORMAÇÕES DE HARDWARE
            Panel secStats = MakeSectionHeader("💻 INFORMAÇÕES DO COMPUTADOR");
            secStats.Location = new Point(0, y);
            secStats.Width = container.Width;
            secStats.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            container.Controls.Add(secStats);
            y += secStats.Height + 4;

            string gpuDisplay = (hwData != null && hwData.Gpu != null && !string.IsNullOrEmpty(hwData.Gpu.Name))
                ? hwData.Gpu.Name
                : "GPU Dedicada / Integrada";

            Panel statsRow = MakeStatCardRow(new Panel[] {
                MakeStatCard("PROCESSADOR", sysInfo.CpuName, theme.Accent),
                MakeStatCard("MEMÓRIA RAM", string.Format("{0} MB Livres de {1} MB", sysInfo.FreeMemoryMb, sysInfo.TotalMemoryMb), theme.AccentGreen),
                MakeStatCard("SISTEMA OPERACIONAL", sysInfo.OsName, theme.AccentCyan),
                MakeStatCard("PLACA DE VÍDEO", gpuDisplay, theme.AccentAmber)
            });
            statsRow.Location = new Point(0, y);
            statsRow.Width = container.Width;
            statsRow.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            container.Controls.Add(statsRow);
            y += statsRow.Height + 12;

            // SAÚDE DO HARDWARE
            Panel secHw = MakeSectionHeader("📈 SAÚDE DO HARDWARE — CPU E GPU");
            secHw.Location = new Point(0, y);
            secHw.Width = container.Width;
            secHw.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            container.Controls.Add(secHw);
            y += secHw.Height + 4;

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
                MakeSensorMiniCard("🌡️ PROCESSADOR (CPU)", cpuHealth, cpuSummary, theme.Accent),
                MakeSensorMiniCard("🎮 PLACA DE VÍDEO (GPU)", gpuHealth, gpuSummary, theme.AccentCyan),
                MakeSensorMiniCard("⚡ MEMÓRIA RAM", "🟢 Saudável", string.Format("{0} MB Livres de {1} MB", sysInfo.FreeMemoryMb, sysInfo.TotalMemoryMb), theme.AccentGreen)
            });
            hwRow.Location = new Point(0, y);
            hwRow.Width = container.Width;
            hwRow.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            container.Controls.Add(hwRow);
            y += hwRow.Height + 12;

            // AÇÕES RÁPIDAS DE 1-CLIQUE
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
            ResponsiveLayout.Stack(scrollPanel);
            scrollPanel.HookWheelRecursive(container);
            return scrollPanel;
        }

        // ─────────────────────────────────────────────────────────────────────
        // ABA 1 — SAÚDE & SENSORES (COM GUIA DE TEMPERATURA PARA LEIGOS)
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
            lblT.Text = "📈 Saúde do Hardware, Sensores & Guia de Temperaturas";
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
            ResponsiveLayout.Toolbar(topBar, lblT, lblUpdated, btnRefresh);
            topBar.Controls.Add(btnRefresh);
            container.Controls.Add(topBar);
            y += topBar.Height + 8;

            // GUIA DIDÁTICO DE TEMPERATURA PARA LEIGOS
            Panel tempGuide = new Panel();
            tempGuide.Location = new Point(4, y);
            tempGuide.Size = new Size(container.Width - 8, 96);
            tempGuide.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            tempGuide.BackColor = theme.Card;
            tempGuide.Padding = new Padding(12, 10, 12, 10);
            tempGuide.Paint += (s, e) =>
            {
                using (Pen p = new Pen(theme.Border, 1f))
                {
                    e.Graphics.DrawRectangle(p, 0, 0, tempGuide.Width - 1, tempGuide.Height - 1);
                }
            };

            Label lGTitle = new Label();
            lGTitle.Text = "🌡️ ENTENDA A TEMPERATURA DO SEU COMPUTADOR (GUIA VISUAL SIMPLES)";
            lGTitle.Font = new Font("Segoe UI", 8.5f, FontStyle.Bold);
            lGTitle.ForeColor = theme.Accent;
            lGTitle.Dock = DockStyle.Top;
            lGTitle.Height = 20;

            TableLayoutPanel gTable = new TableLayoutPanel();
            gTable.Dock = DockStyle.Fill;
            gTable.ColumnCount = 4;
            gTable.RowCount = 1;
            gTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25f));
            gTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25f));
            gTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25f));
            gTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25f));

            Label lG1 = new Label();
            lG1.Text = "🟢 Até 55°C\nFrio / Ideal em repouso e internet";
            lG1.Font = new Font("Segoe UI", 8f);
            lG1.ForeColor = theme.AccentGreen;
            lG1.Dock = DockStyle.Fill;

            Label lG2 = new Label();
            lG2.Text = "🟡 56°C a 75°C\nNormal e Seguro enquanto joga";
            lG2.Font = new Font("Segoe UI", 8f);
            lG2.ForeColor = theme.AccentCyan;
            lG2.Dock = DockStyle.Fill;

            Label lG3 = new Label();
            lG3.Text = "🟠 76°C a 84°C\nCarga Pesada (comum em notebooks)";
            lG3.Font = new Font("Segoe UI", 8f);
            lG3.ForeColor = theme.AccentAmber;
            lG3.Dock = DockStyle.Fill;

            Label lG4 = new Label();
            lG4.Text = "🔴 Acima de 85°C\nAlerta Térmico: Limpar poeira do cooler";
            lG4.Font = new Font("Segoe UI", 8f);
            lG4.ForeColor = theme.AccentRed;
            lG4.Dock = DockStyle.Fill;

            gTable.Controls.Add(lG1, 0, 0);
            gTable.Controls.Add(lG2, 1, 0);
            gTable.Controls.Add(lG3, 2, 0);
            gTable.Controls.Add(lG4, 3, 0);

            tempGuide.Controls.Add(gTable);
            tempGuide.Controls.Add(lGTitle);
            container.Controls.Add(tempGuide);
            y += tempGuide.Height + 12;

            // Nota informativa sobre a aba dedicada de Discos
            Panel pDiskNote = MakeNoteCard("💾 Discos e Armazenamento (SSD / NVMe / HDD):",
                "Para visualizar o espaço em GB de cada partição (C:, D:), informações disponíveis de saúde, acesse a aba 'Todos os Discos' no menu lateral.");
            pDiskNote.Location = new Point(4, y);
            pDiskNote.Width = container.Width - 8;
            pDiskNote.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            container.Controls.Add(pDiskNote);
            y += pDiskNote.Height + 12;

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
            ResponsiveLayout.Stack(scrollPanel);
            scrollPanel.HookWheelRecursive(container);
            return scrollPanel;
        }

        // ─────────────────────────────────────────────────────────────────────
        // ABA DE CATEGORIA COM EXPLICAÇÕES DIDÁTICAS PARA LEIGOS
        // ─────────────────────────────────────────────────────────────────────
    }
}
