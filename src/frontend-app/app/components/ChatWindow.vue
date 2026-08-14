<script setup lang="ts">
const { messages, isLoading, sendMessage, conversations, activeConversationId } = useAiChat()

const activeTitle = computed(() => {
  const active = conversations.value.find((c) => c.id === activeConversationId.value)
  return active?.title || 'Assistente de IA'
})

const draft = ref('')
const messageListRef = ref<{ $el: HTMLElement } | null>(null)

async function handleSend() {
  if (!draft.value.trim() || isLoading.value) return
  const prompt = draft.value
  draft.value = ''
  await sendMessage(prompt)
  await nextTick()
  scrollToBottom()
}

function handleKeydown(event: KeyboardEvent) {
  if (event.key === 'Enter' && !event.shiftKey) {
    event.preventDefault()
    handleSend()
  }
}

function scrollToBottom() {
  const el = messageListRef.value?.$el as HTMLElement | undefined
  if (el) el.scrollTop = el.scrollHeight
}
</script>

<template>
  <v-card class="d-flex flex-column" elevation="2" height="600">
    <v-card-title class="text-h6">{{ activeTitle }}</v-card-title>
    <v-divider />

    <v-card-text ref="messageListRef" class="flex-grow-1 overflow-y-auto">
      <div
        v-for="message in messages"
        :key="message.id"
        class="d-flex mb-3"
        :class="message.role === 'user' ? 'justify-end' : 'justify-start'"
      >
        <v-sheet
          :color="message.role === 'user' ? 'primary' : (message.isError ? 'error' : 'grey-lighten-3')"
          :class="[
            'pa-3 rounded-lg',
            message.role === 'user' || message.isError ? 'text-white' : 'text-black'
          ]"
          max-width="75%"
        >
          {{ message.content }}
        </v-sheet>
      </div>

      <div v-if="isLoading" class="d-flex justify-start mb-3">
        <v-sheet color="grey-lighten-3" class="pa-3 rounded-lg d-flex align-center" max-width="75%">
          <v-progress-circular indeterminate size="20" width="2" class="mr-2" />
          <span class="text-body-2">Pensando...</span>
        </v-sheet>
      </div>

      <v-alert v-if="messages.length === 0 && !isLoading" type="info" variant="tonal" density="compact">
        Envie uma pergunta técnica para começar a conversa.
      </v-alert>
    </v-card-text>

    <v-divider />

    <v-card-actions class="pa-3">
      <v-textarea
        v-model="draft"
        label="Digite sua dúvida técnica..."
        rows="1"
        auto-grow
        max-rows="4"
        density="comfortable"
        variant="outlined"
        hide-details
        class="flex-grow-1 mr-2"
        :disabled="isLoading"
        @keydown="handleKeydown"
      />
      <v-btn
        color="primary"
        icon="mdi-send"
        :disabled="!draft.trim() || isLoading"
        :loading="isLoading"
        @click="handleSend"
      />
    </v-card-actions>
  </v-card>
</template>
