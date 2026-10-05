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

        [DllImport("winmm.dll", EntryPoint = "timeGetDevCaps")]
        public static extern int TimeGetDevCaps(ref TIMECAPS ptc, int cbtc);

        [StructLayout(LayoutKind.Sequential)]
        public struct TIMECAPS
        {
            public uint wPeriodMin;
            public uint wPeriodMax;
        }

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

            RunProcess("cmd.exe", "/c \"echo off | clip\"");
            log("Área de transferência (Clipboard) esvaziada.");
            log("Memória RAM liberada instantaneamente!");
        }

        public static void CleanAllTemporaryFiles(Action<string> log)
        {
            log("Iniciando limpeza de arquivos temporários do sistema...");

            string userTemp = Path.GetTempPath();
            log("Limpando pasta Temporária do Usuário (%TEMP%)...");
            long c1 = CleanTempDirectory(userTemp, log);

            string winDir = Environment.GetFolderPath(Environment.SpecialFolder.Windows);
            string winTemp = Path.Combine(winDir, "Temp");
            log("Limpando pasta C:\\Windows\\Temp...");
            long c2 = CleanTempDirectory(winTemp, log);

            string prefetch = Path.Combine(winDir, "Prefetch");
            log("Limpando cache Prefetch do Windows...");
            long c3 = CleanTempDirectory(prefetch, log);

            string softDist = Path.Combine(winDir, @"SoftwareDistribution\Download");
            log("Limpando cache de instaladores antigos do Windows Update...");
            long c4 = CleanTempDirectory(softDist, log);

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

        #region Utilitários de Validação com Regex

        public static bool ValidateAndRunProcess(string fileName, string args, bool wait = true)
        {
            if (string.IsNullOrEmpty(fileName) || !Regex.IsMatch(fileName, @"^[a-zA-Z0-9_\-\.\s]+$"))
                return false;

            int exitCode = RunProcess(fileName, args, wait);
            return exitCode != -1;
        }

        public static bool ValidateAndSetRegDword(string rootHive, string subKey, string valueName, int value)
        {
            if (string.IsNullOrEmpty(subKey) || !Regex.IsMatch(subKey, @"^[a-zA-Z0-9_\\\-\s]+$"))
                return false;

            return SetRegDword(rootHive, subKey, valueName, value);
        }

        #endregion
    }
}
