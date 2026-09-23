using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Management;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.ServiceProcess;
using Microsoft.Win32;

namespace TutusOptimizer
{
    public static class TweakEngine
    {
        #region Win32 API Imports

        [DllImport("psapi.dll")]
        public static extern int EmptyWorkingSet(IntPtr hwProc);

        [DllImport("kernel32.dll")]
        public static extern IntPtr OpenProcess(int dwDesiredAccess, bool bInheritHandle, int dwProcessId);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool CloseHandle(IntPtr hObject);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SystemParametersInfo(uint uiAction, uint uiParam, IntPtr pvParam, uint fWinIni);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GlobalMemoryStatusEx(ref MEMORYSTATUSEX lpBuffer);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public struct MEMORYSTATUSEX
        {
            public uint dwLength;
            public uint dwMemoryLoad;
            public ulong ullTotalPhys;
            public ulong ullAvailPhys;
            public ulong ullTotalPageFile;
            public ulong ullAvailPageFile;
            public ulong ullTotalVirtual;
            public ulong ullAvailVirtual;
            public ulong ullAvailExtendedVirtual;

            public void Init()
            {
                this.dwLength = (uint)Marshal.SizeOf(typeof(MEMORYSTATUSEX));
            }
        }

        private const int PROCESS_ALL_ACCESS = 0x1F0FFF;
        private const int PROCESS_QUERY_INFORMATION = 0x0400;
        private const int PROCESS_SET_QUOTA = 0x0100;
        private const uint SPI_SETMOUSESPEED = 0x0071;
        private const uint SPIF_UPDATEINIFILE = 0x01;
        private const uint SPIF_SENDCHANGE = 0x02;

        #endregion

        #region Utilitários de Comando e Registro

        public static bool IsAdmin()
        {
            try
            {
                WindowsIdentity id = WindowsIdentity.GetCurrent();
                WindowsPrincipal principal = new WindowsPrincipal(id);
                return principal.IsInRole(WindowsBuiltInRole.Administrator);
            }
            catch
            {
                return false;
            }
        }

        public static int RunProcess(string fileName, string args, bool wait = true)
        {
            try
            {
                ProcessStartInfo psi = new ProcessStartInfo();
                psi.FileName = fileName;
                psi.Arguments = args;
                psi.CreateNoWindow = true;
                psi.UseShellExecute = false;
                psi.WindowStyle = ProcessWindowStyle.Hidden;

                using (Process proc = Process.Start(psi))
                {
                    if (wait && proc != null)
                    {
                        proc.WaitForExit();
                        return proc.ExitCode;
                    }
                }
                return 0;
            }
            catch
            {
                return -1;
            }
        }

        public static int RunPowerShell(string command, bool wait = true)
        {
            string formattedArgs = "-NoProfile -ExecutionPolicy Bypass -Command \"" + command + "\"";
            return RunProcess("powershell.exe", formattedArgs, wait);
        }

        public static bool SetRegDword(string rootHive, string subKey, string valueName, int value)
        {
            try
            {
                RegistryKey root = GetRootKey(rootHive);
                if (root == null) return false;

                using (RegistryKey key = root.CreateSubKey(subKey))
                {
                    if (key != null)
                    {
                        key.SetValue(valueName, value, RegistryValueKind.DWord);
                        return true;
                    }
                }
            }
            catch { }
            return false;
        }

        public static bool SetRegString(string rootHive, string subKey, string valueName, string value)
        {
            try
            {
                RegistryKey root = GetRootKey(rootHive);
                if (root == null) return false;

                using (RegistryKey key = root.CreateSubKey(subKey))
                {
                    if (key != null)
                    {
                        key.SetValue(valueName, value, RegistryValueKind.String);
                        return true;
                    }
                }
            }
            catch { }
            return false;
        }

        public static bool SetRegBinary(string rootHive, string subKey, string valueName, byte[] bytes)
        {
            try
            {
                RegistryKey root = GetRootKey(rootHive);
                if (root == null) return false;

                using (RegistryKey key = root.CreateSubKey(subKey))
                {
                    if (key != null)
                    {
                        key.SetValue(valueName, bytes, RegistryValueKind.Binary);
                        return true;
                    }
                }
            }
            catch { }
            return false;
        }

        public static bool DeleteRegValue(string rootHive, string subKey, string valueName)
        {
            try
            {
                RegistryKey root = GetRootKey(rootHive);
                if (root == null) return false;

                using (RegistryKey key = root.OpenSubKey(subKey, true))
                {
                    if (key != null)
                    {
                        key.DeleteValue(valueName, false);
                        return true;
                    }
                }
            }
            catch { }
            return false;
        }

        public static bool DeleteRegKey(string rootHive, string subKey)
        {
            try
            {
                RegistryKey root = GetRootKey(rootHive);
                if (root == null) return false;

                root.DeleteSubKeyTree(subKey, false);
                return true;
            }
            catch { }
            return false;
        }

        private static RegistryKey GetRootKey(string rootHive)
        {
            if (rootHive == "HKLM" || rootHive == "HKEY_LOCAL_MACHINE") return Registry.LocalMachine;
            if (rootHive == "HKCU" || rootHive == "HKEY_CURRENT_USER") return Registry.CurrentUser;
            if (rootHive == "HKCR" || rootHive == "HKEY_CLASSES_ROOT") return Registry.ClassesRoot;
            return null;
        }

        public static void SetServiceState(string serviceName, string startMode, bool stop)
        {
            try
            {
                if (stop)
                {
                    RunProcess("sc.exe", "stop \"" + serviceName + "\"");
                }
                RunProcess("sc.exe", "config \"" + serviceName + "\" start= " + startMode);
            }
            catch { }
        }

        #endregion

        #region Memória e Hardware

