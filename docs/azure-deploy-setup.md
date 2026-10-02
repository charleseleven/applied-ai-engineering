# Deploy no Azure — setup manual (Tasks #158, #159)

O workflow `.github/workflows/deploy.yml` já faz build do backend (.NET) e do frontend (Nuxt) em todo push para `main`. Os jobs de **deploy** ficam "pendentes" (pulados com um aviso) até você configurar os recursos no Azure e os segredos abaixo no GitHub — nada é criado automaticamente na sua assinatura.

Pré-requisito: Azure CLI já instalada e autenticada nesta máquina (`az login`), conforme configurado nas PBIs #180-#184.

## 1. Backend — App Service (tier F1, gratuito)

```bash
# Ajuste os nomes conforme preferir (o nome do Web App precisa ser único globalmente no Azure)
RESOURCE_GROUP="agilepredict-rg"
LOCATION="brazilsouth"
APP_SERVICE_PLAN="agilepredict-plan"
WEBAPP_NAME="agilepredict-api-SEUNOME"

az group create --name $RESOURCE_GROUP --location $LOCATION

az appservice plan create \
  --name $APP_SERVICE_PLAN \
  --resource-group $RESOURCE_GROUP \
  --sku F1 \
  --is-linux

az webapp create \
  --name $WEBAPP_NAME \
  --resource-group $RESOURCE_GROUP \
  --plan $APP_SERVICE_PLAN \
  --runtime "DOTNETCORE:10.0"

# Baixa o publish profile (contém a credencial de deploy)
az webapp deployment list-publishing-profiles \
  --name $WEBAPP_NAME \
  --resource-group $RESOURCE_GROUP \
  --xml > publish-profile.xml
```

No GitHub: **Settings → Secrets and variables → Actions**
- Secret `AZURE_WEBAPP_PUBLISH_PROFILE` → cole o conteúdo de `publish-profile.xml` (depois apague esse arquivo local, ele contém uma credencial).
- Variable (não secret) `AZURE_WEBAPP_NAME` → o valor de `$WEBAPP_NAME`.

⚠️ Antes de rodar o deploy de verdade, também é preciso configurar a connection string do SQL Server e os secrets de LLM/Gemini como **Application Settings** do Web App (`az webapp config appsettings set`), já que o F1 não roda `dotnet user-secrets`.

## 2. Frontend — Static Web Apps (tier gratuito)

```bash
SWA_NAME="agilepredict-frontend-SEUNOME"

az staticwebapp create \
  --name $SWA_NAME \
  --resource-group $RESOURCE_GROUP \
  --location "eastus2" \
  --sku Free

# Pega o deployment token
az staticwebapp secrets list \
  --name $SWA_NAME \
  --resource-group $RESOURCE_GROUP \
  --query "properties.apiKey" -o tsv
```

No GitHub: secret `AZURE_STATIC_WEB_APPS_API_TOKEN` → o token retornado acima.

## 3. Custo zero (Task #159)

- App Service **F1**: gratuito, mas com limites (60 min de CPU/dia, sem SSL customizado, "sleep" quando ocioso). Suficiente para validar a arquitetura AI-First sem custo.
- Static Web Apps **Free**: gratuito de verdade (100 GB de banda/mês), ideal para a SPA Nuxt gerada estaticamente (`pnpm run generate`).
- Nenhum dos dois cobra nada enquanto ficar dentro desses limites — não é trial, é tier permanente gratuito.

## 4. Depois de configurar

Rode o workflow manualmente (`Actions → Build & Deploy (AgilePredict) → Run workflow`) ou dê push em `main` — os jobs `deploy-backend`/`deploy-frontend` vão parar de ser pulados assim que os secrets acima existirem.
