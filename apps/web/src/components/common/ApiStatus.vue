<script setup lang="ts">
import { CircleAlert, CircleCheck, RefreshCw } from '@lucide/vue';
import { Alert, AlertDescription, AlertTitle } from '@/components/ui/alert';
import { Button } from '@/components/ui/button';
import { useApiHealth } from '@/hooks/useApiHealth';

const { health, error, loading, refresh } = useApiHealth();
</script>

<template>
  <section class="flex flex-col items-start gap-3">
    <Alert v-if="health" data-testid="api-ok">
      <CircleCheck />
      <AlertTitle>API no ar</AlertTitle>
      <AlertDescription>
        Versão {{ health.version }}, {{ new Date(health.time).toLocaleString('pt-BR') }}
      </AlertDescription>
    </Alert>
    <Alert v-else-if="error" variant="destructive" data-testid="api-error">
      <CircleAlert />
      <AlertTitle>Não foi possível falar com a API</AlertTitle>
      <AlertDescription>{{ error }}</AlertDescription>
    </Alert>
    <Button variant="outline" size="sm" :disabled="loading" @click="refresh">
      <RefreshCw :class="{ 'animate-spin': loading }" />
      Verificar de novo
    </Button>
  </section>
</template>