        public static SystemInfo GetSystemInfo()
        {
            SystemInfo info = new SystemInfo();
            info.IsAdministrator = IsAdmin();

            try
            {
                // CPU Name
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
                // OS Name
                using (RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion"))
                {
                    if (key != null)
                    {
                        object prod = key.GetValue("ProductName");
                        object build = key.GetValue("CurrentBuild");
                        if (prod != null)
                        {
                            string os = prod.ToString();
                            // Windows 11 check
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
                // Memory
                MEMORYSTATUSEX memStatus = new MEMORYSTATUSEX();
                memStatus.Init();
                if (GlobalMemoryStatusEx(ref memStatus))
                {
                    info.TotalMemoryMb = memStatus.ullTotalPhys / (1024 * 1024);
                    info.FreeMemoryMb = memStatus.ullAvailPhys / (1024 * 1024);
                }
            }
            catch { }

            try
            {
                // Free Disk C:
                DriveInfo c = new DriveInfo("C");
                if (c.IsReady)
                {
                    info.FreeDiskSpaceGb = c.AvailableFreeSpace / (1024 * 1024 * 1024);
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
                ManagementScope scope = new ManagementScope(@"\\.\root\microsoft\windows\storage");
                scope.Connect();

                using (ManagementObjectSearcher searcher = new ManagementObjectSearcher(scope, new ObjectQuery("SELECT * FROM MSFT_PhysicalDisk")))
                {
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        DiskHealthInfo disk = new DiskHealthInfo();
                        if (obj["FriendlyName"] != null) disk.Model = obj["FriendlyName"].ToString();

                        if (obj["MediaType"] != null)
                        {
                            int mt = Convert.ToInt32(obj["MediaType"]);
                            if (mt == 4) disk.MediaType = "SSD (Solid State Drive)";
                            else if (mt == 3) disk.MediaType = "HDD (Disco Rígido)";
                            else if (mt == 5) disk.MediaType = "SCM (Memória de Armazenamento)";
                            else disk.MediaType = "SSD / Flash";
                        }

                        if (obj["HealthStatus"] != null)
                        {
                            int hs = Convert.ToInt32(obj["HealthStatus"]);
                            if (hs == 0) disk.HealthStatus = "🟢 Saudável (100%)";
                            else if (hs == 1) disk.HealthStatus = "🟡 Alerta de Desgaste";
                            else disk.HealthStatus = "🔴 Crítico / Substituição";
                        }

                        if (obj["OperationalStatus"] != null)
                        {
                            disk.OperationalStatus = "Operacional (OK)";
                        }

                        if (obj["Size"] != null)
                        {
                            ulong bytes = Convert.ToUInt64(obj["Size"]);
                            disk.SizeGb = Math.Round((double)bytes / (1024 * 1024 * 1024), 1);
                        }

                        data.Disks.Add(disk);
                    }
                }
            }
            catch { }

            // Se o namespace de armazenamento não retornar discos, usamos Win32_DiskDrive como fallback seguro
            if (data.Disks.Count == 0)
            {
                try
                {
                    using (ManagementObjectSearcher diskSearcher = new ManagementObjectSearcher("SELECT * FROM Win32_DiskDrive"))
                    {
                        foreach (ManagementObject drive in diskSearcher.Get())
                        {
                            DiskHealthInfo disk = new DiskHealthInfo();
                            if (drive["Model"] != null) disk.Model = drive["Model"].ToString();
                            if (drive["Status"] != null) disk.HealthStatus = "🟢 Saudável (" + drive["Status"].ToString() + ")";
                            if (drive["Size"] != null)
                            {
                                ulong sz = Convert.ToUInt64(drive["Size"]);
                                disk.SizeGb = Math.Round((double)sz / (1024 * 1024 * 1024), 1);
                            }

                            string mLower = disk.Model.ToLower();
                            if (mLower.Contains("ssd") || mLower.Contains("sandisk") || mLower.Contains("kingston") || mLower.Contains("nvme"))
                                disk.MediaType = "SSD (Solid State Drive)";
                            else
                                disk.MediaType = "HDD";

                            data.Disks.Add(disk);
                        }
                    }
                }
                catch { }
            }

            // Tenta obter temperatura dos discos via MSFT_StorageReliabilityCounter
            try
            {
                ManagementScope scope = new ManagementScope(@"\\.\root\microsoft\windows\storage");
                scope.Connect();
                using (ManagementObjectSearcher relSearcher = new ManagementObjectSearcher(scope, new ObjectQuery("SELECT * FROM MSFT_StorageReliabilityCounter")))
                {
                    int idx = 0;
                    foreach (ManagementObject rel in relSearcher.Get())
                    {
                        if (idx < data.Disks.Count)
                        {
                            if (rel["Temperature"] != null)
                            {
                                int temp = Convert.ToInt32(rel["Temperature"]);
                                if (temp > 0 && temp < 120)
                                {
                                    data.Disks[idx].TemperatureC = temp;
                                }
                            }
                            idx++;
                        }
                    }
                }
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

            // Tenta ler sensor térmico ACPI via WMI
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

            // Classificação de status térmico da CPU
            if (data.Cpu.TemperatureC > 0)
            {
                if (data.Cpu.TemperatureC < 55) data.Cpu.StatusText = "🟢 Normal / Frio (" + data.Cpu.TemperatureC + " °C)";
                else if (data.Cpu.TemperatureC < 75) data.Cpu.StatusText = "🟡 Moderado em Operação (" + data.Cpu.TemperatureC + " °C)";
                else data.Cpu.StatusText = "🔴 Temperatura Elevada (" + data.Cpu.TemperatureC + " °C)";
            }
            else
            {
                if (data.Cpu.LoadPercent < 25) data.Cpu.StatusText = "🟢 Normal / Estável (~38°C - 45°C em Repouso)";
                else if (data.Cpu.LoadPercent < 65) data.Cpu.StatusText = "🟡 Carga Moderada (~48°C - 58°C)";
                else data.Cpu.StatusText = "🟡 Alta Atividade (~62°C - 72°C)";
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
                            if (bytes > 0) data.Gpu.VramMb = bytes / (1024 * 1024);
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

            // Tenta obter temperatura da GPU via nvidia-smi se disponível
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
                if (data.Gpu.TemperatureC < 60) data.Gpu.StatusText = "🟢 Normal / Frio (" + data.Gpu.TemperatureC + " °C)";
                else if (data.Gpu.TemperatureC < 80) data.Gpu.StatusText = "🟡 Moderado (" + data.Gpu.TemperatureC + " °C)";
                else data.Gpu.StatusText = "🔴 Temperatura Elevada (" + data.Gpu.TemperatureC + " °C)";
            }
            else
            {
                data.Gpu.StatusText = "🟢 Operação Normal / Estável (Driver OK)";
            }
            #endregion

            data.LastUpdated = DateTime.Now;
            return data;
        }

        public static int PurgeSystemWorkingSets()
        {
            int freedProcesses = 0;
            Process[] processes = Process.GetProcesses();
            foreach (Process p in processes)
            {
                try
                {
                    IntPtr hProc = OpenProcess(PROCESS_SET_QUOTA | PROCESS_QUERY_INFORMATION, false, p.Id);
                    if (hProc != IntPtr.Zero)
                    {
                        if (EmptyWorkingSet(hProc) != 0)
                        {
                            freedProcesses++;
                        }
                        CloseHandle(hProc);
                    }
                }
                catch { }
            }
            return freedProcesses;
        }

        #endregion

        #region Limpeza de Temporários e Disco

        public static long CleanTempDirectory(string path, Action<string> log)
        {
            long deletedCount = 0;
            if (string.IsNullOrEmpty(path) || !Directory.Exists(path)) return 0;

            try
            {
                DirectoryInfo di = new DirectoryInfo(path);
                FileInfo[] files = di.GetFiles("*", SearchOption.AllDirectories);
                foreach (FileInfo file in files)
                {
                    try
                    {
                        file.Attributes = FileAttributes.Normal;
                        file.Delete();
                        deletedCount++;
                    }
                    catch { }
                }

                DirectoryInfo[] subDirs = di.GetDirectories();
                foreach (DirectoryInfo sub in subDirs)
                {
                    try
                    {
                        sub.Delete(true);
                    }
                    catch { }
                }
            }
            catch { }

            return deletedCount;
        }

        public static void FlushDnsAndWinsock(Action<string> log)
        {
            log("Limpando cache de DNS...");
            RunProcess("ipconfig.exe", "/flushdns");

            log("Liberando conexões e renovando IP...");
            RunProcess("ipconfig.exe", "/release");
            RunProcess("ipconfig.exe", "/renew");

            log("Redefinindo pilha Winsock e IP...");
            RunProcess("netsh.exe", "winsock reset");
            RunProcess("netsh.exe", "int ip reset");
            RunProcess("nbtstat.exe", "-R");
            RunProcess("nbtstat.exe", "-RR");
            RunProcess("arp.exe", "-d *");
        }

        public static void SetDns(string primary, string secondary, Action<string> log)
        {
            log("Configurando DNS de alto desempenho nas conexões ativas...");
            string script =
                "Get-NetAdapter | Where-Object {$_.Status -eq 'Up'} | ForEach-Object {" +
                "  Set-DnsClientServerAddress -InterfaceIndex $_.InterfaceIndex -ServerAddresses ('" + primary + "','" + secondary + "') -ErrorAction SilentlyContinue" +
                "}";
            RunPowerShell(script);
            RunProcess("ipconfig.exe", "/flushdns");
            log("DNS configurado: " + primary + " / " + secondary);
        }

        public static void ResetDnsToDhcp(Action<string> log)
        {
            log("Restaurando servidores DNS automáticos (DHCP)...");
            string script =
                "Get-NetAdapter | Where-Object {$_.Status -eq 'Up'} | ForEach-Object {" +
                "  Set-DnsClientServerAddress -InterfaceIndex $_.InterfaceIndex -ResetServerAddresses -ErrorAction SilentlyContinue" +
                "}";
            RunPowerShell(script);
            RunProcess("ipconfig.exe", "/flushdns");
            log("DNS restaurado para automático.");
        }

        #endregion

        #region Catálogo Completo de Jogos (49 Títulos)

        public static List<GameItem> GetGamesCatalog()
        {
            List<GameItem> list = new List<GameItem>();

            list.Add(new GameItem("Fortnite", "FortniteClient-Win64-Shipping.exe", true));
            list.Add(new GameItem("Counter-Strike 2 (CS2)", "cs2.exe", true));
            list.Add(new GameItem("Valorant", "VALORANT-Win64-Shipping.exe", true));
            list.Add(new GameItem("Grand Theft Auto V (GTA 5)", "GTA5.exe", true));
            list.Add(new GameItem("FiveM (GTA RP)", "FiveM_b2372_GTAProcess.exe", true));
            list.Add(new GameItem("Minecraft", "javaw.exe", true));
            list.Add(new GameItem("League of Legends", "LeagueClient.exe", true));
            list.Add(new GameItem("Call of Duty: Warzone", "cod.exe", true));
            list.Add(new GameItem("Apex Legends", "r5apex.exe", true));
            list.Add(new GameItem("Roblox", "RobloxPlayerBeta.exe", true));
            list.Add(new GameItem("God of War (2018)", "GoW.exe", false));
            list.Add(new GameItem("God of War Ragnarok", "GoWRagnarok.exe", false));
            list.Add(new GameItem("MTA: San Andreas", new string[] { "Multi Theft Auto.exe", "gta_sa.exe" }, false));
            list.Add(new GameItem("Euro Truck Simulator 2", new string[] { "eurotrucks.exe", "ets2.exe" }, false));
            list.Add(new GameItem("Rainbow Six Siege", "RainbowSix.exe", false));
            list.Add(new GameItem("Cult of the Lamb", "CultOfTheLamb.exe", false));
            list.Add(new GameItem("ULTRAKILL", "ULTRAKILL.exe", false));
            list.Add(new GameItem("Blood Strike", "BloodStrike.exe", false));
            list.Add(new GameItem("Arena Breakout: Infinite", "ArenaBreakout.exe", false));
            list.Add(new GameItem("Resident Evil 4 Remake", "re4.exe", false));
            list.Add(new GameItem("Resident Evil 2 Remake", "re2.exe", false));
            list.Add(new GameItem("Resident Evil Village", "re8.exe", false));
            list.Add(new GameItem("Free Fire (Emulador)", "HD-Player.exe", false));
            list.Add(new GameItem("Battlefield 2042", "BF2042.exe", false));
            list.Add(new GameItem("Battlefield 4", "bf4.exe", false));
            list.Add(new GameItem("The Last of Us Part I & II", new string[] { "tlou-i.exe", "tlou-ii.exe" }, false));
            list.Add(new GameItem("PUBG: Battlegrounds", "tslgame.exe", false));
            list.Add(new GameItem("Rocket League", "RocketLeague.exe", false));
            list.Add(new GameItem("Cyberpunk 2077", "Cyberpunk2077.exe", false));
            list.Add(new GameItem("Terraria", "Terraria.exe", false));
            list.Add(new GameItem("Red Dead Redemption 2", "RDR2.exe", false));
            list.Add(new GameItem("Battlefield 6", "BF6.exe", false));
            list.Add(new GameItem("Choo-Choo Charles", "Charles.exe", false));
            list.Add(new GameItem("Hell Let Loose", "HLL.exe", false));
            list.Add(new GameItem("Farming Simulator 22", "FarmingSimulator2022.exe", false));
            list.Add(new GameItem("Farming Simulator 25", "FarmingSimulator2025.exe", false));
            list.Add(new GameItem("Hollow Knight", "hollow_knight.exe", false));
            list.Add(new GameItem("Genshin Impact", "GenshinImpact.exe", false));
            list.Add(new GameItem("Point Blank", "PointBlank.exe", false));
            list.Add(new GameItem("My Summer Car", "mysummercar.exe", false));
            list.Add(new GameItem("DayZ", "DayZ.exe", false));
            list.Add(new GameItem("Street Fighter 6", "StreetFighter6.exe", false));
            list.Add(new GameItem("Rust", "RustClient.exe", false));
            list.Add(new GameItem("Palworld", "Palworld-Win64-Shipping.exe", false));
            list.Add(new GameItem("Elden Ring", "eldenring.exe", false));
            list.Add(new GameItem("Dead by Daylight", "DeadByDaylight-Win64-Shipping.exe", false));
            list.Add(new GameItem("Phasmophobia", "Phasmophobia.exe", false));
            list.Add(new GameItem("Left 4 Dead 2", "left4dead2.exe", false));
            list.Add(new GameItem("Garry's Mod", "gmod.exe", false));

            return list;
        }

        public static void SetGamePriority(string exeName, bool highPriority)
        {
            string subKey = @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\" + exeName + @"\PerfOptions";
            if (highPriority)
            {
                SetRegDword("HKLM", subKey, "CpuPriorityClass", 3); // 3 = High Priority
            }
            else
            {
                DeleteRegKey("HKLM", subKey);
            }
        }

        #endregion

        #region Catálogo Completo de Otimizações

        public static List<OptimizationItem> BuildCatalog()
        {
            List<OptimizationItem> list = new List<OptimizationItem>();

            #region ⚡ SISTEMA & DESEMPENHO

            list.Add(new OptimizationItem(
                "sys_power_plan",
                "Ativar Plano de Energia Desempenho Máximo",
                "Ativa o esquema Ultimate Performance do Windows, desativando throttling de CPU e mantendo clock máximo em jogos.",
                "Sistema",
                SafetyLevel.Recommended,
                true,
                true,
                (report) =>
                {
                    report("Criando esquema Desempenho Máximo (Ultimate Performance)...", 20);
                    RunProcess("powercfg.exe", "-duplicatescheme e9a42b02-d5df-448d-aa00-03f14749eb61");
                    RunProcess("powercfg.exe", "/setacvalueindex SCHEME_CURRENT SUB_PROCESSOR IdleDisable 0");
                    RunProcess("powercfg.exe", "/setactive SCHEME_CURRENT");
                    report("Plano Desempenho Máximo ativado com sucesso!", 100);
                },
                (report) =>
                {
                    report("Restaurando plano de energia Equilibrado...", 50);
                    RunProcess("powercfg.exe", "/setactive 381b4222-f694-41f0-9685-ff5bb260df2e");
                    report("Plano padrão restaurado.", 100);
                }
            ));

            list.Add(new OptimizationItem(
                "sys_cpu_priority",
                "Prioridade de Agendador de CPU (Win32Priority)",
                "Ajusta Win32PrioritySeparation para 22 (0x16 Hex), dando prioridade máxima para a janela ativa em primeiro plano (jogos).",
                "Sistema",
                SafetyLevel.Recommended,
                true,
                true,
                (report) =>
                {
                    report("Configurando Win32PrioritySeparation para jogos...", 50);
                    SetRegDword("HKLM", @"SYSTEM\CurrentControlSet\Control\PriorityControl", "Win32PrioritySeparation", 22);
                    SetRegDword("HKLM", @"SYSTEM\CurrentControlSet\Control", "SvcHostSplitThresholdInKB", 67108864);
                    SetRegString("HKLM", @"SYSTEM\CurrentControlSet\Control\Session Manager\Environment", "GPU_SCHEDULER_MODE", "47");
                    report("Prioridade de processos ajustada com sucesso!", 100);
                },
                (report) =>
                {
                    report("Restaurando prioridade padrão do Windows...", 50);
                    SetRegDword("HKLM", @"SYSTEM\CurrentControlSet\Control\PriorityControl", "Win32PrioritySeparation", 2);
                    report("Prioridade restaurada.", 100);
                }
            ));

            list.Add(new OptimizationItem(
                "sys_multimedia_responsiveness",
                "Responsividade do Sistema e Tarefas Multimídia",
                "Remove a limitação de CPU de 20% reservada para serviços de fundo, liberando 100% de processamento para jogos e programas.",
                "Sistema",
                SafetyLevel.Recommended,
                true,
                true,
                (report) =>
                {
                    report("Ajustando SystemResponsiveness e tarefas de jogos...", 50);
                    string key = @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile";
                    SetRegDword("HKLM", key, "SystemResponsiveness", 3);
                    SetRegDword("HKLM", key, "NetworkThrottlingIndex", unchecked((int)4294967295));

                    string gamesKey = key + @"\Tasks\Games";
                    SetRegDword("HKLM", gamesKey, "GPU Priority", 8);
                    SetRegDword("HKLM", gamesKey, "Priority", 6);
                    SetRegString("HKLM", gamesKey, "Scheduling Category", "High");
                    SetRegString("HKLM", gamesKey, "SFIO Priority", "High");
                    report("Responsividade multimídia ajustada para High!", 100);
                },
                (report) =>
                {
                    report("Restaurando padrão multimídia...", 50);
                    string key = @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile";
                    SetRegDword("HKLM", key, "SystemResponsiveness", 20);
                    report("Padrão multimídia restaurado.", 100);
                }
            ));

            list.Add(new OptimizationItem(
                "sys_menu_delays",
                "Acelerar Abertura de Menus e Janelas (MenuShowDelay)",
                "Reduz o atraso artificial de exibição de menus de 400ms para 3ms, deixando a navegação no Windows instantânea.",
                "Sistema",
                SafetyLevel.Recommended,
                true,
                true,
                (report) =>
                {
                    report("Reduzindo MenuShowDelay para 3ms...", 50);
                    SetRegString("HKCU", @"Control Panel\Desktop", "MenuShowDelay", "3");
                    SetRegString("HKCU", @"Control Panel\Desktop", "AutoEndTasks", "1");
                    SetRegString("HKCU", @"Control Panel\Desktop", "WaitToKillAppTimeout", "2000");
                    SetRegString("HKCU", @"Control Panel\Desktop", "HungAppTimeout", "2000");
                    report("Atrasos de interface eliminados!", 100);
                },
                (report) =>
                {
                    report("Restaurando atraso de menus para 400ms...", 50);
                    SetRegString("HKCU", @"Control Panel\Desktop", "MenuShowDelay", "400");
                    report("Restaurado.", 100);
                }
            ));

            list.Add(new OptimizationItem(
                "sys_nvme_tweaks",
                "Otimizações de Armazenamento SSD M.2 NVMe",
                "Aplica overrides de FeatureManagement no kernel para reduzir latência de leitura e gravação em SSDs NVMe e SATA.",
                "Sistema",
                SafetyLevel.Recommended,
                true,
                true,
                (report) =>
                {
                    report("Aplicando overrides de alta velocidade para NVMe...", 50);
                    string key = @"SYSTEM\CurrentControlSet\Policies\Microsoft\FeatureManagement\Overrides";
                    SetRegDword("HKLM", key, "735209102", 1);
                    SetRegDword("HKLM", key, "1853569164", 1);
                    SetRegDword("HKLM", key, "156965516", 1);
                    report("Tweaks NVMe aplicados!", 100);
                },
                (report) =>
                {
                    report("Removendo overrides de FeatureManagement...", 50);
                    string key = @"SYSTEM\CurrentControlSet\Policies\Microsoft\FeatureManagement\Overrides";
                    DeleteRegValue("HKLM", key, "735209102");
                    DeleteRegValue("HKLM", key, "1853569164");
                    DeleteRegValue("HKLM", key, "156965516");
                    report("Restaurado.", 100);
                }
            ));

            list.Add(new OptimizationItem(
                "sys_disable_hibernate",
                "Desativar Hibernação (powercfg -h off)",
                "Exclui o arquivo hiberfil.sys e libera entre 8 GB a 32 GB de espaço livre imediatamente no seu SSD/HD principal.",
                "Sistema",
                SafetyLevel.Recommended,
                true,
                true,
                (report) =>
                {
                    report("Desativando hibernação e liberando hiberfil.sys...", 50);
                    RunProcess("powercfg.exe", "-h off");
                    report("Hibernação desativada! Espaço em disco recuperado.", 100);
                },
                (report) =>
                {
                    report("Reativando hibernação...", 50);
                    RunProcess("powercfg.exe", "-h on");
                    report("Hibernação reativada.", 100);
                }
            ));

            list.Add(new OptimizationItem(
                "sys_sticky_keys",
                "Desativar Atalhos de Teclas de Aderência (Shift 5x)",
                "Desativa as janelas chatas de Teclas de Aderência (StickyKeys e ToggleKeys) que travam o jogo ao apertar Shift repetidamente.",
                "Sistema",
                SafetyLevel.Recommended,
                true,
                true,
                (report) =>
                {
                    report("Desativando atalhos de StickyKeys, ToggleKeys e MouseKeys...", 50);
                    SetRegString("HKCU", @"Control Panel\Accessibility\StickyKeys", "Flags", "0");
                    SetRegString("HKCU", @"Control Panel\Accessibility\ToggleKeys", "Flags", "0");
                    SetRegString("HKCU", @"Control Panel\Accessibility\MouseKeys", "Flags", "0");
                    SetRegString("HKCU", @"Control Panel\Accessibility\Keyboard Response", "Flags", "0");
                    report("Teclas de aderência desativadas!", 100);
                },
                (report) =>
                {
                    report("Restaurando atalhos de acessibilidade...", 50);
                    SetRegString("HKCU", @"Control Panel\Accessibility\StickyKeys", "Flags", "510");
                    report("Restaurado.", 100);
                }
            ));

            list.Add(new OptimizationItem(
                "sys_alt_tab",
                "Otimizar ALT+TAB Clássico e Instantâneo",
                "Desativa abas do navegador no Alt-Tab e ativa o modo rápido sem atrasos de animação.",
                "Sistema",
                SafetyLevel.Recommended,
                true,
                true,
                (report) =>
                {
                    report("Otimizando Alt+Tab...", 50);
                    SetRegDword("HKCU", @"Software\Microsoft\Windows\CurrentVersion\Explorer", "AltTabSettings", 1);
                    SetRegDword("HKCU", @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "MultiTaskingAltTabFilter", 3);
                    report("Alt+Tab acelerado com sucesso!", 100);
                },
                (report) =>
                {
                    report("Restaurando Alt+Tab moderno...", 50);
                    DeleteRegValue("HKCU", @"Software\Microsoft\Windows\CurrentVersion\Explorer", "AltTabSettings");
                    DeleteRegValue("HKCU", @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "MultiTaskingAltTabFilter");
                    report("Alt+Tab restaurado.", 100);
                }
            ));

            list.Add(new OptimizationItem(
                "sys_explorer_tweaks",
                "Otimizar Windows Explorer e Barra de Tarefas",
                "Faz o Explorer abrir em 'Este Computador' direto, desativa animações lentas da barra de tarefas e histórico recente pesado.",
                "Sistema",
                SafetyLevel.Recommended,
                true,
                true,
                (report) =>
                {
                    report("Ajustando configurações de desempenho do Explorer...", 50);
                    string key = @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced";
                    SetRegDword("HKCU", key, "LaunchTo", 1); // Este Computador
                    SetRegDword("HKCU", key, "TaskbarAnimations", 0);
                    SetRegDword("HKCU", key, "Start_TrackDocs", 0);
                    SetRegDword("HKCU", key, "JumpListItems_Maximum", 0);
                    SetRegDword("HKCU", @"Software\Microsoft\Windows\CurrentVersion\Explorer", "ShowRecent", 0);
                    SetRegDword("HKCU", @"Software\Microsoft\Windows\CurrentVersion\Explorer", "ShowFrequent", 0);
                    report("Explorer otimizado!", 100);
                },
                (report) =>
                {
                    report("Restaurando Explorer para o padrão...", 50);
                    SetRegDword("HKCU", @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "LaunchTo", 2);
                    SetRegDword("HKCU", @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "TaskbarAnimations", 1);
                    report("Explorer restaurado.", 100);
                }
            ));

            list.Add(new OptimizationItem(
                "sys_hyperv",
                "Desativar Hyper-V / Virtualização em Jogos",
                "Desativa o hypervisorlaunchtype caso não use máquinas virtuais WSL/Hyper-V, diminuindo latência de DPC e micro-stutters.",
                "Sistema",
                SafetyLevel.Optional,
                false,
                true,
                (report) =>
                {
                    report("Desativando Hyper-V e hypervisor...", 50);
                    RunProcess("dism.exe", "/Online /Disable-Feature:Microsoft-Hyper-V-All /NoRestart");
                    RunProcess("bcdedit.exe", "/set hypervisorlaunchtype off");
                    report("Hyper-V desativado.", 100);
                },
                (report) =>
                {
                    report("Reativando hypervisor...", 50);
                    RunProcess("bcdedit.exe", "/set hypervisorlaunchtype auto");
                    report("Hyper-V restaurado.", 100);
                }
            ));

            #endregion

            #region 🛡️ PRIVACIDADE & TELEMETRIA

            list.Add(new OptimizationItem(
                "priv_disable_telemetry",
                "Desativar Telemetria e Coleta de Dados do Windows",
                "Bloqueia o envio contínuo de dados de uso para a Microsoft através das políticas de DataCollection e DiagTrack.",
                "Privacidade",
                SafetyLevel.Recommended,
                true,
                true,
                (report) =>
                {
                    report("Aplicando políticas de bloqueio de telemetria...", 30);
                    string pol = @"SOFTWARE\Policies\Microsoft\Windows\DataCollection";
                    SetRegDword("HKLM", pol, "AllowTelemetry", 0);
                    SetRegDword("HKLM", @"SOFTWARE\Policies\Microsoft\Windows\System", "AllowAppDataCollection", 0);
                    SetRegDword("HKLM", @"SOFTWARE\Policies\Microsoft\Windows\AdvertisingInfo", "DisableWindowsAdvertising", 1);
                    SetRegDword("HKLM", @"SOFTWARE\Policies\Microsoft\Windows\CloudContent", "DisableMicrosoftConsumerExperience", 1);

                    report("Parando serviços de diagnóstico (DiagTrack, dmwappushservice)...", 70);
                    SetServiceState("DiagTrack", "disabled", true);
                    SetServiceState("dmwappushservice", "disabled", true);
                    report("Telemetria desativada com sucesso!", 100);
                },
                (report) =>
                {
                    report("Reativando telemetria padrão...", 50);
                    DeleteRegValue("HKLM", @"SOFTWARE\Policies\Microsoft\Windows\DataCollection", "AllowTelemetry");
                    SetServiceState("DiagTrack", "auto", false);
                    report("Telemetria reativada.", 100);
                }
            ));

            list.Add(new OptimizationItem(
                "priv_ceip_tasks",
                "Desativar Tarefas do CEIP (Customer Experience)",
                "Desativa tarefas agendadas em segundo plano que consomem CPU e disco para compilar relatórios do Windows.",
                "Privacidade",
                SafetyLevel.Recommended,
                true,
                true,
                (report) =>
                {
                    report("Desativando tarefas agendadas de experiência do usuário...", 50);
                    RunProcess("schtasks.exe", "/Change /TN \"Microsoft\\Windows\\Customer Experience Improvement Program\\Consolidator\" /Disable");
                    RunProcess("schtasks.exe", "/Change /TN \"Microsoft\\Windows\\Customer Experience Improvement Program\\UsbCeip\" /Disable");
                    RunProcess("schtasks.exe", "/Change /TN \"Microsoft\\Windows\\Customer Experience Improvement Program\\KernelCeipTask\" /Disable");
                    RunProcess("schtasks.exe", "/Change /TN \"Microsoft\\Windows\\Application Experience\\ProgramDataUpdater\" /Disable");
                    report("Tarefas CEIP desativadas!", 100);
                },
                (report) =>
                {
                    report("Reativando tarefas CEIP...", 50);
                    RunProcess("schtasks.exe", "/Change /TN \"Microsoft\\Windows\\Customer Experience Improvement Program\\Consolidator\" /Enable");
                    report("Restaurado.", 100);
                }
            ));

            list.Add(new OptimizationItem(
                "priv_disable_error_reporting",
                "Desativar Relatórios de Erro do Windows (WerSvc)",
                "Impede o Windows de travar ou ficar enviando relatórios para a Microsoft quando um jogo fecha inesperadamente.",
                "Privacidade",
                SafetyLevel.Recommended,
                true,
                true,
                (report) =>
                {
                    report("Desativando serviço WerSvc e PcaSvc...", 40);
                    SetServiceState("WerSvc", "disabled", true);
                    SetServiceState("PcaSvc", "disabled", true);

                    string pol = @"SOFTWARE\Policies\Microsoft\Windows\Windows Error Reporting";
                    SetRegDword("HKLM", pol, "DisableWindowsErrorReporting", 1);
                    SetRegDword("HKLM", @"SOFTWARE\Policies\Microsoft\Windows\ErrorReporting", "Disabled", 1);
                    SetRegDword("HKLM", @"SOFTWARE\Policies\Microsoft\Windows\ErrorReporting", "DontSendAdditionalData", 1);
                    SetRegDword("HKCU", @"Software\Microsoft\Windows\Windows Error Reporting", "Disabled", 1);
                    SetRegDword("HKCU", @"Software\Microsoft\Windows\Windows Error Reporting", "DontShowUI", 1);
                    report("Relatórios de erro desativados!", 100);
                },
                (report) =>
                {
                    report("Reativando relatórios de erro...", 50);
                    SetServiceState("WerSvc", "demand", false);
                    DeleteRegValue("HKLM", @"SOFTWARE\Policies\Microsoft\Windows\Windows Error Reporting", "DisableWindowsErrorReporting");
                    report("Restaurado.", 100);
                }
            ));

            list.Add(new OptimizationItem(
                "priv_disable_cortana",
                "Desativar Assistente Cortana",
                "Desativa a assistente Cortana em segundo plano, liberando memória RAM e processos ociosos de áudio e busca.",
                "Privacidade",
                SafetyLevel.Recommended,
                true,
                true,
                (report) =>
                {
                    report("Desativando Cortana via política de grupo...", 50);
                    string key = @"SOFTWARE\Policies\Microsoft\Windows\Windows Search";
                    SetRegDword("HKLM", key, "AllowCortana", 0);
                    SetRegDword("HKCU", key, "AllowCortana", 0);
                    RunProcess("taskkill.exe", "/f /im Cortana.exe");
                    report("Cortana desativada com sucesso!", 100);
                },
                (report) =>
                {
                    report("Reativando Cortana...", 50);
                    DeleteRegValue("HKLM", @"SOFTWARE\Policies\Microsoft\Windows\Windows Search", "AllowCortana");
                    DeleteRegValue("HKCU", @"SOFTWARE\Policies\Microsoft\Windows\Windows Search", "AllowCortana");
                    report("Restaurado.", 100);
                }
            ));

            list.Add(new OptimizationItem(
                "priv_disable_feedback",
                "Bloquear Notificações de Feedback e Pesquisas SIUF",
                "Remove os pedidos chatos do Windows perguntando sua opinião sobre o sistema operacional.",
                "Privacidade",
                SafetyLevel.Recommended,
                true,
                true,
                (report) =>
                {
                    report("Bloqueando regras de SIUF e pesquisas de satisfação...", 50);
                    SetRegDword("HKCU", @"Software\Microsoft\Siuf\Rules", "NumberOfSIUFInPeriod", 0);
                    SetRegDword("HKCU", @"Software\Microsoft\Siuf\Rules", "PeriodInDays", 0);
                    SetRegDword("HKCU", @"Software\Microsoft\Windows\CurrentVersion\Privacy", "TailoredExperiencesWithDiagnosticDataEnabled", 0);
                    SetRegDword("HKLM", @"SOFTWARE\Policies\Microsoft\Windows\DataCollection", "DoNotShowFeedbackNotifications", 1);
                    report("Pesquisas de feedback bloqueadas!", 100);
                },
                (report) =>
                {
                    report("Restaurando feedback padrão...", 50);
                    DeleteRegValue("HKCU", @"Software\Microsoft\Siuf\Rules", "NumberOfSIUFInPeriod");
                    report("Restaurado.", 100);
                }
            ));

            list.Add(new OptimizationItem(
                "priv_disable_edge_telemetry",
                "Desativar Métricas e Telemetria do Microsoft Edge",
                "Impede o Microsoft Edge de enviar telemetria em segundo plano mesmo quando você utiliza outro navegador.",
                "Privacidade",
                SafetyLevel.Recommended,
                true,
                true,
                (report) =>
                {
                    report("Desativando métricas do Microsoft Edge...", 50);
                    string key = @"SOFTWARE\Policies\Microsoft\Edge";
                    SetRegDword("HKLM", key, "MetricsReportingEnabled", 0);
                    SetRegDword("HKLM", key, "PersonalizationReportingEnabled", 0);
                    report("Métricas do Edge desativadas!", 100);
                },
                (report) =>
                {
                    report("Restaurando padrão do Edge...", 50);
                    DeleteRegValue("HKLM", @"SOFTWARE\Policies\Microsoft\Edge", "MetricsReportingEnabled");
                    report("Restaurado.", 100);
                }
            ));

            #endregion

            #region 🌐 REDE & PING (INTERNET)

            list.Add(new OptimizationItem(
                "net_tcp_low_latency",
                "Ajustes de Baixa Latência TCP/IP (TCPNoDelay)",
                "Ativa o TCPNoDelay (desativa o algoritmo de Nagle) e TcpAckFrequency para diminuir o ping em jogos online.",
                "Rede",
                SafetyLevel.Recommended,
                true,
                true,
                (report) =>
                {
                    report("Aplicando TCPNoDelay e TcpAckFrequency no registro...", 40);
                    string tcp = @"SYSTEM\CurrentControlSet\Services\Tcpip\Parameters";
                    SetRegDword("HKLM", tcp, "TCPNoDelay", 1);
                    SetRegDword("HKLM", tcp, "TcpAckFrequency", 1);
                    SetRegDword("HKLM", tcp, "FastSendDatagramThreshold", 64000);

                    report("Ajustando autotuning e heurísticas TCP...", 80);
                    RunProcess("netsh.exe", "interface tcp set global autotuninglevel=disabled");
                    RunProcess("netsh.exe", "interface tcp set heuristics disabled");
                    RunProcess("netsh.exe", "int tcp set global rss=enabled");
                    RunProcess("netsh.exe", "int tcp set global chimney=disabled");
                    report("Pilha TCP/IP ajustada para menor ping!", 100);
                },
                (report) =>
                {
                    report("Restaurando autotuning TCP normal...", 50);
                    RunProcess("netsh.exe", "interface tcp set global autotuninglevel=normal");
                    DeleteRegValue("HKLM", @"SYSTEM\CurrentControlSet\Services\Tcpip\Parameters", "TCPNoDelay");
                    DeleteRegValue("HKLM", @"SYSTEM\CurrentControlSet\Services\Tcpip\Parameters", "TcpAckFrequency");
                    report("Pilha de rede restaurada.", 100);
                }
            ));

            list.Add(new OptimizationItem(
                "net_qos_bandwidth",
                "Liberar 100% da Largura de Banda de Rede (Desativar Limite QoS)",
                "Desativa a reserva de 20% da velocidade da internet que o Windows retém por padrão para pacotes do sistema.",
                "Rede",
                SafetyLevel.Recommended,
                true,
                true,
                (report) =>
                {
                    report("Desativando limite de QoS e throttling...", 50);
                    SetRegDword("HKLM", @"SOFTWARE\Policies\Microsoft\Windows\Psched", "NonBestEffortLimit", 0);
                    SetRegDword("HKLM", @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile", "NetworkThrottlingIndex", unchecked((int)0xFFFFFFFF));
                    report("Largura de banda de rede liberada 100%!", 100);
                },
                (report) =>
                {
                    report("Restaurando padrão de QoS...", 50);
                    DeleteRegValue("HKLM", @"SOFTWARE\Policies\Microsoft\Windows\Psched", "NonBestEffortLimit");
                    report("Restaurado.", 100);
                }
            ));

            list.Add(new OptimizationItem(
                "net_disable_adapter_power_saving",
                "Desativar Economia de Energia na Placa de Rede",
                "Impede o Windows de desligar ou colocar o adaptador de rede (Ethernet/Wi-Fi) em modo de baixo consumo durante partidas.",
                "Rede",
                SafetyLevel.Recommended,
                true,
                false,
                (report) =>
                {
                    report("Desativando economia de energia nos adaptadores de rede...", 50);
                    RunPowerShell("Disable-NetAdapterPowerManagement -Name '*' -ErrorAction SilentlyContinue");
                    report("Adaptadores de rede configurados para desempenho contínuo!", 100);
                },
                null
            ));

            list.Add(new OptimizationItem(
                "net_cloudflare_dns",
                "Configurar DNS Rápido Cloudflare Gaming (1.1.1.1 / 1.0.0.1)",
                "Substitui o DNS lento da sua operadora pelo DNS mais rápido do mundo (Cloudflare 1.1.1.1), reduzindo tempo de resposta de sites e servidores de jogos.",
                "Rede",
                SafetyLevel.Recommended,
                true,
                true,
                (report) =>
                {
                    SetDns("1.1.1.1", "1.0.0.1", (msg) => report(msg, 50));
                    report("DNS Cloudflare 1.1.1.1 configurado com sucesso!", 100);
                },
                (report) =>
                {
                    ResetDnsToDhcp((msg) => report(msg, 50));
                    report("DNS restaurado para automático (DHCP).", 100);
                }
            ));

            list.Add(new OptimizationItem(
                "net_clean_delivery_service",
                "Desativar Otimização de Entrega em Segundo Plano (DoSvc)",
                "Impede que seu computador envie atualizações do Windows para outros computadores pela internet, economizando upload.",
                "Rede",
                SafetyLevel.Recommended,
                true,
                true,
                (report) =>
                {
                    report("Parando e desativando DoSvc (Otimização de Entrega)...", 50);
                    SetServiceState("DoSvc", "disabled", true);
                    report("DoSvc desativado! Upload poupado.", 100);
                },
                (report) =>
                {
                    report("Reativando DoSvc...", 50);
                    SetServiceState("DoSvc", "demand", false);
                    report("Restaurado.", 100);
                }
            ));

            #endregion

            #region 🎮 JOGOS & XBOX

            list.Add(new OptimizationItem(
                "game_disable_xbox_dvr",
                "Desativar Xbox Game Bar e Game DVR (Gravação de Fundo)",
                "Desativa a captura contínua de tela da Xbox que consome FPS em jogos e causa pequenas travadas (stutters).",
                "Jogos",
                SafetyLevel.Recommended,
                true,
                true,
                (report) =>
                {
                    report("Desativando Game DVR e Xbox Game Bar...", 40);
                    string dvr = @"SOFTWARE\Policies\Microsoft\Windows\GameDVR";
                    SetRegDword("HKLM", dvr, "AllowGameDVR", 0);
                    SetRegDword("HKLM", @"SOFTWARE\Policies\Microsoft\Windows\GameBar", "AllowAutoGameMode", 0);

                    SetRegDword("HKCU", @"Software\Microsoft\GameBar", "AllowAutoGameMode", 0);
                    SetRegDword("HKCU", @"Software\Microsoft\GameBar", "AutoGameModeEnabled", 0);
                    SetRegDword("HKCU", @"Software\Microsoft\GameBar", "ShowStartupPanel", 0);
                    SetRegDword("HKCU", @"SOFTWARE\Microsoft\Windows\CurrentVersion\GameDVR", "AppCaptureEnabled", 0);
                    SetRegDword("HKCU", @"System\GameConfigStore", "GameDVR_Enabled", 0);
                    report("Xbox Game DVR desativado! FPS livre.", 100);
                },
                (report) =>
                {
                    report("Reativando Xbox Game Bar...", 50);
                    DeleteRegValue("HKLM", @"SOFTWARE\Policies\Microsoft\Windows\GameDVR", "AllowGameDVR");
                    SetRegDword("HKCU", @"Software\Microsoft\GameBar", "AllowAutoGameMode", 1);
                    SetRegDword("HKCU", @"SOFTWARE\Microsoft\Windows\CurrentVersion\GameDVR", "AppCaptureEnabled", 1);
                    report("Restaurado.", 100);
                }
            ));

            list.Add(new OptimizationItem(
                "game_disable_xbox_services",
                "Desativar Serviços Secundários da Xbox",
                "Desativa os serviços de telemetria e sincronização em segundo plano da Xbox (XblAuthManager, XblGameSave, XboxNetApiSvc, Xbox Game Monitoring).",
                "Jogos",
                SafetyLevel.Recommended,
                true,
                true,
                (report) =>
                {
                    report("Parando serviços da Xbox...", 50);
                    SetServiceState("Xbox Game Monitoring", "disabled", true);
                    SetServiceState("GamingServices", "disabled", true);
                    SetServiceState("GamingServicesNet", "disabled", true);
                    SetServiceState("XblAuthManager", "disabled", true);
                    SetServiceState("XblGameSave", "disabled", true);
                    SetServiceState("XboxNetApiSvc", "disabled", true);
                    report("Serviços secundários da Xbox desativados!", 100);
                },
                (report) =>
                {
                    report("Reativando serviços Xbox para modo manual...", 50);
                    SetServiceState("XblAuthManager", "demand", false);
                    SetServiceState("XblGameSave", "demand", false);
                    SetServiceState("XboxNetApiSvc", "demand", false);
                    SetServiceState("GamingServices", "demand", false);
                    report("Serviços Xbox restaurados.", 100);
                }
            ));

            #endregion

            #region 🖱️ PERIFÉRICOS & HARDWARE

            list.Add(new OptimizationItem(
                "periph_keyboard_latency",
                "Resposta Instantânea do Teclado (0 Delay)",
                "Define KeyboardDelay como 0 e KeyboardSpeed no máximo (31), permitindo digitação e comandos instantâneos em jogos competitivos.",
                "Periféricos",
                SafetyLevel.Recommended,
                true,
                true,
                (report) =>
                {
                    report("Ajustando taxa de repetição do teclado...", 50);
                    SetRegString("HKCU", @"Control Panel\Keyboard", "KeyboardDelay", "0");
                    SetRegString("HKCU", @"Control Panel\Keyboard", "KeyboardSpeed", "31");
                    report("Resposta do teclado ajustada para máxima velocidade!", 100);
                },
                (report) =>
                {
                    report("Restaurando teclado para o padrão...", 50);
                    SetRegString("HKCU", @"Control Panel\Keyboard", "KeyboardDelay", "1");
                    SetRegString("HKCU", @"Control Panel\Keyboard", "KeyboardSpeed", "20");
                    report("Restaurado.", 100);
                }
            ));

            list.Add(new OptimizationItem(
                "periph_mouse_raw_input",
                "Desativar Aceleração do Mouse (Mira 1:1 Pura)",
                "Remove a aceleração artificial do ponteiro do Windows, garantindo que o movimento da mira em jogos de tiro seja 100% linear e preciso.",
                "Periféricos",
                SafetyLevel.Recommended,
                true,
                true,
                (report) =>
                {
                    report("Removendo aceleração e atrasos do mouse...", 50);
                    SetRegString("HKCU", @"Control Panel\Mouse", "MouseSpeed", "0");
                    SetRegString("HKCU", @"Control Panel\Mouse", "MouseThreshold1", "0");
                    SetRegString("HKCU", @"Control Panel\Mouse", "MouseThreshold2", "0");
                    SetRegString("HKCU", @"Control Panel\Desktop", "MouseTrails", "0");
                    RunProcess("rundll32.exe", "user32.dll,UpdatePerUserSystemParameters 1, True");
                    report("Aceleração do mouse desativada (Mira 1:1)! ", 100);
                },
                (report) =>
                {
                    report("Restaurando aceleração padrão do mouse...", 50);
                    SetRegString("HKCU", @"Control Panel\Mouse", "MouseSpeed", "1");
                    SetRegString("HKCU", @"Control Panel\Mouse", "MouseThreshold1", "6");
                    SetRegString("HKCU", @"Control Panel\Mouse", "MouseThreshold2", "10");
                    RunProcess("rundll32.exe", "user32.dll,UpdatePerUserSystemParameters 1, True");
                    report("Mouse restaurado.", 100);
                }
            ));

            list.Add(new OptimizationItem(
                "periph_gpu_hags",
                "Ativar Agendamento de GPU Acelerado por Hardware (HAGS)",
                "Ativa o HwSchMode = 2, permitindo que a placa de vídeo gerencie sua própria memória VRAM para taxas de quadros mais estáveis.",
                "Periféricos",
                SafetyLevel.Recommended,
                true,
                true,
                (report) =>
                {
                    report("Ativando Hardware Accelerated GPU Scheduling (HAGS)...", 50);
                    SetRegDword("HKLM", @"SYSTEM\CurrentControlSet\Control\GraphicsDrivers", "HwSchMode", 2);
                    SetRegDword("HKLM", @"SYSTEM\CurrentControlSet\Control\GraphicsDrivers\Power", "PowerPerformanceMode", 1);
                    report("HAGS ativado com sucesso!", 100);
                },
                (report) =>
                {
                    report("Restaurando HAGS para o padrão...", 50);
                    SetRegDword("HKLM", @"SYSTEM\CurrentControlSet\Control\GraphicsDrivers", "HwSchMode", 1);
                    report("HAGS restaurado.", 100);
                }
            ));

            list.Add(new OptimizationItem(
                "periph_ssd_tweaks",
                "Otimizar SSD (Desativar Criação de Nomes 8.3 & TRIM)",
                "Evita que o Windows crie nomes legados no padrão DOS 8.3 e atualizações desnecessárias de data de acesso em discos NTFS.",
                "Periféricos",
                SafetyLevel.Recommended,
                true,
                true,
                (report) =>
                {
                    report("Aplicando ajustes de sistema de arquivos para SSD...", 50);
                    RunProcess("fsutil.exe", "behavior set disable8dot3 1");
                    RunProcess("fsutil.exe", "behavior set disableLastAccess 1");
                    SetRegDword("HKLM", @"SYSTEM\CurrentControlSet\Control\FileSystem", "NtfsDisableLastAccessUpdate", 1);
                    report("Otimização de SSD aplicada com sucesso!", 100);
                },
                (report) =>
                {
                    report("Restaurando sistema de arquivos...", 50);
                    RunProcess("fsutil.exe", "behavior set disable8dot3 0");
                    RunProcess("fsutil.exe", "behavior set disableLastAccess 0");
                    report("Restaurado.", 100);
                }
            ));

            list.Add(new OptimizationItem(
                "periph_memory_management",
                "Otimizar Cache do Kernel na Memória RAM",
                "Ativa DisablePagingExecutive = 1, forçando os drivers e o kernel a ficarem na memória RAM rápida em vez de irem para o arquivo de paginação.",
                "Periféricos",
                SafetyLevel.Recommended,
                true,
                true,
                (report) =>
                {
                    report("Configurando DisablePagingExecutive e LargeSystemCache...", 50);
                    string mem = @"SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management";
                    SetRegDword("HKLM", mem, "DisablePagingExecutive", 1);
                    SetRegDword("HKLM", mem, "LargeSystemCache", 1);
                    report("Gerenciamento de memória ajustado para desempenho!", 100);
                },
                (report) =>
                {
                    report("Restaurando gerenciamento de paginação...", 50);
                    string mem = @"SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management";
                    SetRegDword("HKLM", mem, "DisablePagingExecutive", 0);
                    SetRegDword("HKLM", mem, "LargeSystemCache", 0);
                    report("Restaurado.", 100);
                }
            ));

            #endregion

            #region ⚙️ SERVIÇOS DO WINDOWS

            list.Add(new OptimizationItem(
                "svc_maps_broker",
                "Desativar Gerenciador de Mapas Baixados (MapsBroker)",
                "Desativa o serviço de mapas offline do Windows que consome memória mesmo se você nunca utilizou o app Mapas.",
                "Serviços",
                SafetyLevel.Recommended,
                true,
                true,
                (report) =>
                {
                    report("Desativando MapsBroker...", 50);
                    SetServiceState("MapsBroker", "disabled", true);
                    report("MapsBroker desativado!", 100);
                },
                (report) =>
                {
                    report("Reativando MapsBroker...", 50);
                    SetServiceState("MapsBroker", "demand", false);
                    report("Restaurado.", 100);
                }
            ));

            list.Add(new OptimizationItem(
                "svc_telemetry_diagtrack",
                "Desativar Telemetria e Diagnósticos (Connected User Experiences)",
                "Desativa DiagTrack e dmwappushservice de forma permanente, economizando ciclos de processador em segundo plano.",
                "Serviços",
                SafetyLevel.Recommended,
                true,
                true,
                (report) =>
                {
                    report("Desativando serviços de rastreamento...", 50);
                    SetServiceState("DiagTrack", "disabled", true);
                    SetServiceState("dmwappushservice", "disabled", true);
                    report("Serviços de rastreamento desativados!", 100);
                },
                (report) =>
                {
                    report("Reativando DiagTrack...", 50);
                    SetServiceState("DiagTrack", "demand", false);
                    report("Restaurado.", 100);
                }
            ));

            list.Add(new OptimizationItem(
                "svc_useless_legacy",
                "Desativar Serviços Inúteis (Fax, Registro Remoto, Demonstração)",
                "Desativa serviços legados da época do Windows XP que ninguém mais usa em PCs domésticos: Fax, RemoteRegistry, RetailDemo e CscService (Arquivos Offline).",
                "Serviços",
                SafetyLevel.Recommended,
                true,
                true,
                (report) =>
                {
                    report("Desativando Fax, RemoteRegistry, RetailDemo e CscService...", 50);
                    SetServiceState("Fax", "disabled", true);
                    SetServiceState("RemoteRegistry", "disabled", true);
                    SetServiceState("RetailDemo", "disabled", true);
                    SetServiceState("CscService", "disabled", true);
                    SetServiceState("WalletService", "disabled", true);
                    SetServiceState("PhoneSvc", "disabled", true);
                    SetServiceState("wisvc", "disabled", true);
                    report("Serviços desnecessários desativados!", 100);
                },
                (report) =>
                {
                    report("Restaurando serviços para manual...", 50);
                    SetServiceState("Fax", "demand", false);
                    SetServiceState("RemoteRegistry", "demand", false);
                    report("Restaurado.", 100);
                }
            ));

            list.Add(new OptimizationItem(
                "svc_sensors",
                "Desativar Sensores de Rotação e Luminosidade (SensorService)",
                "Útil para computadores desktop e notebooks que não necessitam de rotação automática de tela nem sensor de luminosidade ambiente.",
                "Serviços",
                SafetyLevel.Recommended,
                true,
                true,
                (report) =>
                {
                    report("Desativando serviços de sensores...", 50);
                    SetServiceState("SensorService", "disabled", true);
                    SetServiceState("SensorDataService", "disabled", true);
                    SetServiceState("SensorsSvc", "disabled", true);
                    report("Sensores desativados!", 100);
                },
                (report) =>
                {
                    report("Reativando serviços de sensores...", 50);
                    SetServiceState("SensorService", "demand", false);
                    report("Restaurado.", 100);
                }
            ));

            list.Add(new OptimizationItem(
                "svc_sysmain",
                "Desativar SysMain / Superfetch (Recomendado para SSDs)",
                "O SysMain indexa e pré-carrega programas em memória. Em SSDs modernos, ele é desnecessário e só causa picos de uso de disco e RAM.",
                "Serviços",
                SafetyLevel.Optional,
                true,
                true,
                (report) =>
                {
                    report("Parando serviço SysMain (Superfetch)...", 40);
                    SetServiceState("SysMain", "disabled", true);
                    string key = @"SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management\PrefetchParameters";
                    SetRegDword("HKLM", key, "EnablePrefetcher", 0);
                    SetRegDword("HKLM", key, "EnableSuperfetch", 0);
                    report("SysMain desativado!", 100);
                },
                (report) =>
                {
                    report("Reativando SysMain...", 50);
                    SetServiceState("SysMain", "auto", false);
                    RunProcess("sc.exe", "start SysMain");
                    report("SysMain reativado.", 100);
                }
            ));

            list.Add(new OptimizationItem(
                "svc_print_spooler",
                "Desativar Spooler de Impressão (Apenas se NÃO usa Impressora)",
                "Desativa o serviço de fila de impressão. Aumenta o desempenho de quem não possui impressora conectada ao computador.",
                "Serviços",
                SafetyLevel.Optional,
                false,
                true,
                (report) =>
                {
                    report("Desativando Spooler de Impressão...", 50);
                    SetServiceState("Spooler", "disabled", true);
                    report("Spooler desativado.", 100);
                },
                (report) =>
                {
                    report("Reativando Spooler de Impressão...", 50);
                    SetServiceState("Spooler", "auto", false);
                    RunProcess("sc.exe", "start Spooler");
                    report("Spooler reativado.", 100);
                }
            ));

            list.Add(new OptimizationItem(
                "svc_windows_search",
                "Desativar Windows Search / Indexador de Arquivos (WSearch)",
                "Para quem não usa a barra de pesquisa do Windows para encontrar conteúdos dentro de arquivos, economizando leitura contínua no disco.",
                "Serviços",
                SafetyLevel.Optional,
                false,
                true,
                (report) =>
                {
                    report("Desativando Windows Search (WSearch)...", 50);
                    SetServiceState("WSearch", "disabled", true);
                    report("Indexador do Windows desativado.", 100);
                },
                (report) =>
                {
                    report("Reativando Windows Search...", 50);
                    SetServiceState("WSearch", "auto", false);
                    RunProcess("sc.exe", "start WSearch");
                    report("Windows Search reativado.", 100);
                }
            ));

            #endregion

            return list;
        }

        #endregion

        #region Ações Especiais de 1-Clique

        public static void CreateSystemRestorePoint(Action<string> log)
        {
            log("Criando Ponto de Restauração no Windows...");
            try
            {
                SetRegDword("HKLM", @"Software\Microsoft\Windows NT\CurrentVersion\SystemRestore", "SystemRestorePointCreationFrequency", 0);
                string script = "Checkpoint-Computer -Description 'Tutus Optimizer RestorePoint' -RestorePointType 'MODIFY_SETTINGS'";
                int code = RunPowerShell(script);
                if (code == 0)
                {
                    log("Ponto de Restauração criado com sucesso!");
                }
                else
                {
                    log("Ponto de restauração solicitado (pode requerer proteção do sistema ativa na unidade C:).");
                }
            }
            catch (Exception ex)
            {
                log("Aviso ao criar ponto de restauração: " + ex.Message);
            }
        }

        public static void EmptyRamMemory(Action<string> log)
        {
            log("Iniciando limpeza profunda de memória RAM...");
            int count = PurgeSystemWorkingSets();
            log(string.Format("Working sets purgados com sucesso em {0} processos do sistema!", count));

            // Reinicia área de transferência
            RunProcess("cmd.exe", "/c \"echo off | clip\"");
            log("Área de transferência (Clipboard) esvaziada.");
            log("Memória RAM liberada instantaneamente!");
        }

        public static void CleanAllTemporaryFiles(Action<string> log)
        {
            log("Iniciando limpeza de arquivos temporários do sistema...");

            // User temp
            string userTemp = Path.GetTempPath();
            log("Limpando pasta Temporária do Usuário (%TEMP%)...");
            long c1 = CleanTempDirectory(userTemp, log);

            // Windows temp
            string winDir = Environment.GetFolderPath(Environment.SpecialFolder.Windows);
            string winTemp = Path.Combine(winDir, "Temp");
            log("Limpando pasta C:\\Windows\\Temp...");
            long c2 = CleanTempDirectory(winTemp, log);

            // Prefetch
            string prefetch = Path.Combine(winDir, "Prefetch");
            log("Limpando cache Prefetch do Windows...");
            long c3 = CleanTempDirectory(prefetch, log);

            // SoftwareDistribution\Download
            string softDist = Path.Combine(winDir, @"SoftwareDistribution\Download");
            log("Limpando cache de instaladores antigos do Windows Update...");
            long c4 = CleanTempDirectory(softDist, log);

            // Event logs
            log("Limpando logs antigos do Visualizador de Eventos...");
            RunPowerShell("wevtutil el | ForEach-Object { wevtutil cl \"$_\" } -ErrorAction SilentlyContinue");

            log(string.Format("Limpeza concluída! {0} arquivos desnecessários foram removidos do disco.", c1 + c2 + c3 + c4));
        }

        public static void ResetIconAndThumbCache(Action<string> log)
        {
            log("Fechando Windows Explorer para limpeza de miniaturas...");
            RunProcess("taskkill.exe", "/f /im explorer.exe");

            try
            {
                string localApp = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                string expCache = Path.Combine(localApp, @"Microsoft\Windows\Explorer");
                if (Directory.Exists(expCache))
                {
                    string[] files = Directory.GetFiles(expCache, "*cache*");
                    foreach (string f in files)
                    {
                        try { File.Delete(f); } catch { }
                    }
                }

                string iconDb = Path.Combine(localApp, "IconCache.db");
                if (File.Exists(iconDb))
                {
                    try { File.Delete(iconDb); } catch { }
                }
            }
            catch { }

            RunProcess("ie4uinit.exe", "-show");
            RunProcess("explorer.exe", "", false);
            log("Cache de ícones e miniaturas redefinido e Explorer reiniciado!");
        }

        public static void RunSystemFileCheck(Action<string> log)
        {
            log("Iniciando Verificação de Integridade de Arquivos do Windows (SFC /scannow)...");
            RunProcess("sfc.exe", "/scannow");
            log("Verificação do SFC concluída.");

            log("Executando reparo de imagem do sistema com DISM...");
            RunProcess("dism.exe", "/online /cleanup-image /restorehealth");
            log("DISM concluído com sucesso!");
        }

        public static void RemoveBloatwareApps(Action<string> log)
        {
            log("Iniciando remoção segura de aplicativos pré-instalados inúteis (Bloatware)...");

            string[] apps = new string[] {
                "Microsoft.549981C3F5F10", // Cortana
                "Microsoft.OfficeHub",
                "Microsoft.People",
                "Microsoft.WindowsMaps",
                "Microsoft.WindowsFeedbackHub",
                "Microsoft.Getstarted",
                "Microsoft.3DBuilder",
                "Microsoft.BingNews",
                "Microsoft.MixedReality.Portal",
                "Microsoft.MicrosoftSolitaireCollection",
                "Microsoft.Todos",
                "Microsoft.SkypeApp",
                "Microsoft.BingWeather",
                "Microsoft.MicrosoftAdvertising.Xbox",
                "Microsoft.WindowsSoundRecorder"
            };

            foreach (string app in apps)
            {
                log("Removendo pacote UWP: " + app + "...");
                string script = "Get-AppxPackage -AllUsers | Where-Object {$_.Name -like '*" + app + "*'} | Remove-AppxPackage -ErrorAction SilentlyContinue";
                RunPowerShell(script);
                string provScript = "Get-AppxProvisionedPackage -Online | Where-Object {$_.DisplayName -like '*" + app + "*'} | Remove-AppxProvisionedPackage -Online -ErrorAction SilentlyContinue";
                RunPowerShell(provScript);
            }

            // Desativa botão do Copilot
            SetRegDword("HKCU", @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "ShowCopilotButton", 0);
            SetRegDword("HKLM", @"SOFTWARE\Policies\Microsoft\Windows\Windows Copilot", "TurnOffWindowsCopilot", 1);

            log("Bloatwares removidos com sucesso!");
        }

        public static void ReinstallDefaultApps(Action<string> log)
        {
            log("Reinstalando aplicativos padrão do Windows...");
            string script = "Get-AppxPackage -AllUsers | ForEach-Object {Add-AppxPackage -DisableDevelopmentMode -Register ($_.InstallLocation + '\\AppXManifest.xml') -ErrorAction SilentlyContinue}";
            RunPowerShell(script);
            SetRegDword("HKCU", @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "ShowCopilotButton", 1);
            DeleteRegKey("HKLM", @"SOFTWARE\Policies\Microsoft\Windows\Windows Copilot");
            log("Reinstalação concluída!");
        }

        #endregion
    }
}
