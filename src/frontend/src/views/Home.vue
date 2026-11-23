<script>
import { mapGetters } from "vuex";
import moment from "moment";

// Início. Por enquanto só a estrutura (seletor de mês e resumo); os números entram com o backend.
export default {
  data() {
    return {
      mes: moment().startOf("month"),
    };
  },
  computed: {
    ...mapGetters(["usuario"]),
    mesLabel() {
      const label = new Intl.DateTimeFormat("pt-BR", { month: "long", year: "numeric" }).format(this.mes.toDate());
      return label.charAt(0).toUpperCase() + label.slice(1);
    },
    resumo() {
      return [
        { key: "entradas", label: "Entradas do mês", value: null, icon: "arrow-up", tone: "income" },
        { key: "saidas", label: "Saídas do mês", value: null, icon: "arrow-down", tone: "expense" },
        { key: "saldo", label: "Saldo previsto", value: null, icon: "wallet", tone: "" },
      ];
    },
  },
  methods: {
    mudarMes(delta) {
      this.mes = this.mes.clone().add(delta, "month");
    },
  },
};
</script>

<template>
  <div class="page">
    <div class="page__header">
      <div>
        <p class="text-muted home__hello">Olá, {{ usuario?.getFirstName() }}</p>
        <h1 class="page__title">Início</h1>
      </div>
      <div class="month" role="group" aria-label="Mês">
        <button type="button" class="month__btn" aria-label="Mês anterior" @click="mudarMes(-1)">
          <Icon name="chevron-left" />
        </button>
        <span class="month__label" aria-live="polite">{{ mesLabel }}</span>
        <button type="button" class="month__btn" aria-label="Próximo mês" @click="mudarMes(1)">
          <Icon name="chevron-right" />
        </button>
      </div>
    </div>

    <div class="grid grid--3">
      <Card v-for="item in resumo" :key="item.key">
        <div class="stat">
          <span class="stat__icon" :class="item.tone && `stat__icon--${item.tone}`"><Icon :name="item.icon" /></span>
          <div>
            <p class="stat__label">{{ item.label }}</p>
            <p class="stat__value">{{ item.value === null ? "—" : formatMoney(item.value) }}</p>
          </div>
        </div>
      </Card>
    </div>

    <Card class="home__recent" title="Últimos lançamentos">
      <Empty title="Nenhum lançamento ainda" text="Quando os lançamentos estiverem prontos, os últimos aparecem aqui." />
    </Card>
  </div>
</template>

<style scoped>
.home__hello {
  margin: 0 0 2px;
}
.home__recent {
  margin-top: 16px;
}
.month {
  display: inline-flex;
  align-items: center;
  gap: 4px;
  padding: 4px;
  background: var(--surface);
  border: 1px solid var(--border);
  border-radius: var(--radius);
}
.month__btn {
  display: inline-flex;
  padding: 6px;
  border: 0;
  border-radius: var(--radius-sm);
  background: transparent;
  cursor: pointer;
  color: var(--text-muted);
}
.month__btn:hover {
  background: var(--surface-muted);
  color: var(--text);
}
.month__label {
  min-width: 150px;
  text-align: center;
  font-weight: 600;
}
.stat {
  display: flex;
  align-items: center;
  gap: 14px;
}
.stat__icon {
  display: inline-flex;
  padding: 10px;
  border-radius: var(--radius);
  background: var(--surface-muted);
}
.stat__icon--income {
  background: var(--success-bg);
  color: var(--income);
}
.stat__icon--expense {
  background: var(--danger-bg);
  color: var(--expense);
}
.stat__label {
  margin: 0;
  font-size: 13px;
  color: var(--text-muted);
}
.stat__value {
  margin: 2px 0 0;
  font-size: 20px;
  font-weight: 700;
}
</style>
