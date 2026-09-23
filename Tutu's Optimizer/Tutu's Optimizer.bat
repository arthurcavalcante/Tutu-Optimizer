@echo off
setlocal enabledelayedexpansion
title Tutu's Windows Optimizer
chcp 65001 >nul

:: --- VERIFICAÇÃO DE ADMINISTRADOR ---
net session >nul 2>&1
if %errorLevel% neq 0 (
    echo.
    echo [ERRO] VOCÊ PRECISA EXECUTAR O OTIMIZADOR NO MODO ADMINISTRADOR!
    echo Clique com o botao direito no arquivo e selecione "Executar como administrador".
    echo.
    pause
    exit /b
)

:loading
cls
echo.
echo  ╔══════════════════════════════════════════════════════╗
echo  ║                TUTU'S OPTIMIZER 2026                 ║
echo  ╚══════════════════════════════════════════════════════╝
echo.
echo   Inicializando modulos do sistema...
echo.

set "bar="
for /L %%i in (1,1,25) do (
    set "bar=!bar!█"
    cls
    echo.
    echo  ╔══════════════════════════════════════════════════════╗
    echo  ║                TUTU'S OPTIMIZER 2026                 ║
    echo  ╚══════════════════════════════════════════════════════╝
    echo.
    set "progresso=!bar!"
    set "espacos="
    for /L %%j in (%%i,1,24) do set "espacos=!espacos! "
    echo           Aguarde: [!progresso!!espacos!]
    echo.
    timeout /t 0 /nobreak >nul
)
:menu

:menuPrincipal
cls
echo.
echo  ╔═══════════════════════════════════════════════════════════════════╗
echo  ║                         TUTU'S OPTIMIZER 2026                     ║
echo  ╠═══════════════════════════════════════════════════════════════════╣
echo  ║                            MENU PRINCIPAL                         ║
echo  ╚═══════════════════════════════════════════════════════════════════╝
echo.
echo    [0] Otimização Personalizada
echo.
echo    [1] Criar Ponto de Restauracao       [2] Otimizacao do Windows
echo    [3] Otimizacao de Jogos              [4] Otimizacao de Perifericos
echo    [5] Config. Inicializacao Windows    [6] Liberar Memoria RAM
echo    [7] Melhorar Conexao/Ping            [8] Sair
echo.
echo  ╚═══════════════════════════════════════════════════════════════════╝
echo.
set /p opcao=" Escolha uma opção: "

if "%opcao%"=="0" goto otimizar-tudo-personalizado
if "%opcao%"=="1" goto pontorestauracao
if "%opcao%"=="2" goto menuwindows
if "%opcao%"=="3" goto prioridadegames
if "%opcao%"=="4" goto perifericos
if "%opcao%"=="5" goto autorun
if "%opcao%"=="6" goto limparram
if "%opcao%"=="7" goto internet-fix
if "%opcao%"=="8" goto sair

echo Opção inválida. Tente novamente.
pause
cls
goto menu
:menu

:: Otimizar Tudo Personalizado :: -------------------------------------------------------------- ::

:otimizar-tudo-personalizado
call :internet-fix
call :opcao1
call :opcao3
call :opcao4
call :opcao6
call :OTIMIZAR
call :OTIMIZAR_SERVICOS
call :opcao11
call :opcao14
call :opcao15
call :opcao17
call :opcao18
call :opcao19
call :opcao23
call :opcao28
call :opcao29
call :otimizarSSD
call :otimizarGPU
call :otimizarMouse
call :otimizarRAM
call :otimizarTeclado
goto menu

:: Otimizar Conexão/Ping :: -------------------------------------------------------------------- ::

:internet-fix

cls
echo.
echo  ╔═════════════════════════════════════════════════════════════════════════════╗
echo  ║                              OTIMIZACAO DE REDE                             ║
echo  ╚═════════════════════════════════════════════════════════════════════════════╝
echo.
echo     Status atual: [ Inicializando processos de rede... ]
echo.
echo     ┌────────────────────────────────────────────────────┐
echo     │ ░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░ │ 0%%
echo     └────────────────────────────────────────────────────┘
ping localhost -n 2 >nul

cls
echo.
echo  ╔═════════════════════════════════════════════════════════════════════════════╗
echo  ║                              OTIMIZACAO DE REDE                             ║
echo  ╚═════════════════════════════════════════════════════════════════════════════╝
echo.
echo     Status atual: [ Limpando Cache DNS, IP e Winsock... ]
echo.
echo     ┌────────────────────────────────────────────────────┐
echo     │ ████████░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░ │ 15%%
echo     └────────────────────────────────────────────────────┘
:: --- 1. LIMPEZA COMPLETA DE REDE E WINSOCK ---
ipconfig /flushdns >nul
ipconfig /release >nul
ipconfig /renew >nul
netsh winsock reset >nul
netsh int ip reset >nul
ping localhost -n 2 >nul

cls
echo.
echo  ╔═════════════════════════════════════════════════════════════════════════════╗
echo  ║                             OTIMIZACAO DE REDE                              ║
echo  ╚═════════════════════════════════════════════════════════════════════════════╝
echo.
echo     Status atual: [ Ajustando pilha TCP/IP e Servidores DNS ]
echo.
echo     ┌────────────────────────────────────────────────────┐
echo     │ ███████████████░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░ │ 30%%
echo     └────────────────────────────────────────────────────┘

:: --- 2. OTIMIZACAO TCP/IP ---
netsh interface tcp set global autotuninglevel=disabled >nul 2>&1
netsh interface tcp set heuristics disabled >nul 2>&1
netsh int tcp set global rss=enabled >nul 2>&1
netsh int tcp set global chimney=disabled >nul 2>&1

:: --- 3. CONFIGURANDO DNS EM TODAS AS INTERFACES ATIVAS ---

for /f "tokens=3*" %%a in ('netsh interface show interface ^| findstr /C:"Connected" /C:"Conectado"') do (
    
    :: Configura IPv4 (Principal e Secundario)
    netsh interface ipv4 set dnsservers name="%%b" static 76.76.2.4 primary validate=no >nul 2>&1
    netsh interface ipv4 add dnsservers name="%%b" 1.1.1.3 index=2 validate=no >nul 2>&1

    :: Configura IPv6 (Principal e Secundario)
    netsh interface ipv6 set dnsservers name="%%b" static 2606:4700:4700::1113 primary validate=no >nul 2>&1
    netsh interface ipv6 add dnsservers name="%%b" 2606:1a40::4 index=2 validate=no >nul 2>&1
)

:: Limpa o cache para aplicar imediatamente
ipconfig /flushdns >nul 2>&1

cls
echo.
echo  ╔═════════════════════════════════════════════════════════════════════════════╗
echo  ║                              OTIMIZACAO DE REDE                             ║
echo  ╚═════════════════════════════════════════════════════════════════════════════╝
echo.
echo     Status atual: [ Desativando Limitador de Rede e QoS... ]
echo.
echo     ┌────────────────────────────────────────────────────┐
echo     │ █████████████████████████░░░░░░░░░░░░░░░░░░░░░░░░░ │ 50%%
echo     └────────────────────────────────────────────────────┘
:: --- 4. NETWORK THROTTLING E QOS ---
reg add "HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile" /v NetworkThrottlingIndex /t REG_DWORD /d 0xffffffff /f >nul
reg add "HKLM\SOFTWARE\Policies\Microsoft\Windows\Psched" /v NonBestEffortLimit /t REG_DWORD /d 0 /f >nul
ping localhost -n 2 >nul

cls
echo.
echo  ╔═════════════════════════════════════════════════════════════════════════════╗
echo  ║                              OTIMIZACAO DE REDE                             ║
echo  ╚═════════════════════════════════════════════════════════════════════════════╝
echo.
echo     Status atual: [ Aplicando Tweaks de Baixa Latencia no Registro... ]
echo.
echo     ┌────────────────────────────────────────────────────┐
echo     │ ████████████████████████████████░░░░░░░░░░░░░░░░░░ │ 65%%
echo     └────────────────────────────────────────────────────┘
:: --- 5. OTIMIZACOES AVANCADAS DE REGISTRO ---
reg add "HKLM\SYSTEM\CurrentControlSet\Services\Tcpip\Parameters" /v TCPNoDelay /t REG_DWORD /d 1 /f >nul
reg add "HKLM\SYSTEM\CurrentControlSet\Services\Tcpip\Parameters" /v TcpAckFrequency /t REG_DWORD /d 1 /f >nul
reg add "HKLM\SYSTEM\CurrentControlSet\Services\Tcpip\Parameters" /v FastSendDatagramThreshold /t REG_DWORD /d 64000 /f >nul
reg add "HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile\Tasks\Games" /v "GPU Priority" /t REG_DWORD /d 8 /f >nul
reg add "HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile\Tasks\Games" /v "Priority" /t REG_DWORD /d 6 /f >nul
reg add "HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile\Tasks\Games" /v "Scheduling Category" /t REG_SZ /d "High" /f >nul
ping localhost -n 2 >nul

cls
echo.
echo  ╔═════════════════════════════════════════════════════════════════════════════╗
echo  ║                              OTIMIZACAO DE REDE                             ║
echo  ╚═════════════════════════════════════════════════════════════════════════════╝
echo.
echo     Status atual: [ Otimizando Servicos... ]
echo.
echo     ┌────────────────────────────────────────────────────┐
echo     │ ████████████████████████████████████████░░░░░░░░░░ │ 80%%
echo     └────────────────────────────────────────────────────┘
:: --- 6. DESABILITAR IPv6 E 8. SERVICOS DE REDE ---
reg add "HKLM\SYSTEM\CurrentControlSet\Services\Tcpip6\Parameters" /v DisabledComponents /t REG_DWORD /d 255 /f >nul
sc stop DoSvc >nul 2>&1
sc config DoSvc start= disabled >nul 2>&1
sc stop lmhosts >nul 2>&1
sc config lmhosts start= disabled >nul 2>&1
ping localhost -n 2 >nul

cls
echo.
echo  ╔═════════════════════════════════════════════════════════════════════════════╗
echo  ║                              OTIMIZACAO DE REDE                             ║
echo  ╚═════════════════════════════════════════════════════════════════════════════╝
echo.
echo     Status atual: [ Desativando Economia de Energia na Rede... ]
echo.
echo     ┌────────────────────────────────────────────────────┐
echo     │ ████████████████████████████████████████████████░░ │ 95%%
echo     └────────────────────────────────────────────────────┘
:: --- 7. GERENCIAMENTO DE ENERGIA DA REDE ---
powershell -Command "Disable-NetAdapterPowerManagement -Name '*' -ErrorAction SilentlyContinue" >nul
ping localhost -n 2 >nul

cls
echo.
echo  ╔═════════════════════════════════════════════════════════════════════════════╗
echo  ║                              OTIMIZACAO DE REDE                             ║
echo  ╚═════════════════════════════════════════════════════════════════════════════╝
echo.
echo     Status atual: [ Otimizacao de rede concluida com sucesso! ]
echo.
echo     ┌────────────────────────────────────────────────────┐
echo     │ ██████████████████████████████████████████████████ │ 100%%
echo     └────────────────────────────────────────────────────┘
echo.
pause
cls
goto :menu

:: Sair do programa :: -------------------------------------------------------------------- ::

:sair
Echo Saindo do programa...
echo 1
echo 2
echo 3
exit

:: Liberar memória RAM :: -------------------------------------------------------------------- ::

:limparram
:: --- 0% --- Preparação
cls
echo.
echo  ╔═════════════════════════════════════════════════════════════════════════════╗
echo  ║                          OTIMIZACAO DE MEMORIA RAM                          ║
echo  ╚═════════════════════════════════════════════════════════════════════════════╝
echo.
echo    Status atual: [ Preparando processos de otimizacao para Windows 11... ]
echo.
echo    ┌──────────────────────────────────────────────────┐
echo    │ ░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░ │ 0%%
echo    └──────────────────────────────────────────────────┘
ping localhost -n 2 >nul

:: --- 25% --- EmptyStandbyList
cls
echo.
echo  ╔═════════════════════════════════════════════════════════════════════════════╗
echo  ║                          OTIMIZACAO DE MEMORIA RAM                          ║
echo  ╚═════════════════════════════════════════════════════════════════════════════╝
echo.
echo    Status atual: [ Limpando cache de memoria (EmptyStandbyList)... ]
echo.
echo    ┌──────────────────────────────────────────────────┐
echo    │ █████████████░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░ │ 25%%
echo    └──────────────────────────────────────────────────┘
set "emptyStandbyList=%~dp0EmptyStandbyList.exe"
if not exist "%emptyStandbyList%" (
    echo.
    echo    [!] ERRO: O arquivo EmptyStandbyList.exe nao foi encontrado.
    ping localhost -n 3 >nul
) else (
    "%emptyStandbyList%" workingsets >nul 2>&1
    "%emptyStandbyList%" modifiedpagelist >nul 2>&1
    "%emptyStandbyList%" standbylist >nul 2>&1
)

:: --- 50% --- Windows Explorer
cls
echo.
echo  ╔═════════════════════════════════════════════════════════════════════════════╗
echo  ║                          OTIMIZACAO DE MEMORIA RAM                          ║
echo  ╚═════════════════════════════════════════════════════════════════════════════╝
echo.
echo    Status atual: [ Reiniciando Windows Explorer para liberar RAM retida... ]
echo.
echo    ┌──────────────────────────────────────────────────┐
echo    │ █████████████████████████░░░░░░░░░░░░░░░░░░░░░░░░░ │ 50%%
echo    └──────────────────────────────────────────────────┘
taskkill /f /im explorer.exe >nul 2>&1
start explorer.exe >nul 2>&1

:: --- 75% --- Clipboard
cls
echo.
echo  ╔═════════════════════════════════════════════════════════════════════════════╗
echo  ║                          OTIMIZACAO DE MEMORIA RAM                          ║
echo  ╚═════════════════════════════════════════════════════════════════════════════╝
echo.
echo    Status atual: [ Limpando historico da Area de Transferencia... ]
echo.
echo    ┌──────────────────────────────────────────────────┐
echo    │ ██████████████████████████████████████░░░░░░░░░░░░ │ 75%%
echo    └──────────────────────────────────────────────────┘
cmd /c "echo off | clip"

:: --- 100% --- SysMain (Superfetch) e Conclusão
cls
echo.
echo  ╔═════════════════════════════════════════════════════════════════════════════╗
echo  ║                          OTIMIZACAO DE MEMORIA RAM                          ║
echo  ╚═════════════════════════════════════════════════════════════════════════════╝
echo.
echo    Status atual: [ Interrompendo servico SysMain (Superfetch)... ]
echo.
echo    ┌──────────────────────────────────────────────────┐
echo    │ ██████████████████████████████████████████████████ │ 100%%
echo    └──────────────────────────────────────────────────┘
sc config "SysMain" start=disabled >nul 2>&1
net stop "SysMain" >nul 2>&1
ping localhost -n 2 >nul

:: --- TELA DE SUCESSO ---
cls
echo.
echo  ╔═════════════════════════════════════════════════════════════════════════════╗
echo  ║                     MEMORIA RAM OTIMIZADA COM SUCESSO!                      ║
echo  ╚═════════════════════════════════════════════════════════════════════════════╝
echo.
echo    [+] Resumo das alteracoes:
echo        - Cache de memoria em espera esvaziado.
echo        - Memory Leaks do Windows Explorer corrigidos.
echo        - Area de transferencia (Clipboard) higienizada.
echo        - Servico SysMain desativado para poupar RAM.
echo.
echo  ───────────────────────────────────────────────────────────────────────────────
echo    Pressione qualquer tecla para voltar ao menu principal...
pause >nul
cls
goto :menu

:: Criar ponto de restauração :: -------------------------------------------------------------------- ::

:pontorestauracao
cls
echo.
echo  ╔═════════════════════════════════════════════════════════════════════════════╗
echo  ║                  PONTO DE RESTAURACAO CRIADO COM SUCESSO!                   ║
echo  ╚═════════════════════════════════════════════════════════════════════════════╝
echo.
echo    [+] Criando ponto de protecao do sistema...

reg add "HKLM\Software\Microsoft\Windows NT\CurrentVersion\SystemRestore" /v SystemRestorePointCreationFrequency /t REG_DWORD /d 0 /f >nul
powershell -Command "Checkpoint-Computer -Description 'Game Booster RestorePoint' -RestorePointType 'MODIFY_SETTINGS'" >nul 2>&1

echo.
echo    [+] Resumo das alteracoes:
echo        - Limite de frequencia de criacao desativado no Registro.
echo        - Ponto de Restauracao 'Game Booster RestorePoint' gerado.
echo        - Estado atual do sistema salvo com seguranca.
echo.
echo  ───────────────────────────────────────────────────────────────────────────────
echo    Pressione qualquer tecla para voltar ao menu principal...
pause >nul
cls
goto :menu

:: Otimização de windows (Para jogos) :: -------------------------------------------------------------------- ::

:menuwindows
cls
echo.
echo  ╔═════════════════════════════════════════════════════════════════════════════╗
echo  ║                     FERRAMENTA DE OTIMIZACAO - WINDOWS 11                   ║
echo  ╚═════════════════════════════════════════════════════════════════════════════╝
echo.
echo    [1]  Otimizar Energia                 [2]  Desativar Efeitos Visuais
echo    [3]  Tweaks de Privacidade            [4]  Desativar Telemetria
echo    [5]  Desativar TOTALMENTE a Xbox      [6]  Desativar Relatorios de Erro
echo    [7]  Otimizar ALT+TAB                 [8]  Desativar Servicos Inuteis
echo    [9]  Desativar Hibernacao             [10] Otimizar Explorer
echo    [11] Desativar Indexacao Arquivos     [12] Debloater (Apps Desnecessarios)
echo    [13] Desativar Notificacoes           [14] Desativar Cortana
echo    [15] Bloquear Feedback Automatico     [16] Desativar SmartScreen
echo    [17] Desativar Overlays (Xbox)        [18] Resetar Cache de Miniaturas
echo    [19] Desativar Prefetch/Superfetch    [20] Fechar Explorer
echo    [21] Iniciar Explorer                 [22] Desativar UAC
echo    [23] Desativar Hyper-V (VM)           [24] Verificar/Reparar Arquivos
echo    [25] Limpar Cache de Rede             [26] Limpar Arquivos Temporarios
echo    [27] Desativar Anti-Malware           [28] Desativar Download Maps Manager
echo    [29] Desativar TimeStamp              [30] REINICIAR PC
echo    [31] Menu Principal
echo.
echo  ╚═════════════════════════════════════════════════════════════════════════════╝
echo.
set /p op= " Digite a opcao desejada: " 

if "%op%"==" " goto menuwindows
if "%op%"=="" goto menuwindows
if "%op%"=="1" goto opcao1
if "%op%"=="2" goto opcao2
if "%op%"=="3" goto opcao3
if "%op%"=="4" goto opcao4
if "%op%"=="5" goto opcao5
if "%op%"=="6" goto opcao6
if "%op%"=="7" goto opcao7
if "%op%"=="8" goto opcao8
if "%op%"=="9" goto opcao9
if "%op%"=="10" goto opcao10
if "%op%"=="11" goto opcao11
if "%op%"=="12" goto opcao12
if "%op%"=="13" goto opcao13
if "%op%"=="14" goto opcao14
if "%op%"=="15" goto opcao15
if "%op%"=="16" goto opcao16
if "%op%"=="17" goto opcao17
if "%op%"=="18" goto opcao18
if "%op%"=="19" goto opcao19
if "%op%"=="20" goto opcao20
if "%op%"=="21" goto opcao21
if "%op%"=="22" goto opcao22
if "%op%"=="23" goto opcao23
if "%op%"=="24" goto opcao24
if "%op%"=="25" goto opcao25
if "%op%"=="26" goto opcao26
if "%op%"=="27" goto opcao27
if "%op%"=="28" goto opcao28
if "%op%"=="29" goto opcao29
if "%op%"=="30" goto opcao30
if "%op%"=="31" goto menuPrincipal

:: Mensagem caso digitem uma opcao invalida

echo Opcao Invalida! Tente novamente.
ping localhost -n 2 >nul
goto menuwindows

:: Opção 1 - Otimizar a energia :: -------------------------------------------------------------------- ::


:opcao1
cls
echo.
echo  ╔═════════════════════════════════════════════════════════════════════════════╗
echo  ║                         OTIMIZANDO ENERGIA E SISTEMA                        ║
echo  ╚═════════════════════════════════════════════════════════════════════════════╝
echo.
echo    Status atual: [ Inicializando processos... ]
echo.
echo    ┌────────────────────────────────────────────────────┐
echo    │ ░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░ │ 0%%
echo    └────────────────────────────────────────────────────┘
:: --- Plano de Energia ---
powercfg -duplicatescheme e9a42b02-d5df-448d-aa00-03f14749eb61 >nul 2>&1
powercfg.exe /setacvalueindex SCHEME_CURRENT SUB_PROCESSOR IdleDisable 0 >nul 2>&1
powercfg.exe /setactive SCHEME_CURRENT >nul 2>&1
ping localhost -n 2 >nul

