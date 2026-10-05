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
                SetRegDword("HKLM", subKey, "CpuPriorityClass", 3);
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
                "Ativa o esquema Ultimate Performance do Windows, mantendo o processador em clock máximo em jogos.",
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
                "Prioridade de Agendador de CPU (Modo Foco em Jogos)",
                "Ajusta a prioridade para que a janela ativa em primeiro plano (seu jogo) tenha resposta imediata do processador.",
                "Sistema",
                SafetyLevel.Recommended,
                true,
                true,
                (report) =>
                {
                    report("Configurando prioridade máxima para a janela ativa...", 50);
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
                "Libera 100% do processamento para jogos, removendo a reserva de 20% do Windows para tarefas secundárias.",
                "Sistema",
                SafetyLevel.Recommended,
                true,
                true,
                (report) =>
                {
                    report("Ajustando responsividade multimídia e jogos...", 50);
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
                "sys_game_mode",
                "Ativar Modo de Jogo Oficial do Windows",
                "Ativa o Game Mode nativo do Windows, impedindo que o Windows Update e rotinas pesadas roubem FPS durante partidas.",
                "Sistema",
                SafetyLevel.Recommended,
                true,
                true,
                (report) =>
                {
                    report("Ativando Modo de Jogo Oficial...", 50);
                    SetRegDword("HKCU", @"Software\Microsoft\GameBar", "AutoGameModeEnabled", 1);
                    SetRegDword("HKCU", @"Software\Microsoft\GameBar", "AllowAutoGameMode", 1);
                    report("Modo de Jogo ativado!", 100);
                },
                (report) =>
                {
                    report("Desativando Modo de Jogo...", 50);
                    SetRegDword("HKCU", @"Software\Microsoft\GameBar", "AutoGameModeEnabled", 0);
                    report("Restaurado.", 100);
                }
            ));

            list.Add(new OptimizationItem(
                "sys_visual_effects",
                "Otimizar Efeitos Visuais para Máxima Fluidez",
                "Desativa sombras sob janelas e efeitos de transparência pesados do Windows, liberando memória e acelerando renderização.",
                "Sistema",
                SafetyLevel.Recommended,
                true,
                true,
                (report) =>
                {
                    report("Ajustando efeitos visuais para melhor desempenho...", 50);
                    string desktop = @"Control Panel\Desktop";
                    SetRegString("HKCU", desktop, "UserPreferencesMask", "90,12,03,80,10,00,00,00");
                    SetRegDword("HKCU", @"Software\Microsoft\Windows\CurrentVersion\Explorer\VisualEffects", "VisualFXSetting", 2);
                    SetRegDword("HKCU", @"Software\Microsoft\Windows\DWM", "EnableAeroPeek", 0);
                    report("Efeitos visuais otimizados!", 100);
                },
                (report) =>
                {
                    report("Restaurando efeitos visuais...", 50);
                    SetRegDword("HKCU", @"Software\Microsoft\Windows\CurrentVersion\Explorer\VisualEffects", "VisualFXSetting", 1);
                    report("Restaurado.", 100);
                }
            ));

            list.Add(new OptimizationItem(
                "sys_menu_delays",
                "Acelerar Abertura de Menus e Janelas (MenuShowDelay)",
                "Reduz o atraso artificial de exibição de menus de 400ms para 3ms, deixando a navegação instantânea.",
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
                "Aplica ajustes no kernel para reduzir latência de leitura e gravação em SSDs NVMe e SATA.",
                "Sistema",
                SafetyLevel.Recommended,
                true,
                true,
                (report) =>
                {
                    report("Aplicando ajustes de alta velocidade para NVMe...", 50);
                    string key = @"SYSTEM\CurrentControlSet\Policies\Microsoft\FeatureManagement\Overrides";
                    SetRegDword("HKLM", key, "735209102", 1);
                    SetRegDword("HKLM", key, "1853569164", 1);
                    SetRegDword("HKLM", key, "156965516", 1);
                    report("Ajustes NVMe aplicados!", 100);
                },
                (report) =>
                {
                    report("Removendo ajustes de FeatureManagement...", 50);
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
                "Desativar Janela Chata de Teclas de Aderência (Shift 5x)",
                "Desativa as janelas chatas de Teclas de Aderência que travam o jogo ao apertar Shift repetidamente.",
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
                "Otimizar Windows Explorer e Pastas",
                "Faz o Explorer abrir em 'Este Computador' direto e desativa histórico recente pesado.",
                "Sistema",
                SafetyLevel.Recommended,
                true,
                true,
                (report) =>
                {
                    report("Ajustando configurações de desempenho do Explorer...", 50);
                    string key = @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced";
                    SetRegDword("HKCU", key, "LaunchTo", 1);
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
                "Desativar Hyper-V / Virtualização Profunda",
                "Desativa a camada de virtualização caso não use máquinas virtuais WSL, diminuindo latência de DPC nos jogos.",
                "Sistema",
                SafetyLevel.Optional,
                false,
                true,
                (report) =>
                {
                    report("Desativando Hyper-V...", 50);
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
                "Bloqueia o envio contínuo de dados de uso e diagnósticos para a Microsoft.",
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
                "Desativa a assistente Cortana em segundo plano, liberando memória RAM e processos ociosos.",
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
                "Bloquear Notificações de Feedback e Pesquisas",
                "Remove os pedidos chatos do Windows perguntando sua opinião sobre o sistema.",
                "Privacidade",
                SafetyLevel.Recommended,
                true,
                true,
                (report) =>
                {
                    report("Bloqueando pesquisas de satisfação...", 50);
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

            list.Add(new OptimizationItem(
                "priv_disable_location",
                "Desativar Rastreamento de Localização em Segundo Plano",
                "Desativa os serviços de localização do Windows para quem joga no PC de mesa, poupando bateria e CPU.",
                "Privacidade",
                SafetyLevel.Recommended,
                true,
                true,
                (report) =>
                {
                    report("Desativando serviço de localização...", 50);
                    SetRegDword("HKLM", @"SOFTWARE\Microsoft\Windows\CurrentVersion\CapabilityAccessManager\ConsentStore\location", "Value", 0);
                    SetServiceState("lfsvc", "disabled", true);
                    report("Localização desativada!", 100);
                },
                (report) =>
                {
                    report("Reativando localização...", 50);
                    SetServiceState("lfsvc", "demand", false);
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
                "Substitui o DNS lento da sua operadora pelo DNS mais rápido do mundo (Cloudflare 1.1.1.1), reduzindo tempo de resposta de sites e jogos.",
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

            list.Add(new OptimizationItem(
                "net_dns_cache_ttl",
                "Otimizar Cache DNS para Resposta Ultrarrápida",
                "Aumenta o tempo que o Windows guarda endereços de sites já visitados, evitando consultas lentas repetidas à internet.",
                "Rede",
                SafetyLevel.Recommended,
                true,
                true,
                (report) =>
                {
                    report("Ajustando TTL do cache de DNS no registro...", 50);
                    string key = @"SYSTEM\CurrentControlSet\Services\Dnscache\Parameters";
                    SetRegDword("HKLM", key, "MaxCacheTtl", 86400);
                    SetRegDword("HKLM", key, "MaxNegativeCacheTtl", 5);
                    report("Cache de DNS otimizado!", 100);
                },
                (report) =>
                {
                    report("Restaurando cache DNS...", 50);
                    string key = @"SYSTEM\CurrentControlSet\Services\Dnscache\Parameters";
                    DeleteRegValue("HKLM", key, "MaxCacheTtl");
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
                "Desativa os serviços de telemetria e sincronização em segundo plano da Xbox para quem joga na Steam, Epic, etc.",
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
                "Define KeyboardDelay como 0 e KeyboardSpeed no máximo (31), permitindo comandos instantâneos em jogos competitivos.",
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
                "Remove a aceleração artificial do ponteiro do Windows, garantindo mira 100% linear e precisa em jogos de tiro.",
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
                    report("Aceleração do mouse desativada (Mira 1:1)!", 100);
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
                "Ativa o HwSchMode = 2, permitindo que a placa de vídeo gerencie sua própria VRAM para taxas de quadros mais estáveis.",
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
                "Força os drivers e o kernel a ficarem na memória RAM rápida em vez de irem para o arquivo de paginação no disco.",
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

            list.Add(new OptimizationItem(
                "periph_gpu_preemption",
                "Otimizar Preemptividade de GPU (Menor Input Lag)",
                "Configura o agendador de GPU para responder imediatamente a comandos de renderização sem acumular filas.",
                "Periféricos",
                SafetyLevel.Recommended,
                true,
                true,
                (report) =>
                {
                    report("Ajustando GPU Preemption para mínima latência...", 50);
                    string key = @"SYSTEM\CurrentControlSet\Control\GraphicsDrivers\Scheduler";
                    SetRegDword("HKLM", key, "EnablePreemption", 1);
                    report("Preemptividade de GPU configurada!", 100);
                },
                (report) =>
                {
                    report("Restaurando padrão de GPU...", 50);
                    string key = @"SYSTEM\CurrentControlSet\Control\GraphicsDrivers\Scheduler";
                    DeleteRegValue("HKLM", key, "EnablePreemption");
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
                "Desativa DiagTrack e dmwappushservice permanentemente, economizando ciclos de processador em segundo plano.",
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
                "Desativa serviços legados da época do Windows XP que ninguém mais usa: Fax, RemoteRegistry, RetailDemo e CscService.",
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
                "Útil para computadores desktop e notebooks que não necessitam de rotação de tela nem sensor de luz ambiente.",
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
                "O SysMain indexa e pré-carrega programas em memória. Em SSDs modernos ele é desnecessário e só causa picos de uso de disco.",
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
                "Desativa o serviço de fila de impressão. Aumenta o desempenho de quem não possui impressora conectada ao PC.",
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
                "Para quem não usa a barra de pesquisa do Windows para ler conteúdos dentro de arquivos, economizando leitura contínua no disco.",
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

            list.Add(new OptimizationItem(
                "svc_diagnostic_policy",
                "Desativar Serviço de Políticas de Diagnóstico (DPS)",
                "Impede o serviço DPS de ficar executando testes de hardware e rede em segundo plano durante jogos.",
                "Serviços",
                SafetyLevel.Recommended,
                true,
                true,
                (report) =>
                {
                    report("Desativando DPS (Diagnostic Policy Service)...", 50);
                    SetServiceState("DPS", "disabled", true);
                    report("DPS desativado com sucesso!", 100);
                },
                (report) =>
                {
                    report("Reativando DPS...", 50);
                    SetServiceState("DPS", "auto", false);
                    report("DPS restaurado.", 100);
                }
            ));

            #endregion

            AddResearchedOptimizations(list);
            return list;
        }

        #endregion

    }
}
