# context

Estado compartilhado por uma árvore de componentes, sem ser global. É o "Context" do React; no Vue o mecanismo se chama `provide`/`inject`. Cada arquivo aqui exporta a chave tipada (`InjectionKey`) e um par `provideX()` / `useX()`.

Use context quando só uma parte da tela precisa do dado (ex.: o formulário de um lançamento e seus campos). Estado global do app (usuário logado, preferências) vai em `store/` (Pinia).
