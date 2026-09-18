import { get } from '../../../shared/api/httpClient'
import type { Response as HealthResponse } from '../../../shared/api/generated/types.gen'

export type Availability = HealthResponse

export function getAvailability(signal?: AbortSignal): Promise<Availability> {
  return get<Availability>('/health', { signal })
}
