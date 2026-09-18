import type { ApiResponse } from './contracts'
import { environment } from '../config/environment'

type RequestOptions = {
  signal?: AbortSignal
}

export class ApiRequestError extends Error {
  public readonly code: string | undefined
  public readonly correlationId: string | undefined
  public readonly fieldErrors: Record<string, string[]> | undefined
  public readonly status: number

  public constructor({ code, correlationId, fieldErrors, message, status }: {
    code?: string
    correlationId?: string
    fieldErrors?: Record<string, string[]>
    message: string
    status: number
  }) {
    super(message)
    this.name = 'ApiRequestError'
    this.code = code
    this.correlationId = correlationId
    this.fieldErrors = fieldErrors
    this.status = status
  }
}

export async function get<T>(path: string, { signal }: RequestOptions = {}): Promise<T> {
  const response = await fetch(new URL(path, environment.apiBaseUrl), {
    headers: { Accept: 'application/json' },
    signal,
  })
  const body = await parseResponse<T>(response)

  if (!response.ok || !body.succeeded || body.data === null) {
    throw new ApiRequestError({
      code: body.error?.code,
      correlationId: body.correlationId,
      fieldErrors: body.errors ?? undefined,
      message: body.error?.message ?? 'Não foi possível concluir a solicitação.',
      status: response.status,
    })
  }

  return body.data
}

async function parseResponse<T>(response: Response): Promise<ApiResponse<T>> {
  try {
    return await response.json() as ApiResponse<T>
  } catch {
    throw new ApiRequestError({
      message: 'A API respondeu em um formato inválido.',
      status: response.status,
    })
  }
}
