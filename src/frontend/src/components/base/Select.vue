<script>
import FormField from "./FormField.vue";
import Icon from "./Icon.vue";

let seq = 0;

// options: [{ label, value }]. Valor vazio ("") mostra o placeholder.
export default {
  name: "Select",
  components: { FormField, Icon },
  inheritAttrs: false,
  props: {
    modelValue: { type: [String, Number, Boolean, null], default: "" },
    options: { type: Array, default: () => [] },
    label: { type: String, default: "" },
    placeholder: { type: String, default: "Selecione" },
    required: { type: Boolean, default: false },
    disabled: { type: Boolean, default: false },
    error: { type: String, default: "" },
    hint: { type: String, default: "" },
  },
  emits: ["update:modelValue", "change"],
  data() {
    return { uid: `select-${++seq}` };
  },
  methods: {
    onChange(event) {
      const option = this.options[event.target.selectedIndex - 1];
      const value = option ? option.value : "";
      this.$emit("update:modelValue", value);
      this.$emit("change", value);
    },
  },
};
</script>

<template>
  <FormField :id="uid" :label="label" :required="required" :error="error" :hint="hint" :class="$attrs.class">
    <div class="field__control select" :class="{ 'field__control--disabled': disabled }">
      <select
        :id="uid"
        class="field__input select__native"
        :disabled="disabled"
        :required="required"
        :aria-invalid="error ? 'true' : undefined"
        @change="onChange"
      >
        <option value="" :selected="modelValue === '' || modelValue === null">{{ placeholder }}</option>
        <option
          v-for="(option, index) in options"
          :key="index"
          :value="index"
          :selected="option.value === modelValue"
        >
          {{ option.label }}
        </option>
      </select>
      <Icon name="chevron-down" :size="16" class="select__chevron" />
    </div>
  </FormField>
</template>

<style scoped>
.select {
  position: relative;
}
.select__native {
  appearance: none;
  padding-right: 32px;
  cursor: pointer;
}
.select__chevron {
  position: absolute;
  right: 10px;
  pointer-events: none;
  color: var(--text-muted);
}
</style>
