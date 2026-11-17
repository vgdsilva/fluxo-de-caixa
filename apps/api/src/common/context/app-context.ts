import { AsyncLocalStorage } from 'node:async_hooks';
import { v7 as uuidv7 } from 'uuid';

/** Por onde a alteração entrou. Vai para a auditoria (spec da fase 0). */
export type Channel = 'ui' | 'chat' | 'import' | 'job' | 'auth';
export type Role = 'admin' | 'user';

/**
 * Contexto de cada requisição (ou job): quem é o usuário, por qual canal entrou e o CorrelationId
 * que liga logs e auditoria. Guardado num AsyncLocalStorage, o equivalente Node do AsyncLocal<T>
 * do .NET: cada requisição enxerga o próprio valor, mesmo atravessando awaits.
 *
 * Quem abre: CorrelationIdMiddleware (toda requisição) e runAsSystem (jobs e seeds).
 * Quem preenche o usuário: SessionGuard (cookie de sessão) e o login.
 * Quem lê: filtro owner, subscribers de dono e de auditoria, guards.
 */
interface AppContextStore {
  correlationId: string;
  channel: Channel;
  userId?: string;
  role?: Role;
  sessionId?: string;
  /** Jobs e seeds: sem filtro de dono. Só via runAsSystem (src/database/system-context.ts). */
  system?: boolean;
}

const storage = new AsyncLocalStorage<AppContextStore>();

export const AppContext = {
  /** Abre o contexto de uma requisição. Tudo que rodar dentro de `fn` enxerga este contexto. */
  run<T>(init: { correlationId?: string; channel?: Channel }, fn: () => T): T {
    return storage.run(
      { correlationId: init.correlationId ?? uuidv7(), channel: init.channel ?? 'ui' },
      fn,
    );
  },

  /** Não use direto: chame runAsSystem de src/database/system-context.ts. */
  runAsSystemUnsafe<T>(fn: () => T): T {
    const parent = storage.getStore();
    return storage.run(
      { correlationId: parent?.correlationId ?? uuidv7(), channel: 'job', system: true },
      fn,
    );
  },

  /** Identifica o usuário da requisição atual (middleware de dev; sessão a partir da fatia 4). */
  setUser(userId: string, role: Role, sessionId?: string): void {
    const store = requireStore();
    store.userId = userId;
    store.role = role;
    if (sessionId) store.sessionId = sessionId;
  },

  /** Sessão desta requisição (para "encerrar as outras" e marcar a atual na lista). */
  sessionId(): string | undefined {
    return storage.getStore()?.sessionId;
  },

  setChannel(channel: Channel): void {
    requireStore().channel = channel;
  },

  userId(): string | undefined {
    return storage.getStore()?.userId;
  },

  role(): Role | undefined {
    return storage.getStore()?.role;
  },

  isSystem(): boolean {
    return storage.getStore()?.system === true;
  },

  correlationId(): string | undefined {
    return storage.getStore()?.correlationId;
  },

  channel(): Channel {
    return storage.getStore()?.channel ?? 'job';
  },
};

function requireStore(): AppContextStore {
  const store = storage.getStore();
  if (!store) {
    throw new Error('AppContext fora de uma requisição (falta o CorrelationIdMiddleware).');
  }
  return store;
}
