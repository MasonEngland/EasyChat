import { useState } from 'react'
import { useNavigate } from 'react-router'
import '../styles/Landing.css'

function Landing() {
  const [roomId, setRoomId] = useState("")
  const navigate = useNavigate()

  const createRoom = async () => {
    const res = await fetch('http://localhost:3000/api/Chat/Create', { method: 'POST' })
    const roomId = await res.text()
    navigate(`/chat/${roomId}`)
  }

  const joinRoom = () => {
    if (!roomId.trim()) return
    navigate(`/chat/${roomId}`)
  }

  return (
    <div className="landing-wrapper">
      <div className="landing-card">
        <h1 className="landing-title">EasyChat</h1>
        <p className="landing-sub">By Mason and Judah</p>

        <div className="landing-form">
          <div className="divider"><span>Create or join</span></div>

          <button className="landing-btn primary" onClick={createRoom}>
            Create a new room
          </button>

          <div className="join-row">
            <input
              className="landing-input"
              type="text"
              placeholder="Room ID"
              value={roomId}
              onChange={e => setRoomId(e.target.value)}
              onKeyDown={e => e.key === 'Enter' && joinRoom()}
            />
            <button className="landing-btn secondary" onClick={joinRoom}>
              Join
            </button>
          </div>
        </div>
      </div>
    </div>
  )
}

export default Landing