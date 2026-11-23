<script>
export default {
  name: "InlineSpinner",
  props: {
    size: { type: Number, default: 18 },
    label: { type: String, default: "Carregando" },
  },
};
</script>

<template>
  <span class="spinner" role="status" :style="{ width: `${size}px`, height: `${size}px` }">
    <span class="sr-only">{{ label }}</span>
  </span>
</template>

<style scoped>
.spinner {
  display: inline-block;
  border: 2px solid currentColor;
  border-right-color: transparent;
  border-radius: 50%;
  animation: spin 0.7s linear infinite;
  opacity: 0.8;
}
@keyframes spin {
  to { transform: rotate(360deg); }
}
</style>
