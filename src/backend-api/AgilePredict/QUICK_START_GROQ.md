# 🚀 GUIA DE EXECUÇÃO - GROQ + LLAMA-4 (PROFISSIONAL)

## ✅ Implementação Enterprise-Grade

- ✅ **Polly Resilience Patterns** - Retry + Circuit Breaker
- ✅ **HttpClient Factory** - Connection pooling e lifetime management
- ✅ **Configuration Validation** - DataAnnotations com fail-fast
- ✅ **Secret Manager** - API keys seguras
- ✅ **Dependency Injection** - Interface segregation
- ✅ **JSON Cycle Handling** - Prevenção de referências circulares
- ✅ **Observability** - Logs estruturados de retry/circuit breaker

---

## ⚡ EXECUÇÃO (1 Comando)

### **Na raiz do projeto:**

```cmd
scripts\setup-professional.bat
```

**O que esse script faz:**
1. 📦 **Instala pacotes NuGet:**
   - `Microsoft.Extensions.Http.Polly` (retry + circuit breaker)
   - `Microsoft.Extensions.Options.DataAnnotations` (validação)

2. 🔐 **Configura Secret Manager:**
   - API Key da Groq (não versionada no Git)
   - URL da API: `https://api.groq.com/openai`
   - Modelo: `meta-llama/llama-4-scout-17b-16e-instruct`

3. 🔨 **Compila em Release:**
   - Validação de configuração em startup
   - Otimizações de produção

4. 🚀 **Inicia a aplicação:**
   - Porta: `https://localhost:7194`
   - Documentação Scalar: `/scalar/v1`

---

## 🧪 TESTAR ENDPOINTS

**DEIXE A APLICAÇÃO RODANDO** e abra **OUTRO TERMINAL**:

```cmd
scripts\test-endpoints.bat
```

**Testes incluídos:**
- 🏥 Health Check
- 🧪 Prompt simples
- 🔬 Parâmetros avançados (temperatura, maxTokens)
- 🇧🇷 Teste em português
- 📊 Validação de rate limiting (429 handling)

---

## 📊 Resultados Esperados

### ✅ **Health Check**
```json
{
  "status": "Healthy",
  "service": "LLM API",
  "timestamp": "2026-01-10T..."
}
```

### ✅ **Resposta do Llama-4**
```json
{
  "success": true,
  "content": "IA é a simulação de processos de inteligência humana por sistemas computacionais...",
  "model": "meta-llama/llama-4-scout-17b-16e-instruct",
  "tokensUsed": 42,
  "responseTime": "2026-01-10T14:30:25Z"
}
```

### 🔄 **Logs de Resiliência (console)**
```
[Retry] Tentativa 1 de 3 após 2s. Razão: RequestTimeout
[Circuit Breaker] ⚠️  Circuito ABERTO por 30s. Razão: ServiceUnavailable
[Circuit Breaker] ✅ Circuito FECHADO. Requisições normalizadas.
```

---

## 🏗️ Arquitetura Implementada

### **Camadas**
```
┌─────────────────────────────────────┐
│   Controllers (Presentation)        │  ← Validação de entrada
├─────────────────────────────────────┤
│   Services (Business Logic)         │  ← Orquestração
├─────────────────────────────────────┤
│   HttpClient + Polly (Resilience)   │  ← Retry + Circuit Breaker
├─────────────────────────────────────┤
│   Groq API (External)                │  ← Llama-4 Scout
└─────────────────────────────────────┘
```

### **Polly Policies**

#### **1. Retry Policy**
```csharp
Tentativas: 3
Backoff: Exponencial (2s → 4s → 8s)
Casos: 5xx, Timeout, 429 (Too Many Requests)
```

#### **2. Circuit Breaker**
```csharp
Limite: 5 falhas consecutivas
Break Duration: 30 segundos
Estados: Closed → Open → Half-Open → Closed
```

### **Configuration Validation**
```csharp
[Required] ApiUrl
[Required] ApiKey (via Secret Manager)
[Required] DefaultModel
[Range(1, 300)] TimeoutSeconds
[Range(1, 10)] MaxRetries
```

