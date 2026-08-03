# ⚙️ Configuração da API Groq com Llama-4

## 📋 Informações da API

- **Provider:** Groq (https://groq.com)
- **API Base URL:** https://api.groq.com/openai
- **Modelo:** meta-llama/llama-4-scout-17b-16e-instruct
- **Custo:** GRATUITO ✅
- **Limite de Rate:** 30 requisições/minuto (tier gratuito)

---

## 🔐 Sua API Key

```
YOUR_GROQ_API_KEY_HERE
```

**⚠️ IMPORTANTE:** Esta chave está visível aqui para configuração inicial, mas deve ser movida para Secret Manager imediatamente.

---

## 🚀 Configuração Rápida (Desenvolvimento)

### Opção 1: Via Secret Manager (RECOMENDADO)
```bash
cd AgilePredict

# Inicializar Secret Manager
dotnet user-secrets init

# Configurar API Key da Groq
dotnet user-secrets set "LlmSettings:ApiKey" "YOUR_GROQ_API_KEY_HERE"

# Verificar
dotnet user-secrets list
```

### Opção 2: Via Environment Variable (Linux/Mac)
```bash
export LlmSettings__ApiKey="YOUR_GROQ_API_KEY_HERE"
```

### Opção 3: Via Environment Variable (Windows)
```cmd
set LlmSettings__ApiKey=YOUR_GROQ_API_KEY_HERE
```

---

## 🧪 Testar a Configuração

### 1. Health Check
```bash
curl https://localhost:7194/api/ai/health
```

**Resposta esperada:**
```json
{
  "status": "Healthy",
  "service": "LLM API",
  "timestamp": "2026-07-10T..."
}
```

### 2. Teste com Llama-4
```bash
curl -X POST https://localhost:7194/api/ai/test \
  -H "Content-Type: application/json" \
  -d '{
	"prompt": "Responda em uma frase: O que é engenharia de IA?",
	"temperature": 0.7,
	"maxTokens": 100,
	"model": "meta-llama/llama-4-scout-17b-16e-instruct"
  }'
```

**Resposta esperada (200 OK):**
```json
{
  "success": true,
  "content": "Engenharia de IA é a disciplina que aplica princípios de engenharia para projetar, desenvolver e implementar sistemas inteligentes.",
  "model": "meta-llama/llama-4-scout-17b-16e-instruct",
  "tokensUsed": 45,
  "responseTime": "2026-07-10T..."
}
```

---

## 📊 Limites da API Groq (Tier Gratuito)

| Métrica | Limite |
|---------|--------|
| Requisições/minuto | 30 |
| Tokens/minuto | 14,400 |
| Tokens/dia | 14,400 |
| Modelos disponíveis | Llama, Mixtral, Gemma |

**Fonte:** https://console.groq.com/docs/rate-limits

---

## 🔄 Comparação: OpenAI vs Groq

| Característica | OpenAI (GPT-3.5) | Groq (Llama-4) |
|----------------|------------------|----------------|
| **Custo** | $0.0005/1K tokens | **GRATUITO** ✅ |
| **Velocidade** | ~500 tokens/s | ~1000 tokens/s ⚡ |
| **Qualidade** | Excelente | Muito Boa |
| **Rate Limit** | 3,500 req/min | 30 req/min |
| **Uso** | Produção | Desenvolvimento/MVP |

---

## 🌐 Deploy para Produção com Groq

### Azure App Service
```bash
az webapp config appsettings set \
  --name agilepredict-api \
  --resource-group rg-agilepredict \
  --settings \
	LlmSettings__ApiKey="YOUR_GROQ_API_KEY_HERE" \
	LlmSettings__ApiUrl="https://api.groq.com/openai" \
	LlmSettings__DefaultModel="meta-llama/llama-4-scout-17b-16e-instruct"
```

### AWS Elastic Beanstalk
```bash
eb setenv \
  LlmSettings__ApiKey="YOUR_GROQ_API_KEY_HERE" \
  LlmSettings__ApiUrl="https://api.groq.com/openai" \
  LlmSettings__DefaultModel="meta-llama/llama-4-scout-17b-16e-instruct"
```

---

## 📚 Recursos Úteis

- **Documentação Groq:** https://console.groq.com/docs
- **Console Groq:** https://console.groq.com
- **Modelos disponíveis:** https://console.groq.com/docs/models
- **Rate Limits:** https://console.groq.com/docs/rate-limits

---

## 🎯 Próximos Passos

1. ✅ Configurar API Key no Secret Manager
2. ✅ Testar endpoint `/api/ai/test`
3. ✅ Verificar health check `/api/ai/health`
4. 🔄 Implementar rate limiting no código (opcional)
5. 🔄 Adicionar cache de respostas (opcional)
6. 🔄 Monitorar uso da API no console Groq

---

## ⚠️ Notas de Segurança

1. **NUNCA** commite a API Key no Git
2. Use Secret Manager em desenvolvimento
3. Use Environment Variables em produção
4. Rotacione a chave periodicamente no console Groq
5. Monitore uso para detectar acessos não autorizados
