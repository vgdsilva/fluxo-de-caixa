<script setup lang="ts">
import { ClipboardList, Home, LogOut, Mail, User, Users } from '@lucide/vue';
import { computed } from 'vue';
import { RouterLink, useRouter } from 'vue-router';
import BrandMark from '@/components/common/BrandMark.vue';
import { Button } from '@/components/ui/button';
import { useAuthStore } from '@/store/auth.store';

/**
 * Casca das telas do app: cabeçalho com a marca, o menu e "Sair". No celular o menu vira uma
 * faixa que rola para o lado, logo abaixo do cabeçalho.
 */
const auth = useAuthStore();
const router = useRouter();

const links = computed(() => [
  { to: '/', label: 'Início', icon: Home },
  { to: '/perfil', label: 'Meu perfil', icon: User },
  ...(auth.user?.role === 'admin'
    ? [
        { to: '/admin/usuarios', label: 'Usuários', icon: Users },
        { to: '/admin/convites', label: 'Convites', icon: Mail },
        { to: '/admin/auditoria', label: 'Auditoria', icon: ClipboardList },
      ]
    : []),
]);

async function logout() {
  await auth.logout();
  await router.replace({ name: 'login' });
}
</script>

<template>
  <div class="min-h-dvh">
    <header class="border-b bg-white">
      <div class="mx-auto flex h-14 max-w-5xl items-center justify-between gap-4 px-4">
        <RouterLink to="/" aria-label="Início">
          <BrandMark />
        </RouterLink>
        <div v-if="auth.user" class="flex items-center gap-3 text-sm">
          <span class="hidden text-muted-foreground sm:inline">{{ auth.user.name }}</span>
          <Button variant="ghost" size="sm" @click="logout">
            <LogOut />
            Sair
          </Button>
        </div>
      </div>
      <nav class="mx-auto max-w-5xl overflow-x-auto px-4" aria-label="Menu">
        <ul class="flex gap-1 pb-2">
          <li v-for="link in links" :key="link.to">
            <RouterLink
              :to="link.to"
              class="flex items-center gap-1.5 rounded-md px-3 py-1.5 text-sm whitespace-nowrap text-muted-foreground hover:bg-muted hover:text-foreground"
              exact-active-class="bg-muted font-medium text-foreground"
            >
              <component :is="link.icon" class="size-4" />
              {{ link.label }}
            </RouterLink>
          </li>
        </ul>
      </nav>
    </header>
    <main class="mx-auto max-w-5xl px-4 py-6">
      <slot />
    </main>
  </div>
</template>
