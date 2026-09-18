import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { render, screen } from '@testing-library/react'
import { MemoryRouter } from 'react-router-dom'
import { describe, expect, it, vi } from 'vitest'
import { AvailabilityPage } from './AvailabilityPage'

describe('AvailabilityPage', () => {
  it('informa quando a API e a base de dados estão disponíveis', async () => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue(new Response(JSON.stringify({
      correlationId: '42f8513f-2d46-4a63-8e7e-56ef5d5b7779',
      data: { status: 'Healthy' },
      error: null,
      errors: null,
      pagination: null,
      succeeded: true,
    }), { status: 200 })))

    renderPage()

    expect(await screen.findByText('Disponível')).toBeTruthy()
    expect(screen.getByText('A API está conectada à base de dados.')).toBeTruthy()
  })

  it('apresenta a falha e o código de correlação retornados pela API', async () => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue(new Response(JSON.stringify({
      correlationId: '42f8513f-2d46-4a63-8e7e-56ef5d5b7779',
      data: null,
      error: { code: 'database_unavailable', message: 'A base de dados está indisponível.' },
      errors: null,
      pagination: null,
      succeeded: false,
    }), { status: 503 })))

    renderPage()

    expect(await screen.findByText('Serviço indisponível')).toBeTruthy()
    expect(screen.getByText(/42f8513f-2d46-4a63-8e7e-56ef5d5b7779/)).toBeTruthy()
  })
})

function renderPage() {
  const queryClient = new QueryClient({
    defaultOptions: { queries: { retry: false } },
  })

  return render(
    <QueryClientProvider client={queryClient}>
      <MemoryRouter>
        <AvailabilityPage />
      </MemoryRouter>
    </QueryClientProvider>,
  )
}
