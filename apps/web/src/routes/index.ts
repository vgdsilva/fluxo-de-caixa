import { createRouter, createWebHistory } from 'vue-router';
import { useAuthStore } from '@/store/auth.store';

declare module 'vue-router' {
  interface RouteMeta {
    /** Abre sem login. */
    public?: boolean;
    /** 'auth' = tela dividida do login (sem o cabeçalho do app). */
    layout?: 'app' | 'auth';
  }
}

// As rotas apontam para páginas em pages/ (uma pasta por área, ex.: pages/auth/).
export const router = createRouter({
  history: createWebHistory(),
  routes: [
    {
      path: '/',
      name: 'home',
      component: () => import('@/pages/HomePage.vue'),
    },
    {
      path: '/login',
      name: 'login',
      component: () => import('@/pages/auth/LoginPage.vue'),
      meta: { public: true, layout: 'auth' },
    },
    {
      path: '/login/sessao',
      name: 'login-session',
      component: () => import('@/pages/auth/LoginSessionPage.vue'),
      meta: { public: true, layout: 'auth' },
    },
    {
      path: '/login/erro',
      name: 'login-error',
      component: () => import('@/pages/auth/LoginErrorPage.vue'),
      meta: { public: true, layout: 'auth' },
    },
    {
      path: '/setup',
      name: 'setup',
      component: () => import('@/pages/auth/SetupPage.vue'),
      meta: { public: true, layout: 'auth' },
    },
    { path: '/:pathMatch(.*)*', redirect: '/' },
  ],
});

/**
 * Guarda de navegação: tela que não é pública exige login. Ela só esconde telas; quem protege
 * os dados de verdade é a API (SessionGuard), que responde 401 sem sessão válida.
 */
router.beforeEach(async (to) => {
  const auth = useAuthStore();
  if (to.meta.public) {
    // Já logado e abrindo o formulário de login: vai direto para o app.
    if (to.name === 'login' && auth.checked && auth.isLoggedIn) return { name: 'home' };
    return true;
  }
  try {
    await auth.ensureChecked();
  } catch {
    // API fora do ar: deixa abrir e a tela mostra o erro.
    return true;
  }
  if (!auth.isLoggedIn) {
    auth.redirectTo = to.fullPath;
    return { name: 'login' };
  }
  return true;
});
