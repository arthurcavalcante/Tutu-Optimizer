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
        #region Gerenciamento de Janela e Tray

        private Rectangle normalWindowBounds;

        public void ToggleMaximizeWindow()
        {
            if (this.WindowState == FormWindowState.Maximized)
            {
                this.WindowState = FormWindowState.Normal;
                if (!normalWindowBounds.IsEmpty) this.Bounds = normalWindowBounds;
            }
            else
            {
                normalWindowBounds = this.WindowState == FormWindowState.Normal ? this.Bounds : this.RestoreBounds;
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
                iClose.Click += (s, e) => { forceExit = true; this.Close(); };

                trayMenu.Items.Add(iOpen);
                trayMenu.Items.Add(iExpand);
                trayMenu.Items.Add(iMin);
                trayMenu.Items.Add(new ToolStripSeparator());
                trayMenu.Items.Add(iOpt);
                trayMenu.Items.Add(iRam);
                trayMenu.Items.Add(new ToolStripSeparator());
                trayMenu.Items.Add(iClose);

                trayIcon = new NotifyIcon();
                trayIcon.Text = "Tutu's Windows Optimizer Pro 2026";
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
                cp.Style |= 0x00010000 | 0x00040000; // WS_MAXIMIZEBOX | WS_THICKFRAME: native resizing and Snap
                cp.ClassStyle |= 0x8;    // CS_DBLCLKS
                return cp;
            }
        }

        protected override void WndProc(ref Message m)
        {
            // The entire window is drawn by our controls, including while a modal
            // completion dialog disables/reactivates it. DefWindowProc would draw
            // a native frame over the custom title bar because WS_THICKFRAME is set.
            if (m.Msg == 0x83 || m.Msg == 0x85) // WM_NCCALCSIZE / WM_NCPAINT
            { m.Result = IntPtr.Zero; return; }
            if (m.Msg == 0x86) // WM_NCACTIVATE
            {
                // Preserve native activation behavior without repainting the frame.
                m.LParam = new IntPtr(-1);
                base.WndProc(ref m);
                return;
            }
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
                    long packed = m.LParam.ToInt64();
                    Point p = this.PointToClient(new Point(unchecked((short)(packed & 0xffff)), unchecked((short)((packed >> 16) & 0xffff))));
                    int border = Padding.Left;

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
            StopPageTransition();
            base.OnResize(e);
            Padding = WindowState == FormWindowState.Maximized ? Padding.Empty : new Padding(8);
            UpdateMaximizeButton();
            UpdateResponsiveShell();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (settings.MinimizeToTray && !forceExit && !closeAfterDiagnostic && e.CloseReason == CloseReason.UserClosing)
            { e.Cancel = true; Hide(); return; }
            if (diagnosticCancellation != null)
            { closeAfterDiagnostic = true; diagnosticCancellation.Cancel(); e.Cancel = true; return; }
            if (standaloneOperations > 0 || (worker != null && worker.IsBusy))
            { e.Cancel = true; lblStatus.Text = "Aguarde a operação terminar antes de fechar."; return; }
            if (liveMonitorTimer != null)
            {
                StopPageTransition();
                liveMonitorTimer.Stop();
                liveMonitorTimer.Dispose();
            }

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
