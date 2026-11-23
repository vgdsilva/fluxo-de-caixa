let cacheCleanerIntervalId = null;

export const initializeCacheCleaner = (store) => {
  endCacheCleaner();
  cacheCleanerIntervalId = setInterval(() => {
    store.dispatch("ViewCacheModule/checkAndClearExpiredCaches");
  }, 60 * 1000);
};

export const endCacheCleaner = () => {
  if (cacheCleanerIntervalId) clearInterval(cacheCleanerIntervalId);
  cacheCleanerIntervalId = null;
};

/** Limpa tudo que é da sessão do usuário (cache de telas, parâmetros, menus). */
export const clearAllSessionData = (store) => {
  if (!store) return;
  store.dispatch("ViewCacheModule/clearAll");
  store.commit("ViewParametersModule/CLEAR");
  store.dispatch("ItensMenuModule/clear");
  sessionStorage.clear();
};
