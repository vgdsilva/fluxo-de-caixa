<script>
export default {
  name: "Card",
  props: {
    title: { type: String, default: "" },
    subtitle: { type: String, default: "" },
    padding: { type: Boolean, default: true },
  },
};
</script>

<template>
  <section class="card" :class="{ 'card--flush': !padding }">
    <header v-if="title || $slots.actions" class="card__header">
      <div>
        <h2 v-if="title" class="card__title">{{ title }}</h2>
        <p v-if="subtitle" class="card__subtitle">{{ subtitle }}</p>
      </div>
      <div v-if="$slots.actions" class="card__actions"><slot name="actions" /></div>
    </header>
    <slot />
    <footer v-if="$slots.footer" class="card__footer"><slot name="footer" /></footer>
  </section>
</template>

<style scoped>
.card {
  background: var(--surface);
  border: 1px solid var(--border);
  border-radius: var(--radius-lg);
  padding: 20px;
  box-shadow: var(--shadow-sm);
}
.card--flush {
  padding: 0;
  overflow: hidden;
}
.card__header {
  display: flex;
  align-items: flex-start;
  justify-content: space-between;
  gap: 12px;
  margin-bottom: 16px;
}
.card--flush .card__header {
  padding: 16px 20px 0;
}
.card__title {
  font-size: 16px;
}
.card__subtitle {
  margin: 4px 0 0;
  color: var(--text-muted);
  font-size: 13px;
}
.card__actions {
  display: flex;
  gap: 8px;
}
.card__footer {
  margin-top: 16px;
}
</style>
