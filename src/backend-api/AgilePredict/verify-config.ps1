# Script para verificar configuração antes de rodar setup-groq.bat

Write-Host "==================================================" -ForegroundColor Cyan
Write-Host "🔍 Verificando Configuração Atual" -ForegroundColor Cyan
Write-Host "==================================================" -ForegroundColor Cyan
Write-Host ""

# Verificar appsettings.json
Write-Host "📄 Verificando appsettings.json..." -ForegroundColor Yellow
if (Test-Path "AgilePredict\appsettings.json") {
	Write-Host "✅ Arquivo encontrado" -ForegroundColor Green
	$content = Get-Content "AgilePredict\appsettings.json" -Raw
	if ($content -match "LlmSettings") {
		Write-Host "✅ Seção LlmSettings encontrada" -ForegroundColor Green
	} else {
		Write-Host "⚠️  Seção LlmSettings NÃO encontrada" -ForegroundColor Red
		Write-Host "📋 Conteúdo do arquivo:" -ForegroundColor Yellow
		Write-Host $content
	}
} else {
	Write-Host "❌ appsettings.json NÃO encontrado" -ForegroundColor Red
}

Write-Host ""

# Verificar Program.cs
Write-Host "📄 Verificando Program.cs..." -ForegroundColor Yellow
if (Test-Path "AgilePredict\Program.cs") {
	Write-Host "✅ Arquivo encontrado" -ForegroundColor Green
	$content = Get-Content "AgilePredict\Program.cs" -Raw
	if ($content -match "LlmConfiguration") {
		Write-Host "✅ LlmConfiguration encontrada" -ForegroundColor Green
	} else {
		Write-Host "⚠️  LlmConfiguration NÃO encontrada" -ForegroundColor Red
	}

	if ($content -match "ILlmIntegrationService") {
		Write-Host "✅ ILlmIntegrationService registrado" -ForegroundColor Green
	} else {
		Write-Host "⚠️  ILlmIntegrationService NÃO registrado" -ForegroundColor Red
	}
} else {
	Write-Host "❌ Program.cs NÃO encontrado" -ForegroundColor Red
}

Write-Host ""

# Verificar arquivos criados
Write-Host "📄 Verificando arquivos criados..." -ForegroundColor Yellow
$files = @(
	"AgilePredict\Controllers\AiTestController.cs",
	"AgilePredict\Services\LlmIntegrationService.cs",
	"AgilePredict\Services\Interfaces\ILlmIntegrationService.cs",
	"AgilePredict\Models\Configuration\LlmConfiguration.cs",
	"AgilePredict\Models\DTOs\LlmRequest.cs",
	"AgilePredict\Models\DTOs\LlmResponse.cs",
	"scripts\setup-groq.bat",
	"docs\GROQ_CONFIGURATION.md"
)

foreach ($file in $files) {
	if (Test-Path $file) {
		Write-Host "  ✅ $file" -ForegroundColor Green
	} else {
		Write-Host "  ❌ $file" -ForegroundColor Red
	}
}

Write-Host ""
Write-Host "==================================================" -ForegroundColor Cyan
Write-Host "✅ Verificação concluída!" -ForegroundColor Cyan
Write-Host "==================================================" -ForegroundColor Cyan
