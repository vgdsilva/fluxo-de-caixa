import { onMounted, ref } from 'vue';
import { useRouter } from 'vue-router';
import type { FeedItem } from '@/components/common/MessageFeed.vue';
import { ApiError } from '@/services/api';
import { authService } from '@/services/auth.service';
import { useAuthStore } from '@/store/auth.store';

/** Tempo mínimo de cada passo na tela, para dar para ler o que está acontecendo. */
const MIN_STEP_MS = 450;
/** Quanto tempo o feed verde fica na tela antes de abrir o app. */
const SUCCESS_HOLD_MS = 1200;

const wait = (ms: number) => new Promise((resolve) => setTimeout(resolve, ms));

/**
 * A tela de sessão do login: consome as credenciais que o formulário deixou na store e executa
 * os passos mostrando cada um no feed. Sucesso: feed verde e vai para o app. Erro: guarda a
 * mensagem e vai para a tela de erro.
 */
export function useLoginSession() {
  const router = useRouter();
  const auth = useAuthStore();

  const items = ref<FeedItem[]>([
    { id: 'credentials', label: 'Validando suas credenciais', status: 'pending' },
    { id: 'session', label: 'Abrindo uma sessão segura', status: 'pending' },
    { id: 'profile', label: 'Carregando seu perfil', status: 'pending' },
  ]);
  const tone = ref<'neutral' | 'success' | 'error'>('neutral');
  const welcome = ref<string | null>(null);

  async function step<T>(id: string, fn: () => Promise<T>): Promise<T> {
    const item = items.value.find((i) => i.id === id)!;
    item.status = 'running';
    try {
      const [result] = await Promise.all([fn(), wait(MIN_STEP_MS)]);
      item.status = 'done';
      return result;
    } catch (e) {
      item.status = 'error';
      throw e;
    }
  }

  async function run(): Promise<void> {
    const credentials = auth.takePendingLogin();
    if (!credentials) {
      // Chegou aqui sem passar pelo formulário (ex.: recarregou a página).
      await router.replace({ name: 'login' });
      return;
    }

    try {
      await step('credentials', () => authService.login(credentials));
      // Confirma que o cookie da sessão foi aceito pelo navegador e vale numa chamada nova.
      const me = await step('session', () => authService.me());
      await step('profile', async () => auth.setUser(me));

      tone.value = 'success';
      welcome.value = `Tudo certo! Bem-vindo(a), ${me.name.split(' ')[0]}.`;
      await wait(SUCCESS_HOLD_MS);
      const target = auth.redirectTo || '/';
      auth.redirectTo = '/';
      await router.replace(target);
    } catch (e) {
      tone.value = 'error';
      auth.fail(e instanceof ApiError ? e.message : 'Algo deu errado ao entrar. Tente de novo.');
      await wait(MIN_STEP_MS);
      await router.replace({ name: 'login-error' });
    }
  }

  onMounted(run);

  return { items, tone, welcome };
}
