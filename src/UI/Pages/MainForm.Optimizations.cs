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
        private Panel CreateCategoryTab(string categoryName)
        {
            Panel root = new Panel();
            root.Dock = DockStyle.Fill;
            root.BackColor = Color.Transparent;

            Panel topBar = new Panel();
            topBar.Dock = DockStyle.Top;
            topBar.Height = 52;
            topBar.BackColor = Color.Transparent;

            Label lblT = new Label();
            lblT.Text = "Otimizações de " + categoryName + " (Com Explicações Fáceis)";
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
            ResponsiveLayout.Toolbar(topBar, lblT, btnSelectAll, btnDeselectAll);
            root.Controls.Add(topBar);

            ModernScrollPanel scrollPanel = new ModernScrollPanel(theme);
            scrollPanel.Dock = DockStyle.Fill;
            Panel listContainer = scrollPanel.Content;

            int y = 4;
            foreach (OptimizationItem item in allTweaks)
            {
                if (item.Category == categoryName)
                {
                    Panel card = CreateFriendlyTweakCard(item);
                    card.Location = new Point(4, y);
                    card.Width = listContainer.Width - 12;
                    card.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
                    listContainer.Controls.Add(card);
                    y += card.Height + 10;
                }
            }

            scrollPanel.SetContentHeight(y + 16);
            ResponsiveLayout.Stack(scrollPanel);
            scrollPanel.HookWheelRecursive(listContainer);
            root.Controls.Add(scrollPanel);
            scrollPanel.BringToFront();

            return root;
        }

        private Panel CreateFriendlyTweakCard(OptimizationItem item)
        {
            Panel card = new Panel();
            card.Height = 92;
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
            chk.Location = new Point(14, 20);
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
            lblT.Location = new Point(42, 8);
            lblT.AutoSize = true;

            string explanation = tweakBeginnerExplanations.ContainsKey(item.Id)
                ? tweakBeginnerExplanations[item.Id]
                : (string.IsNullOrEmpty(item.SimpleExplanation) ? item.Description : item.SimpleExplanation);

            Label lblD = new Label();
            lblD.Text = "💡 O que faz: " + explanation;
            lblD.Font = new Font("Segoe UI", 8.5f);
            lblD.ForeColor = theme.TextSecondary;
            lblD.Location = new Point(43, 30);
            lblD.Size = new Size(card.Width - 160, 36);
            lblD.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;

            string safetyNote = tweakSafetyInfo.ContainsKey(item.Id)
                ? tweakSafetyInfo[item.Id]
                : (string.IsNullOrEmpty(item.RiskInfo) ? "Seguro. Pode reverter a qualquer momento." : item.RiskInfo);

            Label lblSafe = new Label();
            lblSafe.Text = "🛡️ " + safetyNote;
            lblSafe.Font = new Font("Segoe UI", 7.5f, FontStyle.Italic);
            lblSafe.ForeColor = theme.AccentGreen;
            lblSafe.Location = new Point(43, 68);
            lblSafe.AutoSize = true;

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
            lblBadge.Location = new Point(card.Width - 120, 10);
            lblBadge.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            lblBadge.AutoSize = true;

            card.Controls.Add(chk);
            card.Controls.Add(lblT);
            card.Controls.Add(lblD);
            card.Controls.Add(lblSafe);
            card.Controls.Add(lblBadge);
            Button settingsButton = null;
            if (!string.IsNullOrEmpty(item.SettingsUri))
            {
                chk.Visible = false;
                lblBadge.Text = "GUIADO";
                settingsButton = CreateSubtleButton("Abrir ajuste", (s, e) => {
                    try { Process.Start(new ProcessStartInfo(item.SettingsUri) { UseShellExecute = true }); }
                    catch (Exception ex) { MessageBox.Show(this, "Não foi possível abrir esta página do Windows: " + ex.Message, "Ajuste indisponível", MessageBoxButtons.OK, MessageBoxIcon.Information); }
                }, 120, 30);
                card.Controls.Add(settingsButton);
            }
            bool arranging = false;
            card.Resize += (s, e) => {
                if (arranging) return;
                arranging = true;
                try {
                    lblT.AutoSize = false;
                    lblT.Width = Math.Max(1, card.Width - 180);
                    lblT.Height = ResponsiveLayout.TextHeight(lblT, lblT.Width, 22);
                    lblD.SetBounds(43, lblT.Bottom + 4, Math.Max(1, card.Width - 58), 36);
                    lblD.Height = ResponsiveLayout.TextHeight(lblD, lblD.Width, 36);
                    lblSafe.AutoSize = false;
                    lblSafe.SetBounds(43, lblD.Bottom + 6, Math.Max(1, card.Width - 58), 18);
                    lblSafe.Height = ResponsiveLayout.TextHeight(lblSafe, lblSafe.Width, 18);
                    lblBadge.Left = card.Width - 120;
                    card.Height = lblSafe.Bottom + 12;
                    if (settingsButton != null)
                    {
                        settingsButton.Location = new Point(43, lblSafe.Bottom + 8);
                        card.Height = settingsButton.Bottom + 12;
                    }
                } finally { arranging = false; }
            };

            if (settingsButton == null)
            {
                card.Click += (s, e) => chk.Checked = !chk.Checked;
                lblT.Click += (s, e) => chk.Checked = !chk.Checked;
                lblD.Click += (s, e) => chk.Checked = !chk.Checked;
                lblSafe.Click += (s, e) => chk.Checked = !chk.Checked;
            }

            return card;
        }

        // ─────────────────────────────────────────────────────────────────────
        // ABA 5 — JOGOS (IFEO)
        // ─────────────────────────────────────────────────────────────────────
        private Label lblGameStats;
        private bool isGameSearchPlaceholder = true;

    }
}