---

## 📚 Documentação Adicional

### **Arquitetura Detalhada**
```
docs/ARCHITECTURE_LLM.md
```
- Princípios SOLID aplicados
- Fluxo de requisição (diagrama)
- Patterns de resiliência explicados
- Estratégias de teste

### **Configuração da Groq**
```
docs/GROQ_CONFIGURATION.md
```
- Informações da API
- Limites do tier gratuito (30 req/min)
- Deploy para Azure/AWS
- Troubleshooting

---

## 🔍 Verificar Configuração

### **Ver secrets:**
```cmd
cd AgilePredict
dotnet user-secrets list
```

### **Output esperado:**
```
LlmSettings:ApiKey = YOUR_GROQ_API_KEY_HERE
LlmSettings:ApiUrl = https://api.groq.com/openai
LlmSettings:DefaultModel = meta-llama/llama-4-scout-17b-16e-instruct
LlmSettings:TimeoutSeconds = 30
LlmSettings:MaxRetries = 3
```

---

## 🌐 Endpoints Disponíveis

| Método | Endpoint | Descrição |
|--------|----------|-----------|
| GET | `/api/ai/health` | Health check do serviço LLM |
| POST | `/api/ai/test/simple` | Teste com prompt simples |
| POST | `/api/ai/test` | Teste completo com parâmetros |
| GET | `/scalar/v1` | Documentação interativa (Scalar) |
| GET | `/openapi/v1.json` | Especificação OpenAPI |

---

## ⚡ Exemplos de Uso

### **1. Health Check**
```bash
curl -k https://localhost:7194/api/ai/health
```

### **2. Prompt Simples**
```bash
curl -k -X POST https://localhost:7194/api/ai/test/simple \
  -H "Content-Type: application/json" \
  -d '{"prompt":"Explique IA em uma frase"}'
```

### **3. Prompt Avançado**
```bash
curl -k -X POST https://localhost:7194/api/ai/test \
  -H "Content-Type: application/json" \
  -d '{
    "prompt": "Conte uma história sobre IA",
    "temperature": 0.7,
    "maxTokens": 150,
    "model": "meta-llama/llama-4-scout-17b-16e-instruct"
  }'
```

---

## 🎯 Checklist de Qualidade

- ✅ Pacotes NuGet instalados (Polly, Options)
- ✅ Secret Manager configurado
- ✅ Compilação sem erros/warnings
- ✅ Health check retorna 200 OK
- ✅ Teste com Llama-4 retorna resposta válida
- ✅ Retry policy detecta falhas transientes
- ✅ Circuit breaker protege contra cascata
- ✅ Logs estruturados no console
- ✅ Documentação Scalar acessível
- ✅ Configuração validada em startup

---

## 🚨 Troubleshooting

### **Erro: "Polly could not be found"**
Execute manualmente:
```cmd
cd AgilePredict
dotnet add package Microsoft.Extensions.Http.Polly
dotnet restore
```

### **Erro: "ApiKey is required"**
Configure o Secret Manager:
```cmd
cd AgilePredict
dotnet user-secrets set "LlmSettings:ApiKey" "YOUR_GROQ_API_KEY_HERE"
```

### **Erro: SSL/TLS**
Use flag `-k` no curl:
```cmd
curl -k https://localhost:7194/api/ai/health
```

### **Erro: "Circuit breaker is open"**
Aguarde 30 segundos ou reinicie a aplicação.

---

## 🎓 Próximos Passos

1. ✅ **Produção**: Mover API Key para Azure Key Vault
2. ✅ **Métricas**: Integrar Application Insights
3. ✅ **Cache**: Implementar Redis para respostas frequentes
4. ✅ **Rate Limiting**: Adicionar middleware de throttling
5. ✅ **Testes**: Expandir cobertura de unit/integration tests

---

