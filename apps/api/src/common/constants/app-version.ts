import { readFileSync } from 'node:fs';
import { join } from 'node:path';

// Compilado em dist/common/constants/app-version.js; três níveis acima fica o package.json da API.
const pkg = JSON.parse(readFileSync(join(__dirname, '..', '..', '..', 'package.json'), 'utf8')) as {
  version: string;
};

export const APP_VERSION = pkg.version;
