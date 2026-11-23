<script>
import { mapState } from "vuex";
import { ToastHost, BlockScreen, InlineSpinner } from "@/index.js";

export default {
  components: { ToastHost, BlockScreen, InlineSpinner },
  computed: {
    ...mapState(["systemLoading"]),
  },
  created() {
    this.$store.dispatch("bootstrap");
  },
};
</script>

<template>
  <div v-if="systemLoading" class="app-loading">
    <InlineSpinner :size="28" />
  </div>
  <RouterView v-else />
  <ToastHost />
  <BlockScreen />
</template>

<style scoped>
.app-loading {
  display: flex;
  align-items: center;
  justify-content: center;
  height: 100%;
  color: var(--brand-700);
}
</style>
