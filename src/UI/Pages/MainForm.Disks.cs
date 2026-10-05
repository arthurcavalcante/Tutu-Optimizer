using System;
using System.Drawing;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TutusOptimizer
{
    public partial class MainForm
    {
        private Panel CreateDisksTab()
        {
            ModernScrollPanel scroll = new ModernScrollPanel(theme) { Dock = DockStyle.Fill };
            Panel container = scroll.Content;
            int y = 0;
            Panel banner = MakePageBanner("Discos e saúde", "Veja o espaço dos volumes e consulte o estado, a temperatura e o desgaste do disco associado a cada unidade.");
            banner.Location = new Point(0, y);
            banner.Width = container.Width;
            banner.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            container.Controls.Add(banner);
            y += banner.Height + 12;
            Panel toolbar = new Panel { Location = new Point(4, y), Size = new Size(container.Width - 8, 46), Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right };
            Label note = DiagnosticLabel(toolbar, "Estado geral informado pelo Windows", 0, 12, 24);
            note.Width = 340;
            note.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            Button refresh = CreateSubtleButton("Atualizar discos", null, 150, 34);
            refresh.Location = new Point(toolbar.Width - 150, 4);
            refresh.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            toolbar.Controls.Add(refresh);
            refresh.Click += (s, e) =>
            {
                Panel replacement = CreateDisksTab();
                replacement.Dock = DockStyle.Fill;
                Panel old = tabViews[11];
                tabViews[11] = replacement;
                contentPanel.Controls.Add(replacement);
                replacement.BringToFront();
                old.Dispose();
            };
            container.Controls.Add(toolbar);
            y += toolbar.Height + 8;
            var drives = TweakEngine.GetAllDrivesDetail();
            List<Action<List<DiskHealthReport>>> updateCards = new List<Action<List<DiskHealthReport>>>();
            List<Label> lifeLabels = new List<Label>();
            if (drives.Count == 0)
            {
                DiagnosticLabel(container, "Nenhuma unidade disponível para consulta.", 14, y, 40);
                y += 48;
            }
            foreach (DiskDriveDetail drive in drives)
            {
                Panel card = DiagnosticCard(container, ref y, drive.Name + "  " + drive.VolumeLabel, 276);
                Label title = (Label)card.Controls[0];
                title.AutoEllipsis = true;
                Label health = DiagnosticLabel(card, "Saúde: " + drive.HealthStatus, 14, 46, 24);
                health.ForeColor = DiskStatusColor(drive.HealthReport);
                Label life = DiagnosticLabel(card, drive.HealthReport == null ? "Vida útil estimada restante: N/D" : drive.HealthReport.LifeRemainingText, 14, 74, 24);
                life.Font = new Font("Segoe UI", 9, FontStyle.Bold);
                life.ForeColor = theme.TextSecondary;
                lifeLabels.Add(life);
                Label model = DiagnosticLabel(card, drive.HealthReport == null ? "Modelo não identificado para este volume" : "Disco " + drive.HealthReport.Number + " • " + drive.HealthReport.Model + " • " + drive.HealthReport.Bus, 14, 102, 24);
                model.AutoEllipsis = true;
                model.ForeColor = theme.TextSecondary;
                Label format = DiagnosticLabel(card, "Arquivos: " + drive.FileSystem + "   •   " + drive.DriveTypeDesc, 14, 130, 24);
                format.ForeColor = theme.TextSecondary;
                Label usage = DiagnosticLabel(card, string.Format("Usado: {0:0.0} GB de {1:0.0} GB ({2}%)", drive.UsedGb, drive.TotalGb, drive.UsedPercent), 14, 162, 24);
                usage.ForeColor = theme.TextSecondary;
                Panel track = new Panel { Location = new Point(14, 188), Size = new Size(card.Width - 28, 10), BackColor = theme.Border, Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right };
                Panel fill = new Panel { Height = 10, BackColor = drive.UsedPercent > 90 ? theme.AccentRed : theme.Accent };
                track.Controls.Add(fill);
                track.Resize += (s, e) => fill.Width = (int)(track.Width * Math.Max(0, Math.Min(100, drive.UsedPercent)) / 100.0);
                fill.Width = (int)(track.Width * drive.UsedPercent / 100.0);
                card.Controls.Add(track);
                Label free = DiagnosticLabel(card, string.Format("Livre: {0:0.0} GB", drive.FreeGb), 14, 218, 26);
                free.ForeColor = theme.AccentGreen;
                free.Width = 230;
                free.Anchor = AnchorStyles.Top | AnchorStyles.Left;
                Button view = DiagnosticButton(card, "Ver saúde", card.Width - 154, 214, 140);
                view.Anchor = AnchorStyles.Top | AnchorStyles.Right;
                Action<DiskHealthReport> update = report =>
                {
                    if (card.IsDisposed) return;
                    drive.HealthReport = report;
                    if (report == null) { life.Text = "Vida útil estimada restante: N/D"; life.ForeColor = theme.TextSecondary; return; }
                    life.Text = report.LifeRemainingText;
                    int? percent = report.EstimatedLifeRemainingPercent;
                    life.ForeColor = !percent.HasValue ? theme.TextSecondary : percent <= 10 ? theme.AccentRed : percent <= 30 ? theme.AccentAmber : theme.AccentGreen;
                    health.Text = "Saúde: " + report.Status + (report.Temperature.HasValue ? "   •   " + report.Temperature + " °C" : "   •   Temperatura: N/D");
                    health.ForeColor = DiskStatusColor(report);
                };
                view.Click += (s, e) => ShowDiskHealth(drive.Name, update);
                updateCards.Add(reports => update(DiskHealthService.ForVolume(reports, drive.Name)));
            }
            Label explanation = DiagnosticLabel(container, "Vida útil restante = 100% menos o desgaste informado pelo disco. Campos sem leitura aparecem como N/D.", 14, y, 42);
            explanation.ForeColor = theme.TextSecondary;
            y += 54;
            scroll.SetContentHeight(y + 20);
            ResponsiveLayout.Stack(scroll);
            scroll.HookWheelRecursive(container);
            bool requested = false;
            scroll.VisibleChanged += async (s, e) =>
            {
                if (!scroll.Visible || requested || drives.Count == 0) return;
                requested = true;
                foreach (Label label in lifeLabels) label.Text = "Vida útil estimada restante: consultando...";
                try
                {
                    List<DiskHealthReport> reports = await Task.Run(() => DiskHealthService.Read(true, null));
                    if (scroll.IsDisposed) return;
                    foreach (var update in updateCards) update(reports);
                }
                catch
                {
                    if (scroll.IsDisposed) return;
                    foreach (Label label in lifeLabels) label.Text = "Vida útil estimada restante: N/D";
                }
            };
            return scroll;
        }
    }
}
