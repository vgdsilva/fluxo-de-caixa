/*
 * Junta as rotas de todos os módulos (na referência este arquivo é gerado).
 * Cada módulo exporta um array de rotas filhas do layout Main (prefixo /index).
 * Telas são carregadas sob demanda (import dinâmico) para manter o bundle inicial pequeno.
 */
import perfilRoutes from "./perfil.js";
import financeiroRoutes from "./financeiro.js";
import adminRoutes from "./admin.js";

export const fullModulesRoutes = [
  ...perfilRoutes,
  ...financeiroRoutes,
  ...adminRoutes,
];
