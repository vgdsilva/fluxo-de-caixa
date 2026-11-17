import { fileURLToPath, URL } from 'node:url';
import tailwindcss from '@tailwindcss/vite';
import vue from '@vitejs/plugin-vue';
import { loadEnv } from 'vite';
import { defineConfig } from 'vitest/config';

export default defineConfig(({ mode }) => {
  // Lê apps/web/.env (o prefixo '' carrega também as variáveis sem VITE_, que ficam só aqui no Node).
  const env = loadEnv(mode, process.cwd(), '');

  return {
    plugins: [vue(), tailwindcss()],
    resolve: {
      alias: {
        '@': fileURLToPath(new URL('./src', import.meta.url)),
      },
    },
    server: {
      port: 5173,
      // Em desenvolvimento o Vite repassa /api para o Nest. Assim o front sempre chama a mesma
      // origem, como será em produção (um container só), e não precisamos de CORS.
      proxy: {
        '/api': env.API_PROXY_TARGET || 'http://localhost:3000',
      },
    },
    test: {
      environment: 'node',
      passWithNoTests: true,
    },
  };
});
