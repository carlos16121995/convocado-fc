import { zodResolver } from '@hookform/resolvers/zod'
import { useMemo, useState } from 'react'
import { useForm } from 'react-hook-form'
import { z } from 'zod'
import {
  Accordion,
  Alert,
  AutocompleteSelect,
  Badge,
  Button,
  Card,
  CardHeader,
  CardTitle,
  Checkbox,
  Dialog,
  DateInput,
  Dropdown,
  DropdownItem,
  EmptyState,
  ErrorState,
  FieldMessage,
  Icon,
  ImageCarousel,
  Input,
  InlineConfirmation,
  JerseyIcon,
  Label,
  MultiAutocompleteSelect,
  LoadingState,
  Pagination,
  ProfileAvatar,
  MenuAvatar,
  RadioGroup,
  Select,
  Sidebar,
  Skeleton,
  Spinner,
  Switch,
  Table,
  Textarea,
  Toast,
  Tooltip,
  useTheme,
} from '../../components/ui'
import type { IconName, Theme } from '../../components/ui'
import './DesignSystemPage.css'

const colorTokens = [
  ['background', 'Plano de fundo da aplicação.'],
  ['surface', 'Superfícies elevadas, como cards e menus.'],
  ['surface-hover', 'Estado hover de superfícies interativas.'],
  ['surface-active', 'Estado ativo de superfícies interativas.'],
  ['text', 'Texto principal e títulos.'],
  ['text-secondary', 'Texto de apoio e conteúdo secundário.'],
  ['text-muted', 'Metadados e conteúdo de menor ênfase.'],
  ['border', 'Divisores e bordas padrão.'],
  ['border-strong', 'Bordas com ênfase e controles.'],
  ['primary', 'Ações principais e marca.'],
  ['secondary', 'Ações complementares.'],
  ['success', 'Sucesso: resultado positivo, distinto da cor de marca.'],
  ['warning', 'Aviso: atenção necessária, em amarelo.'],
  ['alert', 'Alerta: situação prioritária, em laranja.'],
  ['danger', 'Erro: falha ou ação destrutiva, em vermelho.'],
  ['processing', 'Processando: operação em andamento, em azul.'],
  ['info', 'Informação contextual; compatível com processing.'],
  ['focus', 'Indicador de foco acessível.'],
] as const

const iconGroups: Array<{ description: string; icons: Array<{ label: string; name: IconName }>; title: string }> = [
  {
    title: 'Financeiro',
    description: 'Cobrança, saldo e indicadores financeiros.',
    icons: [
      { name: 'mensalidades', label: 'Mensalidades' }, { name: 'financeiro', label: 'Financeiro' }, { name: 'dinheiro', label: 'Dinheiro' }, { name: 'investimento', label: 'Investimento' }, { name: 'decadencia', label: 'Decadência' }, { name: 'falido', label: 'Falido' },
    ],
  },
  {
    title: 'Pessoas e operação',
    description: 'Identidades, equipes e visão da operação.',
    icons: [
      { name: 'usuario', label: 'Usuário' }, { name: 'usuarios', label: 'Usuários' }, { name: 'times', label: 'Times' }, { name: 'relatorios', label: 'Relatórios' }, { name: 'partidas', label: 'Partidas' }, { name: 'campo', label: 'Campo de futebol' },
    ],
  },
  {
    title: 'Ações e planejamento',
    description: 'Navegação, gestão e ações sobre registros.',
    icons: [
      { name: 'pesquisar', label: 'Pesquisar' }, { name: 'gerenciar', label: 'Gerenciar' }, { name: 'objetivos', label: 'Objetivos' }, { name: 'metas', label: 'Metas' }, { name: 'adicionar', label: 'Adicionar' }, { name: 'remover', label: 'Remover' }, { name: 'excluir', label: 'Excluir' }, { name: 'separar', label: 'Separar' },
    ],
  },
  {
    title: 'Futebol e viagem',
    description: 'Itens de campo, uniforme e deslocamento.',
    icons: [
      { name: 'bola', label: 'Bola de futebol' }, { name: 'cone', label: 'Cone de trânsito' }, { name: 'luva', label: 'Luva de goleiro' }, { name: 'chuteira', label: 'Chuteira com travas' }, { name: 'caneleira', label: 'Caneleira / escudo' }, { name: 'viagem', label: 'Viagem' },
    ],
  },
  {
    title: 'Campo e infraestrutura',
    description: 'Estrutura, conservação e acompanhamento da partida.',
    icons: [
      { name: 'grama', label: 'Grama' }, { name: 'traves', label: 'Traves' }, { name: 'placar', label: 'Placar' }, { name: 'trator', label: 'Trator' }, { name: 'manutencao', label: 'Manutenção' },
    ],
  },
  {
    title: 'Convocação',
    description: 'Etapas de chamada, confirmação e organização da equipe.',
    icons: [
      { name: 'chamada', label: 'Chamada' }, { name: 'presenca', label: 'Presença' }, { name: 'escalacao', label: 'Escalação' },
    ],
  },
  {
    title: 'Ambiente e apoio',
    description: 'Condições externas e conteúdos de suporte.',
    icons: [
      { name: 'chuva', label: 'Chuva' }, { name: 'sol', label: 'Sol' }, { name: 'nuvem', label: 'Nuvem' }, { name: 'regras', label: 'Regras' }, { name: 'informacoes', label: 'Informações' },
    ],
  },
]

