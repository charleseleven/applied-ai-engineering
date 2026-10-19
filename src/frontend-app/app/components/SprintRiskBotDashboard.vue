<script setup lang="ts">
import type { SprintRiskAssessment } from '~/composables/useSprintRiskBot'

const { report, isLoading, errorMessage, loadReport } = useSprintRiskBot()

onMounted(() => { loadReport() })

const atRiskCount = computed(() => report.value?.assessments.filter((a) => a.isAtRisk).length ?? 0)
const activeSprintCount = computed(() => report.value?.assessments.length ?? 0)

const sparklineValues = computed(() => report.value?.historicalVelocitySeries.map((s) => s.dailyVelocity) ?? [])

function riskChipColor(level: SprintRiskAssessment['riskLevel']) {
  if (level === 'Critical') return 'error'
  if (level === 'Warning') return 'warning'
  return 'success'
}

function riskChipLabel(level: SprintRiskAssessment['riskLevel']) {
  if (level === 'Critical') return 'Crítico'
  if (level === 'Warning') return 'Atenção'
  return 'No ritmo'
}

function progressColor(assessment: SprintRiskAssessment) {
  return riskChipColor(assessment.riskLevel)
}

function completionPercent(assessment: SprintRiskAssessment) {
  return assessment.totalStoryPoints > 0
    ? Math.round((assessment.completedStoryPoints / assessment.totalStoryPoints) * 100)
    : 0
}

function formatPace(pace: number | null) {
  return pace === null ? 'Sprint encerrada' : `${pace.toFixed(1)} pts/dia`
}
</script>

