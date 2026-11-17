import { healthResponseSchema, type HealthResponse } from '@fluxo/contracts';
import { apiGet } from './http';

/** Chamadas HTTP da feature health. Os tipos vêm de @fluxo/contracts, nunca escritos à mão. */
export function fetchHealth(): Promise<HealthResponse> {
  return apiGet('/health', healthResponseSchema);
}
