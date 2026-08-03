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
