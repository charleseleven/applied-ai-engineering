# 🚀 Deployment para Azure App Service - AgilePredict API

## 📋 PRÉ-REQUISITOS

- Azure CLI instalado: `az --version`
- .NET 8.0 SDK instalado
- Conta Azure ativa
- API Key da OpenAI (ou outra LLM)

---

## 🔧 PASSO 1: Criar Recursos no Azure

### 1.1 - Login no Azure
```bash
az login
```

### 1.2 - Definir variáveis
```bash
# Configurações do projeto
RESOURCE_GROUP="rg-agilepredict-prod"
LOCATION="brazilsouth"
APP_SERVICE_PLAN="asp-agilepredict-prod"
WEB_APP_NAME="agilepredict-api-prod"
SQL_SERVER_NAME="sql-agilepredict-prod"
SQL_DB_NAME="AgilePredictDb"
SQL_ADMIN_USER="sqladmin"
SQL_ADMIN_PASSWORD="Seu@Password@Forte123"  # MUDE ISSO!
```

### 1.3 - Criar Resource Group
```bash
az group create \
  --name $RESOURCE_GROUP \
  --location $LOCATION
```

### 1.4 - Criar App Service Plan (Linux)
```bash
az appservice plan create \
  --name $APP_SERVICE_PLAN \
  --resource-group $RESOURCE_GROUP \
  --location $LOCATION \
  --is-linux \
  --sku B1  # Pode usar F1 (Free) para testes
```

### 1.5 - Criar Web App
```bash
az webapp create \
  --name $WEB_APP_NAME \
  --resource-group $RESOURCE_GROUP \
  --plan $APP_SERVICE_PLAN \
  --runtime "DOTNET|8.0"
```

### 1.6 - Criar SQL Server e Database (opcional - se não usar LocalDB)
```bash
# Criar SQL Server
az sql server create \
  --name $SQL_SERVER_NAME \
  --resource-group $RESOURCE_GROUP \
  --location $LOCATION \
  --admin-user $SQL_ADMIN_USER \
  --admin-password $SQL_ADMIN_PASSWORD

# Criar Database
az sql db create \
  --name $SQL_DB_NAME \
  --server $SQL_SERVER_NAME \
  --resource-group $RESOURCE_GROUP \
  --service-objective Basic  # Tier mais barato

# Permitir acesso de Azure Services
az sql server firewall-rule create \
  --name AllowAzureServices \
  --server $SQL_SERVER_NAME \
  --resource-group $RESOURCE_GROUP \
  --start-ip-address 0.0.0.0 \
  --end-ip-address 0.0.0.0
```

---

## 🔐 PASSO 2: Configurar Secrets (Environment Variables)

### 2.1 - Configurar API Key da LLM (OpenAI)
```bash
az webapp config appsettings set \
  --name $WEB_APP_NAME \
  --resource-group $RESOURCE_GROUP \
  --settings \
	LlmSettings__ApiKey="sk-sua-chave-openai-aqui" \
	LlmSettings__ApiUrl="https://api.openai.com" \
	LlmSettings__DefaultModel="gpt-3.5-turbo" \
	LlmSettings__TimeoutSeconds="30" \
	LlmSettings__MaxRetries="3"
```

### 2.2 - Configurar Connection String (se usar SQL Azure)
```bash
# Obter connection string
SQL_CONNECTION_STRING="Server=tcp:$SQL_SERVER_NAME.database.windows.net,1433;Database=$SQL_DB_NAME;User ID=$SQL_ADMIN_USER;Password=$SQL_ADMIN_PASSWORD;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"

az webapp config connection-string set \
  --name $WEB_APP_NAME \
  --resource-group $RESOURCE_GROUP \
  --connection-string-type SQLAzure \
  --settings DefaultConnection="$SQL_CONNECTION_STRING"
```

