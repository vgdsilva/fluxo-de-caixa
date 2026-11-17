import { onBeforeUnmount, onMounted, ref } from 'vue';

/** Está com internet? Atualiza sozinho quando a conexão cai ou volta. */
export function useOnlineStatus() {
  const online = ref(typeof navigator === 'undefined' ? true : navigator.onLine);
  const update = () => (online.value = navigator.onLine);
  onMounted(() => {
    window.addEventListener('online', update);
    window.addEventListener('offline', update);
  });
  onBeforeUnmount(() => {
    window.removeEventListener('online', update);
    window.removeEventListener('offline', update);
  });
  return online;
}
