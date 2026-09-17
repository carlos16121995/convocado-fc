import { Children, forwardRef, isValidElement, useId, useMemo, useRef, useState, type ChangeEvent, type FocusEvent, type ForwardedRef, type InputHTMLAttributes, type KeyboardEvent, type ReactNode, type SelectHTMLAttributes, type TextareaHTMLAttributes } from 'react'
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
type SelectOption = { disabled: boolean; label: string; value: string }

function textFromNode(node: ReactNode): string {
  return Children.toArray(node).map((child) => {
    if (typeof child === 'string' || typeof child === 'number') return String(child)
    if (isValidElement<{ children?: ReactNode }>(child)) return textFromNode(child.props.children)
    return ''
  }).join('').trim()
}

function selectOptionsFromChildren(children: ReactNode): SelectOption[] {
  return Children.toArray(children).flatMap((child) => {
    if (!isValidElement<{ children?: ReactNode; disabled?: boolean; value?: string | number }>(child)) return []

    if (child.type === 'optgroup') return selectOptionsFromChildren(child.props.children)
    if (child.type !== 'option') return []

    const label = textFromNode(child.props.children)
    return [{ disabled: Boolean(child.props.disabled), label, value: String(child.props.value ?? label) }]
  })
}

function assignForwardedRef<T>(ref: ForwardedRef<T>, element: T | null) {
  if (typeof ref === 'function') ref(element)
  else if (ref) ref.current = element
}

function setNativeValue(element: HTMLInputElement | HTMLSelectElement, value: string) {
  const prototype = element instanceof HTMLInputElement ? HTMLInputElement.prototype : HTMLSelectElement.prototype
  Object.getOwnPropertyDescriptor(prototype, 'value')?.set?.call(element, value)
}

export const Select = forwardRef<HTMLSelectElement, SelectProps>(function Select({ children, className, defaultValue, disabled = false, id, invalid = false, name, onBlur, onChange, value, ...nativeProps }, ref) {
  const generatedId = useId()
  const selectId = id ?? generatedId
  const listId = `${selectId}-options`
  const triggerRef = useRef<HTMLButtonElement>(null)
  const nativeSelectRef = useRef<HTMLSelectElement>(null)
  const [isOpen, setIsOpen] = useState(false)
  const [internalValue, setInternalValue] = useState(() => String(defaultValue ?? ''))
  const selectedValue = value === undefined ? internalValue : String(value)
  const options = useMemo(() => selectOptionsFromChildren(children), [children])
  const selectedOption = options.find((option) => option.value === selectedValue)
  const placeholder = options.find((option) => option.disabled && option.value === '')

  function selectOption(option: SelectOption) {
    if (option.disabled) return
    if (value === undefined) setInternalValue(option.value)
    if (nativeSelectRef.current) {
      setNativeValue(nativeSelectRef.current, option.value)
      nativeSelectRef.current.dispatchEvent(new Event('change', { bubbles: true }))
    }
    setIsOpen(false)
    triggerRef.current?.focus()
  }

  return (
    <span className={classes('ui-select-field', isOpen && 'ui-select-field--open')} onBlur={(event) => {
      if (event.currentTarget.contains(event.relatedTarget)) return
      setIsOpen(false)
      onBlur?.(event as unknown as FocusEvent<HTMLSelectElement>)
    }}>
      <select aria-hidden="true" className="ui-select__native" disabled={disabled} name={name} onChange={onChange} ref={(element) => {
        nativeSelectRef.current = element
        assignForwardedRef(ref, element)
      }} tabIndex={-1} value={selectedValue} {...nativeProps}>{children}</select>
      <button
        aria-controls={listId}
        aria-expanded={isOpen}
        aria-haspopup="listbox"
        aria-invalid={invalid || nativeProps['aria-invalid']}
        className={classes('ui-select', invalid && 'ui-input--invalid', className)}
        disabled={disabled}
        id={selectId}
        onClick={() => setIsOpen((open) => !open)}
        onKeyDown={(event) => {
          if (event.key === 'ArrowDown') {
            event.preventDefault()
            setIsOpen(true)
          }
          if (event.key === 'Escape') setIsOpen(false)
        }}
        ref={triggerRef}
        type="button"
      >
        <span className={classes('ui-select__value', !selectedOption && 'ui-select__placeholder')}>{selectedOption?.label ?? placeholder?.label ?? 'Selecione uma opção'}</span>
        <span aria-hidden="true" className="ui-select-field__indicator">⌄</span>
      </button>
      {isOpen && !disabled && (
        <ul className="ui-select__menu" id={listId} role="listbox">
          {options.map((option) => (
            <li key={option.value} role="none">
              <button aria-selected={option.value === selectedValue} disabled={option.disabled} onClick={() => selectOption(option)} type="button">
                {option.label}
              </button>
            </li>
          ))}
        </ul>
      )}
    </span>
  )
})

