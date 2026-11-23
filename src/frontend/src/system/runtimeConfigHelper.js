// Endereço do backend. Mesmo papel do runtimeConfigHelper da referência,
// sem o /frontend-settings (aqui não há provedores de login alternativos).

export const BACKEND_ENDPOINT = () => getBackendBaseEndpoint() + "/api";

function getBackendBaseEndpoint() {
  let endpoint = import.meta.env.VITE_SVC_BACKEND;
  if (!endpoint || endpoint.trim() == "") {
    endpoint = window.location.origin;
  }
  return endpoint.replace(/\/+$/, "");
}

export const IS_DEV = () => (import.meta.env.VITE_VERSION || "dev") == "dev"
