<script setup lang="ts">
import { Check, Copy, MessageCircle } from '@lucide/vue';
import { ref } from 'vue';
import { Button } from '@/components/ui/button';

/**
 * Mostra um link de uso único (convite, redefinição de senha) com botões de copiar e de abrir
 * no WhatsApp. O link só aparece uma vez: a API não guarda o token, só o hash.
 */
const props = defineProps<{ link: string; expiresLabel?: string; whatsappText?: string }>();
const copied = ref(false);

async function copy() {
  await navigator.clipboard.writeText(props.link);
  copied.value = true;
  setTimeout(() => (copied.value = false), 2000);
}

function whatsapp() {
  const text = `${props.whatsappText ?? 'Seu link do Fluxo de Caixa:'} ${props.link}`;
  window.open(`https://wa.me/?text=${encodeURIComponent(text)}`, '_blank', 'noopener');
}
</script>

<template>
  <div class="rounded-lg border border-emerald-300 bg-emerald-50 p-3">
    <p class="mb-2 text-xs font-medium text-emerald-800">
      Copie agora: este link não aparece de novo.
      <span v-if="expiresLabel" class="font-normal">{{ expiresLabel }}</span>
    </p>
    <code class="block break-all rounded bg-white px-2 py-1.5 text-xs text-neutral-800">{{
      link
    }}</code>
    <div class="mt-2 flex flex-wrap gap-2">
      <Button size="sm" variant="outline" @click="copy">
        <Check v-if="copied" />
        <Copy v-else />
        {{ copied ? 'Copiado' : 'Copiar link' }}
      </Button>
      <Button size="sm" variant="outline" @click="whatsapp">
        <MessageCircle />
        Enviar pelo WhatsApp
      </Button>
    </div>
  </div>
</template>
