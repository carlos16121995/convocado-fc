import {
  Banknote,
  BanknoteX,
  Calendar,
  CalendarDays,
  ChartLine,
  ChartNoAxesCombined,
  Cloud,
  CloudRain,
  ChevronDown,
  Goal,
  Info,
  LogIn,
  LogOut,
  ListChecks,
  Megaphone,
  Menu,
  Minus,
  Plane,
  Plus,
  ScrollText,
  Search,
  Settings,
  Shield,
  ShieldCheck,
  Shirt,
  Split,
  Sun,
  Target,
  Tractor,
  Trash2,
  TrendingDown,
  UserCheck,
  UserRound,
  UsersRound,
  WalletCards,
  Wrench,
  type LucideIcon,
  type LucideProps,
} from 'lucide-react'
import type { ReactNode, SVGProps } from 'react'

const iconMap = {
  mensalidades: CalendarDays,
  financeiro: WalletCards,
  usuario: UserRound,
  usuarios: UsersRound,
  times: Shield,
  relatorios: ChartNoAxesCombined,
  partidas: Calendar,
  campo: Goal,
  pesquisar: Search,
  gerenciar: Settings,
  objetivos: Target,
  metas: Goal,
  excluir: Trash2,
  adicionar: Plus,
  remover: Minus,
  separar: Split,
  dinheiro: Banknote,
  decadencia: TrendingDown,
  falido: BanknoteX,
  investimento: ChartLine,
  viagem: Plane,
  camisa: Shirt,
  chuva: CloudRain,
  sol: Sun,
  nuvem: Cloud,
  regras: ScrollText,
  informacoes: Info,
  menu: Menu,
  'chevron-down': ChevronDown,
  entrar: LogIn,
  sair: LogOut,
  trator: Tractor,
  manutencao: Wrench,
  chamada: Megaphone,
  presenca: UserCheck,
  escalacao: ListChecks,
  caneleira: ShieldCheck,
} satisfies Record<string, LucideIcon>

function CustomIconFrame({ absoluteStrokeWidth: _absoluteStrokeWidth, children, size = 24, strokeWidth = 2, ...props }: LucideProps & { children: ReactNode }) {
  return <svg fill="none" height={size} viewBox="0 0 64 64" width={size} {...props}><g fill="none" stroke="currentColor" strokeLinecap="round" strokeLinejoin="round" strokeWidth={strokeWidth}>{children}</g></svg>
}

function SoccerBallIcon(props: LucideProps) {
  return <CustomIconFrame {...props}>
    <circle cx="32" cy="32" r="25" />
    <path d="m32 21 8 6-3 9H27l-3-9 8-6Z" fill="currentColor" stroke="none" />
    <path d="M32 21v-8M40 27l9-4M37 36l5 10M27 36l-5 10M24 27l-9-4M13 23l-4 9 5 10 8 4M51 23l4 9-5 10-8 4M22 46l10 11 10-11" />
  </CustomIconFrame>
}

function FootballConeIcon(props: LucideProps) {
  return <CustomIconFrame {...props}>
    <path d="m24 10 16 0 13 38H11L24 10Z" />
    <path d="M16 28h32M13 38h38M7 51h50v5H7z" />
  </CustomIconFrame>
}

function GoalkeeperGloveIcon(props: LucideProps) {
  return <CustomIconFrame {...props}>
    <path d="M20 54c-6-4-9-10-9-18V24a4 4 0 0 1 8 0v8V12a4 4 0 0 1 8 0v18V9a4 4 0 0 1 8 0v21V14a4 4 0 0 1 8 0v19l5-8a4 4 0 0 1 7 4l-8 16c-4 8-10 11-18 11H20Z" />
    <path d="M19 38h24M24 46h17" />
  </CustomIconFrame>
}

function FootballBootIcon(props: LucideProps) {
  return <CustomIconFrame {...props}>
    <g transform="translate(5 5) scale(.84)">
      <path d="m11 27 11-9 8 17 12 4c6 1 10 5 11 11H10c-3 0-5-3-5-6 0-7 2-12 6-17Z" fill="currentColor" fillOpacity=".12" />
      <path d="m11 27 11-9 8 17 12 4c6 1 10 5 11 11H10c-3 0-5-3-5-6 0-7 2-12 6-17Z" />
      <path d="m18 25 10 5M20 21l9 5M10 50h43M16 50v5M29 50v5M42 50v5" />
    </g>
  </CustomIconFrame>
}

function GrassIcon(props: LucideProps) {
  return <CustomIconFrame {...props}>
    <path d="M8 52h48M13 52c0-7 2-13 5-18M18 52c0-6-1-12-4-16M24 52c0-8 3-15 7-20M30 52c0-6-1-12-3-17M37 52c0-8 3-15 7-20M43 52c0-6-1-12-3-17M50 52c0-7 2-13 5-18" />
  </CustomIconFrame>
}

function SoccerGoalsIcon(props: LucideProps) {
  return <CustomIconFrame {...props}>
    <path d="M14 44V20H50V44" strokeLinecap="butt" strokeLinejoin="miter" />
  </CustomIconFrame>
}

function ScoreboardIcon(props: LucideProps) {
  return <CustomIconFrame {...props}>
    <rect height="30" rx="3" width="50" x="7" y="12" />
    <path d="M32 12v30M17 42v11M47 42v11M20 20v14M17 23l3-3M39 20h7v14h-7Z" />
    <circle cx="32" cy="24" fill="currentColor" r="1.25" stroke="none" />
    <circle cx="32" cy="30" fill="currentColor" r="1.25" stroke="none" />
  </CustomIconFrame>
}

const customIconMap = {
  bola: SoccerBallIcon,
  cone: FootballConeIcon,
  luva: GoalkeeperGloveIcon,
  chuteira: FootballBootIcon,
  grama: GrassIcon,
  traves: SoccerGoalsIcon,
  placar: ScoreboardIcon,
} satisfies Record<string, (props: LucideProps) => ReactNode>

export type IconName = keyof typeof iconMap | keyof typeof customIconMap
type IconProps = LucideProps & { name: IconName }

export function Icon({ name, ...props }: IconProps) {
  const CustomIcon = customIconMap[name as keyof typeof customIconMap]
  if (CustomIcon) return <CustomIcon {...props} />

  const LucideIcon = iconMap[name as keyof typeof iconMap]
  return <LucideIcon {...props} />
}

type JerseyIconProps = Omit<SVGProps<SVGSVGElement>, 'children'> & { number: number }

export function JerseyIcon({ number, ...props }: JerseyIconProps) {
  const jerseyNumber = Math.min(999, Math.max(1, Math.trunc(number) || 1))

  return (
    <svg fill="none" viewBox="0 0 64 64" {...props}>
      <path d="M20 10 13 14 6 28l10 6 4-7v28h24V27l4 7 10-6-7-14-7-4-4 7H24l-4-7Z" stroke="currentColor" strokeLinecap="round" strokeLinejoin="round" strokeWidth="3" />
      <text fill="currentColor" fontFamily="inherit" fontSize={jerseyNumber > 99 ? '11' : jerseyNumber > 9 ? '14' : '18'} fontWeight="800" textAnchor="middle" x="32" y="42">{jerseyNumber}</text>
    </svg>
  )
}
