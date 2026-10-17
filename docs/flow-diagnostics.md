# Diagnóstico Preditivo de Fluxo (Epic #209)

Documentação técnica da feature "Scrum Master - Inteligência Preditiva e Diagnóstico de
Gargalos em Fluxos Ágeis", cobrindo os itens de trabalho do Azure DevOps:

| Item | Tipo | Título | Estado |
|---|---|---|---|
| [#209](https://dev.azure.com/eleven11C/Applied%20AI%20Engineering/_workitems/edit/209) | Epic | Scrum Master - Inteligência Preditiva e Diagnóstico de Gargalos em Fluxos Ágeis | Done |
| [#210](https://dev.azure.com/eleven11C/Applied%20AI%20Engineering/_workitems/edit/210) | Feature | Diagnóstico Preditivo de Gargalos de Fluxo | Done |
| [#211](https://dev.azure.com/eleven11C/Applied%20AI%20Engineering/_workitems/edit/211) | PBI (US) | Como Scrum Master, quero visualizar o Cycle Time de cada card comparado à média histórica | Done |
| [#212](https://dev.azure.com/eleven11C/Applied%20AI%20Engineering/_workitems/edit/212) | Task | Coleta de dados de Work Items via API do Azure DevOps | Done |
| [#213](https://dev.azure.com/eleven11C/Applied%20AI%20Engineering/_workitems/edit/213) | Task | Cálculo estatístico (Cycle Time, Lead Time, baseline) | Done |
| [#214](https://dev.azure.com/eleven11C/Applied%20AI%20Engineering/_workitems/edit/214) | Task | Geração de insights de diagnóstico via LLM | Done |
| [#215](https://dev.azure.com/eleven11C/Applied%20AI%20Engineering/_workitems/edit/215) | Task | Painel visual de diagnóstico (frontend) | Done |

## 1. Objetivo e critérios de aceitação (US #211)

> Como Scrum Master, quero visualizar o Cycle Time de cada card comparado à média histórica da
> equipe, para identificar gargalos de fluxo antes que afetem o prazo da sprint.

- **AC #1** — O Cycle Time de cada card ativo é comparado a uma linha de base estatística
  construída a partir dos últimos 90 dias de itens concluídos.
- **AC #2** — Cards que ultrapassam o tempo de permanência tolerado em um status crítico (ex.:
  "Code Review > 3 dias") são destacados como alerta de gargalo, junto com o responsável.
- **AC #3** — O painel exibe sugestões textuais de mitigação geradas por IA a partir do
  diagnóstico estatístico.

A feature funciona com **qualquer organização/projeto do Azure DevOps** ao qual o usuário tenha
acesso — não fica presa à organização configurada no servidor (ver seção 5).

## 2. Arquitetura

```
Frontend (Nuxt)                  Backend (.NET)
──────────────                   ───────────────
flow-diagnostics.vue      POST    FlowDiagnosticsController
  │  (form: Boards URL,  ──────►    │  resolve org/project/PAT
  │   iteration path, PAT)          │  (URL colada OU campos
  ▼                                 │   explícitos OU fallback
useFlowDiagnostics.ts               │   do appsettings/user-secrets)
  │  (useState compartilhado)       ▼
  ▼                            FlowDiagnosticsOrchestrator
FlowDiagnosticsPanel.vue            │
  (KPIs, tabela de gargalos,        ├──► AzureDevOpsWorkItemFlowDataSource  (Task #212)
   tabela de cards, vazão           │      WIQL + workitemsbatch + /updates (histórico de status)
   semanal, insight de IA)          │
                                    ├──► FlowMetricsCalculator              (Task #213)
                                    │      Cycle Time, Lead Time, baseline por status,
                                    │      vazão semanal, detecção de gargalo
                                    │
                                    └──► FlowInsightGenerator               (Task #214)
                                           prompt de sistema + ILlmIntegrationService (Groq)
                                           → resumo + recomendações em JSON
```

Toda a análise é **sob demanda** (`POST /api/flow-diagnostics`), diferente do agente autônomo de
monitoramento de sprints (PBI #150/Epic #142), que roda em background sobre os dados internos do
próprio `AgilePredictDb` — são duas features independentes, sem overlap de código.

## 3. Backend — arquivos e responsabilidades

### Task #212 — Coleta de dados (`AzureDevOpsWorkItemFlowDataSource.cs`)
[`Services/AzureDevOpsWorkItemFlowDataSource.cs`](../src/backend-api/AgilePredict/AgilePredict/Services/AzureDevOpsWorkItemFlowDataSource.cs)

Implementa `IWorkItemFlowDataSource` usando a API REST do Azure DevOps diretamente (sem SDK):

1. **`GetActiveWorkItemsAsync`** — roda uma consulta WIQL filtrando por `System.IterationPath`
   e excluindo os status configurados em `DoneStatuses`/`Removed`.
2. **`GetCompletedWorkItemsSinceAsync`** — WIQL filtrando itens já concluídos (`DoneStatuses`)
   com `ChangedDate` dentro da janela de baseline (90 dias por padrão).
3. Para cada ID retornado, busca os campos (`workitemsbatch`) e o **histórico de transições de
   status** (`/_apis/wit/workitems/{id}/updates`), reconstruindo uma lista cronológica de
   `WorkItemStatusPeriod` (status + quando entrou/saiu).
4. O `Authorization` (Basic, PAT em Base64) é montado **por requisição** a partir da
   `AzureDevOpsConnection` recebida — nunca fixado no `HttpClient` compartilhado — para que
   requisições concorrentes de organizações diferentes nunca vazem PAT entre si.

Saída normalizada: [`WorkItemFlowSnapshot`](../src/backend-api/AgilePredict/AgilePredict/Models/Flow/WorkItemFlowSnapshot.cs)
(`ExternalId`, `Title`, `AssignedTo`, `CreatedAtUtc`, `CurrentStatus`, `IsDone`, `StatusHistory`).

### Task #213 — Cálculo estatístico (`FlowMetricsCalculator.cs`)
[`Services/FlowMetricsCalculator.cs`](../src/backend-api/AgilePredict/AgilePredict/Services/FlowMetricsCalculator.cs)

Serviço puro (sem I/O), testável isoladamente a partir de snapshots já coletados:

- **Cycle Time** de um card = soma das durações nos status configurados como "trabalho ativo"
  (`StartStatuses`: `In Progress`, `Doing`, `Code Review` por padrão).
- **Lead Time** = tempo total desde a criação do card até agora.
- **Baseline por status** (`StatusBaselineStats`): para cada status, calcula média e desvio
  padrão da duração histórica (itens concluídos nos últimos 90 dias). O limite de gargalo
  (`ThresholdHours`) é `média + 1.5×desvio padrão` quando há amostra suficiente
  (`MinimumSampleSizeForBaseline = 5`); caso contrário, usa um limite fixo de
  `DefaultCriticalThresholdDays = 3` dias (reflete literalmente o exemplo do AC #2:
  "Code Review > 3 dias").
- **Baseline de Cycle Time**: mesma lógica (média + 1.5×desvio) aplicada ao Cycle Time total dos
  itens concluídos, para marcar `IsCycleTimeAboveBaseline` em cada card ativo (AC #1).
- **Gargalo** (`IsCurrentStatusBottleneck`): verdadeiro quando o tempo no status atual excede o
  `ThresholdHours` daquele status. Severidade é `Critical` quando excede 1.5× o limite, senão
  `Warning`.
- **Vazão semanal** (`WeeklyThroughput`): contagem de itens concluídos agrupados por semana
  (segunda-feira como início), dentro da janela de baseline.

Saída: [`FlowMetricsPayload`](../src/backend-api/AgilePredict/AgilePredict/Models/DTOs/Flow/FlowMetricsPayload.cs)
— o contrato JSON compartilhado entre a camada de IA (Task #214) e o frontend (Task #215).

### Task #214 — Insight via LLM (`FlowInsightGenerator.cs`)
[`Services/FlowInsightGenerator.cs`](../src/backend-api/AgilePredict/AgilePredict/Services/FlowInsightGenerator.cs)

- Serializa o `FlowMetricsPayload` inteiro como JSON e envia para `ILlmIntegrationService`
  (mesma infraestrutura de LLM usada pelo chat/playground do projeto, hoje apontando para a API
  da Groq — `LlmSettings` em `appsettings.json`/user-secrets) junto com um **system prompt**
  especializado em métricas de fluxo/Scrum, instruindo a LLM a:
  - priorizar cards com `isCurrentStatusBottleneck` e `isCycleTimeAboveBaseline` verdadeiros;
  - citar título do card e responsável nas recomendações;
  - responder **estritamente em JSON**: `{"summary": "...", "recommendations": ["...", "..."]}`.
- `Temperature = 0.3`, `MaxTokens = 700` (resposta objetiva, baixa criatividade).
- Se a LLM não devolver JSON válido, há um **fallback textual**: usa a resposta bruta como
  resumo e tenta extrair recomendações de linhas que começam com `-`, `*` ou `1.`/`1)`.
- Se a chamada à LLM falhar completamente (erro de rede, API fora do ar, etc.), o
  `FlowDiagnosticsOrchestrator` captura a exceção e marca `AiInsightUnavailable = true` — **o
  relatório estatístico continua sendo retornado normalmente**, já que as métricas não dependem
  da IA (AC #3 é um complemento, não um bloqueador).

### Orquestração (`FlowDiagnosticsOrchestrator.cs`)
[`Services/FlowDiagnosticsOrchestrator.cs`](../src/backend-api/AgilePredict/AgilePredict/Services/FlowDiagnosticsOrchestrator.cs)

Pipeline único (`GenerateReportAsync`): coleta itens ativos + itens concluídos (baseline) →
calcula métricas → tenta gerar insight de IA → monta o `FlowDiagnosticReport` final.

### Endpoint HTTP (`FlowDiagnosticsController.cs`)
[`Controllers/FlowDiagnosticsController.cs`](../src/backend-api/AgilePredict/AgilePredict/Controllers/FlowDiagnosticsController.cs)

`POST /api/flow-diagnostics` (POST em vez de GET porque a requisição pode carregar um PAT, que
nunca deve trafegar em query string/logs).

**Request** (`FlowDiagnosticsRequest`):

```jsonc
{
  "boardsUrl": "https://dev.azure.com/minhaOrg/MeuProjeto/_sprints/taskboard/Time/MeuProjeto/Sprint%204",
  // OU, em vez de boardsUrl: "organization": "minhaOrg", "project": "MeuProjeto"
  "iterationPath": "MeuProjeto\\Sprint 4",   // obrigatório
  "personalAccessToken": "..."               // opcional — ver seção 5 (fallback de servidor)
}
```

Resolução de organização/projeto/PAT, em ordem de prioridade:
1. **`boardsUrl`** — qualquer URL do Azure Boards (work item, board, sprint, backlog...) daquela
   organização; `AzureDevOpsBoardsUrlParser` extrai `{organização, projeto}` automaticamente,
   suportando tanto o domínio moderno (`dev.azure.com/{org}/{project}/...`) quanto o legado
   (`{org}.visualstudio.com/{project}/...`).
2. **`organization` + `project`** explícitos, se `boardsUrl` não for informado.
3. Se nenhum dos dois vier na requisição, usa os valores padrão configurados no servidor
   (`AzureDevOpsSettings:Organization`/`Project`).
4. **`personalAccessToken`**: se omitido, usa `AzureDevOpsSettings:PersonalAccessToken` do
   servidor; se nenhum dos dois existir, retorna `400 Bad Request`.

O controller também **tolera o erro comum de colar a URL da sprint no campo "Caminho da
iteração"**: se o valor começar com `http(s)://`, tenta extrair o Iteration Path automaticamente
(`AzureDevOpsBoardsUrlParser.TryExtractIterationPath`) em vez de enviar a URL literal ao Azure
DevOps (o que geraria uma consulta WIQL malformada).

Erros da API do Azure DevOps (`HttpRequestException`) são traduzidos em mensagens específicas e
retornados como `502 Bad Gateway`: PAT inválido/expirado (401), PAT sem escopo "Work Items
(Read)" (403), organização/projeto não encontrados (404), Iteration Path incorreto (400).

### Configuração (`appsettings.json`)

```json
"FlowAnalyticsSettings": {
  "BaselineWindowDays": 90,
  "StartStatuses": ["In Progress", "Doing", "Code Review"],
  "DoneStatuses": ["Done", "Closed", "Resolved"],
  "DefaultCriticalThresholdDays": 3.0,
  "StdDevMultiplier": 1.5,
  "MinimumSampleSizeForBaseline": 5
},
"AzureDevOpsSettings": {
  "ApiUrl": "https://dev.azure.com/",
  "Organization": "",   // fallback opcional — normalmente vem por requisição
  "Project": "",        // fallback opcional
  "PersonalAccessToken": "", // nunca hardcoded — ver seção 5
  "ApiVersion": "7.1",
  "TimeoutSeconds": 30
}
```

## 4. Frontend — arquivos e responsabilidades

### Task #215 — Painel visual

- [`app/pages/flow-diagnostics.vue`](../src/frontend-app/app/pages/flow-diagnostics.vue) — tela
  com o formulário (URL do Azure Boards, Iteration Path, PAT opcional) e botão "Gerar
  diagnóstico". Disponível em `http://localhost:3000/flow-diagnostics`.
- [`app/composables/useFlowDiagnostics.ts`](../src/frontend-app/app/composables/useFlowDiagnostics.ts) —
  estado compartilhado (`useState`: `report`, `isLoading`, `errorMessage`) e a chamada
  `axios.post(${apiBaseUrl}/api/flow-diagnostics, ...)`. Valida no cliente que `boardsUrl` e
  `iterationPath` não estão vazios antes de disparar a requisição; o PAT é enviado apenas se
  preenchido (`|| undefined`) e **nunca é persistido** pelo composable.
- [`app/components/FlowDiagnosticsPanel.vue`](../src/frontend-app/app/components/FlowDiagnosticsPanel.vue) —
  renderiza o relatório:
  - 3 cartões de KPI: gargalos ativos, cards acima da baseline de Cycle Time, vazão da última
    semana concluída;
  - card de "Diagnóstico gerado por IA" (resumo + lista de recomendações, ou aviso de
    indisponibilidade se `aiInsightUnavailable`);
  - tabela de "Alertas de gargalo por status e responsável" (card, responsável, status, tempo no
    status, limite, severidade com chip colorido Crítico/Atenção);
  - tabela de "Cards ativos da sprint" (Cycle Time com ícone de alerta quando acima da baseline,
    Lead Time);
  - vazão semanal exibida como uma série de valores por semana.

## 5. Como configurar o Personal Access Token padrão do servidor

Para não precisar informar um PAT a cada diagnóstico, configure-o uma vez via
`dotnet user-secrets` (nunca em `appsettings.json`/git):

```bash
cd src/backend-api/AgilePredict/AgilePredict
dotnet user-secrets set "AzureDevOpsSettings:PersonalAccessToken" "SEU_PAT_AQUI"
dotnet user-secrets set "AzureDevOpsSettings:Organization" "eleven11C"
dotnet user-secrets set "AzureDevOpsSettings:Project" "Applied AI Engineering"
```

O PAT precisa do escopo **"Work Items (Read)"** na organização alvo. Com isso configurado, o
campo "Personal Access Token" da tela pode ficar em branco sempre que a sprint analisada for
dessa organização/projeto padrão; para analisar outra organização, informe um PAT daquela
organização no campo (ele é enviado direto ao seu backend local e nunca é salvo).

> Nota: este PAT é **diferente** do PAT usado pelo servidor MCP interno do Claude Code
> (`.mcp/.env`) — aquele é usado apenas por mim (assistente) para ler/atualizar work items
> durante o desenvolvimento; este (`AzureDevOpsSettings`) é o PAT usado pela própria aplicação
> web para os usuários finais da feature de Diagnóstico de Fluxo.

## 6. Como executar e validar a feature

### 6.1 Backend

```bash
cd src/backend-api/AgilePredict/AgilePredict
dotnet run --launch-profile http
```

Confirme que subiu em `http://localhost:5240` e que a documentação interativa está acessível em
`http://localhost:5240/scalar/v1` (ou `/swagger`, conforme configurado).

### 6.2 Frontend

```bash
cd src/frontend-app
pnpm run dev
```

Confirme que subiu em `http://localhost:3000` (ou a próxima porta livre, se a 3000 estiver
ocupada — o terminal do Nuxt informa a porta real).

### 6.3 Passo a passo para validar o Diagnóstico de Fluxo

1. Abra `http://localhost:3000/flow-diagnostics` no navegador.
2. No campo **"URL do Azure Boards"**, cole qualquer link do Azure Boards da organização/projeto
   que deseja analisar — por exemplo, a URL do board ou de um work item. Exemplo real do
   projeto:
   `https://dev.azure.com/eleven11C/Applied%20AI%20Engineering/_boards/board/t/Applied%20AI%20Engineering%20Team/Backlog%20items`
3. No campo **"Caminho da iteração (sprint)"**, você pode:
   - colar a URL da sprint (taskboard ou backlog), ex.:
     `https://dev.azure.com/eleven11C/Applied%20AI%20Engineering/_sprints/taskboard/Applied%20AI%20Engineering%20Team/Applied%20AI%20Engineering/2026-09-Sprint-02`
     — o caminho é extraído automaticamente; ou
   - digitar o Iteration Path exato, ex.: `Applied AI Engineering\2026-09-Sprint-02`.
4. Deixe o campo **"Personal Access Token"** em branco se você já configurou o PAT padrão do
   servidor (seção 5); caso contrário, informe um PAT com escopo "Work Items (Read)" daquela
   organização.
5. Clique em **"Gerar diagnóstico"**. O botão mostra um spinner de carregamento enquanto o
   backend coleta os Work Items (WIQL), monta o histórico de status de cada card e chama a LLM.
6. Resultado esperado na tela:
   - Três cartões de KPI no topo (gargalos ativos, cards acima da baseline, vazão da última
     semana);
   - Um card "Diagnóstico gerado por IA" com um resumo em texto corrido e uma lista de
     recomendações específicas (citando cards e responsáveis);
   - Uma tabela de "Alertas de gargalo" (vazia com mensagem de sucesso, se nenhum card estiver
     travado) com chips coloridos "Crítico"/"Atenção";
   - Uma tabela com todos os cards ativos da sprint, Cycle Time e Lead Time, com um ícone de
     tendência quando o Cycle Time está acima da baseline histórica;
   - A vazão semanal dos últimos ~90 dias.
7. Para testar o tratamento de erro:
   - Informe um PAT inválido → espera-se uma mensagem de erro amigável ("Personal Access Token
     inválido ou expirado").
   - Cole uma URL que não seja do Azure DevOps no campo "URL do Azure Boards" → erro 400 com
     mensagem explicando o formato esperado.
   - Informe um Iteration Path que não existe → a lista de cards ativos volta vazia, mas o
     relatório ainda é gerado.
8. Para testar o fallback de IA: se a chave da LLM (`LlmSettings:ApiKey`, configurada via
   `dotnet user-secrets`) estiver ausente/inválida, o card de IA deve mostrar o aviso "Não foi
   possível gerar o diagnóstico de IA neste momento" — e as demais seções (métricas, gargalos,
   vazão) continuam funcionando normalmente, confirmando o AC #3 como complemento não-bloqueante.

### 6.4 Encerrando os servidores

Depois de validar, finalize os processos de `dotnet run` e `pnpm run dev` (Ctrl+C nos terminais,
ou finalize os processos em segundo plano) para não deixá-los consumindo recursos.
