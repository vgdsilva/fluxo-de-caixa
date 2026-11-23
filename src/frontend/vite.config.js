import { fileURLToPath, URL } from 'node:url'
import { defineConfig, loadEnv } from 'vite'
import vue from '@vitejs/plugin-vue'
import { devMockApi } from './mock/devMockApi.js'

// https://vite.dev/config/
export default defineConfig(({ mode }) => {
  const env = loadEnv(mode, process.cwd(), '')
  const useMock = mode === 'mock'

  return {
    plugins: [
      vue(),
      // `npm run dev:mock`: responde /api/* com dados falsos enquanto o WebAPI não está pronto.
      useMock && devMockApi(),
    ].filter(Boolean),

    resolve: {
      alias: {
        '@': fileURLToPath(new URL('./src', import.meta.url)),
      },
    },

    build: {
      sourcemap: false,
      emptyOutDir: true,
      chunkSizeWarningLimit: 1000,
    },

    server: {
      port: 8080,
      // Mesmo origin do backend em dev: o front chama /api e o Vite repassa para o WebAPI.
      proxy: useMock
        ? undefined
        : {
            '/api': {
              target: env.VITE_PROXY_BACKEND || 'http://localhost:5039',
              changeOrigin: true,
            },
          },
    },
  }
})
