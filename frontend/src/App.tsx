import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { BrowserRouter, Link, Route, Routes } from 'react-router-dom'
import { Card, ThemeProvider } from './components/ui'
import { AvailabilityPage } from './features/availability/AvailabilityPage'
import { DesignSystemPage } from './features/design-system/DesignSystemPage'
import './App.css'

const queryClient = new QueryClient()
function AboutPage() { return <main className="app-page"><Card><p className="app-eyebrow">Status</p><h1>Base inicial criada.</h1><p>A disponibilidade da integração é verificada na página inicial.</p><Link className="app-link" to="/">Voltar</Link></Card></main> }
function App() { return <ThemeProvider><QueryClientProvider client={queryClient}><BrowserRouter><Routes><Route path="/" element={<AvailabilityPage />} /><Route path="/about" element={<AboutPage />} /><Route path="/design-system" element={<DesignSystemPage />} /></Routes></BrowserRouter></QueryClientProvider></ThemeProvider> }
export default App