cls
echo.
echo  ╔═════════════════════════════════════════════════════════════════════════════╗
echo  ║                         OTIMIZANDO ENERGIA E SISTEMA                        ║
echo  ╚═════════════════════════════════════════════════════════════════════════════╝
echo.
echo    Status atual: [ Ajustando prioridades de GPU e Tarefas... ]
echo.
echo    ┌────────────────────────────────────────────────────┐
echo    │ ██████████░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░ │ 20%%
echo    └────────────────────────────────────────────────────┘
:: --- Otimizações de Registro (Multimedia & Tasks) ---
reg add "HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile\Tasks\Games" /v "GPU Priority" /t REG_DWORD /d 8 /f >nul
reg add "HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile\Tasks\Games" /v "Priority" /t REG_DWORD /d 6 /f >nul
reg add "HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile\Tasks\Games" /v "Scheduling Category" /t REG_SZ /d "High" /f >nul
reg add "HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile\Tasks\Games" /v "SFIO Priority" /t REG_SZ /d "High" /f >nul
ping localhost -n 2 >nul

cls
echo.
echo  ╔═════════════════════════════════════════════════════════════════════════════╗
echo  ║                         OTIMIZANDO ENERGIA E SISTEMA                        ║
echo  ╚═════════════════════════════════════════════════════════════════════════════╝
echo.
echo    Status atual: [ Melhorando resposta de Rede e Sistema... ]
echo.
echo    ┌────────────────────────────────────────────────────┐
echo    │ ████████████████████░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░ │ 40%%
echo    └────────────────────────────────────────────────────┘
:: --- Rede e Responsividade ---
reg add "HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile" /v "NetworkThrottlingIndex" /t REG_DWORD /d 4294967295 /f >nul
reg add "HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile" /v "SystemResponsiveness" /t REG_DWORD /d 3 /f >nul
ping localhost -n 2 >nul
:: 1. HKEY_CURRENT_USER\Control Panel\Desktop
reg add "HKCU\Control Panel\Desktop" /v "MenuShowDelay" /t REG_SZ /d "100" /f
reg add "HKCU\Control Panel\Desktop" /v "AutoEndTasks" /t REG_SZ /d "1" /f
:: 2. HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile\Tasks\Games
reg add "HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile\Tasks\Games" /v "Scheduling Category" /t REG_SZ /d "High" /f
reg add "HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile\Tasks\Games" /v "SFIO Priority" /t REG_SZ /d "High" /f
reg add "HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile\Tasks\Games" /v "Priority" /t REG_DWORD /d 6 /f
:: 3. HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management
reg add "HKLM\SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management" /v "FeatureSettings" /t REG_DWORD /d 1 /f
reg add "HKLM\SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management" /v "FeatureSettingsOverride" /t REG_DWORD /d 3 /f
reg add "HKLM\SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management" /v "FeaturesSettingsOverrideMask" /t REG_DWORD /d 3 /f
cls
echo.
echo  ╔═════════════════════════════════════════════════════════════════════════════╗
echo  ║                         OTIMIZANDO ENERGIA E SISTEMA                        ║
echo  ╚═════════════════════════════════════════════════════════════════════════════╝
echo.
echo    Status atual: [ Otimizando agendamento de GPU e Processos... ]
echo.
echo    ┌────────────────────────────────────────────────────┐
echo    │ ██████████████████████████████░░░░░░░░░░░░░░░░░░░░ │ 60%%
echo    └────────────────────────────────────────────────────┘
:: --- Prioridade do Processador e GPU Scheduler ---
reg add "HKLM\SYSTEM\CurrentControlSet\Control\PriorityControl" /v "Win32PrioritySeparation" /t REG_DWORD /d 22 /f >nul
reg add "HKLM\SYSTEM\CurrentControlSet\Control\Session Manager\Environment" /v "GPU_SCHEDULER_MODE" /t REG_SZ /d "47" /f >nul
reg add "HKLM\SYSTEM\CurrentControlSet\Control" /v "SvcHostSplitThresholdInKB" /t REG_DWORD /d 67108864 /f >nul
ping localhost -n 2 >nul
bcdedit /deletevalue numproc >nul 2>&1
ping localhost -n 2 >nul
bcdedit /deletevalue truncatemem >nul 2>&1
ping localhost -n 2 >nul

cls
echo.
echo  ╔═════════════════════════════════════════════════════════════════════════════╗
echo  ║                         OTIMIZANDO ENERGIA E SISTEMA                        ║
echo  ╚═════════════════════════════════════════════════════════════════════════════╝
echo.
echo    Status atual: [ Removendo atrasos de menus e acessibilidade... ]
echo.
echo    ┌────────────────────────────────────────────────────┐
echo    │ ████████████████████████████████████████░░░░░░░░░░ │ 80%%
echo    └────────────────────────────────────────────────────┘
:: --- Interface e Acessibilidade ---
reg add "HKCU\Control Panel\Desktop" /v "MenuShowDelay" /t REG_DWORD /d 3 /f >nul
reg add "HKCU\Control Panel\Accessibility\MouseKeys" /v "Flags" /t REG_SZ /d "0" /f >nul
reg add "HKCU\Control Panel\Accessibility\StickyKeys" /v "Flags" /t REG_SZ /d "0" /f >nul
reg add "HKCU\Control Panel\Accessibility\Keyboard Response" /v "Flags" /t REG_SZ /d "0" /f >nul
reg add "HKCU\Control Panel\Accessibility\ToggleKeys" /v "Flags" /t REG_SZ /d "0" /f >nul
ping localhost -n 2 >nul

cls
echo.
echo  ╔═════════════════════════════════════════════════════════════════════════════╗
echo  ║                         OTIMIZANDO ENERGIA E SISTEMA                        ║
echo  ╚═════════════════════════════════════════════════════════════════════════════╝
echo.
echo    Status atual: [ Aplicando Tweaks para Armazenamento SSD M.2 NVMe... ]
echo.
echo    ┌────────────────────────────────────────────────────┐
echo    │ ████████████████████████████████████████████████░░ │ 95%%
echo    └────────────────────────────────────────────────────┘
:: --- Melhoria do Armazenamento SSD M.2 NVME! ---
reg add HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Policies\Microsoft\FeatureManagement\Overrides /v 735209102 /t REG_DWORD /d 1 /f >nul
reg add HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Policies\Microsoft\FeatureManagement\Overrides /v 1853569164 /t REG_DWORD /d 1 /f >nul
reg add HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Policies\Microsoft\FeatureManagement\Overrides /v 156965516 /t REG_DWORD /d 1 /f >nul
ping localhost -n 2 >nul

cls
echo.
echo  ╔═════════════════════════════════════════════════════════════════════════════╗
echo  ║                         OTIMIZANDO ENERGIA E SISTEMA                        ║
echo  ╚═════════════════════════════════════════════════════════════════════════════╝
echo.
echo    Status atual: [ Otimizacao concluida com sucesso! ]
echo.
echo    ┌────────────────────────────────────────────────────┐
echo    │ ██████████████████████████████████████████████████ │ 100%%
echo    └────────────────────────────────────────────────────┘
echo.
echo    Abrindo painel de energia para conferencia...
timeout /t 2 >nul
powercfg.cpl
pause
goto menuwindows

:: Opção 2 - Desativar efeitos visuais :: -------------------------------------------------------------------- ::

:opcao2
echo.
echo  ╔═════════════════════════════════════════════════════════════════════════════╗
echo  ║                          DESATIVAR EFEITOS VISUAIS                          ║
echo  ╚═════════════════════════════════════════════════════════════════════════════╝
echo.
echo    Status atual: [ Inicializando configuracoes de desempenho... ]
echo.
echo    ┌────────────────────────────────────────────────────┐
echo    │ ░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░ │ 0%%
echo    └────────────────────────────────────────────────────┘
:: --- Desativa Efeitos Visuais de Janelas ---
reg add "HKCU\Software\Microsoft\Windows\CurrentVersion\Explorer\VisualEffects" /v VisualFXSetting /t REG_DWORD /d 2 /f >nul
ping localhost -n 2 >nul

cls
echo.
echo  ╔═════════════════════════════════════════════════════════════════════════════╗
echo  ║                          DESATIVAR EFEITOS VISUAIS                          ║
echo  ╚═════════════════════════════════════════════════════════════════════════════╝
echo.
echo    Status atual: [ Desativando transparencia do sistema... ]
echo.
echo    ┌────────────────────────────────────────────────────┐
echo    │ █████████████████████████░░░░░░░░░░░░░░░░░░░░░░░░░ │ 50%%
echo    └────────────────────────────────────────────────────┘
:: --- Desativa Transparência e Máscara de Preferências ---
reg add "HKCU\Software\Microsoft\Windows\CurrentVersion\Themes\Personalize" /v EnableTransparency /t REG_DWORD /d 0 /f >nul
reg add "HKCU\Control Panel\Desktop" /v UserPreferencesMask /t REG_BINARY /d 9012038010000000 /f >nul
reg add "HKCU\Control Panel\Desktop" /v VisualFXSetting /t REG_DWORD /d 2 /f >nul
ping localhost -n 2 >nul

cls
echo.
echo  ╔═════════════════════════════════════════════════════════════════════════════╗
echo  ║                          DESATIVAR EFEITOS VISUAIS                          ║
echo  ╚═════════════════════════════════════════════════════════════════════════════╝
echo.
echo    Status atual: [ Efeitos visuais desativados com sucesso! ]
echo.
echo    ┌────────────────────────────────────────────────────┐
echo    │ ██████████████████████████████████████████████████ │ 100%%
echo    └────────────────────────────────────────────────────┘
echo.
pause
goto menuwindows

:: Opção 3 - Tweaks de privacidade :: -------------------------------------------------------------------- ::

:opcao3
cls

echo.
echo  ╔═════════════════════════════════════════════════════════════════════════════╗
echo  ║                         APLICANDO TWEAKS DE PRIVACIDADE                     ║
echo  ╚═════════════════════════════════════════════════════════════════════════════╝
echo.
echo    Status atual: [ Aplicando politicas de coleta de dados... ]
echo.
echo    ┌────────────────────────────────────────────────────┐
echo    │ ░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░ │ 0%%
echo    └────────────────────────────────────────────────────┘
:: --- Bloqueio de Telemetria e Regras SIUF ---
reg add "HKLM\SOFTWARE\Policies\Microsoft\Windows\DataCollection" /v AllowTelemetry /t REG_DWORD /d 0 /f >nul
reg add "HKCU\Software\Microsoft\Siuf\Rules" /v NumberOfSIUFInPeriod /t REG_DWORD /d 0 /f >nul
reg add "HKCU\Software\Microsoft\Siuf\Rules" /v PeriodInNanoSeconds /t REG_QWORD /d 0 /f >nul
ping localhost -n 2 >nul

cls
echo.
echo  ╔═════════════════════════════════════════════════════════════════════════════╗
echo  ║                         APLICANDO TWEAKS DE PRIVACIDADE                     ║
echo  ╚═════════════════════════════════════════════════════════════════════════════╝
echo.
echo    Status atual: [ Desativando Agendamentos de Experiencia do Usuario... ]
echo.
echo    ┌────────────────────────────────────────────────────┐
echo    │ █████████████░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░ │ 25%%
echo    └────────────────────────────────────────────────────┘
:: --- Desativação de Tarefas Agendadas (CEIP) ---
schtasks /Change /TN "Microsoft\Windows\Customer Experience Improvement Program\Consolidator" /Disable >nul 2>&1
schtasks /Change /TN "Microsoft\Windows\Customer Experience Improvement Program\UsbCeip" /Disable >nul 2>&1
schtasks /Change /TN "Microsoft\Windows\Customer Experience Improvement Program\KernelCeipTask" /Disable >nul 2>&1
schtasks /Change /TN "Microsoft\Windows\Application Experience\ProgramDataUpdater" /Disable >nul 2>&1
ping localhost -n 2 >nul

cls
echo.
echo  ╔═════════════════════════════════════════════════════════════════════════════╗
echo  ║                         APLICANDO TWEAKS DE PRIVACIDADE                     ║
echo  ╚═════════════════════════════════════════════════════════════════════════════╝
echo.
echo    Status atual: [ Interrompendo servicos de diagnostico e rastreamento... ]
echo.
echo    ┌────────────────────────────────────────────────────┐
echo    │ █████████████████████████░░░░░░░░░░░░░░░░░░░░░░░░░ │ 50%%
echo    └────────────────────────────────────────────────────┘
:: --- Desativação e Parada de Serviços ---
sc config DiagTrack start= disabled >nul 2>&1
sc stop DiagTrack >nul 2>&1
sc config dmwappushservice start= disabled >nul 2>&1
sc stop dmwappushservice >nul 2>&1
ping localhost -n 2 >nul

cls
echo.
echo  ╔═════════════════════════════════════════════════════════════════════════════╗
echo  ║                         APLICANDO TWEAKS DE PRIVACIDADE                     ║
echo  ╚═════════════════════════════════════════════════════════════════════════════╝
echo.
echo    Status atual: [ Removendo sugestoes e recomendacoes do Windows... ]
echo.
echo    ┌────────────────────────────────────────────────────┐
echo    │ █████████████████████████████████████████░░░░░░░░░ │ 80%%
echo    └────────────────────────────────────────────────────┘
:: --- Remoção de Conteúdo Patrocinado e Recomendações do Menu Iniciar ---
reg add "HKCU\Software\Microsoft\Windows\CurrentVersion\ContentDeliveryManager" /v SubscribedContent-338387Enabled /t REG_DWORD /d 0 /f >nul
reg add "HKCU\Software\Microsoft\Windows\CurrentVersion\ContentDeliveryManager" /v SubscribedContent-353694Enabled /t REG_DWORD /d 0 /f >nul
reg add "HKCU\Software\Microsoft\Windows\CurrentVersion\ContentDeliveryManager" /v SubscribedContent-353696Enabled /t REG_DWORD /d 0 /f >nul
reg add "HKCU\Software\Microsoft\Windows\CurrentVersion\ContentDeliveryManager" /v SubscribedContent-338388Enabled /t REG_DWORD /d 0 /f >nul
reg add "HKCU\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced" /v Start_Recommendations /t REG_DWORD /d 0 /f >nul
ping localhost -n 2 >nul

cls
echo.
echo  ╔═════════════════════════════════════════════════════════════════════════════╗
echo  ║                         APLICANDO TWEAKS DE PRIVACIDADE                     ║
echo  ╚═════════════════════════════════════════════════════════════════════════════╝
echo.
echo    Status atual: [ Tweaks de privacidade aplicados com sucesso! ]
echo.
echo    ┌────────────────────────────────────────────────────┐
echo    │ ██████████████████████████████████████████████████ │ 100%%
echo    └────────────────────────────────────────────────────┘
echo.
pause
goto menuwindows

:: Opção 4 - Desativar telemetria :: -------------------------------------------------------------------- ::

:opcao4

cls
echo.
echo  ╔═════════════════════════════════════════════════════════════════════════════╗
echo  ║                     DESATIVAR TELEMETRIA E COLETA DE DADOS                  ║
echo  ╚═════════════════════════════════════════════════════════════════════════════╝
echo.
echo    Status atual: [ Aplicando politicas de Telemetria e Diagnosticos... ]
echo.
echo    ┌────────────────────────────────────────────────────┐
echo    │ ░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░ │ 0%%
echo    └────────────────────────────────────────────────────┘
:: --- Telemetria e Coleta de Dados Principal ---
REG ADD "HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\DataCollection" /v "AllowTelemetry" /t REG_DWORD /d 0 /f >nul 2>&1
REG ADD "HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\System" /v "AllowAppDataCollection" /t REG_DWORD /d 0 /f >nul 2>&1
ping localhost -n 2 >nul

cls
echo.
echo  ╔═════════════════════════════════════════════════════════════════════════════╗
echo  ║                     DESATIVAR TELEMETRIA E COLETA DE DADOS                  ║
echo  ╚═════════════════════════════════════════════════════════════════════════════╝
echo.
echo    Status atual: [ Bloqueando IDs de Anuncios e Experiencias de Consumo... ]
echo.
echo    ┌────────────────────────────────────────────────────┐
echo    │ █████████████░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░ │ 25%%
echo    └────────────────────────────────────────────────────┘
:: --- Propaganda, Experiência de Consumo e Localizações de Update ---
REG ADD "HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\AdvertisingInfo" /v "DisableWindowsAdvertising" /t REG_DWORD /d 1 /f >nul 2>&1
REG ADD "HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\CloudContent" /v "DisableMicrosoftConsumerExperience" /t REG_DWORD /d 1 /f >nul 2>&1
REG ADD "HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\WindowsUpdate" /v "DoNotConnectToWindowsUpdateInternetLocations" /t REG_DWORD /d 1 /f >nul 2>&1
ping localhost -n 2 >nul

cls
echo.
echo  ╔═════════════════════════════════════════════════════════════════════════════╗
echo  ║                     DESATIVAR TELEMETRIA E COLETA DE DADOS                  ║
echo  ╚═════════════════════════════════════════════════════════════════════════════╝
echo.
echo    Status atual: [ NOVO: Bloqueando Telemetria do Microsoft Edge... ]
echo.
echo    ┌────────────────────────────────────────────────────┐
echo    │ █████████████████████████░░░░░░░░░░░░░░░░░░░░░░░░░ │ 50%%
echo    └────────────────────────────────────────────────────┘
:: --- [NOVO WINDOWS 11]: Remove o envio de dados do Edge para a Microsoft ---
REG ADD "HKLM\SOFTWARE\Policies\Microsoft\Edge" /v "MetricsReportingEnabled" /t REG_DWORD /d 0 /f >nul 2>&1
REG ADD "HKLM\SOFTWARE\Policies\Microsoft\Edge" /v "PersonalizationReportingEnabled" /t REG_DWORD /d 0 /f >nul 2>&1
ping localhost -n 2 >nul

cls
echo.
echo  ╔═════════════════════════════════════════════════════════════════════════════╗
echo  ║                     DESATIVAR TELEMETRIA E COLETA DE DADOS                  ║
echo  ╚═════════════════════════════════════════════════════════════════════════════╝
echo.
echo    Status atual: [ NOVO: Desativando servicos ocultos de rastreamento... ]
echo.
echo    ┌────────────────────────────────────────────────────┐
echo    │ █████████████████████████████████████████░░░░░░░░░ │ 80%%
echo    └────────────────────────────────────────────────────┘
:: --- [NOVO WINDOWS 11]: Desativa os serviços de telemetria persistentes ---
sc config WbioSrvc start= disabled >nul 2>&1
sc stop WbioSrvc >nul 2>&1
sc config DPS start= disabled >nul 2>&1
sc stop DPS >nul 2>&1
ping localhost -n 2 >nul

cls
echo.
echo  ╔═════════════════════════════════════════════════════════════════════════════╗
echo  ║                     DESATIVAR TELEMETRIA E COLETA DE DADOS                  ║
echo  ╚═════════════════════════════════════════════════════════════════════════════╝
echo.
echo    Status atual: [ Telemetria e Privacidade otimizadas com sucesso! ]
echo.
echo    ┌────────────────────────────────────────────────────┐
echo    │ ██████████████████████████████████████████████████ │ 100%%
echo    └────────────────────────────────────────────────────┘
echo.
pause
goto menuwindows

:: Opção 5 - Desativar XBOX :: -------------------------------------------------------------------- ::

:opcao5

echo.
echo  ╔═════════════════════════════════════════════════════════════════════════════╗
echo  ║                         DESATIVAR TOTALMENTE A XBOX                         ║
echo  ╚═════════════════════════════════════════════════════════════════════════════╝
echo.
echo    [1] Iniciar Otimizacao (Remover Xbox e Servicos)
echo    [2] Reverter Otimizacao (Restaurar Xbox e Servicos)
echo    [3] Voltar ao Menu Principal
echo.
echo  ╚═════════════════════════════════════════════════════════════════════════════╝
echo.
set /p escolha= Digite a opcao desejada: 

if "%escolha%"=="1" goto OTIMIZAR
if "%escolha%"=="2" goto REVERTER
if "%escolha%"=="3" goto menuwindows
echo Opcao Invalida! Tente novamente.
ping localhost -n 2 >nul
goto submenuxbox


:OTIMIZAR
cls
echo.
echo  ╔═════════════════════════════════════════════════════════════════════════════╗
echo  ║                         DESATIVAR TOTALMENTE A XBOX                         ║
echo  ╚═════════════════════════════════════════════════════════════════════════════╝
echo.
echo    Status atual: [ Interrompendo servicos nativos do Xbox... ]
echo.
echo    ┌────────────────────────────────────────────────────┐
echo    │ ██████████░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░ │ 20%%
echo    └────────────────────────────────────────────────────┘
sc stop "Xbox Game Monitoring" >nul 2>&1
sc config "Xbox Game Monitoring" start= disabled >nul 2>&1
sc stop "GamingServices" >nul 2>&1
sc config "GamingServices" start= disabled >nul 2>&1
sc stop "GamingServicesNet" >nul 2>&1
sc config "GamingServicesNet" start= disabled >nul 2>&1
sc stop "XblGameSave" >nul 2>&1
sc config "XblGameSave" start= disabled >nul 2>&1
sc stop "XboxNetApiSvc" >nul 2>&1
sc config "XboxNetApiSvc" start= disabled >nul 2>&1
ping localhost -n 2 >nul

