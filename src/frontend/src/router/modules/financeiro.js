// Telas do módulo financeiro. Por enquanto apontam para a tela "em construção".
const EmConstrucao = () => import("@/views/EmConstrucao.vue");

export default [
  { path: "/index/lancamentos", name: "fluxo.lancamentos", component: EmConstrucao, meta: { title: "Lançamentos" } },
  { path: "/index/contas", name: "fluxo.contas", component: EmConstrucao, meta: { title: "Contas" } },
  { path: "/index/categorias", name: "fluxo.categorias", component: EmConstrucao, meta: { title: "Categorias" } },
];
