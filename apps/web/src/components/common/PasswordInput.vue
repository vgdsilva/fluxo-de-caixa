<script setup lang="ts">
import { Eye, EyeOff } from '@lucide/vue';
import { ref } from 'vue';
import { Input } from '@/components/ui/input';

/** Campo de senha com o botão de olho para mostrar/esconder o que foi digitado. */
defineOptions({ inheritAttrs: false });
const model = defineModel<string>({ default: '' });
const visible = ref(false);
</script>

<template>
  <div class="relative">
    <Input v-bind="$attrs" v-model="model" :type="visible ? 'text' : 'password'" class="pr-10" />
    <button
      type="button"
      class="absolute inset-y-0 right-0 flex w-10 items-center justify-center text-emerald-600 hover:text-emerald-700"
      :aria-label="visible ? 'Esconder senha' : 'Mostrar senha'"
      :aria-pressed="visible"
      @click="visible = !visible"
    >
      <Eye v-if="visible" class="size-4" />
      <EyeOff v-else class="size-4" />
    </button>
  </div>
</template>