<template>
  <v-card class="pa-6" elevation="2">
    <div class="d-flex align-center mb-4 flex-wrap ga-2">
      <v-card-title class="px-0 text-h6 flex-grow-1">Bot Preditivo de Risco de Sprint</v-card-title>
      <v-btn variant="tonal" prepend-icon="mdi-refresh" :loading="isLoading" @click="loadReport">
        Atualizar
      </v-btn>
    </div>

    <template v-if="isLoading && !report">
      <v-skeleton-loader type="list-item-two-line" class="mb-4" />
      <v-skeleton-loader type="list-item-two-line" class="mb-4" />
    </template>

    <v-alert v-else-if="errorMessage" type="error" variant="tonal" class="mb-4">{{ errorMessage }}</v-alert>

    <template v-else-if="report">
      <!-- KPIs -->
      <v-row class="mb-2">
        <v-col cols="12" sm="4">
          <v-card variant="tonal" color="primary">
            <v-card-text>
              <div class="text-caption">Capacidade histórica do time</div>
              <div class="text-h5">{{ report.historicalDailyCapacity.toFixed(1) }} pts/dia</div>
              <div class="text-caption text-medium-emphasis">
                {{ report.historicalSprintsUsed > 0
                  ? `baseado em ${report.historicalSprintsUsed} sprint(s) concluída(s)`
                  : 'valor padrão (sem histórico suficiente)' }}
              </div>
            </v-card-text>
          </v-card>
        </v-col>
        <v-col cols="12" sm="4">
          <v-card variant="tonal" color="error">
            <v-card-text>
              <div class="text-caption">Sprints em risco</div>
              <div class="text-h5">{{ atRiskCount }} / {{ activeSprintCount }}</div>
            </v-card-text>
          </v-card>
        </v-col>
        <v-col cols="12" sm="4">
          <v-card variant="tonal" color="secondary">
            <v-card-text>
              <div class="text-caption">Sprints ativas analisadas</div>
              <div class="text-h5">{{ activeSprintCount }}</div>
            </v-card-text>
          </v-card>
        </v-col>
      </v-row>

      <!-- Gráfico: tendência de capacidade histórica -->
      <v-card variant="outlined" class="mb-4">
        <v-card-text>
          <div class="text-caption text-medium-emphasis mb-2">
            <v-icon icon="mdi-chart-line" size="small" class="mr-1" />
            Tendência de velocidade (Story Points/dia) das sprints concluídas usadas na projeção
          </div>
          <v-sparkline
            v-if="sparklineValues.length > 0"
            :model-value="sparklineValues"
            color="primary"
            line-width="2"
            padding="8"
            smooth
            :height="60"
            auto-draw
          />
          <div v-else class="text-caption text-medium-emphasis">
            Sem Sprints concluídas no histórico — usando capacidade padrão.
          </div>
          <div v-if="report.historicalVelocitySeries.length > 0" class="d-flex flex-wrap ga-2 mt-2">
            <v-chip
              v-for="sample in report.historicalVelocitySeries"
              :key="sample.sprintId"
              size="small"
              variant="tonal"
            >
              {{ sample.sprintTitle }}: {{ sample.dailyVelocity.toFixed(1) }} pts/dia
            </v-chip>
          </div>
        </v-card-text>
      </v-card>

      <!-- Diagnóstico de IA -->
      <v-card variant="outlined" class="mb-4">
        <v-card-title class="text-subtitle-1">
          <v-icon icon="mdi-robot-outline" class="mr-2" />
          Diagnóstico do bot
        </v-card-title>
        <v-card-text>
          <v-alert v-if="report.aiInsightUnavailable" type="warning" variant="tonal">
            Não foi possível gerar o diagnóstico de IA neste momento. As avaliações estatísticas abaixo continuam válidas.
          </v-alert>
          <template v-else>
            <p class="mb-3">{{ report.aiNarrative }}</p>
            <v-list v-if="report.aiRecommendations.length" density="compact">
              <v-list-item v-for="(recommendation, index) in report.aiRecommendations" :key="index">
                <template #prepend>
                  <v-icon icon="mdi-lightbulb-on-outline" color="amber-darken-2" />
                </template>
                <v-list-item-title>{{ recommendation }}</v-list-item-title>
              </v-list-item>
            </v-list>
          </template>
        </v-card-text>
      </v-card>

      <!-- Sprints ativas -->
      <v-alert v-if="activeSprintCount === 0" type="info" variant="tonal">Nenhuma Sprint ativa no momento.</v-alert>
      <v-row v-else>
        <v-col v-for="assessment in report.assessments" :key="assessment.sprintId" cols="12" sm="6" md="4">
          <v-card variant="outlined" height="100%">
            <v-card-text>
              <div class="d-flex justify-space-between align-start mb-2 ga-2">
                <span class="text-subtitle-1 font-weight-medium">{{ assessment.sprintTitle }}</span>
                <v-chip :color="riskChipColor(assessment.riskLevel)" size="small" variant="flat">
                  {{ riskChipLabel(assessment.riskLevel) }}
                </v-chip>
              </div>

              <div class="text-caption text-medium-emphasis mb-1">
                {{ assessment.completedStoryPoints }} / {{ assessment.totalStoryPoints }} Story Points concluídos
              </div>
              <v-progress-linear
                :model-value="completionPercent(assessment)"
                :color="progressColor(assessment)"
                height="18"
                rounded
                class="mb-3"
              >
                <template #default><strong class="text-caption">{{ completionPercent(assessment) }}%</strong></template>
              </v-progress-linear>

              <v-table density="compact">
                <tbody>
                  <tr>
                    <td class="text-medium-emphasis">Dias restantes</td>
                    <td class="text-right">{{ assessment.remainingDays }}</td>
                  </tr>
                  <tr>
                    <td class="text-medium-emphasis">Ritmo necessário</td>
                    <td class="text-right">{{ formatPace(assessment.requiredDailyPace) }}</td>
                  </tr>
                  <tr>
                    <td class="text-medium-emphasis">Projeção de entrega</td>
                    <td class="text-right">{{ assessment.projectedTotalCompletion.toFixed(1) }} pts</td>
                  </tr>
                  <tr v-if="assessment.projectedShortfall > 0">
                    <td class="text-medium-emphasis">Déficit projetado</td>
                    <td class="text-right text-error font-weight-medium">{{ assessment.projectedShortfall.toFixed(1) }} pts</td>
                  </tr>
                </tbody>
              </v-table>
            </v-card-text>
          </v-card>
        </v-col>
      </v-row>
    </template>
  </v-card>
</template>
