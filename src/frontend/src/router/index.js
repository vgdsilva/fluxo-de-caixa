import { createRouter, createWebHistory } from "vue-router";
import store from "@/store";
import { Auth, Main } from "@/index.js";
import AuthLogin from "@/views/auth/Login.vue";
import AuthLogout from "@/views/auth/AuthLogout.vue";
import AuthSession from "@/views/auth/AuthSession.vue";
import Home from "@/views/Home.vue";
import { hasSession } from "@/system/authSessionHelper.js";
import ability from "@/services/ability.js";
import { getApplicationName, getApplicationTitle } from "@/system/applicationConfigHelper.js";
import { fullModulesRoutes } from "./modules/index.js";

/*
 * Rotas (mesma organização da referência):
 *  /index/...  layout Main (barra do topo + conteúdo), exige login
 *  /auth/...   layout Auth (tela de login), público
 * As rotas de cada módulo ficam em router/modules/.
 */
const routes = [
  {
    path: "/index",
    component: Main,
    children: [
      { path: "", name: getApplicationName(), component: Home, meta: { title: "Início" } },
      ...fullModulesRoutes,
    ],
  },
  {
    path: "/auth",
    component: Auth,
    redirect: { name: "auth.login" },
    children: [
      { path: "login", name: "auth.login", component: AuthLogin, meta: { public: true } },
      { path: "logout", name: "auth.logout", component: AuthLogout, meta: { public: true } },
      { path: "session", name: "auth.session", component: AuthSession, meta: { public: true } },
    ],
  },
  { path: "/", redirect: "/index/" },
  { path: "/:pathMatch(.*)*", redirect: "/index/" },
];

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes,
  scrollBehavior(to, from, savedPosition) {
    if (savedPosition) return savedPosition;
    if (to.hash) return { el: to.hash, behavior: "smooth" };
    return { top: 0, left: 0 };
  },
});

router.beforeEach(async (to) => {
  const logged = hasSession();

  if (to.meta.public) {
    // Já logado e abrindo o login: vai direto para o início.
    if (to.name === "auth.login" && logged) return "/index/";
    return true;
  }

  if (!logged) {
    return { name: "auth.login", query: to.fullPath !== "/index/" ? { redirect: to.fullPath } : {} };
  }

  // Recarregou a página com sessão guardada: monta o estado antes de entrar.
  if (!store.state.systemLoaded) {
    const ok = await store.dispatch("loadSystem");
    if (!ok) return { name: "auth.login" };
  }

  // meta.ability = ["manage", "Admin"], por exemplo.
  if (to.meta.ability && !ability.can(...to.meta.ability)) {
    return "/index/";
  }

  return true;
});

router.afterEach((to) => {
  const title = to.meta.title;
  document.title = title ? `${title} · ${getApplicationTitle()}` : getApplicationTitle();
  store.commit("main/SET_PAGE_TITLE", title ?? "");
});

export default router;
