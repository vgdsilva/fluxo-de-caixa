import { createRouter, createWebHistory } from 'vue-router';

// As rotas apontam para páginas dentro de features/. A guarda de autenticação entra na fatia 4,
// e só esconde telas: quem protege os dados de verdade é a API.
export const router = createRouter({
  history: createWebHistory(),
  routes: [
    {
      path: '/',
      name: 'home',
      component: () => import('@/features/home/HomePage.vue'),
    },
  ],
});
