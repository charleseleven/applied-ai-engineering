<script setup lang="ts">
const { loadConversations } = useAiChat()

onMounted(() => {
  loadConversations()
})
</script>

<template>
  <div>
    <h1 class="text-h3 mb-4">Chat com IA</h1>
    <v-row>
      <v-col cols="12" sm="4" md="3">
        <ChatHistorySidebar />
      </v-col>
      <v-col cols="12" sm="8" md="9">
        <ChatWindow />
      </v-col>
    </v-row>
  </div>
</template>
