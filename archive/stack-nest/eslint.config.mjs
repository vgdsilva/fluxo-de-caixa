// Configuração única de lint para o monorepo (flat config do ESLint).
import js from '@eslint/js';
import prettier from 'eslint-config-prettier';
import vue from 'eslint-plugin-vue';
import globals from 'globals';
import tseslint from 'typescript-eslint';

export default tseslint.config(
  {
    ignores: ['**/dist/**', '**/coverage/**', '**/node_modules/**'],
  },
  js.configs.recommended,
  ...tseslint.configs.recommended,

  // API (Node, CommonJS em runtime)
  {
    files: ['apps/api/**/*.ts'],
    languageOptions: { globals: { ...globals.node, ...globals.jest } },
    rules: {
      // Nest usa classes vazias decoradas como módulos.
      '@typescript-eslint/no-extraneous-class': 'off',
      // Regra da spec: controllers e serviços não usam o EntityManager direto fora da camada de dados.
      // (verificada com dependency-cruiser quando os testes entrarem)
    },
  },

  // Web (Vue 3)
  ...vue.configs['flat/recommended'].map((config) => ({
    ...config,
    files: ['apps/web/**/*.{ts,vue}'],
  })),
  {
    files: ['apps/web/**/*.{ts,vue}'],
    languageOptions: {
      globals: globals.browser,
      parserOptions: { parser: tseslint.parser, extraFileExtensions: ['.vue'] },
    },
    rules: {
      // O web só fala com a API por HTTP: nunca importa código de apps/api.
      'no-restricted-imports': [
        'error',
        {
          patterns: [
            {
              group: ['**/api/src/**', '**/apps/api/**', '@fluxo/api', '@fluxo/api/*'],
              message: 'O web só fala com a API por HTTP.',
            },
          ],
        },
      ],
    },
  },

  // Componentes gerados pelo CLI do shadcn-vue (Button, Card...): mantidos como o CLI entrega.
  {
    files: ['apps/web/src/components/ui/**/*.vue'],
    rules: {
      'vue/multi-word-component-names': 'off',
      'vue/require-default-prop': 'off',
    },
  },

  // Configs de ferramentas (Node)
  {
    files: ['**/*.config.{js,ts}', '**/tsup.config.ts'],
    languageOptions: { globals: globals.node },
  },

  // Desliga regras de estilo que brigam com o Prettier. Fica por último.
  prettier,
);
