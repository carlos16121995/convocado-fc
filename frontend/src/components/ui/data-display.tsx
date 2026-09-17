import { Fragment, useMemo, useState, type ReactNode } from 'react'
import { Button, Spinner } from './primitives'

export type TableColumn = { key: string; label: string; sortable?: boolean }
export type TableRow = {
  cells: Record<string, ReactNode>
  details?: ReactNode
  id: string
  sortValues?: Record<string, number | string>
}

export type CarouselSlide = { alt: string; image: string; title: string }
export function ImageCarousel({ slides }: { slides: CarouselSlide[] }) { const [active, setActive] = useState(0); const slide = slides[active]; return <div aria-label="Carrossel de imagens" className="ui-carousel" role="region"><img alt={slide.alt} className="ui-carousel__image" src={slide.image} /><div className="ui-carousel__caption"><strong>{slide.title}</strong><span>{active + 1} / {slides.length}</span></div><div className="ui-carousel__controls"><Button aria-label="Imagem anterior" disabled={active === 0} onClick={() => setActive(active - 1)} variant="outline">←</Button><div className="ui-carousel__dots">{slides.map((item, index) => <button aria-label={`Ir para ${item.title}`} aria-pressed={index === active} className="ui-carousel__dot" key={item.title} onClick={() => setActive(index)} type="button" />)}</div><Button aria-label="Próxima imagem" disabled={active === slides.length - 1} onClick={() => setActive(active + 1)} variant="outline">→</Button></div></div> }

type Sort = { direction: 'ascending' | 'descending'; key: string }

export function Table({ caption, columns, expandable = false, rows }: { caption: string; columns: TableColumn[]; expandable?: boolean; rows: TableRow[] }) {
  const [expandedRowId, setExpandedRowId] = useState<string | null>(null)
  const [sort, setSort] = useState<Sort | null>(null)
  const sortedRows = useMemo(() => {
    if (!sort) return rows

    return [...rows].sort((left, right) => {
      const leftValue = left.sortValues?.[sort.key] ?? left.cells[sort.key] ?? ''
      const rightValue = right.sortValues?.[sort.key] ?? right.cells[sort.key] ?? ''
      const comparison = String(leftValue).localeCompare(String(rightValue), 'pt-BR', { numeric: true })
      return sort.direction === 'ascending' ? comparison : -comparison
    })
  }, [rows, sort])

  function toggleSort(column: TableColumn) {
    if (!column.sortable) return

    setSort((currentSort) => ({
      key: column.key,
      direction: currentSort?.key === column.key && currentSort.direction === 'ascending' ? 'descending' : 'ascending',
    }))
  }

  return (
    <div className="ui-table-wrapper">
      <table className="ui-table">
        <caption>{caption}</caption>
        <thead>
          <tr>
            {expandable && <th className="ui-table__expand-header" scope="col">Detalhes</th>}
            {columns.map((column) => {
              const columnSort = sort?.key === column.key ? sort.direction : 'none'
              return (
                <th aria-sort={column.sortable ? columnSort : undefined} key={column.key} scope="col">
                  {column.sortable ? <button className="ui-table__sort" onClick={() => toggleSort(column)} type="button">{column.label}<span aria-hidden="true">{columnSort === 'ascending' ? '↑' : columnSort === 'descending' ? '↓' : '↕'}</span></button> : column.label}
                </th>
              )
            })}
          </tr>
        </thead>
        <tbody>
          {sortedRows.map((row) => {
            const isExpanded = expandedRowId === row.id
            return (
              <Fragment key={row.id}>
                <tr>
                  {expandable && <td className="ui-table__expand-cell"><button aria-expanded={isExpanded} aria-label={`${isExpanded ? 'Minimizar' : 'Expandir'} detalhes da linha`} className="ui-table__expand" onClick={() => setExpandedRowId(isExpanded ? null : row.id)} type="button">⌄</button></td>}
                  {columns.map((column) => <td key={column.key}>{row.cells[column.key]}</td>)}
                </tr>
                {expandable && row.details && <tr className="ui-table__details" data-expanded={isExpanded}><td colSpan={columns.length + 1}><div aria-hidden={!isExpanded} className="ui-table__details-content"><div className="ui-table__details-content-inner">{row.details}</div></div></td></tr>}
              </Fragment>
            )
          })}
        </tbody>
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
