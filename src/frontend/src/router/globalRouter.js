let routerInstance = null

export function setGlobalRouter(router) {
  routerInstance = router
}

export function getGlobalRouter() {
  return routerInstance
}
