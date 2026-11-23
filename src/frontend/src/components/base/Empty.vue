<script>
import Icon from "./Icon.vue";

// Estado vazio de listas e telas. config: { title, text, icon } (igual ao emptyConfig da referência).
export default {
  name: "Empty",
  components: { Icon },
  props: {
    config: { type: Object, default: () => ({}) },
    title: { type: String, default: "" },
    text: { type: String, default: "" },
    icon: { type: String, default: "inbox" },
  },
};
</script>

<template>
  <div class="empty">
    <Icon :name="config.iconName || icon" :size="32" class="empty__icon" />
    <p class="empty__title">{{ config.title || title || "Nada por aqui ainda." }}</p>
    <p v-if="config.text || text" class="empty__text">{{ config.text || text }}</p>
    <slot />
  </div>
</template>

<style scoped>
.empty {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 6px;
  padding: 40px 16px;
  text-align: center;
  color: var(--text-muted);
}
.empty__icon {
  margin-bottom: 4px;
}
.empty__title {
  margin: 0;
  font-weight: 600;
  color: var(--text);
}
.empty__text {
  margin: 0;
  max-width: 420px;
}
</style>
