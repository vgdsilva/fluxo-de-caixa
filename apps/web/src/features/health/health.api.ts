import { healthResponseSchema, type HealthResponse } from '@fluxo/contracts';
import { apiGet } from '@/shared/http';

export function fetchHealth(): Promise<HealthResponse> {
  return apiGet('/health', healthResponseSchema);
}