const formSchema = z.object({
  category: z.string().min(1, 'Selecione uma categoria.'),
  name: z.string().min(3, 'Informe pelo menos 3 caracteres.'),
  notifications: z.boolean(),
  terms: z.boolean().refine((value) => value, 'Confirme que os dados estão corretos.'),
})

type ExampleForm = z.infer<typeof formSchema>
type ToastTone = 'success' | 'warning' | 'alert' | 'danger' | 'info'

function Section({ children, description, id, title }: { children: React.ReactNode; description: string; id: string; title: string }) {
  return (
    <section className="ds-section" id={id}>
      <header className="ds-section__header">
        <h2>{title}</h2>
        <p>{description}</p>
      </header>
      {children}
    </section>
  )
}

function ThemeSelector() {
  const { setTheme, theme } = useTheme()
  const themes: Array<[Theme, string]> = [['light', 'Light'], ['dark', 'Dark'], ['system', 'System']]

  return (
    <div aria-label="Tema" className="ds-theme-selector" role="group">
      {themes.map(([value, label]) => (
        <Button aria-pressed={theme === value} key={value} onClick={() => setTheme(value)} variant={theme === value ? 'primary' : 'ghost'}>
          {label}
        </Button>
      ))}
    </div>
  )
}

function Colors({ visible }: { visible: boolean }) {
  if (!visible) return null
  return (
    <Section description="Tokens semânticos. A intenção, e não um valor de cor, é consumida pelos componentes." id="colors" title="Colors">
      <div className="ds-token-grid">
        {colorTokens.map(([token, purpose]) => (
          <Card className="ds-token-card" key={token}>
            <div aria-label={`Token ${token}`} className="ds-token-card__swatch" style={{ backgroundColor: `var(--color-${token})` }} />
            <strong>{token}</strong>
            <span>{purpose}</span>
          </Card>
        ))}
      </div>
    </Section>
  )
}

function Iconography({ query, visible }: { query: string; visible: boolean }) {
  const [shirtNumber, setShirtNumber] = useState(10)
  if (!visible) return null

  return (
    <Section description="Ícones vetoriais públicos para navegação, estados e ações. Todos usam currentColor e, portanto, respeitam o tema por meio dos tokens." id="icons" title="Icons">
      <div className="ds-icon-groups">
        {iconGroups.map((group) => {
          const icons = group.icons.filter((icon) => !query || `${icon.label} ${icon.name} ${group.title} ${group.description}`.toLocaleLowerCase().includes(query))
          if (query && icons.length === 0) return null
          return (
          <div className="ds-icon-group" key={group.title}>
            <div><h3>{group.title}</h3><p>{group.description}</p></div>
            <div className="ds-icon-grid">
              {icons.map((icon) => (
                <Card className="ds-icon-card" key={icon.name}>
                  <Icon aria-hidden="true" name={icon.name} size={28} strokeWidth={1.8} />
                  <strong>{icon.label}</strong>
                  <code>{icon.name}</code>
                </Card>
              ))}
            </div>
          </div>
          )
        })}
        <Card className="ds-jersey-card">
          <div><h3>Camisa numerada</h3><p>Variação pública para a parte traseira do uniforme, com número centralizado de 1 a 999.</p></div>
          <div aria-label="Prévia de camisas numeradas" className="ds-jersey-samples">
            {[1, 10, 99, 999].map((number) => <span key={number}><JerseyIcon aria-hidden="true" number={number} /><small>{number}</small></span>)}
          </div>
          <div className="ds-jersey-control"><Label htmlFor="shirt-number">Prévia personalizada</Label><Input id="shirt-number" max={999} min={1} onChange={(event) => setShirtNumber(Math.min(999, Math.max(1, Number(event.target.value) || 1)))} type="number" value={shirtNumber} /><JerseyIcon aria-label={`Camisa número ${shirtNumber}`} number={shirtNumber} /></div>
        </Card>
      </div>
    </Section>
  )
}

