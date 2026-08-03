# 🔍 DIAGNÓSTICO - Health Check Unhealthy

## ❌ Problema: Status "Unhealthy"

Isso significa que o serviço LLM não conseguiu validar a conexão com a API Groq.

---

## 🔧 PASSO 1: Verificar Logs da Aplicação

### **No terminal onde o `dotnet run` está executando, procure por:**

- ❌ Erros de `HttpClient`
- ❌ Erros de `LlmIntegrationService`
- ❌ Mensagens de `Circuit Breaker`
- ❌ Exceções relacionadas à API Key

**Copie e cole os erros aqui para análise.**

---

## 🔧 PASSO 2: Verificar Secret Manager

### **Em outro PowerShell, execute:**

```powershell
cd E:\engenharia-de-ia\engenharia-de-ia-aplicada\applied-ai-engineering\src\backend-api\AgilePredict

dotnet user-secrets list
```

**Deve mostrar:**
```
LlmSettings:ApiKey = YOUR_GROQ_API_KEY_HERE
LlmSettings:ApiUrl = https://api.groq.com/openai
LlmSettings:DefaultModel = meta-llama/llama-4-scout-17b-16e-instruct
LlmSettings:TimeoutSeconds = 30
LlmSettings:MaxRetries = 3
```

**Se NÃO aparecer ou estiver diferente**, reconfigure:
```powershell
$apiKey = "YOUR_GROQ_API_KEY_HERE"
dotnet user-secrets set "LlmSettings:ApiKey" $apiKey
dotnet user-secrets set "LlmSettings:ApiUrl" "https://api.groq.com/openai"
dotnet user-secrets set "LlmSettings:DefaultModel" "meta-llama/llama-4-scout-17b-16e-instruct"
dotnet user-secrets set "LlmSettings:TimeoutSeconds" "30"
dotnet user-secrets set "LlmSettings:MaxRetries" "3"
```

---

## 🔧 PASSO 3: Testar API Groq Diretamente

### **Teste se a API Key está válida:**

```powershell
$headers = @{
	"Authorization" = "Bearer YOUR_GROQ_API_KEY_HERE"
	"Content-Type" = "application/json"
}

$body = @{
	model = "meta-llama/llama-4-scout-17b-16e-instruct"
	messages = @(
		@{
			role = "user"
			content = "Teste"
		}
	)
	max_tokens = 10
} | ConvertTo-Json

Invoke-RestMethod -Uri "https://api.groq.com/openai/v1/chat/completions" -Method Post -Headers $headers -Body $body
```

**Se retornar erro:**
- ❌ **401 Unauthorized** → API Key inválida ou expirada
- ❌ **404 Not Found** → URL ou modelo incorreto
- ❌ **429 Too Many Requests** → Limite de requisições atingido

---

## 🔧 PASSO 4: Verificar Configuração do appsettings.json

### **Verifique se o arquivo está correto:**

```powershell
Get-Content .\appsettings.json
```

**Deve conter:**
```json
{
  "ConnectionStrings": { ... },
  "Logging": { ... },
  "AllowedHosts": "*",
  "LlmSettings": {
	"ApiUrl": "https://api.groq.com/openai",
	"ApiKey": "YOUR_API_KEY_HERE",
	"DefaultModel": "meta-llama/llama-4-scout-17b-16e-instruct",
	"TimeoutSeconds": 30,
	"MaxRetries": 3
  }
}
```

---

## 🔧 PASSO 5: Reiniciar a Aplicação

### **Depois de verificar/corrigir:**

1. **Parar a aplicação:** `Ctrl + C` no terminal onde está rodando
2. **Recompilar:**
   ```powershell
   dotnet build
   ```
3. **Executar novamente:**
   ```powershell
   dotnet run
   ```
4. **Testar health check novamente:**
   ```powershell
   curl -k https://localhost:7194/api/ai/health
   ```

---

## 🎯 Causas Comuns de "Unhealthy"

| Causa | Como Verificar | Solução |
|-------|----------------|---------|
| **API Key não configurada** | `dotnet user-secrets list` | Reconfigurar secrets |
| **API Key inválida** | Testar direto na API Groq | Obter nova chave |
| **URL incorreta** | Verificar appsettings.json | Corrigir para `https://api.groq.com/openai` |
| **Modelo não existe** | Logs da aplicação | Verificar nome do modelo |
| **Timeout** | Aumentar TimeoutSeconds | `dotnet user-secrets set "LlmSettings:TimeoutSeconds" "60"` |
| **Firewall/Proxy** | Testar `curl https://api.groq.com` | Configurar proxy |

---

## 📋 Próximas Ações

1. **Execute o PASSO 2** (verificar secrets)
2. **Copie os erros do console** da aplicação
3. **Me envie os logs** para análise detalhada

---

**Execute os comandos acima e me mostre o resultado!** 🔍
