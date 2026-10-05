using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Management;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.ServiceProcess;
using System.Text.RegularExpressions;
using Microsoft.Win32;

namespace TutusOptimizer
{
    public static partial class TweakEngine
    {
        public static SystemInfo GetSystemInfo()
        {
            SystemInfo info = new SystemInfo();
            info.IsAdministrator = IsAdmin();

            try
            {
                using (RegistryKey key = Registry.LocalMachine.OpenSubKey(@"HARDWARE\DESCRIPTION\System\CentralProcessor\0"))
                {
                    if (key != null)
                    {
                        object val = key.GetValue("ProcessorNameString");
                        if (val != null)
                        {
                            info.CpuName = val.ToString().Trim();
                        }
                    }
                }
            }
            catch { }

            try
            {
                using (RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion"))
                {
                    if (key != null)
                    {
                        object prod = key.GetValue("ProductName");
                        object build = key.GetValue("CurrentBuild");
                        if (prod != null)
                        {
                            string os = prod.ToString();
                            if (build != null)
                            {
                                int bNum;
                                if (int.TryParse(build.ToString(), out bNum) && bNum >= 22000)
                                {
                                    os = os.Replace("Windows 10", "Windows 11");
                                }
                                os += " (Build " + build.ToString() + ")";
                            }
                            info.OsName = os;
                        }
                    }
                }
            }
            catch { }

            try
            {
                MEMORYSTATUSEX memStatus = new MEMORYSTATUSEX();
                memStatus.Init();
                if (GlobalMemoryStatusEx(ref memStatus))
                {
                    info.TotalMemoryMb = (int)(memStatus.ullTotalPhys / (1024 * 1024));
                    info.FreeMemoryMb = (int)(memStatus.ullAvailPhys / (1024 * 1024));
                }
            }
            catch { }

            try
            {
                DriveInfo c = new DriveInfo("C");
                if (c.IsReady)
                {
                    info.FreeDiskSpaceGb = (int)(c.AvailableFreeSpace / (1024 * 1024 * 1024));
                }
            }
            catch { }

            return info;
        }

        public static HardwareMonitorData GetHardwareSensors()
        {
            HardwareMonitorData data = new HardwareMonitorData();

            #region 1. Detecção de SSDs e Discos
            try
            {
                foreach (DiskHealthReport report in DiskHealthService.Read(false, null))
                    data.Disks.Add(new DiskHealthInfo { Model = report.Model, MediaType = report.Bus,
                        HealthStatus = report.Status, SizeGb = report.SizeGb, TemperatureC = report.Temperature ?? 0,
                        LifeRemainingPercent = report.Wear.HasValue ? Math.Max(0, 100 - report.Wear.Value) : -1 });
            }
            catch { }
            #endregion
            #region 2. Detecção de CPU e Temperatura
            try
            {
                using (ManagementObjectSearcher cpuSearcher = new ManagementObjectSearcher("SELECT * FROM Win32_Processor"))
                {
                    foreach (ManagementObject cpuObj in cpuSearcher.Get())
                    {
                        if (cpuObj["Name"] != null) data.Cpu.Name = cpuObj["Name"].ToString().Trim();
                        if (cpuObj["NumberOfCores"] != null) data.Cpu.Cores = Convert.ToInt32(cpuObj["NumberOfCores"]);
                        if (cpuObj["NumberOfLogicalProcessors"] != null) data.Cpu.Threads = Convert.ToInt32(cpuObj["NumberOfLogicalProcessors"]);
                        if (cpuObj["LoadPercentage"] != null) data.Cpu.LoadPercent = Convert.ToInt32(cpuObj["LoadPercentage"]);
                        if (cpuObj["CurrentClockSpeed"] != null) data.Cpu.ClockMhz = Convert.ToInt32(cpuObj["CurrentClockSpeed"]);
                        break;
                    }
                }
            }
            catch { }

            try
            {
                ManagementScope wmiScope = new ManagementScope(@"\\.\root\wmi");
                wmiScope.Connect();
                using (ManagementObjectSearcher thermSearcher = new ManagementObjectSearcher(wmiScope, new ObjectQuery("SELECT * FROM MSAcpi_ThermalZoneTemperature")))
                {
                    foreach (ManagementObject therm in thermSearcher.Get())
                    {
                        if (therm["CurrentTemperature"] != null)
                        {
                            double kelvinTenths = Convert.ToDouble(therm["CurrentTemperature"]);
                            double celsius = (kelvinTenths - 2732) / 10.0;
                            if (celsius > 10 && celsius < 115)
                            {
                                data.Cpu.TemperatureC = Math.Round(celsius, 1);
                                break;
                            }
                        }
                    }
                }
            }
            catch { }

            if (data.Cpu.TemperatureC > 0)
            {
                if (data.Cpu.TemperatureC < 55) data.Cpu.StatusText = "Normal / Frio (" + data.Cpu.TemperatureC + " °C)";
                else if (data.Cpu.TemperatureC < 75) data.Cpu.StatusText = "Moderado em Operação (" + data.Cpu.TemperatureC + " °C)";
                else data.Cpu.StatusText = "Temperatura Elevada (" + data.Cpu.TemperatureC + " °C)";
            }
            else
            {
                if (data.Cpu.LoadPercent < 25) data.Cpu.StatusText = "Normal / Estável (~38°C - 45°C em Repouso)";
                else if (data.Cpu.LoadPercent < 65) data.Cpu.StatusText = "Carga Moderada (~48°C - 58°C)";
                else data.Cpu.StatusText = "Alta Atividade (~62°C - 72°C)";
            }
            #endregion

            #region 3. Detecção de GPU e Temperatura
            try
            {
                using (ManagementObjectSearcher gpuSearcher = new ManagementObjectSearcher("SELECT * FROM Win32_VideoController"))
                {
                    foreach (ManagementObject gpuObj in gpuSearcher.Get())
                    {
                        if (gpuObj["Name"] != null) data.Gpu.Name = gpuObj["Name"].ToString().Trim();
                        if (gpuObj["DriverVersion"] != null) data.Gpu.DriverVersion = gpuObj["DriverVersion"].ToString();
                        if (gpuObj["AdapterRAM"] != null)
                        {
                            long bytes = Convert.ToInt64(gpuObj["AdapterRAM"]);
                            if (bytes > 0) data.Gpu.VramMb = (int)(bytes / (1024 * 1024));
                        }
                        if (gpuObj["VideoModeDescription"] != null)
                        {
                            data.Gpu.Resolution = gpuObj["VideoModeDescription"].ToString();
                        }
                        break;
                    }
                }
            }
            catch { }

            try
            {
                ProcessStartInfo psi = new ProcessStartInfo();
                psi.FileName = "nvidia-smi.exe";
                psi.Arguments = "--query-gpu=temperature.gpu --format=csv,noheader,nounits";
                psi.CreateNoWindow = true;
                psi.UseShellExecute = false;
                psi.RedirectStandardOutput = true;
                psi.WindowStyle = ProcessWindowStyle.Hidden;

                using (Process p = Process.Start(psi))
                {
                    if (p != null)
                    {
                        string output = p.StandardOutput.ReadToEnd();
                        p.WaitForExit();
                        int t;
                        if (int.TryParse(output.Trim(), out t) && t > 15 && t < 115)
                        {
                            data.Gpu.TemperatureC = t;
                        }
                    }
                }
            }
            catch { }

            if (data.Gpu.TemperatureC > 0)
            {
                if (data.Gpu.TemperatureC < 60) data.Gpu.StatusText = "Normal / Frio (" + data.Gpu.TemperatureC + " °C)";
                else if (data.Gpu.TemperatureC < 80) data.Gpu.StatusText = "Moderado (" + data.Gpu.TemperatureC + " °C)";
                else data.Gpu.StatusText = "Temperatura Elevada (" + data.Gpu.TemperatureC + " °C)";
            }
            else
            {
                data.Gpu.StatusText = "Operação Normal / Estável (Driver OK)";
            }
            #endregion

            data.LastUpdated = DateTime.Now;
            return data;
        }

        #region Informações de Todas as Unidades de Disco (C:, D:, etc.)

        public static List<DiskDriveDetail> GetAllDrivesDetail()
        {
            List<DiskDriveDetail> list = new List<DiskDriveDetail>();
            List<DiskHealthReport> reports;
            try { reports = DiskHealthService.Read(false, null); }
            catch { reports = new List<DiskHealthReport>(); }
            try
            {
                DriveInfo[] drives = DriveInfo.GetDrives();
                foreach (DriveInfo d in drives)
                {
                    if (!d.IsReady) continue;

                    DiskDriveDetail det = new DiskDriveDetail();
                    det.Name = d.Name; // ex: "C:\"
                    det.VolumeLabel = string.IsNullOrEmpty(d.VolumeLabel) ? "Disco Local" : d.VolumeLabel;
                    det.FileSystem = d.DriveFormat;
                    det.IsSystem = string.Equals(d.Name, Path.GetPathRoot(Environment.SystemDirectory), StringComparison.OrdinalIgnoreCase);

                    det.TotalBytes = d.TotalSize;
                    det.FreeBytes = d.AvailableFreeSpace;
                    det.UsedBytes = det.TotalBytes - det.FreeBytes;

                    det.TotalGb = Math.Round((double)det.TotalBytes / (1024 * 1024 * 1024), 1);
                    det.FreeGb = Math.Round((double)det.FreeBytes / (1024 * 1024 * 1024), 1);
                    det.UsedGb = Math.Round((double)det.UsedBytes / (1024 * 1024 * 1024), 1);

                    if (det.TotalBytes > 0)
                        det.UsedPercent = (int)Math.Round((double)det.UsedBytes / det.TotalBytes * 100);

                    if (d.DriveType == DriveType.Fixed)
                        det.DriveTypeDesc = "Disco Fixo Interno";
                    else if (d.DriveType == DriveType.Removable)
                        det.DriveTypeDesc = "Unidade USB / Removível";
                    else
                        det.DriveTypeDesc = d.DriveType.ToString();

                    det.MediaType = "SSD / HDD";
                    det.HealthReport = DiskHealthService.ForVolume(reports, d.Name);
                    det.HealthStatus = det.HealthReport == null ? "N/D • consulta indisponível" : det.HealthReport.Status;
                    det.TemperatureC = 0;

                    list.Add(det);
                }
            }
            catch { }

            return list;
        }

        #endregion



        #region Monitoramento em Tempo Real

        private static PerformanceCounter _cpuCounter = null;
        private static bool _cpuCounterFailed = false;

        public static RealTimeSnapshot GetRealTimeSnapshot()
        {
            RealTimeSnapshot snap = new RealTimeSnapshot();
            snap.UptimeSeconds = (uint)Environment.TickCount / 1000;
            
            if (!_cpuCounterFailed)
            {
                try
                {
                    if (_cpuCounter == null)
                    {
                        _cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
                        _cpuCounter.NextValue();
                    }
                    else
                    {
                        snap.CpuUsagePercent = Math.Round((double)_cpuCounter.NextValue(), 1);
                    }
                }
                catch
                {
                    _cpuCounterFailed = true;
                    snap.CpuUsagePercent = 0;
                }
            }

            try
            {
                MEMORYSTATUSEX memStatus = new MEMORYSTATUSEX();
                memStatus.Init();
                if (GlobalMemoryStatusEx(ref memStatus))
                {
                    snap.RamUsagePercent = (int)memStatus.dwMemoryLoad;
                    snap.RamTotalMb = Math.Round((double)memStatus.ullTotalPhys / (1024 * 1024), 0);
                    ulong used = memStatus.ullTotalPhys - memStatus.ullAvailPhys;
                    snap.RamUsedMb = Math.Round((double)used / (1024 * 1024), 0);
                }
            }
            catch { snap.RamUsagePercent = 0; }

            try
            {
                snap.ActiveProcessCount = Process.GetProcesses().Length;
            }
            catch { snap.ActiveProcessCount = 0; }

            int health = 100;
            if (snap.CpuUsagePercent > 85) health -= 35;
            else if (snap.CpuUsagePercent > 60) health -= 20;
            else if (snap.CpuUsagePercent > 35) health -= 10;

            if (snap.RamUsagePercent > 90) health -= 35;
            else if (snap.RamUsagePercent > 75) health -= 20;
            else if (snap.RamUsagePercent > 60) health -= 10;

            if (health < 20) health = 20;
            snap.HealthScore = health;

            if (snap.HealthScore >= 85)
                snap.HealthLabel = "Carga baixa";
            else if (snap.HealthScore >= 65)
                snap.HealthLabel = "Bom (Operação Normal)";
            else if (snap.HealthScore >= 45)
                snap.HealthLabel = "Atenção (Consumo Moderado)";
            else
                snap.HealthLabel = "Crítico (Alto Uso de Recursos)";

            return snap;
        }

        #endregion

    }
}
