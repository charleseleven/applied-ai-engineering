#!/bin/bash

# 🚀 Script de Setup Rápido para US-21
# Execute este script para configurar o projeto AgilePredict com integração LLM

set -e  # Exit on error

echo "=================================================="
echo "🚀 AgilePredict - Setup US-21 (Integração LLM)"
echo "=================================================="
echo ""

# Verificar se estamos na pasta correta
if [ ! -f "AgilePredict/AgilePredict.csproj" ]; then
	echo "❌ ERRO: Execute este script na pasta raiz do projeto (onde está a pasta AgilePredict)"
	exit 1
fi

echo "✅ Pasta correta detectada"
echo ""

# PASSO 1: Instalar pacotes NuGet
echo "📦 PASSO 1: Instalando pacotes NuGet..."
cd AgilePredict

dotnet add package Microsoft.Extensions.Http.Polly --version 8.0.0
dotnet add package Polly.Extensions.Http --version 3.0.0
dotnet add package Microsoft.Extensions.Options.DataAnnotations --version 8.0.0

echo "✅ Pacotes instalados"
echo ""

# PASSO 2: Backup do Program.cs original
echo "💾 PASSO 2: Fazendo backup do Program.cs original..."
if [ -f "Program.cs" ] && [ ! -f "Program_BACKUP.cs" ]; then
	cp Program.cs Program_BACKUP.cs
	echo "✅ Backup criado: Program_BACKUP.cs"
else
	echo "ℹ️  Backup já existe ou arquivo não encontrado"
fi
echo ""

# PASSO 3: Substituir Program.cs
echo "🔄 PASSO 3: Atualizando Program.cs..."
if [ -f "Program_COM_LLM.cs" ]; then
	cp Program_COM_LLM.cs Program.cs
	echo "✅ Program.cs atualizado"
else
	echo "⚠️  Arquivo Program_COM_LLM.cs não encontrado - você precisará atualizar manualmente"
fi
echo ""

# PASSO 4: Atualizar appsettings.json
echo "⚙️  PASSO 4: Atualizando appsettings.json..."
if [ -f "appsettings_UPDATED.json" ]; then
	cp appsettings.json appsettings_BACKUP.json 2>/dev/null || true
	cp appsettings_UPDATED.json appsettings.json
	echo "✅ appsettings.json atualizado (backup em appsettings_BACKUP.json)"
else
	echo "⚠️  Arquivo appsettings_UPDATED.json não encontrado - você precisará atualizar manualmente"
fi
echo ""

# PASSO 5: Configurar Secret Manager
echo "🔐 PASSO 5: Configurando Secret Manager..."
read -p "Você tem uma API Key da OpenAI? (s/n): " has_api_key

if [ "$has_api_key" = "s" ] || [ "$has_api_key" = "S" ]; then
	dotnet user-secrets init

	echo ""
	read -p "Cole sua API Key da OpenAI (sk-...): " api_key

	if [ ! -z "$api_key" ]; then
		dotnet user-secrets set "LlmSettings:ApiKey" "$api_key"
		echo "✅ API Key configurada no Secret Manager"
	else
		echo "⚠️  API Key vazia - você precisará configurar depois"
	fi
else
	echo "ℹ️  Você pode configurar a API Key depois com:"
	echo "   dotnet user-secrets init"
	echo "   dotnet user-secrets set \"LlmSettings:ApiKey\" \"sk-sua-chave\""
fi
echo ""

# PASSO 6: Build do projeto
echo "🔨 PASSO 6: Building o projeto..."
dotnet build

if [ $? -eq 0 ]; then
	echo "✅ Build concluído com sucesso"
else
	echo "❌ Erro ao fazer build - verifique os erros acima"
	exit 1
fi
echo ""

# PASSO 7: Instruções finais
echo "=================================================="
echo "✅ SETUP CONCLUÍDO COM SUCESSO!"
echo "=================================================="
echo ""
echo "📋 Próximos passos:"
echo ""
echo "1. Rodar a aplicação:"
echo "   cd AgilePredict"
echo "   dotnet run"
echo ""
echo "2. Testar o endpoint:"
echo "   curl -X POST https://localhost:7194/api/ai/test \\"
echo "     -H \"Content-Type: application/json\" \\"
echo "     -d '{\"prompt\":\"Diga apenas: Teste OK\",\"temperature\":0.3,\"maxTokens\":50}'"
echo ""
echo "3. Ver o Swagger:"
echo "   https://localhost:7194/swagger"
echo ""
echo "4. Health check da LLM:"
echo "   curl https://localhost:7194/api/ai/health"
echo ""
echo "📚 Documentação completa:"
echo "   - docs/US-21_IMPLEMENTATION_CHECKLIST.md"
echo "   - docs/deployment/AZURE_DEPLOYMENT.md"
echo "   - docs/deployment/AWS_DEPLOYMENT.md"
echo ""
echo "🎉 Boa sorte!"
echo ""
