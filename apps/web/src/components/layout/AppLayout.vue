<script setup lang="ts">
import { LogOut } from '@lucide/vue';
import { useRouter } from 'vue-router';
import { Button } from '@/components/ui/button';
import { useAuthStore } from '@/store/auth.store';

// Casca das telas do app (cabeçalho com o usuário e "Sair"). O menu entra na fatia 6.
const auth = useAuthStore();
const router = useRouter();

async function logout() {
  await auth.logout();
  await router.replace({ name: 'login' });
}
</script>

<template>
  <div class="min-h-dvh">
    <header class="border-b">
      <div class="mx-auto flex h-14 max-w-3xl items-center justify-between gap-4 px-4">
        <span class="flex items-center gap-2 font-semibold">
          <img src="/favicon.ico" alt="" class="size-6 rounded-md" />
          Fluxo de Caixa
        </span>
        <div v-if="auth.user" class="flex items-center gap-3 text-sm">
          <span class="text-muted-foreground">{{ auth.user.name }}</span>
          <Button variant="ghost" size="sm" @click="logout">
            <LogOut />
            Sair
          </Button>
        </div>
      </div>
    </header>
    <main class="mx-auto max-w-3xl px-4 py-6">
      <slot />
    </main>
  </div>
</template>
