@echo off
REM ==========================================
REM PASSO 1: Backup e Atualização do appsettings.json
REM ==========================================

echo.
echo ==================================================
echo 📋 PASSO 1: Atualizando appsettings.json
echo ==================================================
echo.

cd AgilePredict

REM Fazer backup do arquivo original
if exist appsettings.json (
	echo 💾 Criando backup do appsettings.json original...
	copy appsettings.json appsettings.json.backup
	echo ✅ Backup criado: appsettings.json.backup
	echo.
)

REM Copiar o arquivo atualizado
echo 📝 Atualizando appsettings.json com configurações da Groq...
copy /Y appsettings.BACKUP_GROQ.json appsettings.json
echo ✅ appsettings.json atualizado!
echo.

cd ..

REM ==========================================
REM PASSO 2: Configurar Secret Manager
REM ==========================================

echo ==================================================
echo 🔐 PASSO 2: Configurando Secret Manager
echo ==================================================
echo.

cd AgilePredict

REM API Key da Groq
set GROQ_API_KEY=YOUR_GROQ_API_KEY_HERE

echo 🔧 Inicializando Secret Manager...
dotnet user-secrets init 2>nul
echo ✅ Secret Manager inicializado
echo.

echo 🔑 Configurando API Key da Groq...
dotnet user-secrets set "LlmSettings:ApiKey" "%GROQ_API_KEY%"
echo ✅ API Key configurada
echo.

echo 🌐 Configurando URL da API...
dotnet user-secrets set "LlmSettings:ApiUrl" "https://api.groq.com/openai"
echo ✅ URL configurada
echo.

echo 🦙 Configurando modelo Llama-4...
dotnet user-secrets set "LlmSettings:DefaultModel" "meta-llama/llama-4-scout-17b-16e-instruct"
echo ✅ Modelo configurado
echo.

echo 📋 Secrets configurados:
dotnet user-secrets list
echo.

cd ..

REM ==========================================
REM PASSO 3: Verificar Program.cs
REM ==========================================

echo ==================================================
echo 📄 PASSO 3: Verificando Program.cs
echo ==================================================
echo.

findstr /C:"LlmConfiguration" AgilePredict\Program.cs >nul
if %errorlevel% equ 0 (
	echo ✅ Program.cs já tem configuração LLM
) else (
	echo ⚠️  Program.cs precisa ser atualizado
	echo 📝 Copie o conteúdo de Program_COM_LLM.cs para Program.cs
	echo.
	pause
)

echo.

REM ==========================================
REM RESUMO FINAL
REM ==========================================

echo ==================================================
echo ✅ CONFIGURAÇÃO CONCLUÍDA!
echo ==================================================
echo.
echo 🦙 Modelo: Llama-4 Scout (meta-llama/llama-4-scout-17b-16e-instruct)
echo 🌐 API: Groq (https://api.groq.com)
echo 💰 Custo: GRATUITO
echo 🔐 API Key: Configurada via Secret Manager
echo.
echo 📋 Próximos passos:
echo.
echo 1. Verificar se Program.cs tem as configurações LLM
echo    (compare com Program_COM_LLM.cs)
echo.
echo 2. Rodar a aplicação:
echo    cd AgilePredict
echo    dotnet run
echo.
echo 3. Testar endpoint (em outro terminal):
echo    curl -X POST https://localhost:7194/api/ai/test ^
echo      -H "Content-Type: application/json" ^
echo      -d "{\"prompt\":\"Olá Llama!\",\"temperature\":0.7,\"maxTokens\":100}"
echo.
echo 4. Health check:
echo    curl https://localhost:7194/api/ai/health
echo.
echo 🎉 Pronto!
echo.
pause
