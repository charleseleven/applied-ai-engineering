<script setup lang="ts">
const { isLoading, lastResult, ask } = useFaq()

const draft = ref('')

function handleAsk() {
  if (!draft.value.trim() || isLoading.value) return
  ask(draft.value)
}

function handleKeydown(event: KeyboardEvent) {
  if (event.key === 'Enter' && !event.shiftKey) {
    event.preventDefault()
    handleAsk()
  }
}

function similarityPercent(value: number) {
  return `${Math.round(value * 100)}%`
}
</script>

<template>
  <v-card class="pa-6" elevation="2">
    <v-card-title class="px-0 text-h6">FAQ Inteligente (RAG)</v-card-title>
    <v-card-text class="px-0">
      <p class="text-body-2 mb-4">
        Pergunte algo sobre as Tasks cadastradas. A resposta é gerada com base apenas nos 3
        registros mais similares encontrados no banco — sem informações externas.
      </p>

      <v-textarea
        v-model="draft"
        label="Digite sua pergunta..."
        rows="1"
        auto-grow
        max-rows="4"
        density="comfortable"
        variant="outlined"
        hide-details
        :disabled="isLoading"
        @keydown="handleKeydown"
      />

      <v-btn
        class="mt-3"
        color="primary"
        prepend-icon="mdi-magnify"
        :disabled="!draft.trim() || isLoading"
        :loading="isLoading"
        @click="handleAsk"
      >
        Perguntar
      </v-btn>

      <div v-if="lastResult" class="mt-6">
        <v-alert
          :type="lastResult.success ? 'info' : 'error'"
          variant="tonal"
          class="mb-4"
        >
          <div class="text-caption mb-1">Pergunta: {{ lastResult.question }}</div>
          <div class="text-body-1" style="white-space: pre-wrap">
            {{ lastResult.success ? lastResult.answer : lastResult.errorMessage }}
          </div>
        </v-alert>

        <template v-if="lastResult.success && lastResult.sources.length > 0">
          <div class="text-subtitle-2 mb-2">Fontes usadas (Tasks mais similares):</div>
          <v-list density="compact" class="pa-0">
            <v-list-item
              v-for="source in lastResult.sources"
              :key="source.taskId"
              :title="source.title"
              :subtitle="`Task #${source.taskId} · similaridade ${similarityPercent(source.similarity)}`"
            />
          </v-list>
        </template>
      </div>
    </v-card-text>
  </v-card>
</template>
