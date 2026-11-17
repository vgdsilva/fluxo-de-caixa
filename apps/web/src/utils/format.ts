const TIME_ZONE = 'America/Sao_Paulo';

/** Data e hora no fuso de exibição do app (São Paulo), ex.: 17/11/2026 19:05. */
export function formatDateTime(iso: string | null | undefined): string {
  if (!iso) return '—';
  return new Intl.DateTimeFormat('pt-BR', {
    timeZone: TIME_ZONE,
    dateStyle: 'short',
    timeStyle: 'short',
  }).format(new Date(iso));
}

/** "Chrome no Windows", "Safari no iPhone"... a partir do user agent (só para exibir). */
export function describeDevice(userAgent: string | null): string {
  if (!userAgent) return 'Aparelho desconhecido';
  const ua = userAgent;
  const browser = /Edg\//.test(ua)
    ? 'Edge'
    : /Firefox\//.test(ua)
      ? 'Firefox'
      : /Chrome\//.test(ua)
        ? 'Chrome'
        : /Safari\//.test(ua)
          ? 'Safari'
          : /curl\//.test(ua)
            ? 'curl'
            : 'Navegador';
  const os = /iPhone/.test(ua)
    ? 'iPhone'
    : /iPad/.test(ua)
      ? 'iPad'
      : /Android/.test(ua)
        ? 'Android'
        : /Windows/.test(ua)
          ? 'Windows'
          : /Mac OS X/.test(ua)
            ? 'Mac'
            : /Linux/.test(ua)
              ? 'Linux'
              : '';
  return os ? `${browser} no ${os}` : browser;
}
