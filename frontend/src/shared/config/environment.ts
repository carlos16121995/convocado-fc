const configuredApiBaseUrl = import.meta.env.VITE_API_BASE_URL?.trim()

export const environment = {
  apiBaseUrl: configuredApiBaseUrl || 'http://localhost:5080',
}
