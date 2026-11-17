import { createHash, randomBytes } from 'node:crypto';

/**
 * Tokens de uso único (sessão, convite, reset de senha): 32 bytes aleatórios em base64url.
 * O token só existe no link/cookie; no banco fica apenas o SHA-256 (64 caracteres hex),
 * então quem lê o banco não consegue usar o token.
 */
export function generateToken(): { token: string; hash: string } {
  const token = randomBytes(32).toString('base64url');
  return { token, hash: hashToken(token) };
}

export function hashToken(token: string): string {
  return createHash('sha256').update(token).digest('hex');
}
