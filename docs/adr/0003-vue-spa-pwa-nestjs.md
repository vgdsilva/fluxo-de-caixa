# 0003 Vue 3 SPA + PWA no front, NestJS no back

**Status:** aceita (16/11/2026). Substitui a ideia inicial de back em .NET.

**Contexto.** Uso principal pelo iPhone, inclusive com sinal ruim. Uma linguagem só no front e no back facilita compartilhar contratos e o trabalho com IA.

**Decisão.** Front em Vue 3 (`<script setup>`, Vite, Pinia, Vue Router, PrimeVue) como SPA instalável (PWA via `vite-plugin-pwa`, fatia 6). Back em NestJS 11, cuja estrutura (módulos, injeção de dependência, guards, interceptors) é a mais próxima do ASP.NET no mundo Node.

**Consequências.** A casca do app abre sem sinal; só os dados dependem da API. No front, cada funcionalidade é uma pasta em `src/features/`; no back, um módulo Nest.
