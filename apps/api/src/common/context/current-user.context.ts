import { AsyncLocalStorage } from 'node:async_hooks';

/**
 * Quem está fazendo a requisição atual. Guardado num AsyncLocalStorage, que é o equivalente Node
 * do AsyncLocal<T> do .NET: cada requisição enxerga o próprio valor, mesmo atravessando awaits,
 * sem precisar passar o usuário de função em função.
 *
 * Lido pelo filtro `owner` e pelo subscriber de dono (src/database). Preenchido por middleware:
 * na fatia 2 pelo DevUserMiddleware (cabeçalho de desenvolvimento), na fatia 4 pela sessão.
 */
interface CurrentUserStore {
  userId?: string;
  /** Jobs e seeds: sem filtro de dono. Só via runAsSystem (src/database/system-context.ts). */
  system?: boolean;
}

const storage = new AsyncLocalStorage<CurrentUserStore>();

export const CurrentUserContext = {
  runAsUser<T>(userId: string, fn: () => T): T {
    return storage.run({ userId }, fn);
  },

  /** Não use direto: chame runAsSystem de src/database/system-context.ts. */
  runAsSystemUnsafe<T>(fn: () => T): T {
    return storage.run({ system: true }, fn);
  },

  userId(): string | undefined {
    return storage.getStore()?.userId;
  },

  isSystem(): boolean {
    return storage.getStore()?.system === true;
  },
};
