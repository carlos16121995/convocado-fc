import { type ReactNode } from 'react'
import { Button, Spinner } from './primitives'

export type TableColumn = { key: string; label: string }
export type TableRow = Record<string, ReactNode>

export function Table({ caption, columns, rows }: { caption: string; columns: TableColumn[]; rows: TableRow[] }) {
  return (
    <div className="ui-table-wrapper">
      <table className="ui-table">
        <caption>{caption}</caption>
        <thead><tr>{columns.map((column) => <th key={column.key} scope="col">{column.label}</th>)}</tr></thead>
        <tbody>{rows.map((row, index) => <tr key={index}>{columns.map((column) => <td key={column.key}>{row[column.key]}</td>)}</tr>)}</tbody>
      </table>
    </div>
  )
}

export function Pagination({ currentPage, onPageChange, totalPages }: { currentPage: number; onPageChange: (page: number) => void; totalPages: number }) {
  return (
    <nav aria-label="Paginação" className="ui-pagination">
      <Button disabled={currentPage === 1} onClick={() => onPageChange(currentPage - 1)} variant="outline">Anterior</Button>
      <span aria-current="page">Página {currentPage} de {totalPages}</span>
      <Button disabled={currentPage === totalPages} onClick={() => onPageChange(currentPage + 1)} variant="outline">Próxima</Button>
    </nav>
  )
}

export function EmptyState({ action, children, title }: { action?: ReactNode; children: ReactNode; title: string }) {
  return <section className="ui-state"><strong>{title}</strong><span>{children}</span>{action}</section>
}

export function LoadingState({ children = 'Buscando informações…' }: { children?: ReactNode }) {
  return <section className="ui-state"><Spinner /><span>{children}</span></section>
}

export function ErrorState({ action, children = 'Não foi possível carregar os dados.', title = 'Algo deu errado' }: { action?: ReactNode; children?: ReactNode; title?: string }) {
  return <section className="ui-state ui-state--error"><strong>{title}</strong><span>{children}</span>{action}</section>
}
