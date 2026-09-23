using System;
using System.Collections.Generic;

namespace TutusOptimizer
{
    public enum SafetyLevel
    {
        Recommended, // Seguro e recomendado para todos os usuários
        Optional,    // Recomendado para gamers ou uso específico
        Advanced     // Ajustes avançados (ex: desativar UAC, Defender)
    }

    public class OptimizationItem
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
        public SafetyLevel Safety { get; set; }
        public bool IsSelected { get; set; }
        public bool CanRevert { get; set; }

        // Callbacks: Action<logMessage, progressPercent>
        public Action<Action<string, int>> ApplyAction { get; set; }
        public Action<Action<string, int>> RevertAction { get; set; }

        public OptimizationItem(string id, string title, string description, string category, SafetyLevel safety, bool isSelected, bool canRevert, Action<Action<string, int>> applyAction, Action<Action<string, int>> revertAction)
        {
            this.Id = id;
            this.Title = title;
            this.Description = description;
            this.Category = category;
            this.Safety = safety;
            this.IsSelected = isSelected;
            this.CanRevert = canRevert;
            this.ApplyAction = applyAction;
            this.RevertAction = revertAction;
        }
    }

    public class GameItem
    {
        public string Name { get; set; }
        public string[] ExeNames { get; set; }
        public bool IsSelected { get; set; }

        public GameItem(string name, string[] exeNames, bool isSelected)
        {
            this.Name = name;
            this.ExeNames = exeNames;
            this.IsSelected = isSelected;
        }

        public GameItem(string name, string singleExe, bool isSelected)
        {
            this.Name = name;
            this.ExeNames = new string[] { singleExe };
            this.IsSelected = isSelected;
        }
    }

    public class SystemInfo
    {
        public string OsName { get; set; }
        public string CpuName { get; set; }
        public ulong TotalMemoryMb { get; set; }
        public ulong FreeMemoryMb { get; set; }
        public long FreeDiskSpaceGb { get; set; }
        public bool IsAdministrator { get; set; }

        public SystemInfo()
        {
            this.OsName = "Windows";
            this.CpuName = "Processador";
            this.TotalMemoryMb = 0;
            this.FreeMemoryMb = 0;
            this.FreeDiskSpaceGb = 0;
            this.IsAdministrator = false;
        }
    }

    public class DiskHealthInfo
    {
        public string Model { get; set; }
        public string MediaType { get; set; }
        public string HealthStatus { get; set; }
        public string OperationalStatus { get; set; }
        public double SizeGb { get; set; }
        public int TemperatureC { get; set; }
        public int LifeRemainingPercent { get; set; }
        public bool IsSmartHealthy { get; set; }

        public DiskHealthInfo()
        {
            this.Model = "Disco de Armazenamento";
            this.MediaType = "SSD";
            this.HealthStatus = "Saudável (100%)";
            this.OperationalStatus = "OK";
            this.SizeGb = 0;
            this.TemperatureC = -1;
            this.LifeRemainingPercent = 100;
            this.IsSmartHealthy = true;
        }
    }

    public class CpuSensorInfo
    {
        public string Name { get; set; }
        public int Cores { get; set; }
        public int Threads { get; set; }
        public int LoadPercent { get; set; }
        public int ClockMhz { get; set; }
        public double TemperatureC { get; set; }
        public string StatusText { get; set; }

        public CpuSensorInfo()
        {
            this.Name = "Processador Intel / AMD";
            this.Cores = 4;
            this.Threads = 4;
            this.LoadPercent = 0;
            this.ClockMhz = 0;
            this.TemperatureC = -1;
            this.StatusText = "Normal / Estável";
        }
    }

    public class GpuSensorInfo
    {
        public string Name { get; set; }
        public string DriverVersion { get; set; }
        public long VramMb { get; set; }
        public int TemperatureC { get; set; }
        public string Resolution { get; set; }
        public string StatusText { get; set; }

        public GpuSensorInfo()
        {
            this.Name = "Placa de Vídeo";
            this.DriverVersion = "N/A";
            this.VramMb = 0;
            this.TemperatureC = -1;
            this.Resolution = "N/A";
            this.StatusText = "Operação Normal / Estável";
        }
    }

    public class HardwareMonitorData
    {
        public List<DiskHealthInfo> Disks { get; set; }
        public CpuSensorInfo Cpu { get; set; }
        public GpuSensorInfo Gpu { get; set; }
        public DateTime LastUpdated { get; set; }

        public HardwareMonitorData()
        {
            this.Disks = new List<DiskHealthInfo>();
            this.Cpu = new CpuSensorInfo();
            this.Gpu = new GpuSensorInfo();
            this.LastUpdated = DateTime.Now;
        }
    }
}
