<script setup lang="ts">
const { queue, dismissAlert } = useAppAlerts()

const show = ref(false)
const current = ref<{ id: string; message: string; severity: string } | null>(null)

function severityColor(severity: string) {
  if (severity === 'Critical') return 'error'
  if (severity === 'Warning') return 'warning'
  return 'info'
}

function showNext() {
  if (show.value || queue.value.length === 0) return
  current.value = queue.value[0] ?? null
  show.value = true
}

watch(queue, showNext, { immediate: true, deep: true })

function handleClose() {
  show.value = false
  if (current.value) {
    dismissAlert(current.value.id)
  }
  current.value = null
  nextTick(showNext)
}
</script>

<template>
  <v-snackbar
    v-model="show"
    :color="current ? severityColor(current.severity) : 'info'"
    timeout="6000"
    location="bottom right"
    @update:model-value="(value: boolean) => !value && handleClose()"
  >
    {{ current?.message }}
    <template #actions>
      <v-btn variant="text" @click="handleClose">Fechar</v-btn>
    </template>
  </v-snackbar>
</template>