cls
echo.
echo  ╔═════════════════════════════════════════════════════════════════════════════╗
echo  ║                         DESATIVAR TOTALMENTE A XBOX                         ║
echo  ╚═════════════════════════════════════════════════════════════════════════════╝
echo.
echo    Status atual: [ Removendo pacotes de Apps Xbox (Aguarde)... ]
echo.
echo    ┌────────────────────────────────────────────────────┐
echo    │ █████████████████████████░░░░░░░░░░░░░░░░░░░░░░░░░ │ 50%%
echo    └────────────────────────────────────────────────────┘
powershell -command "Get-AppxPackage *xboxapp* | Remove-AppxPackage" >nul 2>&1
powershell -command "Get-AppxPackage *xboxgamemode* | Remove-AppxPackage" >nul 2>&1
powershell -command "Get-AppxPackage *Microsoft.XboxGameOverlay* | Remove-AppxPackage" >nul 2>&1
powershell -command "Get-AppxPackage *Microsoft.GamingServices* | Remove-AppxPackage" >nul 2>&1

cls
echo.
echo  ╔═════════════════════════════════════════════════════════════════════════════╗
echo  ║                         DESATIVAR TOTALMENTE A XBOX                         ║
echo  ╚═════════════════════════════════════════════════════════════════════════════╝
echo.
echo    Status atual: [ Bloqueando GameDVR, Telemetria e Windows Update... ]
echo.
echo    ┌────────────────────────────────────────────────────┐
echo    │ █████████████████████████████████████████░░░░░░░░░ │ 80%%
echo    └────────────────────────────────────────────────────┘
REG ADD "HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\GameDVR" /v "AllowGameDVR" /t REG_DWORD /d 0 /f >nul 2>&1
REG ADD "HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\GameBar" /v "AllowAutoGameMode" /t REG_DWORD /d 0 /f >nul 2>&1
REG ADD "HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\DataCollection" /v "AllowTelemetry" /t REG_DWORD /d 0 /f >nul 2>&1
REG ADD "HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\System" /v "AllowAppDataCollection" /t REG_DWORD /d 0 /f >nul 2>&1
REG ADD "HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\AdvertisingInfo" /v "DisableWindowsAdvertising" /t REG_DWORD /d 1 /f >nul 2>&1
REG ADD "HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\CloudContent" /v "DisableMicrosoftConsumerExperience" /t REG_DWORD /d 1 /f >nul 2>&1
REG ADD "HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\WindowsUpdate" /v "DoNotConnectToWindowsUpdateInternetLocations" /t REG_DWORD /d 1 /f >nul 2>&1
sc stop DiagTrack >nul 2>&1
sc config DiagTrack start= disabled >nul 2>&1
sc stop dmwappushservice >nul 2>&1
sc config dmwappushservice start= disabled >nul 2>&1
sc stop wuauserv >nul 2>&1
sc config wuauserv start= disabled >nul 2>&1
sc stop dosvc >nul 2>&1
sc config dosvc start= disabled >nul 2>&1
ping localhost -n 2 >nul

cls
echo.
echo  ╔═════════════════════════════════════════════════════════════════════════════╗
echo  ║                         DESATIVAR TOTALMENTE A XBOX                         ║
echo  ╚═════════════════════════════════════════════════════════════════════════════╝
echo.
echo    Status atual: [ Otimizacao concluida com sucesso! ]
echo.
echo    ┌────────────────────────────────────────────────────┐
echo    │ ██████████████████████████████████████████████████ │ 100%%
echo    └────────────────────────────────────────────────────┘
echo.
pause
goto REINICIAR


:REVERTER
cls
echo.
echo  ╔═════════════════════════════════════════════════════════════════════════════╗
echo  ║                         DESATIVAR TOTALMENTE A XBOX                         ║
echo  ╚═════════════════════════════════════════════════════════════════════════════╝
echo.
echo    Status atual: [ Revertendo servicos e chaves de Registro... ]
echo.
echo    ┌────────────────────────────────────────────────────┐
echo    │ █████████████████████████░░░░░░░░░░░░░░░░░░░░░░░░░ │ 50%%
echo    └────────────────────────────────────────────────────┘
sc config "Xbox Game Monitoring" start= demand >nul 2>&1
sc config "GamingServices" start= demand >nul 2>&1
sc config "GamingServicesNet" start= demand >nul 2>&1
sc config "XblAuthManager" start= demand >nul 2>&1
sc config "XblGameSave" start= demand >nul 2>&1
sc config "XboxNetApiSvc" start= demand >nul 2>&1
REG DELETE "HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\GameDVR" /f >nul 2>&1
REG DELETE "HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\GameBar" /f >nul 2>&1
REG DELETE "HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\DataCollection" /f >nul 2>&1
REG DELETE "HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\System" /v "AllowAppDataCollection" /f >nul 2>&1
REG DELETE "HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\AdvertisingInfo" /f >nul 2>&1
REG DELETE "HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\CloudContent" /f >nul 2>&1
REG DELETE "HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\WindowsUpdate" /v "DoNotConnectToWindowsUpdateInternetLocations" /f >nul 2>&1
sc config DiagTrack start= auto >nul 2>&1
sc start DiagTrack >nul 2>&1
sc config dmwappushservice start= demand >nul 2>&1
sc config wuauserv start= auto >nul 2>&1
sc start wuauserv >nul 2>&1
sc config dosvc start= demand >nul 2>&1
ping localhost -n 2 >nul

cls
echo.
echo  ╔═════════════════════════════════════════════════════════════════════════════╗
echo  ║                         DESATIVAR TOTALMENTE A XBOX                         ║
echo  ╚═════════════════════════════════════════════════════════════════════════════╝
echo.
echo    Status atual: [ Reversao concluida com sucesso! ]
echo.
echo    ┌────────────────────────────────────────────────────┐
echo    │ ██████████████████████████████████████████████████ │ 100%%
echo    └────────────────────────────────────────────────────┘
echo.
pause
goto REINICIAR


:REINICIAR
cls
echo.
echo  ╔═════════════════════════════════════════════════════════════════════════════╗
echo  ║                            REINICIALIZACAO RECOMENDADA                      ║
echo  ╚═════════════════════════════════════════════════════════════════════════════╝
echo.
echo    Deseja reiniciar o computador agora para aplicar as alteracoes? [S/N]
echo.
set /p resp= Digite sua resposta: 

if /i "%resp%"=="S" (
    echo.
    echo    Reiniciando o sistema em 5 segundos...
    shutdown /r /t 5
    pause
    exit
) else (
    echo.
    echo    Operacao cancelada. Retornando ao menu principal...
    ping localhost -n 2 >nul
    goto menuwindows
)


:: Opção 6 - Desativar relatório de erro do windows :: -------------------------------------------------------------------- ::

:opcao6
cls
echo.
echo  ╔═════════════════════════════════════════════════════════════════════════════╗
echo  ║                         DESATIVAR RELATORIOS DE ERRO                        ║
echo  ╚═════════════════════════════════════════════════════════════════════════════╝
echo.
echo    Status atual: [ Interrompendo o Servico de Relatorios (WerSvc)... ]
echo.
echo    ┌────────────────────────────────────────────────────┐
echo    │ ░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░ │ 0%%
echo    └────────────────────────────────────────────────────┘
:: --- Interrompe e Desativa o Serviço Principal de Erros ---
sc stop "WerSvc" >nul 2>&1
sc config "WerSvc" start= disabled >nul 2>&1
ping localhost -n 2 >nul

cls
echo.
echo  ╔═════════════════════════════════════════════════════════════════════════════╗
echo  ║                         DESATIVAR RELATORIOS DE ERRO                        ║
echo  ╚═════════════════════════════════════════════════════════════════════════════╝
echo.
echo    Status atual: [ NOVO: Desativando Assistente de Compatibilidade (PcaSvc)... ]
echo.
echo    ┌────────────────────────────────────────────────────┐
echo    │ █████████████░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░ │ 25%%
echo    └────────────────────────────────────────────────────┘
:: --- [NOVO WINDOWS 11]: Desativa o Assistente de Compatibilidade que gera logs extras ---
sc stop "PcaSvc" >nul 2>&1
sc config "PcaSvc" start= disabled >nul 2>&1
ping localhost -n 2 >nul

cls
echo.
echo  ╔═════════════════════════════════════════════════════════════════════════════╗
echo  ║                         DESATIVAR RELATORIOS DE ERRO                        ║
echo  ╚═════════════════════════════════════════════════════════════════════════════╝
echo.
echo    Status atual: [ Bloqueando envio de dados adicionais na maquina... ]
echo.
echo    ┌────────────────────────────────────────────────────┐
echo    │ █████████████████████████░░░░░░░░░░░░░░░░░░░░░░░░░ │ 50%%
echo    └────────────────────────────────────────────────────┘
:: --- Políticas Globais de Erro do Sistema ---
REG ADD "HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\ErrorReporting" /v "DontSendAdditionalData" /t REG_DWORD /d 1 /f >nul 2>&1
REG ADD "HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\ErrorReporting" /v "Disabled" /t REG_DWORD /d 1 /f >nul 2>&1
REG ADD "HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\Windows Error Reporting" /v "DisableWindowsErrorReporting" /t REG_DWORD /d 1 /f >nul 2>&1
ping localhost -n 2 >nul

cls
echo.
echo  ╔═════════════════════════════════════════════════════════════════════════════╗
echo  ║                         DESATIVAR RELATORIOS DE ERRO                        ║
echo  ╚═════════════════════════════════════════════════════════════════════════════╝
echo.
echo    Status atual: [ NOVO: Bloqueando geracao de Dumps de erro locais... ]
echo.
echo    ┌────────────────────────────────────────────────────┐
echo    │ █████████████████████████████████████████░░░░░░░░░ │ 80%%
echo    └────────────────────────────────────────────────────┘
:: --- [NOVO WINDOWS 11]: Impede a criação de relatórios e telemetria no escopo do usuário atual ---
REG ADD "HKCU\Software\Microsoft\Windows\Windows Error Reporting" /v "Disabled" /t REG_DWORD /d 1 /f >nul 2>&1
REG ADD "HKCU\Software\Microsoft\Windows\Windows Error Reporting" /v "DontShowUI" /t REG_DWORD /d 1 /f >nul 2>&1
ping localhost -n 2 >nul

cls
echo.
echo  ╔═════════════════════════════════════════════════════════════════════════════╗
echo  ║                         DESATIVAR RELATORIOS DE ERRO                        ║
echo  ╚═════════════════════════════════════════════════════════════════════════════╝
echo.
echo    Status atual: [ Relatorios de Erro desativados com sucesso! ]
echo.
echo    ┌────────────────────────────────────────────────────┐
echo    │ ██████████████████████████████████████████████████ │ 100%%
echo    └────────────────────────────────────────────────────┘
echo.
pause
goto menuwindows

:: Opção 7 - Otimizar ALT + TAB :: -------------------------------------------------------------------- ::

:opcao7
cls
echo.
echo  ╔═════════════════════════════════════════════════════════════════════════════╗
echo  ║                              OPCOES ALT + TAB                               ║
echo  ╚═════════════════════════════════════════════════════════════════════════════╝
echo.
echo    [1] Otimizar ALT + TAB (Estilo Classico e Veloz)
echo    [2] Reverter ALT + TAB (Padrao Windows 11)
echo    [3] Voltar para o Menu Principal
echo.
echo  ╚═════════════════════════════════════════════════════════════════════════════╝
echo.
set /p escolhaAltTab= Digite a opcao desejada: 

if "%escolhaAltTab%"=="1" goto otimizarAltTab
if "%escolhaAltTab%"=="2" goto reverterAltTab
if "%escolhaAltTab%"=="3" goto menuwindows
echo Opcao Invalida! Tente novamente.
ping localhost -n 2 >nul
goto submenuAlttab


:otimizarAltTab
cls
echo.
echo  ╔═════════════════════════════════════════════════════════════════════════════╗
echo  ║                              OPCOES ALT + TAB                               ║
echo  ╚═════════════════════════════════════════════════════════════════════════════╝
echo.
echo    Status atual: [ Ativando interface classica do Alt+TAB... ]
echo.
echo    ┌────────────────────────────────────────────────────┐
echo    │ █████████████░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░ │ 25%%
echo    └────────────────────────────────────────────────────┘
:: Ativa o Alt+TAB clássico ultra rápido (sem desfoque/blur pesado)
reg add "HKCU\Software\Microsoft\Windows\CurrentVersion\Explorer" /v AltTabSettings /t REG_DWORD /D 1 /f >nul 2>&1
ping localhost -n 1 >nul

cls
echo.
echo  ╔═════════════════════════════════════════════════════════════════════════════╗
echo  ║                              OPCOES ALT + TAB                               ║
echo  ╚═════════════════════════════════════════════════════════════════════════════╝
echo.
echo    Status atual: [ NOVO: Removendo abas do Edge do alternador... ]
echo.
echo    ┌────────────────────────────────────────────────────┐
echo    │ █████████████████████████░░░░░░░░░░░░░░░░░░░░░░░░░ │ 50%%
echo    └────────────────────────────────────────────────────┘
:: [NOVO WINDOWS 11]: Faz o Alt+TAB ignorar as abas do Edge e focar só nos programas ativos
reg add "HKCU\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced" /v "MultiTaskingAltTabFilter" /t REG_DWORD /d 3 /f >nul 2>&1
ping localhost -n 1 >nul

cls
echo.
echo  ╔═════════════════════════════════════════════════════════════════════════════╗
echo  ║                              OPCOES ALT + TAB                               ║
echo  ╚═════════════════════════════════════════════════════════════════════════════╝
echo.
echo    Status atual: [ Reiniciando o Windows Explorer... ]
echo.
echo    ┌────────────────────────────────────────────────────┐
echo    │ █████████████████████████████████████████░░░░░░░░░ │ 80%%
echo    └────────────────────────────────────────────────────┘
taskkill /f /im explorer.exe >nul 2>&1
timeout /t 2 /nobreak >nul
start explorer.exe
ping localhost -n 1 >nul

cls
echo.
echo  ╔═════════════════════════════════════════════════════════════════════════════╗
echo  ║                              OPCOES ALT + TAB                               ║
echo  ╚═════════════════════════════════════════════════════════════════════════════╝
echo.
echo    Status atual: [ ALT + TAB otimizado com sucesso! ]
echo.
echo    ┌────────────────────────────────────────────────────┐
echo    │ ██████████████████████████████████████████████████ │ 100%%
echo    └────────────────────────────────────────────────────┘
echo.
pause
goto submenuAlttab


:reverterAltTab
cls
echo.
echo  ╔═════════════════════════════════════════════════════════════════════════════╗
echo  ║                              OPCOES ALT + TAB                               ║
echo  ╚═════════════════════════════════════════════════════════════════════════════╝
echo.
echo    Status atual: [ Restaurando configuracoes nativas e abas... ]
echo.
echo    ┌────────────────────────────────────────────────────┐
echo    │ █████████████████████████░░░░░░░░░░░░░░░░░░░░░░░░░ │ 50%%
echo    └────────────────────────────────────────────────────┘
:: Remove as chaves e traz de volta o Alt+TAB moderno com efeito blur e abas do Edge
reg delete "HKCU\Software\Microsoft\Windows\CurrentVersion\Explorer" /v AltTabSettings /f >nul 2>&1
reg delete "HKCU\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced" /v "MultiTaskingAltTabFilter" /f >nul 2>&1
ping localhost -n 1 >nul

cls
echo.
echo  ╔═════════════════════════════════════════════════════════════════════════════╗
echo  ║                              OPCOES ALT + TAB                               ║
echo  ╚═════════════════════════════════════════════════════════════════════════════╝
echo.
echo    Status atual: [ Reiniciando o Windows Explorer... ]
echo.
echo    ┌────────────────────────────────────────────────────┐
echo    │ █████████████████████████████████████████░░░░░░░░░ │ 80%%
echo    └────────────────────────────────────────────────────┘
taskkill /f /im explorer.exe >nul 2>&1
timeout /t 2 /nobreak >nul
start explorer.exe
ping localhost -n 1 >nul

cls
echo.
echo  ╔═════════════════════════════════════════════════════════════════════════════╗
echo  ║                              OPCOES ALT + TAB                               ║
echo  ╚═════════════════════════════════════════════════════════════════════════════╝
echo.
echo    Status atual: [ ALT + TAB restaurado com sucesso! ]
echo.
echo    ┌────────────────────────────────────────────────────┐
echo    │ ██████████████████████████████████████████████████ │ 100%%
echo    └────────────────────────────────────────────────────┘
echo.
pause
goto submenuAlttab
goto :opcao7

:: Opção 8 - Remover serviços inuteis :: -------------------------------------------------------------------- ::

:opcao8

cls
echo.
echo  ╔═════════════════════════════════════════════════════════════════════════════╗
echo  ║                  OTIMIZADOR DE SERVICOS DO WINDOWS 11                       ║
echo  ╚═════════════════════════════════════════════════════════════════════════════╝
echo.
echo    [i] Esta opcao permite desativar servicos desnecessarios que rodam
echo        em segundo plano, liberando consumo de RAM, CPU, disco e rede.
echo.
echo    ───────────────────────────────────────────────────────────────────────────
echo.
echo    [1] Desativar Servicos Inuteis (Recomendado)
echo    [2] Reverter Otimizacao (Restaurar Padroes)
echo    [3] Voltar ao Menu Principal
echo.
echo  ───────────────────────────────────────────────────────────────────────────────
echo.
set /p opcao=   [?] Digite a opcao desejada: 
cls

if "%opcao%"=="1" goto OTIMIZAR_SERVICOS
if "%opcao%"=="2" goto REVERTER_SERVICOS
if "%opcao%"=="3" goto :menuwindows

goto :opcao8


:: ==============================================================================
:: OTIMIZACAO DE SERVICOS
:: ==============================================================================

:OTIMIZAR_SERVICOS
cls
echo.
echo  ╔═════════════════════════════════════════════════════════════════════════════╗
echo  ║                         OTIMIZANDO SERVICOS DO WINDOWS 11                   ║
echo  ╚═════════════════════════════════════════════════════════════════════════════╝
echo.
echo    Status atual: [ Fase 1: Desativando serviços de Impressão, Busca e RAM... ]
echo.
echo    ┌────────────────────────────────────────────────────┐
echo    │ ██████░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░ │ 10%%
echo    └────────────────────────────────────────────────────┘

:: 1. Print Spooler
sc stop Spooler >nul 2>&1
sc config Spooler start= disabled >nul 2>&1

:: 2. Windows Search (WSearch)
sc stop WSearch >nul 2>&1
sc config WSearch start= disabled >nul 2>&1

:: 3. SysMain (Superfetch)
sc stop SysMain >nul 2>&1
sc config SysMain start= disabled >nul 2>&1

ping localhost -n 2 >nul
cls
echo.
echo  ╔═════════════════════════════════════════════════════════════════════════════╗
echo  ║                         OTIMIZANDO SERVICOS DO WINDOWS 11                   ║
echo  ╚═════════════════════════════════════════════════════════════════════════════╝
echo.
echo    Status atual: [ Fase 2: Cortando Telemetrias, Rastreadores e Erros... ]
echo.
echo    ┌────────────────────────────────────────────────────┐
echo    │ ████████████████░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░ │ 30%%
echo    └────────────────────────────────────────────────────┘

:: 4. Connected User Experiences and Telemetry (DiagTrack)
sc stop DiagTrack >nul 2>&1
sc config DiagTrack start= disabled >nul 2>&1

:: 5. dmwappushservice
sc stop dmwappushservice >nul 2>&1
sc config dmwappushservice start= disabled >nul 2>&1

:: 6. Windows Error Reporting Service (WerSvc)
sc stop WerSvc >nul 2>&1
sc config WerSvc start= disabled >nul 2>&1

:: 7. Windows Biometric Service (WbioSrvc)
sc stop WbioSrvc >nul 2>&1
sc config WbioSrvc start= disabled >nul 2>&1

ping localhost -n 2 >nul
cls
echo.
echo  ╔═════════════════════════════════════════════════════════════════════════════╗
echo  ║                         OTIMIZANDO SERVICOS DO WINDOWS 11                   ║
echo  ╚═════════════════════════════════════════════════════════════════════════════╝
echo.
echo    Status atual: [ Fase 3: Desativando Windows Update e Entregas P2P... ]
echo.
echo    ┌────────────────────────────────────────────────────┐
echo    │ ████████████████████████░░░░░░░░░░░░░░░░░░░░░░░░░░ │ 45%%
echo    └────────────────────────────────────────────────────┘

