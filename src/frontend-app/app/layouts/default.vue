<script setup lang="ts">
const { mobile } = useDisplay()
const drawerOpen = ref(false)

const navLinks = [
  { to: '/', label: 'Home' },
  { to: '/ia-playground', label: 'IA Playground' },
  { to: '/chat', label: 'Chat IA' },
  { to: '/faq', label: 'FAQ' },
  { to: '/tasks', label: 'Tasks' },
  { to: '/flow-diagnostics', label: 'Diagnóstico de Fluxo' },
  { to: '/predictive-panel', label: 'Painel Preditivo' }
]
</script>

<template>
  <v-app>
    <v-app-bar color="primary" density="comfortable">
      <v-app-bar-nav-icon v-if="mobile" @click="drawerOpen = !drawerOpen" />
      <v-app-bar-title>Applied AI Engineering</v-app-bar-title>
      <v-spacer />
      <template v-if="!mobile">
        <v-btn v-for="link in navLinks" :key="link.to" variant="text" :to="link.to">
          {{ link.label }}
        </v-btn>
      </template>
    </v-app-bar>

    <v-navigation-drawer v-if="mobile" v-model="drawerOpen" temporary>
      <v-list nav>
        <v-list-item
          v-for="link in navLinks"
          :key="link.to"
          :to="link.to"
          :title="link.label"
          @click="drawerOpen = false"
        />
      </v-list>
    </v-navigation-drawer>

    <v-main>
      <v-container class="py-8">
        <slot />
      </v-container>
    </v-main>
  </v-app>
</template>
