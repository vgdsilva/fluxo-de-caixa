import type { LoginInput, Me } from '@fluxo/contracts';
import { defineStore } from 'pinia';
import { computed, ref } from 'vue';
import { ApiError } from '@/services/api';
import { authService } from '@/services/auth.service';
import { setupService } from '@/services/setup.service';

/**
 * Estado de autenticação do app (store do Pinia, um singleton com estado).
 *
 * O fluxo de login passa por três telas: o formulário guarda as credenciais aqui (`startLogin`),
 * a tela de sessão as consome uma única vez (`takePendingLogin`) e, se der erro, a mensagem fica
 * em `lastError` para a tela de erro. A senha nunca vai para a URL nem para o armazenamento do
 * navegador: só fica na memória até a tela de sessão usar.
 */
export const useAuthStore = defineStore('auth', () => {
  const user = ref<Me | null>(null);
  /** Já perguntamos à API se há sessão? (evita chamar /me a cada navegação) */
  const checked = ref(false);
  const pendingLogin = ref<LoginInput | null>(null);
  const lastError = ref<string | null>(null);
  /** Para onde ir depois de entrar (rota que o usuário tentou abrir). */
  const redirectTo = ref<string>('/');
  /** Usuário digitado, para preencher de novo ao "Tentar novamente". */
  const lastUsername = ref('');

  const isLoggedIn = computed(() => user.value !== null);
  /** O sistema ainda não tem admin (SETUP_DB = 0)? null = ainda não perguntamos. */
  const setupRequired = ref<boolean | null>(null);

  async function ensureSetupChecked(): Promise<boolean> {
    if (setupRequired.value === null) setupRequired.value = await setupService.isRequired();
    return setupRequired.value;
  }

  function markSetupDone(): void {
    setupRequired.value = false;
  }

  async function ensureChecked(): Promise<void> {
    if (checked.value) return;
    try {
      user.value = await authService.me();
    } catch (e) {
      if (!(e instanceof ApiError && e.status === 401)) throw e;
      user.value = null;
    } finally {
      checked.value = true;
    }
  }

  function startLogin(input: LoginInput): void {
    pendingLogin.value = input;
    lastUsername.value = input.username;
    lastError.value = null;
  }

  function takePendingLogin(): LoginInput | null {
    const input = pendingLogin.value;
    pendingLogin.value = null;
    return input;
  }

  function setUser(me: Me): void {
    user.value = me;
    checked.value = true;
  }

  function fail(message: string): void {
    lastError.value = message;
  }

  async function logout(): Promise<void> {
    try {
      await authService.logout();
    } finally {
      clearSession();
    }
  }

  /** Esquece o usuário (logout ou sessão expirada). */
  function clearSession(): void {
    user.value = null;
    checked.value = true;
  }

  return {
    user,
    checked,
    isLoggedIn,
    setupRequired,
    ensureSetupChecked,
    markSetupDone,
    lastError,
    redirectTo,
    lastUsername,
    ensureChecked,
    startLogin,
    takePendingLogin,
    setUser,
    fail,
    logout,
    clearSession,
  };
});