:: 8. Windows Update (wuauserv)
sc stop wuauserv >nul 2>&1
sc config wuauserv start= disabled >nul 2>&1

:: 9. Delivery Optimization (dosvc)
sc stop dosvc >nul 2>&1
sc config dosvc start= disabled >nul 2>&1

ping localhost -n 2 >nul
cls
echo.
echo  ╔═════════════════════════════════════════════════════════════════════════════╗
echo  ║                         OTIMIZANDO SERVICOS DO WINDOWS 11                   ║
echo  ╚═════════════════════════════════════════════════════════════════════════════╝
echo.
echo    Status atual: [ Fase 4: Limpando serviços do ecossistema Xbox... ]
echo.
echo    ┌────────────────────────────────────────────────────┐
echo    │ ████████████████████████████████░░░░░░░░░░░░░░░░░░ │ 60%%
echo    └────────────────────────────────────────────────────┘

:: 10. Xbox Live Auth Manager
sc stop XblAuthManager >nul 2>&1
sc config XblAuthManager start= disabled >nul 2>&1

:: 11. Xbox Live Game Save
sc stop XblGameSave >nul 2>&1
sc config XblGameSave start= disabled >nul 2>&1

:: 12. Xbox Live Networking
sc stop XboxNetApiSvc >nul 2>&1
sc config XboxNetApiSvc start= disabled >nul 2>&1

ping localhost -n 2 >nul
cls
echo.
echo  ╔═════════════════════════════════════════════════════════════════════════════╗
echo  ║                         OTIMIZANDO SERVICOS DO WINDOWS 11                   ║
echo  ╚═════════════════════════════════════════════════════════════════════════════╝
echo.
echo    Status atual: [ Fase 5: Removendo acessos remotos e assistentes antigos... ]
echo.
echo    ┌────────────────────────────────────────────────────┐
echo    │ ██████████████████████████████████████░░░░░░░░░░░░ │ 75%%
echo    └────────────────────────────────────────────────────┘

:: 13. Remote Registry
sc stop RemoteRegistry >nul 2>&1
sc config RemoteRegistry start= disabled >nul 2>&1

:: 14. Program Compatibility Assistant Service
sc stop PcaSvc >nul 2>&1
sc config PcaSvc start= disabled >nul 2>&1

:: 15. Windows Image Acquisition (WIA)
sc stop stisvc >nul 2>&1
sc config stisvc start= disabled >nul 2>&1

:: 16. Bluetooth Support Service
sc stop bthserv >nul 2>&1
sc config bthserv start= disabled >nul 2>&1

:: 17. Windows Defender Antivirus
sc stop WinDefend >nul 2>&1
sc config WinDefend start= disabled >nul 2>&1

ping localhost -n 2 >nul
cls
echo.
echo  ╔═════════════════════════════════════════════════════════════════════════════╗
echo  ║                         OTIMIZANDO SERVICOS DO WINDOWS 11                   ║
echo  ╚═════════════════════════════════════════════════════════════════════════════╝
echo.
echo    Status atual: [ Fase 6: Desativando ferramentas Mobile, Mapas e Toque... ]
echo.
echo ┌────────────────────────────────────────────────────┐
echo │ ███████████████████████████████████████████░░░░░░ │ 90%%
echo └────────────────────────────────────────────────────┘

:: 18. Windows Time
sc stop W32Time >nul 2>&1
sc config W32Time start= disabled >nul 2>&1

:: 19. Downloaded Maps Manager
sc stop MapsBroker >nul 2>&1
sc config MapsBroker start= disabled >nul 2>&1

:: 20. Windows Insider Service
sc stop wisvc >nul 2>&1
sc config wisvc start= disabled >nul 2>&1

:: 21. Touch Keyboard and Handwriting Panel Service
sc stop TabletInputService >nul 2>&1
sc config TabletInputService start= disabled >nul 2>&1

:: 22. Windows Connect Now
sc stop wcncsvc >nul 2>&1
sc config wcncsvc start= disabled >nul 2>&1

:: 23. Offline Files
sc stop CscService >nul 2>&1
sc config CscService start= disabled >nul 2>&1

:: 24. Windows Mobile Hotspot Service
sc stop icssvc >nul 2>&1
sc config icssvc start= disabled >nul 2>&1

:: 25. Fax Service
sc stop Fax >nul 2>&1
sc config Fax start= disabled >nul 2>&1

ping localhost -n 2 >nul
cls
echo.
echo  ╔═════════════════════════════════════════════════════════════════════════════╗
echo  ║                         OTIMIZANDO SERVICOS DO WINDOWS 11                   ║
echo  ╚═════════════════════════════════════════════════════════════════════════════╝
echo.
echo    Status atual: [ Fase 7: Removendo Bloatwares (Carteira, Sensores e Telefonia) ]
echo.
echo    ┌────────────────────────────────────────────────────┐
echo    │ ████████████████████████████████████████████████░░ │ 98%%
echo    └────────────────────────────────────────────────────┘

:: 26. WalletService - Carteira do Windows
sc stop WalletService >nul 2>&1
sc config WalletService start= disabled >nul 2>&1

:: 27. PhoneSvc - Serviço de Telefone / Vínculo com Celular
sc stop PhoneSvc >nul 2>&1
sc config PhoneSvc start= disabled >nul 2>&1

:: 28. EnterpriseAppMgmtSvc - Gerenciamento de Apps Corporativos
sc stop EnterpriseAppMgmtSvc >nul 2>&1
sc config EnterpriseAppMgmtSvc start= disabled >nul 2>&1

:: 29. SensorService / SensorDataService / SensorsSvc - Sensores em Desktops
sc stop SensorService >nul 2>&1
sc config SensorService start= disabled >nul 2>&1
sc stop SensorDataService >nul 2>&1
sc config SensorDataService start= disabled >nul 2>&1
sc stop SensorsSvc >nul 2>&1
sc config SensorsSvc start= disabled >nul 2>&1

:: 30. RetailDemo - Modo Demonstração de Loja
sc stop RetailDemo >nul 2>&1
sc config RetailDemo start= disabled >nul 2>&1

ping localhost -n 2 >nul
cls
echo.
echo  ╔═════════════════════════════════════════════════════════════════════════════╗
echo  ║                         OTIMIZANDO SERVICOS DO WINDOWS 11                   ║
echo  ╚═════════════════════════════════════════════════════════════════════════════╝
echo.
echo    Status atual: [ Otimizacao de servicos concluida com sucesso! ]
echo.
echo    ┌────────────────────────────────────────────────────┐
echo    │ ██████████████████████████████████████████████████ │ 100%%
echo    └────────────────────────────────────────────────────┘
echo.
echo    Foram desativados 30 servicos desnecessarios para jogos e performance.
echo.
echo    ATENCAO:
echo    - Windows Update foi desativado. Lembre-se de atualizar manualmente.
echo    - Windows Defender foi desativado. Certifique-se de ter outro antivirus.
echo    - Recursos como Impressao, Biometria ou Bluetooth devem ser reativados
echo      caso voce precise utiliza-los no dia a dia.
echo.
pause
goto :opcao8


:: ==============================================================================
:: REVERSAO DA OTIMIZACAO
:: ==============================================================================
:REVERTER_SERVICOS
cls
echo.
echo  ╔═════════════════════════════════════════════════════════════════════════════╗
echo  ║                      REVERTENDO OTIMIZACAO DE SERVICOS                      ║
echo  ╚═════════════════════════════════════════════════════════════════════════════╝
echo.
echo    Status atual: [ Restaurando servicos essenciais do sistema... ]
echo.
echo    ┌────────────────────────────────────────────────────┐
echo    │ ████████████░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░ │ 25%%
echo    └────────────────────────────────────────────────────┘

sc config Spooler start= auto >nul 2>&1
sc start Spooler >nul 2>&1
sc config WSearch start= auto >nul 2>&1
sc start WSearch >nul 2>&1
sc config SysMain start= auto >nul 2>&1
sc start SysMain >nul 2>&1
sc config DiagTrack start= demand >nul 2>&1
sc config dmwappushservice start= demand >nul 2>&1
sc config WerSvc start= demand >nul 2>&1
sc config WbioSrvc start= demand >nul 2>&1

ping localhost -n 2 >nul
cls
echo.
echo  ╔═════════════════════════════════════════════════════════════════════════════╗
echo  ║                      REVERTENDO OTIMIZACAO DE SERVICOS                      ║
echo  ╚═════════════════════════════════════════════════════════════════════════════╝
echo.
echo    Status atual: [ Reativando Windows Update, Defender e Xbox Live... ]
echo.
echo    ┌────────────────────────────────────────────────────┐
echo    │ ████████████████████████████░░░░░░░░░░░░░░░░░░░░░░ │ 55%%
echo    └────────────────────────────────────────────────────┘

sc config wuauserv start= auto >nul 2>&1
sc start wuauserv >nul 2>&1
sc config dosvc start= demand >nul 2>&1
sc config XblAuthManager start= manual >nul 2>&1
sc config XblGameSave start= manual >nul 2>&1
sc config XboxNetApiSvc start= manual >nul 2>&1
sc config RemoteRegistry start= demand >nul 2>&1
sc config PcaSvc start= demand >nul 2>&1
sc config stisvc start= demand >nul 2>&1
sc config bthserv start= demand >nul 2>&1
sc config WinDefend start= auto >nul 2>&1
sc start WinDefend >nul 2>&1

ping localhost -n 2 >nul
cls
echo.
echo  ╔═════════════════════════════════════════════════════════════════════════════╗
echo  ║                      REVERTENDO OTIMIZACAO DE SERVICOS                      ║
echo  ╚═════════════════════════════════════════════════════════════════════════════╝
echo.
echo    Status atual: [ Restaurando conectividade e bloatwares de sensores... ]
echo.
echo    ┌────────────────────────────────────────────────────┐
echo    │ ████████████████████████████████████████████░░░░░░ │ 85%%
echo    └────────────────────────────────────────────────────┘

sc config W32Time start= demand >nul 2>&1
sc config MapsBroker start= demand >nul 2>&1
sc config wisvc start= demand >nul 2>&1
sc config TabletInputService start= demand >nul 2>&1
sc config wcncsvc start= demand >nul 2>&1
sc config CscService start= demand >nul 2>&1
sc config icssvc start= manual >nul 2>&1
sc config Fax start= demand >nul 2>&1

:: Restringindo os novos serviços adicionados
sc config WalletService start= demand >nul 2>&1
sc config PhoneSvc start= manual >nul 2>&1
sc config EnterpriseAppMgmtSvc start= manual >nul 2>&1
sc config SensorService start= manual >nul 2>&1
sc config SensorDataService start= manual >nul 2>&1
sc config SensorsSvc start= manual >nul 2>&1
sc config RetailDemo start= manual >nul 2>&1

ping localhost -n 2 >nul
cls
echo.
echo  ╔═════════════════════════════════════════════════════════════════════════════╗
echo  ║                      REVERTENDO OTIMIZACAO DE SERVICOS                      ║
echo  ╚═════════════════════════════════════════════════════════════════════════════╝
echo.
echo    Status atual: [ Reversao concluida com sucesso! ]
echo.
echo    ┌────────────────────────────────────────────────────┐
echo    │ ██████████████████████████████████████████████████ │ 100%%
echo    └────────────────────────────────────────────────────┘
echo.
echo    Todos os 30 servicos retornaram para seus estados originais padrões.
echo.
pause
goto :opcao8

:: Opção 9 - Desativar Hibernação :: -------------------------------------------------------------------- ::

:opcao9
cls
echo Desativando Hibernação...
powercfg -h off
pause
cls
goto :menuwindows

:: Opção 10 - Otimizar windows explorer :: -------------------------------------------------------------------- ::

:opcao10
cls
echo.
echo  ╔═════════════════════════════════════════════════════════════════════════════╗
echo  ║                     OTIMIZANDO O WINDOWS EXPLORER E SISTEMA                    ║
echo  ╚═════════════════════════════════════════════════════════════════════════════╝
echo.
echo     Status atual: [ Fase 1: Ajustando Registro do Explorer ]
echo.
echo     ┌────────────────────────────────────────────────────┐
echo     │ ████░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░ │ 10%%
echo     └────────────────────────────────────────────────────┘
:: Desativando o Acesso Rápido e Histórico
reg add "HKCU\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced" /v LaunchTo /t REG_DWORD /d 1 /f >nul
reg add "HKCU\Software\Microsoft\Windows\CurrentVersion\Explorer" /v ShowRecent /t REG_DWORD /d 0 /f >nul
reg add "HKCU\Software\Microsoft\Windows\CurrentVersion\Explorer" /v ShowFrequent /t REG_DWORD /d 0 /f >nul
reg add "HKCU\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced" /v Start_TrackDocs /t REG_DWORD /d 0 /f >nul
reg add "HKCU\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced" /v JumpListItems_Maximum /t REG_DWORD /d 0 /f >nul
reg delete "HKCU\Software\Microsoft\Windows\CurrentVersion\Explorer\TypedPaths" /f >nul 2>&1
ping localhost -n 2 >nul

cls
echo.
echo  ╔═════════════════════════════════════════════════════════════════════════════╗
echo  ║                     OTIMIZANDO O WINDOWS EXPLORER E SISTEMA                    ║
echo  ╚═════════════════════════════════════════════════════════════════════════════╝
echo.
echo     Status atual: [ Fase 2: Otimizando Resposta do Sistema (NOVO) ]
echo.
echo     ┌────────────────────────────────────────────────────┐
echo     │ ████████████████████░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░ │ 40%%
echo     └────────────────────────────────────────────────────┘
:: NOVAS FUNÇÕES: Acelerar Menu Show Delay e diminuir tempo de espera para fechar apps travados
reg add "HKCU\Control Panel\Desktop" /v MenuShowDelay /t REG_SZ /d 10 /f >nul
reg add "HKCU\Control Panel\Desktop" /v WaitToKillAppTimeout /t REG_SZ /d 2000 /f >nul
reg add "HKCU\Control Panel\Desktop" /v HungAppTimeout /t REG_SZ /d 2000 /f >nul
ping localhost -n 2 >nul

cls
echo.
echo  ╔═════════════════════════════════════════════════════════════════════════════╗
echo  ║                     OTIMIZANDO O WINDOWS EXPLORER E SISTEMA                    ║
echo  ╚═════════════════════════════════════════════════════════════════════════════╝
echo.
echo     Status atual: [ Fase 3: Desativando Efeitos Visuais Inúteis (NOVO) ]
echo.
echo     ┌────────────────────────────────────────────────────┐
echo     │ ████████████████████████████████████░░░░░░░░░░░░░░ │ 70%%
echo     └────────────────────────────────────────────────────┘
:: NOVAS FUNÇÕES: Desativa animações de janelas para dar mais fluidez ao sistema
reg add "HKCU\Control Panel\Desktop\WindowMetrics" /v MinAnimate /t REG_SZ /d 0 /f >nul
reg add "HKCU\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced" /v TaskbarAnimations /t REG_DWORD /d 0 /f >nul
ping localhost -n 2 >nul

cls
echo.
echo  ╔═════════════════════════════════════════════════════════════════════════════╗
echo  ║                     OTIMIZANDO O WINDOWS EXPLORER E SISTEMA                    ║
echo  ╚═════════════════════════════════════════════════════════════════════════════╝
echo.
echo     Status atual: [ Fase 4: Reiniciando a Interface do Windows ]
echo.
echo     ┌────────────────────────────────────────────────────┐
echo     │ ████████████████████████████████████████████████░░ │ 98%%
echo     └────────────────────────────────────────────────────┘
:: Reiniciando o Explorer para aplicar tudo de uma vez
taskkill /f /im explorer.exe >nul
start explorer.exe
ping localhost -n 3 >nul

cls
echo.
echo  ╔═════════════════════════════════════════════════════════════════════════════╗
echo  ║                   OTIMIZAÇÃO CONCLUÍDA COM SUCESSO!                         ║
echo  ╚═════════════════════════════════════════════════════════════════════════════╝
echo.
echo     [+] O Explorer foi reiniciado.
echo     [+] Histórico e Acesso Rápido limpos.
echo     [+] Resposta do sistema e menus acelerados!
echo.
echo  ───────────────────────────────────────────────────────────────────────────────
echo.
pause
goto :menuwindows

:: Opção 11 - Desativar Indexação de arquivos :: -------------------------------------------------------------------- ::

:opcao11
cls
echo Desativando Indxação de Arquivos...
net stop "Windows Search" >nul 2>&1
sc config "WSearch" start= disabled >nul 2>&1
pause
cls
goto :menuwindows

:: Opção 12 - Remover ou reinstalar APP's Padrão :: -------------------------------------------------------------------- ::

:opcao12
cls
echo.
echo  ╔═════════════════════════════════════════════════════════════════════════════╗
echo  ║                     REMOVER OU REINSTALAR APPS PADRÃO                       ║
echo  ╚═════════════════════════════════════════════════════════════════════════════╝
echo.
echo     [1] Remover Bloatwares e Desativar Copilot, Cortana e Recursos
echo     [2] Reinstalar Todos os Apps Padrão do Windows
echo     [3] Voltar ao Menu Principal
echo.
echo  ───────────────────────────────────────────────────────────────────────────────
echo.
set /p opcao= » Escolha uma opcao: 

if "%opcao%"=="1" goto REMOVER
if "%opcao%"=="2" goto REINSTALAR
if "%opcao%"=="3" goto :menuwindows
goto :opcao12

:REMOVER
cls
echo.
echo  ╔═════════════════════════════════════════════════════════════════════════════╗
echo  ║                         DESINSTALANDO BLOATWARES                            ║
echo  ╚═════════════════════════════════════════════════════════════════════════════╝
echo.
echo     Status atual: [ Fase 1: Removendo Apps Inúteis via PowerShell ]
echo.

:: Lista completa dos apps para remover
set "APPS_TO_REMOVE=Microsoft.549981C3F5F10|Cortana Microsoft.OfficeHub|OfficeHub Microsoft.MicrosoftOfficeHub Microsoft.WindowsPhone|Phone Microsoft.People|People Microsoft.WindowsMaps|Maps Microsoft.WindowsFeedbackHub|Feedback Microsoft.Getstarted|GetStarted Microsoft.WindowsAlarms|Alarms Microsoft.3DBuilder|3DBuilder Microsoft.BingNews|News Microsoft.OneDriveSync|OneDrive Microsoft.MixedReality.Portal|Reality Microsoft.MicrosoftSolitaireCollection|Solitaire Microsoft.MicrosoftStickyNotes|StickyNotes Microsoft.YourPhone|YourPhone Microsoft.Todos|To-Do Microsoft.SkypeApp|Skype Microsoft.BingWeather|Weather Microsoft.MicrosoftAdvertising.Xbox|XboxAds Microsoft.XboxApp|Xbox Microsoft.XboxGamingOverlay|XboxOverlay Microsoft.XboxGameCallableUI|XboxUI Microsoft.MicrosoftOfficeOneNote|OneNote Microsoft.WindowsCommunicationsApps|CommsApps Microsoft.WindowsSoundRecorder|Recorder"

:: Log de erros
set "LOG_FILE=%TEMP%\bloatware_removal_log.txt"
echo Removendo bloatwares... > "%LOG_FILE%"
echo Data: %date% %time% >> "%LOG_FILE%"
echo. >> "%LOG_FILE%"

:: Loop de remoção com verificação
for %%A in (%APPS_TO_REMOVE%) do (
    set "APP_NAME=%%A"
    set "APP_CLEAN=!APP_NAME:*|=!"
    echo.
    echo     [-] Removendo: !APP_CLEAN!...
    
    powershell -NoProfile -Command "Get-AppxPackage -AllUsers | Where-Object {$_.Name -like '*!APP_CLEAN!*'} | Remove-AppxPackage -ErrorAction SilentlyContinue" >> "%LOG_FILE%" 2>&1
    
    :: Verificar se o app foi removido
    powershell -NoProfile -Command "(Get-AppxPackage -AllUsers | Where-Object {$_.Name -like '*!APP_CLEAN!*'}).Count" > "%TEMP%\app_check.txt" 2>&1
    set /p APP_COUNT=<"%TEMP%\app_check.txt"
    
    if "!APP_COUNT!"=="0" (
        echo         ✓ Removido com sucesso!
    ) else (
        echo         ✗ Não foi possível remover ou já estava adicionado
    )
)

:: Limpar apps de provisionamento (para novos usuários)
echo.
echo     [-] Removendo provisionamento...
for %%A in (%APPS_TO_REMOVE%) do (
    set "APP_NAME=%%A"
    set "APP_CLEAN=!APP_NAME:*|=!"
    powershell -NoProfile -Command "Get-AppxProvisionedPackage -Online | Where-Object {$_.DisplayName -like '*!APP_CLEAN!*'} | Remove-AppxProvisionedPackage -Online -ErrorAction SilentlyContinue" >> "%LOG_FILE%" 2>&1
)

