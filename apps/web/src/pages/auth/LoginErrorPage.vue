<script setup lang="ts">
import { RotateCcw } from '@lucide/vue';
import { computed } from 'vue';
import { useRouter } from 'vue-router';
import MessageFeed, { type FeedItem } from '@/components/common/MessageFeed.vue';
import AuthLayout from '@/components/layout/AuthLayout.vue';
import { Button } from '@/components/ui/button';
import { useAuthStore } from '@/store/auth.store';

const router = useRouter();
const auth = useAuthStore();

const items = computed<FeedItem[]>(() => [
  {
    id: 'error',
    label: 'Não foi possível entrar',
    status: 'error',
    detail: auth.lastError ?? 'Tente de novo.',
  },
]);

function retry() {
  auth.lastError = null;
  void router.replace({ name: 'login' });
}
</script>

<template>
  <AuthLayout>
    <h1 class="text-2xl font-semibold">Algo deu errado</h1>
    <p class="mt-1 mb-8 text-sm text-muted-foreground">Confira os dados e tente de novo.</p>

    <MessageFeed :items="items" tone="error" data-testid="login-error" />

    <Button size="lg" class="mt-6 h-10 w-full" @click="retry">
      <RotateCcw />
      Tentar novamente
    </Button>
  </AuthLayout>
</template>
