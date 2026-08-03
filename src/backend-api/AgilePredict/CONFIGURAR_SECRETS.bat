@echo off
REM ==========================================
REM CONFIGURAR SECRET MANAGER - VERSAO ROBUSTA
REM Detecta automaticamente a estrutura do projeto
REM ==========================================

echo.
echo ========================================================================
echo CONFIGURACAO SECRET MANAGER - GROQ API
echo ========================================================================
echo.

REM ==========================================
REM Detectar localização do projeto
REM ==========================================

echo Detectando estrutura do projeto...
echo.

REM Cenário 1: Estamos na pasta raiz (backend-api ou similar)
if exist "AgilePredict\AgilePredict.csproj" (
	echo [OK] Detectado: Pasta raiz do projeto
	echo [OK] Arquivo encontrado em: AgilePredict\AgilePredict.csproj
	echo.
	set PROJECT_PATH=AgilePredict\AgilePredict.csproj
	set WORK_DIR=AgilePredict
	goto :configure
)

REM Cenário 2: Já estamos dentro da pasta AgilePredict
if exist "AgilePredict.csproj" (
	echo [OK] Detectado: Dentro da pasta AgilePredict
	echo [OK] Arquivo encontrado em: AgilePredict.csproj
	echo.
	set PROJECT_PATH=AgilePredict.csproj
	set WORK_DIR=.
	goto :configure
)

REM Cenário 3: Estamos um nível acima (src ou similar)
if exist "backend-api\AgilePredict\AgilePredict.csproj" (
	echo [OK] Detectado: Pasta src ou nivel acima
	echo [OK] Arquivo encontrado em: backend-api\AgilePredict\AgilePredict.csproj
	echo.
	set PROJECT_PATH=backend-api\AgilePredict\AgilePredict.csproj
	set WORK_DIR=backend-api\AgilePredict
	goto :configure
)

REM Não encontrou o arquivo
echo [ERRO] Nao foi possivel encontrar AgilePredict.csproj
echo.
echo Estrutura esperada:
echo.
echo    SuaPasta\
echo    ^|-- AgilePredict\
echo    ^|   ^|-- AgilePredict.csproj  ^<--- Arquivo necessario
echo    ^|   ^|-- Program.cs
echo    ^|   ^|-- appsettings.json
echo.
echo Navegue ate a pasta correta e execute novamente.
echo.
pause
exit /b 1

:configure

REM ==========================================
REM Configurar Secret Manager
REM ==========================================

echo ========================================================================
echo CONFIGURANDO SECRET MANAGER
echo ========================================================================
echo.

set GROQ_API_KEY=YOUR_GROQ_API_KEY_HERE

echo [1/6] Inicializando Secret Manager...
dotnet user-secrets init --project "%PROJECT_PATH%"

if %errorlevel% neq 0 (
	echo.
	echo [AVISO] Secret Manager pode ja estar inicializado
	echo         Continuando...
	echo.
)

echo.
echo [2/6] Configurando API Key da Groq...
dotnet user-secrets set "LlmSettings:ApiKey" "%GROQ_API_KEY%" --project "%PROJECT_PATH%"

echo.
echo [3/6] Configurando URL da API...
dotnet user-secrets set "LlmSettings:ApiUrl" "https://api.groq.com/openai" --project "%PROJECT_PATH%"

echo.
echo [4/6] Configurando modelo Llama-4...
dotnet user-secrets set "LlmSettings:DefaultModel" "meta-llama/llama-4-scout-17b-16e-instruct" --project "%PROJECT_PATH%"

echo.
echo [5/6] Configurando timeout...
dotnet user-secrets set "LlmSettings:TimeoutSeconds" "30" --project "%PROJECT_PATH%"

echo.
echo [6/6] Configurando retries...
dotnet user-secrets set "LlmSettings:MaxRetries" "3" --project "%PROJECT_PATH%"

echo.
echo ========================================================================
echo CONFIGURACAO CONCLUIDA!
echo ========================================================================
echo.
echo Secrets configurados:
echo ------------------------------------------------------------------------
dotnet user-secrets list --project "%PROJECT_PATH%"
echo ------------------------------------------------------------------------
echo.
echo.
echo ========================================================================
echo PROXIMOS PASSOS
echo ========================================================================
echo.
echo 1. Compilar o projeto:
echo.
echo    cd %WORK_DIR%
echo    dotnet build
echo.
echo 2. Executar a aplicacao:
echo.
echo    dotnet run
echo.
echo 3. Testar (em outro terminal):
echo.
echo    curl -k https://localhost:7194/api/ai/health
echo.
echo ========================================================================
echo.
echo Deseja compilar e executar agora? (S/N)
set /p COMPILE_NOW="> "

if /i "%COMPILE_NOW%"=="S" (
	echo.
	echo Compilando projeto...
	echo.
	cd "%WORK_DIR%"
	dotnet build

	if %errorlevel% equ 0 (
		echo.
		echo [OK] Projeto compilado com sucesso!
		echo.
		echo Iniciando aplicacao...
		echo.
		dotnet run
	) else (
		echo.
		echo [ERRO] Falha na compilacao
		echo.
		pause
	)
) else (
	echo.
	echo OK! Execute manualmente quando estiver pronto.
	echo.
	pause
)