:: Fase 2: Tweaks de registro
cls
echo.
echo  ╔═════════════════════════════════════════════════════════════════════════════╗
echo  ║                         DESINSTALANDO BLOATWARES                            ║
echo  ╚═════════════════════════════════════════════════════════════════════════════╝
echo.
echo     Status atual: [ Fase 2: Desativando Copilot, Cortana e Sugestões ]
echo.

:: Copilot desativar
echo     [-] Desativando Copilot...
reg add "HKCU\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced" /v ShowCopilotButton /t REG_DWORD /d 0 /f >nul 2>&1
reg add "HKLM\SOFTWARE\Policies\Microsoft\Windows\Windows Copilot" /v TurnOffWindowsCopilot /t REG_DWORD /d 1 /f >nul 2>&1

:: Cortana desativar
echo     [-] Desativando Cortana...
reg add "HKLM\SOFTWARE\Policies\Microsoft\Windows\Windows Search" /v "AllowCortana" /t REG_DWORD /d 0 /f >nul 2>&1

:: Sugestões do menu iniciar
echo     [-] Removendo sugestões e anúncios...
reg add "HKCU\Software\Microsoft\Windows\CurrentVersion\ContentDeliveryManager" /v "SystemPaneSuggestionsEnabled" /t REG_DWORD /d 0 /f >nul 2>&1
reg add "HKCU\Software\Microsoft\Windows\CurrentVersion\ContentDeliveryManager" /v "SoftLandingEnabled" /t REG_DWORD /d 0 /f >nul 2>&1
reg add "HKCU\Software\Microsoft\Windows\CurrentVersion\ContentDeliveryManager" /v "RotatingLockScreenEnabled" /t REG_DWORD /d 0 /f >nul 2>&1
reg add "HKCU\Software\Microsoft\Windows\CurrentVersion\ContentDeliveryManager" /v "RotatingLockScreenOverlayEnabled" /t REG_DWORD /d 0 /f >nul 2>&1

:: Telemetria (opcional)
echo     [-] Desativando telemetria...
reg add "HKLM\SOFTWARE\Policies\Microsoft\Windows\DataCollection" /v "AllowTelemetry" /t REG_DWORD /d 0 /f >nul 2>&1

ping localhost -n 2 >nul

cls
echo.
echo  ╔═════════════════════════════════════════════════════════════════════════════╗
echo  ║                       PROCESSO CONCLUÍDO COM SUCESSO!                       ║
echo  ╚═════════════════════════════════════════════════════════════════════════════╝
echo.
echo     [+] Bloatwares limpos do sistema.
echo     [+] Copilot e Cortana desativados com sucesso.
echo     [+] Sugestões e anúncios do menu iniciar removidos.
echo.
echo     [LOG] Registro de erros em: %LOG_FILE%
echo.
echo  ───────────────────────────────────────────────────────────────────────────────
echo.
pause
goto :opcao12

:REINSTALAR
cls
echo.
echo  ╔═════════════════════════════════════════════════════════════════════════════╗
echo  ║                        REINSTALANDO APPS DO SISTEMA                         ║
echo  ╚═════════════════════════════════════════════════════════════════════════════╝
echo.
echo     Status atual: [ Fase 1: Restaurando Pacotes AppX da Microsoft ]
echo.

:: Reinstalação dos apps padrão (mais robusta)
powershell -NoProfile -ExecutionPolicy Bypass -Command "Get-AppxPackage -AllUsers | % {Add-AppxPackage -DisableDevelopmentMode -Register ($_.InstallLocation + '\AppXManifest.xml')}"

:: Se falhar, tenta pelo Windows Update
if %errorlevel% neq 0 (
    echo.
    echo     [!] A reinstalação direta falhou, tentando via Windows Update...
    powershell -NoProfile -Command "Get-AppxPackage -PackageTypeFilter Bundle -AllUsers | % {Add-AppxPackage -Register ($_.InstallLocation + '\AppXManifest.xml') -DisableDevelopmentMode}" 2>&1
)

:: Fase 2: Reativar recursos
cls
echo.
echo  ╔═════════════════════════════════════════════════════════════════════════════╗
echo  ║                        REINSTALANDO APPS DO SISTEMA                         ║
echo  ╚═════════════════════════════════════════════════════════════════════════════╝
echo.
echo     Status atual: [ Fase 2: Reativando Serviços de Interface ]
echo.

:: Reativando Copilot e Cortana
echo     [+] Reativando Copilot...
reg add "HKCU\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced" /v ShowCopilotButton /t REG_DWORD /d 1 /f >nul 2>&1
reg delete "HKLM\SOFTWARE\Policies\Microsoft\Windows\Windows Copilot" /f >nul 2>&1

echo     [+] Reativando Cortana...
reg delete "HKLM\SOFTWARE\Policies\Microsoft\Windows\Windows Search" /v "AllowCortana" /f >nul 2>&1

echo     [+] Reativando sugestões...
reg add "HKCU\Software\Microsoft\Windows\CurrentVersion\ContentDeliveryManager" /v "SystemPaneSuggestionsEnabled" /t REG_DWORD /d 1 /f >nul 2>&1

ping localhost -n 2 >nul

cls
echo.
echo  ╔═════════════════════════════════════════════════════════════════════════════╗
echo  ║                       RESTAURAÇÃO CONCLUÍDA COM SUCESSO!                    ║
echo  ╚═════════════════════════════════════════════════════════════════════════════╝
echo.
echo     [+] Aplicativos padrões do Windows reinstalados.
echo     [+] Recursos de IA e assistentes reativados.
echo.
echo     [NOTA] Alguns apps podem exigir reinicialização para reativar
echo.
echo  ───────────────────────────────────────────────────────────────────────────────
echo.
pause
goto :opcao12

:: Opção 13 - Desativar notificações do sistema :: -------------------------------------------------------------------- ::

:opcao13
cls
echo.
echo   ╔═════════════════════════════════════════════════════════════════════════════╗
echo   ║                     DESATIVANDO NOTIFICAÇÕES DO SISTEMA                     ║
echo   ╚═════════════════════════════════════════════════════════════════════════════╝
echo.
reg add "HKCU\Software\Microsoft\Windows\CurrentVersion\PushNotifications" /v ToastEnabled /t REG_DWORD /d 0 /f >nul 2>&1
reg add "HKLM\Software\Policies\Microsoft\Windows\CurrentVersion\PushNotifications" /v NoToastApplicationNotification /t REG_DWORD /d 1 /f >nul 2>&1
ping localhost -n 2 >nul

echo   Status atual: [ Notificações Desativadas com Sucesso! ]
echo.
echo    ┌────────────────────────────────────────────────────┐
echo    │ ██████████████████████████████████████████████████ │ 100%%
echo    └────────────────────────────────────────────────────┘
echo.
echo    As notificações visuais e sonoras do sistema foram desligadas.
echo.
pause
goto :menuwindows

:: Opção 14 - Desativar Cortana :: -------------------------------------------------------------------- ::

:opcao14
cls
echo.
echo   ╔═════════════════════════════════════════════════════════════════════════════╗
echo   ║                         DESATIVANDO A CORTANA                               ║
echo   ╚═════════════════════════════════════════════════════════════════════════════╝
echo.

:: Aplica a restrição na máquina local e no usuário atual
reg add "HKLM\SOFTWARE\Policies\Microsoft\Windows\Windows Search" /v "AllowCortana" /t REG_DWORD /d 0 /f >nul 2>&1
reg add "HKCU\SOFTWARE\Policies\Microsoft\Windows\Windows Search" /v "AllowCortana" /t REG_DWORD /d 0 /f >nul 2>&1

:: Fecha o processo da Cortana caso esteja ativo na memória
taskkill /f /im Cortana.exe >nul 2>&1

ping localhost -n 2 >nul
echo   Status atual: [ Cortana desativada com sucesso! ]
echo.
echo    ┌────────────────────────────────────────────────────┐
echo    │ ██████████████████████████████████████████████████ │ 100%%
echo    └────────────────────────────────────────────────────┘
echo.
echo    A assistente Cortana foi desativada e não consumirá mais RAM.
echo.
pause
cls
goto :menuwindows

:: Opção 15 - Bloqueando envio de feedback automático :: -------------------------------------------------------------------- ::

:opcao15
cls
echo.
echo   ╔═════════════════════════════════════════════════════════════════════════════╗
echo   ║                   BLOQUEANDO ENVIO DE FEEDBACK AUTOMÁTICO                   ║
echo   ╚═════════════════════════════════════════════════════════════════════════════╝
echo.

:: Desativa os períodos de amostragem do SIUF (Suas chaves originais)
reg add "HKCU\Software\Microsoft\Siuf\Rules" /v NumberOfSIUFInPeriod /t REG_DWORD /d 0 /f >nul 2>&1
reg add "HKCU\Software\Microsoft\Siuf\Rules" /v PeriodInDays /t REG_DWORD /d 0 /f >nul 2>&1

:: Bloqueios adicionais para garantir que o Windows nunca peça feedback (Garantia Extra)
reg add "HKCU\Software\Microsoft\Windows\CurrentVersion\Privacy" /v TailoredExperiencesWithDiagnosticDataEnabled /t REG_DWORD /d 0 /f >nul 2>&1
reg add "HKLM\SOFTWARE\Policies\Microsoft\Windows\DataCollection" /v DoNotShowFeedbackNotifications /t REG_DWORD /d 1 /f >nul 2>&1

ping localhost -n 2 >nul
echo   Status atual: [ Feedback Automático Bloqueado! ]
echo.
echo    ┌────────────────────────────────────────────────────┐
echo    │ ██████████████████████████████████████████████████ │ 100%%
echo    └────────────────────────────────────────────────────┘
echo.
echo    As solicitações de feedback e experiências personalizadas foram desativadas.
echo.
pause
cls
goto :menuwindows

:: Opção 16 - Desativanto smartscreen :: -------------------------------------------------------------------- ::

:opcao16
cls
echo.
echo   ╔═════════════════════════════════════════════════════════════════════════════╗
echo   ║                         DESATIVANDO O SMARTSCREEN                           ║
echo   ╚═════════════════════════════════════════════════════════════════════════════╝
echo.

:: Desativa o SmartScreen no Windows Explorer (Suas chaves originais otimizadas)
reg add "HKLM\Software\Microsoft\Windows\CurrentVersion\Explorer" /v SmartScreenEnabled /t REG_SZ /d Off /f >nul 2>&1
reg add "HKLM\Software\Microsoft\Windows\CurrentVersion\Policies\Attachments" /v ScanWithAntiVirus /t REG_DWORD /d 1 /f >nul 2>&1

:: Desativa o SmartScreen para Apps da Microsoft Store e Arquivos da Web (Windows 10/11)
reg add "HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\AppHost" /v EnableWebContentEvaluation /t REG_DWORD /d 0 /f >nul 2>&1
reg add "HKCU\Software\Microsoft\Windows\CurrentVersion\AppHost" /v EnableWebContentEvaluation /t REG_DWORD /d 0 /f >nul 2>&1

:: Desativa o SmartScreen no Microsoft Edge
reg add "HKLM\SOFTWARE\Policies\Microsoft\Edge" /v SmartScreenEnabled /t REG_DWORD /d 0 /f >nul 2>&1

ping localhost -n 2 >nul
echo   Status atual: [ SmartScreen Desativado com Sucesso! ]
echo.
echo    ┌────────────────────────────────────────────────────┐
echo    │ ██████████████████████████████████████████████████ │ 100%%
echo    └────────────────────────────────────────────────────┘
echo.
echo    O SmartScreen foi totalmente desligado para arquivos, apps e navegadores.
echo.
pause
cls
goto :menuwindows

:: Opção 17 - Desativar Ou reverter overlays :: -------------------------------------------------------------------- ::

:opcao17
cls
echo.
echo   ╔═════════════════════════════════════════════════════════════════════════════╗
echo   ║                        OTIMIZAR OU REVERTER OVERLAYS                        ║
echo   ╚═════════════════════════════════════════════════════════════════════════════╝
echo.
echo     [1] Desativar Overlays e Recursos de Jogo (Xbox Game Bar)
echo     [2] Reverter Overlays ao Padrão de Fábrica
echo     [3] Voltar ao Menu Principal
echo.
set /p escolha_overlay="   Escolha uma opção: "

if "%escolha_overlay%"=="1" goto desativar_overlay
if "%escolha_overlay%"=="2" goto reverter_overlay
if "%escolha_overlay%"=="3" goto :menuwindows
goto :opcao17

:desativar_overlay
cls
echo.
echo   ╔═════════════════════════════════════════════════════════════════════════════╗
echo   ║                         DESATIVANDO OVERLAYS DE JOGO                        ║
echo   ╚═════════════════════════════════════════════════════════════════════════════╝
echo.

:: Desativa recursos da GameBar e Modo de Jogo (Suas chaves originais)
reg add "HKCU\Software\Microsoft\GameBar" /v "AllowAutoGameMode" /t REG_DWORD /d 0 /f >nul 2>&1
reg add "HKCU\Software\Microsoft\GameBar" /v "AutoGameModeEnabled" /t REG_DWORD /d 0 /f >nul 2>&1
reg add "HKCU\Software\Microsoft\GameBar" /v "ShowStartupPanel" /t REG_DWORD /d 0 /f >nul 2>&1

:: Bloqueia o Overlay em segundo plano e a Gravação de clipes (Otimização extra de FPS)
reg add "HKCU\SOFTWARE\Microsoft\Windows\CurrentVersion\GameDVR" /v "AppCaptureEnabled" /t REG_DWORD /d 0 /f >nul 2>&1
reg add "HKCU\System\GameConfigStore" /v "GameDVR_Enabled" /t REG_DWORD /d 0 /f >nul 2>&1

ping localhost -n 2 >nul
echo   Status atual: [ Overlays Desativados com Sucesso! ]
echo.
echo    ┌────────────────────────────────────────────────────┐
echo    │ ██████████████████████████████████████████████████ │ 100%%
echo    └────────────────────────────────────────────────────┘
echo.
echo    Xbox Game Bar e capturas em segundo plano foram completamente desligados.
echo.
pause
goto :menuwindows

:reverter_overlay
cls
echo.
echo   ╔═════════════════════════════════════════════════════════════════════════════╗
echo   ║                         REVERTENDO OVERLAYS DE JOGO                         ║
echo   ╚═════════════════════════════════════════════════════════════════════════════╝
echo.

:: Reativa recursos originais do Windows
reg add "HKCU\Software\Microsoft\GameBar" /v "AllowAutoGameMode" /t REG_DWORD /d 1 /f >nul 2>&1
reg add "HKCU\Software\Microsoft\GameBar" /v "AutoGameModeEnabled" /t REG_DWORD /d 1 /f >nul 2>&1
reg add "HKCU\Software\Microsoft\GameBar" /v "ShowStartupPanel" /t REG_DWORD /d 1 /f >nul 2>&1
reg add "HKCU\SOFTWARE\Microsoft\Windows\CurrentVersion\GameDVR" /v "AppCaptureEnabled" /t REG_DWORD /d 1 /f >nul 2>&1
reg add "HKCU\System\GameConfigStore" /v "GameDVR_Enabled" /t REG_DWORD /d 1 /f >nul 2>&1

ping localhost -n 2 >nul
echo   Status atual: [ Restauração Concluída com Sucesso! ]
echo.
echo    ┌────────────────────────────────────────────────────┐
echo    │ ██████████████████████████████████████████████████ │ 100%%
echo    └────────────────────────────────────────────────────┘
echo.
echo    Os recursos de jogo e overlays retornaram ao padrão do Windows.
echo.
pause
goto :menuwindows

:: Opção 18 - Cache em miniaturas :: -------------------------------------------------------------------- ::

:opcao18
cls
echo.
echo   ╔═════════════════════════════════════════════════════════════════════════════╗
echo   ║                    RESETANDO CACHE DE ÍCONES E MINIATURAS                   ║
echo   ╚═════════════════════════════════════════════════════════════════════════════╝
echo.

:: Fecha o Windows Explorer para liberar os arquivos de cache que estão em uso
taskkill /f /im explorer.exe >nul 2>&1

:: Deleta os arquivos de cache de ícones e miniaturas (Suas chaves originais)
del /f /s /q "%LocalAppData%\Microsoft\Windows\Explorer\iconcache*" >nul 2>&1
del /f /s /q "%LocalAppData%\Microsoft\Windows\Explorer\thumbcache*" >nul 2>&1

:: Limpa o arquivo oculto clássico de cache de ícones (Garantia extra contra ícones brancos)
del /f /q "%userprofile%\AppData\Local\IconCache.db" >nul 2>&1

:: Força o sistema a atualizar os ícones e reinicia o Windows Explorer
ie4uinit.exe -show >nul 2>&1
start explorer.exe

ping localhost -n 2 >nul
echo   Status atual: [ Cache Resetado e Explorer Reiniciado! ]
echo.
echo    ┌────────────────────────────────────────────────────┐
echo    │ ██████████████████████████████████████████████████ │ 100%%
echo    └────────────────────────────────────────────────────┘
echo.
echo    O cache de miniaturas e ícones foi limpo e recriado com sucesso.
echo.
pause
cls
goto :menuwindows

:: Opção 19 - Prefetch e Superfetch :: -------------------------------------------------------------------- ::

:opcao19
cls
echo.
echo   ╔═════════════════════════════════════════════════════════════════════════════╗
echo   ║                DESATIVANDO PREFETCH E SUPERFETCH (SYSMAIN)                  ║
echo   ╚═════════════════════════════════════════════════════════════════════════════╝
echo.

:: Para e desativa o serviço SysMain (Suas linhas originais)
sc stop "SysMain" >nul 2>&1
sc config "SysMain" start= disabled >nul 2>&1

:: Altera os parâmetros de memória no Registro (Suas chaves originais)
reg add "HKLM\SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management\PrefetchParameters" /v EnablePrefetcher /t REG_DWORD /d 0 /f >nul 2>&1
reg add "HKLM\SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management\PrefetchParameters" /v EnableSuperfetch /t REG_DWORD /d 0 /f >nul 2>&1

:: Desativa funções adicionais de cache de inicialização pendentes (Garantia extra)
reg add "HKLM\SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management\PrefetchParameters" /v EnableBootTrace /t REG_DWORD /d 0 /f >nul 2>&1

ping localhost -n 2 >nul
echo   Status atual: [ SysMain e Prefetch Desativados! ]
echo.
echo    ┌────────────────────────────────────────────────────┐
echo    │ ██████████████████████████████████████████████████ │ 100%%
echo    └────────────────────────────────────────────────────┘
echo.
echo    O uso excessivo de disco (100%%) causado pelo SysMain foi mitigado.
echo.
pause
cls
goto :menuwindows

:: Opção 20 - Fechar windows explorer :: -------------------------------------------------------------------- ::

:opcao20
cls
Echo Fechando Windows Explorer...
taskkill /f /im explorer.exe
echo Concluido!
pause
cls
goto :menuwindows

:: Opção 21 - Iniciar windows explorer :: -------------------------------------------------------------------- ::

:opcao21
cls
echo Iniciando Windows Explorer...
start explorer.exe
echo Concluido!
pause
cls
goto :menuwindows

:: Opção 22 - Desativar UAC :: -------------------------------------------------------------------- ::

:opcao22
cls
echo Desativando UAC...
reg add "HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System" /v EnableLUA /t REG_DWORD /d 0 /f
Echo Desativado com Sucesso!
pause
cls
goto :menuwindows

:: Opção 23 - Desativar hyper-v :: -------------------------------------------------------------------- ::

:opcao23
cls
echo Desativando Hyper-V...
dism /Online /Disable-Feature:Microsoft-Hyper-V-All /NoRestart
bcdedit /set hypervisorlaunchtype off
Echo Desativado com Sucesso!
pause
cls
goto :menuwindows

:: Opção 24 - Arrumando windows :: -------------------------------------------------------------------- ::

:opcao24
cls
echo.
echo   ╔═════════════════════════════════════════════════════════════════════════════╗
echo   ║                  VERIFICAÇÃO E REPARO DE ARQUIVOS DO SYSTEM                 ║
echo   ╚═════════════════════════════════════════════════════════════════════════════╝
echo.
echo   [ Etapa 1 de 3 ] Executando SFC (Verificador de Arquivos do Sistema)...
echo   ┌────────────────────────────────────────────────────┐
echo   │ ██████████████░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░ │ 30%%
echo   └────────────────────────────────────────────────────┘
echo.
sfc /scannow
echo.
echo ───────────────────────────────────────────────────────────────────────────────
echo.
echo   [ Etapa 2 de 3 ] Executando DISM (Reparo da Imagem do Windows)...
echo   ┌────────────────────────────────────────────────────┐
echo   │ ████████████████████████████████░░░░░░░░░░░░░░░░░░ │ 65%%
echo   └────────────────────────────────────────────────────┘
echo.
dism /online /cleanup-image /restorehealth
echo.
echo ───────────────────────────────────────────────────────────────────────────────
echo.
echo   [ Etapa 3 de 3 ] Executando CHKDSK (Verificação de Erros no Disco)...
echo   ┌────────────────────────────────────────────────────┐
echo   │ ██████████████████████████████████████████████░░░░ │ 90%%
echo   └────────────────────────────────────────────────────┘
echo.
:: O 'echo Y |' responde automaticamente "Sim" caso o Windows pergunte se deseja agendar a verificação para o próximo boot.
:: O '/X' força a desmontagem do volume se necessário.
echo Y | chkdsk C: /F /R /X
echo.
echo ───────────────────────────────────────────────────────────────────────────────
ping localhost -n 2 >nul
cls

