<script>
import Logo from "@/components/base/Logo.vue";
import LoginShowcase from "./LoginShowcase.vue";
import { getApplicationVersion, getApplicationTitle } from "@/system/applicationConfigHelper.js";

// Layout das telas públicas (login, saída, sessão expirada) no redesign claro.
// Desktop: painel verde com prévias do app à esquerda e formulário à direita. Celular: só o formulário.
export default {
  name: "Auth",
  components: { Logo, LoginShowcase },
  computed: {
    version: () => getApplicationVersion(),
    title: () => getApplicationTitle(),
    year: () => new Date().getFullYear(),
  },
};
</script>

<template>
  <div class="auth">
    <aside class="auth__panel">
      <Logo variant="light" :size="32" />
      <div class="auth__panel-body">
        <p class="auth__headline">Seu dinheiro organizado e o fim do mês sem surpresas.</p>
        <LoginShowcase />
      </div>
    </aside>

    <main class="auth__main">
      <div class="auth__content">
        <RouterView />
      </div>
      <footer class="auth__footer">
        <span>{{ title }} · versão {{ version }}</span>
        <span>© {{ year }} · Uso pessoal</span>
      </footer>
    </main>
  </div>
</template>

<style scoped>
.auth {
  display: flex;
  min-height: 100%;
  background: var(--surface);
}
.auth__panel {
  display: none;
  flex-direction: column;
  gap: 32px;
  width: 52%;
  padding: 32px 40px;
  background: var(--brand-500);
  color: var(--text-on-brand);
  overflow: hidden;
}
.auth__panel-body {
  display: flex;
  flex: 1;
  flex-direction: column;
  justify-content: center;
  gap: 28px;
}
.auth__headline {
  margin: 0;
  max-width: 480px;
  font-size: 28px;
  font-weight: 700;
  line-height: 1.25;
}
.auth__main {
  display: flex;
  flex: 1;
  flex-direction: column;
}
.auth__content {
  display: flex;
  flex: 1;
  align-items: center;
  justify-content: center;
  padding: 32px 16px;
}
.auth__footer {
  display: flex;
  justify-content: center;
  gap: 16px;
  flex-wrap: wrap;
  padding: 16px;
  font-size: 12px;
  color: var(--text-muted);
}

@media (min-width: 1000px) {
  .auth__panel {
    display: flex;
  }
}
</style>
