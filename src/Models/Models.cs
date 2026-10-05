using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace TutusOptimizer
{
    // Safety levels for optimization items
    public enum SafetyLevel
    {
        Recommended,
        Optional,
        Advanced
    }

    // Optimization item with callbacks
    public class OptimizationItem
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
        public SafetyLevel Safety { get; set; }
        public bool IsSelected { get; set; }
        public bool CanRevert { get; set; }
        public Action<Action<string, int>> ApplyAction { get; set; }
        public Action<Action<string, int>> RevertAction { get; set; }
        
        public string SimpleExplanation { get; set; }
        public string RiskInfo { get; set; }
        public string SettingsUri { get; set; }

        public OptimizationItem(string id, string title, string description, string category, SafetyLevel safety, bool isSelected, bool canRevert, Action<Action<string, int>> applyAction, Action<Action<string, int>> revertAction)
        {
            Id = id;
            Title = title;
            Description = description;
            Category = category;
            Safety = safety;
            IsSelected = isSelected;
            CanRevert = canRevert;
            ApplyAction = applyAction;
            RevertAction = revertAction;
            SimpleExplanation = string.Empty;
            RiskInfo = string.Empty;
        }

        public OptimizationItem(string id, string title, string description, string category, SafetyLevel safety, bool isSelected, bool canRevert, Action<Action<string, int>> applyAction, Action<Action<string, int>> revertAction, string simpleExplanation, string riskInfo)
        {
            Id = id;
            Title = title;
            Description = description;
            Category = category;
            Safety = safety;
            IsSelected = isSelected;
            CanRevert = canRevert;
            ApplyAction = applyAction;
            RevertAction = revertAction;
            SimpleExplanation = simpleExplanation;
            RiskInfo = riskInfo;
        }
    }

    // Game item for IFEO priority
    public class GameItem
    {
        public string Name { get; set; }
        public string[] ExeNames { get; set; }
        public bool IsSelected { get; set; }

        public GameItem(string name, string exeName)
        {
            Name = name;
            ExeNames = new string[] { exeName };
            IsSelected = false;
        }

        public GameItem(string name, string exeName, bool isSelected)
        {
            Name = name;
            ExeNames = new string[] { exeName };
            IsSelected = isSelected;
        }

        public GameItem(string name, string[] exeNames)
        {
            Name = name;
            ExeNames = exeNames;
            IsSelected = false;
        }

        public GameItem(string name, string[] exeNames, bool isSelected)
        {
            Name = name;
            ExeNames = exeNames;
            IsSelected = isSelected;
        }
    }

    // System information
    public class SystemInfo
    {
        public string OsName { get; set; }
        public string CpuName { get; set; }
        public int TotalMemoryMb { get; set; }
        public int FreeMemoryMb { get; set; }
        public int FreeDiskSpaceGb { get; set; }
        public bool IsAdministrator { get; set; }
    }

    // Disk health info
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
    }

    // CPU sensor info
    public class CpuSensorInfo
    {
        public string Name { get; set; }
        public int Cores { get; set; }
        public int Threads { get; set; }
        public double LoadPercent { get; set; }
        public int ClockMhz { get; set; }
        public double TemperatureC { get; set; }
        public string StatusText { get; set; }
    }

    // GPU sensor info  
    public class GpuSensorInfo
    {
        public string Name { get; set; }
        public string DriverVersion { get; set; }
        public int VramMb { get; set; }
        public int TemperatureC { get; set; }
        public string Resolution { get; set; }
        public string StatusText { get; set; }
    }

    // Hardware monitor data
    public class HardwareMonitorData
    {
        public List<DiskHealthInfo> Disks { get; set; }
        public CpuSensorInfo Cpu { get; set; }
        public GpuSensorInfo Gpu { get; set; }
        public DateTime LastUpdated { get; set; }

        public HardwareMonitorData()
        {
            Disks = new List<DiskHealthInfo>();
            Cpu = new CpuSensorInfo();
            Gpu = new GpuSensorInfo();
        }
    }

    // NEW: Real-time system snapshot for live monitoring
    public class RealTimeSnapshot
    {
        public DateTime Timestamp { get; set; }
        public double CpuUsagePercent { get; set; }
        public double RamUsagePercent { get; set; }
        public double RamUsedMb { get; set; }
        public double RamTotalMb { get; set; }
        public double DiskReadKBs { get; set; }
        public double DiskWriteKBs { get; set; }
        public double NetworkSendKBs { get; set; }
        public double NetworkReceiveKBs { get; set; }
        public int ActiveProcessCount { get; set; }
        public long UptimeSeconds { get; set; }
        public double UptimeHours 
        { 
            get { return UptimeSeconds / 3600.0; } 
            set { UptimeSeconds = (long)(value * 3600); } 
        }
        
        // Health score from 0-100
        public int HealthScore { get; set; }
        public string HealthLabel { get; set; } // "Excelente", "Bom", "Atenção", "Crítico"
        
        public RealTimeSnapshot()
        {
            Timestamp = DateTime.Now;
            HealthScore = 100;
            HealthLabel = "Excelente";
        }
    }

    // Informações completas de cada unidade de disco (C:, D:, etc.)
    public class DiskDriveDetail
    {
        public string Name { get; set; }
        public string VolumeLabel { get; set; }
        public string FileSystem { get; set; }
        public string DriveTypeDesc { get; set; }
        public long TotalBytes { get; set; }
        public long FreeBytes { get; set; }
        public long UsedBytes { get; set; }
        public double TotalGb { get; set; }
        public double FreeGb { get; set; }
        public double UsedGb { get; set; }
        public int UsedPercent { get; set; }
        public string HealthStatus { get; set; }
        public string MediaType { get; set; }
        public int TemperatureC { get; set; }
        public bool IsSystem { get; set; }
        public DiskHealthReport HealthReport { get; set; }
    }

    // Resultados do Benchmark do Kernel e Hardware
    // NEW: App settings for configuration page
    public class AppSettings
    {
        public bool DarkMode { get; set; }
        public int RefreshIntervalMs { get; set; }
        public bool StartMinimized { get; set; }
        public bool ShowNotifications { get; set; }
        public bool AutoCreateRestorePoint { get; set; }
        public string CustomDnsPrimary { get; set; }
        public string CustomDnsSecondary { get; set; }
        public string Language { get; set; }
        public string AccentHex { get; set; }
        public bool MinimizeToTray { get; set; }
        public bool AnimationsEnabled { get; set; }
        
        public AppSettings()
        {
            DarkMode = true;
            RefreshIntervalMs = 2000;
            StartMinimized = false;
            ShowNotifications = true;
            AutoCreateRestorePoint = true;
            CustomDnsPrimary = "1.1.1.1";
            CustomDnsSecondary = "1.0.0.1";
            Language = "pt-BR";
            AccentHex = "#6366F1";
            MinimizeToTray = false;
            AnimationsEnabled = true;
        }
    }
}
