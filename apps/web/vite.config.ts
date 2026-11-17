import { fileURLToPath, URL } from 'node:url';
import tailwindcss from '@tailwindcss/vite';
import vue from '@vitejs/plugin-vue';
import { loadEnv } from 'vite';
import { VitePWA } from 'vite-plugin-pwa';
import { defineConfig } from 'vitest/config';

export default defineConfig(({ mode }) => {
  // Lê apps/web/.env (o prefixo '' carrega também as variáveis sem VITE_, que ficam só aqui no Node).
  const env = loadEnv(mode, process.cwd(), '');

  return {
    plugins: [
      vue(),
      tailwindcss(),
      // PWA: manifest + service worker. A "casca" do app (HTML, JS, CSS, fontes, ícones) fica em
      // cache e abre sem sinal; os dados (/api) nunca são cacheados, sempre vêm da API.
      VitePWA({
        registerType: 'autoUpdate',
        includeAssets: ['favicon.ico', 'icons/apple-touch-icon.png'],
        manifest: {
          name: 'Fluxo de Caixa',
          short_name: 'Fluxo',
          description: 'Quanto vai sobrar nos próximos meses, e cabe esta compra?',
          lang: 'pt-BR',
          start_url: '/',
          scope: '/',
          display: 'standalone',
          orientation: 'portrait',
          background_color: '#ffffff',
          theme_color: '#171717',
          icons: [
            { src: '/icons/icon-192.png', sizes: '192x192', type: 'image/png' },
            { src: '/icons/icon-512.png', sizes: '512x512', type: 'image/png' },
            {
              src: '/icons/maskable-512.png',
              sizes: '512x512',
              type: 'image/png',
              purpose: 'maskable',
            },
          ],
        },
        workbox: {
          globPatterns: ['**/*.{js,css,html,ico,png,svg,woff2}'],
          navigateFallback: '/index.html',
          navigateFallbackDenylist: [new RegExp('^/api/')],
          cleanupOutdatedCaches: true,
        },
      }),
    ],
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
