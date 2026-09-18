export type ApiError = {
  code: string
  message: string
}

export type PageInfo = {
  page: number
  pageSize: number
  totalItems: number
  totalPages: number
}

export type ApiResponse<T> = {
  data: T | null
  correlationId: string
  pagination: PageInfo | null
  error: ApiError | null
  errors: Record<string, string[]> | null
  succeeded: boolean
}
