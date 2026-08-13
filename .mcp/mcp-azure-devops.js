import { Server } from "@modelcontextprotocol/sdk/server/index.js";
import { StdioServerTransport } from "@modelcontextprotocol/sdk/server/stdio.js";

const server = new Server({
  name: "azure-devops-mcp",
  version: "1.0.0",
}, {
  capabilities: { tools: {} }
});

const PAT = process.env.AZURE_PAT;
const ORG = "eleven11C";
const PROJECT = "Applied AI Engineering";

server.setRequestHandler("tools/call", async (request) => {
  if (request.params.name === "get_work_item") {
    const { id } = request.params.arguments;
    const url = `https://dev.azure.com/${ORG}/${PROJECT}/_apis/wit/workitems/${id}?api-version=7.0`;

    const response = await fetch(url, {
      headers: { 'Authorization': 'Basic ' + Buffer.from(':' + PAT).toString('base64') }
    });

    const data = await response.json();
    return {
      content: [{
        type: "text",
        text: `Work Item ${id}: ${data.fields['System.Title']}\nDescrição: ${data.fields['System.Description']}`
      }]
    };
  }
});

const transport = new StdioServerTransport();
await server.connect(transport);
