@echo off
REM ==========================================
REM CORRIGIR ERRO SECRET MANAGER
REM ==========================================

echo.
echo ========================================================================
echo CORRIGINDO ERRO DO SECRET MANAGER
echo ========================================================================
echo.

REM Verificar se o arquivo .csproj existe
if exist "AgilePredict\AgilePredict.csproj" (
	echo [OK] Arquivo AgilePredict.csproj encontrado!
	echo.

	echo Inicializando Secret Manager com caminho explicito...
	dotnet user-secrets init --project AgilePredict\AgilePredict.csproj

	if %errorlevel% equ 0 (
		echo.
		echo [OK] Secret Manager inicializado!
		echo.

		REM Configurar secrets
		set GROQ_API_KEY=YOUR_GROQ_API_KEY_HERE

		echo Configurando API Key...
		dotnet user-secrets set "LlmSettings:ApiKey" "%GROQ_API_KEY%" --project AgilePredict\AgilePredict.csproj

		echo Configurando API URL...
		dotnet user-secrets set "LlmSettings:ApiUrl" "https://api.groq.com/openai" --project AgilePredict\AgilePredict.csproj

		echo Configurando modelo...
		dotnet user-secrets set "LlmSettings:DefaultModel" "meta-llama/llama-4-scout-17b-16e-instruct" --project AgilePredict\AgilePredict.csproj

		echo Configurando timeout...
		dotnet user-secrets set "LlmSettings:TimeoutSeconds" "30" --project AgilePredict\AgilePredict.csproj

		echo Configurando retries...
		dotnet user-secrets set "LlmSettings:MaxRetries" "3" --project AgilePredict\AgilePredict.csproj

		echo.
		echo [OK] Todos os secrets configurados!
		echo.
		echo Verificando configuracao:
		echo ----------------------------------------
		dotnet user-secrets list --project AgilePredict\AgilePredict.csproj
		echo ----------------------------------------
		echo.
		echo ========================================================================
		echo SUCESSO! Agora execute:
		echo.
		echo    cd AgilePredict
		echo    dotnet run
		echo ========================================================================
		echo.
	) else (
		echo.
		echo [ERRO] Falha ao inicializar Secret Manager
		echo.
	)
) else (
	echo [ERRO] Arquivo AgilePredict.csproj nao encontrado!
	echo.
	echo Estrutura esperada:
	echo.
	echo    SuaPastaRaiz\
	echo    ^|-- AgilePredict\
	echo    ^|   ^|-- AgilePredict.csproj
	echo    ^|   ^|-- Program.cs
	echo    ^|   ^|-- ...
	echo    ^|-- scripts\
	echo    ^|-- ...
	echo.
	echo Certifique-se de executar este script na pasta raiz do projeto.
	echo.
)

pause
