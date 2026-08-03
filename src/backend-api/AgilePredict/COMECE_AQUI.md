# 🎯 GUIA RÁPIDO - 3 PASSOS

## ✅ Tudo está pronto! Só falta executar.

---

## 📌 PASSO 1: Instalar e Configurar

### **Clique duas vezes neste arquivo:**
```
INSTALAR_E_CONFIGURAR.bat
```

### **O que ele faz automaticamente:**
- ✅ Instala pacotes NuGet (Polly + Options)
- ✅ Configura sua API Key no Secret Manager
- ✅ Compila o projeto
- ✅ Pergunta se quer iniciar a aplicação

### **Tempo:** ~2-3 minutos

---

## 📌 PASSO 2: Aguardar inicialização

### **Você verá no console:**
```
info: Microsoft.Hosting.Lifetime[14]
	  Now listening on: https://localhost:7194
info: Microsoft.Hosting.Lifetime[0]
	  Application started. Press Ctrl+C to shut down.
```

### **🎉 Pronto! API está rodando!**

---

## 📌 PASSO 3: Testar

### **Opção A - Script Automático (RECOMENDADO)**

Abra **OUTRO TERMINAL** e execute:
```cmd
scripts\test-endpoints.bat
```

### **Opção B - Testar manualmente**

#### **Health Check:**
```cmd
curl -k https://localhost:7194/api/ai/health
```

**Resposta esperada:**
```json
{"status":"Healthy","service":"LLM API","timestamp":"..."}
```

---

#### **Teste com Llama-4:**
```cmd
curl -k -X POST https://localhost:7194/api/ai/test/simple ^
  -H "Content-Type: application/json" ^
  -d "{\"prompt\":\"Explique IA em uma frase\"}"
```

**Resposta esperada:**
```json
{
  "success": true,
  "content": "IA é a simulação de inteligência humana por máquinas...",
  "model": "meta-llama/llama-4-scout-17b-16e-instruct",
  "tokensUsed": 42
}
```

---

#### **Documentação Interativa:**

Abra no navegador:
```
https://localhost:7194/scalar/v1
```

---

## 🎯 Resumo

| Passo | Ação | Arquivo |
|-------|------|---------|
| 1️⃣ | Instalar e configurar | `INSTALAR_E_CONFIGURAR.bat` |
| 2️⃣ | Aguardar "Now listening..." | (console) |
| 3️⃣ | Testar endpoints | `scripts\test-endpoints.bat` |

---

## 🚨 Se houver erros

### **Erro: "Polly não encontrado"**
Execute manualmente:
```cmd
cd AgilePredict
dotnet add package Microsoft.Extensions.Http.Polly
dotnet restore
dotnet build
```

### **Erro: "API Key required"**
Execute manualmente:
```cmd
cd AgilePredict
dotnet user-secrets set "LlmSettings:ApiKey" "YOUR_GROQ_API_KEY_HERE"
```

### **Erro: "Não compila"**
Verifique se está na pasta correta:
```cmd
dir AgilePredict\AgilePredict.csproj
```
Se não aparecer, navegue até a pasta raiz do projeto.

---

## ✅ Sucesso!

Se os testes passarem, você terá:

- ✅ API rodando em `https://localhost:7194`
- ✅ Groq + Llama-4 configurado e funcionando
- ✅ Retry + Circuit Breaker ativos
- ✅ Secret Manager com API Key segura
- ✅ Documentação interativa no Scalar

---

**🚀 Execute agora:** `INSTALAR_E_CONFIGURAR.bat`
