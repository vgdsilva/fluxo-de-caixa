<script>
// Logo "fluxo" (opção A do design: setas entra e sai).
// variant: "light" para fundo verde-escuro, "dark" para fundo claro.
export default {
  name: "Logo",
  props: {
    variant: { type: String, default: "dark" },
    size: { type: Number, default: 28 },
    showName: { type: Boolean, default: true },
  },
};
</script>

<template>
  <span class="logo" :class="`logo--${variant}`">
    <svg :width="size" :height="size" viewBox="0 0 32 32" aria-hidden="true">
      <rect width="32" height="32" rx="8" class="logo__bg" />
      <path d="M11 23V9m0 0-4 4m4-4 4 4" fill="none" class="logo__up" stroke-width="2.6" stroke-linecap="round" stroke-linejoin="round" />
      <path d="M21 9v14m0 0-4-4m4 4 4-4" fill="none" class="logo__down" stroke-width="2.6" stroke-linecap="round" stroke-linejoin="round" />
    </svg>
    <span v-if="showName" class="logo__name">fluxo</span>
  </span>
</template>

<style scoped>
.logo {
  display: inline-flex;
  align-items: center;
  gap: 8px;
  font-weight: 700;
  font-size: 18px;
  letter-spacing: -0.02em;
  text-decoration: none;
}
.logo__bg { fill: var(--brand-600); }
.logo__up { stroke: var(--brand-100); }
.logo__down { stroke: #fff; }
.logo--dark .logo__name { color: var(--text); }
.logo--light .logo__name { color: var(--text-on-brand); }
.logo--light .logo__bg { fill: rgb(255 255 255 / 0.12); }
</style>
