import { cloneElement, isValidElement, useId, useState, type ButtonHTMLAttributes, type HTMLAttributes, type ReactNode } from 'react'
import { classes } from './utils'

export type ButtonVariant = 'primary' | 'secondary' | 'outline' | 'ghost' | 'destructive'

type ButtonProps = ButtonHTMLAttributes<HTMLButtonElement> & {
  variant?: ButtonVariant
  loading?: boolean
}

export function Button({ className, children, disabled, loading = false, variant = 'primary', ...props }: ButtonProps) {
  return (
    <button className={classes('ui-button', `ui-button--${variant}`, className)} disabled={disabled || loading} {...props}>
      {loading && <Spinner size="sm" aria-label="Carregando" />}
      <span>{children}</span>
    </button>
  )
}

export type CardTone = 'default' | 'success' | 'warning' | 'alert' | 'danger' | 'info'

type CardProps = HTMLAttributes<HTMLElement> & { children: ReactNode; tone?: CardTone }

export function Card({ className, children, tone = 'default', ...props }: CardProps) {
  return <section className={classes('ui-card', `ui-card--${tone}`, className)} {...props}>{children}</section>
}

export function CardHeader({ className, ...props }: HTMLAttributes<HTMLDivElement>) {
  return <div className={classes('ui-card__header', className)} {...props} />
}

export function CardTitle({ className, ...props }: HTMLAttributes<HTMLHeadingElement>) {
  return <h3 className={classes('ui-card__title', className)} {...props} />
}

export type BadgeTone = 'neutral' | 'primary' | 'success' | 'warning' | 'alert' | 'danger' | 'info' | 'processing'

export function Badge({ className, tone = 'neutral', ...props }: HTMLAttributes<HTMLSpanElement> & { tone?: BadgeTone }) {
  return <span className={classes('ui-badge', `ui-badge--${tone}`, className)} {...props} />
}

export type AlertTone = 'success' | 'warning' | 'alert' | 'danger' | 'info'

const alertTitles: Record<AlertTone, string> = {
  success: 'Sucesso',
  warning: 'Atenção',
  alert: 'Alerta',
  danger: 'Erro',
  info: 'Informação',
}

export function Alert({ children, className, title, tone = 'info' }: { children: ReactNode; className?: string; title?: string; tone?: AlertTone }) {
  return (
    <div className={classes('ui-alert', `ui-alert--${tone}`, className)} role="status">
      <strong>{title ?? alertTitles[tone]}</strong>
      <span>{children}</span>
    </div>
  )
}

export function Spinner({ className, size = 'md', tone = 'primary', ...props }: HTMLAttributes<HTMLSpanElement> & { size?: 'sm' | 'md' | 'lg'; tone?: 'primary' | 'processing' }) {
  return <span className={classes('ui-spinner', `ui-spinner--${size}`, `ui-spinner--${tone}`, className)} {...props} />
}

export function Skeleton({ className, ...props }: HTMLAttributes<HTMLDivElement>) {
  return <div aria-label="Carregando" className={classes('ui-skeleton', className)} {...props} />
}

export function Tooltip({ children, content }: { children: ReactNode; content: string }) {
  const [isVisible, setIsVisible] = useState(false)
  const tooltipId = useId()
  const child = isValidElement<{ 'aria-describedby'?: string }>(children)
    ? cloneElement(children, { 'aria-describedby': isVisible ? tooltipId : undefined })
    : children

  return (
    <span className="ui-tooltip" onBlur={(event) => { if (!event.currentTarget.contains(event.relatedTarget)) setIsVisible(false) }} onFocus={() => setIsVisible(true)} onMouseEnter={() => setIsVisible(true)} onMouseLeave={() => setIsVisible(false)}>
      {child}
      {isVisible && <span className="ui-tooltip__content" id={tooltipId} role="tooltip">{content}</span>}
    </span>
  )
}
