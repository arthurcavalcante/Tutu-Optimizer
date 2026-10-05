using System;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TutusOptimizer
{
    public partial class MainForm
    {
        private CancellationTokenSource diagnosticCancellation;
        private bool closeAfterDiagnostic;
        private int standaloneOperations;

        private Panel DiagnosticCard(Panel container, ref int y, string title, int height)
        {
            Panel card = new Panel { Location = new Point(4, y), Size = new Size(container.Width - 8, height),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right, BackColor = theme.Card };
            card.Paint += (s, e) => { using (Pen pen = new Pen(theme.Border)) e.Graphics.DrawRectangle(pen, 0, 0, card.Width - 1, card.Height - 1); };
            Label heading = DiagnosticLabel(card, title, 12, 12, 28);
            heading.Font = new Font("Segoe UI", 11, FontStyle.Bold);
            container.Controls.Add(card);
            y += height + 12;
            return card;
        }

        private Label DiagnosticLabel(Panel parent, string text, int x, int y, int height)
        {
            Label label = new Label { Text = text, Location = new Point(x, y), Size = new Size(Math.Max(100, parent.Width - x - 14), height),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right, ForeColor = theme.TextPrimary,
                Font = new Font("Segoe UI", 9), UseMnemonic = false };
            parent.Controls.Add(label);
            return label;
        }

        private ComboBox DiagnosticCombo(Panel parent, int y, object[] choices)
        {
            ComboBox combo = new ThemedComboBox(theme) { Location = new Point(210, y), Size = new Size(Math.Max(120, parent.Width - 226), 28),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right, DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = theme.Surface, ForeColor = theme.TextPrimary, Font = new Font("Segoe UI", 9) };
            combo.Items.AddRange(choices);
            if (combo.Items.Count > 0) combo.SelectedIndex = 0;
            parent.Controls.Add(combo);
            return combo;
        }

        private NumericUpDown DiagnosticNumber(Panel parent, string caption, int y, int min, int max, int value)
        {
            Label label = DiagnosticLabel(parent, caption, 14, y + 3, 24);
            label.Width = 190;
            label.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            NumericUpDown number = new NumericUpDown { Minimum = min, Maximum = max, Value = Math.Min(max, value),
                Location = new Point(210, y), Size = new Size(150, 26), BackColor = theme.Surface, ForeColor = theme.TextPrimary,
                Font = new Font("Segoe UI", 9) };
            parent.Controls.Add(number);
            return number;
        }

        private Button DiagnosticButton(Panel parent, string text, int x, int y, int width)
        {
            Button button = new AnimatedButton { Text = text, Location = new Point(x, y), Size = new Size(width, 34),
                BackColor = theme.Accent, ForeColor = ContrastText(theme.Accent), FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9, FontStyle.Bold), Cursor = Cursors.Hand, UseMnemonic = false };
            button.FlatAppearance.BorderSize = 0;
            parent.Controls.Add(button);
            return button;
        }

        private Panel CreateBenchmarkTab()
        {
            ModernScrollPanel scroll = new ModernScrollPanel(theme) { Dock = DockStyle.Fill };
            Panel container = scroll.Content;
            int y = 0;
            Panel banner = MakePageBanner("Benchmark e teste de estresse", "Meça CPU, RAM e SSD/HDD. Configure o teste para sua máquina e acompanhe o progresso.");
            banner.Location = new Point(0, y);
            banner.Width = container.Width;
            banner.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            container.Controls.Add(banner);
            y += banner.Height + 12;

            Panel system = DiagnosticCard(container, ref y, "CPU e memória", 120);
            DiagnosticLabel(system, "Cálculo em uma ou várias threads e cópia de RAM. Feche outros aplicativos para comparar execuções.", 14, 42, 30);
            Button runSystem = DiagnosticButton(system, "Testar CPU e RAM", 14, 76, 190);

            Panel disk = DiagnosticCard(container, ref y, "Benchmark de SSD / HDD", 240);
            Label driveLabel = DiagnosticLabel(disk, "Unidade / volume", 14, 46, 24);
            driveLabel.Width = 190; driveLabel.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            ComboBox drives = DiagnosticCombo(disk, 42, new object[0]);
            foreach (DriveInfo drive in DriveInfo.GetDrives())
            {
                try { if (drive.IsReady && (drive.DriveType == DriveType.Fixed || drive.DriveType == DriveType.Removable)) drives.Items.Add(drive.Name); }
                catch (IOException) { }
                catch (UnauthorizedAccessException) { }
            }
            if (drives.Items.Count > 0) drives.SelectedIndex = 0;
            Label sizeLabel = DiagnosticLabel(disk, "Tamanho do arquivo", 14, 80, 24);
            sizeLabel.Width = 190; sizeLabel.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            ComboBox sizes = DiagnosticCombo(disk, 76, new object[] { "64 MB • rápido", "256 MB • padrão", "1024 MB • ampliado" });
            sizes.SelectedIndex = 1;
            DiagnosticLabel(disk, "Leitura e gravação sequencial + leitura aleatória 4 KiB (QD1).\r\nUsa um arquivo temporário na unidade escolhida, removido ao concluir ou cancelar.\r\nO cache do Windows e do dispositivo influencia a leitura; estas taxas medem este teste de arquivos.", 14, 112, 78);
            Button runDisk = DiagnosticButton(disk, "Testar unidade", 14, 194, 190);

            Panel stress = DiagnosticCard(container, ref y, "Teste de estresse • CPU e RAM", 346);
            Label profileLabel = DiagnosticLabel(stress, "Perfil da máquina", 14, 46, 24);
            profileLabel.Width = 190; profileLabel.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            ComboBox profiles = DiagnosticCombo(stress, 42, new object[] { "Leve / notebook", "Equilibrado / desktop", "Intenso / workstation", "Personalizado" });
            int maximumWorkers = Math.Min(64, Environment.ProcessorCount);
            NumericUpDown duration = DiagnosticNumber(stress, "Duração (segundos)", 78, 5, 1800, 60);
            NumericUpDown threads = DiagnosticNumber(stress, "Threads de CPU", 110, 1, maximumWorkers, Math.Max(1, maximumWorkers / 2));
            NumericUpDown load = DiagnosticNumber(stress, "Carga por thread (%)", 142, 10, 100, 50);
            NumericUpDown memory = DiagnosticNumber(stress, "Memória para testar (MB)", 174, 0, 1024, 64);
            bool settingProfile = false;
            profiles.SelectedIndexChanged += (s, e) =>
            {
                if (profiles.SelectedIndex == 3) return;
                settingProfile = true;
                int index = profiles.SelectedIndex;
                duration.Value = index == 0 ? 60 : index == 1 ? 180 : 300;
                threads.Value = index == 0 ? Math.Max(1, maximumWorkers / 2) : maximumWorkers;
                load.Value = index == 0 ? 50 : index == 1 ? 75 : 100;
                memory.Value = index == 0 ? 64 : index == 1 ? 256 : 512;
                settingProfile = false;
            };
            EventHandler custom = (s, e) => { if (!settingProfile) profiles.SelectedIndex = 3; };
            duration.ValueChanged += custom; threads.ValueChanged += custom; load.ValueChanged += custom; memory.ValueChanged += custom;
            DiagnosticLabel(stress, "A memória será limitada a 25% da RAM livre. Use 0 MB para testar somente CPU.\r\nA carga é uma meta por thread; o uso total depende da máquina. O teste aumenta consumo e calor.\r\nAcompanhe a temperatura em Saúde e Sensores e cancele se necessário. GPU não é testada.", 14, 213, 80);
            Button runStress = DiagnosticButton(stress, "Iniciar estresse", 14, 300, 190);

            Panel output = DiagnosticCard(container, ref y, "Progresso e resultado", 310);
            Label status = DiagnosticLabel(output, "Pronto para testar.", 14, 44, 40);
            ProgressBar progressBar = new ProgressBar { Location = new Point(14, 86), Size = new Size(output.Width - 28, 12), Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right };
            output.Controls.Add(progressBar);
            TextBox results = new TextBox { Location = new Point(14, 108), Size = new Size(output.Width - 28, 144),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right, Multiline = true, ReadOnly = true,
                ScrollBars = ScrollBars.Vertical, BackColor = theme.Surface, ForeColor = theme.TextPrimary, Font = new Font("Segoe UI", 9) };
            output.Controls.Add(results);
            Button cancel = DiagnosticButton(output, "Cancelar", 14, 264, 120);
            cancel.Enabled = false;
            Button export = DiagnosticButton(output, "Salvar resultado", 146, 264, 150);
            export.Enabled = false;
            cancel.Click += (s, e) => { if (diagnosticCancellation != null) { diagnosticCancellation.Cancel(); status.Text = "Cancelando e liberando recursos..."; cancel.Enabled = false; } };
            export.Click += (s, e) =>
            {
                using (SaveFileDialog dialog = new SaveFileDialog { Filter = "Texto (*.txt)|*.txt", FileName = "benchmark-" + DateTime.Now.ToString("yyyyMMdd-HHmmss") + ".txt" })
                {
                    if (dialog.ShowDialog(this) != DialogResult.OK) return;
                    try { File.WriteAllText(dialog.FileName, results.Text, System.Text.Encoding.UTF8); }
                    catch (Exception ex) { MessageBox.Show(this, "Não foi possível salvar: " + ex.Message); }
                }
            };
            Control[] inputs = { runSystem, runDisk, runStress, drives, sizes, profiles, duration, threads, load, memory };
            runSystem.Click += (s, e) => StartDiagnostic("CPU e RAM", (token, report) => BenchmarkService.RunSystem(token, report), inputs, cancel, export, status, progressBar, results);
            runDisk.Click += (s, e) =>
            {
                if (drives.SelectedItem == null) { status.Text = "Nenhuma unidade disponível."; return; }
                string root = drives.SelectedItem.ToString();
                int size = new int[] { 64, 256, 1024 }[sizes.SelectedIndex];
                StartDiagnostic("Disco " + root, (token, report) =>
                {
                    DiskBenchmarkResult result = BenchmarkService.RunDisk(root, size, token, report);
                    return string.Format("Unidade: {0} • arquivo: {1} MB\r\nGravação sequencial: {2:0.0} MB/s\r\nLeitura sequencial: {3:0.0} MB/s\r\nLeitura aleatória 4 KiB / QD1: {4:0} IOPS\r\n\r\nLeitura sujeita ao cache do Windows e do dispositivo.", result.Drive, result.SizeMb, result.WriteMBs, result.ReadMBs, result.RandomReadIops);
                }, inputs, cancel, export, status, progressBar, results);
            };
            runStress.Click += (s, e) =>
            {
                StressOptions options = new StressOptions { DurationSeconds = (int)duration.Value, Workers = (int)threads.Value, CpuLoadPercent = (int)load.Value, MemoryMb = (int)memory.Value };
                StartDiagnostic("Estresse", (token, report) => BenchmarkService.RunStress(options, token, report), inputs, cancel, export, status, progressBar, results);
            };
            scroll.SetContentHeight(y + 20);
            ResponsiveLayout.Stack(scroll);
            scroll.HookWheelRecursive(container);
            return scroll;
        }

        private async void StartDiagnostic(string name, Func<CancellationToken, Action<string, int>, string> action,
            Control[] inputs, Button cancel, Button export, Label status, ProgressBar bar, TextBox results)
        {
            if (diagnosticCancellation != null || (worker != null && worker.IsBusy) || standaloneOperations > 0)
            { status.Text = "Aguarde a operação atual terminar."; return; }
            diagnosticCancellation = new CancellationTokenSource();
            CancellationTokenSource operationSource = diagnosticCancellation;
            CancellationToken token = diagnosticCancellation.Token;
            foreach (Control input in inputs) input.Enabled = false;
            cancel.Enabled = true; export.Enabled = false; bar.Value = 0;
            results.Clear(); status.Text = "Iniciando " + name + "...";
            Progress<Tuple<string, int>> progress = new Progress<Tuple<string, int>>(p =>
            {
                if (!object.ReferenceEquals(diagnosticCancellation, operationSource) || token.IsCancellationRequested || status.IsDisposed) return;
                status.Text = p.Item1;
                bar.Value = Math.Max(0, Math.Min(100, p.Item2));
            });
            try
            {
                string result = await Task.Run(() => action(token, (message, percent) => ((IProgress<Tuple<string, int>>)progress).Report(Tuple.Create(message, percent))), token);
                results.Text = name + " • " + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + "\r\n" + result;
                status.Text = "Concluído."; bar.Value = 100; export.Enabled = true;
                AppendLog(name + " concluído.", theme.AccentGreen);
            }
            catch (OperationCanceledException) { status.Text = "Teste cancelado. Recursos liberados."; }
            catch (Exception ex) { status.Text = "Falha: " + ex.GetBaseException().Message; AppendLog(status.Text, theme.AccentRed); }
            finally
            {
                diagnosticCancellation.Dispose(); diagnosticCancellation = null;
                foreach (Control input in inputs) input.Enabled = true;
                cancel.Enabled = false;
                if (closeAfterDiagnostic) Close();
            }
        }
    }
}