### 2.3 - Habilitar HTTPS e HTTP/2
```bash
az webapp update \
  --name $WEB_APP_NAME \
  --resource-group $RESOURCE_GROUP \
  --https-only true \
  --http20-enabled true
```

---

## 📦 PASSO 3: Deploy da Aplicação

### 3.1 - Publicar aplicação localmente
```bash
cd AgilePredict
dotnet publish -c Release -o ./publish
```

### 3.2 - Deploy via Azure CLI
```bash
cd publish
zip -r ../deploy.zip .
cd ..

az webapp deployment source config-zip \
  --name $WEB_APP_NAME \
  --resource-group $RESOURCE_GROUP \
  --src deploy.zip
```

### 3.3 - OU Deploy via GitHub Actions (recomendado)
Crie o arquivo `.github/workflows/azure-deploy.yml`:

```yaml
name: Deploy to Azure App Service

on:
  push:
	branches: [ main ]
  workflow_dispatch:

jobs:
  deploy:
	runs-on: ubuntu-latest

	steps:
	- uses: actions/checkout@v3

	- name: Setup .NET
	  uses: actions/setup-dotnet@v3
	  with:
		dotnet-version: '8.0.x'

	- name: Restore dependencies
	  run: dotnet restore AgilePredict/AgilePredict.csproj

	- name: Build
	  run: dotnet build AgilePredict/AgilePredict.csproj -c Release --no-restore

	- name: Publish
	  run: dotnet publish AgilePredict/AgilePredict.csproj -c Release -o ./publish

	- name: Deploy to Azure
	  uses: azure/webapps-deploy@v2
	  with:
		app-name: ${{ secrets.AZURE_WEBAPP_NAME }}
		publish-profile: ${{ secrets.AZURE_WEBAPP_PUBLISH_PROFILE }}
		package: ./publish
```

---

## 🔍 PASSO 4: Verificar Deploy

### 4.1 - Obter URL da aplicação
```bash
az webapp show \
  --name $WEB_APP_NAME \
  --resource-group $RESOURCE_GROUP \
  --query defaultHostName \
  --output tsv
```

### 4.2 - Testar endpoint
```bash
WEBAPP_URL=$(az webapp show --name $WEB_APP_NAME --resource-group $RESOURCE_GROUP --query defaultHostName -o tsv)

# Testar health check
curl https://$WEBAPP_URL/api/ai/health

# Testar endpoint de LLM
curl -X POST https://$WEBAPP_URL/api/ai/test \
  -H "Content-Type: application/json" \
  -d '{
	"prompt": "Diga apenas: Deployment OK",
	"temperature": 0.3,
	"maxTokens": 50
  }'
```

### 4.3 - Ver logs em tempo real
```bash
az webapp log tail \
  --name $WEB_APP_NAME \
  --resource-group $RESOURCE_GROUP
```

---

## 📊 PASSO 5: Configurar Monitoramento

### 5.1 - Habilitar Application Insights
```bash
az monitor app-insights component create \
  --app $WEB_APP_NAME-insights \
  --location $LOCATION \
  --resource-group $RESOURCE_GROUP \
  --application-type web

# Obter Instrumentation Key
INSTRUMENTATION_KEY=$(az monitor app-insights component show \
  --app $WEB_APP_NAME-insights \
  --resource-group $RESOURCE_GROUP \
  --query instrumentationKey -o tsv)

# Configurar no Web App
az webapp config appsettings set \
  --name $WEB_APP_NAME \
  --resource-group $RESOURCE_GROUP \
  --settings \
	APPLICATIONINSIGHTS_CONNECTION_STRING="InstrumentationKey=$INSTRUMENTATION_KEY"
```

### 5.2 - Configurar alertas
```bash
# Alerta de erro 500
az monitor metrics alert create \
  --name "high-error-rate" \
  --resource-group $RESOURCE_GROUP \
  --scopes "/subscriptions/{subscription-id}/resourceGroups/$RESOURCE_GROUP/providers/Microsoft.Web/sites/$WEB_APP_NAME" \
  --condition "count Http5xx > 10" \
  --window-size 5m \
  --evaluation-frequency 1m
```

