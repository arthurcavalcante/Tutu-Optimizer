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
        private Panel CreateGamesTab()
        {
            Panel root = new Panel();
            root.Dock = DockStyle.Fill;
            root.BackColor = Color.Transparent;

            Panel topBar = new Panel();
            topBar.Dock = DockStyle.Top;
            topBar.Height = 84;
            topBar.BackColor = Color.Transparent;

            Label lblT = new Label();
            lblT.Text = "🎮 Catálogo de Otimização de Jogos (Prioridade de CPU/GPU)";
            lblT.Font = new Font("Segoe UI", 11.5f, FontStyle.Bold);
            lblT.ForeColor = theme.TextPrimary;
            lblT.UseMnemonic = false;
            lblT.Location = new Point(4, 6);
            lblT.AutoSize = true;

            Label lblSub = new Label();
            lblSub.Text = "Aplica prioridade alta (IFEO) no registro para jogos selecionados, eliminando micro-travamentos e estabilizando FPS.";
            lblSub.Font = new Font("Segoe UI", 8.5f);
            lblSub.ForeColor = theme.TextSecondary;
            lblSub.UseMnemonic = false;
            lblSub.Location = new Point(6, 28);
            lblSub.AutoSize = true;

            // Painel de Ações e Filtro
            FlowLayoutPanel actionFlow = new FlowLayoutPanel();
            actionFlow.Location = new Point(4, 48);
            actionFlow.Size = new Size(root.Width - 8, 34);
            actionFlow.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            actionFlow.BackColor = Color.Transparent;
            actionFlow.WrapContents = true;

            txtGameSearch = new TextBox();
            txtGameSearch.Font = new Font("Segoe UI", 9.5f);
            txtGameSearch.ForeColor = Color.Gray;
            txtGameSearch.Text = "🔍 Buscar jogo ou .exe...";
            txtGameSearch.BackColor = theme.Card;
            txtGameSearch.BorderStyle = BorderStyle.FixedSingle;
            txtGameSearch.Size = new Size(220, 26);
            txtGameSearch.Margin = new Padding(0, 2, 8, 0);
            isGameSearchPlaceholder = true;

            txtGameSearch.GotFocus += (s, e) =>
            {
                if (isGameSearchPlaceholder)
                {
                    txtGameSearch.Text = "";
                    txtGameSearch.ForeColor = theme.TextPrimary;
                    isGameSearchPlaceholder = false;
                }
            };
            txtGameSearch.LostFocus += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(txtGameSearch.Text))
                {
                    isGameSearchPlaceholder = true;
                    txtGameSearch.Text = "🔍 Buscar jogo ou .exe...";
                    txtGameSearch.ForeColor = Color.Gray;
                    FilterGames("");
                }
            };
            txtGameSearch.TextChanged += (s, e) =>
            {
                if (!isGameSearchPlaceholder)
                    FilterGames(txtGameSearch.Text);
            };

            Button btnSelectAll = CreateSubtleButton("Marcar Todos", (s, e) => ToggleAllGames(true), 105, 26);
            btnSelectAll.Margin = new Padding(0, 2, 6, 0);

            Button btnDeselectAll = CreateSubtleButton("Desmarcar Todos", (s, e) => ToggleAllGames(false), 120, 26);
            btnDeselectAll.Margin = new Padding(0, 2, 6, 0);

            Button btnAdd = CreateSubtleButton("+ Adicionar Jogo (.exe)", (s, e) => AddCustomGame(), 165, 26);
            btnAdd.Margin = new Padding(0, 2, 8, 0);

            lblGameStats = new Label();
            lblGameStats.Font = new Font("Segoe UI", 8.5f, FontStyle.Bold);
            lblGameStats.ForeColor = theme.Accent;
            lblGameStats.AutoSize = true;
            lblGameStats.Margin = new Padding(4, 7, 0, 0);
            lblGameStats.UseMnemonic = false;

            actionFlow.Controls.Add(txtGameSearch);
            actionFlow.Controls.Add(btnSelectAll);
            actionFlow.Controls.Add(btnDeselectAll);
            actionFlow.Controls.Add(btnAdd);
            actionFlow.Controls.Add(lblGameStats);

            topBar.Controls.Add(lblT);
            topBar.Controls.Add(lblSub);
            topBar.Controls.Add(actionFlow);
            bool arrangingHeader = false;
            topBar.Resize += (s, e) => {
                if (arrangingHeader) return;
                arrangingHeader = true;
                try {
                    lblT.AutoSize = false;
                    lblSub.AutoSize = false;
                    lblT.Width = Math.Max(1, topBar.Width - 12);
                    lblT.Height = ResponsiveLayout.TextHeight(lblT, lblT.Width, 24);
                    lblSub.SetBounds(6, lblT.Bottom + 4, Math.Max(1, topBar.Width - 12), 24);
                    lblSub.Height = ResponsiveLayout.TextHeight(lblSub, lblSub.Width, 24);
                    actionFlow.SetBounds(4, lblSub.Bottom + 8, Math.Max(1, topBar.Width - 8), 72);
                    actionFlow.PerformLayout();
                    int bottom = 0;
                    foreach (Control action in actionFlow.Controls) bottom = Math.Max(bottom, action.Bottom + action.Margin.Bottom);
                    actionFlow.Height = bottom + 4;
                    topBar.Height = actionFlow.Bottom + 8;
                } finally { arrangingHeader = false; }
            };
            root.Controls.Add(topBar);

            gamesScrollPanel = new ModernScrollPanel(theme);
            gamesScrollPanel.Dock = DockStyle.Fill;
            Panel container = gamesScrollPanel.Content;

            flowGames = new FlowLayoutPanel();
            flowGames.Location = new Point(0, 0);
            flowGames.Width = Math.Max(300, container.ClientSize.Width - 10);
            flowGames.AutoSize = false;
            flowGames.WrapContents = true;
            flowGames.Padding = new Padding(4);
            flowGames.BackColor = Color.Transparent;

            container.Controls.Add(flowGames);
            root.Controls.Add(gamesScrollPanel);
            gamesScrollPanel.BringToFront();

            container.Resize += (s, e) =>
            {
                if (flowGames != null && container != null)
                {
                    flowGames.Width = Math.Max(300, container.ClientSize.Width - 10);
                    string query = (isGameSearchPlaceholder || txtGameSearch == null) ? "" : txtGameSearch.Text;
                    PopulateGamesGrid(query);
                }
            };

            PopulateGamesGrid("");
            gamesScrollPanel.HookWheelRecursive(container);
            return root;
        }

        private void PopulateGamesGrid(string search)
        {
            if (flowGames == null) return;
            flowGames.SuspendLayout();
            flowGames.Controls.Clear();
            string filter = (search == "🔍 Buscar jogo ou .exe...") ? "" : search.Trim().ToLower();

            int containerW = flowGames.Width > 50 ? flowGames.Width : 800;
            int cols = Math.Max(1, (containerW - 16) / 250);
            int cardW = Math.Max(210, ((containerW - 16) / cols) - 8);

            int matchCount = 0;
            int selectedCount = 0;

            foreach (GameItem g in allGames)
            {
                if (g.IsSelected) selectedCount++;

                bool match = string.IsNullOrEmpty(filter) ||
                             g.Name.ToLower().Contains(filter) ||
                             (g.ExeNames != null && g.ExeNames.Length > 0 && g.ExeNames[0].ToLower().Contains(filter));

                if (match)
                {
                    matchCount++;
                    Panel tile = new Panel();
                    tile.Size = new Size(cardW, 64);
                    tile.BackColor = theme.Card;
                    tile.Margin = new Padding(4);
                    tile.Padding = new Padding(8);
                    tile.Cursor = Cursors.Hand;
                    tile.Paint += (s, e) =>
                    {
                        using (Pen p = new Pen(theme.Border, 1f))
                        {
                            e.Graphics.DrawRectangle(p, 0, 0, tile.Width - 1, tile.Height - 1);
                        }
                    };

                    CheckBox chk = new CheckBox();
                    chk.Checked = g.IsSelected;
                    chk.Location = new Point(10, 22);
                    chk.Size = new Size(18, 18);
                    chk.Cursor = Cursors.Hand;
                    chk.CheckedChanged += (s, e) =>
                    {
                        g.IsSelected = chk.Checked;
                        UpdateGameStatsLabel();
                    };
                    gameCheckBoxes[g] = chk;

                    Label lName = new Label();
                    lName.Text = g.Name;
                    lName.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
                    lName.ForeColor = theme.TextPrimary;
                    lName.Location = new Point(34, 8);
                    lName.Size = new Size(cardW - 42, 18);
                    lName.UseMnemonic = false;
                    lName.AutoEllipsis = true;

                    Label lExe = new Label();
                    lExe.Text = (g.ExeNames != null && g.ExeNames.Length > 0) ? g.ExeNames[0] : "";
                    lExe.Font = new Font("Segoe UI", 7.5f);
                    lExe.ForeColor = theme.TextSecondary;
                    lExe.Location = new Point(35, 26);
                    lExe.Size = new Size(cardW - 44, 16);
                    lExe.UseMnemonic = false;
                    lExe.AutoEllipsis = true;

                    Label lBadge = new Label();
                    lBadge.Text = "★ ALTA PRIORIDADE";
                    lBadge.Font = new Font("Segoe UI", 7f, FontStyle.Bold);
                    lBadge.ForeColor = theme.AccentGreen;
                    lBadge.Location = new Point(35, 43);
                    lBadge.AutoSize = true;
                    lBadge.UseMnemonic = false;

                    tile.Controls.Add(chk);
                    tile.Controls.Add(lName);
                    tile.Controls.Add(lExe);
                    tile.Controls.Add(lBadge);

                    tile.Click += (s, e) => chk.Checked = !chk.Checked;
                    lName.Click += (s, e) => chk.Checked = !chk.Checked;
                    lExe.Click += (s, e) => chk.Checked = !chk.Checked;
                    lBadge.Click += (s, e) => chk.Checked = !chk.Checked;

                    flowGames.Controls.Add(tile);
                }
            }

            int rows = (int)Math.Ceiling((double)matchCount / Math.Max(1, cols));
            int totalH = Math.Max(200, rows * 72 + 20);
            flowGames.Height = totalH;
            flowGames.ResumeLayout();

            if (gamesScrollPanel != null)
            {
                gamesScrollPanel.SetContentHeight(totalH + 40);
            }

            UpdateGameStatsLabel();
        }

        private void UpdateGameStatsLabel()
        {
            if (lblGameStats == null) return;
            int sel = 0;
            foreach (GameItem g in allGames) { if (g.IsSelected) sel++; }
            lblGameStats.Text = string.Format("{0} jogos no catálogo ({1} selecionados)", allGames.Count, sel);
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
            UpdateGameStatsLabel();
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
                    if (!InputValidator.ValidateExeFileName(file))
                    {
                        MessageBox.Show(this, "Nome de executável inválido. Informe apenas o nome do arquivo terminado em .exe.", "Erro de Validação", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
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
    }
}
