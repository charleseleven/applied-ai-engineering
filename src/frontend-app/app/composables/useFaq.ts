import axios from 'axios'

export interface FaqSource {
  taskId: number
  title: string
  similarity: number
}

export interface FaqResult {
  question: string
  answer: string
  success: boolean
  errorMessage?: string | null
  sources: FaqSource[]
}

const REQUEST_TIMEOUT_MS = 35000 // mesmo raciocínio do useAiChat: levemente acima do timeout do backend

export function useFaq() {
  const config = useRuntimeConfig()
  const isLoading = useState<boolean>('faq-loading', () => false)
  const lastResult = useState<FaqResult | null>('faq-last-result', () => null)

  async function ask(question: string) {
    const trimmed = question.trim()
    if (!trimmed || isLoading.value) return

    isLoading.value = true

    try {
      const { data } = await axios.post<FaqResult>(
        `${config.public.apiBaseUrl}/api/Faq/ask`,
        { question: trimmed },
        { timeout: REQUEST_TIMEOUT_MS }
      )
      lastResult.value = data
    } catch (error) {
      lastResult.value = {
        question: trimmed,
        answer: '',
        success: false,
        errorMessage: resolveErrorMessage(error),
        sources: []
      }
    } finally {
      isLoading.value = false
    }
  }

  return { isLoading, lastResult, ask }
}

function resolveErrorMessage(error: unknown): string {
  if (axios.isAxiosError(error)) {
    if (error.code === 'ECONNABORTED') {
      return 'Tempo limite excedido ao aguardar resposta. Tente novamente.'
    }
    if (!error.response) {
      return 'Não foi possível conectar ao servidor. Verifique sua conexão e tente novamente.'
    }
    const backendMessage = error.response.data?.error ?? error.response.data?.errorMessage
    return backendMessage || `Erro do servidor (status ${error.response.status}). Tente novamente.`
  }
  return 'Ocorreu um erro inesperado. Tente novamente.'
}