function Typography({ visible }: { visible: boolean }) {
  if (!visible) return null
  return (
    <Section description="Escala tipográfica, pesos e conteúdos auxiliares." id="typography" title="Typography">
      <Card className="ds-typography">
        <h1>Heading H1</h1><h2>Heading H2</h2><h3>Heading H3</h3><h4>Heading H4</h4><h5>Heading H5</h5><h6>Heading H6</h6>
        <p>Texto de corpo para leitura de conteúdo e explicações detalhadas.</p>
        <small>Texto pequeno para detalhes complementares.</small>
        <p className="ds-muted">Texto muted para metadados e baixa ênfase.</p>
        <Label>Label de campo</Label>
        <span className="ds-caption">Caption: atualizado há poucos minutos</span>
        <a href="#components">Link de exemplo</a>
        <p><span className="ds-regular">Regular</span> · <span className="ds-semibold">Semibold</span> · <strong>Bold</strong></p>
      </Card>
    </Section>
  )
}

function ProfileAvatarExample({ visible }: { visible: boolean }) {
  const [position, setPosition] = useState<'goalkeeper' | 'defender' | 'midfielder' | 'attacker'>('midfielder')
  if (!visible) return null
  return <Section description="Foto circular com a posição do atleta sobreposta e seletor compacto para trocar rapidamente entre as funções em campo." id="profile-avatar" title="Profile avatar"><Card className="ds-avatar-showcase"><ProfileAvatar name="Mariana Souza" position={position} onPositionChange={setPosition} size="lg" /><div><h3>ProfileAvatar</h3><p>Abra o ícone no canto da foto para selecionar outra posição.</p><div className="ds-avatar-positions">{['goalkeeper', 'defender', 'midfielder', 'attacker'].map((item) => <Badge key={item} tone={position === item ? 'primary' : 'neutral'}>{item === 'goalkeeper' ? 'Goleiro' : item === 'defender' ? 'Zagueiro' : item === 'midfielder' ? 'Meio-campo' : 'Ataque'}</Badge>)}</div></div><div className="ds-menu-avatar-example"><MenuAvatar name="Mariana Souza" position={position} size="md" /><div><h3>MenuAvatar</h3><p>Versão compacta e não editável para menus, chats e posts.</p></div></div></Card></Section>
}

