<script>
import FormField from "./FormField.vue";

let seq = 0;

export default {
  name: "Input",
  components: { FormField },
  inheritAttrs: false,
  props: {
    modelValue: { type: [String, Number], default: "" },
    label: { type: String, default: "" },
    placeholder: { type: String, default: "" },
    type: { type: String, default: "text" },
    required: { type: Boolean, default: false },
    disabled: { type: Boolean, default: false },
    readonly: { type: Boolean, default: false },
    error: { type: String, default: "" },
    hint: { type: String, default: "" },
    autocomplete: { type: String, default: undefined },
    id: { type: String, default: "" },
  },
  emits: ["update:modelValue", "enter", "blur"],
  data() {
    return { uid: this.id || `input-${++seq}` };
  },
  methods: {
    focus() {
      this.$refs.input?.focus();
    },
  },
};
</script>

<template>
  <FormField :id="uid" :label="label" :required="required" :error="error" :hint="hint" :class="$attrs.class">
    <div class="field__control" :class="{ 'field__control--disabled': disabled || readonly }">
      <input
        :id="uid"
        ref="input"
        class="field__input"
        :type="type"
        :value="modelValue"
        :placeholder="placeholder"
        :disabled="disabled"
        :readonly="readonly"
        :required="required"
        :autocomplete="autocomplete"
        :aria-invalid="error ? 'true' : undefined"
        :aria-describedby="error ? `${uid}-error` : hint ? `${uid}-hint` : undefined"
        @input="$emit('update:modelValue', $event.target.value)"
        @keydown.enter="$emit('enter', $event)"
        @blur="$emit('blur', $event)"
      />
      <slot name="suffix" />
    </div>
  </FormField>
</template>
