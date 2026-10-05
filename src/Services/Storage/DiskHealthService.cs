using System;
using System.Collections.Generic;
using System.Management;
using System.Text;

namespace TutusOptimizer
{
    public sealed class DiskHealthReport
    {
        public int Number;
        public string Model = "N/D", Serial = "N/D", Bus = "N/D";
        public int? HealthCode;
        public double SizeGb;
        public int? Temperature, Wear;
        public ulong? PowerOnHours, ReadErrors, WriteErrors;
        public string CounterNote = "Clique em Ver saúde para consultar os contadores disponíveis.";
        public List<string> Volumes = new List<string>();
        public int? EstimatedLifeRemainingPercent
        {
            get { return Wear.HasValue && Wear.Value >= 0 && Wear.Value <= 100 ? (int?)(100 - Wear.Value) : null; }
        }
        public string LifeRemainingText
        {
            get { return "Vida útil estimada restante: " + (EstimatedLifeRemainingPercent.HasValue ? EstimatedLifeRemainingPercent.Value + "%" : "N/D"); }
        }
        public string Status
        {
            get { return HealthCode == 0 ? "Saudável" : HealthCode == 1 ? "Atenção" : HealthCode == 2 ? "Crítico" : "N/D"; }
        }
        public string Details()
        {
            StringBuilder text = new StringBuilder();
            text.AppendLine("DISCO " + Number + " • " + Model);
            text.AppendLine("Volumes associados: " + string.Join(", ", Volumes.ToArray()));
            text.AppendLine("Número de série: " + Serial);
            text.AppendLine("Conexão: " + Bus);
            text.AppendLine();
            text.AppendLine("Estado informado pelo Windows: " + Status);
            text.AppendLine(LifeRemainingText);
            text.AppendLine("Temperatura: " + (Temperature.HasValue ? Temperature + " °C" : "N/D"));
            text.AppendLine("Desgaste consumido: " + (Wear.HasValue ? Wear + "%" : "N/D"));
            text.AppendLine("Horas ligado: " + (PowerOnHours.HasValue ? PowerOnHours.ToString() + " h" : "N/D"));
            text.AppendLine("Erros de leitura não corrigidos: " + (ReadErrors.HasValue ? ReadErrors.ToString() : "N/D"));
            text.AppendLine("Erros de gravação não corrigidos: " + (WriteErrors.HasValue ? WriteErrors.ToString() : "N/D"));
            text.AppendLine();
            text.AppendLine(CounterNote);
            text.AppendLine();
            text.AppendLine("Vida útil restante = 100% menos o desgaste informado pelo disco.\r\nÉ uma estimativa de desgaste, separada do estado geral do Windows. Campos sem leitura aparecem como N/D.");
            return text.ToString();
        }
    }

    public static class DiskHealthService
    {
        private const string StorageNamespace = @"\\.\root\Microsoft\Windows\Storage";
        private static object Value(ManagementBaseObject item, string property)
        {
            try { return item[property]; }
            catch (ManagementException) { return null; }
        }
        private static int? IntValue(ManagementBaseObject item, string property)
        { object value = Value(item, property); return value == null ? (int?)null : Convert.ToInt32(value); }
        private static ulong? LongValue(ManagementBaseObject item, string property)
        { object value = Value(item, property); return value == null ? (ulong?)null : Convert.ToUInt64(value); }
        private static string TextValue(ManagementBaseObject item, string property)
        { return Convert.ToString(Value(item, property)).Trim(); }

        public static string VolumeRoot(string value)
        {
            if (string.IsNullOrEmpty(value) || !char.IsLetter(value[0])) return null;
            if (value.Length != 1 && (value.Length < 2 || value[1] != ':')) return null;
            return char.ToUpperInvariant(value[0]) + @":\";
        }

        public static DiskHealthReport ForVolume(IEnumerable<DiskHealthReport> reports, string volume)
        {
            string root = VolumeRoot(volume);
            if (root == null) return null;
            DiskHealthReport match = null;
            foreach (DiskHealthReport report in reports)
                if (report.Volumes.Exists(v => string.Equals(v, root, StringComparison.OrdinalIgnoreCase)))
                {
                    // Ambiguous layouts (e.g. spanned volumes) must not borrow one disk's counters.
                    if (match != null && match.Number != report.Number) return null;
                    match = report;
                }
            return match;
        }

