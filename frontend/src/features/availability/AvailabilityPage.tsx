import { useQuery } from '@tanstack/react-query'
import { Link } from 'react-router-dom'
import { Badge, Button, Card, CardHeader, CardTitle, ErrorState, LoadingState } from '../../components/ui'
import { ApiRequestError } from '../../shared/api/httpClient'
import { getAvailability } from './api/getAvailability'
import './AvailabilityPage.css'

export function AvailabilityPage() {
  const availabilityQuery = useQuery({
    queryKey: ['availability'],
    queryFn: ({ signal }) => getAvailability(signal),
    retry: false,
  })

  return (
    <main className="app-page">
      <section className="availability-page">
        <header className="availability-page__header">
          <p className="app-eyebrow">Convocado FC</p>
          <h1>Disponibilidade do serviço</h1>
          <p>Acompanhe a conexão entre a aplicação e a base de dados.</p>
        </header>

        <Card tone={availabilityQuery.data?.status === 'Healthy' ? 'success' : 'default'}>
          <CardHeader>
            <CardTitle>API</CardTitle>
            {availabilityQuery.data?.status === 'Healthy' && <Badge tone="success">Disponível</Badge>}
          </CardHeader>

          {availabilityQuery.isPending && <LoadingState>Consultando a API e a base de dados…</LoadingState>}

          {availabilityQuery.isError && (
            <ErrorState
              action={<Button loading={availabilityQuery.isFetching} onClick={() => void availabilityQuery.refetch()}>Tentar novamente</Button>}
              title="Serviço indisponível"
            >
              {getErrorMessage(availabilityQuery.error)}
            </ErrorState>
          )}

          {availabilityQuery.data?.status === 'Healthy' && (
            <p className="availability-page__success">A API está conectada à base de dados.</p>
          )}
        </Card>

        <Link className="app-link" to="/design-system">Abrir Design System</Link>
      </section>
    </main>
  )
}

function getErrorMessage(error: Error): string {
  if (error instanceof ApiRequestError && error.correlationId) {
    return `${error.message} Código de correlação: ${error.correlationId}.`
  }

  return 'Não foi possível verificar a disponibilidade. Confirme se a API está em execução e tente novamente.'
}
