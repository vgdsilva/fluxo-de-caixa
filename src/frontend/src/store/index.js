import { createStore } from "vuex";
import * as MainModule from "@/store/modules/main.js";
import * as ItensMenuModule from "@/store/modules/itensMenuModule.js";
import * as ViewCacheModule from "@/store/modules/viewCacheModule.js";
import * as ViewParametersModule from "@/store/modules/viewParameterModule.js";
import SessaoService from "@/services/acesso/SessaoService.js";
import ability, { defineRulesFor } from "@/services/ability.js";
import { UsuarioFactory } from "@/entities/acesso/Usuario.js";
import {
  getAuthSession,
  getSessionUser,
  saveAuthSession,
  clearAuthSession,
  scheduleRefresh,
  hasSession,
} from "@/system/authSessionHelper.js";
import { initializeCacheCleaner, endCacheCleaner, clearAllSessionData } from "@/store/helper.js";
import { getGlobalRouter } from "@/router/globalRouter.js";

let loadSystemInFlight = null;

/*
 * Store principal (Vuex com módulos namespaced, como na referência).
 * Ações de sistema: bootstrap, login, loadSystem, logout.
 */
const store = createStore({
  modules: {
    main: MainModule,
    ItensMenuModule,
    ViewCacheModule,
    ViewParametersModule,
  },
  state: {
    systemLoading: false,
    systemLoaded: false,
    user: null,
  },
  getters: {
    usuario: (state) => (state.user ? UsuarioFactory.create(state.user) : null),
    isLogged: (state) => !!state.user,
  },
  mutations: {
    setSystemLoading(state, value) {
      state.systemLoading = value;
    },
    setSystemLoaded(state, value) {
      state.systemLoaded = value;
    },
    SET_USER(state, user) {
      state.user = user;
    },
  },
  actions: {
    /** Chamado uma vez pelo App.vue. Se já existe sessão guardada, carrega o sistema. */
    async bootstrap({ dispatch, commit, state }) {
      if (hasSession() && !state.systemLoaded) {
        await dispatch("loadSystem");
      }
      commit("setSystemLoading", false);
    },

    /** Recebe a resposta do POST /auth/login. */
    async loginSuccessful({ dispatch }, authResponse) {
      saveAuthSession(authResponse);
      await dispatch("loadSystem");
    },

    /**
     * Monta o estado do usuário logado: dados, permissões e menus.
     * Bootstrap e guarda de rota podem chamar ao mesmo tempo: a segunda chamada reaproveita a primeira.
     */
    loadSystem({ dispatch }) {
      if (!loadSystemInFlight) {
        loadSystemInFlight = dispatch("doLoadSystem").finally(() => {
          loadSystemInFlight = null;
        });
      }
      return loadSystemInFlight;
    },

    async doLoadSystem({ commit, dispatch }) {
      const session = getAuthSession();
      if (!session) return false;

      commit("SET_USER", getSessionUser());
      dispatch("updateAbility");
      scheduleRefresh();
      initializeCacheCleaner(store);

      // Atualiza os dados do usuário com o backend (nome pode ter mudado em outro aparelho).
      try {
        const response = await SessaoService.sMe();
        if (response?.data) commit("SET_USER", { ...getSessionUser(), ...response.data });
        dispatch("updateAbility");
      } catch (error) {
        if (error?.response?.status === 401) return false;
        console.warn("Não foi possível atualizar os dados do usuário", error);
      }

      commit("setSystemLoaded", true);
      return true;
    },

    updateAbility({ state, dispatch }) {
      ability.update(defineRulesFor(state.user));
      dispatch("ItensMenuModule/load", ability);
    },

    async logout({ commit }, { reason } = {}) {
      const session = getAuthSession();
      if (session?.refreshToken) {
        // Revoga a sessão no banco; se falhar, segue com a saída local.
        await SessaoService.sLogout(session.refreshToken).catch(() => {});
      }
      // Zera o usuário antes de limpar a sessão: assim o aviso de "sessão encerrada" (main.js) não dispara.
      commit("SET_USER", null);
      clearAuthSession();
      endCacheCleaner();
      clearAllSessionData(store);
      commit("setSystemLoaded", false);
      ability.update([]);

      const router = getGlobalRouter();
      if (router) router.replace({ path: "/auth/logout", query: reason ? { reason } : {} });
    },

    /** Sessão encerrada por outro motivo (outra aba saiu, refresh negado). */
    sessionEnded({ commit }) {
      clearAllSessionData(store);
      commit("SET_USER", null);
      commit("setSystemLoaded", false);
      ability.update([]);
    },
  },
});

export default store;
