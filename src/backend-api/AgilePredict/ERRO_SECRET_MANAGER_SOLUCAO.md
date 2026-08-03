# 🎯 SOLUÇÃO RÁPIDA - Erro Secret Manager

## ❌ Você recebeu este erro:
```
Could not find a MSBuild project file
```

---

## ✅ SOLUÇÃO IMEDIATA

### **Execute este arquivo (clique duas vezes):**
```
CONFIGURAR_SECRETS.bat
```

### **O que ele faz:**
- ✅ Detecta automaticamente onde está o projeto
- ✅ Usa o caminho correto com `--project`
- ✅ Configura todos os secrets
- ✅ Pergunta se quer compilar e executar

---

## 🔍 Explicação do Erro

O comando `dotnet user-secrets init` precisa ser executado:

1. **De dentro da pasta do projeto:**
   ```powershell
   cd AgilePredict
   dotnet user-secrets init
   ```

2. **OU com o parâmetro `--project`:**
   ```powershell
   dotnet user-secrets init --project AgilePredict\AgilePredict.csproj
   ```

---

## 🚀 Depois que Configurar

### **1. Compilar:**
```powershell
cd AgilePredict
dotnet build
```

### **2. Executar:**
```powershell
dotnet run
```

### **3. Testar (outro terminal):**
```powershell
curl -k https://localhost:7194/api/ai/health
```

---

## 📋 Alternativa Manual (PowerShell)

Se preferir fazer manualmente:

```powershell
# Navegar para a pasta do projeto
cd AgilePredict

# Inicializar Secret Manager
dotnet user-secrets init

# Configurar secrets
$apiKey = "YOUR_GROQ_API_KEY_HERE"
dotnet user-secrets set "LlmSettings:ApiKey" $apiKey
dotnet user-secrets set "LlmSettings:ApiUrl" "https://api.groq.com/openai"
dotnet user-secrets set "LlmSettings:DefaultModel" "meta-llama/llama-4-scout-17b-16e-instruct"
dotnet user-secrets set "LlmSettings:TimeoutSeconds" "30"
dotnet user-secrets set "LlmSettings:MaxRetries" "3"

# Verificar
dotnet user-secrets list

# Compilar e executar
dotnet build
dotnet run
```

---

## ✅ Verificação Final

Depois de configurar, execute:
```powershell
dotnet user-secrets list --project AgilePredict\AgilePredict.csproj
```

**Deve mostrar:**
```
LlmSettings:ApiKey = YOUR_GROQ_API_KEY_HERE
LlmSettings:ApiUrl = https://api.groq.com/openai
LlmSettings:DefaultModel = meta-llama/llama-4-scout-17b-16e-instruct
LlmSettings:TimeoutSeconds = 30
LlmSettings:MaxRetries = 3
```

---

**🎉 Execute agora:** `CONFIGURAR_SECRETS.bat`
