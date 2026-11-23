import { defineConfig } from 'tsup';

// Gera ESM (para o Vite) e CommonJS (para o NestJS, que roda em CommonJS) a partir do mesmo código.
export default defineConfig({
  entry: ['src/index.ts'],
  format: ['esm', 'cjs'],
  dts: true,
  clean: true,
  sourcemap: true,
  target: 'es2023',
});