type DateInputProps = Omit<InputProps, 'defaultValue' | 'onChange' | 'type' | 'value'> & {
  defaultValue?: string
  mode?: 'date' | 'month'
  onChange?: (event: ChangeEvent<HTMLInputElement>) => void
  value?: string
}

const monthNames = ['Janeiro', 'Fevereiro', 'Março', 'Abril', 'Maio', 'Junho', 'Julho', 'Agosto', 'Setembro', 'Outubro', 'Novembro', 'Dezembro']

function formatDateValue(value: string, mode: 'date' | 'month') {
  if (!value) return mode === 'date' ? 'DD/MM/AAAA' : 'Mês e ano'

  if (mode === 'month') {
    const [year, month] = value.split('-')
    return `${monthNames[Number(month) - 1]} de ${year}`
  }

  const [year, month, day] = value.split('-')
  return `${day}/${month}/${year}`
}

function parseDateValue(value: string, mode: 'date' | 'month') {
  if (!value) return undefined
  const [year, month, day] = value.split('-').map(Number)
  if (!year || !month || (mode === 'date' && !day)) return undefined
  return new Date(year, month - 1, day ?? 1)
}

function toDateValue(date: Date, mode: 'date' | 'month') {
  const year = date.getFullYear()
  const month = String(date.getMonth() + 1).padStart(2, '0')
  if (mode === 'month') return `${year}-${month}`
  return `${year}-${month}-${String(date.getDate()).padStart(2, '0')}`
}

const weekDays = ['D', 'S', 'T', 'Q', 'Q', 'S', 'S']