echo.
echo   ╔═════════════════════════════════════════════════════════════════════════════╗
echo   ║                    DIAGNÓSTICO E REPARO CONCLUÍDOS!                         ║
echo   ╚═════════════════════════════════════════════════════════════════════════════╝
echo.
echo   Status atual: [ Sistema verificado com sucesso! ]
echo.
echo    ┌────────────────────────────────────────────────────┐
echo    │ ██████████████████████████████████████████████████ │ 100%%
echo    └────────────────────────────────────────────────────┘
echo.
echo    Nota: Se o CHKDSK agendou uma checagem, ela ocorrerá na próxima reinicialização.
echo.
pause
cls
goto :menuwindows

:: Opção 25 - Limpando cache da rede :: -------------------------------------------------------------------- ::

:opcao25
cls
echo.
echo   ╔═════════════════════════════════════════════════════════════════════════════╗
echo   ║                         LIMPEZA PROFUNDA DO CACHE DE REDE                   ║
echo   ╚═════════════════════════════════════════════════════════════════════════════╝
echo.
echo    [ Configurando ] Liberando e renovando IP do adaptador...
ipconfig /release >nul 2>&1
ipconfig /renew >nul 2>&1

echo    [ Configurando ] Limpando cache DNS e tabelas ARP...
ipconfig /flushdns >nul 2>&1
arp -d * >nul 2>&1

echo    [ Configurando ] Resetando NetBIOS e catálogo Winsock...
nbtstat -R >nul 2>&1
nbtstat -RR >nul 2>&1
netsh winsock reset >nul 2>&1
netsh int ip reset >nul 2>&1

ping localhost -n 2 >nul
cls

echo.
echo   ╔═════════════════════════════════════════════════════════════════════════════╗
echo   ║                         CACHE DE REDE REPARADO!                             ║
echo   ╚═════════════════════════════════════════════════════════════════════════════╝
echo.
echo   Status atual: [ Limpeza e Reset Concluídos com Sucesso! ]
echo.
echo    ┌────────────────────────────────────────────────────┐
echo    │ ██████████████████████████████████████████████████ │ 100%%
echo    └────────────────────────────────────────────────────┘
echo.
echo    O cache DNS foi limpo, as tabelas IP resetadas e o Winsock restaurado.
echo    Nota: É altamente recomendável reiniciar o computador para aplicar os efeitos.
echo.
pause
cls
goto :menuwindows

:: Opção 26 - Limpando arquivos temporários windows :: -------------------------------------------------------------------- ::

:opcao26
cls
echo.
echo   ╔═════════════════════════════════════════════════════════════════════════════╗
echo   ║                    LIMPEZA AVANÇADA E LOGS DO SISTEMA                       ║
echo   ╚═════════════════════════════════════════════════════════════════════════════╝
echo.

:: Teste de privilégios de Administrador necessário para os Logs e Spooler
net session >nul 2>&1
if %errorlevel% neq 0 (
    echo   [ERRO]: Esta função exige privilégios de Administrador!
    echo   Por favor, reinicie o otimizador clicando com o botão direito e "Executar como Adm".
    echo.
    pause
    goto :menuwindows
)

echo    [ Executando ] Expurgando pastas temporárias de Caches...
:: Definição de variáveis seguras (aspas duplas para evitar quebras por espaços no nome de usuário)
set "windows=%windir%"
set "systemdrive=%systemdrive%"
set "userprofile=%userprofile%"
set "temp_dir=%temp%"
set "history=%userprofile%\Local Settings\History"
set "cookies=%userprofile%\Cookies"
set "recent=%userprofile%\Recent"

:: Execução da limpeza de arquivos (Suas regras originais aperfeiçoadas)
if exist "%windows%\temp\" del /s /f /q "%windows%\temp\*.*" >nul 2>&1
if exist "%windows%\Prefetch\" del /s /f /q "%windows%\Prefetch\*.exe" >nul 2>&1
if exist "%windows%\Prefetch\" del /s /f /q "%windows%\Prefetch\*.dll" >nul 2>&1
if exist "%windows%\Prefetch\" del /s /f /q "%windows%\Prefetch\*.pf" >nul 2>&1
if exist "%windows%\system32\dllcache\" del /s /f /q "%windows%\system32\dllcache\*.*" >nul 2>&1
if exist "%systemdrive%\Temp\" del /s /f /q "%systemdrive%\Temp\*.*" >nul 2>&1
if exist "%temp_dir%\" del /s /f /q "%temp_dir%\*.*" >nul 2>&1
if exist "%history%\" del /s /f /q "%history%\*.*" >nul 2>&1
if exist "%userprofile%\Local Settings\Temporary Internet Files\" del /s /f /q "%userprofile%\Local Settings\Temporary Internet Files\*.*" >nul 2>&1
if exist "%userprofile%\Local Settings\Temp\" del /s /f /q "%userprofile%\Local Settings\Temp\*.*" >nul 2>&1
if exist "%recent%\" del /s /f /q "%recent%\*.*" >nul 2>&1
if exist "%cookies%\" del /s /f /q "%cookies%\*.*" >nul 2>&1

echo    [ Executando ] Limpando logs e histórico do Visualizador de Eventos...
:: Loop corrigido de forma segura para limpar os logs sem fechar o script prematuramente
for /F "tokens=*" %%G in ('wevtutil.exe el') do wevtutil.exe cl "%%G" >nul 2>&1

echo    [ Executando ] Ativando assistente de armazenamento do Windows...
:: Executa a Limpeza de Disco nativa focada em atualizações antigas (Windows Update) de forma silenciosa
cleanmgr /autoclean >nul 2>&1

ping localhost -n 2 >nul
cls

echo.
echo   ╔═════════════════════════════════════════════════════════════════════════════╗
echo   ║                         SISTEMA TOTALMENTE LIMPO!                           ║
echo   ╚═════════════════════════════════════════════════════════════════════════════╝
echo.
echo   Status atual: [ Limpeza Profunda Concluída com Sucesso! ]
echo.
echo    ┌────────────────────────────────────────────────────┐
echo    │ ██████████████████████████████████████████████████ │ 100%%
echo    └────────────────────────────────────────────────────┘
echo.
echo    Arquivos inúteis removidos e logs esvaziados. Espaço em disco recuperado!
echo.
pause
cls
goto :menuwindows

:: Opção 27 - Desativar windows defender :: -------------------------------------------------------------------- ::

:opcao27
cls
echo Desativando Windows Defender (Incluindo Anti-Malware Executables)...
reg add "HKLM\SYSTEM\CurrentControlSet\Services\wdboot" /v Start /t REG_DWORD /d 4 /f
reg add "HKLM\SYSTEM\CurrentControlSet\Services\wdfilter" /v Start /t REG_DWORD /d 4 /f
reg add "HKLM\SYSTEM\CurrentControlSet\Services\WinDefend" /v Start /t REG_DWORD /d 4 /f
reg add "HKLM\SYSTEM\CurrentControlSet\Services\SecurityHealthService" /v Start /t REG_DWORD /d 4 /f
reg add "HKLM\SYSTEM\CurrentControlSet\Services\wdnisdrv" /v Start /t REG_DWORD /d 4 /f
reg add "HKLM\SYSTEM\CurrentControlSet\Services\mssecflt" /v Start /t REG_DWORD /d 4 /f
reg add "HKLM\SYSTEM\CurrentControlSet\Services\WdNisSvc" /v Start /t REG_DWORD /d 4 /f
reg add "HKLM\SYSTEM\CurrentControlSet\Services\Sense" /v Start /t REG_DWORD /d 4 /f
reg add "HKLM\SYSTEM\CurrentControlSet\Services\wscsvc" /v Start /t REG_DWORD /d 4 /f
reg add "HKLM\SOFTWARE\Policies\Microsoft\Windows Defender" /v DisableAntiSpyware /t REG_DWORD /d 1 /f
reg add "HKLM\SOFTWARE\Policies\Microsoft\Windows Defender" /v DisableRoutinelyTakingAction /t REG_DWORD /d 1 /f
reg add "HKLM\SOFTWARE\Policies\Microsoft\Windows Defender" /v ServiceKeepAlive /t REG_DWORD /d 0 /f
reg add "HKLM\SOFTWARE\Policies\Microsoft\Windows Defender\Real-Time Protection" /v DisableBehaviorMonitoring /t REG_DWORD /d 1 /f
reg add "HKLM\SOFTWARE\Policies\Microsoft\Windows Defender\Real-Time Protection" /v DisableIOAVProtection /t REG_DWORD /d 1 /f
reg add "HKLM\SOFTWARE\Policies\Microsoft\Windows Defender\Real-Time Protection" /v DisableOnAccessProtection /t REG_DWORD /d 1 /f
reg add "HKLM\SOFTWARE\Policies\Microsoft\Windows Defender\Real-Time Protection" /v DisableRealtimeMonitoring /t REG_DWORD /d 1 /f
reg add "HKLM\SOFTWARE\Policies\Microsoft\Windows Defender\Reporting" /v DisableEnhancedNotifications /t REG_DWORD /d 1 /f
reg add "HKLM\SOFTWARE\Policies\Microsoft\Windows Defender Security Center\Notifications" /v DisableNotifications /t REG_DWORD /d 1 /f
reg add "HKCU\Software\Policies\Microsoft\Windows\CurrentVersion\PushNotifications" /v NoToastApplicationNotification /t REG_DWORD /d 1 /f
reg add "HKCU\Software\Policies\Microsoft\Windows\CurrentVersion\PushNotifications" /v NoToastApplicationNotificationOnLockScreen /t REG_DWORD /d 1 /f
Echo Desativado com Sucesso!
pause
cls
goto :menuwindows

:: Opção 28 - Desativar download maps manager :: -------------------------------------------------------------------- ::

:opcao28
cls
echo Desativando Download Maps Manager...
reg add "HKLM\SYSTEM\CurrentControlSet\Services\MapsBroker" /v Start /t REG_DWORD /d 4 /f
Echo Desativado com Sucesso!
pause
cls
goto :menuwindows

:: Opção 29 - Desativar TimeStamp :: -------------------------------------------------------------------- ::

:opcao29
cls
echo Desativando TimeStamp...
reg add "HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\FileSystem" /v NtfsDisableLastAccessUpdate /t REG_DWORD /d 1 /f
Echo Desativado com Sucesso!
pause
cls
goto :menuwindows

:: Opção 30 - Reinicar PC :: -------------------------------------------------------------------- ::

:opcao30
cls
echo.
echo   ╔═════════════════════════════════════════════════════════════════════════════╗
echo   ║                         REINICIALIZAÇÃO DO SISTEMA                          ║
echo   ╚═════════════════════════════════════════════════════════════════════════════╝
echo.
echo     Muitas alterações exigem a reinicialização para entrar em vigor.
echo     Deseja reiniciar o computador agora?
echo.
echo     [1] Sim, reiniciar agora (Recomendado)
echo     [2] Não, voltar ao menu principal
echo.
set /p resposta="   Digite o número da opção: "

if "%resposta%"=="1" goto :confirmar_reiniciar
goto :cancelar_reiniciar

:confirmar_reiniciar
cls
echo.
echo   ╔═════════════════════════════════════════════════════════════════════════════╗
echo   ║                         REINICIANDO O COMPUTADOR...                         ║
echo   ╚═════════════════════════════════════════════════════════════════════════════╝
echo.
echo   Status atual: [ O Windows será reiniciado em 5 segundos! ]
echo.
echo    ┌────────────────────────────────────────────────────┐
echo    │ ██████████████████████████████████████████████████ │ 100%%
echo    └────────────────────────────────────────────────────┘
echo.
echo    Salvando configurações do otimizador e fechando o sistema...
echo.
:: O parâmetro /c adiciona uma mensagem personalizada na tela azul de reinício do Windows
shutdown /r /t 5 /c "Otimizador finalizado com sucesso! Reiniciando para aplicar as alteracoes." >nul 2>&1
pause
exit

:cancelar_reiniciar
cls
echo.
echo   ╔═════════════════════════════════════════════════════════════════════════════╗
echo   ║                        REINICIALIZAÇÃO ADIADA                               ║
echo   ╚═════════════════════════════════════════════════════════════════════════════╝
echo.
echo   Status atual: [ Operação cancelada pelo usuário ]
echo.
echo    ┌────────────────────────────────────────────────────┐
echo    │ ░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░ │ 0%%
echo    └────────────────────────────────────────────────────┘
echo.
echo    Lembre-se de reiniciar manualmente mais tarde para aplicar as otimizações.
echo.
pause
cls
goto :menuwindows

:: Opção 31 - Voltar para o menu :: -------------------------------------------------------------------- ::

:opcao31
cls
goto :menu

:: Prioridade Dos Jogos :: -------------------------------------------------------------------- ::

