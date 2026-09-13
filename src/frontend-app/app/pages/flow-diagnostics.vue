<script setup lang="ts">
const { report, isLoading, errorMessage, loadDiagnostics } = useFlowDiagnostics()
const iterationPath = ref('')

function handleSubmit() {
  loadDiagnostics(iterationPath.value)
}
</script>

<template>
  <div>
    <h1 class="text-h3 mb-4">Diagnóstico Preditivo de Fluxo</h1>
    <p class="text-medium-emphasis mb-6">
      Painel executivo do Scrum Master: Cycle Time por card comparado à baseline histórica,
      alertas de gargalo por status/responsável e sugestões de mitigação geradas por IA.
    </p>

    <v-card class="pa-4 mb-6" elevation="2">
      <v-form @submit.prevent="handleSubmit">
        <v-row align="center">
          <v-col cols="12" sm="8">
            <v-text-field
              v-model="iterationPath"
              label="Caminho da iteração (sprint)"
              placeholder="Applied AI Engineering\Sprint 4"
              hint="Iteration Path exato configurado no Azure DevOps"
              persistent-hint
              density="comfortable"
            />
          </v-col>
          <v-col cols="12" sm="4">
            <v-btn
              type="submit"
              color="primary"
              prepend-icon="mdi-magnify"
              :loading="isLoading"
              block
            >
              Gerar diagnóstico
            </v-btn>
          </v-col>
        </v-row>
      </v-form>
    </v-card>

    <v-alert v-if="errorMessage" type="error" variant="tonal" class="mb-4">{{ errorMessage }}</v-alert>

    <FlowDiagnosticsPanel v-if="report" :report="report" />

    <v-alert v-else-if="!isLoading && !errorMessage" type="info" variant="tonal">
      Informe o caminho da iteração e clique em "Gerar diagnóstico" para analisar a sprint atual.
    </v-alert>
  </div>
</template>
