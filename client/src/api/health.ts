import { getJson } from './client'

export interface HealthResponse {
  status: string
  database: string
}

export const getHealth = (signal?: AbortSignal) => getJson<HealthResponse>('/api/health', signal)
