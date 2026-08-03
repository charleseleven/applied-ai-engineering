@echo off
REM ==========================================
REM SETUP RÁPIDO - GROQ + LLAMA-4
REM Versão: Sem Polly (simplificado)
REM ==========================================

echo.
echo ==================================================
echo 🦙 SETUP RÁPIDO - GROQ + LLAMA-4
echo ==================================================
echo.

cd AgilePredict

REM ==========================================
REM PASSO 1: Configurar Secret Manager
REM ==========================================

echo ==================================================
echo 🔐 PASSO 1: Configurando Secret Manager
echo ==================================================
echo.

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

echo ⏱️ Configurando timeout...
dotnet user-secrets set "LlmSettings:TimeoutSeconds" "30"
echo ✅ Timeout configurado
echo.

echo 🔄 Configurando retries...
dotnet user-secrets set "LlmSettings:MaxRetries" "3"
echo ✅ Retries configurados
echo.

echo.
echo 📋 Secrets configurados:
dotnet user-secrets list
echo.

REM ==========================================
REM PASSO 2: Build do Projeto
REM ==========================================

echo ==================================================
echo 🔨 PASSO 2: Compilando o projeto
echo ==================================================
echo.

dotnet build

if %errorlevel% neq 0 (
	echo.
	echo ❌ ERRO: Falha ao compilar o projeto
	echo 📋 Verifique os erros acima
	echo.
	pause
	exit /b 1
)

echo.
echo ✅ Projeto compilado com sucesso!
echo.

REM ==========================================
REM PASSO 3: Iniciar a Aplicação
REM ==========================================

echo ==================================================
echo 🚀 PASSO 3: Iniciando a aplicação
echo ==================================================
echo.
echo ⚠️  A aplicação será iniciada em modo DESENVOLVIMENTO
echo.
echo 📋 Endpoints disponíveis após inicialização:
echo.
echo    🏥 Health Check:
echo       GET https://localhost:7194/api/ai/health
echo.
echo    🧪 Teste Simples:
echo       POST https://localhost:7194/api/ai/test/simple
echo       Body: { "prompt": "Olá Llama!" }
echo.
echo    🔬 Teste Completo:
echo       POST https://localhost:7194/api/ai/test
echo       Body: { "prompt": "Teste", "temperature": 0.7, "maxTokens": 100 }
echo.
echo    📚 Documentação Scalar:
echo       https://localhost:7194/scalar/v1
echo.
echo ==================================================
echo.
echo ⏱️  Aguardando 3 segundos...
echo.

timeout /t 3 /nobreak >nul

echo 🚀 Iniciando aplicação...
echo.

dotnet run

cd ..
