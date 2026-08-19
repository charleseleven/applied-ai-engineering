import { Server } from "@modelcontextprotocol/sdk/server/index.js";
import { StdioServerTransport } from "@modelcontextprotocol/sdk/server/stdio.js";
import { CallToolRequestSchema, ListToolsRequestSchema } from "@modelcontextprotocol/sdk/types.js";
import { config } from "dotenv";
import { fileURLToPath } from "node:url";
import { dirname, join } from "node:path";

const __dirname = dirname(fileURLToPath(import.meta.url));
config({ path: join(__dirname, ".env") });

const server = new Server({
  name: "azure-devops-mcp",
  version: "2.0.0",
}, {
  capabilities: { tools: {} }
});

// PAT via variável de ambiente (mesmo nome usado pela extensão azure-devops da Azure CLI,
// para não exigir uma variável diferente entre o servidor MCP e validações via CLI).
const PAT = process.env.AZURE_DEVOPS_EXT_PAT;

// Organização/projeto têm um padrão (evita repetir em toda chamada), mas são sempre
// sobrescrevíveis por chamada — o usuário alterna entre várias organizações/projetos,
// então nada aqui pode ficar travado no processo do servidor.
const DEFAULT_ORG = process.env.AZURE_DEVOPS_DEFAULT_ORG || "eleven11C";
const DEFAULT_PROJECT = process.env.AZURE_DEVOPS_DEFAULT_PROJECT || "Applied AI Engineering";

function authHeaders() {
  return { Authorization: "Basic " + Buffer.from(":" + PAT).toString("base64") };
}

async function getWorkItem(id, organization, project) {
  const url = `https://dev.azure.com/${organization}/${encodeURIComponent(project)}/_apis/wit/workitems/${id}?$expand=relations&api-version=7.0`;
  const response = await fetch(url, { headers: authHeaders() });
  if (!response.ok) {
    throw new Error(`Azure DevOps API retornou ${response.status} para o work item ${id}`);
  }
  return response.json();
}

async function getWorkItemsBatch(ids, organization) {
  if (ids.length === 0) return [];
  const url = `https://dev.azure.com/${organization}/_apis/wit/workitems?ids=${ids.join(",")}&api-version=7.0`;
  const response = await fetch(url, { headers: authHeaders() });
  if (!response.ok) {
    throw new Error(`Azure DevOps API retornou ${response.status} buscando os work items ${ids.join(",")}`);
  }
  const data = await response.json();
  return data.value;
}

async function updateWorkItem(id, organization, project, fields) {
  const fieldMap = {
    priority: "Microsoft.VSTS.Common.Priority",
    iterationPath: "System.IterationPath",
    areaPath: "System.AreaPath",
    state: "System.State"
  };

  const patch = Object.entries(fields)
    .filter(([, value]) => value !== undefined && value !== null && value !== "")
    .map(([key, value]) => ({ op: "add", path: `/fields/${fieldMap[key]}`, value }));

  if (patch.length === 0) {
    throw new Error("Nenhum campo informado para atualizar (priority, iterationPath, areaPath ou state).");
  }

  const url = `https://dev.azure.com/${organization}/${encodeURIComponent(project)}/_apis/wit/workitems/${id}?api-version=7.0`;
  const response = await fetch(url, {
    method: "PATCH",
    headers: { ...authHeaders(), "Content-Type": "application/json-patch+json" },
    body: JSON.stringify(patch)
  });

  const rawBody = await response.text();
  let body;
  try {
    body = rawBody ? JSON.parse(rawBody) : null;
  } catch {
    body = null;
  }

  if (!response.ok) {
    // Repassa a mensagem de erro real do Azure DevOps (ex: transição de estado inválida
    // para o tipo de work item, campo somente-leitura, etc.) em vez de mascará-la.
    const detail = body?.message || rawBody || "(corpo de resposta vazio)";
    throw new Error(`Azure DevOps API retornou ${response.status} ao atualizar o work item ${id}: ${detail}`);
  }

  if (!body) {
    throw new Error(`Azure DevOps API retornou status ${response.status} mas com corpo vazio/inválido ao atualizar o work item ${id}.`);
  }

  return body;
}

