import { useState } from 'react'
import type { IconName } from './icons'
import { Icon } from './icons'
import { classes } from './utils'

export type PlayerPosition = 'goalkeeper' | 'defender' | 'midfielder' | 'attacker'
const positions: Array<{ icon: IconName; label: string; value: PlayerPosition }> = [
  { value: 'goalkeeper', label: 'Goleiro', icon: 'luva' }, { value: 'defender', label: 'Zagueiro', icon: 'caneleira' }, { value: 'midfielder', label: 'Meio-campo', icon: 'camisa' }, { value: 'attacker', label: 'Ataque', icon: 'bola' },
]

export function ProfileAvatar({ alt = '', imageUrl, name = 'Usuário', onPositionChange, position = 'midfielder', size = 'md' }: { alt?: string; imageUrl?: string; name?: string; onPositionChange?: (position: PlayerPosition) => void; position?: PlayerPosition; size?: 'sm' | 'md' | 'lg' }) {
  const [open, setOpen] = useState(false)
  const current = positions.find((item) => item.value === position) ?? positions[2]
  return <div className={classes('ui-profile-avatar', `ui-profile-avatar--${size}`)}><div className="ui-profile-avatar__photo">{imageUrl ? <img alt={alt || name} src={imageUrl} /> : <span aria-hidden="true">{name.slice(0, 2).toUpperCase()}</span>}</div><button aria-expanded={open} aria-label={`Posição: ${current.label}. Alterar posição`} className="ui-profile-avatar__position" onClick={() => setOpen(!open)} type="button"><Icon name={current.icon} size={size === 'sm' ? 13 : 16} /></button>{open && <div className="ui-profile-avatar__options" role="menu">{positions.map((item) => <button aria-label={item.label} aria-pressed={item.value === position} key={item.value} onClick={() => { onPositionChange?.(item.value); setOpen(false) }} role="menuitem" type="button"><Icon name={item.icon} size={16} /><span>{item.label}</span></button>)}</div>}</div>
}
