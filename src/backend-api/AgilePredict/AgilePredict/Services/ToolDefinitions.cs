namespace AgilePredict.Services
{
    /// <summary>
    /// Mapeamento declarativo das ações do sistema expostas à LLM via function calling.
    /// Cada tool aqui precisa ter um handler correspondente em ToolOrchestrationService.
    /// </summary>
    public static class ToolDefinitions
    {
        public const string UpdateTaskStatusToolName = "update_task_status";

        public static readonly IReadOnlyList<object> AllTools = new List<object>
        {
            new
            {
                type = "function",
                function = new
                {
                    name = UpdateTaskStatusToolName,
                    description =
                        "Atualiza o status de uma Task existente no Hub Ágil. Use isso quando o usuário pedir " +
                        "explicitamente para mudar o status/estado de uma tarefa (ex: 'marcar a tarefa 15 como " +
                        "concluída', 'mover a task 3 para em andamento').",
                    parameters = new
                    {
                        type = "object",
                        properties = new
                        {
                            taskId = new
                            {
                                type = "integer",
                                description = "ID numérico da Task a ser atualizada"
                            },
                            status = new
                            {
                                type = "string",
                                @enum = new[] { "To Do", "In Progress", "Done" },
                                description = "Novo status da Task"
                            }
                        },
                        required = new[] { "taskId", "status" }
                    }
                }
            }
        };
    }
}