:prioridadegames
cls
set "ESC="
cls
echo(
set "lines[0]=                        ________                                      "
set "lines[1]=                       /  _____/_____    ____   ____   ______"
set "lines[2]=                      /   \  ___\__  \  /    \_/ __ \ /  ___/"
set "lines[3]=                      \    \_\  \/ __ \|    Y Y  \  ___/ \___ \ "
set "lines[4]=                       \______  (____  /__|_|  /\___  >____  >"
set "lines[5]=                              \/     \/      \/     \/     \/ "

for /L %%j in (0,1,110) do (
set /a "corR=corBaseR + (variacaoR * %%j / 82)"
set /a "corG=corBaseG + (variacaoG * %%j / 82)"
set /a "corB=corBaseB + (variacaoB * %%j / 82)"
set "esc[%%j]=!ESC![38;2;!corR!;!corG!;!corB!m"
)

for /L %%i in (0,1,5) do (
set "texto=!lines[%%i]!"
set "textoGradiente="
for /L %%j in (0,1,82) do (
set "char=!texto:~%%j,1!"
if "!char!" == " " set "char= "
set "textoGradiente=!textoGradiente!!esc[%%j]!!char!"
)
echo( !textoGradiente!!ESC![0m
)

echo.
echo                          Escolha o %op%jogo%w% que voce quer %op%priorizar%w%:
echo.
echo           %m%[ %m%1 %m%]%w% Fortnite                                 %m%[ %m%2 %m%]%w% Gta V
echo.
echo           %m%[ %m%3 %m%]%w% FiveM                                    %m%[ %m%4 %m%]%w% CS2
echo.
echo           %m%[ %m%5 %m%]%w% Minecraft                                %m%[ %m%6 %m%]%w% Valorant
echo.
echo           %m%[ %m%7 %m%]%w% League of Legends                        %m%[ %m%8 %m%]%w% Warzone
echo.
echo           %m%[ %m%9 %m%]%w% Apex Legends                            %m%[ %m%10 %m%]%w% Roblox
echo.
echo           %m%[ %m%11 %m%]%w% God Of War (2018 e ragnarok)            %m%[ %m%12 %m%]%w% MTA 
echo.
echo           %m%[ %m%13 %m%]%w% Euro Truck Simulator (1 e 2)            %m%[ %m%14 %m%]%w% Tom Clancy's Rainbow Six Siege
echo.   
echo           %m%[ %m%15 %m%]%w% Cult of the Lamb                        %m%[ %m%16 %m%]%w% ULTRAKILL
echo.      
echo           %m%[ %m%17 %m%]%w% Blood Strike                            %m%[ %m%18 %m%]%w% Arena Breakout
echo.    
echo           %m%[ %m%19 %m%]%w% Resident Evil 4 Remake                   %m%[ %m%20 %m%]%w% Resident Evil 2 Remake
echo.    
echo           %m%[ %m%21 %m%]%w% Resident Evil Village                    %m%[ %m%22 %m%]%w% Free Fire + Bluestacks
echo.    
echo           %m%[ %m%23 %m%]%w% Battlefield 2042                        %m%[ %m%24 %m%]%w% Battlefield 4
echo.    
echo           %m%[ %m%25 %m%]%w% The last Of US 1 e 2                    %m%[ %m%26 %m%]%w% PUBG
echo.
echo           %m%[ %m%27 %m%]%w% Rocket League                           %m%[ %m%28 %m%]%w% Cyberpunk 2077
echo.
echo           %m%[ %m%29 %m%]%w% Terraria                                 %m%[ %m%30 %m%]%w% Red Dead Redemption 2
echo.
echo           %m%[ %m%31 %m%]%w% Battlefield 6                            %m%[ %m%32 %m%]%w% Choo Choo Charles
echo.
echo           %m%[ %m%33 %m%]%w% Hell Let Loose                           %m%[ %m%34 %m%]%w% Farming Simulator 22
echo.
echo           %m%[ %m%35 %m%]%w% Farming Simulator 25                     %m%[ %m%36 %m%]%w% Hollow Knight
echo.
echo           %m%[ %m%37 %m%]%w% Genshin Impact                           %m%[ %m%38 %m%]%w% Point Blank
echo.
echo           %m%[ %m%39 %m%]%w% My Summer Car                            %m%[ %m%40 %m%]%w% DayZ
echo.
echo           %m%[ %m%41 %m%]%w% Street Fighter 6                         %m%[ %m%42 %m%]%w% Rust
echo.
echo           %m%[ %m%43 %m%]%w% Palworld                                 %m%[ %m%44 %m%]%w% Elden Ring
echo.
echo           %m%[ %m%45 %m%]%w% Dead by Daylight                        %m%[ %m%46 %m%]%w% Phasmophobia
echo.
echo           %m%[ %m%47 %m%]%w% Left 4 Dead 2                           %m%[ %m%48 %m%]%w% Garry's Mod
echo.
echo           %op%[ %op%49%op% ]%op% Entre no Discord e sugira jogos!%w%          %op%[ %op%50 %op%]%op% Voltar ao Menu Principal%w%
echo.
echo           %op%[ %op%51%op% ]%op% REVERTA AO PADRÃO DO WINDOWS%w%    
echo.
set /p jogo="   Digite o numero: "
cls
if "%jogo%"=="1" goto priorizar_fortnite
if "%jogo%"=="2" goto priorizar_gtav
if "%jogo%"=="3" goto priorizar_fivem
if "%jogo%"=="4" goto priorizar_cs2
if "%jogo%"=="5" goto priorizar_minecraft
if "%jogo%"=="6" goto priorizar_valorant
if "%jogo%"=="7" goto priorizar_lol
if "%jogo%"=="8" goto priorizar_warzone
if "%jogo%"=="9" goto priorizar_apex
if "%jogo%"=="10" goto priorizar_roblox
if "%jogo%"=="11" goto priorizar_gow
if "%jogo%"=="12" goto priorizar_mta
if "%jogo%"=="13" goto priorizar_ets
if "%jogo%"=="14" goto priorizar_r6
if "%jogo%"=="15" goto priorizar_cult
if "%jogo%"=="16" goto priorizar_ultrakill
if "%jogo%"=="17" goto priorizar_bloodstrike
if "%jogo%"=="18" goto priorizar_arenabreakout
if "%jogo%"=="19" goto priorizar_residentevil4remake
if "%jogo%"=="20" goto priorizar_residentevil2remake
if "%jogo%"=="21" goto priorizar_residentevilvillage
if "%jogo%"=="22" goto priorizar_freefire
if "%jogo%"=="23" goto priorizar_battlefield2042
if "%jogo%"=="24" goto priorizar_battlefield4
if "%jogo%"=="25" goto priorizar_tlol
if "%jogo%"=="26" goto priorizar_pubg
if "%jogo%"=="27" goto priorizar_rocketleague
if "%jogo%"=="28" goto priorizar_cyberpunk
if "%jogo%"=="29" goto priorizar_terraria
if "%jogo%"=="30" goto priorizar_rdr2
if "%jogo%"=="31" goto priorizar_battlefield6
if "%jogo%"=="32" goto priorizar_choochoo
if "%jogo%"=="33" goto priorizar_hll
if "%jogo%"=="34" goto priorizar_fs22
if "%jogo%"=="35" goto priorizar_fs25
if "%jogo%"=="36" goto priorizar_hollowknight
if "%jogo%"=="37" goto priorizar_genshin
if "%jogo%"=="38" goto priorizar_pointblank
if "%jogo%"=="39" goto priorizar_mysummercar
if "%jogo%"=="40" goto priorizar_dayz
if "%jogo%"=="41" goto priorizar_sf6
if "%jogo%"=="42" goto priorizar_rust
if "%jogo%"=="43" goto priorizar_palworld
if "%jogo%"=="44" goto priorizar_eldenring
if "%jogo%"=="45" goto priorizar_dbd
if "%jogo%"=="46" goto priorizar_phasmo
if "%jogo%"=="47" goto priorizar_l4d2
if "%jogo%"=="48" goto priorizar_gmod
if "%jogo%"=="49" start https://discord.gg/UufDNqWQ8j & goto :prioridadegames
if "%jogo%"=="50" goto menuwindows
if "%jogo%"=="51" goto reverterjogos
cls
goto :prioridadegames

:priorizar_fortnite
echo Aumentando prioridade do Fortnite...
reg add "HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\FortniteClient-Win64-Shipping.exe" /f >nul 2>&1
reg add "HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\FortniteClient-Win64-Shipping.exe\PerfOptions" /v CpuPriorityClass /t REG_DWORD /d 3 /f >nul 2>&1
pause
goto :prioridadegames

:priorizar_gtav
echo Aumentando prioridade do GTA V...
reg add "HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\GTA5.exe" /f >nul 2>&1
reg add "HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\GTA5.exe\PerfOptions" /v CpuPriorityClass /t REG_DWORD /d 3 /f >nul 2>&1
pause
goto :prioridadegames

:priorizar_fivem
echo Aumentando prioridade do FiveM...
reg add "HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\FiveM_b2372_GTAProcess.exe" /f >nul 2>&1
reg add "HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\FiveM_b2372_GTAProcess.exe\PerfOptions" /v CpuPriorityClass /t REG_DWORD /d 3 /f >nul 2>&1
pause
goto :prioridadegames

:priorizar_cs2
echo Aumentando prioridade do CS2...
reg add "HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\cs2.exe" /f >nul 2>&1
reg add "HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\cs2.exe\PerfOptions" /v CpuPriorityClass /t REG_DWORD /d 3 /f >nul 2>&1
pause
goto :prioridadegames

:priorizar_minecraft
echo Aumentando prioridade do Minecraft...
reg add "HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\javaw.exe" /f >nul 2>&1
reg add "HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\javaw.exe\PerfOptions" /v CpuPriorityClass /t REG_DWORD /d 3 /f >nul 2>&1
pause
goto :prioridadegames

:priorizar_valorant
echo Aumentando prioridade do Valorant...
reg add "HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\VALORANT-Win64-Shipping.exe" /f >nul 2>&1
reg add "HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\VALORANT-Win64-Shipping.exe\PerfOptions" /v CpuPriorityClass /t REG_DWORD /d 3 /f >nul 2>&1
pause
goto :prioridadegames

:priorizar_lol
echo Aumentando prioridade do League of Legends...
reg add "HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\LeagueClient.exe" /f >nul 2>&1
reg add "HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\LeagueClient.exe\PerfOptions" /v CpuPriorityClass /t REG_DWORD /d 3 /f >nul 2>&1
pause
goto :prioridadegames

:priorizar_warzone
echo Aumentando prioridade do Warzone...
reg add "HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\cod.exe" /f >nul 2>&1
reg add "HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\cod.exe\PerfOptions" /v CpuPriorityClass /t REG_DWORD /d 3 /f >nul 2>&1
pause
goto :prioridadegames

:priorizar_apex
echo Aumentando prioridade do Apex Legends...
reg add "HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\r5apex.exe" /f >nul 2>&1
reg add "HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\r5apex.exe\PerfOptions" /v CpuPriorityClass /t REG_DWORD /d 3 /f >nul 2>&1
pause
goto :prioridadegames

:priorizar_roblox
echo Aumentando prioridade do Roblox...
reg add "HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\RobloxPlayerBeta.exe" /f >nul 2>&1
reg add "HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\RobloxPlayerBeta.exe\PerfOptions" /v CpuPriorityClass /t REG_DWORD /d 3 /f >nul 2>&1
pause
goto :prioridadegames

:priorizar_gow
echo Aumentando prioridade do God of War...
reg add "HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\GoW.exe" /f >nul 2>&1
reg add "HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\GoW.exe\PerfOptions" /v CpuPriorityClass /t REG_DWORD /d 3 /f >nul 2>&1
goto :priorizar_gow_ragnarok

:priorizar_gow_ragnarok
echo Aumentando prioridade do God of War Ragnarok...
reg add "HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\GoWRagnarok.exe" /f >nul 2>&1
reg add "HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\GoWRagnarok.exe\PerfOptions" /v CpuPriorityClass /t REG_DWORD /d 3 /f >nul 2>&1
pause
goto :prioridadegames

:priorizar_mta
echo Aumentando prioridade do MTA e GTA SA...
reg add "HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\Multi Theft Auto.exe" /f >nul 2>&1
reg add "HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\Multi Theft Auto.exe\PerfOptions" /v CpuPriorityClass /t REG_DWORD /d 3 /f >nul 2>&1
reg add "HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\gta_sa.exe" /f >nul 2>&1
reg add "HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\gta_sa.exe\PerfOptions" /v CpuPriorityClass /t REG_DWORD /d 3 /f >nul 2>&1
pause
goto :prioridadegames

:priorizar_ets
echo Aumentando prioridade do Euro Truck Simulator 1 e 2...
reg add "HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\eurotrucks.exe" /f >nul 2>&1
reg add "HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\eurotrucks.exe\PerfOptions" /v CpuPriorityClass /t REG_DWORD /d 3 /f >nul 2>&1
reg add "HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\ets2.exe" /f >nul 2>&1
reg add "HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\ets2.exe\PerfOptions" /v CpuPriorityClass /t REG_DWORD /d 3 /f >nul 2>&1
pause
goto :prioridadegames

:priorizar_r6
echo Aumentando prioridade do Rainbow Six Siege...
reg add "HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\RainbowSix.exe" /f >nul 2>&1
reg add "HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\RainbowSix.exe\PerfOptions" /v CpuPriorityClass /t REG_DWORD /d 3 /f >nul 2>&1
pause
goto :prioridadegames

:priorizar_cult
echo Aumentando prioridade do Cult Of the Lamb...
reg add "HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\CultOfTheLamb.exe" /f >nul 2>&1
reg add "HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\CultOfTheLamb.exe\PerfOptions" /v CpuPriorityClass /t REG_DWORD /d 3 /f >nul 2>&1
pause
goto :prioridadegames

:priorizar_ultrakill
echo Aumentando prioridade do Ultrakill...
reg add "HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\ULTRAKILL.exe" /f >nul 2>&1
reg add "HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\ULTRAKILL.exe\PerfOptions" /v CpuPriorityClass /t REG_DWORD /d 3 /f >nul 2>&1
pause
goto :prioridadegames

:priorizar_bloodstrike
echo Aumentando prioridade do BloodStrike...
reg add "HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\BloodStrike.exe" /f >nul 2>&1
reg add "HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\BloodStrike.exe\PerfOptions" /v CpuPriorityClass /t REG_DWORD /d 3 /f >nul 2>&1
pause
goto :prioridadegames

:priorizar_arenabreakout
echo Aumentando prioridade do Arena Breakout...
reg add "HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\ArenaBreakout.exe" /f >nul 2>&1
reg add "HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\ArenaBreakout.exe\PerfOptions" /v CpuPriorityClass /t REG_DWORD /d 3 /f >nul 2>&1
pause
goto :prioridadegames

:priorizar_residentevil4remake
echo Aumentando prioridade do Resident Evil 4 Remake...
reg add "HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\re4.exe" /f >nul 2>&1
reg add "HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\re4.exe\PerfOptions" /v CpuPriorityClass /t REG_DWORD /d 3 /f >nul 2>&1
pause
goto :prioridadegames

:priorizar_residentevil2remake
echo Aumentando prioridade do Resident Evil 2 Remake...
reg add "HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\re2.exe" /f >nul 2>&1
reg add "HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\re2.exe\PerfOptions" /v CpuPriorityClass /t REG_DWORD /d 3 /f >nul 2>&1
pause
goto :prioridadegames

:priorizar_residentevilvillage
echo Aumentando prioridade do Resident Evil Village...
reg add "HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\re8.exe" /f >nul 2>&1
reg add "HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\re8.exe\PerfOptions" /v CpuPriorityClass /t REG_DWORD /d 3 /f >nul 2>&1
pause
goto :prioridadegames

:priorizar_freefire
echo Aumentando prioridade do Free Fire...
reg add "HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\HD-Player.exe" /f >nul 2>&1
reg add "HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\HD-Player.exe\PerfOptions" /v CpuPriorityClass /t REG_DWORD /d 3 /f >nul 2>&1
pause
goto :prioridadegames

:priorizar_battlefield2042
echo Aumentando prioridade do Battlefield 2042...
reg add "HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\BF2042.exe" /f >nul 2>&1
reg add "HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\BF2042.exe\PerfOptions" /v CpuPriorityClass /t REG_DWORD /d 3 /f >nul 2>&1
pause
goto :prioridadegames

:priorizar_battlefield4
echo Aumentando prioridade do Battlefield 4...
reg add "HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\bf4.exe" /f >nul 2>&1
reg add "HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\bf4.exe\PerfOptions" /v CpuPriorityClass /t REG_DWORD /d 3 /f >nul 2>&1
pause
goto :prioridadegames

:priorizar_tlol
echo Aumentando prioridade do The Last of Us...
reg add "HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\tlou-i.exe" /f >nul 2>&1
reg add "HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\tlou-i.exe\PerfOptions" /v CpuPriorityClass /t REG_DWORD /d 3 /f >nul 2>&1
reg add "HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\tlou-ii.exe" /f >nul 2>&1
reg add "HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\tlou-ii.exe\PerfOptions" /v CpuPriorityClass /t REG_DWORD /d 3 /f >nul 2>&1
pause
goto :prioridadegames

:priorizar_pubg
echo Aumentando prioridade do PUBG...
reg add "HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\tslgame.exe" /f >nul 2>&1
reg add "HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\tslgame.exe\PerfOptions" /v CpuPriorityClass /t REG_DWORD /d 3 /f >nul 2>&1
pause
goto :prioridadegames

:priorizar_rocketleague
echo Aumentando prioridade do Rocket League...
reg add "HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\RocketLeague.exe" /f >nul 2>&1
reg add "HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\RocketLeague.exe\PerfOptions" /v CpuPriorityClass /t REG_DWORD /d 3 /f >nul 2>&1
pause
goto :prioridadegames

:priorizar_cyberpunk
echo Aumentando prioridade do Cyberpunk 2077...
reg add "HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\Cyberpunk2077.exe" /f >nul 2>&1
reg add "HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\Cyberpunk2077.exe\PerfOptions" /v CpuPriorityClass /t REG_DWORD /d 3 /f >nul 2>&1
pause
goto :prioridadegames

:priorizar_terraria
echo Aumentando prioridade do Terraria...
reg add "HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\Terraria.exe" /f >nul 2>&1
reg add "HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\Terraria.exe\PerfOptions" /v CpuPriorityClass /t REG_DWORD /d 3 /f >nul 2>&1
pause
goto :prioridadegames

:priorizar_rdr2
echo Aumentando prioridade do Red Dead Redemption 2...
reg add "HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\RDR2.exe" /f >nul 2>&1
reg add "HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\RDR2.exe\PerfOptions" /v CpuPriorityClass /t REG_DWORD /d 3 /f >nul 2>&1
pause
goto :prioridadegames

:priorizar_battlefield6
echo Aumentando prioridade do Battlefield 6...
reg add "HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\BF6.exe" /f >nul 2>&1
reg add "HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\BF6.exe\PerfOptions" /v CpuPriorityClass /t REG_DWORD /d 3 /f >nul 2>&1
pause
goto :prioridadegames

:priorizar_choochoo
echo Aumentando prioridade do Choo Choo Charles...
reg add "HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\Charles.exe" /f >nul 2>&1
reg add "HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\Charles.exe\PerfOptions" /v CpuPriorityClass /t REG_DWORD /d 3 /f >nul 2>&1
pause
goto :prioridadegames

:priorizar_hll
echo Aumentando prioridade do Hell Let Loose...
reg add "HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\HLL.exe" /f >nul 2>&1
reg add "HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\HLL.exe\PerfOptions" /v CpuPriorityClass /t REG_DWORD /d 3 /f >nul 2>&1
pause
goto :prioridadegames

:priorizar_fs22
echo Aumentando prioridade do Farming Simulator 22...
reg add "HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\FarmingSimulator2022.exe" /f >nul 2>&1
reg add "HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\FarmingSimulator2022.exe\PerfOptions" /v CpuPriorityClass /t REG_DWORD /d 3 /f >nul 2>&1
pause
goto :prioridadegames

:priorizar_fs25
echo Aumentando prioridade do Farming Simulator 25...
reg add "HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\FarmingSimulator2025.exe" /f >nul 2>&1
reg add "HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\FarmingSimulator2025.exe\PerfOptions" /v CpuPriorityClass /t REG_DWORD /d 3 /f >nul 2>&1
pause
goto :prioridadegames

:priorizar_hollowknight
echo Aumentando prioridade do Hollow Knight...
reg add "HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\hollow_knight.exe" /f >nul 2>&1
reg add "HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\hollow_knight.exe\PerfOptions" /v CpuPriorityClass /t REG_DWORD /d 3 /f >nul 2>&1
pause
goto :prioridadegames

:priorizar_genshin
echo Aumentando prioridade do Genshin Impact...
reg add "HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\GenshinImpact.exe" /f >nul 2>&1
reg add "HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\GenshinImpact.exe\PerfOptions" /v CpuPriorityClass /t REG_DWORD /d 3 /f >nul 2>&1
pause
goto :prioridadegames

:priorizar_pointblank
echo Aumentando prioridade do Point Blank...
reg add "HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\PointBlank.exe" /f >nul 2>&1
reg add "HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\PointBlank.exe\PerfOptions" /v CpuPriorityClass /t REG_DWORD /d 3 /f >nul 2>&1
pause
goto :prioridadegames

:priorizar_mysummercar
echo Aumentando prioridade do My Summer Car...
reg add "HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\mysummercar.exe" /f >nul 2>&1
reg add "HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\mysummercar.exe\PerfOptions" /v CpuPriorityClass /t REG_DWORD /d 3 /f >nul 2>&1
pause
goto :prioridadegames

:priorizar_dayz
echo Aumentando prioridade do DayZ...
reg add "HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\DayZ.exe" /f >nul 2>&1
reg add "HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\DayZ.exe\PerfOptions" /v CpuPriorityClass /t REG_DWORD /d 3 /f >nul 2>&1
pause
goto :prioridadegames

:priorizar_sf6
echo Aumentando prioridade do Street Fighter 6...
reg add "HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\StreetFighter6.exe" /f >nul 2>&1
reg add "HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\StreetFighter6.exe\PerfOptions" /v CpuPriorityClass /t REG_DWORD /d 3 /f >nul 2>&1
pause
goto :prioridadegames

:priorizar_rust
echo Aumentando prioridade do Rust...
reg add "HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\RustClient.exe" /f >nul 2>&1
reg add "HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\RustClient.exe\PerfOptions" /v CpuPriorityClass /t REG_DWORD /d 3 /f >nul 2>&1
pause
goto :prioridadegames

:: NOVOS JOGOS ADICIONADOS AQUI:
:priorizar_palworld
echo Aumentando prioridade do Palworld...
reg add "HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\Palworld-Win64-Shipping.exe" /f >nul 2>&1
reg add "HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\Palworld-Win64-Shipping.exe\PerfOptions" /v CpuPriorityClass /t REG_DWORD /d 3 /f >nul 2>&1
pause
goto :prioridadegames

:priorizar_eldenring
echo Aumentando prioridade do Elden Ring...
reg add "HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\eldenring.exe" /f >nul 2>&1
reg add "HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\eldenring.exe\PerfOptions" /v CpuPriorityClass /t REG_DWORD /d 3 /f >nul 2>&1
pause
goto :prioridadegames

:priorizar_dbd
echo Aumentando prioridade do Dead by Daylight...
reg add "HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\DeadByDaylight-Win64-Shipping.exe" /f >nul 2>&1
reg add "HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\DeadByDaylight-Win64-Shipping.exe\PerfOptions" /v CpuPriorityClass /t REG_DWORD /d 3 /f >nul 2>&1
pause
goto :prioridadegames

:priorizar_phasmo
echo Aumentando prioridade do Phasmophobia...
reg add "HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\Phasmophobia.exe" /f >nul 2>&1
reg add "HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\Phasmophobia.exe\PerfOptions" /v CpuPriorityClass /t REG_DWORD /d 3 /f >nul 2>&1
pause
goto :prioridadegames

:priorizar_l4d2
echo Aumentando prioridade do Left 4 Dead 2...
reg add "HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\left4dead2.exe" /f >nul 2>&1
reg add "HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\left4dead2.exe\PerfOptions" /v CpuPriorityClass /t REG_DWORD /d 3 /f >nul 2>&1
pause
goto :prioridadegames

:priorizar_gmod
echo Aumentando prioridade do Garry's Mod...
reg add "HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\gmod.exe" /f >nul 2>&1
reg add "HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\gmod.exe\PerfOptions" /v CpuPriorityClass /t REG_DWORD /d 3 /f >nul 2>&1
pause
goto :prioridadegames


:reverterjogos
echo Revertendo prioridade de todos os jogos ao padrao do Windows...
reg delete "HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\FortniteClient-Win64-Shipping.exe\PerfOptions" /f >nul 2>&1
reg delete "HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\GTA5.exe\PerfOptions" /f >nul 2>&1
reg delete "HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\FiveM_b2372_GTAProcess.exe\PerfOptions" /f >nul 2>&1
reg delete "HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\cs2.exe\PerfOptions" /f >nul 2>&1
reg delete "HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\javaw.exe\PerfOptions" /f >nul 2>&1
reg delete "HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\VALORANT-Win64-Shipping.exe\PerfOptions" /f >nul 2>&1
reg delete "HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\LeagueClient.exe\PerfOptions" /f >nul 2>&1
reg delete "HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\cod.exe\PerfOptions" /f >nul 2>&1
reg delete "HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\r5apex.exe\PerfOptions" /f >nul 2>&1
reg delete "HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\RobloxPlayerBeta.exe\PerfOptions" /f >nul 2>&1
reg delete "HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\GoW.exe\PerfOptions" /f >nul 2>&1
reg delete "HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\GoWRagnarok.exe\PerfOptions" /f >nul 2>&1
reg delete "HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\Multi Theft Auto.exe\PerfOptions" /f >nul 2>&1
reg delete "HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\gta_sa.exe\PerfOptions" /f >nul 2>&1
reg delete "HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\eurotrucks.exe\PerfOptions" /f >nul 2>&1
reg delete "HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\ets2.exe\PerfOptions" /f >nul 2>&1
reg delete "HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\RainbowSix.exe\PerfOptions" /f >nul 2>&1
reg delete "HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\CultOfTheLamb.exe\PerfOptions" /f >nul 2>&1
reg delete "HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\ULTRAKILL.exe\PerfOptions" /f >nul 2>&1
reg delete "HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\BloodStrike.exe\PerfOptions" /f >nul 2>&1
reg delete "HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\ArenaBreakout.exe\PerfOptions" /f >nul 2>&1
reg delete "HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\re4.exe\PerfOptions" /f >nul 2>&1
reg delete "HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\re2.exe\PerfOptions" /f >nul 2>&1
reg delete "HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\re8.exe\PerfOptions" /f >nul 2>&1
reg delete "HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\HD-Player.exe\PerfOptions" /f >nul 2>&1
reg delete "HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\BF2042.exe\PerfOptions" /f >nul 2>&1
reg delete "HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\bf4.exe\PerfOptions" /f >nul 2>&1
reg delete "HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\tlou-i.exe\PerfOptions" /f >nul 2>&1
reg delete "HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\tlou-ii.exe\PerfOptions" /f >nul 2>&1
reg delete "HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\tslgame.exe\PerfOptions" /f >nul 2>&1
reg delete "HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\RocketLeague.exe\PerfOptions" /f >nul 2>&1
reg delete "HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\Cyberpunk2077.exe\PerfOptions" /f >nul 2>&1
reg delete "HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\Terraria.exe\PerfOptions" /f >nul 2>&1
reg delete "HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\RDR2.exe\PerfOptions" /f >nul 2>&1
reg delete "HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\BF6.exe\PerfOptions" /f >nul 2>&1
reg delete "HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\Charles.exe\PerfOptions" /f >nul 2>&1
reg delete "HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\HLL.exe\PerfOptions" /f >nul 2>&1
reg delete "HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\FarmingSimulator2022.exe\PerfOptions" /f >nul 2>&1
reg delete "HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\FarmingSimulator2025.exe\PerfOptions" /f >nul 2>&1
reg delete "HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\hollow_knight.exe\PerfOptions" /f >nul 2>&1
reg delete "HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\GenshinImpact.exe\PerfOptions" /f >nul 2>&1
reg delete "HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\PointBlank.exe\PerfOptions" /f >nul 2>&1
reg delete "HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\mysummercar.exe\PerfOptions" /f >nul 2>&1
reg delete "HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\DayZ.exe\PerfOptions" /f >nul 2>&1
reg delete "HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\StreetFighter6.exe\PerfOptions" /f >nul 2>&1
reg delete "HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\RustClient.exe\PerfOptions" /f >nul 2>&1
reg delete "HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\Palworld-Win64-Shipping.exe\PerfOptions" /f >nul 2>&1
reg delete "HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\eldenring.exe\PerfOptions" /f >nul 2>&1
reg delete "HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\DeadByDaylight-Win64-Shipping.exe\PerfOptions" /f >nul 2>&1
reg delete "HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\Phasmophobia.exe\PerfOptions" /f >nul 2>&1
reg delete "HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\left4dead2.exe\PerfOptions" /f >nul 2>&1
reg delete "HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\gmod.exe\PerfOptions" /f >nul 2>&1
echo Prioridades de todos os jogos removidas com sucesso!
pause
goto :prioridadegames

:: Opção 4 - Otimizar perifericos :: -------------------------------------------------------------------- ::

:perifericos
@echo off

cls
echo  ╔════════════════════════════════════════════════════════════╗
echo  ║               PAINEL DE OTIMIZACAO DE HARDWARE             ║
echo  ╚════════════════════════════════════════════════════════════╝
echo.
echo    Escolha a opcao que voce deseja otimizar:
echo.
echo      [1] Otimizar Armazenamento HDD       [2] Otimizar Armazenamento SSD
echo      [3] Verificar Temperatura (HW)       [4] Otimizar Resposta do Teclado
echo      [5] Otimizar Precisao do Mouse       [6] NOVO: Otimizar Memoria RAM
echo      [7] NOVO: Forcar Maximo de GPU       [8] Reverter Otimizacoes de HW
echo.
echo      [9] Voltar ao Menu Principal
echo.
echo  ╚═════════════════════════════════════════════════════════════╝
echo.
set /p opcao=" Digite o numero desejada: "

:: Antes de ir para qualquer opção, voltamos o CMD para o fundo preto (0F) para não espalhar o azul
if "%opcao%"=="1" ( color 0F & goto otimizarHDD )
if "%opcao%"=="2" ( color 0F & goto otimizarSSD )
if "%opcao%"=="3" ( color 0F & goto verificarTemp )
if "%opcao%"=="4" ( color 0F & goto otimizarTeclado )
if "%opcao%"=="5" ( color 0F & goto otimizarMouse )
if "%opcao%"=="6" ( color 0F & goto otimizarRAM )
if "%opcao%"=="7" ( color 0F & goto otimizarGPU )
if "%opcao%"=="8" ( color 0F & goto reverterperifericos )
if "%opcao%"=="9" ( color 0F & endlocal & goto menuPrincipal )

echo Opcao Invalida! Tente novamente.
ping localhost -n 2 >nul
goto :perifericos

:otimizarHDD
cls
echo.
echo  ╔═════════════════════════════════════════════════════════════════════════════╗
echo  ║                            OTIMIZANDO ARMAZENAMENTO HDD                     ║
echo  ╚═════════════════════════════════════════════════════════════════════════════╝
echo.
echo    Status atual: [ Aplicando tweaks e abrindo desfragmentador... ]
echo.
echo    ┌────────────────────────────────────────────────────┐
echo    │ ██████████████████████████████░░░░░░░░░░░░░░░░░░░░ │ 60%%
echo    └────────────────────────────────────────────────────┘
fsutil behavior set disableLastAccess 2
fsutil behavior set disable8dot3 0
dfrgui.exe
ping localhost -n 2 >nul

cls
echo.
echo  ╔═════════════════════════════════════════════════════════════════════════════╗
echo  ║                            OTIMIZANDO ARMAZENAMENTO HDD                     ║
echo  ╚═════════════════════════════════════════════════════════════════════════════╝
echo.
echo    Status atual: [ Otimizacao concluida com sucesso! ]
echo.
echo    ┌────────────────────────────────────────────────────┐
echo    │ ██████████████████████████████████████████████████ │ 100%%
echo    └────────────────────────────────────────────────────┘
echo.
pause
goto :perifericos

:otimizarSSD
cls
echo.
echo  ╔═════════════════════════════════════════════════════════════════════════════╗
echo  ║                            OTIMIZANDO ARMAZENAMENTO SSD                     ║
echo  ╚═════════════════════════════════════════════════════════════════════════════╝
echo.
echo    Status atual: [ Desativando agendamentos e limpando arquivos... ]
echo.
echo    ┌────────────────────────────────────────────────────┐
echo    │ ██████████████████████████████░░░░░░░░░░░░░░░░░░░░ │ 60%%
echo    └────────────────────────────────────────────────────┘
fsutil behavior set disableLastAccess 0
fsutil behavior set disable8dot3 1
cleanmgr.exe
ping localhost -n 2 >nul

cls
echo.
echo  ╔═════════════════════════════════════════════════════════════════════════════╗
echo  ║                            OTIMIZANDO ARMAZENAMENTO SSD                     ║
echo  ╚═════════════════════════════════════════════════════════════════════════════╝
echo.
echo    Status atual: [ Otimizacao concluida com sucesso! ]
echo.
echo    ┌────────────────────────────────────────────────────┐
echo    │ ██████████████████████████████████████████████████ │ 100%%
echo    └────────────────────────────────────────────────────┘
echo.
pause
goto :perifericos

:verificarTemp
cls
echo.
echo  ╔═════════════════════════════════════════════════════════════════════════════╗
echo  ║                            MONITORAMENTO DE TEMPERATURA                     ║
echo  ╚═════════════════════════════════════════════════════════════════════════════╝
echo.
echo    Status atual: [ Inicializando OpenHardwareMonitor... ]
echo.
echo    ┌────────────────────────────────────────────────────┐
echo    │ ██████████████████████████████████████████████░░░░ │ 90%%
echo    └────────────────────────────────────────────────────┘
start "" "%~dp0OpenHardwareMonitor.exe"
ping localhost -n 2 >nul
echo.
pause
goto :perifericos

:otimizarTeclado
cls
echo.
echo  ╔═════════════════════════════════════════════════════════════════════════════╗
echo  ║                            OTIMIZANDO RESPOSTA DO TECLADO                   ║
echo  ╚═════════════════════════════════════════════════════════════════════════════╝
echo.
echo    Status atual: [ Reduzindo delays e aplicando FilterKeys... ]
echo.
echo    ┌────────────────────────────────────────────────────┐
echo    │ ██████████████████████████████░░░░░░░░░░░░░░░░░░░░ │ 60%%
echo    └────────────────────────────────────────────────────┘
reg add "HKCU\Control Panel\Keyboard" /v KeyboardDelay /t REG_SZ /d 0 /f >nul
reg add "HKCU\Control Panel\Keyboard" /v KeyboardSpeed /t REG_SZ /d 31 /f >nul


cls
echo.
echo  ╔═════════════════════════════════════════════════════════════════════════════╗
echo  ║                            OTIMIZANDO RESPOSTA DO TECLADO                   ║
echo  ╚═════════════════════════════════════════════════════════════════════════════╝
echo.
echo    Status atual: [ Teclado otimizado com sucesso! ]
echo.
echo    ┌────────────────────────────────────────────────────┐
echo    │ ██████████████████████████████████████████████████ │ 100%%
echo    └────────────────────────────────────────────────────┘
echo.
pause
goto :perifericos

:otimizarMouse
cls
echo.
echo  ╔═════════════════════════════════════════════════════════════════════════════╗
echo  ║                            OTIMIZANDO PRECISÃO DO MOUSE                     ║
echo  ╚═════════════════════════════════════════════════════════════════════════════╝
echo.
echo    Status atual: [ Removendo aceleracao e rastro do ponteiro... ]
echo.
echo    ┌────────────────────────────────────────────────────┐
echo    │ ██████████████████████████████░░░░░░░░░░░░░░░░░░░░ │ 60%%
echo    └────────────────────────────────────────────────────┘
reg add "HKCU\Control Panel\Mouse" /v MouseSpeed /t REG_SZ /d 0 /f >nul
reg add "HKCU\Control Panel\Mouse" /v MouseThreshold1 /t REG_SZ /d 0 /f >nul
reg add "HKCU\Control Panel\Mouse" /v MouseThreshold2 /t REG_SZ /d 0 /f >nul
reg add "HKCU\Control Panel\Desktop" /v MouseTrails /t REG_SZ /d 0 /f >nul
RUNDLL32.EXE USER32.DLL,UpdatePerUserSystemParameters ,1 ,True >nul
ping localhost -n 2 >nul

cls
echo.
echo  ╔═════════════════════════════════════════════════════════════════════════════╗
echo  ║                            OTIMIZANDO PRECISÃO DO MOUSE                     ║
echo  ╚═════════════════════════════════════════════════════════════════════════════╝
echo.
echo    Status atual: [ Mouse otimizado com sucesso! ]
echo.
echo    ┌────────────────────────────────────────────────────┐
echo    │ ██████████████████████████████████████████████████ │ 100%%
echo    └────────────────────────────────────────────────────┘
echo.
pause
goto :perifericos

:otimizarRAM
cls
echo.
echo  ╔═════════════════════════════════════════════════════════════════════════════╗
echo  ║                            OTIMIZANDO MEMÓRIA RAM                           ║
echo  ╚═════════════════════════════════════════════════════════════════════════════╝
echo.
echo    Status atual: [ Ajustando PagingExecutive e LargeSystemCache... ]
echo.
echo    ┌────────────────────────────────────────────────────┐
echo    │ ██████████████████████████████░░░░░░░░░░░░░░░░░░░░ │ 60%%
echo    └────────────────────────────────────────────────────┘
reg add "HKLM\SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management" /v "DisablePagingExecutive" /t REG_DWORD /d 1 /f >nul 2>&1
reg add "HKLM\SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management" /v "LargeSystemCache" /t REG_DWORD /d 1 /f >nul 2>&1
ping localhost -n 2 >nul

cls
echo.
echo  ╔═════════════════════════════════════════════════════════════════════════════╗
echo  ║                            OTIMIZANDO MEMÓRIA RAM                           ║
echo  ╚═════════════════════════════════════════════════════════════════════════════╝
echo.
echo    Status atual: [ Memoria RAM otimizada com sucesso! ]
echo.
echo    ┌────────────────────────────────────────────────────┐
echo    │ ██████████████████████████████████████████████████ │ 100%%
echo    └────────────────────────────────────────────────────┘
echo.
pause
goto :perifericos

:otimizarGPU
cls
echo.
echo  ╔═════════════════════════════════════════════════════════════════════════════╗
echo  ║                            FORÇANDO MÁXIMO DA GPU                           ║
echo  ╚═════════════════════════════════════════════════════════════════════════════╝
echo.
echo    Status atual: [ Ativando HAGS e gerenciamento de energia maxima... ]
echo.
echo    ┌────────────────────────────────────────────────────┐
echo    │ ██████████████████████████████░░░░░░░░░░░░░░░░░░░░ │ 60%%
echo    └────────────────────────────────────────────────────┘
reg add "HKLM\SYSTEM\CurrentControlSet\Control\GraphicsDrivers" /v "HwSchMode" /t REG_DWORD /d 2 /f >nul 2>&1
reg add "HKLM\SYSTEM\CurrentControlSet\Control\GraphicsDrivers\Power" /v "PowerPerformanceMode" /t REG_DWORD /d 1 /f >nul 2>&1
ping localhost -n 2 >nul

cls
echo.
echo  ╔═════════════════════════════════════════════════════════════════════════════╗
echo  ║                            FORÇANDO MÁXIMO DA GPU                           ║
echo  ╚═════════════════════════════════════════════════════════════════════════════╝
echo.
echo    Status atual: [ GPU otimizada com sucesso! ]
echo.
echo    ┌────────────────────────────────────────────────────┐
echo    │ ██████████████████████████████████████████████████ │ 100%%
echo    └────────────────────────────────────────────────────┘
echo.
pause
goto :perifericos


:reverterperifericos
cls
echo.
echo  ╔═════════════════════════════════════════════════════════════════════════════╗
echo  ║                         PAINEL DE REVERSÃO DE HARDWARE                      ║
echo  ╚═════════════════════════════════════════════════════════════════════════════╝
echo.
echo    Escolha o recurso que deseja reverter para o padrao:
echo.
echo      [1] Reverter Mouse                   [2] Reverter Teclado
echo      [3] Reverter SSD                     [4] Reverter HDD
echo.
echo      [5] Reverter TODOS                   [0] Voltar ao Menu Anterior
echo.
echo  ╚═════════════════════════════════════════════════════════════════════════════╝
echo.
set /p opcao=" Digite o numero desejado: "
cls

if "%opcao%"=="1" goto revert_mouse
if "%opcao%"=="2" goto revert_teclado
if "%opcao%"=="3" goto revert_ssd
if "%opcao%"=="4" goto revert_hdd
if "%opcao%"=="5" goto reverter_tudo
if "%opcao%"=="0" goto perifericos
goto reverterperifericos

:revert_mouse
cls
echo.
echo  ╔═════════════════════════════════════════════════════════════════════════════╗
echo  ║                            REVERTENDO CONFIGURAÇÕES DO MOUSE                ║
echo  ╚═════════════════════════════════════════════════════════════════════════════╝
echo.
echo    Status atual: [ Restaurando valores originais do registro... ]
echo.
echo    ┌────────────────────────────────────────────────────┐
echo    │ ██████████████████████████████████████████████░░░░ │ 90%%
echo    └────────────────────────────────────────────────────┘
reg add "HKCU\Control Panel\Mouse" /v MouseSpeed /t REG_SZ /d 1 /f >nul
reg add "HKCU\Control Panel\Mouse" /v MouseThreshold1 /t REG_SZ /d 6 /f >nul
reg add "HKCU\Control Panel\Mouse" /v MouseThreshold2 /t REG_SZ /d 10 /f >nul
reg add "HKCU\Control Panel\Desktop" /v MouseTrails /t REG_SZ /d -1 /f >nul
RUNDLL32.EXE USER32.DLL,UpdatePerUserSystemParameters ,1 ,True >nul
ping localhost -n 2 >nul
echo.
echo    Mouse restaurado com sucesso!
echo.
pause
goto reverterperifericos

:revert_teclado
cls
echo.
echo  ╔═════════════════════════════════════════════════════════════════════════════╗
echo  ║                           REVERTENDO CONFIGURAÇÕES DO TECLADO               ║
echo  ╚═════════════════════════════════════════════════════════════════════════════╝
echo.
echo    Status atual: [ Restaurando valores originais do registro... ]
echo.
echo    ┌────────────────────────────────────────────────────┐
echo    │ ██████████████████████████████████████████████░░░░ │ 90%%
echo    └────────────────────────────────────────────────────┘
reg add "HKCU\Control Panel\Keyboard" /v KeyboardDelay /t REG_SZ /d 1 /f >nul
reg add "HKCU\Control Panel\Keyboard" /v KeyboardSpeed /t REG_SZ /d 20 /f >nul
ping localhost -n 2 >nul
echo.
echo    Teclado restaurado com sucesso!
echo.
pause
goto reverterperifericos

:revert_ssd
cls
echo.
echo  ╔═════════════════════════════════════════════════════════════════════════════╗
echo  ║                             REVERTENDO CONFIGURAÇÕES DO SSD                 ║
echo  ╚═════════════════════════════════════════════════════════════════════════════╝
echo.
echo    Status atual: [ Reativando tarefas agendadas e acessos... ]
echo.
echo    ┌────────────────────────────────────────────────────┐
echo    │ ██████████████████████████████████████████████░░░░ │ 90%%
echo    └────────────────────────────────────────────────────┘
schtasks /Change /TN "\Microsoft\Windows\Defrag\ScheduledDefrag" /Enable >nul
fsutil behavior set disableLastAccess 1 >nul
fsutil behavior set disable8dot3 2 >nul
ping localhost -n 2 >nul
echo.
echo    Configuracoes do SSD restauradas com sucesso!
echo.
pause
goto reverterperifericos

:revert_hdd
cls
echo.
echo  ╔═════════════════════════════════════════════════════════════════════════════╗
echo  ║                             REVERTENDO CONFIGURAÇÕES DO HDD                 ║
echo  ╚═════════════════════════════════════════════════════════════════════════════╝
echo.
echo    Status atual: [ Restaurando indexacao e comportamento de arquivos... ]
echo.
echo    ┌────────────────────────────────────────────────────┐
echo    │ ██████████████████████████████████████████████░░░░ │ 90%%
echo    └────────────────────────────────────────────────────┘
fsutil behavior set disableLastAccess 0 >nul
fsutil behavior set disable8dot3 1 >nul
ping localhost -n 2 >nul
echo.
echo    Configuracoes do HDD restauradas com sucesso!
echo.
pause
goto reverterperifericos

:reverter_tudo
cls
echo.
echo  ╔═════════════════════════════════════════════════════════════════════════════╗
echo  ║                            REVERTENDO TODAS AS OTIMIZAÇÕES                  ║
echo  ╚═════════════════════════════════════════════════════════════════════════════╝
echo.
echo    Status atual: [ Executando restauracao completa do sistema... ]
echo.
echo    ┌────────────────────────────────────────────────────┐
echo    │ ████████████████████░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░ │ 40%%
echo    └────────────────────────────────────────────────────┘
:: Chama as funções internas silenciando retornos repetidos
reg add "HKCU\Control Panel\Mouse" /v MouseSpeed /t REG_SZ /d 1 /f >nul
reg add "HKCU\Control Panel\Mouse" /v MouseThreshold1 /t REG_SZ /d 6 /f >nul
reg add "HKCU\Control Panel\Mouse" /v MouseThreshold2 /t REG_SZ /d 10 /f >nul
reg add "HKCU\Control Panel\Desktop" /v MouseTrails /t REG_SZ /d -1 /f >nul
RUNDLL32.EXE USER32.DLL,UpdatePerUserSystemParameters ,1 ,True >nul

cls
echo.
echo  ╔═════════════════════════════════════════════════════════════════════════════╗
echo  ║                            REVERTENDO TODAS AS OTIMIZAÇÕES                  ║
echo  ╚═════════════════════════════════════════════════════════════════════════════╝
echo.
echo    Status atual: [ Restaurando Teclado e Discos... ]
echo.
echo    ┌────────────────────────────────────────────────────┐
echo    │ ████████████████████████████████████████░░░░░░░░░░ │ 80%%
echo    └────────────────────────────────────────────────────┘
reg add "HKCU\Control Panel\Keyboard" /v KeyboardDelay /t REG_SZ /d 1 /f >nul
reg add "HKCU\Control Panel\Keyboard" /v KeyboardSpeed /t REG_SZ /d 20 /f >nul
schtasks /Change /TN "\Microsoft\Windows\Defrag\ScheduledDefrag" /Enable >nul
fsutil behavior set disableLastAccess 1 >nul
fsutil behavior set disable8dot3 2 >nul
fsutil behavior set disableLastAccess 0 >nul
fsutil behavior set disable8dot3 1 >nul
ping localhost -n 2 >nul

cls
echo.
echo  ╔═════════════════════════════════════════════════════════════════════════════╗
echo  ║                            REVERTENDO TODAS AS OTIMIZAÇÕES                  ║
echo  ╚═════════════════════════════════════════════════════════════════════════════╝
echo.
echo    Status atual: [ Todos os padrões originais foram redefinidos! ]
echo.
echo    ┌────────────────────────────────────────────────────┐
echo    │ ██████████████████████████████████████████████████ │ 100%%
echo    └────────────────────────────────────────────────────┘
echo.
pause
goto reverterperifericos


:revertertudo
start https://youtu.be/_Mc3urSaUL8?feature=shared
pause
cls
goto :menu

:autorun
cls
echo.
echo  ╔═════════════════════════════════════════════════════════════════════════════╗
echo  ║                          CONFIGURANDO INICIALIZAÇÃO                         ║
echo  ╚═════════════════════════════════════════════════════════════════════════════╝
echo.
echo    Status atual: [ Abrindo ferramenta Autoruns... ]
echo.
echo    ┌────────────────────────────────────────────────────┐
echo    │ ██████████████████████████████████████████████░░░░ │ 90%%
echo    └────────────────────────────────────────────────────┘
start "" "%~dp0Autoruns.exe"
pause
cls
goto menu

:tempera
cls
echo.
echo  ╔═════════════════════════════════════════════════════════════════════════════╗
echo  ║                            MONITORAMENTO DE TEMPERATURA                     ║
echo  ╚═════════════════════════════════════════════════════════════════════════════╝
echo.
echo    Status atual: [ Inicializando OpenHardwareMonitor... ]
echo.
echo    ┌────────────────────────────────────────────────────┐
echo    │ ██████████████████████████████████████████████░░░░ │ 90%%
echo    └────────────────────────────────────────────────────┘
start "" "%~dp0OpenHardwareMonitor.exe"
pause
cls
goto :menu