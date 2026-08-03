# ==========================================
# ENCONTRAR E CONFIGURAR - PowerShell
# ==========================================

Write-Host ""
Write-Host "========================================================================"
Write-Host "LOCALIZANDO ARQUIVO AgilePredict.csproj" -ForegroundColor Cyan
Write-Host "========================================================================"
Write-Host ""

# Procurar o arquivo .csproj
Write-Host "Procurando arquivo AgilePredict.csproj..." -ForegroundColor Yellow
Write-Host ""

$csprojFile = Get-ChildItem -Path . -Filter "AgilePredict.csproj" -Recurse -ErrorAction SilentlyContinue | Select-Object -First 1

if ($null -eq $csprojFile) {
	Write-Host "[ERRO] Arquivo AgilePredict.csproj nao encontrado!" -ForegroundColor Red
	Write-Host ""
	Write-Host "Por favor:" -ForegroundColor Yellow
	Write-Host "1. Navegue ate a pasta raiz do projeto" -ForegroundColor Yellow
	Write-Host "2. Execute este script novamente" -ForegroundColor Yellow
	Write-Host ""
	Read-Host "Pressione Enter para sair"
	exit 1
}

Write-Host "[ENCONTRADO]" -ForegroundColor Green
Write-Host "Caminho: $($csprojFile.FullName)" -ForegroundColor Green
Write-Host ""
Write-Host ""

# Configurar Secret Manager
Write-Host "========================================================================"
Write-Host "CONFIGURANDO SECRET MANAGER" -ForegroundColor Cyan
Write-Host "========================================================================"
Write-Host ""

$apiKey = "YOUR_GROQ_API_KEY_HERE"
$projectPath = $csprojFile.FullName

Write-Host "[1/6] Inicializando Secret Manager..." -ForegroundColor Yellow
dotnet user-secrets init --project $projectPath
Write-Host ""

Write-Host "[2/6] Configurando API Key..." -ForegroundColor Yellow
dotnet user-secrets set "LlmSettings:ApiKey" $apiKey --project $projectPath
Write-Host ""

Write-Host "[3/6] Configurando API URL..." -ForegroundColor Yellow
dotnet user-secrets set "LlmSettings:ApiUrl" "https://api.groq.com/openai" --project $projectPath
Write-Host ""

Write-Host "[4/6] Configurando modelo..." -ForegroundColor Yellow
dotnet user-secrets set "LlmSettings:DefaultModel" "meta-llama/llama-4-scout-17b-16e-instruct" --project $projectPath
Write-Host ""

Write-Host "[5/6] Configurando timeout..." -ForegroundColor Yellow
dotnet user-secrets set "LlmSettings:TimeoutSeconds" "30" --project $projectPath
Write-Host ""

Write-Host "[6/6] Configurando retries..." -ForegroundColor Yellow
dotnet user-secrets set "LlmSettings:MaxRetries" "3" --project $projectPath
Write-Host ""

Write-Host "========================================================================"
Write-Host "CONFIGURACAO CONCLUIDA!" -ForegroundColor Green
Write-Host "========================================================================"
Write-Host ""

Write-Host "Secrets configurados:" -ForegroundColor Cyan
Write-Host "------------------------------------------------------------------------"
dotnet user-secrets list --project $projectPath
Write-Host "------------------------------------------------------------------------"
Write-Host ""
Write-Host ""

# Ir para o diretório do projeto
$projectDir = Split-Path -Parent $projectPath
Write-Host "Navegando para: $projectDir" -ForegroundColor Yellow
Set-Location $projectDir
Write-Host ""

Write-Host "========================================================================"
Write-Host "PROXIMOS PASSOS" -ForegroundColor Cyan
Write-Host "========================================================================"
Write-Host ""
Write-Host "Deseja compilar e executar agora? (S/N)" -ForegroundColor Yellow
$response = Read-Host

if ($response -eq "S" -or $response -eq "s") {
	Write-Host ""
	Write-Host "Compilando projeto..." -ForegroundColor Yellow
	dotnet build

	if ($LASTEXITCODE -eq 0) {
		Write-Host ""
		Write-Host "[OK] Projeto compilado com sucesso!" -ForegroundColor Green
		Write-Host ""
		Write-Host "Iniciando aplicacao..." -ForegroundColor Yellow
		Write-Host ""
		dotnet run
	} else {
		Write-Host ""
		Write-Host "[ERRO] Falha na compilacao" -ForegroundColor Red
		Write-Host ""
		Read-Host "Pressione Enter para sair"
	}
} else {
	Write-Host ""
	Write-Host "OK! Execute manualmente quando estiver pronto:" -ForegroundColor Yellow
	Write-Host ""
	Write-Host "  dotnet build" -ForegroundColor Cyan
	Write-Host "  dotnet run" -ForegroundColor Cyan
	Write-Host ""
	Read-Host "Pressione Enter para sair"
}
.\configurar-secrets.ps1