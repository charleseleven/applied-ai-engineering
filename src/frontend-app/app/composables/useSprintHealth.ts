import axios from 'axios'

export interface SprintHealthSummary {
  sprintId: number
  sprintTitle: string
  completionPercent: number
  timeElapsedPercent: number
  isAtRisk: boolean
}

export function useSprintHealth() {
  const config = useRuntimeConfig()
  const summaries = useState<SprintHealthSummary[]>('sprint-health-summaries', () => [])
  const isLoading = useState<boolean>('sprint-health-loading', () => false)
  const errorMessage = useState<string | null>('sprint-health-error', () => null)

  async function loadHealthSummary() {
    isLoading.value = true
    errorMessage.value = null
    try {
      const { data } = await axios.get<SprintHealthSummary[]>(
        `${config.public.apiBaseUrl}/api/SprintAlerts/health-summary`
      )
      summaries.value = data
    } catch {
      errorMessage.value = 'Não foi possível carregar a saúde das Sprints. Verifique se o backend está no ar.'
    } finally {
      isLoading.value = false
    }
  }

  return { summaries, isLoading, errorMessage, loadHealthSummary }
}
