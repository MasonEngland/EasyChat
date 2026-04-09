import { Routes, Route } from 'react-router'
import Landing from './pages/Landing'
import Chat from './pages/Chat'

function App() {
  return (
    <Routes>
      <Route path="/" element={<Landing />} />
      <Route path="/chat/:roomId" element={<Chat />} />
    </Routes>
  )
}

export default App