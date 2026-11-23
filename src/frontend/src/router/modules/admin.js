// Administração: só para quem tem `manage` (papel Admin).
const EmConstrucao = () => import("@/views/EmConstrucao.vue");

export default [
  {
    path: "/index/admin",
    name: "fluxo.admin",
    component: EmConstrucao,
    meta: { title: "Administração", ability: ["manage", "Admin"] },
  },
];
