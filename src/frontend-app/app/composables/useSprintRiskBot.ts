import axios from 'axios'

export interface SprintRiskAssessment {
  sprintId: number
  sprintTitle: string
  totalStoryPoints: number
  completedStoryPoints: number
  remainingStoryPoints: number
  elapsedDays: number
  remainingDays: number
  /** Nulo quando a Sprint já terminou com pontos pendentes (ritmo não é mais calculável). */
  requiredDailyPace: number | null
  projectedAdditionalPoints: number
  projectedTotalCompletion: number
  projectedShortfall: number
  isAtRisk: boolean
  riskLevel: 'OnTrack' | 'Warning' | 'Critical'
}

export interface SprintHistoricalVelocitySample {
  sprintId: number
  sprintTitle: string
  dailyVelocity: number
}

export interface SprintRiskReport {
  generatedAtUtc: string
  historicalDailyCapacity: number
  historicalSprintsUsed: number
  historicalVelocitySeries: SprintHistoricalVelocitySample[]
  assessments: SprintRiskAssessment[]
  aiNarrative: string
  aiRecommendations: string[]
  aiInsightUnavailable: boolean
}

export function useSprintRiskBot() {
  const config = useRuntimeConfig()
  const report = useState<SprintRiskReport | null>('sprint-risk-bot-report', () => null)
  const isLoading = useState<boolean>('sprint-risk-bot-loading', () => false)
  const errorMessage = useState<string | null>('sprint-risk-bot-error', () => null)

  async function loadReport() {
    isLoading.value = true
    errorMessage.value = null
    try {
      const { data } = await axios.get<SprintRiskReport>(
        `${config.public.apiBaseUrl}/api/sprint-risk-bot/report`
      )
      report.value = data
    } catch {
      errorMessage.value = 'Não foi possível carregar o relatório do bot de risco. Verifique se o backend está no ar.'
      report.value = null
    } finally {
      isLoading.value = false
    }
  }

  return { report, isLoading, errorMessage, loadReport }
}
