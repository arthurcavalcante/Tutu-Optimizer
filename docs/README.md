# Tutu's Optimizer

O aplicativo principal está em **`Tutu's Optimizer.exe`**, na raiz. Abra esse arquivo para usar o otimizador; o manifesto solicita os privilégios administrativos necessários às ações do Windows.

## Organização

```text
Tutu's Optimizer/
├── Tutu's Optimizer.exe    ← aplicativo principal
├── src/
│   ├── App/                ← inicialização, janela, estado e execução
│   ├── Models/             ← modelos e preferências
│   ├── Validation/         ← validação de entradas
│   ├── Services/
│   │   ├── Diagnostics/    ← benchmark e estresse
│   │   ├── Storage/        ← associação dos discos e saúde
│   │   ├── Settings/       ← persistência das preferências
│   │   └── Windows/        ← catálogo, ações e monitoramento
│   └── UI/
│       ├── Pages/          ← páginas do aplicativo
│       ├── Navigation/     ← barra lateral
│       ├── Controls/       ← controles e componentes reutilizáveis
│       ├── Theming/        ← paletas de cores
│       └── Animations/     ← transições, interpolação e efeitos
├── assets/                 ← ícone e manifesto
├── scripts/                ← compilação
├── tests/                  ← verificações e capturas
├── docs/                   ← documentação
└── build/
    ├── bin/                ← saída da compilação
    └── archive/            ← executáveis das versões anteriores
```

As classes parciais mantêm a integração da janela, com cada página em seu próprio arquivo. O executável pode ser aberto diretamente na raiz, sem depender do diretório de trabalho para encontrar o ícone.

## Fluidez e animações

- Troca de página com sobreposição gradual e deslocamento leve em 180 ms.
- Botões com transição de cor ao passar o mouse e pressionar, em 120 ms.
- Rolagem suave em 140 ms, acumulando os movimentos rápidos da roda do mouse.
- Medidores de CPU, RAM e carga com atualização interpolada em 220 ms.
- Monitor atualizado em segundo plano, sem iniciar consultas simultâneas.

Em **Personalização**, desmarque **Animações suaves** para usar mudanças imediatas. A preferência é salva em `%LOCALAPPDATA%\TutusOptimizer\settings.xml`. As animações também respeitam a [preferência de animação da área do cliente do Windows](https://learn.microsoft.com/en-us/windows/win32/winauto/client-area-animation); ficam desativadas em alto contraste e sessões remotas. Os timers só funcionam durante cada efeito e são liberados junto com os controles. Redimensionar, reconstruir o tema ou trocar rapidamente de página encerra a transição anterior.

## Layout responsivo

A janela pode ser reduzida até 760 × 560. Arraste a barra superior para mover; arraste as bordas ou cantos para redimensionar. O botão quadrado e o duplo clique na barra superior maximizam/restauram. Arrastar uma janela maximizada restaura seu tamanho e acompanha o ponteiro. A maximização respeita a barra de tarefas.

Abaixo de 1000 pixels, a barra lateral usa ícones com dicas. Em janelas maiores, os nomes e grupos reaparecem. Os cartões, textos, barras de ações e rolagem se adaptam à largura, preservando a página e as seleções.

O catálogo ganhou dez opções, incluindo duas políticas reversíveis do Edge e oito ajustes guiados nas configurações do Windows. Veja [as opções e as fontes da pesquisa](OPTIMIZATIONS.md).

## Recursos

- **Discos e saúde:** volumes associados aos discos pelo número informado pelo Windows, com estado geral, modelo, conexão, espaço usado/livre e consulta de temperatura, desgaste e contadores disponíveis. A vida útil estimada restante usa `100 − desgaste consumido`; campos ausentes ficam como N/D, sem converter o estado “Saudável” em 100%. Os [contadores de confiabilidade](https://learn.microsoft.com/en-us/windows-hardware/drivers/storage/msft-storagereliabilitycounter) dependem do suporte e do acesso ao driver.
- **Benchmark e estresse:** testes próprios de CPU/RAM e SSD/HDD, seleção do volume, arquivos de 64/256/1024 MB, leitura/gravação sequencial e leitura aleatória de 4 KiB em QD1. O cache do Windows/dispositivo influencia as taxas. O arquivo temporário é exclusivo e removido ao concluir, cancelar ou falhar.
- **Estresse:** perfis leve, equilibrado, intenso e personalizado; duração de 5 a 1800 segundos, até 64 threads limitadas pela máquina, carga de 10 a 100% por thread e memória até 1024 MB, limitada a 25% da RAM livre. Não testa GPU nem controla a temperatura automaticamente.
- **Personalização:** temas claro/escuro, seis cores, animações, intervalo do monitor, restauração automática e minimizar para a bandeja. Trocar o tema preserva a cor selecionada.

## Compilar e verificar

Na raiz do projeto:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\build.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\build.ps1 -CheckOnly
powershell -NoProfile -ExecutionPolicy Bypass -File .\tests\run.ps1
```

O script encontra os arquivos `.cs` recursivamente em `src`, usa o compilador do .NET Framework e gera `build/bin/TutusOptimizer.exe`. A compilação normal atualiza **`Tutu's Optimizer.exe`** na raiz. Se o aplicativo estiver aberto e impedir a substituição, a nova compilação permanece em `build/bin`; feche o aplicativo e execute o script novamente. `-CheckOnly` compila em um arquivo temporário sem substituir o aplicativo.

Os testes verificam associação dos volumes, percentuais de desgaste, benchmark, cancelamento, limpeza após falha, layout nas resoluções 920×600 e 1160×750 nos dois temas e encerramento dos diagnósticos. Também cobrem transições, navegação rápida, redimensionamento, animações desativadas, rolagem acumulada e liberação dos efeitos ao reconstruir o layout. Não aplicam otimizações nem alteram as preferências salvas. As capturas ficam em `tests/artifacts`.
