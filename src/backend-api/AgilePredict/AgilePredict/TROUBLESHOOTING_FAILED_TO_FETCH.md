# 🔧 Solução: "Failed to fetch" no Scalar UI

## 🎯 Problemas Identificados e Solucionados

### ✅ PROBLEMA 1: Aplicação rodando em porta errada
**Causa:** Aplicação estava rodando apenas em HTTP (porta 5240), mas você estava acessando HTTPS (porta 7194)

**Solução:** Aplicação agora está rodando em:
- ✅ **HTTPS: https://localhost:7194**
- ✅ **HTTP: http://localhost:5240**

### ⚠️ PROBLEMA 2: Prompt vazio na requisição
**Causa:** O corpo da requisição no Scalar tinha:
```json
{
  "prompt": "",          // ❌ VAZIO - irá falhar na validação
  "temperature": 1,
  "maxTokens": 1,
  "model": null
}
```

**Solução:** Use um prompt válido com no mínimo 1 caractere.

---

## 🧪 COMO TESTAR CORRETAMENTE

### Método 1: Usar o Scalar UI (Browser)

1. **Acesse:** https://localhost:7194/scalar/v1

2. **Navegue até:** `POST /api/ai/test`

3. **Preencha o Body com valores válidos:**
```json
{
  "prompt": "What is Agile methodology?",
  "temperature": 0.7,
  "maxTokens": 100,
  "model": null
}
```

4. **Clique em "Send"**

5. **Resultado esperado:** Status 200 OK com resposta da IA

---

### Método 2: Usar o arquivo AgilePredict.http

1. **Abra:** `AgilePredict/AgilePredict.http` no Visual Studio

2. **Localize a seção "Test LLM Connection - Full Request"**

3. **Clique em "Send Request"** acima do request

4. **Request:**
```http
POST https://localhost:7194/api/ai/test
Content-Type: application/json

{
  "prompt": "Explain what Agile methodology is in one sentence.",
  "temperature": 0.7,
  "maxTokens": 100
}
```

---

### Método 3: Usar curl (Terminal/PowerShell)

```powershell
# PowerShell
$body = @{
	prompt = "What is Agile?"
	temperature = 0.7
	maxTokens = 100
} | ConvertTo-Json

Invoke-RestMethod -Uri "https://localhost:7194/api/ai/test" `
	-Method POST `
	-ContentType "application/json" `
	-Body $body `
	-SkipCertificateCheck
```

Ou com curl:

```bash
curl -X POST https://localhost:7194/api/ai/test \
  -H "Content-Type: application/json" \
  -d '{
	"prompt": "What is Agile?",
	"temperature": 0.7,
	"maxTokens": 100
  }' \
  --insecure
```

---

## ✅ Validação de Campos

O endpoint `/api/ai/test` tem as seguintes validações:

| Campo | Tipo | Obrigatório | Validação |
|-------|------|-------------|-----------|
| **prompt** | string | ✅ Sim | 1-4000 caracteres |
| **temperature** | number | Não | 0.0 - 1.0 (padrão: 0.7) |
| **maxTokens** | number | Não | 1 - 4000 (padrão: 500) |
| **model** | string | Não | Usa DefaultModel se não especificado |

---

## ❌ Erros Comuns e Soluções

### Erro: "Failed to fetch"
**Causas possíveis:**
1. ✅ **Aplicação não está rodando** → Execute `dotnet run --launch-profile https`
2. ✅ **Porta errada** → Use https://localhost:7194 (não 5240)
3. ⚠️ **Certificado SSL inválido** → Aceite o certificado no browser ou use `--insecure` no curl

### Erro: 400 Bad Request - "O prompt é obrigatório"
**Causa:** Prompt vazio ou não enviado  
**Solução:** Preencha o campo `prompt` com texto válido

### Erro: 400 Bad Request - "A temperatura deve estar entre 0.0 e 1.0"
**Causa:** Valor inválido para `temperature`  
**Solução:** Use valores entre 0.0 e 1.0

### Erro: 500 Internal Server Error
**Causas possíveis:**
1. API Key não configurada → Execute `dotnet user-secrets set "LlmSettings:ApiKey" "sua-chave"`
2. API Groq fora do ar → Verifique https://console.groq.com
3. Limite de requisições excedido → Aguarde ou use outra API Key

---

## 🔍 Como Verificar se a Aplicação está Rodando

### Via Terminal:
```powershell
# Verificar se a porta 7194 está em uso
netstat -ano | findstr :7194
```

### Via Health Check:
```powershell
# Health Check básico
Invoke-RestMethod -Uri "https://localhost:7194/health" -SkipCertificateCheck

# Health Check detalhado
Invoke-RestMethod -Uri "https://localhost:7194/health/ready" -SkipCertificateCheck
```

---

## 📝 Exemplo de Resposta Bem-Sucedida

```json
{
  "success": true,
  "content": "Agile methodology is an iterative approach to software development that emphasizes flexibility, collaboration, and customer feedback.",
  "errorMessage": null,
  "model": "meta-llama/llama-4-scout-17b-16e-instruct",
  "tokensUsed": 42,
  "responseTime": "2025-07-26T15:30:45.123Z"
}
```

---

## 🚀 Comandos Rápidos

### Iniciar aplicação:
```powershell
cd E:\engenharia-de-ia\engenharia-de-ia-aplicada\applied-ai-engineering\src\backend-api\AgilePredict\AgilePredict
dotnet run --launch-profile https
```

### Teste rápido:
```powershell
# Teste simples (endpoint /api/ai/test/simple)
Invoke-RestMethod -Uri "https://localhost:7194/api/ai/test/simple" `
	-Method POST `
	-ContentType "application/json" `
	-Body '"What is a sprint?"' `
	-SkipCertificateCheck
```

---

## 📞 Troubleshooting Adicional

Se ainda tiver problemas:

1. **Verifique os logs da aplicação** no terminal onde executou `dotnet run`

2. **Teste o Health Check da LLM:**
```powershell
Invoke-RestMethod -Uri "https://localhost:7194/api/ai/health" -SkipCertificateCheck
```

3. **Verifique se a API Key está configurada:**
```powershell
dotnet user-secrets list
```

4. **Reconfigure a API Key se necessário:**
```powershell
dotnet user-secrets set "LlmSettings:ApiKey" "gsk_SUA_CHAVE_AQUI"
```

---

**Status Atual:** ✅ Aplicação rodando em https://localhost:7194  
**Próximo passo:** Use um dos métodos acima com um prompt válido!