function ButtonsAndInputs({ visible = true }: { visible?: boolean }) {
  const [radio, setRadio] = useState('standard')
  const [selectedTeam, setSelectedTeam] = useState('')
  const [selectedTeams, setSelectedTeams] = useState<string[]>([])
  const [switchOn, setSwitchOn] = useState(true)

  if (!visible) return null
  return (
    <Section description="Variantes, estados e controles de entrada disponíveis publicamente." id="components" title="Components">
      <div className="ds-component-stack" id="components">
        <Card>
          <CardHeader><CardTitle>Button</CardTitle><Badge tone="primary">Variantes</Badge></CardHeader>
          <div className="ds-inline-samples">
            <Button>Primary</Button><Button variant="secondary">Secondary</Button><Button variant="outline">Outline</Button><Button variant="ghost">Ghost</Button><Button variant="destructive">Destructive</Button>
            <Button loading>Loading</Button><Button disabled>Disabled</Button><Button className="ds-focus-preview" variant="outline">Focus</Button>
          </div>
          <FieldMessage>Hover, clique e use Tab para validar os estados interativos e o foco.</FieldMessage>
        </Card>

        <div className="ds-card-variants">
          <Card><CardTitle>Card padrão</CardTitle><FieldMessage>Superfície neutra para conteúdo geral.</FieldMessage></Card>
          <Card tone="success"><CardTitle>Card success</CardTitle><FieldMessage>Resultado positivo ou estado concluído.</FieldMessage></Card>
          <Card tone="warning"><CardTitle>Card warning</CardTitle><FieldMessage>Conteúdo que requer atenção.</FieldMessage></Card>
        </div>

        <div className="ds-two-columns">
          <Card>
            <CardHeader><CardTitle>Inputs</CardTitle></CardHeader>
            <div className="ds-field-stack">
              <div><Label htmlFor="component-input">Input</Label><Input id="component-input" placeholder="Digite uma informação" /></div>
              <div><Label htmlFor="component-textarea">Textarea</Label><Textarea id="component-textarea" placeholder="Descreva uma observação" /></div>
              <div><Label htmlFor="component-select">Select</Label><Select defaultValue="" id="component-select"><option disabled value="">Selecione uma opção</option><option>Primeira opção</option><option>Segunda opção</option></Select></div>
              <div><Label htmlFor="component-autocomplete">Select com autocomplete</Label><AutocompleteSelect id="component-autocomplete" onValueChange={setSelectedTeam} options={[{ label: 'Atlético Central', value: 'atletico', description: 'Sub-17 · São Paulo' }, { label: 'Estrela do Norte', value: 'estrela', description: 'Adulto · Recife' }, { label: 'União Esportiva', value: 'uniao', description: 'Sub-20 · Curitiba' }]} value={selectedTeam} />{selectedTeam && <FieldMessage>Equipe selecionada com filtro por texto.</FieldMessage>}</div>
              <div><Label htmlFor="component-multi-autocomplete">Multi-select com autocomplete</Label><MultiAutocompleteSelect id="component-multi-autocomplete" onValuesChange={setSelectedTeams} options={[{ label: 'Atlético Central', value: 'atletico' }, { label: 'Estrela do Norte', value: 'estrela' }, { label: 'União Esportiva', value: 'uniao' }]} values={selectedTeams} />{selectedTeams.length > 0 && <FieldMessage>{selectedTeams.length} equipe(s) selecionada(s).</FieldMessage>}</div>
              <Checkbox label="Checkbox selecionável" />
              <RadioGroup name="component-radio" onChange={setRadio} options={[{ label: 'Standard', value: 'standard' }, { label: 'Avançado', value: 'advanced' }]} value={radio} />
              <Switch checked={switchOn} label="Switch habilitado" onCheckedChange={setSwitchOn} />
            </div>
          </Card>

          <Card>
            <CardHeader><CardTitle>Overlay e feedback</CardTitle></CardHeader>
            <div className="ds-field-stack">
              <Tooltip content="Descrição adicional exibida por foco ou hover; informações essenciais devem permanecer visíveis."><Button variant="outline">Tooltip</Button></Tooltip>
              <Dropdown label="Abrir dropdown"><DropdownItem>Editar</DropdownItem><DropdownItem>Duplicar</DropdownItem><DropdownItem>Arquivar</DropdownItem></Dropdown>
              <Skeleton className="ds-skeleton-line" /><Skeleton className="ds-skeleton-line ds-skeleton-line--short" />
              <span className="ds-spinner-sample"><Spinner size="lg" tone="processing" /> Processando</span>
            </div>
          </Card>
        </div>

        <Card>
          <CardHeader><CardTitle>Accordion</CardTitle><Badge tone="neutral">Expansível</Badge></CardHeader>
          <Accordion items={[
            { id: 'tokens', title: 'Como os tokens são aplicados?', content: 'Todos os componentes consomem tokens semânticos para se adaptar automaticamente aos temas Light, Dark e System.' },
            { id: 'a11y', title: 'Como validar acessibilidade?', content: 'Navegue com Tab e utilize os controles nativos ou ARIA para verificar foco, expansão e mensagens de estado.' },
            { id: 'mobile', title: 'Como funciona em telas menores?', content: 'O conteúdo se reorganiza verticalmente e tabelas preservam a leitura por meio de rolagem horizontal localizada.' },
          ]} />
        </Card>

        <InlineConfirmationExample />
        <Card><CardHeader><CardTitle>Carrossel de imagens</CardTitle></CardHeader><ImageCarousel slides={[{ title: 'Treino intenso', alt: 'Jogadores treinando em campo', image: 'https://images.unsplash.com/photo-1579952363873-27f3bade9f55?auto=format&fit=crop&w=1000&q=80' }, { title: 'Dia de jogo', alt: 'Campo de futebol iluminado', image: 'https://images.unsplash.com/photo-1526232761682-d26e03ac148e?auto=format&fit=crop&w=1000&q=80' }, { title: 'Trabalho em equipe', alt: 'Bola de futebol no gramado', image: 'https://images.unsplash.com/photo-1553778263-73a83bab9b0c?auto=format&fit=crop&w=1000&q=80' }]} /></Card>
      </div>
    </Section>
  )
}

