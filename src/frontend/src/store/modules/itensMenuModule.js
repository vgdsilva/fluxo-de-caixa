/*
 * Itens de menu (barra do topo no desktop, barra inferior no celular).
 * Na referência vêm do backend por perfil; aqui são fixos e filtrados pelo CASL (`ability`).
 */
const defaultItems = [
    { key: "inicio", label: "Início", icon: "home", route: "/index/" },
    { key: "lancamentos", label: "Lançamentos", icon: "list", route: "/index/lancamentos" },
    { key: "contas", label: "Contas", icon: "wallet", route: "/index/contas" },
    { key: "categorias", label: "Categorias", icon: "tag", route: "/index/categorias" },
];

const adminItems = [
    { key: "administracao", label: "Administração", icon: "settings", route: "/index/admin", subject: "Admin" },
];

export const state = {
    itensMenu: [],
    itensAdmin: [],
};

export const getters = {
    findByPath: (state) => (path) =>
        [...state.itensMenu, ...state.itensAdmin].find(i => i.route === path) ?? null,
};

export const mutations = {
    SET(state, { itensMenu, itensAdmin }) {
        state.itensMenu = itensMenu;
        state.itensAdmin = itensAdmin;
    },
    CLEAR(state) {
        state.itensMenu = [];
        state.itensAdmin = [];
    },
};

export const actions = {
    load({ commit }, ability) {
        commit("SET", {
            itensMenu: defaultItems,
            itensAdmin: adminItems.filter(i => ability.can("manage", i.subject)),
        });
    },
    clear({ commit }) {
        commit("CLEAR");
    },
};

export const namespaced = true;
