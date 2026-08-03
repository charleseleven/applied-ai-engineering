# 🎯 GUIA VISUAL - Passo a Passo

## 📍 ONDE VOCÊ ESTÁ AGORA:
```
E:\engenharia-de-ia\engenharia-de-ia-aplicada\applied-ai-engineering\src\backend-api\
```

## ✅ SOLUÇÃO DEFINITIVA

### **Passo 1: Abrir PowerShell no lugar certo**

1. Abra o **Explorador de Arquivos**
2. Navegue para: `E:\engenharia-de-ia\engenharia-de-ia-aplicada\applied-ai-engineering\src\backend-api\AgilePredict`
3. Na barra de endereço, digite: `powershell` e pressione **Enter**
4. O PowerShell abrirá **direto na pasta correta**

---

### **Passo 2: Copiar e Colar TUDO de uma vez**

No PowerShell que acabou de abrir, cole **TODOS** estes comandos:

```powershell
# Confirmar que estamos no lugar certo
Get-Location
Write-Host "Arquivo .csproj existe?" (Test-Path .\AgilePredict.csproj) -ForegroundColor Yellow

# Configurar Secret Manager
dotnet user-secrets init

# Configurar API Key
$apiKey = "YOUR_GROQ_API_KEY_HERE"
dotnet user-secrets set "LlmSettings:ApiKey" $apiKey
dotnet user-secrets set "LlmSettings:ApiUrl" "https://api.groq.com/openai"
dotnet user-secrets set "LlmSettings:DefaultModel" "meta-llama/llama-4-scout-17b-16e-instruct"
dotnet user-secrets set "LlmSettings:TimeoutSeconds" "30"
dotnet user-secrets set "LlmSettings:MaxRetries" "3"

# Verificar
Write-Host "`n=== SECRETS CONFIGURADOS ===" -ForegroundColor Green
dotnet user-secrets list

# Compilar
Write-Host "`n=== COMPILANDO ===" -ForegroundColor Yellow
dotnet build

# Executar
Write-Host "`n=== EXECUTANDO ===" -ForegroundColor Cyan
dotnet run
```

---

## 📊 Estrutura de Pastas (Para Referência)

```
E:\engenharia-de-ia\
└── engenharia-de-ia-aplicada\
	└── applied-ai-engineering\
		└── src\
			└── backend-api\
				├── AgilePredict\           ← ABRA O POWERSHELL AQUI
				│   ├── AgilePredict.csproj  ← Este arquivo precisa estar aqui
				│   ├── Program.cs
				│   ├── appsettings.json
				│   ├── Controllers\
				│   ├── Services\
				│   └── Models\
				├── scripts\
				└── (outros arquivos)
```

---

## 🎥 Passo a Passo Visual

### **Método 1: Pelo Explorador (RECOMENDADO)**

1. **Explorador de Arquivos** → Navegue até `backend-api\AgilePredict`
2. **Shift + Clique Direito** na pasta vazia
3. Escolha **"Abrir janela do PowerShell aqui"** ou **"Abrir no Terminal"**
4. Cole os comandos acima

---

### **Método 2: Pelo Terminal do Visual Studio**

Se você está no VS Code ou Visual Studio:

1. Pressione **Ctrl + `** (abre o terminal)
2. Execute:
   ```powershell
   cd E:\engenharia-de-ia\engenharia-de-ia-aplicada\applied-ai-engineering\src\backend-api\AgilePredict
   ```
3. Cole os comandos de configuração

---

### **Método 3: Comando Direto (Um único comando)**

Abra qualquer PowerShell e cole **TUDO ISSO**:

```powershell
# Navegar e configurar tudo de uma vez
cd "E:\engenharia-de-ia\engenharia-de-ia-aplicada\applied-ai-engineering\src\backend-api\AgilePredict"

dotnet user-secrets init

$apiKey = "YOUR_GROQ_API_KEY_HERE"
dotnet user-secrets set "LlmSettings:ApiKey" $apiKey
dotnet user-secrets set "LlmSettings:ApiUrl" "https://api.groq.com/openai"
dotnet user-secrets set "LlmSettings:DefaultModel" "meta-llama/llama-4-scout-17b-16e-instruct"
dotnet user-secrets set "LlmSettings:TimeoutSeconds" "30"
dotnet user-secrets set "LlmSettings:MaxRetries" "3"

dotnet user-secrets list
dotnet build
dotnet run
```

---

## ✅ Quando Der Certo, Você Verá:

```
=== SECRETS CONFIGURADOS ===
LlmSettings:ApiKey = YOUR_GROQ_API_KEY_HERE
LlmSettings:ApiUrl = https://api.groq.com/openai
LlmSettings:DefaultModel = meta-llama/llama-4-scout-17b-16e-instruct
LlmSettings:TimeoutSeconds = 30
LlmSettings:MaxRetries = 3

=== COMPILANDO ===
Build succeeded.

=== EXECUTANDO ===
info: Microsoft.Hosting.Lifetime[14]
	  Now listening on: https://localhost:7194
```

---

## 🧪 Testar (Outro Terminal)

```powershell
curl -k https://localhost:7194/api/ai/health
```

---

**🎯 RECOMENDAÇÃO:** Use o **Método 1** (Explorador de Arquivos) - é o mais fácil e confiável!

**Me avise quando conseguir!** 🚀
