import { useEffect, useRef, type ReactNode } from 'react'
import { Button } from './primitives'
import { classes } from './utils'

export function Dialog({ children, onOpenChange, open, title }: { children: ReactNode; onOpenChange: (open: boolean) => void; open: boolean; title: string }) {
  const dialogRef = useRef<HTMLDialogElement>(null)

  useEffect(() => {
    const dialog = dialogRef.current
    if (!dialog) return

    if (open && !dialog.open) dialog.showModal()
    if (!open && dialog.open) dialog.close()
  }, [open])

  return (
    <dialog aria-labelledby="dialog-title" className="ui-dialog" onClose={() => onOpenChange(false)} ref={dialogRef}>
      <div className="ui-dialog__content">
        <div className="ui-dialog__header">
          <h2 id="dialog-title">{title}</h2>
          <Button aria-label="Fechar diálogo" onClick={() => onOpenChange(false)} variant="ghost">×</Button>
        </div>
        {children}
      </div>
    </dialog>
  )
}

export function Dropdown({ children, label }: { children: ReactNode; label: string }) {
  return (
    <details className="ui-dropdown">
      <summary>{label}</summary>
      <div className="ui-dropdown__menu" role="menu">{children}</div>
    </details>
  )
}

export function DropdownItem({ children, className, ...props }: React.ButtonHTMLAttributes<HTMLButtonElement>) {
  return <button className={classes('ui-dropdown__item', className)} role="menuitem" type="button" {...props}>{children}</button>
}

export function Toast({ children, onDismiss, tone = 'info' }: { children: ReactNode; onDismiss?: () => void; tone?: 'success' | 'warning' | 'alert' | 'danger' | 'info' }) {
  return (
    <div className={`ui-toast ui-toast--${tone}`} role="status">
      <span>{children}</span>
      {onDismiss && <button aria-label="Fechar notificação" onClick={onDismiss} type="button">×</button>}
    </div>
  )
}

export function InlineConfirmation({ cancelLabel = 'Cancelar', confirmLabel = 'Confirmar', description, onCancel, onConfirm, title }: { cancelLabel?: string; confirmLabel?: string; description?: string; onCancel: () => void; onConfirm: () => void; title: string }) {
  return (
    <section className="ui-inline-confirmation" role="alert">
      <div>
        <strong>{title}</strong>
        {description && <p>{description}</p>}
      </div>
      <div className="ui-inline-confirmation__actions">
        <Button onClick={onCancel} variant="outline">{cancelLabel}</Button>
        <Button onClick={onConfirm} variant="destructive">{confirmLabel}</Button>
      </div>
    </section>
  )
}