**Configuração:** Enterprise-Grade ✅  
**Resiliência:** Retry + Circuit Breaker ✅  
**Segurança:** Secret Manager ✅  
**Observabilidade:** Logs estruturados ✅  

**🚀 Execute agora:** `scripts\setup-professional.bat`

#### ✅ **Health Check** (deve retornar):
```json
{
  "status": "Healthy",
  "service": "LLM API",
  "timestamp": "2026-01-..."
}
```

#### ✅ **Teste com Llama-4** (deve retornar 200 OK):
```json
{
  "success": true,
  "content": "IA é a simulação de inteligência humana por máquinas...",
  "model": "meta-llama/llama-4-scout-17b-16e-instruct",
  "tokensUsed": 45,
  "responseTime": "2026-01-..."
}
```

---

## 🔍 Verificar Configuração Manualmente

### **Ver secrets configurados:**
```cmd
cd AgilePredict
dotnet user-secrets list
```

**Deve exibir:**
```
LlmSettings:ApiKey = YOUR_GROQ_API_KEY_HERE
LlmSettings:ApiUrl = https://api.groq.com/openai
LlmSettings:DefaultModel = meta-llama/llama-4-scout-17b-16e-instruct
LlmSettings:TimeoutSeconds = 30
LlmSettings:MaxRetries = 3
```

---

## 📚 Documentação Interativa (Scalar)

Acesse no navegador:
```
https://localhost:7194/scalar/v1
```

Você verá todos os endpoints documentados, incluindo:
- `/api/ai/health` - Health check
- `/api/ai/test` - Teste completo
- `/api/ai/test/simple` - Teste simples
- `/api/projects` - CRUD de projetos
- `/api/sprints` - CRUD de sprints
- `/api/tasks` - CRUD de tarefas

---

## 🧪 Testar Manualmente com curl

### Health Check:
```cmd
curl -k https://localhost:7194/api/ai/health
```

### Teste simples:
```cmd
curl -k -X POST https://localhost:7194/api/ai/test/simple ^
  -H "Content-Type: application/json" ^
  -d "{\"prompt\":\"Olá Llama!\"}"
```

### Teste completo:
```cmd
curl -k -X POST https://localhost:7194/api/ai/test ^
  -H "Content-Type: application/json" ^
  -d "{\"prompt\":\"Explique IA\",\"temperature\":0.7,\"maxTokens\":100}"
```

---

## ❌ Troubleshooting

### **Erro: "The SSL connection could not be established"**
Use a flag `-k` no curl para ignorar certificados de desenvolvimento:
```cmd
curl -k https://localhost:7194/api/ai/health
```

### **Erro: "LlmConfiguration is not registered"**
Execute novamente o script:
```cmd
scripts\setup-and-run.bat
```

### **Erro: "API Key is required"**
Verifique se o Secret Manager está configurado:
```cmd
cd AgilePredict
dotnet user-secrets list
```

Se não aparecer nada, rode:
```cmd
scripts\setup-groq.bat
```

### **Erro de compilação: "Polly não encontrado"**
Instale manualmente:
```cmd
cd AgilePredict
dotnet add package Microsoft.Extensions.Http.Polly --version 8.0.0
dotnet add package Microsoft.Extensions.Options.DataAnnotations --version 8.0.0
```

---

## 🎉 Pronto!

Se os testes passarem, sua aplicação está configurada e funcionando com:
- ✅ **Groq API** (https://api.groq.com)
- ✅ **Llama-4 Scout** (modelo gratuito)
- ✅ **Retry e Circuit Breaker** (Polly)
- ✅ **Secret Manager** (API Key segura)
- ✅ **Documentação Scalar** (interface interativa)

---

## 📋 Próximos Passos

1. ✅ Integrar o LLM nos endpoints de Projetos/Sprints/Tasks
2. ✅ Adicionar cache de respostas
3. ✅ Implementar rate limiting
4. ✅ Monitorar uso da API no console Groq
5. ✅ Deploy para produção (Azure/AWS)

**Consulte:** `docs/GROQ_CONFIGURATION.md` para mais detalhes!
