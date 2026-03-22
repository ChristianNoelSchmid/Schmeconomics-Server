// https://nuxt.com/docs/api/configuration/nuxt-config
export default defineNuxtConfig({
  compatibilityDate: '2025-07-15',
  devtools: { enabled: true },

  modules: [
    '@nuxt/ui',
    '@nuxt/hints',
    '@nuxt/eslint',
    '@nuxt/image'
  ],
  runtimeConfig: {
    public: {
      apiBase: 'http://localhost:5153/api/v1'
    }
  }
})
