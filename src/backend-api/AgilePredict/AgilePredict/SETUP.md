# AgilePredict API - Setup Guide

## Prerequisites

- .NET 10 SDK
- SQL Server (LocalDB or Express)
- Groq API Key (get one at https://console.groq.com)

## Configuration

### 1. User Secrets Setup

The application uses User Secrets to store sensitive information like API keys. To configure:

```bash
cd AgilePredict
dotnet user-secrets set "LlmSettings:ApiKey" "your-groq-api-key-here"
```

The flow diagnostics endpoint (`POST /api/flow-diagnostics`) works with **any** Azure DevOps
organization/project the caller has access to — organization, project and the Personal Access
Token (PAT, needs "Work Items (Read)" scope) are normally sent per-request (paste any Azure
Boards URL from that organization, plus the PAT). This lets a Scrum Master analyze different
organizations/projects without restarting or reconfiguring the server.

If you mostly use a single organization, you can optionally set default values so the request
body can omit them:

```bash
dotnet user-secrets set "AzureDevOpsSettings:Organization" "your-organization"
dotnet user-secrets set "AzureDevOpsSettings:Project" "your-project"
dotnet user-secrets set "AzureDevOpsSettings:PersonalAccessToken" "your-pat-here"
```

### 2. Database Setup

Update the connection string in `appsettings.json` if needed, or use User Secrets:

```bash
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "your-connection-string"
```

### 3. Run Migrations

```bash
dotnet ef database update
```

### 4. Run the Application

```bash
dotnet run
```

The API will be available at:
- HTTPS: https://localhost:7xxx
- HTTP: http://localhost:5xxx

## API Documentation

When running in Development mode, access the interactive API documentation at:
- Scalar UI: https://localhost:7xxx/scalar/v1
- OpenAPI spec: https://localhost:7xxx/openapi/v1.json

## Health Checks

Monitor application health at:
- https://localhost:7xxx/health

## Environment Variables (Alternative to User Secrets)

You can also use environment variables:

```bash
# Windows PowerShell
$env:LlmSettings__ApiKey = "your-groq-api-key-here"

# Windows Command Prompt
set LlmSettings__ApiKey=your-groq-api-key-here

# Linux/MacOS
export LlmSettings__ApiKey="your-groq-api-key-here"
```

Note: Use double underscores (`__`) for nested configuration values.
