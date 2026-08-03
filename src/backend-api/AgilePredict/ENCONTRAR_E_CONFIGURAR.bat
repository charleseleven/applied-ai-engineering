@echo off
REM ==========================================
REM ENCONTRAR ARQUIVO .CSPROJ
REM ==========================================

echo.
echo ========================================================================
echo LOCALIZANDO ARQUIVO AgilePredict.csproj
echo ========================================================================
echo.

echo Procurando arquivo AgilePredict.csproj...
echo.
echo Isso pode levar alguns segundos...
echo.

REM Procurar a partir do diretório atual
for /r %%i in (AgilePredict.csproj) do (
	if exist "%%i" (
		echo [ENCONTRADO] %%i
		echo.
		set FOUND_PATH=%%i
		goto :found
	)
)

echo [NAO ENCONTRADO] Arquivo AgilePredict.csproj nao foi encontrado nesta pasta.
echo.
echo Por favor, navegue ate a pasta que contem o arquivo AgilePredict.csproj
echo e execute este script novamente.
echo.
pause
exit /b 1

:found
echo ========================================================================
echo ARQUIVO ENCONTRADO!
echo ========================================================================
echo.
echo Caminho completo:
echo %FOUND_PATH%
echo.
echo.

REM Extrair o diretório do arquivo
for %%F in ("%FOUND_PATH%") do set PROJECT_DIR=%%~dpF

echo Diretorio do projeto:
echo %PROJECT_DIR%
echo.
echo.

echo ========================================================================
echo CONFIGURANDO SECRET MANAGER
echo ========================================================================
echo.

set GROQ_API_KEY=YOUR_GROQ_API_KEY_HERE

echo [1/6] Inicializando Secret Manager...
dotnet user-secrets init --project "%FOUND_PATH%"

if %errorlevel% neq 0 (
	echo.
	echo [AVISO] Secret Manager pode ja estar inicializado
	echo.
)

echo.
echo [2/6] Configurando API Key...
dotnet user-secrets set "LlmSettings:ApiKey" "%GROQ_API_KEY%" --project "%FOUND_PATH%"

echo.
echo [3/6] Configurando API URL...
dotnet user-secrets set "LlmSettings:ApiUrl" "https://api.groq.com/openai" --project "%FOUND_PATH%"

echo.
echo [4/6] Configurando modelo...
dotnet user-secrets set "LlmSettings:DefaultModel" "meta-llama/llama-4-scout-17b-16e-instruct" --project "%FOUND_PATH%"

echo.
echo [5/6] Configurando timeout...
dotnet user-secrets set "LlmSettings:TimeoutSeconds" "30" --project "%FOUND_PATH%"

echo.
echo [6/6] Configurando retries...
dotnet user-secrets set "LlmSettings:MaxRetries" "3" --project "%FOUND_PATH%"

echo.
echo ========================================================================
echo CONFIGURACAO CONCLUIDA!
echo ========================================================================
echo.
echo Secrets configurados:
echo ------------------------------------------------------------------------
dotnet user-secrets list --project "%FOUND_PATH%"
echo ------------------------------------------------------------------------
echo.
echo.
echo ========================================================================
echo PROXIMOS PASSOS
echo ========================================================================
echo.
echo 1. Navegar para o diretorio do projeto:
echo.
echo    cd /d "%PROJECT_DIR%"
echo.
echo 2. Compilar:
echo.
echo    dotnet build
echo.
echo 3. Executar:
echo.
echo    dotnet run
echo.
echo ========================================================================
echo.

pause
