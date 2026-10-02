export interface AppAlert {
  id: string
  message: string
  severity: 'Warning' | 'Critical' | 'info'
}

export function useAppAlerts() {
  const queue = useState<AppAlert[]>('app-alerts-queue', () => [])

  function pushAlert(message: string, severity: AppAlert['severity'] = 'Warning') {
    queue.value.push({
      id: `${Date.now()}-${Math.random().toString(36).slice(2)}`,
      message,
      severity
    })
  }

  function dismissAlert(id: string) {
    queue.value = queue.value.filter((alert) => alert.id !== id)
  }

  return { queue, pushAlert, dismissAlert }
}
