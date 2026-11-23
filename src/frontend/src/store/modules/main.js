// Estado do layout principal: bloqueio de tela, toasts e título da página.
export const state = {
    block: { active: false, message: "" },
    toasts: [],
    pageTitle: "",
};

export const mutations = {
    SET_BLOCK(state, block) {
        state.block = block;
    },
    ADD_TOAST(state, toast) {
        state.toasts.push(toast);
    },
    REMOVE_TOAST(state, id) {
        state.toasts = state.toasts.filter(t => t.id !== id);
    },
    SET_PAGE_TITLE(state, title) {
        state.pageTitle = title;
    },
};

export const namespaced = true;
