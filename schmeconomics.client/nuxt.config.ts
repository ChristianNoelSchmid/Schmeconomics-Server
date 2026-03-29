// https://nuxt.com/docs/api/configuration/nuxt-config
export default defineNuxtConfig({
  compatibilityDate: '2025-07-15',
  devtools: { enabled: true },

  modules: [
    '@nuxt/ui',
    '@nuxt/hints',
    '@nuxt/eslint',
    '@nuxt/image',
    '@vite-pwa/nuxt'
  ],
  runtimeConfig: {
    public: {
      apiBase: 'http://localhost:5153'
    }
  },
  css: [
    '@/styles.css'
  ],
  pwa: {
    registerType: 'autoUpdate',
    manifest: {
      name: 'Schmeconomics',
      short_name: 'Schm',
      theme_color: '#ffffff',
      icons: [
        {
          src: 'pwa-192x192.png',
          sizes: '192x192',
          type: 'image/png',
          purpose: 'any maskable'
        },
        {
          src: 'pwa-512x512.png',
          sizes: '512x512',
          type: 'image/png',
          purpose: 'any maskable'
        },
      ],
    },
    workbox: {
      navigateFallback: '/',
      globPatterns: ['**/*.{js,css,html,png,svg,ico}'],
    },
    devOptions: {
      enabled: true, // Allows you to test PWA features in dev mode
      type: 'module',
    }
  }
})