<script setup lang="ts">
import { RouterView, useRoute } from 'vue-router';
import AppLayout from '@/components/layout/AppLayout.vue';

// As telas de login têm o próprio layout (AuthLayout, tela dividida); o resto usa o do app.
const route = useRoute();
</script>

<template>
  <RouterView v-if="route.meta.layout === 'auth'" />
  <AppLayout v-else>
    <RouterView />
  </AppLayout>
</template>
