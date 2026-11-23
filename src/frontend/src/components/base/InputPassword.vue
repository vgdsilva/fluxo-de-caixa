<script>
import Input from "./Input.vue";
import Icon from "./Icon.vue";

export default {
  name: "InputPassword",
  components: { Input, Icon },
  inheritAttrs: false,
  props: {
    modelValue: { type: String, default: "" },
  },
  emits: ["update:modelValue", "enter"],
  data() {
    return { visible: false };
  },
  methods: {
    focus() {
      this.$refs.input?.focus();
    },
  },
};
</script>

<template>
  <Input
    ref="input"
    v-bind="$attrs"
    :model-value="modelValue"
    :type="visible ? 'text' : 'password'"
    @update:model-value="$emit('update:modelValue', $event)"
    @enter="$emit('enter', $event)"
  >
    <template #suffix>
      <button
        type="button"
        class="field__suffix"
        :aria-label="visible ? 'Ocultar senha' : 'Mostrar senha'"
        :aria-pressed="visible"
        @click="visible = !visible"
      >
        <Icon :name="visible ? 'eye' : 'eye-off'" :size="16" />
      </button>
    </template>
  </Input>
</template>
