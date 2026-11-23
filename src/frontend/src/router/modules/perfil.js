export default [
  {
    path: "/index/perfil",
    name: "fluxo.perfil",
    component: () => import("@/views/perfil/MeuPerfil.vue"),
    meta: { title: "Meu perfil" },
  },
];
