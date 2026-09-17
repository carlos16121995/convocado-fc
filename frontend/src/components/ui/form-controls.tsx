import { forwardRef, useId, type InputHTMLAttributes, type ReactNode, type SelectHTMLAttributes, type TextareaHTMLAttributes } from 'react'
import { classes } from './utils'

export function Label({ children, ...props }: { children: ReactNode } & React.LabelHTMLAttributes<HTMLLabelElement>) {
  return <label className="ui-label" {...props}>{children}</label>
}

type InputProps = InputHTMLAttributes<HTMLInputElement> & { invalid?: boolean }

export const Input = forwardRef<HTMLInputElement, InputProps>(function Input({ className, invalid = false, ...props }, ref) {
  return <input className={classes('ui-input', invalid && 'ui-input--invalid', className)} ref={ref} {...props} />
})

type TextareaProps = TextareaHTMLAttributes<HTMLTextAreaElement> & { invalid?: boolean }

export const Textarea = forwardRef<HTMLTextAreaElement, TextareaProps>(function Textarea({ className, invalid = false, ...props }, ref) {
  return <textarea className={classes('ui-textarea', invalid && 'ui-input--invalid', className)} ref={ref} {...props} />
})

type SelectProps = SelectHTMLAttributes<HTMLSelectElement> & { invalid?: boolean }

export const Select = forwardRef<HTMLSelectElement, SelectProps>(function Select({ children, className, invalid = false, ...props }, ref) {
  return <select className={classes('ui-select', invalid && 'ui-input--invalid', className)} ref={ref} {...props}>{children}</select>
})

type CheckboxProps = Omit<InputHTMLAttributes<HTMLInputElement>, 'type'> & { label: ReactNode }

export const Checkbox = forwardRef<HTMLInputElement, CheckboxProps>(function Checkbox({ className, label, ...props }, ref) {
  return <label className={classes('ui-check-control', className)}>
    <input className="ui-checkbox" ref={ref} type="checkbox" {...props} />
    <span>{label}</span>
  </label>
})

type RadioOption = { label: string; value: string; disabled?: boolean }

export function RadioGroup({ name, onChange, options, value }: { name: string; onChange: (value: string) => void; options: RadioOption[]; value: string }) {
  return (
    <div className="ui-radio-group" role="radiogroup">
      {options.map((option) => (
        <label className="ui-check-control" key={option.value}>
          <input checked={value === option.value} className="ui-radio" disabled={option.disabled} name={name} onChange={() => onChange(option.value)} type="radio" value={option.value} />
          <span>{option.label}</span>
        </label>
      ))}
    </div>
  )
}

export function Switch({ checked, disabled = false, label, onCheckedChange }: { checked: boolean; disabled?: boolean; label: ReactNode; onCheckedChange: (checked: boolean) => void }) {
  const labelId = useId()

  return (
    <div className="ui-switch-control">
      <button aria-checked={checked} aria-labelledby={labelId} className="ui-switch" disabled={disabled} onClick={() => onCheckedChange(!checked)} role="switch" type="button">
        <span className="ui-switch__thumb" />
      </button>
      <span id={labelId}>{label}</span>
    </div>
  )
}

export function FieldMessage({ children, tone = 'default' }: { children: ReactNode; tone?: 'default' | 'error' }) {
  return <p className={`ui-field-message ui-field-message--${tone}`}>{children}</p>
}
