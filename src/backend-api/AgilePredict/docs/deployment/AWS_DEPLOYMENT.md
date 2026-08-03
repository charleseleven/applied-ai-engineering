# 🚀 Deployment para AWS Elastic Beanstalk - AgilePredict API

## 📋 PRÉ-REQUISITOS

- AWS CLI instalado: `aws --version`
- EB CLI instalado: `eb --version`
- .NET 8.0 SDK instalado
- Conta AWS ativa
- API Key da OpenAI (ou outra LLM)

---

## 🔧 PASSO 1: Configurar AWS CLI

```bash
# Configurar credenciais
aws configure

# Definir região
AWS_REGION="sa-east-1"  # São Paulo
```

---

## 📦 PASSO 2: Preparar Aplicação para AWS

### 2.1 - Instalar EB CLI
```bash
pip install awsebcli --upgrade --user
```

### 2.2 - Inicializar aplicação Elastic Beanstalk
```bash
cd AgilePredict
eb init -p "64bit Amazon Linux 2023 v3.1.0 running .NET 8" \
  --region $AWS_REGION \
  agilepredict-api

# Criar arquivo .ebextensions para configurações
mkdir -p .ebextensions
```

### 2.3 - Criar configuração para HTTPS
Crie `.ebextensions/https-redirect.config`:
```yaml
Resources:
  AWSEBV2LoadBalancerListener:
	Type: AWS::ElasticLoadBalancingV2::Listener
	Properties:
	  DefaultActions:
		- Type: redirect
		  RedirectConfig:
			Protocol: HTTPS
			Port: '443'
			Host: '#{host}'
			Path: '/#{path}'
			Query: '#{query}'
			StatusCode: HTTP_301
	  LoadBalancerArn:
		Ref: AWSEBV2LoadBalancer
	  Port: 80
	  Protocol: HTTP
```

---

## 🔐 PASSO 3: Configurar Secrets

### 3.1 - Usar AWS Secrets Manager
```bash
# Criar secret para API Key da LLM
aws secretsmanager create-secret \
  --name agilepredict/llm-api-key \
  --description "OpenAI API Key for AgilePredict" \
  --secret-string "sk-sua-chave-openai-aqui" \
  --region $AWS_REGION

# Criar secret para Connection String
aws secretsmanager create-secret \
  --name agilepredict/connection-string \
  --description "Database Connection String" \
  --secret-string "Server=your-rds-endpoint;Database=AgilePredictDb;User=admin;Password=YourPassword;" \
  --region $AWS_REGION
```

### 3.2 - Configurar IAM Role para acesso aos secrets
Crie `.ebextensions/secrets-policy.config`:
```yaml
Resources:
  AWSEBAutoScalingGroup:
	Metadata:
	  AWS::CloudFormation::Authentication:
		S3Auth:
		  type: "s3"
		  buckets: ["elasticbeanstalk-*"]
		  roleName: 
			"Fn::GetOptionSetting": 
			  Namespace: "aws:autoscaling:launchconfiguration"
			  OptionName: "IamInstanceProfile"
			  DefaultValue: "aws-elasticbeanstalk-ec2-role"

option_settings:
  aws:elasticbeanstalk:application:environment:
	LlmSettings__ApiKey: '{{resolve:secretsmanager:agilepredict/llm-api-key:SecretString}}'
	ConnectionStrings__DefaultConnection: '{{resolve:secretsmanager:agilepredict/connection-string:SecretString}}'
	LlmSettings__ApiUrl: "https://api.openai.com"
	LlmSettings__DefaultModel: "gpt-3.5-turbo"
	LlmSettings__TimeoutSeconds: "30"
	LlmSettings__MaxRetries: "3"
	ASPNETCORE_ENVIRONMENT: "Production"
```

---

## 🚀 PASSO 4: Deploy

### 4.1 - Criar ambiente de produção
```bash
eb create agilepredict-prod-env \
  --platform "64bit Amazon Linux 2023 v3.1.0 running .NET 8" \
  --instance-type t3.small \
  --region $AWS_REGION \
  --envvars \
	ASPNETCORE_ENVIRONMENT=Production,\
	LlmSettings__ApiUrl=https://api.openai.com
```

### 4.2 - Deploy da aplicação
```bash
# Build e deploy
dotnet publish -c Release

# Deploy para EB
eb deploy agilepredict-prod-env
```

### 4.3 - Verificar status
```bash
eb status agilepredict-prod-env
eb health agilepredict-prod-env
```

---

## 🗄️ PASSO 5: Configurar RDS (PostgreSQL/MySQL)

### 5.1 - Criar RDS Instance
```bash
aws rds create-db-instance \
  --db-instance-identifier agilepredict-db \
  --db-instance-class db.t3.micro \
  --engine sqlserver-ex \
  --master-username admin \
  --master-user-password YourStrongPassword123 \
  --allocated-storage 20 \
  --region $AWS_REGION
```

