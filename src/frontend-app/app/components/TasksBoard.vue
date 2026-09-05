<script setup lang="ts">
const { tasks, isLoading, errorMessage, loadTasks } = useTasks()

onMounted(() => {
  loadTasks()
})

const groupedBySprint = computed(() => {
  const groups = new Map<string, typeof tasks.value>()
  for (const task of tasks.value) {
    const key = task.sprint?.title ?? 'Sem Sprint'
    if (!groups.has(key)) groups.set(key, [])
    groups.get(key)!.push(task)
  }
  return Array.from(groups.entries())
})

function statusColor(status: string) {
  if (status === 'Done') return 'success'
  if (status === 'In Progress') return 'primary'
  return 'grey-lighten-1'
}
</script>

<template>
  <v-card class="pa-6" elevation="2">
    <div class="d-flex align-center mb-4">
      <v-card-title class="px-0 text-h6 flex-grow-1">Tasks por Sprint</v-card-title>
      <v-btn variant="tonal" prepend-icon="mdi-refresh" :loading="isLoading" @click="loadTasks">
        Atualizar
      </v-btn>
    </div>

    <v-alert v-if="errorMessage" type="error" variant="tonal" class="mb-4">{{ errorMessage }}</v-alert>

    <div v-for="[sprintTitle, sprintTasks] in groupedBySprint" :key="sprintTitle" class="mb-6">
      <div class="text-subtitle-1 font-weight-bold mb-2">{{ sprintTitle }}</div>
      <v-table density="comfortable">
        <thead>
          <tr>
            <th>ID</th>
            <th>Título</th>
            <th>Prioridade</th>
            <th>Status</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="task in sprintTasks" :key="task.id">
            <td>#{{ task.id }}</td>
            <td>{{ task.title }}</td>
            <td>{{ task.priority }}</td>
            <td>
              <v-chip :color="statusColor(task.status)" size="small" variant="flat">{{ task.status }}</v-chip>
            </td>
          </tr>
        </tbody>
      </v-table>
    </div>

    <v-alert v-if="!isLoading && tasks.length === 0 && !errorMessage" type="info" variant="tonal">
      Nenhuma Task encontrada.
    </v-alert>
  </v-card>
</template>
