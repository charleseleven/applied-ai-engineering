<script setup lang="ts">
const { start, stop } = useSprintAlertPolling()

onMounted(() => start())
onUnmounted(() => stop())
</script>

<template>
  <NuxtLayout>
    <NuxtRouteAnnouncer />
    <NuxtPage />
  </NuxtLayout>
  <GlobalSnackbar />
</template>
