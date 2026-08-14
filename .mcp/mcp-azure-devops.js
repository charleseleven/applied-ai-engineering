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
  version: "1.0.0",
}, {
  capabilities: { tools: {} }
});

const PAT = process.env.AZURE_PAT;
const ORG = "eleven11C";
const PROJECT = "Applied AI Engineering";

function authHeaders() {
  return { Authorization: "Basic " + Buffer.from(":" + PAT).toString("base64") };
}

async function getWorkItem(id) {
  const url = `https://dev.azure.com/${ORG}/${encodeURIComponent(PROJECT)}/_apis/wit/workitems/${id}?$expand=relations&api-version=7.0`;
  const response = await fetch(url, { headers: authHeaders() });
  if (!response.ok) {
    throw new Error(`Azure DevOps API returned ${response.status} for work item ${id}`);
  }
  return response.json();
}

async function getWorkItemsBatch(ids) {
  if (ids.length === 0) return [];
  const url = `https://dev.azure.com/${ORG}/_apis/wit/workitems?ids=${ids.join(",")}&api-version=7.0`;
  const response = await fetch(url, { headers: authHeaders() });
  if (!response.ok) {
    throw new Error(`Azure DevOps API returned ${response.status} fetching work items ${ids.join(",")}`);
  }
  const data = await response.json();
  return data.value;
}

server.setRequestHandler(ListToolsRequestSchema, async () => ({
  tools: [{
    name: "get_work_item",
    description: "Busca um Work Item do Azure DevOps por ID, incluindo descrição, critérios de aceitação e sub-tasks (work items filhos).",
    inputSchema: {
      type: "object",
      properties: {
        id: { type: "number", description: "ID numérico do Work Item" }
      },
      required: ["id"]
    }
  }]
}));

server.setRequestHandler(CallToolRequestSchema, async (request) => {
  if (request.params.name !== "get_work_item") {
    throw new Error(`Unknown tool: ${request.params.name}`);
  }

  const { id } = request.params.arguments;
  const item = await getWorkItem(id);
  const fields = item.fields;

  const childIds = (item.relations ?? [])
    .filter((r) => r.rel === "System.LinkTypes.Hierarchy-Forward")
    .map((r) => Number(r.url.split("/").pop()));

  const children = await getWorkItemsBatch(childIds);

  const childrenText = children.length
    ? children.map((c) => `  - #${c.id} [${c.fields["System.State"]}] ${c.fields["System.Title"]}`).join("\n")
    : "  (nenhuma)";

  const text = [
    `Work Item ${id}: ${fields["System.Title"]}`,
    `Tipo: ${fields["System.WorkItemType"]} | Estado: ${fields["System.State"]}`,
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
});

const transport = new StdioServerTransport();
await server.connect(transport);
