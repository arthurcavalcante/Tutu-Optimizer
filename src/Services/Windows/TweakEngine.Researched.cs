using System;
using System.Collections.Generic;
using Microsoft.Win32;

namespace TutusOptimizer
{
    public static partial class TweakEngine
    {
        private const string EdgePolicy = @"Software\Policies\Microsoft\Edge";
        private const string PolicyBackup = @"Software\TutusOptimizer\OriginalEdgePolicies";

        private static void AddResearchedOptimizations(List<OptimizationItem> list)
        {
            list.Add(EdgeOption("sys_edge_startup", "Desativar pré-carregamento do Edge", "StartupBoostEnabled",
                "Evita que o Edge pré-carregue processos ao entrar no Windows. Pode liberar recursos quando você usa outro navegador.",
                "Opcional. O Edge pode demorar mais para abrir. Reverter restaura a política anterior; reinicie o navegador."));
            list.Add(EdgeOption("sys_edge_background", "Encerrar aplicativos do Edge em segundo plano", "BackgroundModeEnabled",
                "Impede extensões e aplicativos do Edge de continuar executando após fechar o navegador.",
                "Opcional. Extensões deixam de receber notificações com o Edge fechado. Reverter restaura a política anterior."));
            list.Add(SettingsOption("guide_startup", "Revisar programas de inicialização", "Sistema", "ms-settings:startupapps",
                "Desative somente programas que você não precisa ao ligar o PC. Revise o impacto de inicialização de cada aplicativo.", "Ajuste manual. Preserve antivírus e aplicativos necessários ao seu trabalho."));
            list.Add(SettingsOption("guide_windowed", "Otimizações para jogos em janela (Windows 11)", "Periféricos", "ms-settings:display-advancedgraphics-default",
                "Ative otimizações para jogos em janela em jogos DirectX 10/11 compatíveis. Reinicie o jogo e compare a latência.", "Depende da versão do Windows e do jogo. Pode ser desativado por aplicativo."));
            list.Add(SettingsOption("guide_gpu", "Escolher GPU de alto desempenho por jogo", "Periféricos", "ms-settings:display-advancedgraphics",
                "Adicione o executável do jogo em Gráficos e escolha Alto desempenho quando houver mais de uma GPU.", "Ajuste manual. Pode aumentar consumo e temperatura; reinicie o jogo."));
            list.Add(SettingsOption("guide_refresh", "Conferir a taxa de atualização do monitor", "Periféricos", "ms-settings:display-advanced",
                "Abra Vídeo avançado e escolha uma taxa de atualização suportada pelo monitor para tornar os movimentos mais fluidos.", "Não aumenta o FPS renderizado. As opções dependem do monitor, cabo e GPU."));
            list.Add(SettingsOption("guide_storage", "Configurar Sensor de Armazenamento", "Sistema", "ms-settings:storagepolicies",
                "Programe a limpeza de arquivos temporários e revise as regras de Downloads e Lixeira antes de ativar.", "Arquivos removidos podem não ser recuperáveis. Revise os prazos de limpeza."));
            list.Add(SettingsOption("guide_background", "Revisar aplicativos e permissões de fundo", "Sistema", "ms-settings:appsfeatures",
                "Abra as opções avançadas dos aplicativos compatíveis e limite a execução em segundo plano dos que você não usa.", "Nem todo aplicativo oferece essa opção. Pode interromper sincronização e notificações."));
            list.Add(SettingsOption("guide_delivery", "Limitar tráfego de atualizações em segundo plano", "Rede", "ms-settings:delivery-optimization-advanced",
                "Defina limites de download e upload da Otimização de Entrega para reduzir disputa de banda durante jogos.", "Ajuste manual. Limites baixos tornam atualizações mais lentas; não desativa o Windows Update."));
            list.Add(SettingsOption("guide_drivers", "Revisar atualizações opcionais de drivers", "Sistema", "ms-settings:windowsupdate-optionalupdates",
                "Confira drivers disponíveis e instale somente os relevantes para seu hardware ou para corrigir um problema conhecido.", "Não instala nada automaticamente. Alguns drivers exigem reinicialização."));
        }

        private static OptimizationItem SettingsOption(string id, string title, string category, string uri, string explanation, string risk)
        {
            return new OptimizationItem(id, title, explanation, category, SafetyLevel.Optional, false, false,
                null, null, explanation, risk) { SettingsUri = uri };
        }

        private static OptimizationItem EdgeOption(string id, string title, string valueName, string explanation, string risk)
        {
            return new OptimizationItem(id, title, explanation, "Sistema", SafetyLevel.Optional, false, true,
                report => {
                    ChangeEdgePolicy(valueName, false);
                    report("Política aplicada. Feche e reabra o Edge para conferir o efeito.", 100);
                },
                report => {
                    ChangeEdgePolicy(valueName, true);
                    report("Política anterior restaurada (se havia backup). Reinicie o Edge.", 100);
                }, explanation, risk);
        }

        // Keep the first value across repeated applications, including its registry type.
        internal static void ChangeEdgePolicy(string valueName, bool revert)
        {
            if (valueName != "StartupBoostEnabled" && valueName != "BackgroundModeEnabled")
                throw new ArgumentException("Política inválida.", "valueName");
            using (RegistryKey backup = Registry.CurrentUser.CreateSubKey(PolicyBackup + "\\" + valueName))
            using (RegistryKey target = Registry.CurrentUser.CreateSubKey(EdgePolicy))
                SetBackedUpDword(target, backup, valueName, revert);
        }

        internal static void SetBackedUpDword(RegistryKey target, RegistryKey backup, string valueName, bool revert)
        {
            if (revert && backup.GetValue("Saved") == null) return;
            if (!revert)
            {
                if (backup.GetValue("Saved") == null)
                {
                    object original = target.GetValue(valueName, null, RegistryValueOptions.DoNotExpandEnvironmentNames);
                    if (original != null)
                    {
                        backup.SetValue("Value", original, target.GetValueKind(valueName));
                        backup.SetValue("Present", 1);
                    }
                    else backup.SetValue("Present", 0);
                    backup.SetValue("Saved", 1);
                    backup.Flush();
                }
                target.SetValue(valueName, 0, RegistryValueKind.DWord);
            }
            else
            {
                if (Convert.ToInt32(backup.GetValue("Present", 0)) == 1)
                    target.SetValue(valueName, backup.GetValue("Value", null, RegistryValueOptions.DoNotExpandEnvironmentNames), backup.GetValueKind("Value"));
                else target.DeleteValue(valueName, false);
                target.Flush();
                backup.DeleteValue("Saved", false);
            }
        }
    }
}
