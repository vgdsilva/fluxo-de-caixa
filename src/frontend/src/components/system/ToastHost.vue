<script>
import { mapState } from "vuex";
import InlineNotification from "@/components/base/InlineNotification.vue";

// Mostra as mensagens criadas por ServiceUtils.showToast.
export default {
  name: "ToastHost",
  components: { InlineNotification },
  computed: {
    ...mapState("main", ["toasts"]),
  },
};
</script>

<template>
  <div class="toasts" aria-live="polite">
    <InlineNotification
      v-for="toast in toasts"
      :key="toast.id"
      class="toasts__item"
      :kind="toast.type"
      :title="toast.title"
      :text="toast.text"
      closable
      @close="$store.commit('main/REMOVE_TOAST', toast.id)"
    />
  </div>
</template>

<style scoped>
.toasts {
  position: fixed;
  top: 16px;
  right: 16px;
  z-index: 200;
  display: flex;
  flex-direction: column;
  gap: 8px;
  width: min(380px, calc(100vw - 32px));
}
.toasts__item {
  box-shadow: var(--shadow);
}
</style>
