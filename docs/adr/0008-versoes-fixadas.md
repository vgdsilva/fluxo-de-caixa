# 0008 Versões fixadas: Nest 11, TypeScript 5.9, PrimeVue 4.5

**Status:** aceita (16/11/2026)

**Contexto.** Na fatia 1, as versões mais novas de algumas dependências eram incompatíveis com o resto da stack ou mudaram de licença:

- NestJS 12 já saiu, mas `nestjs-zod` 5 e `@nestjs/config` 4 declaram suporte só até o Nest 11.
- TypeScript 7 já saiu, mas `typescript-eslint` aceita até TS 6.0 e `ts-jest` até TS 6; o Nest CLI 11 traz TS 5.9.
- PrimeVue 5 e `@primeuix/themes` 3 deixaram a licença MIT pela "PrimeUI License", que exige chave (o uso pessoal se enquadra na licença Community gratuita, mas com renovação anual) e mostra um aviso na tela sem a chave.

**Decisão.** Fixar NestJS 11, `@nestjs/config` 4, TypeScript `~5.9.3` nos três pacotes, `primevue ~4.5.5` e `@primeuix/themes ~2.0.3` (ambos MIT). As demais dependências ficam na versão mais nova do dia, travadas pelo lockfile.

**Consequências.** Reavaliar quando `nestjs-zod` suportar o Nest 12 e quando `typescript-eslint` suportar TS 7. Para o PrimeVue, ou ficamos no 4.x (MIT, sem novidades), ou pegamos a chave Community gratuita e migramos para o 5, ou trocamos de biblioteca de componentes; a decisão fica com o dono do projeto.
