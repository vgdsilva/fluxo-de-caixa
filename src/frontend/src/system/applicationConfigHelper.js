import appConfig from "./appConfig.js";

export const getApplicationName = () => appConfig.general.applicationName

export const getApplicationTitle = () => appConfig.general.applicationTitle

export const getApplicationVersion = () => import.meta.env.VITE_APP_VERSION || appConfig.general.version

export const getApplicationNameUpperCase = () => getApplicationName().toUpperCase()
