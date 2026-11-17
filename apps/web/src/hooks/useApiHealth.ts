import type { HealthResponse } from '@fluxo/contracts';
import { onMounted, ref } from 'vue';
import { fetchHealth } from '@/services/health.service';

/**
 * Composable (o "hook" do Vue): estado reativo + lógica reaproveitável entre componentes.
 * O componente só desenha; quem chama a API e guarda o estado é isto aqui.
 */
export function useApiHealth() {
  const health = ref<HealthResponse | null>(null);
  const error = ref<string | null>(null);
  const loading = ref(false);

  async function refresh() {
    loading.value = true;
    error.value = null;
    try {
      health.value = await fetchHealth();
    } catch (e) {
      health.value = null;
      error.value = e instanceof Error ? e.message : String(e);
    } finally {
      loading.value = false;
    }
  }

  onMounted(refresh);

  return { health, error, loading, refresh };
}
