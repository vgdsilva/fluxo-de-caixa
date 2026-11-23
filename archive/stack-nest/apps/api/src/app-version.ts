import { readFileSync } from 'node:fs';
import { join } from 'node:path';

// Compilado em dist/app-version.js, então ../package.json é o package.json da API.
const pkg = JSON.parse(readFileSync(join(__dirname, '..', 'package.json'), 'utf8')) as {
  version: string;
};

export const APP_VERSION = pkg.version;
