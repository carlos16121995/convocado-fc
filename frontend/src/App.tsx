import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { BrowserRouter, Link, Route, Routes } from 'react-router-dom'
import './App.css'

const queryClient = new QueryClient()
function HomePage() { return <main><p className="eyebrow">Convocado FC</p><h1>Frontend pronto para evoluir por features.</h1><p>React, TypeScript e Vite com as fronteiras arquiteturais definidas para o produto.</p><Link className="link" to="/about">Ver status do projeto</Link></main> }
function AboutPage() { return <main><p className="eyebrow">Status</p><h1>Base inicial criada.</h1><p>As features serão adicionadas conforme os casos de uso surgirem.</p><Link className="link" to="/">Voltar</Link></main> }
function App() { return <QueryClientProvider client={queryClient}><BrowserRouter><Routes><Route path="/" element={<HomePage />} /><Route path="/about" element={<AboutPage />} /></Routes></BrowserRouter></QueryClientProvider> }
export default App
