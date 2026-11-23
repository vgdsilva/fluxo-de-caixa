<script>
import { mapState } from "vuex";
import InlineSpinner from "@/components/base/InlineSpinner.vue";

// Bloqueio de tela durante operações (ServiceUtils.bloquearTela / desbloquearTela).
export default {
  name: "BlockScreen",
  components: { InlineSpinner },
  computed: {
    ...mapState("main", ["block"]),
  },
};
</script>

<template>
  <div v-if="block.active" class="block" role="status" aria-live="assertive">
    <div class="block__box">
      <InlineSpinner :size="22" />
      <span>{{ block.message }}</span>
    </div>
  </div>
</template>

<style scoped>
.block {
  position: fixed;
  inset: 0;
  z-index: 300;
  display: flex;
  align-items: center;
  justify-content: center;
  background: rgb(255 255 255 / 0.6);
}
.block__box {
  display: flex;
  align-items: center;
  gap: 12px;
  padding: 14px 20px;
  background: var(--surface);
  border: 1px solid var(--border);
  border-radius: var(--radius);
  box-shadow: var(--shadow);
  font-weight: 600;
}
</style>
