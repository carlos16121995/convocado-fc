import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { BrowserRouter, Link, Route, Routes } from 'react-router-dom'
import { Card, ThemeProvider } from './components/ui'
import { DesignSystemPage } from './features/design-system/DesignSystemPage'
import './App.css'

const queryClient = new QueryClient()
function HomePage() { return <main className="app-page"><Card><p className="app-eyebrow">Convocado FC</p><h1>Frontend pronto para evoluir por features.</h1><p>React, TypeScript e Vite com as fronteiras arquiteturais definidas para o produto.</p><Link className="app-link" to="/about">Ver status do projeto</Link><Link className="app-link" to="/design-system">Abrir Design System</Link></Card></main> }
function AboutPage() { return <main className="app-page"><Card><p className="app-eyebrow">Status</p><h1>Base inicial criada.</h1><p>As features serão adicionadas conforme os casos de uso surgirem.</p><Link className="app-link" to="/">Voltar</Link></Card></main> }
function App() { return <ThemeProvider><QueryClientProvider client={queryClient}><BrowserRouter><Routes><Route path="/" element={<HomePage />} /><Route path="/about" element={<AboutPage />} /><Route path="/design-system" element={<DesignSystemPage />} /></Routes></BrowserRouter></QueryClientProvider></ThemeProvider> }
export default App
