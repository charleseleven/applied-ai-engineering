<script setup lang="ts">
import type { FlowDiagnosticReport } from '~/composables/useFlowDiagnostics'

const props = defineProps<{ report: FlowDiagnosticReport }>()

const cardsAboveBaseline = computed(() =>
  props.report.metrics.cards.filter((c) => c.isCycleTimeAboveBaseline).length
)

const activeBottlenecks = computed(() => props.report.bottleneckAlerts.length)

const latestThroughput = computed(() => {
  const points = props.report.metrics.weeklyThroughput
  return points.length > 0 ? points[points.length - 1]!.completedCount : 0
})

function severityColor(severity: string) {
  return severity === 'Critical' ? 'error' : 'warning'
}

function statusColor(currentStatus: string, isBottleneck: boolean, isAboveBaseline: boolean) {
  if (isBottleneck) return 'error'
  if (isAboveBaseline) return 'warning'
  return 'grey-lighten-1'
}

function formatHours(hours: number) {
  if (hours < 24) return `${hours.toFixed(1)}h`
  return `${(hours / 24).toFixed(1)}d`
}

function formatDate(iso: string) {
  return new Date(iso).toLocaleDateString('pt-BR', { day: '2-digit', month: '2-digit' })
}
</script>

<template>
  <div>
    <v-row class="mb-2">
      <v-col cols="12" sm="4">
        <v-card variant="tonal" color="error">
          <v-card-text>
            <div class="text-caption">Gargalos ativos</div>
            <div class="text-h4">{{ activeBottlenecks }}</div>
          </v-card-text>
        </v-card>
      </v-col>
      <v-col cols="12" sm="4">
        <v-card variant="tonal" color="warning">
          <v-card-text>
            <div class="text-caption">Cards acima da baseline de Cycle Time</div>
            <div class="text-h4">{{ cardsAboveBaseline }}</div>
          </v-card-text>
        </v-card>
      </v-col>
      <v-col cols="12" sm="4">
        <v-card variant="tonal" color="primary">
          <v-card-text>
            <div class="text-caption">Vazão da última semana concluída</div>
            <div class="text-h4">{{ latestThroughput }}</div>
          </v-card-text>
        </v-card>
      </v-col>
    </v-row>

    <v-card class="mb-4" elevation="2">
      <v-card-title class="text-h6">
        <v-icon icon="mdi-robot-outline" class="mr-2" />
        Diagnóstico gerado por IA
      </v-card-title>
      <v-card-text>
        <v-alert v-if="report.aiInsightUnavailable" type="warning" variant="tonal">
          Não foi possível gerar o diagnóstico de IA neste momento. As métricas estatísticas abaixo continuam válidas.
        </v-alert>
        <template v-else>
          <p class="mb-3">{{ report.aiDiagnosticSummary }}</p>
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

    <v-card class="mb-4" elevation="2">
      <v-card-title class="text-h6">
        <v-icon icon="mdi-alert-octagon-outline" class="mr-2" />
        Alertas de gargalo por status e responsável
      </v-card-title>
      <v-card-text>
        <v-alert v-if="report.bottleneckAlerts.length === 0" type="success" variant="tonal">
          Nenhum card ultrapassou o limite estatístico de permanência em status crítico.
        </v-alert>
        <v-table v-else density="comfortable">
          <thead>
            <tr>
              <th>Card</th>
              <th>Responsável</th>
              <th>Status</th>
              <th>Tempo no status</th>
              <th>Limite</th>
              <th>Severidade</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="alert in report.bottleneckAlerts" :key="alert.externalId">
              <td>#{{ alert.externalId }} {{ alert.title }}</td>
              <td>{{ alert.assignedTo ?? 'Sem responsável' }}</td>
              <td>{{ alert.status }}</td>
              <td>{{ formatHours(alert.hoursInStatus) }}</td>
              <td>{{ formatHours(alert.thresholdHours) }}</td>
              <td>
                <v-chip :color="severityColor(alert.severity)" size="small" variant="flat">
                  {{ alert.severity === 'Critical' ? 'Crítico' : 'Atenção' }}
                </v-chip>
              </td>
            </tr>
          </tbody>
        </v-table>
      </v-card-text>
    </v-card>

    <v-card class="mb-4" elevation="2">
      <v-card-title class="text-h6">
        <v-icon icon="mdi-chart-line" class="mr-2" />
        Cards ativos da sprint
      </v-card-title>
      <v-card-text>
        <v-table density="comfortable">
          <thead>
            <tr>
              <th>Card</th>
              <th>Responsável</th>
              <th>Status atual</th>
              <th>Cycle Time</th>
              <th>Lead Time</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="card in report.metrics.cards" :key="card.externalId">
              <td>#{{ card.externalId }} {{ card.title }}</td>
              <td>{{ card.assignedTo ?? 'Sem responsável' }}</td>
              <td>
                <v-chip
                  :color="statusColor(card.currentStatus, card.isCurrentStatusBottleneck, card.isCycleTimeAboveBaseline)"
                  size="small"
                  variant="flat"
                >
                  {{ card.currentStatus }}
                </v-chip>
              </td>
              <td>
                {{ formatHours(card.cycleTimeHours) }}
                <v-icon v-if="card.isCycleTimeAboveBaseline" icon="mdi-trending-up" color="warning" size="small" title="Acima da baseline histórica" />
              </td>
              <td>{{ formatHours(card.leadTimeHours) }}</td>
            </tr>
            <tr v-if="report.metrics.cards.length === 0">
              <td colspan="5" class="text-center text-medium-emphasis">Nenhum card ativo encontrado para esta iteração.</td>
            </tr>
          </tbody>
        </v-table>
      </v-card-text>
    </v-card>

    <v-card elevation="2">
      <v-card-title class="text-h6">
        <v-icon icon="mdi-calendar-week" class="mr-2" />
        Vazão semanal (baseline de {{ report.metrics.baselineWindowDays }} dias)
      </v-card-title>
      <v-card-text>
        <v-alert v-if="report.metrics.weeklyThroughput.length === 0" type="info" variant="tonal">
          Sem itens concluídos na janela de baseline para calcular vazão.
        </v-alert>
        <div v-else class="d-flex flex-wrap ga-4">
          <div v-for="point in report.metrics.weeklyThroughput" :key="point.weekStartUtc" class="text-center">
            <div class="text-caption text-medium-emphasis">{{ formatDate(point.weekStartUtc) }}</div>
            <div class="text-h6">{{ point.completedCount }}</div>
          </div>
        </div>
      </v-card-text>
    </v-card>
  </div>
</template>
