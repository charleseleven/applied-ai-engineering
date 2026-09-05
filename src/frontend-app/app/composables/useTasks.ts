import axios from 'axios'

export interface ProjectTask {
  id: number
  title: string
  description: string
  status: string
  storyPoints: number
  priority: string
  assignedTo: string | null
  sprintId: number
  sprint: { id: number; title: string } | null
}

export function useTasks() {
  const config = useRuntimeConfig()
  const tasks = useState<ProjectTask[]>('tasks-list', () => [])
  const isLoading = useState<boolean>('tasks-loading', () => false)
  const errorMessage = useState<string | null>('tasks-error', () => null)

  async function loadTasks() {
    isLoading.value = true
    errorMessage.value = null
    try {
      const { data } = await axios.get<ProjectTask[]>(`${config.public.apiBaseUrl}/api/Tasks`)
      tasks.value = data
    } catch {
      errorMessage.value = 'Não foi possível carregar as Tasks. Verifique se o backend está no ar.'
    } finally {
      isLoading.value = false
    }
  }

  return { tasks, isLoading, errorMessage, loadTasks }
}
