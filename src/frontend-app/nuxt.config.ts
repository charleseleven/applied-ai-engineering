// https://nuxt.com/docs/api/configuration/nuxt-config
export default defineNuxtConfig({
  compatibilityDate: '2025-07-15',
  devtools: { enabled: true },
  modules: ['vuetify-nuxt-module', '@nuxtjs/tailwindcss'],
  css: ['@mdi/font/css/materialdesignicons.css'],
  app: {
    head: {
      link: [
        { rel: 'preconnect', href: 'https://fonts.googleapis.com' },
        { rel: 'preconnect', href: 'https://fonts.gstatic.com', crossorigin: '' },
        {
          rel: 'stylesheet',
          href: 'https://fonts.googleapis.com/css2?family=Inter:wght@400;500;600;700;800&family=JetBrains+Mono:wght@400;500;600&display=swap'
        }
      ]
    }
  },
  runtimeConfig: {
    public: {
      apiBaseUrl: 'http://localhost:5240',
      // Firebase (Task #232 - PBI #230): SDK inicializado, mas sem projeto real ainda.
      // Preencha via NUXT_PUBLIC_FIREBASE_* quando o projeto Firebase existir; nenhum
      // código precisa mudar — o plugin já lida com config vazia sem quebrar a aplicação.
      firebaseApiKey: '',
      firebaseAuthDomain: '',
      firebaseProjectId: '',
      firebaseAppId: ''
    }
  },
  vuetify: {
    vuetifyOptions: {
      theme: {
        defaultTheme: 'light'
      }
    }
  },
  // Evita o re-optimize/reload do Vite em runtime na primeira página que importa esses
  // pacotes (axios via useSprintAlertPolling, firebase via o plugin client-only).
  vite: {
    optimizeDeps: {
      include: ['axios', 'firebase/app', 'firebase/auth', 'firebase/firestore']
    }
  }
})
