<script>
/*
 * Painel lateral do login (redesign): fundo verde com prévias do app em cards brancos.
 * É só ilustração: os números são fixos e não vêm do backend.
 */
export default {
  name: "LoginShowcase",
  data() {
    return {
      kpis: [
        { label: "Saldo", value: "R$ 18.432" },
        { label: "Entradas", value: "R$ 8.750" },
        { label: "Saídas", value: "R$ 6.810" },
      ],
      barras: [
        { entrada: 62, saida: 48 },
        { entrada: 70, saida: 52 },
        { entrada: 58, saida: 44 },
        { entrada: 80, saida: 60 },
        { entrada: 74, saida: 50 },
        { entrada: 88, saida: 64 },
      ],
      categorias: [
        { nome: "Moradia", pct: 34 },
        { nome: "Alimentação", pct: 25 },
        { nome: "Transporte", pct: 12 },
        { nome: "Lazer", pct: 10 },
      ],
      lancamentos: [
        { nome: "Salário", valor: "+ R$ 6.500,00", entrada: true },
        { nome: "Aluguel", valor: "− R$ 2.100,00", entrada: false },
        { nome: "Mercado", valor: "− R$ 412,90", entrada: false },
        { nome: "Freela", valor: "+ R$ 1.250,00", entrada: true },
        { nome: "Netflix", valor: "− R$ 44,90", entrada: false },
      ],
    };
  },
};
</script>

<template>
  <div class="showcase" aria-hidden="true">
    <div class="showcase__col">
      <div class="sc-card">
        <p class="sc-card__title">Visão do mês</p>
        <div class="sc-kpis">
          <div v-for="kpi in kpis" :key="kpi.label" class="sc-kpi">
            <span class="sc-kpi__label">{{ kpi.label }}</span>
            <span class="sc-kpi__value">{{ kpi.value }}</span>
          </div>
        </div>
        <div class="sc-bars">
          <div v-for="(b, i) in barras" :key="i" class="sc-bars__group">
            <span class="sc-bars__bar sc-bars__bar--in" :style="{ height: `${b.entrada}%` }" />
            <span class="sc-bars__bar sc-bars__bar--out" :style="{ height: `${b.saida}%` }" />
          </div>
        </div>
      </div>

      <div class="sc-card">
        <p class="sc-card__title">Gastos por categoria</p>
        <ul class="sc-list">
          <li v-for="c in categorias" :key="c.nome" class="sc-cat">
            <div class="sc-cat__row">
              <span>{{ c.nome }}</span>
              <strong>{{ c.pct }}%</strong>
            </div>
            <div class="sc-cat__track"><span :style="{ width: `${c.pct * 2.4}%` }" /></div>
          </li>
        </ul>
      </div>
    </div>

    <div class="showcase__col showcase__col--offset">
      <div class="sc-card">
        <p class="sc-card__title">Últimos lançamentos</p>
        <ul class="sc-list">
          <li v-for="l in lancamentos" :key="l.nome" class="sc-tx">
            <span class="sc-tx__dot" :class="l.entrada ? 'sc-tx__dot--in' : 'sc-tx__dot--out'" />
            <span class="sc-tx__name">{{ l.nome }}</span>
            <span class="sc-tx__value" :class="{ 'sc-tx__value--in': l.entrada }">{{ l.valor }}</span>
          </li>
        </ul>
      </div>

      <div class="sc-card">
        <p class="sc-card__title"><span class="sc-ai__badge">IA</span> Assistente IA</p>
        <div class="sc-chat">
          <p class="sc-chat__msg sc-chat__msg--me">Gastei 80 no mercado hoje</p>
          <p class="sc-chat__msg">
            Lancei <strong>R$ 80,00</strong> em Alimentação &gt; Mercado, no crédito do Nubank.
            Você usou <strong>79%</strong> do orçamento de mercado.
          </p>
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped>
.showcase {
  display: grid;
  grid-template-columns: repeat(2, minmax(0, 1fr));
  gap: 16px;
  width: 100%;
  max-width: 620px;
}
.showcase__col {
  display: flex;
  flex-direction: column;
  gap: 16px;
}
.showcase__col--offset {
  margin-top: 40px;
}
.sc-card {
  padding: 16px;
  background: #fff;
  border-radius: 18px;
  box-shadow: 0 12px 32px rgb(0 0 0 / 0.12);
  color: var(--text);
  font-size: 12px;
}
.sc-card__title {
  display: flex;
  align-items: center;
  gap: 8px;
  margin: 0 0 12px;
  font-size: 13px;
  font-weight: 700;
}
.sc-kpis {
  display: grid;
  grid-template-columns: repeat(3, minmax(0, 1fr));
  gap: 8px;
}
.sc-kpi {
  display: flex;
  flex-direction: column;
  gap: 2px;
  padding: 8px;
  background: var(--surface-muted);
  border-radius: 10px;
}
.sc-kpi__label {
  font-size: 10px;
  color: var(--text-muted);
}
.sc-kpi__value {
  font-size: 12px;
  font-weight: 700;
  white-space: nowrap;
}
.sc-bars {
  display: flex;
  align-items: flex-end;
  justify-content: space-between;
  gap: 8px;
  height: 90px;
  margin-top: 12px;
}
.sc-bars__group {
  display: flex;
  flex: 1;
  align-items: flex-end;
  gap: 3px;
  height: 100%;
}
.sc-bars__bar {
  flex: 1;
  border-radius: 4px 4px 0 0;
}
.sc-bars__bar--in {
  background: var(--brand-500);
}
.sc-bars__bar--out {
  background: var(--brand-300);
}
.sc-list {
  display: flex;
  flex-direction: column;
  gap: 10px;
  margin: 0;
  padding: 0;
  list-style: none;
}
.sc-cat__row {
  display: flex;
  justify-content: space-between;
  margin-bottom: 4px;
}
.sc-cat__track {
  height: 6px;
  background: var(--brand-100);
  border-radius: 999px;
  overflow: hidden;
}
.sc-cat__track span {
  display: block;
  height: 100%;
  background: var(--brand-500);
  border-radius: 999px;
}
.sc-tx {
  display: flex;
  align-items: center;
  gap: 8px;
}
.sc-tx__dot {
  width: 8px;
  height: 8px;
  border-radius: 50%;
}
.sc-tx__dot--in {
  background: var(--brand-500);
}
.sc-tx__dot--out {
  background: #e0a3a3;
}
.sc-tx__name {
  flex: 1;
}
.sc-tx__value {
  font-weight: 700;
  color: var(--text);
  white-space: nowrap;
}
.sc-tx__value--in {
  color: var(--brand-600);
}
.sc-ai__badge {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  width: 22px;
  height: 22px;
  border-radius: 7px;
  background: var(--brand-500);
  color: #fff;
  font-size: 10px;
}
.sc-chat {
  display: flex;
  flex-direction: column;
  gap: 8px;
}
.sc-chat__msg {
  margin: 0;
  padding: 8px 10px;
  max-width: 90%;
  background: var(--surface-muted);
  border-radius: 12px 12px 12px 4px;
  line-height: 1.4;
}
.sc-chat__msg--me {
  align-self: flex-end;
  background: var(--brand-600);
  color: #fff;
  border-radius: 12px 12px 4px 12px;
}
</style>
