import type { PlayerPosition } from './profile-avatar'
import { Icon } from './icons'
import { classes } from './utils'
const positionIcons = { goalkeeper: 'luva', defender: 'caneleira', midfielder: 'camisa', attacker: 'bola' } as const
export function MenuAvatar({ imageUrl, name = 'Usuário', position = 'midfielder', size = 'sm' }: { imageUrl?: string; name?: string; position?: PlayerPosition; size?: 'sm' | 'md' }) { return <div aria-label={`${name}, ${position}`} className={classes('ui-menu-avatar', `ui-menu-avatar--${size}`)}><div className="ui-menu-avatar__photo">{imageUrl ? <img alt="" src={imageUrl} /> : <span aria-hidden="true">{name.slice(0, 2).toUpperCase()}</span>}</div><span aria-hidden="true" className="ui-menu-avatar__position"><Icon name={positionIcons[position]} size={size === 'sm' ? 12 : 16} /></span></div> }
