<script setup lang="ts">
import { WifiOff } from '@lucide/vue';
import { useOnlineStatus } from '@/hooks/useOnlineStatus';

/** Aviso fixo no topo quando o aparelho está sem internet (o app abre, mas os dados não). */
const online = useOnlineStatus();
</script>

<template>
  <div
    v-if="!online"
    role="status"
    class="sticky top-0 z-50 flex items-center justify-center gap-2 bg-amber-100 px-4 py-2 text-sm text-amber-900"
  >
    <WifiOff class="size-4" />
    Sem conexão. Os dados aparecem quando a internet voltar.
  </div>
</template>
