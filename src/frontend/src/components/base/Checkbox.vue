<script>
let seq = 0;

export default {
  name: "Checkbox",
  props: {
    modelValue: { type: Boolean, default: false },
    label: { type: String, default: "" },
    disabled: { type: Boolean, default: false },
  },
  emits: ["update:modelValue"],
  data() {
    return { uid: `checkbox-${++seq}` };
  },
};
</script>

<template>
  <label class="checkbox" :for="uid">
    <input
      :id="uid"
      type="checkbox"
      :checked="modelValue"
      :disabled="disabled"
      @change="$emit('update:modelValue', $event.target.checked)"
    />
    <span>{{ label }}<slot /></span>
  </label>
</template>

<style scoped>
.checkbox {
  display: inline-flex;
  align-items: center;
  gap: 8px;
  cursor: pointer;
}
.checkbox input {
  width: 16px;
  height: 16px;
  accent-color: var(--brand-500);
}
</style>
