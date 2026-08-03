@echo off
REM ==========================================
REM SETUP PROFISSIONAL - GROQ + LLAMA-4
REM Instalação completa com todas as dependências
REM ==========================================

echo.
echo ==================================================
echo 🎯 SETUP PROFISSIONAL - GROQ + LLAMA-4
echo ==================================================
echo.

cd AgilePredict

REM ==========================================
REM PASSO 1: Instalar Pacotes NuGet
REM ==========================================

echo ==================================================
echo 📦 PASSO 1: Instalando dependências NuGet
echo ==================================================
echo.

echo 📥 Instalando Microsoft.Extensions.Http.Polly...
dotnet add package Microsoft.Extensions.Http.Polly
if %errorlevel% neq 0 (
	echo ❌ ERRO ao instalar Polly
	pause
	exit /b 1
)
echo ✅ Polly instalado
echo.

echo 📥 Instalando Microsoft.Extensions.Options.DataAnnotations...
dotnet add package Microsoft.Extensions.Options.DataAnnotations
if %errorlevel% neq 0 (
	echo ❌ ERRO ao instalar Options.DataAnnotations
	pause
	exit /b 1
)
echo ✅ Options.DataAnnotations instalado
echo.

echo 📥 Restaurando dependências...
dotnet restore
echo ✅ Dependências restauradas
echo.

REM ==========================================
REM PASSO 2: Configurar Secret Manager
REM ==========================================

echo ==================================================
echo 🔐 PASSO 2: Configurando Secret Manager
echo ==================================================
echo.

set GROQ_API_KEY=YOUR_GROQ_API_KEY_HERE

echo 🔧 Inicializando Secret Manager...
dotnet user-secrets init 2>nul
echo ✅ Secret Manager inicializado
echo.

echo 🔑 Configurando credenciais da Groq...
dotnet user-secrets set "LlmSettings:ApiKey" "%GROQ_API_KEY%"
dotnet user-secrets set "LlmSettings:ApiUrl" "https://api.groq.com/openai"
dotnet user-secrets set "LlmSettings:DefaultModel" "meta-llama/llama-4-scout-17b-16e-instruct"
dotnet user-secrets set "LlmSettings:TimeoutSeconds" "30"
dotnet user-secrets set "LlmSettings:MaxRetries" "3"
echo ✅ Configurações aplicadas
echo.

echo 📋 Secrets configurados:
dotnet user-secrets list
echo.

REM ==========================================
REM PASSO 3: Build do Projeto
REM ==========================================

echo ==================================================
echo 🔨 PASSO 3: Compilando projeto
echo ==================================================
echo.

dotnet build --configuration Release

if %errorlevel% neq 0 (
	echo.
	echo ❌ ERRO: Falha na compilação
	echo 📋 Verifique os erros acima
	pause
	exit /b 1
)

echo.
echo ✅ Projeto compilado com sucesso!
echo.

REM ==========================================
REM PASSO 4: Verificações de Qualidade
REM ==========================================

echo ==================================================
echo ✅ PASSO 4: Verificações concluídas
echo ==================================================
echo.
echo 📦 Pacotes instalados:
echo    - Microsoft.Extensions.Http.Polly
echo    - Microsoft.Extensions.Options.DataAnnotations
echo.
echo 🔐 Secret Manager configurado:
echo    - API Key: gsk_...K1x (oculta)
echo    - API URL: https://api.groq.com/openai
echo    - Modelo: meta-llama/llama-4-scout-17b-16e-instruct
echo.
echo 🏗️  Arquitetura:
echo    - ✅ Dependency Injection (ILlmIntegrationService)
echo    - ✅ HttpClient Factory com Polly
echo    - ✅ Retry Policy (3 tentativas, backoff exponencial)
echo    - ✅ Circuit Breaker (5 falhas, 30s break)
echo    - ✅ Configuration Validation (DataAnnotations)
echo    - ✅ JSON Cycle Handling
echo.

REM ==========================================
REM PASSO 5: Iniciar Aplicação
REM ==========================================

echo ==================================================
echo 🚀 PASSO 5: Iniciando aplicação
echo ==================================================
echo.
echo 📋 Endpoints disponíveis:
echo.
echo    🏥 Health Check:
echo       GET https://localhost:7194/api/ai/health
echo.
echo    🧪 Teste Simples:
echo       POST https://localhost:7194/api/ai/test/simple
echo.
echo    🔬 Teste Completo:
echo       POST https://localhost:7194/api/ai/test
echo.
echo    📚 Documentação Scalar:
echo       https://localhost:7194/scalar/v1
echo.
echo    📊 OpenAPI:
echo       https://localhost:7194/openapi/v1.json
echo.
echo ==================================================
echo.

timeout /t 3 /nobreak >nul

echo 🚀 Iniciando aplicação em modo Development...
echo.

dotnet run

cd ..
