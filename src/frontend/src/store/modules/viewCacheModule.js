/*
 * Cache de telas com expiração (versão enxuta do ViewCacheModule da referência).
 * Ex.: guardar filtros de uma listagem ao navegar para o cadastro e voltar.
 */
export const state = {
    cache: [],
};

export const getters = {
    getCacheTela: (state) => (nomeTela) => state.cache.find(x => x.name == nomeTela)?.content,
};

export const mutations = {
    MERGE(state, telaObj) {
        const index = state.cache.findIndex(x => x.name == telaObj.name);
        if (index >= 0) state.cache.splice(index, 1, telaObj);
        else state.cache.push(telaObj);
    },
    CLEAR(state, name) {
        state.cache = state.cache.filter(x => x.name != name);
    },
    CLEAR_EXPIRED(state) {
        const now = Date.now();
        state.cache = state.cache.filter(x => !x.expiresAt || x.expiresAt > now);
    },
    CLEAR_ALL(state) {
        state.cache = [];
    },
};

export const actions = {
    setCacheTela({ commit }, { nomeTela, cacheContent, expirationInSeconds = 30 * 60 }) {
        commit("MERGE", {
            name: nomeTela,
            content: cacheContent,
            expiresAt: expirationInSeconds ? Date.now() + expirationInSeconds * 1000 : null,
        });
    },
    clearCacheTela({ commit }, nomeTela) {
        commit("CLEAR", nomeTela);
    },
    checkAndClearExpiredCaches({ commit }) {
        commit("CLEAR_EXPIRED");
    },
    clearAll({ commit }) {
        commit("CLEAR_ALL");
    },
};

export const namespaced = true;