export const DateInput = forwardRef<HTMLInputElement, DateInputProps>(function DateInput({ className, defaultValue = '', disabled = false, id, mode = 'date', name, onBlur, onChange, value, ...props }, ref) {
  const generatedId = useId()
  const inputId = id ?? generatedId
  const pickerId = `${inputId}-picker`
  const nativeDateRef = useRef<HTMLInputElement>(null)
  const [internalValue, setInternalValue] = useState(defaultValue)
  const selectedValue = value ?? internalValue
  const selectedDate = parseDateValue(selectedValue, mode)
  const [isOpen, setIsOpen] = useState(false)
  const [viewDate, setViewDate] = useState(() => selectedDate ?? new Date())
  const currentDate = new Date()
  const viewYear = viewDate.getFullYear()
  const viewMonth = viewDate.getMonth()
  const daysInViewMonth = new Date(viewYear, viewMonth + 1, 0).getDate()
  const firstWeekDay = new Date(viewYear, viewMonth, 1).getDay()
  const calendarDays = useMemo(() => Array.from({ length: 42 }, (_, index) => {
    const day = index - firstWeekDay + 1
    return day > 0 && day <= daysInViewMonth ? day : undefined
  }), [daysInViewMonth, firstWeekDay])

  function changeValue(nextValue: string) {
    if (value === undefined) setInternalValue(nextValue)
    if (nativeDateRef.current) {
      setNativeValue(nativeDateRef.current, nextValue)
      nativeDateRef.current.dispatchEvent(new Event('input', { bubbles: true }))
    }
  }

  function selectDate(date: Date) {
    changeValue(toDateValue(date, mode))
    setViewDate(date)
    setIsOpen(false)
  }

  function isUnavailable(date: Date) {
    const candidate = toDateValue(date, mode)
    return Boolean((props.min && candidate < props.min) || (props.max && candidate > props.max))
  }

  return (
    <span className={classes('ui-date-input', isOpen && 'ui-date-input--open')} onBlur={(event) => {
      if (event.currentTarget.contains(event.relatedTarget)) return
      setIsOpen(false)
      onBlur?.(event as unknown as FocusEvent<HTMLInputElement>)
    }}>
      <input aria-hidden="true" className="ui-date-input__native" disabled={disabled} name={name} onChange={onChange} ref={(element) => {
        nativeDateRef.current = element
        assignForwardedRef(ref, element)
      }} tabIndex={-1} type="text" value={selectedValue} {...props} />
      <button
        aria-controls={pickerId}
        aria-expanded={isOpen}
        aria-haspopup="dialog"
        className={classes('ui-date-input__trigger', className)}
        disabled={disabled}
        id={inputId}
        onClick={() => setIsOpen((open) => !open)}
        onKeyDown={(event) => {
          if (event.key === 'Escape') setIsOpen(false)
        }}
        type="button"
      >
        <span>{formatDateValue(selectedValue, mode)}</span>
        <span aria-hidden="true" className="ui-date-input__indicator">▦</span>
      </button>
      {isOpen && !disabled && (
        <div aria-label={mode === 'date' ? 'Selecionar data' : 'Selecionar mês'} className="ui-date-picker" id={pickerId} onKeyDown={(event) => {
          if (event.key === 'Escape') setIsOpen(false)
        }} role="dialog">
          <div className="ui-date-picker__header">
            <button aria-label={mode === 'date' ? 'Mês anterior' : 'Ano anterior'} className="ui-date-picker__navigation" onClick={() => setViewDate(new Date(viewYear, viewMonth - (mode === 'date' ? 1 : 12), 1))} type="button">‹</button>
            <strong>{mode === 'date' ? `${monthNames[viewMonth]} de ${viewYear}` : viewYear}</strong>
            <button aria-label={mode === 'date' ? 'Próximo mês' : 'Próximo ano'} className="ui-date-picker__navigation" onClick={() => setViewDate(new Date(viewYear, viewMonth + (mode === 'date' ? 1 : 12), 1))} type="button">›</button>
          </div>
          {mode === 'date' ? (
            <div className="ui-date-picker__calendar">
              {weekDays.map((weekDay, index) => <span aria-label={['Domingo', 'Segunda-feira', 'Terça-feira', 'Quarta-feira', 'Quinta-feira', 'Sexta-feira', 'Sábado'][index]} className="ui-date-picker__weekday" key={`${weekDay}-${index}`}>{weekDay}</span>)}
              {calendarDays.map((day, index) => {
                if (!day) return <span aria-hidden="true" className="ui-date-picker__day-placeholder" key={`empty-${index}`} />
                const date = new Date(viewYear, viewMonth, day)
                const dateValue = toDateValue(date, mode)
                const isSelected = dateValue === selectedValue
                const isToday = dateValue === toDateValue(currentDate, mode)
                return <button aria-label={`${day} de ${monthNames[viewMonth]} de ${viewYear}`} aria-pressed={isSelected} className="ui-date-picker__day" data-selected={isSelected} data-today={isToday} disabled={isUnavailable(date)} key={dateValue} onClick={() => selectDate(date)} type="button">{day}</button>
              })}
            </div>
          ) : (
            <div className="ui-date-picker__months">
              {monthNames.map((monthName, month) => {
                const date = new Date(viewYear, month, 1)
                const monthValue = toDateValue(date, mode)
                return <button aria-pressed={monthValue === selectedValue} data-selected={monthValue === selectedValue} disabled={isUnavailable(date)} key={monthName} onClick={() => selectDate(date)} type="button">{monthName.slice(0, 3)}</button>
              })}
            </div>
          )}
          <div className="ui-date-picker__footer">
            <button className="ui-date-picker__action" onClick={() => changeValue('')} type="button">Limpar</button>
            <button className="ui-date-picker__action" onClick={() => selectDate(currentDate)} type="button">Hoje</button>
          </div>
        </div>
      )}
    </span>
  )
})

export type AutocompleteOption = { description?: string; label: string; value: string }

type AutocompleteSelectProps = {
  disabled?: boolean
  emptyMessage?: string
  id?: string
  invalid?: boolean
  onValueChange: (value: string) => void
  options: AutocompleteOption[]
  placeholder?: string
  value: string
}