---

## 🔐 PASSO 6: Segurança em Produção

### 6.1 - Configurar Managed Identity (para Azure Key Vault)
```bash
# Habilitar Managed Identity
az webapp identity assign \
  --name $WEB_APP_NAME \
  --resource-group $RESOURCE_GROUP

# Obter Principal ID
PRINCIPAL_ID=$(az webapp identity show \
  --name $WEB_APP_NAME \
  --resource-group $RESOURCE_GROUP \
  --query principalId -o tsv)

# Criar Key Vault
az keyvault create \
  --name "kv-agilepredict-prod" \
  --resource-group $RESOURCE_GROUP \
  --location $LOCATION

# Dar acesso ao Web App
az keyvault set-policy \
  --name "kv-agilepredict-prod" \
  --object-id $PRINCIPAL_ID \
  --secret-permissions get list
```

### 6.2 - Mover API Key para Key Vault
```bash
# Adicionar secret no Key Vault
az keyvault secret set \
  --vault-name "kv-agilepredict-prod" \
  --name "LlmApiKey" \
  --value "sk-sua-chave-openai-aqui"

# Referenciar no Web App (syntax especial)
az webapp config appsettings set \
  --name $WEB_APP_NAME \
  --resource-group $RESOURCE_GROUP \
  --settings \
	LlmSettings__ApiKey="@Microsoft.KeyVault(SecretUri=https://kv-agilepredict-prod.vault.azure.net/secrets/LlmApiKey/)"
```

---

## 🧪 PASSO 7: Executar Migrations no SQL Azure

### 7.1 - Criar script de migration
```bash
cd AgilePredict
dotnet ef migrations script --output ./migrations.sql
```

### 7.2 - Executar no Azure SQL
```bash
# Via Azure CLI
az sql db query \
  --server $SQL_SERVER_NAME \
  --database $SQL_DB_NAME \
  --admin-user $SQL_ADMIN_USER \
  --admin-password $SQL_ADMIN_PASSWORD \
  --query-file migrations.sql
```

---

## 📋 CHECKLIST DE PRODUÇÃO

- [ ] API Key armazenada em Azure Key Vault (não em appsettings)
- [ ] HTTPS habilitado (HTTP desabilitado)
- [ ] Connection string com senha forte
- [ ] Application Insights configurado
- [ ] Alertas de erro configurados
- [ ] Logs habilitados e sendo monitorados
- [ ] Firewall do SQL Server configurado
- [ ] Backup automático do banco configurado
- [ ] Staging slot criado para testes (opcional)
- [ ] CI/CD via GitHub Actions funcionando

---

## 💰 ESTIMATIVA DE CUSTOS (Região Brazil South)

| Recurso | SKU | Custo Mensal (USD) |
|---------|-----|-------------------|
| App Service | B1 (Basic) | ~$13 |
| SQL Database | Basic (2GB) | ~$5 |
| Application Insights | Pay-as-you-go | ~$2-5 |
| **TOTAL** | | **~$20-23/mês** |

**Nota:** Pode usar tier Free (F1) para testes, mas tem limitações de performance.

---

## 🔄 ROLLBACK EM CASO DE PROBLEMAS

```bash
# Listar deployments
az webapp deployment list \
  --name $WEB_APP_NAME \
  --resource-group $RESOURCE_GROUP

# Voltar para deployment anterior
az webapp deployment source delete \
  --name $WEB_APP_NAME \
  --resource-group $RESOURCE_GROUP \
  --slot production
```

---

## 📞 SUPORTE

- Azure Status: https://status.azure.com
- Documentação: https://docs.microsoft.com/azure
- Suporte: https://portal.azure.com (abrir ticket)
