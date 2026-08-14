import axios from 'axios'

export interface ChatMessage {
  id: string
  role: 'user' | 'assistant'
  content: string
  isError?: boolean
}

export interface ChatConversationSummary {
  id: number
  title: string
  createdAt: string
  updatedAt: string | null
}

interface ServerChatMessage {
  id: number
  role: string
  content: string
  isError: boolean
}

interface ChatExchangeResult {
  conversationId: number
  conversationTitle: string
  userMessage: ServerChatMessage
  assistantMessage: ServerChatMessage
}

const REQUEST_TIMEOUT_MS = 35000 // levemente acima do LlmConfiguration.TimeoutSeconds (30s) do backend,
                                  // para o backend responder com o erro estruturado antes do Axios abortar

function toChatMessage(m: ServerChatMessage): ChatMessage {
  return { id: String(m.id), role: m.role as ChatMessage['role'], content: m.content, isError: m.isError }
}

export function useAiChat() {
  const config = useRuntimeConfig()
  const apiBase = config.public.apiBaseUrl

  const messages = useState<ChatMessage[]>('ai-chat-messages', () => [])
  const isLoading = useState<boolean>('ai-chat-loading', () => false)
  const activeConversationId = useState<number | null>('ai-chat-active-id', () => null)
  const conversations = useState<ChatConversationSummary[]>('ai-chat-conversations', () => [])

  function pushLocalMessage(role: ChatMessage['role'], content: string, isError = false) {
    const message: ChatMessage = {
      id: `local-${Date.now()}-${Math.random().toString(36).slice(2)}`,
      role,
      content,
      isError
    }
    messages.value.push(message)
    return message
  }

  async function loadConversations() {
    const { data } = await axios.get<ChatConversationSummary[]>(`${apiBase}/api/chatconversations`)
    conversations.value = data
  }

  async function selectConversation(id: number) {
    const { data } = await axios.get<{ id: number; messages: ServerChatMessage[] }>(
      `${apiBase}/api/chatconversations/${id}`
    )
    activeConversationId.value = data.id
    messages.value = data.messages.map(toChatMessage)
  }

  function startNewConversation() {
    activeConversationId.value = null
    messages.value = []
  }

  async function deleteConversation(id: number) {
    await axios.delete(`${apiBase}/api/chatconversations/${id}`)
    conversations.value = conversations.value.filter((c) => c.id !== id)
    if (activeConversationId.value === id) {
      startNewConversation()
    }
  }

  async function sendMessage(prompt: string) {
    const trimmed = prompt.trim()
    if (!trimmed || isLoading.value) return

    const optimisticUserMessage = pushLocalMessage('user', trimmed)
    isLoading.value = true

    // Guarda a conversa alvo no momento do envio: se o usuário trocar de conversa
    // (ou iniciar uma nova) antes da resposta chegar, o resultado não deve ser
    // aplicado à conversa errada — só a lista de conversas é atualizada nesse caso.
    let conversationId = activeConversationId.value

    try {
      if (conversationId === null) {
        const { data } = await axios.post(`${apiBase}/api/chatconversations`, {})
        conversationId = data.id
        activeConversationId.value = conversationId
      }

      const { data } = await axios.post<ChatExchangeResult>(
        `${apiBase}/api/chatconversations/${conversationId}/messages`,
        { prompt: trimmed },
        { timeout: REQUEST_TIMEOUT_MS }
      )

      if (activeConversationId.value === conversationId) {
        // Substitui a mensagem otimista pela versão persistida (id real do servidor)
        const optimisticIndex = messages.value.findIndex((m) => m.id === optimisticUserMessage.id)
        if (optimisticIndex !== -1) {
          messages.value.splice(optimisticIndex, 1, toChatMessage(data.userMessage))
        }
        messages.value.push(toChatMessage(data.assistantMessage))
      }

      await syncConversationSummary(data.conversationId, data.conversationTitle)
    } catch (error) {
      if (activeConversationId.value === conversationId) {
        pushLocalMessage('assistant', resolveErrorMessage(error), true)
      }
    } finally {
      isLoading.value = false
    }
  }

  async function syncConversationSummary(id: number, title: string) {
    const nowIso = new Date().toISOString()
    const existing = conversations.value.find((c) => c.id === id)
    if (existing) {
      existing.title = title
      existing.updatedAt = nowIso
    } else {
      conversations.value.unshift({ id, title, createdAt: nowIso, updatedAt: nowIso })
    }
    conversations.value.sort((a, b) => (b.updatedAt ?? b.createdAt).localeCompare(a.updatedAt ?? a.createdAt))
  }

  return {
    messages,
    isLoading,
    activeConversationId,
    conversations,
    loadConversations,
    selectConversation,
    startNewConversation,
    deleteConversation,
    sendMessage
  }
}

function resolveErrorMessage(error: unknown): string {
  if (axios.isAxiosError(error)) {
    if (error.code === 'ECONNABORTED') {
      return 'Tempo limite excedido ao aguardar resposta da IA. Tente novamente.'
    }
    if (!error.response) {
      return 'Não foi possível conectar ao servidor. Verifique sua conexão e tente novamente.'
    }
    const backendMessage = error.response.data?.errorMessage
    return backendMessage || `Erro do servidor (status ${error.response.status}). Tente novamente.`
  }
  return 'Ocorreu um erro inesperado. Tente novamente.'
}