function InlineConfirmationExample() {
  const [isConfirming, setIsConfirming] = useState(true)
  const [wasDeleted, setWasDeleted] = useState(false)

  return (
    <Card>
      <CardHeader><CardTitle>Confirmação inline</CardTitle><Badge tone="alert">Sem modal</Badge></CardHeader>
      {isConfirming ? <InlineConfirmation cancelLabel="Cancelar" confirmLabel="Excluir" description="A equipe será removida da lista atual. Esta confirmação continua fazendo parte do fluxo da tela." onCancel={() => setIsConfirming(false)} onConfirm={() => { setWasDeleted(true); setIsConfirming(false) }} title="Tem certeza que deseja excluir?" /> : <div className="ds-inline-samples"><Button onClick={() => { setWasDeleted(false); setIsConfirming(true) }} variant="outline">Exibir confirmação</Button>{wasDeleted && <Badge tone="success">Exclusão confirmada</Badge>}</div>}
    </Card>
  )
}

function FormExample({ visible = true }: { visible?: boolean }) {
  const [submitted, setSubmitted] = useState(false)
  const form = useForm<ExampleForm>({
    defaultValues: { category: '', name: '', notifications: true, terms: false },
    resolver: zodResolver(formSchema),
  })
  const notifications = form.watch('notifications')

  if (!visible) return null
  return (
    <Section description="Um fluxo realista com ajuda, validação, erro e campo indisponível." id="forms" title="Forms">
      <Card className="ds-form-card">
        <form noValidate onSubmit={form.handleSubmit(() => setSubmitted(true))}>
          <div className="ds-field-stack">
            <div>
              <Label htmlFor="form-name">Nome da equipe</Label>
              <Input aria-describedby="form-name-help" id="form-name" invalid={Boolean(form.formState.errors.name)} placeholder="Ex.: Convocado FC" {...form.register('name')} />
              <FieldMessage>{form.formState.errors.name?.message ?? 'Use o nome que aparecerá nas comunicações.'}</FieldMessage>
            </div>
            <div>
              <Label htmlFor="form-category">Categoria</Label>
              <Select aria-invalid={Boolean(form.formState.errors.category)} id="form-category" invalid={Boolean(form.formState.errors.category)} {...form.register('category')}>
                <option value="">Selecione uma categoria</option><option value="base">Categorias de base</option><option value="adulto">Adulto</option>
              </Select>
              {form.formState.errors.category && <FieldMessage tone="error">{form.formState.errors.category.message}</FieldMessage>}
            </div>
            <Checkbox label="Os dados da equipe estão corretos" {...form.register('terms')} />
            {form.formState.errors.terms && <FieldMessage tone="error">{form.formState.errors.terms.message}</FieldMessage>}
            <Switch checked={notifications} label="Receber atualizações por e-mail" onCheckedChange={(checked) => form.setValue('notifications', checked)} />
            <div><Label htmlFor="form-disabled">Identificador gerado</Label><Input disabled id="form-disabled" value="Gerado ao salvar" readOnly /><FieldMessage>Este valor só fica disponível após o cadastro.</FieldMessage></div>
            <div className="ds-inline-samples"><Button type="submit">Salvar demonstração</Button>{submitted && <Badge tone="success">Validação concluída</Badge>}</div>
          </div>
        </form>
      </Card>
    </Section>
  )
}

