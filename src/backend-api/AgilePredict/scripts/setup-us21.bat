@echo off
REM 🚀 Script de Setup Rápido para US-21 (Windows)
REM Execute este script para configurar o projeto AgilePredict com integração LLM

echo ==================================================
echo 🚀 AgilePredict - Setup US-21 (Integração LLM)
echo ==================================================
echo.

REM Verificar se estamos na pasta correta
if not exist "AgilePredict\AgilePredict.csproj" (
	echo ❌ ERRO: Execute este script na pasta raiz do projeto
	exit /b 1
)

echo ✅ Pasta correta detectada
echo.

REM PASSO 1: Instalar pacotes NuGet
echo 📦 PASSO 1: Instalando pacotes NuGet...
cd AgilePredict

call dotnet add package Microsoft.Extensions.Http.Polly --version 8.0.0
call dotnet add package Polly.Extensions.Http --version 3.0.0
call dotnet add package Microsoft.Extensions.Options.DataAnnotations --version 8.0.0

echo ✅ Pacotes instalados
echo.

REM PASSO 2: Backup do Program.cs original
echo 💾 PASSO 2: Fazendo backup do Program.cs original...
if exist "Program.cs" (
	if not exist "Program_BACKUP.cs" (
		copy Program.cs Program_BACKUP.cs >nul
		echo ✅ Backup criado: Program_BACKUP.cs
	) else (
		echo ℹ️  Backup já existe
	)
)
echo.

REM PASSO 3: Substituir Program.cs
echo 🔄 PASSO 3: Atualizando Program.cs...
if exist "Program_COM_LLM.cs" (
	copy /Y Program_COM_LLM.cs Program.cs >nul
	echo ✅ Program.cs atualizado
) else (
	echo ⚠️  Arquivo Program_COM_LLM.cs não encontrado
)
echo.

REM PASSO 4: Atualizar appsettings.json
echo ⚙️  PASSO 4: Atualizando appsettings.json...
if exist "appsettings_UPDATED.json" (
	if exist "appsettings.json" copy appsettings.json appsettings_BACKUP.json >nul
	copy /Y appsettings_UPDATED.json appsettings.json >nul
	echo ✅ appsettings.json atualizado
) else (
	echo ⚠️  Arquivo appsettings_UPDATED.json não encontrado
)
echo.

REM PASSO 5: Configurar Secret Manager
echo 🔐 PASSO 5: Configurando Secret Manager...
set /p has_api_key="Você tem uma API Key da OpenAI? (s/n): "

if /i "%has_api_key%"=="s" (
	call dotnet user-secrets init
	echo.
	set /p api_key="Cole sua API Key da OpenAI (sk-...): "

	if not "!api_key!"=="" (
		call dotnet user-secrets set "LlmSettings:ApiKey" "!api_key!"
		echo ✅ API Key configurada no Secret Manager
	) else (
		echo ⚠️  API Key vazia - configure depois
	)
) else (
	echo ℹ️  Configure depois com:
	echo    dotnet user-secrets init
	echo    dotnet user-secrets set "LlmSettings:ApiKey" "sk-sua-chave"
)
echo.

REM PASSO 6: Build do projeto
echo 🔨 PASSO 6: Building o projeto...
call dotnet build

if %ERRORLEVEL% EQU 0 (
	echo ✅ Build concluído com sucesso
) else (
	echo ❌ Erro ao fazer build
	exit /b 1
)
echo.

REM PASSO 7: Instruções finais
echo ==================================================
echo ✅ SETUP CONCLUÍDO COM SUCESSO!
echo ==================================================
echo.
echo 📋 Próximos passos:
echo.
echo 1. Rodar a aplicação:
echo    cd AgilePredict
echo    dotnet run
echo.
echo 2. Testar o endpoint (use PowerShell ou Postman):
echo    Invoke-WebRequest -Method POST `
echo      -Uri https://localhost:7194/api/ai/test `
echo      -ContentType "application/json" `
echo      -Body '{\"prompt\":\"Teste OK\",\"temperature\":0.3}'
echo.
echo 3. Ver o Swagger:
echo    https://localhost:7194/swagger
echo.
echo 4. Health check:
echo    https://localhost:7194/api/ai/health
echo.
echo 📚 Documentação: docs\US-21_IMPLEMENTATION_CHECKLIST.md
echo.
echo 🎉 Boa sorte!
echo.

cd ..
pause
