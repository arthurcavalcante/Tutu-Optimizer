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
            ResponsiveLayout.Toolbar(topBar, lblT, btnClear, btnSave);
            root.Controls.Add(topBar);

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

            logBox.HandleCreated += (s, e) =>
            {
                SetWindowTheme(logBox.Handle, theme.IsDark ? "DarkMode_Explorer" : "Explorer", null);
            };

            logContainer.Controls.Add(logBox);
            root.Controls.Add(logContainer);
            logContainer.BringToFront();

            AppendLog("Tutu's Optimizer Pro 2026 inicializado com sucesso.", theme.Accent);
            AppendLog("Monitor em tempo real ativado a cada " + settings.RefreshIntervalMs + " ms.", theme.AccentGreen);
            AppendLog("Preferências carregadas. Benchmark e estresse disponíveis.", theme.AccentCyan);
            AppendLog("Tema do sistema detectado: " + (theme.IsDark ? "Modo Escuro (Dark)" : "Modo Claro (Light)"), theme.TextSecondary);
            AppendLog("Privilégios de Administrador ativos.", theme.AccentGreen);

            return root;
        }

        // ─────────────────────────────────────────────────────────────────────
        // ABA 10 — CONFIGURAÇÕES & VALIDADOR REGEX (NOVIDADE COMPLETA)
        // ─────────────────────────────────────────────────────────────────────
    }
}