function DataExamples({ visible = true }: { visible?: boolean }) {
  const [page, setPage] = useState(1)
  const rows = [
    { id: 'atletico', cells: { name: 'Atlético Central', status: <Badge tone="success">Ativo</Badge>, updated: 'Hoje' }, details: <p><strong>Responsável:</strong> Mariana Souza · <strong>Categoria:</strong> Sub-17 · <strong>Última atividade:</strong> treino confirmado.</p>, sortValues: { name: 'Atlético Central', status: 'Ativo', updated: '2026-09-17' } },
    { id: 'estrela', cells: { name: 'Estrela do Norte', status: <Badge tone="warning">Pendente</Badge>, updated: 'Ontem' }, details: <p><strong>Responsável:</strong> Rafael Lima · <strong>Categoria:</strong> Adulto · <strong>Pendência:</strong> validar documentação.</p>, sortValues: { name: 'Estrela do Norte', status: 'Pendente', updated: '2026-09-16' } },
  ]

  if (!visible) return null
  return (
    <Section description="Estados frequentes na apresentação de listas e resultados. Clique nos títulos para ordenar ou na seta para expandir os detalhes sem sair da tela." id="data" title="Data">
      <div className="ds-component-stack">
        <Card><Table caption="Equipes cadastradas" columns={[{ key: 'name', label: 'Equipe', sortable: true }, { key: 'status', label: 'Status', sortable: true }, { key: 'updated', label: 'Atualização', sortable: true }]} expandable rows={rows} /><div className="ds-data-footer"><Pagination currentPage={page} onPageChange={setPage} totalPages={3} /></div></Card>
        <div className="ds-state-grid">
          <EmptyState action={<Button variant="outline">Criar equipe</Button>} title="Nenhuma equipe encontrada">Altere os filtros ou crie o primeiro registro.</EmptyState>
          <LoadingState>Carregando tabela e seus indicadores…</LoadingState>
          <ErrorState action={<Button variant="outline">Tentar novamente</Button>}>Verifique sua conexão e tente de novo.</ErrorState>
        </div>
      </div>
    </Section>
  )
}

function DateExamples({ visible = true }: { visible?: boolean }) {
  if (!visible) return null
  return (
    <Section description="Campos de data apoiados pelos controles públicos de Input e Select." id="date-fields" title="Date fields">
      <div className="ds-date-grid">
        <Card><CardHeader><CardTitle>Data padrão</CardTitle></CardHeader><Label htmlFor="date-standard">Dia, mês e ano</Label><DateInput defaultValue="2026-09-17" id="date-standard" /><FieldMessage>Use para uma data específica.</FieldMessage></Card>
        <Card><CardHeader><CardTitle>Dia da semana e recorrência</CardTitle></CardHeader><div className="ds-field-stack"><div><Label htmlFor="date-weekday">Dia da semana</Label><Select defaultValue="monday" id="date-weekday"><option value="monday">Segunda-feira</option><option value="wednesday">Quarta-feira</option><option value="friday">Sexta-feira</option></Select></div><div><Label htmlFor="date-recurrence">Recorrência</Label><Select defaultValue="weekly" id="date-recurrence"><option value="weekly">Semanal</option><option value="biweekly">Quinzenal</option><option value="monthly">Mensal</option></Select></div></div></Card>
        <Card><CardHeader><CardTitle>Mês e ano</CardTitle></CardHeader><Label htmlFor="date-month">Competência</Label><DateInput defaultValue="2026-09" id="date-month" mode="month" /><FieldMessage>Use para filtros e períodos mensais.</FieldMessage></Card>
      </div>
    </Section>
  )
}

function Feedback({ visible = true }: { visible?: boolean }) {
  const [dialogOpen, setDialogOpen] = useState(false)
  const [toast, setToast] = useState<ToastTone | null>(null)
  const tones: ToastTone[] = ['success', 'warning', 'alert', 'danger', 'info']

  if (!visible) return null
  return (
    <Section description="Mensagens de sucesso, aviso, erro e informação em todos os componentes de feedback." id="feedback" title="Feedback">
      <div className="ds-component-stack">
        <div className="ds-alert-grid">{tones.map((tone) => <Alert key={tone} tone={tone}>Mensagem de {tone} exibida a partir do token semântico correspondente.</Alert>)}</div>
        <Card>
          <CardHeader><CardTitle>Badges e Toast</CardTitle></CardHeader>
          <div className="ds-inline-samples">{tones.map((tone) => <Badge key={tone} tone={tone}>{tone}</Badge>)}<Badge tone="processing">processing</Badge></div>
          <div className="ds-inline-samples ds-feedback-actions">
            {tones.map((tone) => <Button key={tone} onClick={() => setToast(tone)} variant="outline">Toast {tone}</Button>)}
            <Button onClick={() => setDialogOpen(true)} variant="secondary">Abrir dialog</Button>
          </div>
          {toast && <div className="ds-toast-region"><Toast onDismiss={() => setToast(null)} tone={toast}>Toast de {toast} acionado com sucesso.</Toast></div>}
        </Card>
      </div>
      <Dialog onOpenChange={setDialogOpen} open={dialogOpen} title="Exemplo de diálogo">
        <p className="ds-dialog-copy">Este conteúdo é aberto pelo componente público de diálogo e respeita os mesmos tokens de superfície, texto e overlay.</p>
        <Button onClick={() => setDialogOpen(false)}>Entendi</Button>
      </Dialog>
    </Section>
  )
}

