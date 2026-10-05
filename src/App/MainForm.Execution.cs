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
        #region Seleção e Execução de Otimizações

        private void ToggleCategory(string category, bool check)
        {
            foreach (OptimizationItem item in allTweaks)
            {
                if (item.Category == category && string.IsNullOrEmpty(item.SettingsUri))
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

            string cleanMessage = InputValidator.SanitizeLogOutput(message);
            string timestamp = DateTime.Now.ToString("HH:mm:ss");
            logBox.SelectionStart = logBox.TextLength;
            logBox.SelectionLength = 0;

            logBox.SelectionColor = theme.TextSecondary;
            logBox.AppendText("[" + timestamp + "] ");

            logBox.SelectionColor = color;
            logBox.AppendText(cleanMessage + Environment.NewLine);
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
            if (!CanChangeAppearance()) return;
            standaloneOperations++;
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
                catch (Exception ex) { throw new InvalidOperationException(name + ": " + ex.Message, ex); }
            };
            w.RunWorkerCompleted += (s, e) =>
            {
                standaloneOperations--;
                if (e.Error != null)
                {
                    AppendLog("[ERRO]: " + e.Error.Message, theme.AccentRed);
                    MessageBox.Show(this, e.Error.Message, "Falha na operação", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                AppendLog(name + " finalizado!", theme.AccentGreen);
                MessageBox.Show(this, name + " finalizado com sucesso!", "Concluído", MessageBoxButtons.OK, MessageBoxIcon.Information);
            };
            w.RunWorkerAsync();
        }

        private void RunOptimization(bool isRevert)
        {
            if (!CanChangeAppearance()) return;
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

            if (!isRevert && settings.AutoCreateRestorePoint)
            {
                AppendLog("Criando Ponto de Restauração de segurança automático...", theme.AccentCyan);
                TweakEngine.CreateSystemRestorePoint((msg) => AppendLog("  🛡️ " + msg, theme.TextSecondary));
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
                        if (InputValidator.ValidateExeFileName(exe))
                        {
                            TweakEngine.SetGamePriority(exe, !isRevert);
                        }
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

            if (e.Error != null)
            {
                lblStatus.Text = "Falha na operação.";
                AppendLog(e.Error.Message, theme.AccentRed);
                MessageBox.Show(this, e.Error.Message, "Falha na operação", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            bool isRevert = e.Result is bool ? (bool)e.Result : false;
            lblStatus.Text = isRevert ? "Reversão concluída!" : "Otimizações aplicadas com sucesso!";

            MessageBox.Show(
                this,
                isRevert ? "As alterações foram revertidas com sucesso!" : "Otimizações aplicadas com sucesso!\nSeu sistema agora está configurado para máxima performance.",
                "Tutu's Optimizer Pro 2026",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information
            );
        }

        #endregion

    }
}
