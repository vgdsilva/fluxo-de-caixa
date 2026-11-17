import { onMounted, ref, type Ref } from 'vue';
import { ApiError } from '@/services/api';

/**
 * Carrega um dado da API ao montar a tela e expõe `data`, `loading`, `error` e `reload`.
 * O equivalente Vue de um "useQuery" simples. Erro vira mensagem pronta para a tela.
 */
export function useResource<T>(loader: () => Promise<T>) {
  const data = ref<T | null>(null) as Ref<T | null>;
  const loading = ref(false);
  const error = ref<string | null>(null);

  async function reload(): Promise<void> {
    loading.value = true;
    error.value = null;
    try {
      data.value = await loader();
    } catch (e) {
      error.value = e instanceof ApiError ? e.message : 'Não foi possível carregar.';
    } finally {
      loading.value = false;
    }
  }

  onMounted(reload);
  return { data, loading, error, reload };
}

/** Executa uma ação (salvar, revogar...) guardando `busy` e a mensagem de erro. */
export function useAction() {
  const busy = ref(false);
  const error = ref<string | null>(null);

  async function run<T>(fn: () => Promise<T>): Promise<T | undefined> {
    busy.value = true;
    error.value = null;
    try {
      return await fn();
    } catch (e) {
      error.value = e instanceof ApiError ? e.message : 'Não foi possível concluir.';
      return undefined;
    } finally {
      busy.value = false;
    }
  }

  return { busy, error, run };
}
