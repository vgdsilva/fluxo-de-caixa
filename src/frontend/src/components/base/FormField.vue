<script>
// Rótulo, mensagem de erro e dica de um campo. Usado por Input, InputPassword e Select.
export default {
  name: "FormField",
  props: {
    id: { type: String, required: true },
    label: { type: String, default: "" },
    required: { type: Boolean, default: false },
    error: { type: String, default: "" },
    hint: { type: String, default: "" },
  },
};
</script>

<template>
  <div class="field" :class="{ 'field--error': error }">
    <label v-if="label" class="field__label" :for="id">
      {{ label }}<span v-if="required" class="field__required" aria-hidden="true"> *</span>
    </label>
    <slot />
    <p v-if="error" :id="`${id}-error`" class="field__message field__message--error" role="alert">{{ error }}</p>
    <p v-else-if="hint" :id="`${id}-hint`" class="field__message">{{ hint }}</p>
  </div>
</template>

<style>
.field {
  display: flex;
  flex-direction: column;
  gap: 6px;
  min-width: 0;
}
.field__label {
  font-size: 13px;
  font-weight: 600;
}
.field__required {
  color: var(--danger);
}
.field__message {
  margin: 0;
  font-size: 12px;
  color: var(--text-muted);
}
.field__message--error {
  color: var(--danger);
}
.field__control {
  display: flex;
  align-items: center;
  height: 40px;
  border: 1px solid var(--border);
  border-radius: var(--radius-sm);
  background: var(--surface);
  transition: border-color 0.15s, box-shadow 0.15s;
}
.field__control:focus-within {
  border-color: var(--brand-500);
  box-shadow: var(--focus-ring);
}
.field--error .field__control {
  border-color: var(--danger);
}
.field__control--disabled {
  background: var(--surface-muted);
  color: var(--text-muted);
}
.field__input {
  flex: 1;
  min-width: 0;
  height: 100%;
  padding: 0 12px;
  border: 0;
  background: transparent;
  font: inherit;
  color: inherit;
  outline: none;
}
.field__input::placeholder {
  color: #9b9b9b;
}
.field__suffix {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  height: 100%;
  padding: 0 10px;
  border: 0;
  background: transparent;
  color: var(--text-muted);
  cursor: pointer;
  border-radius: var(--radius-sm);
}
.field__suffix:hover {
  color: var(--text);
}
</style>
