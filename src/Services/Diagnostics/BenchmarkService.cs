using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace TutusOptimizer
{
    public sealed class DiskBenchmarkResult
    {
        public string Drive;
        public int SizeMb;
        public double WriteMBs, ReadMBs, RandomReadIops;
    }

    public sealed class StressOptions
    {
        public int DurationSeconds = 60;
        public int Workers = 1;
        public int CpuLoadPercent = 50;
        public int MemoryMb = 64;
    }

    public static class BenchmarkService
    {
        private static double calculationSink;

        private static double Calculate(CancellationToken token)
        {
            double value = 0;
            for (int i = 1; i <= 200000; i++)
            {
                if ((i & 4095) == 0) token.ThrowIfCancellationRequested();
                value += Math.Sqrt(i) * Math.Sin(i);
            }
            return value;
        }

        public static string RunSystem(CancellationToken token, Action<string, int> progress)
        {
            progress("CPU: teste de uma thread...", 5);
            Calculate(token); // Warm up JIT before timing.
            Stopwatch watch = Stopwatch.StartNew();
            for (int i = 0; i < 4; i++) calculationSink = Calculate(token);
            double single = watch.Elapsed.TotalMilliseconds;
            progress("CPU: teste em paralelo...", 35);
            int workers = Math.Min(64, Environment.ProcessorCount);
            double[] results = new double[workers];
            watch.Restart();
            Parallel.For(0, workers, new ParallelOptions { CancellationToken = token, MaxDegreeOfParallelism = workers }, k =>
            {
                for (int i = 0; i < 4; i++) results[k] = Calculate(token);
            });
            double multi = watch.Elapsed.TotalMilliseconds;
            calculationSink = results[0];
            progress("RAM: cópia de blocos de memória...", 70);
            byte[] source = new byte[16 * 1024 * 1024];
            byte[] target = new byte[source.Length];
            new Random(42).NextBytes(source);
            Buffer.BlockCopy(source, 0, target, 0, source.Length);
            watch.Restart();
            for (int i = 0; i < 64; i++)
            {
                token.ThrowIfCancellationRequested();
                Buffer.BlockCopy(source, 0, target, 0, source.Length);
            }
            double ram = 1024 / Math.Max(0.001, watch.Elapsed.TotalSeconds);
            for (int i = 0; i < source.Length; i++)
            {
                if ((i & 65535) == 0) token.ThrowIfCancellationRequested();
                if (source[i] != target[i]) throw new InvalidOperationException("Falha na verificação da cópia de memória.");
            }
            progress("Teste do sistema concluído.", 100);
            return string.Format("CPU / 1 thread: {0:0.0} ms\r\nCPU / {1} threads: {2:0.0} ms\r\nRAM / cópia: {3:0.0} MB/s\r\n\r\nCPU: menor tempo é melhor. RAM: maior taxa é melhor.\r\nCompare execuções deste aplicativo com a mesma configuração.", single, workers, multi, ram);
        }

        public static DiskBenchmarkResult RunDisk(string driveRoot, int sizeMb, CancellationToken token, Action<string, int> progress)
        {
            if (sizeMb != 64 && sizeMb != 256 && sizeMb != 1024)
                throw new ArgumentOutOfRangeException("sizeMb");
            DriveInfo drive = new DriveInfo(driveRoot);
            if (!drive.IsReady || (drive.DriveType != DriveType.Fixed && drive.DriveType != DriveType.Removable))
                throw new IOException("Escolha uma unidade local disponível para gravação.");
            long bytes = sizeMb * 1024L * 1024L;
            if (drive.AvailableFreeSpace < bytes + 256L * 1024 * 1024)
                throw new IOException("Espaço insuficiente: reserve o tamanho do teste mais 256 MB livres.");
            string directory = drive.RootDirectory.FullName;
            string temporaryDirectory = Path.GetTempPath();
            if (string.Equals(Path.GetPathRoot(temporaryDirectory), directory, StringComparison.OrdinalIgnoreCase))
                directory = temporaryDirectory;
            string path = Path.Combine(directory, "tutus-benchmark-" + Guid.NewGuid().ToString("N") + ".tmp");
            byte[] block = new byte[1024 * 1024];
            new Random(42).NextBytes(block);
            DiskBenchmarkResult result = new DiskBenchmarkResult { Drive = drive.Name, SizeMb = sizeMb };
            // CreateNew prevents overwriting existing files. DeleteOnClose also cleans up on cancellation.
            using (FileStream file = new FileStream(path, FileMode.CreateNew, FileAccess.ReadWrite, FileShare.None,
                4096, FileOptions.WriteThrough | FileOptions.DeleteOnClose))
            {
                Stopwatch watch = Stopwatch.StartNew();
                for (int i = 0; i < sizeMb; i++)
                {
                    token.ThrowIfCancellationRequested();
                    file.Write(block, 0, block.Length);
                    if (i % 8 == 0) progress("Gravação sequencial...", i * 40 / sizeMb);
                }
                file.Flush(true);
                result.WriteMBs = sizeMb / Math.Max(0.001, watch.Elapsed.TotalSeconds);
                file.Position = 0;
                watch.Restart();
                long read = 0;
                while (read < bytes)
                {
                    token.ThrowIfCancellationRequested();
                    int count = file.Read(block, 0, block.Length);
                    if (count == 0) throw new EndOfStreamException("Leitura incompleta do arquivo de teste.");
                    read += count;
                    if (read % (8 * 1024 * 1024) == 0) progress("Leitura sequencial...", 40 + (int)(read * 40 / bytes));
                }
                result.ReadMBs = sizeMb / Math.Max(0.001, watch.Elapsed.TotalSeconds);
                Random random = new Random(42);
                byte[] small = new byte[4096];
                watch.Restart();
                for (int i = 0; i < 2048; i++)
                {
                    token.ThrowIfCancellationRequested();
                    file.Position = (long)random.Next((int)(bytes / 4096)) * 4096;
                    int offset = 0;
                    while (offset < small.Length)
                    {
                        int count = file.Read(small, offset, small.Length - offset);
                        if (count == 0) throw new EndOfStreamException();
                        offset += count;
                    }
                    if (i % 256 == 0) progress("Leitura aleatória 4 KiB / QD1...", 80 + i * 20 / 2048);
                }
                result.RandomReadIops = 2048 / Math.Max(0.001, watch.Elapsed.TotalSeconds);
            }
            progress("Teste de disco concluído; arquivo temporário removido.", 100);
            return result;
        }

        public static string RunStress(StressOptions options, CancellationToken token, Action<string, int> progress)
        {
            if (options.DurationSeconds < 5 || options.DurationSeconds > 1800 || options.Workers < 1 ||
                options.Workers > Math.Min(64, Environment.ProcessorCount) || options.CpuLoadPercent < 10 ||
                options.CpuLoadPercent > 100 || options.MemoryMb < 0 || options.MemoryMb > 1024)
                throw new ArgumentException("Configuração de estresse fora dos limites.");
            token.ThrowIfCancellationRequested();
            SystemInfo info = TweakEngine.GetSystemInfo();
            if (options.MemoryMb > 0 && (info.FreeMemoryMb <= 0 || options.MemoryMb > info.FreeMemoryMb / 4))
                throw new InvalidOperationException("Reduza a memória do teste para até 25% da RAM livre. Se a leitura da RAM estiver indisponível, use 0 MB.");
            byte[] memory = new byte[options.MemoryMb * 1024 * 1024];
            Stopwatch elapsed = Stopwatch.StartNew();
            long cycles = 0;
            using (CancellationTokenSource stop = CancellationTokenSource.CreateLinkedTokenSource(token))
            {
                Task[] tasks = new Task[options.Workers + (memory.Length > 0 ? 1 : 0)];
                for (int k = 0; k < options.Workers; k++)
                {
                    tasks[k] = Task.Factory.StartNew(() =>
                    {
                        double value = 0;
                        while (!stop.IsCancellationRequested && elapsed.Elapsed.TotalSeconds < options.DurationSeconds)
                        {
                            Stopwatch slice = Stopwatch.StartNew();
                            while (slice.ElapsedMilliseconds < options.CpuLoadPercent && !stop.IsCancellationRequested)
                                for (int i = 1; i <= 2048; i++) value += Math.Sqrt(i);
                            if (double.IsNaN(value) || double.IsInfinity(value)) throw new InvalidOperationException("Erro no cálculo de CPU.");
                            Interlocked.Increment(ref cycles);
                            int rest = Math.Max(0, 100 - (int)slice.ElapsedMilliseconds);
                            if (rest > 0) stop.Token.WaitHandle.WaitOne(rest);
                        }
                        Interlocked.Exchange(ref calculationSink, value);
                    }, CancellationToken.None, TaskCreationOptions.LongRunning, TaskScheduler.Default);
                }
                if (memory.Length > 0)
                {
                    tasks[tasks.Length - 1] = Task.Factory.StartNew(() =>
                    {
                        byte pattern = 0;
                        while (!stop.IsCancellationRequested && elapsed.Elapsed.TotalSeconds < options.DurationSeconds)
                        {
                            pattern++;
                            for (int start = 0; start < memory.Length && !stop.IsCancellationRequested; start += 65536)
                            {
                                int end = Math.Min(memory.Length, start + 65536);
                                for (int i = start; i < end; i++) memory[i] = (byte)(pattern ^ i);
                                for (int i = start; i < end; i++)
                                    if (memory[i] != (byte)(pattern ^ i)) throw new InvalidOperationException("Falha na verificação de memória.");
                            }
                            stop.Token.WaitHandle.WaitOne(50);
                        }
                    }, CancellationToken.None, TaskCreationOptions.LongRunning, TaskScheduler.Default);
                }
                try
                {
                    while (!Task.WaitAll(tasks, 250))
                    {
                        token.ThrowIfCancellationRequested();
                        foreach (Task task in tasks) if (task.IsFaulted) task.GetAwaiter().GetResult();
                        progress(string.Format("Estresse: {0:0}/{1}s • {2} ciclos • CPU {3}% por thread", elapsed.Elapsed.TotalSeconds,
                            options.DurationSeconds, Interlocked.Read(ref cycles), options.CpuLoadPercent),
                            Math.Min(99, (int)(elapsed.Elapsed.TotalSeconds * 100 / options.DurationSeconds)));
                    }
                    token.ThrowIfCancellationRequested();
                }
                finally
                {
                    stop.Cancel();
                    Task.WaitAll(tasks);
                }
            }
            progress("Teste de estresse concluído.", 100);
            return string.Format("Concluído em {0:0.0}s • {1} threads • {2}% de carga por thread • {3} MB\r\n{4} ciclos de CPU. Nenhuma falha detectada nas verificações executadas.\r\nEste teste não certifica estabilidade e não monitora temperatura automaticamente.",
                elapsed.Elapsed.TotalSeconds, options.Workers, options.CpuLoadPercent, options.MemoryMb, cycles);
        }
    }
}
