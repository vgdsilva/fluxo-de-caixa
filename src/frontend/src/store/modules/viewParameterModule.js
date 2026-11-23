// Parâmetros passados de uma tela para outra (ex.: id do registro ao abrir uma edição).
export const state = {
    parameters: {},
};

export const getters = {
    get: (state) => (key) => state.parameters[key],
};

export const mutations = {
    SET(state, { key, value }) {
        state.parameters = { ...state.parameters, [key]: value };
    },
    REMOVE(state, key) {
        const copy = { ...state.parameters };
        delete copy[key];
        state.parameters = copy;
    },
    CLEAR(state) {
        state.parameters = {};
    },
};

export const actions = {
    set({ commit }, payload) {
        commit("SET", payload);
    },
    remove({ commit }, key) {
        commit("REMOVE", key);
    },
};

export const namespaced = true;