        public static List<DiskHealthReport> Read(bool includeCounters, string selectedVolume)
        {
            ManagementScope scope = new ManagementScope(StorageNamespace);
            scope.Connect();
            EnumerationOptions enumeration = new EnumerationOptions { Timeout = TimeSpan.FromSeconds(5), ReturnImmediately = true };
            Dictionary<int, List<string>> volumes = new Dictionary<int, List<string>>();
            using (ManagementObjectSearcher partitions = new ManagementObjectSearcher(scope, new ObjectQuery("SELECT DiskNumber, DriveLetter FROM MSFT_Partition"), enumeration))
            using (ManagementObjectCollection items = partitions.Get())
                foreach (ManagementObject partition in items)
                using (partition)
                {
                    string root = VolumeRoot(TextValue(partition, "DriveLetter"));
                    int? number = IntValue(partition, "DiskNumber");
                    if (root == null || !number.HasValue) continue;
                    if (!volumes.ContainsKey(number.Value)) volumes[number.Value] = new List<string>();
                    if (!volumes[number.Value].Contains(root)) volumes[number.Value].Add(root);
                }
            List<DiskHealthReport> reports = new List<DiskHealthReport>();
            using (ManagementObjectSearcher disks = new ManagementObjectSearcher(scope, new ObjectQuery("SELECT * FROM MSFT_Disk"), enumeration))
            using (ManagementObjectCollection items = disks.Get())
                foreach (ManagementObject disk in items)
                using (disk)
                {
                    int? number = IntValue(disk, "Number");
                    if (!number.HasValue) continue;
                    DiskHealthReport report = new DiskHealthReport { Number = number.Value, Model = TextValue(disk, "FriendlyName"),
                        Serial = TextValue(disk, "SerialNumber"), HealthCode = IntValue(disk, "HealthStatus") };
                    ulong? size = LongValue(disk, "Size");
                    report.SizeGb = size.HasValue ? size.Value / (1024.0 * 1024 * 1024) : 0;
                    if (string.IsNullOrEmpty(report.Model)) report.Model = "N/D";
                    if (string.IsNullOrEmpty(report.Serial)) report.Serial = "N/D";
                    int? bus = IntValue(disk, "BusType");
                    report.Bus = bus == 17 ? "NVMe" : bus == 11 ? "SATA" : bus == 7 ? "USB" : bus == 8 ? "RAID" :
                        bus == 16 ? "Storage Spaces" : bus == 15 || bus == 14 ? "Virtual" : "Outro / N/D";
                    if (volumes.ContainsKey(report.Number)) report.Volumes = volumes[report.Number];
                    if (includeCounters && (selectedVolume == null || report.Volumes.Contains(VolumeRoot(selectedVolume))))
                        ReadCounters(scope, disk, report);
                    reports.Add(report);
                }
            return reports;
        }

        private static void ReadCounters(ManagementScope scope, ManagementObject disk, DiskHealthReport report)
        {
            try
            {
                // This is the read-only method used by Get-StorageReliabilityCounter -Disk.
                using (ManagementClass cmdlets = new ManagementClass(scope, new ManagementPath("PS_StorageCmdlets"), null))
                using (ManagementBaseObject input = cmdlets.GetMethodParameters("GetStorageReliabilityCounter"))
                {
                    input["Disk"] = disk;
                    using (ManagementBaseObject output = cmdlets.InvokeMethod("GetStorageReliabilityCounter", input,
                        new InvokeMethodOptions { Timeout = TimeSpan.FromSeconds(8) }))
                    {
                        if (Convert.ToUInt32(output["ReturnValue"]) != 0)
                            throw new InvalidOperationException("O provedor não disponibilizou os contadores deste disco.");
                        using (ManagementBaseObject counter = output["StorageReliabilityCounter"] as ManagementBaseObject)
                        {
                            if (counter == null) throw new InvalidOperationException("O driver não retornou contadores.");
                            report.Temperature = IntValue(counter, "Temperature");
                            if (report.Temperature <= 0 || report.Temperature >= 120) report.Temperature = null;
                            report.Wear = IntValue(counter, "Wear");
                            if (report.Wear > 100 || report.Wear < 0) report.Wear = null;
                            report.PowerOnHours = LongValue(counter, "PowerOnHours");
                            report.ReadErrors = LongValue(counter, "ReadErrorsUncorrected");
                            report.WriteErrors = LongValue(counter, "WriteErrorsUncorrected");
                            report.CounterNote = "Fonte: contadores de confiabilidade do Windows. N/D significa que o driver não forneceu o campo.";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                report.CounterNote = "Contadores indisponíveis: " + ex.GetBaseException().Message +
                    "\r\nExecute o aplicativo como administrador. Alguns drivers e adaptadores USB/RAID não expõem esses dados.";
            }
        }
    }
}
