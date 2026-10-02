// Task #232 (PBI #230): inicialização do SDK do Firebase (autenticação/banco).
// Client-only porque o SDK JS do Firebase é voltado pro navegador.
// Sem projeto Firebase real ainda — a config vem vazia por padrão (ver nuxt.config.ts),
// e a inicialização é pulada com um aviso em vez de derrubar a aplicação.
import { initializeApp, type FirebaseApp } from 'firebase/app'
import { getAuth, type Auth } from 'firebase/auth'
import { getFirestore, type Firestore } from 'firebase/firestore'

export default defineNuxtPlugin(() => {
  const config = useRuntimeConfig().public

  let app: FirebaseApp | null = null
  let auth: Auth | null = null
  let firestore: Firestore | null = null

  if (config.firebaseApiKey && config.firebaseProjectId) {
    app = initializeApp({
      apiKey: config.firebaseApiKey,
      authDomain: config.firebaseAuthDomain,
      projectId: config.firebaseProjectId,
      appId: config.firebaseAppId
    })
    auth = getAuth(app)
    firestore = getFirestore(app)
  } else {
    // eslint-disable-next-line no-console
    console.warn(
      '[firebase] Config não definida (NUXT_PUBLIC_FIREBASE_*) — SDK carregado, mas não inicializado.'
    )
  }

  return {
    provide: { firebaseApp: app, firebaseAuth: auth, firebaseFirestore: firestore }
  }
})
