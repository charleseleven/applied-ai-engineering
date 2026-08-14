<script setup lang="ts">
const { conversations, activeConversationId, isLoading, selectConversation, startNewConversation, deleteConversation } = useAiChat()

async function handleDelete(event: MouseEvent, id: number) {
  event.stopPropagation()
  if (isLoading.value) return
  await deleteConversation(id)
}

function handleSelect(id: number) {
  if (isLoading.value || id === activeConversationId.value) return
  selectConversation(id)
}
</script>

<template>
  <v-card class="d-flex flex-column" elevation="2" height="600">
    <v-card-actions class="pa-3">
      <v-btn
        color="primary"
        variant="tonal"
        prepend-icon="mdi-plus"
        block
        :disabled="isLoading"
        @click="startNewConversation"
      >
        Nova conversa
      </v-btn>
    </v-card-actions>
    <v-divider />

    <v-list class="flex-grow-1 overflow-y-auto" density="compact" nav>
      <v-list-item
        v-for="conversation in conversations"
        :key="conversation.id"
        :active="conversation.id === activeConversationId"
        :title="conversation.title"
        :disabled="isLoading && conversation.id !== activeConversationId"
        color="primary"
        @click="handleSelect(conversation.id)"
      >
        <template #append>
          <v-btn
            icon="mdi-delete-outline"
            variant="text"
            size="small"
            density="comfortable"
            :disabled="isLoading"
            @click="(event: MouseEvent) => handleDelete(event, conversation.id)"
          />
        </template>
      </v-list-item>

      <v-list-item v-if="conversations.length === 0">
        <v-list-item-subtitle>Nenhuma conversa ainda</v-list-item-subtitle>
      </v-list-item>
    </v-list>
  </v-card>
</template>
