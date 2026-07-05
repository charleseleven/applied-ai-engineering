<template>
  <v-app>
    <v-app-bar color="primary" density="comfortable">
      <v-app-bar-title>Applied AI Engineering</v-app-bar-title>
      <v-spacer />
      <v-btn variant="text" to="/">Home</v-btn>
      <v-btn variant="text" to="/ia-playground">IA Playground</v-btn>
    </v-app-bar>

    <v-main>
      <v-container class="py-8">
        <slot />
      </v-container>
    </v-main>
  </v-app>
</template>
