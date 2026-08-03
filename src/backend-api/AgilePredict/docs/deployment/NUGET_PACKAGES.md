# 📦 Pacotes NuGet Necessários para US-21

## Comandos para Instalar Pacotes

Execute os comandos abaixo na pasta `AgilePredict`:

```bash
cd AgilePredict

# Pacote para Polly (resiliência HTTP)
dotnet add package Microsoft.Extensions.Http.Polly --version 8.0.0

# Pacote base do Polly
dotnet add package Polly.Extensions.Http --version 3.0.0

# Pacote para Options Pattern com validação
dotnet add package Microsoft.Extensions.Options.DataAnnotations --version 8.0.0

# Pacote para Application Insights (opcional - monitoramento)
dotnet add package Microsoft.ApplicationInsights.AspNetCore --version 2.21.0

# Pacote para Health Checks (opcional)
dotnet add package Microsoft.Extensions.Diagnostics.HealthChecks --version 8.0.0
```

## Verificar Instalação

```bash
dotnet list package
```

## Estrutura Esperada do .csproj

Depois de instalar, seu `AgilePredict.csproj` deve conter:

```xml
<Project Sdk="Microsoft.NET.Sdk.Web">

  <PropertyGroup>
	<TargetFramework>net8.0</TargetFramework>
	<Nullable>enable</Nullable>
	<ImplicitUsings>enable</ImplicitUsings>
	<GenerateDocumentationFile>true</GenerateDocumentationFile>
	<NoWarn>$(NoWarn);1591</NoWarn>
  </PropertyGroup>

  <ItemGroup>
	<PackageReference Include="Microsoft.ApplicationInsights.AspNetCore" Version="2.21.0" />
	<PackageReference Include="Microsoft.AspNetCore.OpenApi" Version="8.0.0" />
	<PackageReference Include="Microsoft.EntityFrameworkCore" Version="8.0.0" />
	<PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="8.0.0" />
	<PackageReference Include="Microsoft.EntityFrameworkCore.Tools" Version="8.0.0">
	  <PrivateAssets>all</PrivateAssets>
	  <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
	</PackageReference>
	<PackageReference Include="Microsoft.Extensions.Diagnostics.HealthChecks" Version="8.0.0" />
	<PackageReference Include="Microsoft.Extensions.Http.Polly" Version="8.0.0" />
	<PackageReference Include="Microsoft.Extensions.Options.DataAnnotations" Version="8.0.0" />
	<PackageReference Include="Polly.Extensions.Http" Version="3.0.0" />
	<PackageReference Include="Swashbuckle.AspNetCore" Version="6.5.0" />
  </ItemGroup>

</Project>
```

## Notas Importantes

- **Polly**: Fornece políticas de retry, circuit breaker, timeout
- **Options.DataAnnotations**: Permite validar configurações com Data Annotations
- **Application Insights**: Telemetria e monitoramento (opcional mas recomendado para prod)
- **Health Checks**: Permite criar endpoints de health check customizados
