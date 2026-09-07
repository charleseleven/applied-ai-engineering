---
name: gmud-automation-runner
description: 'Executa a ferramenta GmudAutomation (CLI em src/backend-api/GmudAutomation) para uma GMUD do Azure Boards. Use quando o usuário pedir para "rodar a GMUD", "processar a GMUD", "executar o processo de GMUD", "aplicar/validar a US de GMUD", ou colar uma URL de work item do Azure Boards (dev.azure.com/.../_workitems/edit/...) pedindo para rodar, consultar ou publicar o impacto técnico.'
---

# Executar a automação de GMUD (GmudAutomation)

## Convenção de acionamento
O usuário pode pedir de forma livre, mas os padrões recomendados são:

```
Rodar GMUD: <url-do-work-item-no-azure-boards>                       (Feature 1 apenas — hierarquia + impacto técnico)
GMUD GMUD <sufixo-do-cliente>: <url-do-work-item-no-azure-boards>    (Feature 1 + ambiente "gmud" do cliente, para Feature 2/3)
GMUD PROD <sufixo-do-cliente>: <url-do-work-item-no-azure-boards>    (Feature 1 + ambiente "prd" do cliente, para Feature 2/3)
```

Quando vier no formato `GMUD <AMBIENTE> <sufixo-do-cliente>: <url>`, extraia:
- `<AMBIENTE>` → `GMUD` mapeia para o segmento `gmud` no nome do App Service; `PROD` mapeia para `prd`.
- `<sufixo-do-cliente>` → o sufixo curto usado no nome do App Service (ex: `hur`, `hba`, `mt` — geralmente citado no título/descrição do card da GMUD).

Com isso, os nomes dos App Services ficam determinísticos (padrão `contoso-portalcliente-{api|web}-{ambiente}-{cliente}`,
ver `/memories/repo/contoso-gmud-automation.md`), habilitando também as Features 2/3 (branch de release, merge, PRs)
sem precisar perguntar mais nada de infraestrutura ao usuário. Se o usuário não usar esse formato (só "Rodar GMUD: `<url>`"
ou variações livres), assuma que só a Feature 1 (hierarquia + impacto técnico) deve rodar.

Variações equivalentes que também devem ser reconhecidas para o formato simples: "processar a GMUD <url>", "executa o processo de GMUD para <url>", "valida essa GMUD <url>", ou simplesmente colar a URL do Azure Boards junto com a palavra "GMUD".

## Passo 1 — Decidir o modo (leitura vs. escrita)
- Se o usuário **não disser explicitamente** para publicar/gravar/comentar no card (palavras como "publique", "grave", "poste o comentário", "pode escrever"), rode primeiro em modo **somente leitura**:
  - `hierarchy --url "<url>"` (US 1.1 — mapeia a hierarquia)
  - `impact --url "<url>"` (US 1.2 — identifica impacto técnico, não escreve nada)
- Só rode `run` ou `publish-impact` (que **escrevem** comentários reais no card) depois que o usuário confirmar explicitamente que quer publicar. Isso é uma ação em ambiente compartilhado (Azure Boards real) e deve seguir a mesma cautela usada em ações destrutivas/irreversíveis.

## Passo 2 — Carregar o PAT sem expor o segredo
O PAT nunca deve aparecer no chat. Carregue-o na sessão do terminal a partir de `.mcp/.env` (nunca imprima o valor):

```powershell
$envLine = Get-Content "<raiz-do-repo>\.mcp\.env" | Where-Object { $_ -match '^AZURE_DEVOPS_EXT_PAT=' }
$env:AZURE_DEVOPS_EXT_PAT = ($envLine -split '=',2)[1]
```

Se a organização da URL for diferente de `eleven11C` (ex: `contoso`), use a variável `AZURE_DEVOPS_PAT_<ORG_MAIUSCULO>` em vez de `AZURE_DEVOPS_EXT_PAT` (o próprio CLI resolve isso sozinho, olhando a organização da URL — só é preciso garantir que a variável de ambiente correta esteja carregada na sessão antes de rodar).

## Passo 3 — Rodar o comando
A partir de `src/backend-api/GmudAutomation`:

```powershell
dotnet run --project GmudAutomation.Cli -- <comando> --url "<url>" [--confirm-write]
```

Comandos disponíveis (Feature 1): `hierarchy`, `impact` (somente leitura); `add-comment`, `publish-impact`, `run` (escrevem — exigem `--confirm-write`).

Quando o acionamento veio no formato `GMUD <AMBIENTE> <cliente>: <url>` (Feature 2/3), use também `--ambiente <gmud|prd>` e `--cliente <sufixo>`:

```powershell
dotnet run --project GmudAutomation.Cli -- release-branch --url "<url>" --ambiente gmud --cliente hba --confirm-write
dotnet run --project GmudAutomation.Cli -- pull-requests  --url "<url>" --ambiente gmud --cliente hba --confirm-write
```

- `release-branch` (US 2.1): deriva os repositórios/branches de feature automaticamente a partir do `impact` de cada work item filho, cria a branch de release e mescla sequencialmente. Se der conflito, para imediatamente e mostra o log de arquivos — **não tente resolver sozinho, reporte ao usuário para decisão do Tech Lead**.
- `pull-requests` (US 3.1): roda **depois** de `release-branch` ter criado a branch (usa o mesmo nome de branch, recalculado por `--ambiente`/`--cliente`/data de hoje). PRs de API/WEB/Backend são completadas automaticamente; Database fica só como Draft/Ativa (não completar).
- Repositório sem site conhecido (ex: Database) não passa pela criação de branch de release (não roda em App Service) — a PR de Database sai direto da feature branch identificada.

## Passo 4 — Reportar o resultado
Resuma o resultado (hierarquia encontrada, projetos/branches identificados, branch de release criada, PRs abertas/completadas, se algo foi publicado) e cite os work items e repositórios afetados por ID/nome.

## Reconhecimento automático da organização
O próprio CLI já identifica pela URL se a organização é `contoso` (cliente real — propósito de produção do projeto) ou outra (ex: `eleven11C`, sandbox de testes) e imprime qual delas está sendo usada. Não é necessário perguntar isso ao usuário além de confirmar o modo leitura/escrita.

## Ainda não automatizado (Feature 3, US 3.2)
Renomeio de anexos (Task 206) e publicação do relatório consolidado de release (Task 207/208) existem como serviços no
`Core` (`AttachmentRenamingService`, `ReleaseReportFormatter`, `ReleaseReportPublisher`) mas **não têm comando de CLI
ainda** — dependem de dados reais (anexos, tags de build) que só aparecerão na primeira execução real. Ao rodar uma
GMUD de verdade e chegar nesse ponto, pare e chame o usuário para decidir os próximos passos manualmente ou pedir
para eu terminar o wiring com base no que for observado.
