@echo off
REM ==========================================
REM TESTES DOS ENDPOINTS - GROQ + LLAMA-4
REM ==========================================

echo.
echo ==================================================
echo 🧪 TESTANDO ENDPOINTS DA API LLM
echo ==================================================
echo.

REM Verificar se curl está disponível
where curl >nul 2>&1
if %errorlevel% neq 0 (
	echo ❌ ERRO: curl não encontrado
	echo.
	echo Por favor, instale o curl ou use o Postman/Scalar
	pause
	exit /b 1
)

echo ✅ curl encontrado
echo.

REM ==========================================
REM TESTE 1: Health Check
REM ==========================================

echo ==================================================
echo 🏥 TESTE 1: Health Check
echo ==================================================
echo.
echo 📡 Enviando requisição para: https://localhost:7194/api/ai/health
echo.

curl -k https://localhost:7194/api/ai/health

echo.
echo.

REM ==========================================
REM TESTE 2: Teste Simples
REM ==========================================

echo ==================================================
echo 🧪 TESTE 2: Teste Simples com Llama-4
echo ==================================================
echo.
echo 📡 Enviando prompt: "Explique em uma frase o que é IA"
echo.

curl -k -X POST https://localhost:7194/api/ai/test/simple ^
  -H "Content-Type: application/json" ^
  -d "{\"prompt\":\"Explique em uma frase o que é IA\"}"

echo.
echo.

REM ==========================================
REM TESTE 3: Teste Completo
REM ==========================================

echo ==================================================
echo 🔬 TESTE 3: Teste Completo com Parâmetros
echo ==================================================
echo.
echo 📡 Enviando prompt com temperatura 0.7 e maxTokens 150
echo.

curl -k -X POST https://localhost:7194/api/ai/test ^
  -H "Content-Type: application/json" ^
  -d "{\"prompt\":\"Conte uma história curta sobre um robô que aprende a programar\",\"temperature\":0.7,\"maxTokens\":150}"

echo.
echo.

REM ==========================================
REM TESTE 4: Teste em Português
REM ==========================================

echo ==================================================
echo 🇧🇷 TESTE 4: Teste em Português
echo ==================================================
echo.
echo 📡 Enviando prompt em português
echo.

curl -k -X POST https://localhost:7194/api/ai/test ^
  -H "Content-Type: application/json" ^
  -d "{\"prompt\":\"Quais são os 3 princípios do desenvolvimento ágil?\",\"temperature\":0.5,\"maxTokens\":200}"

echo.
echo.

echo ==================================================
echo ✅ TESTES CONCLUÍDOS!
echo ==================================================
echo.
echo 📋 Se todos os testes retornaram status 200 OK com conteúdo,
echo    a integração com Groq + Llama-4 está funcionando!
echo.
echo 📚 Acesse a documentação interativa:
echo    https://localhost:7194/scalar/v1
echo.
pause
