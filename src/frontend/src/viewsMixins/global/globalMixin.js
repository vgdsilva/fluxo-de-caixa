import { ServiceUtils } from "@/serviceUtils.js";

/*
 * Mixin global (registrado em main.js com app.mixin), como o globalMixin da referência.
 * Só helpers de apresentação usados em várias telas; nada de estado.
 */
export default {
    data() {
        return {
            emptyConfigDefault: { title: "Não foi encontrado registro para apresentação.", text: "" },
            emptyConfigNotFoundItenWithFilter: { title: "Não foi encontrado registro com os filtros informados.", text: "" },
        };
    },
    methods: {
        formatMoney(value) {
            return ServiceUtils.formatMoney(value);
        },
        formatCents(value) {
            return ServiceUtils.formatCents(value);
        },
        formatDate(value, format) {
            return ServiceUtils.formatDate(value, format);
        },
        formatDateTime(value) {
            return ServiceUtils.formatDateTime(value);
        },
        showToast(type, title, text) {
            return ServiceUtils.showToast(this.$store, type, title, text);
        },
        showError(error, title) {
            return ServiceUtils.showError(this.$store, error, title);
        },
    },
};
