<script>
import Empty from "./Empty.vue";
import InlineSpinner from "./InlineSpinner.vue";

/*
 * Tabela simples.
 * columns: [{ field, label, align?: "left"|"right"|"center", width?, format?: (valor, linha) => texto }]
 * Slot por coluna: <template #cell-<field>="{ row, value }">...</template>
 */
export default {
  name: "Table",
  components: { Empty, InlineSpinner },
  props: {
    columns: { type: Array, required: true },
    rows: { type: Array, default: () => [] },
    rowKey: { type: String, default: "id" },
    loading: { type: Boolean, default: false },
    emptyConfig: { type: Object, default: () => ({}) },
    clickable: { type: Boolean, default: false },
  },
  emits: ["row-click"],
  methods: {
    cellValue(row, column) {
      const value = column.field.split(".").reduce((acc, key) => acc?.[key], row);
      return column.format ? column.format(value, row) : value;
    },
  },
};
</script>

<template>
  <div class="table-wrap">
    <table class="table">
      <thead>
        <tr>
          <th
            v-for="column in columns"
            :key="column.field"
            scope="col"
            :style="{ width: column.width, textAlign: column.align || 'left' }"
          >
            {{ column.label }}
          </th>
        </tr>
      </thead>
      <tbody v-if="!loading && rows.length">
        <tr
          v-for="row in rows"
          :key="row[rowKey]"
          :class="{ 'table__row--clickable': clickable }"
          :tabindex="clickable ? 0 : undefined"
          @click="clickable && $emit('row-click', row)"
          @keydown.enter="clickable && $emit('row-click', row)"
        >
          <td v-for="column in columns" :key="column.field" :style="{ textAlign: column.align || 'left' }">
            <slot :name="`cell-${column.field}`" :row="row" :value="cellValue(row, column)">
              {{ cellValue(row, column) }}
            </slot>
          </td>
        </tr>
      </tbody>
    </table>
    <div v-if="loading" class="table__loading"><InlineSpinner /></div>
    <Empty v-else-if="!rows.length" :config="emptyConfig" />
  </div>
</template>

<style scoped>
.table-wrap {
  width: 100%;
  overflow-x: auto;
}
.table {
  width: 100%;
  border-collapse: collapse;
}
th,
td {
  padding: 10px 16px;
  border-bottom: 1px solid var(--border);
  white-space: nowrap;
}
th {
  font-size: 12px;
  font-weight: 600;
  color: var(--text-muted);
  background: var(--surface);
}
tbody tr:last-child td {
  border-bottom: 0;
}
.table__row--clickable {
  cursor: pointer;
}
.table__row--clickable:hover {
  background: var(--surface-muted);
}
.table__loading {
  display: flex;
  justify-content: center;
  padding: 32px;
  color: var(--text-muted);
}
</style>
