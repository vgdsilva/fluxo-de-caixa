import moment from "moment";

/*
 * Utilitários gerais (mesmo papel do ServiceUtils da referência).
 * Formatação, mensagens (toast), bloqueio de tela e checagens simples.
 */

const currencyFormatter = new Intl.NumberFormat("pt-BR", { style: "currency", currency: "BRL" });
const numberFormatter = new Intl.NumberFormat("pt-BR", { minimumFractionDigits: 2, maximumFractionDigits: 2 });

let toastSeq = 0;

export const ServiceUtils = {
    isNull(value) {
        return value === null || value === undefined;
    },
    isNotNull(value) {
        return !ServiceUtils.isNull(value);
    },
    isEmpty(value) {
        return ServiceUtils.isNull(value) || value === "" || (Array.isArray(value) && value.length === 0);
    },
    isArray(value) {
        return Array.isArray(value);
    },
    isDate(value) {
        return value instanceof Date || moment.isMoment(value);
    },

    /** Valor em reais (número). Ex.: 1063.16 -> "R$ 1.063,16" */
    formatMoney(value) {
        if (ServiceUtils.isNull(value) || value === "") return "";
        return currencyFormatter.format(Number(value));
    },
    /** Valor em centavos inteiros. Ex.: 106316 -> "R$ 1.063,16" */
    formatCents(cents) {
        if (ServiceUtils.isNull(cents)) return "";
        return currencyFormatter.format(Number(cents) / 100);
    },
    formatNumber(value) {
        if (ServiceUtils.isNull(value) || value === "") return "";
        return numberFormatter.format(Number(value));
    },
    /** Datas de negócio chegam como 'YYYY-MM-DD'; instantes como ISO. */
    formatDate(value, format = "DD/MM/YYYY") {
        if (!value) return "";
        return moment(value).format(format);
    },
    formatDateTime(value) {
        return ServiceUtils.formatDate(value, "DD/MM/YYYY, HH:mm");
    },
    toIsoDate(value) {
        return value ? moment(value).format("YYYY-MM-DD") : null;
    },

    /** type: success | error | warning | info */
    showToast(store, type, title, text = "", timeout = 5000) {
        const id = ++toastSeq;
        store.commit("main/ADD_TOAST", { id, type, title, text });
        if (timeout > 0) setTimeout(() => store.commit("main/REMOVE_TOAST", id), timeout);
        return id;
    },
    showError(store, error, title = "Não foi possível concluir a operação") {
        ServiceUtils.showToast(store, "error", title, ServiceUtils.getErrorMessage(error), 8000);
    },
    /** Extrai a mensagem de um erro do axios (ProblemDetails do .NET ou { message }). */
    getErrorMessage(error) {
        const data = error?.response?.data;
        if (typeof data === "string" && data) return data;
        if (data?.detail) return data.detail;
        if (data?.title && !data?.errors) return data.title;
        if (data?.errors) {
            const first = Object.values(data.errors).flat()[0];
            if (first) return first;
        }
        if (data?.message) return data.message;
        if (!error?.response) return "Sem conexão com o servidor.";
        return error?.message ?? "Erro inesperado.";
    },

    bloquearTela(store, mensagem = "Carregando...") {
        store.commit("main/SET_BLOCK", { active: true, message: mensagem });
    },
    desbloquearTela(store) {
        store.commit("main/SET_BLOCK", { active: false, message: "" });
    },
};
