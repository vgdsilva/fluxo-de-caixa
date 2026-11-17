import { createPinia } from 'pinia';
import { createApp } from 'vue';
import App from './App.vue';
import { router } from './routes';
import { setUnauthorizedHandler } from './services/api';
import { useAuthStore } from './store/auth.store';
import './assets/styles/main.css';

const app = createApp(App);

app.use(createPinia());
app.use(router);

// Sessão expirou no meio do uso (API respondeu 401): esquece o usuário e volta para o login.
setUnauthorizedHandler(() => {
  useAuthStore().clearSession();
  if (!router.currentRoute.value.meta.public) {
    useAuthStore().redirectTo = router.currentRoute.value.fullPath;
    void router.replace({ name: 'login' });
  }
});

app.mount('#app');
