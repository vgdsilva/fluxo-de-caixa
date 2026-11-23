<script>
import Icon from "./Icon.vue";
import InlineSpinner from "./InlineSpinner.vue";

// kind: primary | secondary | ghost | danger. size: sm | md | lg
export default {
  name: "Button",
  components: { Icon, InlineSpinner },
  props: {
    label: { type: String, default: "" },
    kind: { type: String, default: "primary" },
    size: { type: String, default: "md" },
    icon: { type: String, default: "" },
    type: { type: String, default: "button" },
    disabled: { type: Boolean, default: false },
    loading: { type: Boolean, default: false },
    block: { type: Boolean, default: false },
  },
  emits: ["click"],
};
</script>

<template>
  <button
    class="btn"
    :class="[`btn--${kind}`, `btn--${size}`, { 'btn--block': block }]"
    :type="type"
    :disabled="disabled || loading"
    :aria-busy="loading || undefined"
    @click="$emit('click', $event)"
  >
    <InlineSpinner v-if="loading" :size="14" />
    <Icon v-else-if="icon" :name="icon" :size="size === 'sm' ? 14 : 16" />
    <span v-if="label || $slots.default"><slot>{{ label }}</slot></span>
  </button>
</template>

<style scoped>
.btn {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  gap: 8px;
  border: 1px solid transparent;
  border-radius: var(--radius-sm);
  font: inherit;
  font-weight: 600;
  cursor: pointer;
  transition: background-color 0.15s, border-color 0.15s, color 0.15s;
  white-space: nowrap;
}
.btn--sm { height: 30px; padding: 0 10px; font-size: 13px; }
.btn--md { height: 38px; padding: 0 16px; }
.btn--lg { height: 44px; padding: 0 20px; font-size: 15px; }
.btn--block { width: 100%; }

.btn--primary { background: var(--primary); color: var(--primary-text); }
.btn--primary:hover:not(:disabled) { background: var(--primary-hover); }

.btn--secondary { background: var(--surface); color: var(--text); border-color: var(--border); }
.btn--secondary:hover:not(:disabled) { background: var(--surface-muted); }

.btn--ghost { background: transparent; color: var(--text); }
.btn--ghost:hover:not(:disabled) { background: var(--surface-muted); }

.btn--danger { background: var(--danger); color: #fff; }
.btn--danger:hover:not(:disabled) { filter: brightness(0.92); }

.btn:disabled {
  cursor: not-allowed;
  background: var(--surface-muted);
  color: var(--text-muted);
  border-color: var(--border);
}
</style>
