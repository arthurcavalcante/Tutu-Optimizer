using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Microsoft.Win32;
using System.Net;
using System.Net.NetworkInformation;

namespace TutusOptimizer
{
    public partial class MainForm
    {
        private Panel CreateMaintenanceTab()
        {
            ModernScrollPanel scrollPanel = new ModernScrollPanel(theme);
            scrollPanel.Dock = DockStyle.Fill;
            Panel container = scrollPanel.Content;

            int y = 0;

            Panel banner = MakePageBanner("🧹 Limpeza Profunda & Manutenção do Sistema",
                "Ferramentas completas de manutenção preventiva para limpar gigabytes de arquivos temporários, reparar integridade e purgar memória.");
            banner.Location = new Point(0, y);
            banner.Width = container.Width;
            banner.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            container.Controls.Add(banner);
            y += banner.Height + 10;

            Panel actionsGrid = MakeActionGrid(new ActionCardDef[] {
                new ActionCardDef(
                    "🧹 Limpar Arquivos Temporários (%TEMP%)",
                    "Remove pastas temporárias de usuários, Windows, cache Prefetch e relatórios de erro.",
                    "Executar Limpeza", theme.AccentAmber,
                    (s, e) => RunStandalone("Limpeza de Temporários", (log) => TweakEngine.CleanAllTemporaryFiles(log))
                ),
                new ActionCardDef(
                    "🖼️ Resetar Cache de Miniaturas e Ícones",
                    "Corrige ícones corrompidos e invisíveis e compacta a base IconCache.db do Explorer.",
                    "Resetar Cache", theme.Accent,
                    (s, e) => RunStandalone("Reset de Ícones", (log) => TweakEngine.ResetIconAndThumbCache(log))
                ),
                new ActionCardDef(
                    "🌐 Limpar Cache DNS e Rede (Winsock)",
                    "Redefine o cache de rede e resolve problemas de lentidão, perda de pacotes e DNS corrompido.",
                    "Limpar Rede", theme.AccentCyan,
                    (s, e) => RunStandalone("Reset de Rede", (log) => TweakEngine.FlushDnsAndWinsock(log))
                ),
                new ActionCardDef(
                    "🧠 Purgar Memória RAM",
                    "Esvazia a memória em espera e working sets dos programas sem reiniciar o computador.",
                    "Purgar RAM", theme.AccentGreen,
                    (s, e) => RunStandalone("Limpeza de RAM", (log) => TweakEngine.EmptyRamMemory(log))
                ),
                new ActionCardDef(
                    "🛠️ Reparar Arquivos do Sistema (SFC / DISM)",
                    "Varredura completa por arquivos corrompidos no Windows e reparo automatizado via imagem oficial.",
                    "Iniciar Reparo", theme.AccentAmber,
                    (s, e) => RunStandalone("Reparo de Arquivos", (log) => TweakEngine.RunSystemFileCheck(log))
                ),
                new ActionCardDef(
                    "🗑️ Remover Bloatwares do Windows (UWP)",
                    "Desinstala com segurança aplicativos inúteis pré-instalados pela Microsoft (Cortana, Mapas, Dicas, etc.).",
                    "Remover Bloatware", theme.AccentRed,
                    (s, e) => RunStandalone("Remoção de Bloatwares", (log) => TweakEngine.RemoveBloatwareApps(log))
                )
            });
            actionsGrid.Location = new Point(0, y);
            actionsGrid.Width = container.Width;
            actionsGrid.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            container.Controls.Add(actionsGrid);
            y += actionsGrid.Height + 16;

            scrollPanel.SetContentHeight(y + 20);
            ResponsiveLayout.Stack(scrollPanel);
            scrollPanel.HookWheelRecursive(container);
            return scrollPanel;
        }

        // ─────────────────────────────────────────────────────────────────────
        // ABA 9 — CONSOLE DE LOGS
        // ─────────────────────────────────────────────────────────────────────
    }
}
