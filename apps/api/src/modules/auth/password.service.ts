import { hash, verify } from '@node-rs/argon2';
import { Injectable } from '@nestjs/common';

/** Hash de senha com argon2id (parâmetros padrão da biblioteca, recomendados pela OWASP). */
@Injectable()
export class PasswordService {
  /** Hash usado quando o usuário não existe, para o login levar o mesmo tempo nos dois casos. */
  private dummyHash?: Promise<string>;

  hash(password: string): Promise<string> {
    return hash(password);
  }

  async verify(passwordHash: string | null | undefined, password: string): Promise<boolean> {
    if (!passwordHash) {
      // Gasta o mesmo tempo de uma verificação real: não revela se o usuário existe.
      this.dummyHash ??= hash('senha-que-nao-existe-para-ninguem');
      await verify(await this.dummyHash, password).catch(() => false);
      return false;
    }
    return verify(passwordHash, password).catch(() => false);
  }
}
