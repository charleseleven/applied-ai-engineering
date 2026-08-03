# ==========================================
# COMANDOS PARA COPIAR E COLAR NO POWERSHELL
# Execute linha por linha
# ==========================================

# 1. Voltar para a pasta backend-api (se estiver em outro lugar)
cd E:\engenharia-de-ia\engenharia-de-ia-aplicada\applied-ai-engineering\src\backend-api

# 2. Confirmar que o arquivo existe
Test-Path .\AgilePredict\AgilePredict.csproj

# 3. Entrar na pasta AgilePredict
cd .\AgilePredict

# 4. Confirmar que estamos no lugar certo
Get-Location

# 5. Verificar se o arquivo está aqui
Test-Path .\AgilePredict.csproj

# 6. Inicializar Secret Manager
dotnet user-secrets init

# 7. Configurar a API Key
$apiKey = "YOUR_GROQ_API_KEY_HERE"
dotnet user-secrets set "LlmSettings:ApiKey" $apiKey

# 8. Configurar API URL
dotnet user-secrets set "LlmSettings:ApiUrl" "https://api.groq.com/openai"

# 9. Configurar Modelo
dotnet user-secrets set "LlmSettings:DefaultModel" "meta-llama/llama-4-scout-17b-16e-instruct"

# 10. Configurar Timeout
dotnet user-secrets set "LlmSettings:TimeoutSeconds" "30"

# 11. Configurar Retries
dotnet user-secrets set "LlmSettings:MaxRetries" "3"

# 12. Verificar configurações
Write-Host "`n=== SECRETS CONFIGURADOS ===" -ForegroundColor Green
dotnet user-secrets list

# 13. Compilar
Write-Host "`n=== COMPILANDO ===" -ForegroundColor Yellow
dotnet build

# 14. Executar
Write-Host "`n=== EXECUTANDO ===" -ForegroundColor Cyan
dotnet run
