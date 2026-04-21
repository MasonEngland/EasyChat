import { useEffect, useState } from 'react'
import { useParams, useNavigate } from 'react-router'
import connection from '../lib/signalr'
import type { message } from '../types/socketTypes'
import '../styles/Chat.css'

function Chat() {
  const { roomId } = useParams<{ roomId: string }>()
  const navigate = useNavigate()
  const [name, setName] = useState("")
  const [nameInput, setNameInput] = useState("")
  const [joined, setJoined] = useState(false)
  const [messages, setMessages] = useState<message[]>([])
  const [currMessage, setCurrMessage] = useState("")
  const [aiLoading, setAiLoading] = useState(false)

  const joinRoom = async () => {
    if (!nameInput.trim()) return
    setName(nameInput)
    try {
      await connection.start()
      await connection.invoke("JoinRoom", nameInput, roomId)

      const res = await fetch(`http://localhost:3000/api/Chat/GetMessages/${roomId}`)
      console.log("History status:", res.status)
      if (res.ok) {
        const history = await res.json()
        console.log("History:", history)
        setMessages(history.map((m: any) => ({
          sender: m.user,
          message: m.text
        })))
      }
    } catch (err) {
      console.error("SignalR Connection Error: ", err)
    }
    setJoined(true)
  }

  useEffect(() => {
    connection.on("ReceiveMessage", (user, message) => {
      setMessages(p => [...p, { sender: user, message }])
    })

    connection.on("CatchError", (errorMessage) => {
      navigate('/')
    });
    return () => {
      connection.off("ReceiveMessage");
      connection.off("CatchError");
      connection.stop()
    }
  }, [])

  const sendMessage = async () => {
    const text = currMessage.trim()
    if (!text) return
    setCurrMessage("")

    if (text.toLowerCase().startsWith('/ai ')) {
      const prompt = text.slice(4).trim()
      if (!prompt) return

      // Send the user's /ai message to the room normally first
      try {
        await connection.invoke("SendMessage", name, text)
      } catch (err) {
        console.log(err)
        return
      }
      setMessages(p => [...p, { sender: name, message: text }])

      // Hit the backend — it calls Ollama and broadcasts via SignalR to everyone
      setAiLoading(true)
      try {
        await fetch(`http://localhost:3000/Api/AI/GetResponse`, {
          method: 'POST',
          headers: { 'Content-Type': 'application/json' },
          body: JSON.stringify({ roomId, userMessage: prompt })
        })
      } catch (err) {
        console.error("Error generating AI response: ", err)
      } finally {
        setAiLoading(false)
      }
      return
    }

    // Regular message
    try {
      await connection.invoke("SendMessage", name, text)
    } catch (err) {
      console.log(err)
      return
    }
    setMessages(p => [...p, { sender: name, message: text }])
  }

  return (
    <div className="chat-wrapper">
      {!joined && (
        <div className="nickname-overlay">
          <div className="nickname-popup">
            <h2>Enter a nickname</h2>
            <p>to join room <code>{roomId}</code></p>
            <input
              className="landing-input"
              type="text"
              placeholder="Nickname"
              value={nameInput}
              onChange={e => setNameInput(e.target.value)}
              onKeyDown={e => e.key === 'Enter' && joinRoom()}
              autoFocus
            />
            <button className="landing-btn primary" onClick={joinRoom}>Join</button>
          </div>
        </div>
      )}

      <div className="chat-container">
        <div className="chat-header">
          <button className="home-btn" onClick={() => navigate('/')}>← Home</button>
          <span className="chat-room">Room: <span className="chat-room-id"><code>{roomId}</code></span></span>
          <span className="chat-name">{name}</span>
        </div>

        <div className="messages-section">
          {messages.reduce((groups: any[], val, i) => {
            const prev = messages[i - 1]
            if (prev && prev.sender === val.sender) {
              groups[groups.length - 1].messages.push(val.message)
            } else {
              groups.push({ sender: val.sender, messages: [val.message] })
            }
            return groups
          }, []).map((group, i) => (
            <div key={i} className={`message-row
              ${group.sender === name ? 'mine' : ''}
              ${group.sender === 'EasyChat' ? 'system' : ''}
              ${group.sender === 'AI Assistant' ? 'ai' : ''}`}>
              <span className="message-sender">{group.sender}</span>
              {group.messages.map((msg: string, j: number) => (
                <div key={j} className="message-bubble">{msg}</div>
              ))}
            </div>
          ))}

          {aiLoading && (
            <div className="message-row ai">
              <span className="message-sender">AI Assistant</span>
              <div className="message-bubble ai-typing">
                <span /><span /><span />
              </div>
            </div>
          )}
        </div>

        <form onSubmit={e => { e.preventDefault(); sendMessage() }} className="chat-form">
          <input
            type="text"
            className="message-bar"
            placeholder="Type a message... or /ai <question>"
            value={currMessage}
            onChange={e => setCurrMessage(e.target.value)}
          />
          <button type="submit" className="submit-button" disabled={aiLoading}>
            {aiLoading ? '…' : 'Send'}
          </button>
        </form>
      </div>
    </div>
  )
}

export default Chat