export function AutocompleteSelect({ disabled = false, emptyMessage = 'Nenhuma opção encontrada.', id, invalid = false, onValueChange, options, placeholder = 'Pesquisar e selecionar…', value }: AutocompleteSelectProps) {
  const generatedId = useId()
  const inputId = id ?? generatedId
  const listId = `${inputId}-options`
  const [isOpen, setIsOpen] = useState(false)
  const [query, setQuery] = useState('')
  const selectedOption = options.find((option) => option.value === value)
  const filteredOptions = useMemo(() => {
    const normalizedQuery = query.trim().toLocaleLowerCase('pt-BR')
    return normalizedQuery
      ? options.filter((option) => `${option.label} ${option.description ?? ''}`.toLocaleLowerCase('pt-BR').includes(normalizedQuery))
      : options
  }, [options, query])

  function selectOption(option: AutocompleteOption) {
    onValueChange(option.value)
    setQuery(option.label)
    setIsOpen(false)
  }

  function handleKeyDown(event: KeyboardEvent<HTMLInputElement>) {
    if (event.key === 'ArrowDown') {
      event.preventDefault()
      setIsOpen(true)
    }

    if (event.key === 'Enter' && filteredOptions[0]) {
      event.preventDefault()
      selectOption(filteredOptions[0])
    }

    if (event.key === 'Escape') {
      setIsOpen(false)
    }
  }

  return (
    <div className="ui-autocomplete" onBlur={(event) => {
      if (!event.currentTarget.contains(event.relatedTarget)) setIsOpen(false)
    }}>
      <input
        aria-autocomplete="list"
        aria-controls={listId}
        aria-expanded={isOpen}
        aria-invalid={invalid}
        className={classes('ui-input', 'ui-autocomplete__input', invalid && 'ui-input--invalid')}
        disabled={disabled}
        id={inputId}
        onChange={(event) => {
          setQuery(event.target.value)
          setIsOpen(true)
        }}
        onFocus={() => {
          setQuery('')
          setIsOpen(true)
        }}
        onKeyDown={handleKeyDown}
        placeholder={placeholder}
        role="combobox"
        type="text"
        value={isOpen ? query : selectedOption?.label ?? ''}
      />
      <span aria-hidden="true" className="ui-autocomplete__indicator">⌕</span>
      {isOpen && !disabled && (
        <ul className="ui-autocomplete__menu" id={listId} role="listbox">
          {filteredOptions.length > 0 ? filteredOptions.map((option) => (
            <li aria-selected={option.value === value} key={option.value} role="option">
              <button onClick={() => selectOption(option)} onMouseDown={(event) => event.preventDefault()} type="button">
                <span>{option.label}</span>
                {option.description && <small>{option.description}</small>}
              </button>
            </li>
          )) : <li className="ui-autocomplete__empty">{emptyMessage}</li>}
        </ul>
      )}
    </div>
  )
}

export function MultiAutocompleteSelect({ id, onValuesChange, options, placeholder = 'Filtrar opções…', values }: { id?: string; onValuesChange: (values: string[]) => void; options: AutocompleteOption[]; placeholder?: string; values: string[] }) {
  const generatedId = useId()
  const inputId = id ?? generatedId
  const [query, setQuery] = useState('')
  const [isOpen, setIsOpen] = useState(false)
  const filteredOptions = useMemo(() => options.filter((option) => !values.includes(option.value) && `${option.label} ${option.description ?? ''}`.toLocaleLowerCase('pt-BR').includes(query.trim().toLocaleLowerCase('pt-BR'))), [options, query, values])
  const selectedOptions = options.filter((option) => values.includes(option.value))
  return <div className="ui-multi-autocomplete" onBlur={(event) => { if (!event.currentTarget.contains(event.relatedTarget)) setIsOpen(false) }}><div className="ui-multi-autocomplete__control">{selectedOptions.map((option) => <button aria-label={`Remover ${option.label}`} className="ui-multi-autocomplete__tag" key={option.value} onClick={() => onValuesChange(values.filter((value) => value !== option.value))} type="button">{option.label} ×</button>)}<input aria-controls={`${inputId}-options`} aria-expanded={isOpen} aria-label="Filtrar opções" className="ui-multi-autocomplete__input" id={inputId} onChange={(event) => { setQuery(event.target.value); setIsOpen(true) }} onFocus={() => setIsOpen(true)} placeholder={selectedOptions.length ? '' : placeholder} role="combobox" value={query} /></div>{isOpen && <ul className="ui-autocomplete__menu" id={`${inputId}-options`} role="listbox">{filteredOptions.length ? filteredOptions.map((option) => <li key={option.value} role="option"><button onClick={() => { onValuesChange([...values, option.value]); setQuery('') }} type="button"><span>{option.label}</span>{option.description && <small>{option.description}</small>}</button></li>) : <li className="ui-autocomplete__empty">Nenhuma opção encontrada.</li>}</ul>}</div>
}

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
