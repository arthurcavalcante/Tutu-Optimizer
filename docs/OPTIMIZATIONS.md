# Novas opções e pesquisa

Foram acrescentadas dez opções ao catálogo. Todas começam desmarcadas. As opções **GUIADO** têm um botão **Abrir ajuste** e não entram nos comandos Aplicar/Reverter ou Marcar Todos. A disponibilidade das páginas depende da versão do Windows e do hardware.

| Categoria | Opção | Comportamento |
| --- | --- | --- |
| Sistema | Pré-carregamento do Edge | Desativa `StartupBoostEnabled` para o usuário atual. |
| Sistema | Edge em segundo plano | Desativa `BackgroundModeEnabled` para o usuário atual. |
| Sistema | Programas de inicialização | Abre a lista de inicialização para revisão individual. |
| Sistema | Sensor de Armazenamento | Abre as regras de limpeza para revisão antes de ativar. |
| Sistema | Aplicativos em segundo plano | Abre aplicativos instalados para revisar permissões compatíveis. |
| Sistema | Drivers opcionais | Abre atualizações opcionais, sem instalar automaticamente. |
| Periféricos | Jogos em janela | Abre gráficos padrão para configurar o recurso do Windows 11. |
| Periféricos | GPU por jogo | Abre preferências de gráficos por executável. |
| Periféricos | Taxa de atualização | Abre Vídeo para conferir as opções avançadas do monitor. |
| Rede | Banda para atualizações | Abre os limites avançados da Otimização de Entrega. |

As duas políticas do Edge salvam o valor anterior, o tipo e a ausência do valor em `HKCU\Software\TutusOptimizer\OriginalEdgePolicies`. Aplicar várias vezes preserva o primeiro backup; Reverter restaura esse estado. Falhas de acesso são propagadas ao log. Reinicie o Edge para conferir o resultado. Uma política de organização pode prevalecer sobre a do usuário.

Os benefícios dependem dos recursos disponíveis, dos programas em execução e do jogo. Não há garantia de aumento de FPS. Os novos itens não desativam proteção de segurança nem instalam drivers automaticamente.

## Fontes

A pesquisa no YouTube usou títulos e descrições indexados dos vídeos abaixo; não houve acesso à transcrição completa. Eles citam inicialização, Game Mode e ajustes gráficos. Game Mode, HAGS e captura em segundo plano já existiam no catálogo e não foram duplicados.

- [How To Optimize Windows 11 For GAMING — Best Settings for HIGH FPS & NO DELAY](https://www.youtube.com/watch?v=n8BgHzC4RUQ)
- [How To OPTIMIZE Windows 11 For Gaming in 2026](https://www.youtube.com/watch?v=c-EiQOplgUE)

Os ajustes acrescentados foram conferidos nas fontes oficiais:

- [Microsoft: melhorar o desempenho do PC](https://support.microsoft.com/en-us/windows/experience/performance-optimization/tips-to-improve-pc-performance-in-windows)
- [Microsoft: otimizações para jogos em janela](https://support.microsoft.com/pt-br/windows/hardware/display-graphics/optimizations-for-windowed-games-in-windows-11)
- [Microsoft: política StartupBoostEnabled](https://learn.microsoft.com/en-us/deployedge/microsoft-edge-policies/startupboostenabled)
- [Microsoft: política BackgroundModeEnabled](https://learn.microsoft.com/en-us/deployedge/microsoft-edge-policies/backgroundmodeenabled)
- [Microsoft: endereços das configurações do Windows](https://learn.microsoft.com/en-us/windows/apps/develop/launch/launch-settings)

## Janela

Arraste a barra superior para mover a janela. Arraste as bordas ou os quatro cantos para redimensionar, até 760 × 560. Dê dois cliques na barra superior ou use o botão quadrado para maximizar/restaurar. Ao arrastar uma janela maximizada, ela volta ao tamanho anterior e acompanha o ponteiro. O estilo nativo permite o encaixe do Windows; a maximização respeita a área útil do monitor e a barra de tarefas.

Os testes de janela/catálogo podem ser executados com `powershell -NoProfile -ExecutionPolicy Bypass -File tests/run.ps1 -WindowCatalogOnly`. A reversão é testada em chaves temporárias exclusivas e removidas ao terminar; as políticas reais do Edge não são alteradas.

A moldura nativa não é desenhada sobre a barra personalizada quando uma mensagem de conclusão desativa e reativa a janela. A área do cliente é preservada nas duas formas de `WM_NCCALCSIZE`, e a ativação não repinta a moldura. Os testes abrem e fecham mensagens repetidamente, também com a janela maximizada, verificando a barra superior e a geometria. As capturas da janela ficam em `tests/artifacts/completion-dialog.png` e `tests/artifacts/after-completion-dialog.png`.
