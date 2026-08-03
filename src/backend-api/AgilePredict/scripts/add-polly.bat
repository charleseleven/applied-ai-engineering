@echo off
REM ==========================================
REM ADICIONAR POLLY (OPCIONAL)
REM Adiciona Retry e Circuit Breaker
REM ==========================================

echo.
echo ==================================================
echo 🔄 INSTALANDO POLLY - RETRY E CIRCUIT BREAKER
echo ==================================================
echo.

cd AgilePredict

echo 📦 Instalando Microsoft.Extensions.Http.Polly...
dotnet add package Microsoft.Extensions.Http.Polly

if %errorlevel% neq 0 (
	echo.
	echo ❌ ERRO: Falha ao instalar Polly
	pause
	exit /b 1
)

echo.
echo ✅ Polly instalado com sucesso!
echo.

echo ==================================================
echo 📝 PRÓXIMOS PASSOS
echo ==================================================
echo.
echo 1. Atualize o Program.cs para adicionar as policies:
echo.
echo    Adicione no topo:
echo    using Polly;
echo    using Polly.Extensions.Http;
echo.
echo    No AddHttpClient, adicione:
echo    .AddPolicyHandler(GetRetryPolicy^(^))
echo    .AddPolicyHandler(GetCircuitBreakerPolicy^(^))
echo.
echo    E adicione os métodos no final do arquivo (após app.Run^(^)):
echo.
echo    static IAsyncPolicy^<HttpResponseMessage^> GetRetryPolicy^(^)
echo    {
echo        return HttpPolicyExtensions
echo            .HandleTransientHttpError^(^)
echo            .WaitAndRetryAsync^(3,
echo                retryAttempt =^> TimeSpan.FromSeconds^(Math.Pow^(2, retryAttempt^)^)^);
echo    }
echo.
echo    static IAsyncPolicy^<HttpResponseMessage^> GetCircuitBreakerPolicy^(^)
echo    {
echo        return HttpPolicyExtensions
echo            .HandleTransientHttpError^(^)
echo            .CircuitBreakerAsync^(5, TimeSpan.FromSeconds^(30^)^);
echo    }
echo.
echo 2. OU use o arquivo Program_WITH_POLLY.cs como referência
echo.
pause

cd ..
