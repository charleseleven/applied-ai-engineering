@echo off
REM ==========================================
REM PASSO A PASSO - INSTALAÇÃO COMPLETA
REM Execute este arquivo e siga as instruções
REM ==========================================

echo.
echo ========================================================================
echo    SETUP GROQ + LLAMA-4 - INSTALACAO COMPLETA
echo ========================================================================
echo.
echo Este script instalara todos os pacotes e configurara o projeto.
echo.
echo Pressione qualquer tecla para comecar...
pause >nul
cls

REM ==========================================
REM PASSO 1: Navegar para o projeto
REM ==========================================

echo.
echo ========================================================================
echo PASSO 1/5: Navegando para o diretorio do projeto
echo ========================================================================
echo.

if not exist "AgilePredict\AgilePredict.csproj" (
	echo [ERRO] Arquivo AgilePredict.csproj nao encontrado!
	echo.
	echo Certifique-se de executar este script na pasta raiz do projeto.
	echo.
	pause
	exit /b 1
)

cd AgilePredict
echo [OK] Diretorio correto!
echo.

REM ==========================================
REM PASSO 2: Instalar Polly
REM ==========================================

echo.
echo ========================================================================
echo PASSO 2/5: Instalando Microsoft.Extensions.Http.Polly
echo ========================================================================
echo.
echo Aguarde...
echo.

dotnet add package Microsoft.Extensions.Http.Polly

if %errorlevel% neq 0 (
	echo.
	echo [ERRO] Falha ao instalar Polly!
	echo.
	pause
	exit /b 1
)

echo.
echo [OK] Polly instalado com sucesso!
echo.
timeout /t 2 /nobreak >nul

REM ==========================================
REM PASSO 3: Instalar Options.DataAnnotations
REM ==========================================

echo.
echo ========================================================================
echo PASSO 3/5: Instalando Microsoft.Extensions.Options.DataAnnotations
echo ========================================================================
echo.
echo Aguarde...
echo.

dotnet add package Microsoft.Extensions.Options.DataAnnotations

if %errorlevel% neq 0 (
	echo.
	echo [ERRO] Falha ao instalar Options.DataAnnotations!
	echo.
	pause
	exit /b 1
)

echo.
echo [OK] Options.DataAnnotations instalado com sucesso!
echo.
timeout /t 2 /nobreak >nul

REM ==========================================
REM PASSO 4: Configurar Secret Manager
REM ==========================================

echo.
echo ========================================================================
echo PASSO 4/5: Configurando Secret Manager com API Key da Groq
echo ========================================================================
echo.

set GROQ_API_KEY=YOUR_GROQ_API_KEY_HERE

echo Inicializando Secret Manager...
dotnet user-secrets init 2>nul

echo.
echo Configurando API Key...
dotnet user-secrets set "LlmSettings:ApiKey" "%GROQ_API_KEY%"

echo.
echo Configurando API URL...
dotnet user-secrets set "LlmSettings:ApiUrl" "https://api.groq.com/openai"

echo.
echo Configurando modelo...
dotnet user-secrets set "LlmSettings:DefaultModel" "meta-llama/llama-4-scout-17b-16e-instruct"

echo.
echo Configurando timeout...
dotnet user-secrets set "LlmSettings:TimeoutSeconds" "30"

echo.
echo Configurando retries...
dotnet user-secrets set "LlmSettings:MaxRetries" "3"

echo.
echo [OK] Secret Manager configurado!
echo.
echo Secrets configurados:
echo ----------------------------------------
dotnet user-secrets list
echo ----------------------------------------
echo.
timeout /t 3 /nobreak >nul

REM ==========================================
REM PASSO 5: Compilar projeto
REM ==========================================

echo.
echo ========================================================================
echo PASSO 5/5: Compilando o projeto
echo ========================================================================
echo.
echo Aguarde...
echo.

dotnet build

if %errorlevel% neq 0 (
	echo.
	echo [ERRO] Falha na compilacao!
	echo.
	echo Verifique os erros acima.
	echo.
	pause
	exit /b 1
)

echo.
echo [OK] Projeto compilado com sucesso!
echo.

cd ..

REM ==========================================
REM RESUMO FINAL
REM ==========================================

cls
echo.
echo ========================================================================
echo    INSTALACAO CONCLUIDA COM SUCESSO!
echo ========================================================================
echo.
echo [OK] Pacotes instalados:
echo      - Microsoft.Extensions.Http.Polly
echo      - Microsoft.Extensions.Options.DataAnnotations
echo.
echo [OK] Secret Manager configurado:
echo      - API Key: gsk_...K1x (oculta por seguranca)
echo      - API URL: https://api.groq.com/openai
echo      - Modelo: meta-llama/llama-4-scout-17b-16e-instruct
echo.
echo [OK] Projeto compilado sem erros!
echo.
echo ========================================================================
echo    PROXIMOS PASSOS
echo ========================================================================
echo.
echo 1. INICIAR A APLICACAO:
echo.
echo    cd AgilePredict
echo    dotnet run
echo.
echo    Aguarde ate ver: "Now listening on: https://localhost:7194"
echo.
echo ========================================================================
echo.
echo 2. TESTAR OS ENDPOINTS (em outro terminal):
echo.
echo    scripts\test-endpoints.bat
echo.
echo ========================================================================
echo.
echo 3. ACESSAR DOCUMENTACAO:
echo.
echo    https://localhost:7194/scalar/v1
echo.
echo ========================================================================
echo.
echo Deseja iniciar a aplicacao agora? (S/N)
echo.
set /p START_APP="> "

if /i "%START_APP%"=="S" (
	echo.
	echo Iniciando aplicacao...
	echo.
	cd AgilePredict
	dotnet run
) else (
	echo.
	echo OK! Execute manualmente quando estiver pronto:
	echo.
	echo    cd AgilePredict
	echo    dotnet run
	echo.
	pause
)
