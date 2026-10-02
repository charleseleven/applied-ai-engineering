import axios from 'axios'

interface SprintHealthAlertDto {
  id: number
  severity: 'Warning' | 'Critical'
  message: string
}

const POLL_INTERVAL_MS = 60000

/**
 * Busca periodicamente os alertas do agente de monitoramento de Sprints (PBI #150)
 * ainda não reconhecidos, mostra cada um como Snackbar global (Task #155) e marca
 * como reconhecido pra não repetir no próximo ciclo.
 */
export function useSprintAlertPolling() {
  const config = useRuntimeConfig()
  const { pushAlert } = useAppAlerts()
  let intervalId: ReturnType<typeof setInterval> | null = null

  async function pollOnce() {
    try {
      const { data } = await axios.get<SprintHealthAlertDto[]>(
        `${config.public.apiBaseUrl}/api/SprintAlerts`,
        { params: { onlyUnacknowledged: true } }
      )

      for (const alert of data) {
        pushAlert(alert.message, alert.severity)
        axios.post(`${config.public.apiBaseUrl}/api/SprintAlerts/${alert.id}/acknowledge`).catch(() => {})
      }
    } catch {
      // Falha de polling não deve incomodar o usuário — só tenta de novo no próximo ciclo.
    }
  }

  function start() {
    pollOnce()
    intervalId = setInterval(pollOnce, POLL_INTERVAL_MS)
  }

  function stop() {
    if (intervalId) {
      clearInterval(intervalId)
      intervalId = null
    }
  }

  return { start, stop }
}
