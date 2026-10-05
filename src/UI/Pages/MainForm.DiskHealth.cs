using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TutusOptimizer
{
    public partial class MainForm
    {
        private Color DiskStatusColor(DiskHealthReport report)
        {
            return report == null || !report.HealthCode.HasValue ? theme.TextSecondary :
                report.HealthCode == 0 ? theme.AccentGreen : report.HealthCode == 1 ? theme.AccentAmber :
                report.HealthCode == 2 ? theme.AccentRed : theme.TextSecondary;
        }

        private void ShowDiskHealth(string volume, Action<DiskHealthReport> updated)
        {
            using (Form dialog = new Form())
            {
                dialog.Text = "Saúde do disco • " + volume;
                dialog.Size = new Size(720, 590);
                dialog.MinimumSize = new Size(580, 500);
                dialog.StartPosition = FormStartPosition.CenterParent;
                dialog.BackColor = theme.Bg;
                dialog.ShowInTaskbar = false;
                dialog.Font = new Font("Segoe UI", 9);
                dialog.Padding = new Padding(20);
                Label heading = new Label { Dock = DockStyle.Top, Height = 56, Text = "Consultando o disco associado a " + volume + "...",
                    ForeColor = theme.TextPrimary, Font = new Font("Segoe UI", 12, FontStyle.Bold), UseMnemonic = false };
                TextBox details = new TextBox { Dock = DockStyle.Fill, Multiline = true, ReadOnly = true, ScrollBars = ScrollBars.Vertical,
                    BackColor = theme.Card, ForeColor = theme.TextPrimary, BorderStyle = BorderStyle.FixedSingle, Font = new Font("Segoe UI", 10),
                    Text = "Lendo o estado e os contadores fornecidos pelo Windows..." };
                Panel actions = new Panel { Dock = DockStyle.Bottom, Height = 52 };
                Button refresh = DiagnosticButton(actions, "Atualizar leitura", 0, 16, 160);
                Button close = CreateSubtleButton("Fechar", (s, e) => dialog.Close(), 110, 34);
                close.Location = new Point(actions.Width - 110, 16);
                close.Anchor = AnchorStyles.Top | AnchorStyles.Right;
                actions.Controls.Add(close);
                dialog.CancelButton = close;
                dialog.Controls.Add(details);
                dialog.Controls.Add(heading);
                dialog.Controls.Add(actions);
                EventHandler load = async (s, e) =>
                {
                    if (!refresh.Enabled) return;
                    refresh.Enabled = false;
                    heading.Text = "Consultando saúde de " + volume + "...";
                    try
                    {
                        DiskHealthReport report = await Task.Run(() => DiskHealthService.ForVolume(DiskHealthService.Read(true, volume), volume));
                        if (dialog.IsDisposed) return;
                        heading.Text = report == null ? "Não foi possível associar este volume ao disco" : report.Status + " • " + report.Model;
                        heading.ForeColor = DiskStatusColor(report);
                        details.Text = report == null ? "O Windows não forneceu uma associação única para este volume.\r\nVolumes de rede, discos dinâmicos ou arranjos virtuais podem não expor esta consulta." : report.Details();
                        if (report != null) updated(report);
                    }
                    catch (Exception ex)
                    {
                        if (dialog.IsDisposed) return;
                        heading.Text = "Consulta indisponível";
                        heading.ForeColor = theme.TextSecondary;
                        details.Text = "Não foi possível consultar o provedor de armazenamento do Windows.\r\n" + ex.GetBaseException().Message;
                    }
                    finally { if (!dialog.IsDisposed) refresh.Enabled = true; }
                };
                refresh.Click += load;
                dialog.Shown += (s, e) => { ApplyImmersiveDarkMode(dialog.Handle, theme.IsDark); load(s, e); };
                dialog.ShowDialog(this);
            }
        }
    }
}
