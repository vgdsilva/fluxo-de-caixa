import { onMounted, ref } from 'vue';
import { fetchHealth } from '@/services/health.service';

/** Versão da API (rota pública /api/health), para o rodapé das telas de entrada. */
export function useAppVersion() {
  const version = ref<string | null>(null);
  onMounted(async () => {
    try {
      version.value = (await fetchHealth()).version;
    } catch {
      version.value = null;
    }
  });
  return version;
}
