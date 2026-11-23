<script>
import Icon from "./Icon.vue";

let seq = 0;

// Modal centralizado. Fecha com Esc e clique fora. No celular ocupa a largura toda.
export default {
  name: "ModalCenter",
  components: { Icon },
  props: {
    show: { type: Boolean, default: false },
    title: { type: String, default: "" },
    width: { type: String, default: "520px" },
    closeOnBackdrop: { type: Boolean, default: true },
  },
  emits: ["close"],
  data() {
    return { uid: `modal-${++seq}` };
  },
  watch: {
    show(value) {
      if (value) this.$nextTick(() => this.$refs.dialog?.focus());
    },
  },
  mounted() {
    window.addEventListener("keydown", this.onKeydown);
  },
  unmounted() {
    window.removeEventListener("keydown", this.onKeydown);
  },
  methods: {
    onKeydown(event) {
      if (this.show && event.key === "Escape") this.$emit("close");
    },
    onBackdrop() {
      if (this.closeOnBackdrop) this.$emit("close");
    },
  },
};
</script>

<template>
  <Teleport to="body">
    <div v-if="show" class="modal" @mousedown.self="onBackdrop">
      <div
        ref="dialog"
        class="modal__dialog"
        role="dialog"
        aria-modal="true"
        :aria-labelledby="title ? `${uid}-title` : undefined"
        tabindex="-1"
        :style="{ maxWidth: width }"
      >
        <header class="modal__header">
          <h2 v-if="title" :id="`${uid}-title`" class="modal__title">{{ title }}</h2>
          <button type="button" class="modal__close" aria-label="Fechar" @click="$emit('close')">
            <Icon name="x" />
          </button>
        </header>
        <div class="modal__body"><slot /></div>
        <footer v-if="$slots.footer" class="modal__footer"><slot name="footer" /></footer>
      </div>
    </div>
  </Teleport>
</template>

<style scoped>
.modal {
  position: fixed;
  inset: 0;
  z-index: 100;
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 16px;
  background: rgb(0 0 0 / 0.4);
}
.modal__dialog {
  width: 100%;
  max-height: calc(100vh - 32px);
  overflow: auto;
  background: var(--surface);
  border-radius: var(--radius-lg);
  box-shadow: var(--shadow);
  outline: none;
}
.modal__header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 16px 20px;
  border-bottom: 1px solid var(--border);
}
.modal__title {
  font-size: 16px;
}
.modal__close {
  border: 0;
  background: transparent;
  cursor: pointer;
  color: var(--text-muted);
  padding: 4px;
  border-radius: var(--radius-sm);
}
.modal__body {
  padding: 20px;
}
.modal__footer {
  display: flex;
  justify-content: flex-end;
  gap: 8px;
  padding: 12px 20px;
  border-top: 1px solid var(--border);
}
</style>
