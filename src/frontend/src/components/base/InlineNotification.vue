<script>
import Icon from "./Icon.vue";

// kind: error | warning | success | info
const ICON_BY_KIND = { error: "alert-circle", warning: "alert-circle", success: "check", info: "info" };

export default {
  name: "InlineNotification",
  components: { Icon },
  props: {
    kind: { type: String, default: "info" },
    title: { type: String, default: "" },
    text: { type: String, default: "" },
    closable: { type: Boolean, default: false },
  },
  emits: ["close"],
  computed: {
    icon() {
      return ICON_BY_KIND[this.kind] ?? "info";
    },
  },
};
</script>

<template>
  <div class="notification" :class="`notification--${kind}`" :role="kind === 'error' ? 'alert' : 'status'">
    <Icon :name="icon" :size="18" />
    <div class="notification__body">
      <strong v-if="title">{{ title }}</strong>
      <span v-if="text || $slots.default"><slot>{{ text }}</slot></span>
    </div>
    <button v-if="closable" type="button" class="notification__close" aria-label="Fechar" @click="$emit('close')">
      <Icon name="x" :size="16" />
    </button>
  </div>
</template>

<style scoped>
.notification {
  display: flex;
  align-items: flex-start;
  gap: 10px;
  padding: 12px 14px;
  border-radius: var(--radius-sm);
  font-size: 13px;
}
.notification__body {
  display: flex;
  flex-direction: column;
  gap: 2px;
  flex: 1;
}
.notification--error { background: var(--danger-bg); color: var(--danger); }
.notification--warning { background: var(--warning-bg); color: var(--warning); }
.notification--success { background: var(--success-bg); color: var(--success); }
.notification--info { background: var(--info-bg); color: var(--info); }
.notification__close {
  border: 0;
  background: transparent;
  color: inherit;
  cursor: pointer;
  padding: 0;
}
</style>
