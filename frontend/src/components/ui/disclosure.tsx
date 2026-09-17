import { useId, useState, type ReactNode } from 'react'

export type AccordionItem = { content: ReactNode; id: string; title: string }

export function Accordion({ items }: { items: AccordionItem[] }) {
  const baseId = useId()
  const [openItemIds, setOpenItemIds] = useState<string[]>([])

  return (
    <div className="ui-accordion">
      {items.map((item) => {
        const isOpen = openItemIds.includes(item.id)
        const contentId = `${baseId}-${item.id}`

        return (
          <section className="ui-accordion__item" data-open={isOpen} key={item.id}>
            <button aria-controls={contentId} aria-expanded={isOpen} className="ui-accordion__trigger" onClick={() => setOpenItemIds((currentIds) => isOpen ? currentIds.filter((id) => id !== item.id) : [...currentIds, item.id])} type="button">
              {item.title}<span aria-hidden="true">⌄</span>
            </button>
            <div aria-hidden={!isOpen} className="ui-accordion__content" id={contentId}><div className="ui-accordion__content-inner">{item.content}</div></div>
          </section>
        )
      })}
    </div>
  )
}