### 5.2 - Atualizar Connection String no Secrets Manager
```bash
# Obter endpoint do RDS
RDS_ENDPOINT=$(aws rds describe-db-instances \
  --db-instance-identifier agilepredict-db \
  --query 'DBInstances[0].Endpoint.Address' \
  --output text \
  --region $AWS_REGION)

# Atualizar secret
aws secretsmanager update-secret \
  --secret-id agilepredict/connection-string \
  --secret-string "Server=$RDS_ENDPOINT;Database=AgilePredictDb;User=admin;Password=YourStrongPassword123;" \
  --region $AWS_REGION
```

---

## 📊 PASSO 6: Configurar CloudWatch

### 6.1 - Habilitar logs
```bash
eb logs --cloudwatch-logs enable agilepredict-prod-env
```

### 6.2 - Criar alarme de erro
```bash
aws cloudwatch put-metric-alarm \
  --alarm-name agilepredict-high-error-rate \
  --alarm-description "Alert when error rate is high" \
  --metric-name HTTP5XXErrors \
  --namespace AWS/ElasticBeanstalk \
  --statistic Sum \
  --period 300 \
  --threshold 10 \
  --comparison-operator GreaterThanThreshold \
  --evaluation-periods 1 \
  --region $AWS_REGION
```

---

## 🔄 PASSO 7: Configurar CI/CD com GitHub Actions

Crie `.github/workflows/aws-deploy.yml`:
```yaml
name: Deploy to AWS Elastic Beanstalk

on:
  push:
	branches: [ main ]
  workflow_dispatch:

env:
  AWS_REGION: sa-east-1
  EB_APPLICATION_NAME: agilepredict-api
  EB_ENVIRONMENT_NAME: agilepredict-prod-env

jobs:
  deploy:
	runs-on: ubuntu-latest

	steps:
	- uses: actions/checkout@v3

	- name: Setup .NET
	  uses: actions/setup-dotnet@v3
	  with:
		dotnet-version: '8.0.x'

	- name: Build and Publish
	  run: |
		cd AgilePredict
		dotnet publish -c Release -o ../publish

	- name: Generate deployment package
	  run: |
		cd publish
		zip -r ../deploy.zip .

	- name: Configure AWS credentials
	  uses: aws-actions/configure-aws-credentials@v2
	  with:
		aws-access-key-id: ${{ secrets.AWS_ACCESS_KEY_ID }}
		aws-secret-access-key: ${{ secrets.AWS_SECRET_ACCESS_KEY }}
		aws-region: ${{ env.AWS_REGION }}

	- name: Deploy to Elastic Beanstalk
	  run: |
		aws elasticbeanstalk create-application-version \
		  --application-name ${{ env.EB_APPLICATION_NAME }} \
		  --version-label ${{ github.sha }} \
		  --source-bundle S3Bucket="elasticbeanstalk-${{ env.AWS_REGION }}-$(aws sts get-caller-identity --query Account --output text)",S3Key="${{ github.sha }}.zip" \
		  --region ${{ env.AWS_REGION }}

		aws s3 cp deploy.zip s3://elasticbeanstalk-${{ env.AWS_REGION }}-$(aws sts get-caller-identity --query Account --output text)/${{ github.sha }}.zip

		aws elasticbeanstalk update-environment \
		  --application-name ${{ env.EB_APPLICATION_NAME }} \
		  --environment-name ${{ env.EB_ENVIRONMENT_NAME }} \
		  --version-label ${{ github.sha }} \
		  --region ${{ env.AWS_REGION }}
```

---

## 🧪 PASSO 8: Testar Deployment

```bash
# Obter URL do ambiente
EB_URL=$(eb status agilepredict-prod-env --verbose | grep CNAME | awk '{print $2}')

# Testar health check
curl https://$EB_URL/api/ai/health

# Testar endpoint LLM
curl -X POST https://$EB_URL/api/ai/test \
  -H "Content-Type: application/json" \
  -d '{
	"prompt": "AWS Deployment OK?",
	"temperature": 0.3,
	"maxTokens": 50
  }'
```

---

## 💰 ESTIMATIVA DE CUSTOS (Região São Paulo)

| Recurso | SKU | Custo Mensal (USD) |
|---------|-----|-------------------|
| EC2 (t3.small) | 2 instâncias | ~$30 |
| RDS (db.t3.micro) | SQL Server Express | ~$15 |
| Load Balancer | Application LB | ~$22 |
| CloudWatch | Logs + Metrics | ~$5 |
| **TOTAL** | | **~$72/mês** |

**Nota:** Use t3.micro ou t2.micro para reduzir custos em desenvolvimento.

---

## 🔄 COMANDOS ÚTEIS

```bash
# Ver logs em tempo real
eb logs -f

# SSH no servidor
eb ssh

# Escalar horizontalmente
eb scale 3

# Reiniciar aplicação
eb restart

# Rollback para versão anterior
eb deploy --version previous-version-label
```

---

## 📞 SUPORTE

- AWS Status: https://status.aws.amazon.com
- Documentação: https://docs.aws.amazon.com
- Suporte: https://console.aws.amazon.com/support
