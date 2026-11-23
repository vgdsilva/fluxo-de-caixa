<script>
// Tela provisória para os módulos que ainda não foram feitos.
export default {};
</script>

<template>
  <div class="page">
    <div class="page__header">
      <h1 class="page__title">{{ $route.meta.title }}</h1>
    </div>
    <Card>
      <Empty icon="construction" title="Tela em construção" text="Esta parte do app ainda vai ser feita." />
    </Card>
  </div>
</template>
