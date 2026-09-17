import { zodResolver } from '@hookform/resolvers/zod'
import { useState } from 'react'
import { useForm } from 'react-hook-form'
import { z } from 'zod'
import {
  Alert,
  Badge,
  Button,
  Card,
  CardHeader,
  CardTitle,
  Checkbox,
  Dialog,
  Dropdown,
  DropdownItem,
  EmptyState,
  ErrorState,
  FieldMessage,
  Input,
  Label,
  LoadingState,
  Pagination,
  RadioGroup,
  Select,
  Skeleton,
  Spinner,
  Switch,
  Table,
  Textarea,
  Toast,
  Tooltip,
  useTheme,
} from '../../components/ui'
import type { Theme } from '../../components/ui'
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

const formSchema = z.object({
  category: z.string().min(1, 'Selecione uma categoria.'),
  name: z.string().min(3, 'Informe pelo menos 3 caracteres.'),
  notifications: z.boolean(),
  terms: z.boolean().refine((value) => value, 'Confirme que os dados estão corretos.'),
})

type ExampleForm = z.infer<typeof formSchema>
type ToastTone = 'success' | 'warning' | 'alert' | 'danger' | 'info'

function Section({ children, description, title }: { children: React.ReactNode; description: string; title: string }) {
  return (
    <section className="ds-section">
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

function Colors() {
  return (
    <Section description="Tokens semânticos. A intenção, e não um valor de cor, é consumida pelos componentes." title="Colors">
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

function Typography() {
  return (
    <Section description="Escala tipográfica, pesos e conteúdos auxiliares." title="Typography">
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

function ButtonsAndInputs() {
  const [radio, setRadio] = useState('standard')
  const [switchOn, setSwitchOn] = useState(true)

  return (
    <Section description="Variantes, estados e controles de entrada disponíveis publicamente." title="Components">
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
              <Checkbox label="Checkbox selecionável" />
              <RadioGroup name="component-radio" onChange={setRadio} options={[{ label: 'Standard', value: 'standard' }, { label: 'Avançado', value: 'advanced' }]} value={radio} />
              <Switch checked={switchOn} label="Switch habilitado" onCheckedChange={setSwitchOn} />
            </div>
          </Card>

          <Card>
            <CardHeader><CardTitle>Overlay e feedback</CardTitle></CardHeader>
            <div className="ds-field-stack">
              <Tooltip content="Descrição adicional exibida por toque, foco ou hover."><Button variant="outline">Tooltip</Button></Tooltip>
              <Dropdown label="Abrir dropdown"><DropdownItem>Editar</DropdownItem><DropdownItem>Duplicar</DropdownItem><DropdownItem>Arquivar</DropdownItem></Dropdown>
              <Skeleton className="ds-skeleton-line" /><Skeleton className="ds-skeleton-line ds-skeleton-line--short" />
              <span className="ds-spinner-sample"><Spinner size="lg" tone="processing" /> Processando</span>
            </div>
          </Card>
        </div>
      </div>
    </Section>
  )
}

function FormExample() {
  const [submitted, setSubmitted] = useState(false)
  const form = useForm<ExampleForm>({
    defaultValues: { category: '', name: '', notifications: true, terms: false },
    resolver: zodResolver(formSchema),
  })
  const notifications = form.watch('notifications')

  return (
    <Section description="Um fluxo realista com ajuda, validação, erro e campo indisponível." title="Forms">
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

function DataExamples() {
  const [page, setPage] = useState(1)
  const rows = [
    { name: 'Atlético Central', status: <Badge tone="success">Ativo</Badge>, updated: 'Hoje' },
    { name: 'Estrela do Norte', status: <Badge tone="warning">Pendente</Badge>, updated: 'Ontem' },
  ]

  return (
    <Section description="Estados frequentes na apresentação de listas e resultados." title="Data">
      <div className="ds-component-stack">
        <Card><Table caption="Equipes cadastradas" columns={[{ key: 'name', label: 'Equipe' }, { key: 'status', label: 'Status' }, { key: 'updated', label: 'Atualização' }]} rows={rows} /><div className="ds-data-footer"><Pagination currentPage={page} onPageChange={setPage} totalPages={3} /></div></Card>
        <div className="ds-state-grid">
          <EmptyState action={<Button variant="outline">Criar equipe</Button>} title="Nenhuma equipe encontrada">Altere os filtros ou crie o primeiro registro.</EmptyState>
          <LoadingState>Carregando tabela e seus indicadores…</LoadingState>
          <ErrorState action={<Button variant="outline">Tentar novamente</Button>}>Verifique sua conexão e tente de novo.</ErrorState>
        </div>
      </div>
    </Section>
  )
}

function Feedback() {
  const [dialogOpen, setDialogOpen] = useState(false)
  const [toast, setToast] = useState<ToastTone | null>(null)
  const tones: ToastTone[] = ['success', 'warning', 'alert', 'danger', 'info']

  return (
    <Section description="Mensagens de sucesso, aviso, erro e informação em todos os componentes de feedback." title="Feedback">
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
  return (
    <main className="design-system-page">
      <header className="ds-topbar">
        <div><p className="ds-eyebrow">Convocado FC</p><h1>Design System</h1></div>
        <ThemeSelector />
      </header>
      <div className="ds-content">
        <p className="ds-intro">Documentação executável dos tokens, componentes, variantes e estados disponíveis. Redimensione a janela para validar a composição em celular, tablet e desktop.</p>
        <Colors /><Typography /><ButtonsAndInputs /><FormExample /><DataExamples /><Feedback />
      </div>
    </main>
  )
}
