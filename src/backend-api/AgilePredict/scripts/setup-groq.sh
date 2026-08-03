#!/bin/bash

# 🚀 Script de Setup Rápido - Groq API com Llama-4
# Configura automaticamente a API Key da Groq no projeto

set -e

echo "=================================================="
echo "🦙 Configurando Groq API + Llama-4"
echo "=================================================="
echo ""

# Verificar se estamos na pasta correta
if [ ! -f "AgilePredict/AgilePredict.csproj" ]; then
	echo "❌ ERRO: Execute este script na pasta raiz do projeto"
	exit 1
fi

echo "✅ Pasta correta detectada"
echo ""

cd AgilePredict

# API Key da Groq
GROQ_API_KEY="YOUR_GROQ_API_KEY_HERE"

echo "🔐 Configurando Secret Manager com API Key da Groq..."
dotnet user-secrets init 2>/dev/null || true
dotnet user-secrets set "LlmSettings:ApiKey" "$GROQ_API_KEY"
echo "✅ API Key configurada"
echo ""

echo "⚙️  Configurando URL e Modelo da Groq..."
dotnet user-secrets set "LlmSettings:ApiUrl" "https://api.groq.com/openai"
dotnet user-secrets set "LlmSettings:DefaultModel" "meta-llama/llama-4-scout-17b-16e-instruct"
echo "✅ Configurações aplicadas"
echo ""

echo "📋 Verificando configurações..."
echo ""
dotnet user-secrets list
echo ""

echo "=================================================="
echo "✅ CONFIGURAÇÃO GROQ CONCLUÍDA!"
echo "=================================================="
echo ""
echo "🦙 Modelo: Llama-4 Scout (meta-llama/llama-4-scout-17b-16e-instruct)"
echo "🌐 API: Groq (https://api.groq.com)"
echo "💰 Custo: GRATUITO"
echo ""
echo "📋 Próximos passos:"
echo ""
echo "1. Rodar a aplicação:"
echo "   dotnet run"
echo ""
echo "2. Testar endpoint:"
echo "   curl -X POST https://localhost:7194/api/ai/test \\"
echo "     -H \"Content-Type: application/json\" \\"
echo "     -d '{\"prompt\":\"Olá Llama!\",\"temperature\":0.7,\"maxTokens\":100}'"
echo ""
echo "3. Health check:"
echo "   curl https://localhost:7194/api/ai/health"
echo ""
echo "🎉 Pronto para usar!"
echo ""

cd ..
