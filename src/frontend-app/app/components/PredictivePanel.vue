<script setup lang="ts">
const { summaries, isLoading, errorMessage, loadHealthSummary } = useSprintHealth()

onMounted(() => {
  loadHealthSummary()
})

function progressColor(summary: { completionPercent: number; timeElapsedPercent: number; isAtRisk: boolean }) {
  if (summary.isAtRisk) return 'error'
  if (summary.timeElapsedPercent - summary.completionPercent >= 10) return 'warning'
  return 'success'
}
</script>

<template>
  <v-card class="pa-6" elevation="2">
    <div class="d-flex align-center mb-4">
      <v-card-title class="px-0 text-h6 flex-grow-1">Painel Preditivo de Sprints</v-card-title>
      <v-btn variant="tonal" prepend-icon="mdi-refresh" :loading="isLoading" @click="loadHealthSummary">
        Atualizar
      </v-btn>
    </div>

    <!-- Estado de loading -->
    <template v-if="isLoading && summaries.length === 0">
      <v-skeleton-loader type="list-item-two-line" class="mb-4" />
      <v-skeleton-loader type="list-item-two-line" class="mb-4" />
    </template>

    <!-- Estado de erro -->
    <v-alert v-else-if="errorMessage" type="error" variant="tonal" class="mb-4">
      {{ errorMessage }}
    </v-alert>

    <!-- Estado vazio -->
    <v-alert v-else-if="summaries.length === 0" type="info" variant="tonal">
      Nenhuma Sprint ativa no momento.
    </v-alert>

    <!-- Dados -->
    <div v-else>
      <div v-for="summary in summaries" :key="summary.sprintId" class="mb-6">
        <div class="d-flex justify-space-between align-center mb-1">
          <span class="text-subtitle-1 font-weight-medium">{{ summary.sprintTitle }}</span>
          <v-chip v-if="summary.isAtRisk" color="error" size="small" variant="flat">Em risco</v-chip>
          <v-chip v-else color="success" size="small" variant="flat">No ritmo</v-chip>
        </div>

        <div class="text-caption text-medium-emphasis mb-1">
          {{ summary.completionPercent }}% dos Story Points concluídos ·
          {{ summary.timeElapsedPercent }}% do tempo decorrido
        </div>

        <v-progress-linear
          :model-value="summary.completionPercent"
          :color="progressColor(summary)"
          height="18"
          rounded
        >
          <template #default>
            <strong class="text-caption">{{ summary.completionPercent }}%</strong>
          </template>
        </v-progress-linear>
      </div>
    </div>
  </v-card>
</template>
