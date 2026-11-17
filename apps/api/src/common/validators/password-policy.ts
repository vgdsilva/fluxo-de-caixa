import { BadRequestException } from '@nestjs/common';
import { COMMON_PASSWORDS } from '../constants/common-passwords';

/**
 * Regras de senha além do tamanho mínimo (12, validado pelo schema zod): não pode ser uma senha
 * comum, não pode conter o usuário/nome/e-mail e não pode ser só um caractere repetido.
 * Lança 400 com a mensagem em português.
 */
export function assertStrongPassword(
  password: string,
  context: { username?: string; name?: string; email?: string | null },
): void {
  const lower = password.toLowerCase();
  const compact = lower.replace(/[^a-z0-9]/g, '');

  if (COMMON_PASSWORDS.has(lower) || COMMON_PASSWORDS.has(compact)) {
    throw new BadRequestException('Essa senha é muito comum. Escolha outra.');
  }
  if (/^(.)\1+$/.test(password)) {
    throw new BadRequestException('A senha não pode ser um só caractere repetido.');
  }
  // Sequências óbvias (123456789012, abcdefghijkl...)
  if (isSequence(compact)) {
    throw new BadRequestException('A senha não pode ser uma sequência simples.');
  }

  const personal = [
    context.username,
    context.email?.split('@')[0],
    ...(context.name?.split(/\s+/) ?? []),
  ]
    .map((part) => part?.toLowerCase().trim())
    .filter((part): part is string => !!part && part.length >= 3);
  if (personal.some((part) => lower.includes(part))) {
    throw new BadRequestException('A senha não pode conter seu usuário, nome ou e-mail.');
  }
}

function isSequence(value: string): boolean {
  if (value.length < 6) return false;
  let ascending = true;
  let descending = true;
  for (let i = 1; i < value.length; i++) {
    const diff = value.charCodeAt(i) - value.charCodeAt(i - 1);
    if (diff !== 1) ascending = false;
    if (diff !== -1) descending = false;
  }
  return ascending || descending;
}
