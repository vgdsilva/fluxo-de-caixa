import apiClient from "@/services/apiClientProvider";

/*
 * Helpers de requisição (mesmo papel do requestHelper da referência).
 * Services nunca chamam axios direto: usam estes helpers.
 *
 * Convenção REST do WebAPI (relativePath termina com "/", ex.: "/accounts/"):
 *   listar   GET    {relativePath}?page=1&pageSize=50&sort=campo,-outro&<filtros>
 *   buscar   GET    {relativePath}{id}
 *   criar    POST   {relativePath}
 *   editar   PUT    {relativePath}{id}
 *   remover  DELETE {relativePath}{id}
 *
 * Diferença em relação à referência: lá a edição é POST em `edit`; aqui seguimos o padrão REST do .NET.
 */

function buildParams({ filter, sort, page, pageSize } = {}) {
    const params = {};
    if (filter) {
        Object.entries(filter).forEach(([key, value]) => {
            if (value === undefined || value === null || value === "") return;
            if (value instanceof Date) value = value.toISOString().slice(0, 10);
            else if (typeof value === "object" && "value" in value) value = value.value;
            params[key] = value;
        });
    }
    if (sort) {
        params.sort = Array.isArray(sort)
            ? sort.map(s => (s.desc ? "-" : "") + s.field).join(",")
            : sort;
    }
    if (page) params.page = page;
    if (pageSize) params.pageSize = pageSize;
    return params;
}

function serializePayload(obj) {
    if (obj === null || obj === undefined) return obj;
    if (Array.isArray(obj)) return obj.map(serializePayload);
    if (obj instanceof Date) return obj.toISOString();
    if (typeof obj !== "object") return obj;

    const serialized = {};
    Object.keys(obj).forEach(key => {
        // Campos internos de tela ($ e _) não vão para o backend.
        if (key.startsWith("$") || key.startsWith("_")) return;
        const value = obj[key];
        if (typeof value === "function") return;
        serialized[key] = serializePayload(value);
    });
    return serialized;
}

export const RequestFunctionsHelper = {
    getDefaultRequestHeaders() {
        return {
            "X-Requested-With": "fluxo",
        };
    },
    serializePayload,
    buildParams,
};

function config(extra = {}) {
    return { ...extra, headers: { ...RequestFunctionsHelper.getDefaultRequestHeaders(), ...(extra.headers || {}) } };
}

export const doFindRangeRequest = ({ relativePath, filter, sort, page, pageSize }) => {
    return apiClient().then(x => x.get(relativePath, config({ params: buildParams({ filter, sort, page, pageSize }) })));
};

export const doGetByIdRequest = ({ relativePath, id }) => {
    return apiClient().then(x => x.get(`${relativePath}${id}`, config()));
};

export const doCreateRequest = ({ relativePath, obj }) => {
    return apiClient().then(x => x.post(relativePath, serializePayload(obj), config()));
};

export const doEditRequest = ({ relativePath, obj }) => {
    return apiClient().then(x => x.put(`${relativePath}${obj.id}`, serializePayload(obj), config()));
};

export const doRemoveRequest = ({ relativePath, id }) => {
    return apiClient().then(x => x.delete(`${relativePath}${id}`, config()));
};

export const doGetRequest = ({ path, params, skipAuth }) => {
    return apiClient().then(x => x.get(path, config({ params, skipAuth })));
};

export const doPostRequest = ({ path, payload, skipAuth }) => {
    return apiClient().then(x => x.post(path, payload ? serializePayload(payload) : {}, config({ skipAuth })));
};

export const doPutRequest = ({ path, payload }) => {
    return apiClient().then(x => x.put(path, payload ? serializePayload(payload) : {}, config()));
};