server.setRequestHandler(ListToolsRequestSchema, async () => ({
  tools: [
    {
      name: "get_work_item",
      description: "Busca um Work Item do Azure DevOps por ID, incluindo descrição, critérios de aceitação e sub-tasks (work items filhos).",
      inputSchema: {
        type: "object",
        properties: {
          id: { type: "number", description: "ID numérico do Work Item" },
          organization: { type: "string", description: `Organização do Azure DevOps (padrão: ${DEFAULT_ORG})` },
          project: { type: "string", description: `Projeto do Azure DevOps (padrão: ${DEFAULT_PROJECT})` }
        },
        required: ["id"]
      }
    },
    {
      name: "update_work_item",
      description: "Atualiza campos de um Work Item do Azure DevOps: prioridade, iteration path (sprint), area path (time) e/ou estado (coluna do board). Informe só os campos que quer alterar.",
      inputSchema: {
        type: "object",
        properties: {
          id: { type: "number", description: "ID numérico do Work Item" },
          priority: { type: "number", description: "Nova prioridade (Microsoft.VSTS.Common.Priority)" },
          iterationPath: { type: "string", description: "Novo Iteration Path (ex: 'Applied AI Engineering\\\\Sprint 3')" },
          areaPath: { type: "string", description: "Novo Area Path (time)" },
          state: { type: "string", description: "Novo estado/coluna (ex: 'Active', 'Resolved', 'Closed') — deve ser um estado válido para o tipo do work item, ou a API retorna erro." },
          organization: { type: "string", description: `Organização do Azure DevOps (padrão: ${DEFAULT_ORG})` },
          project: { type: "string", description: `Projeto do Azure DevOps (padrão: ${DEFAULT_PROJECT})` }
        },
        required: ["id"]
      }
    }
  ]
}));

server.setRequestHandler(CallToolRequestSchema, async (request) => {
  const args = request.params.arguments ?? {};
  const organization = args.organization || DEFAULT_ORG;
  const project = args.project || DEFAULT_PROJECT;

  if (request.params.name === "get_work_item") {
    const { id } = args;
    const item = await getWorkItem(id, organization, project);
    const fields = item.fields;

    const childIds = (item.relations ?? [])
      .filter((r) => r.rel === "System.LinkTypes.Hierarchy-Forward")
      .map((r) => Number(r.url.split("/").pop()));

    const children = await getWorkItemsBatch(childIds, organization);

    const childrenText = children.length
      ? children.map((c) => `  - #${c.id} [${c.fields["System.State"]}] ${c.fields["System.Title"]}`).join("\n")
      : "  (nenhuma)";

    const text = [
      `Work Item ${id}: ${fields["System.Title"]}`,
      `Tipo: ${fields["System.WorkItemType"]} | Estado: ${fields["System.State"]}`,
      `Organização/Projeto: ${organization} / ${project}`,
      "",
      "Descrição:",
      fields["System.Description"] ?? "(sem descrição)",
      "",
      "Critérios de Aceitação:",
      fields["Microsoft.VSTS.Common.AcceptanceCriteria"] ?? "(sem critérios de aceitação)",
      "",
      "Sub-tasks:",
      childrenText
    ].join("\n");

    return { content: [{ type: "text", text }] };
  }

  if (request.params.name === "update_work_item") {
    const { id, priority, iterationPath, areaPath, state } = args;
    try {
      const updated = await updateWorkItem(id, organization, project, { priority, iterationPath, areaPath, state });
      const fields = updated.fields;
      const text = [
        `Work Item ${id} atualizado com sucesso.`,
        `Prioridade: ${fields["Microsoft.VSTS.Common.Priority"]}`,
        `Iteration Path: ${fields["System.IterationPath"]}`,
        `Area Path: ${fields["System.AreaPath"]}`,
        `Estado: ${fields["System.State"]}`
      ].join("\n");
      return { content: [{ type: "text", text }] };
    } catch (error) {
      return { content: [{ type: "text", text: `Falha ao atualizar o Work Item ${id}: ${error.message}` }], isError: true };
    }
  }

  throw new Error(`Unknown tool: ${request.params.name}`);
});

const transport = new StdioServerTransport();
await server.connect(transport);
