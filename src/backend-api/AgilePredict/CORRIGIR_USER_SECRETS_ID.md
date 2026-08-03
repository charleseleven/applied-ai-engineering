# ✅ CORRIGIR UserSecretsId no .csproj

## 🎯 Problema:
O arquivo `.csproj` não tem a propriedade `UserSecretsId` configurada, por isso o comando `dotnet user-secrets` está falhando.

---

## ✅ SOLUÇÃO AUTOMÁTICA

### **Execute estes comandos no PowerShell:**

```powershell
# Navegar para a pasta do projeto
cd E:\engenharia-de-ia\engenharia-de-ia-aplicada\applied-ai-engineering\src\backend-api\AgilePredict

# Inicializar User Secrets (isso adiciona o UserSecretsId automaticamente)
dotnet user-secrets init

# Agora configurar os secrets
$apiKey = "YOUR_GROQ_API_KEY_HERE"

dotnet user-secrets set "LlmSettings:ApiKey" $apiKey
dotnet user-secrets set "LlmSettings:ApiUrl" "https://api.groq.com/openai"
dotnet user-secrets set "LlmSettings:DefaultModel" "meta-llama/llama-4-scout-17b-16e-instruct"
dotnet user-secrets set "LlmSettings:TimeoutSeconds" "30"
dotnet user-secrets set "LlmSettings:MaxRetries" "3"

# Verificar
Write-Host "`n=== SECRETS CONFIGURADOS ===" -ForegroundColor Green
dotnet user-secrets list

# Compilar e executar
Write-Host "`n=== COMPILANDO ===" -ForegroundColor Yellow
dotnet build

Write-Host "`n=== EXECUTANDO ===" -ForegroundColor Cyan
dotnet run
```

---

## 🔍 O que o `dotnet user-secrets init` faz:

Adiciona esta linha ao arquivo `.csproj`:

```xml
<PropertyGroup>
  <UserSecretsId>GUID-GERADO-AUTOMATICAMENTE</UserSecretsId>
</PropertyGroup>
```

---

## ✅ Depois de Executar

A aplicação deve iniciar **sem erros** e você poderá testar:

```powershell
# Em outro terminal
curl -k https://localhost:7194/api/ai/health
```

---

**Execute os comandos acima agora!** 🚀