export function DesignSystemPage() {
  const [search, setSearch] = useState('')
  const query = search.trim().toLocaleLowerCase()
  const topics = useMemo(() => [
    { id: 'colors', label: 'Colors', text: `colors tokens ${colorTokens.map(([token, purpose]) => `${token} ${purpose}`).join(' ')}` },
    { id: 'icons', label: 'Icons', text: `icons iconografia ${iconGroups.map((group) => `${group.title} ${group.description} ${group.icons.map((icon) => `${icon.label} ${icon.name}`).join(' ')}`).join(' ')}` },
    { id: 'typography', label: 'Typography', text: 'typography tipografia escala pesos' },
    { id: 'profile-avatar', label: 'Profile avatar', text: 'profile avatar foto perfil usuário goleiro zagueiro meio-campo ataque posição ícones' },
    { id: 'components', label: 'Components', text: 'components componentes button card inputs select accordion overlay feedback' },
    { id: 'forms', label: 'Forms', text: 'forms formulários validação equipe cadastro' },
    { id: 'date-fields', label: 'Date fields', text: 'date fields data calendário recorrência mês ano' },
    { id: 'data', label: 'Data', text: 'data tabela listas paginação estados equipes' },
    { id: 'feedback', label: 'Feedback', text: 'feedback mensagens sucesso aviso erro toast dialog badge' },
  ], [])
  const matches = (id: string) => !query || topics.find((topic) => topic.id === id)?.text.includes(query)
  const filteredTopics = topics.filter((topic) => matches(topic.id))

  return (
    <main className="design-system-page">
      <header className="ds-topbar">
        <div><p className="ds-eyebrow">Convocado FC</p><h1>Design System</h1></div>
        <ThemeSelector />
      </header>
      <div className="ds-layout">
        <div className="ds-sidebar-wrap"><Sidebar items={[{ id: 'foundations', label: 'Fundamentos', icon: 'gerenciar', children: filteredTopics.slice(0, 3).map((topic) => ({ id: topic.id, label: topic.label, icon: topic.id === 'icons' ? 'pesquisar' : 'regras', href: `#${topic.id}` })) }, ...filteredTopics.slice(3).map((topic) => ({ id: topic.id, label: topic.label, icon: 'informacoes' as const, href: `#${topic.id}` }))]} search={search} onSearch={setSearch} team="Convocado FC" user={{ name: 'Convidado', detail: 'Acesso público', guest: true }} onSwitchProfile={() => undefined} onSwitchTeam={() => undefined} onSignOut={() => undefined} /><span className="ds-results">{filteredTopics.length} de {topics.length} tópicos</span></div>
        <div className="ds-content">
        <p className="ds-intro">Documentação executável dos tokens, componentes, variantes e estados disponíveis. Redimensione a janela para validar a composição em celular, tablet e desktop.</p>
        {filteredTopics.length === 0 ? <Card><EmptyState title="Nenhuma correspondência encontrada">Tente buscar por outro tópico, componente ou ícone.</EmptyState></Card> : <><Colors visible={Boolean(matches('colors'))} /><Iconography query={query} visible={Boolean(matches('icons'))} /><Typography visible={Boolean(matches('typography'))} /><ProfileAvatarExample visible={Boolean(matches('profile-avatar'))} /><ButtonsAndInputs visible={Boolean(matches('components'))} /><FormExample visible={Boolean(matches('forms'))} /><DateExamples visible={Boolean(matches('date-fields'))} /><DataExamples visible={Boolean(matches('data'))} /><Feedback visible={Boolean(matches('feedback'))} /></>}
        </div>
      </div>
    </main>
  )
}
