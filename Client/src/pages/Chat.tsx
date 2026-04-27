import { useEffect, useState } from 'react'
import { useParams, useNavigate } from 'react-router'
import connection from '../lib/signalr'
import type { message } from '../types/socketTypes'
import '../styles/Chat.css'

// test roomId: 0a2ce9e6-8a23-42d5-add8-4e22c31ac0fe


type ChatEntry = 
  | {kind: 'text', sender: string, message: string}
  | {kind: 'file', sender: string, fileName: string, fileId: number}
function Chat() {
  const { roomId } = useParams<{ roomId: string }>()
  const navigate = useNavigate()
  const [name, setName] = useState("")
  const [nameInput, setNameInput] = useState("")
  const [joined, setJoined] = useState(false)
  // const [messages, setMessages] = useState<message[]>([])
  const [currMessage, setCurrMessage] = useState("")
  const [aiLoading, setAiLoading] = useState(false)
  const [keepAlive, setKeepAlive] = useState(false)
  // const [fileMessages, setFileMessages] = useState<ChatEntry[]>([])
  const [messages, setMessages] = useState<ChatEntry[]>([])

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
          kind: 'text' as const,
          sender: m.user,
          message: m.text
        })))
      }

      const keepAliveRes = await fetch(`http://localhost:3000/api/Chat/KeepAlive/${roomId}`)
      if (keepAliveRes.ok) {
        const isKeepAlive = await keepAliveRes.json()
        setKeepAlive(isKeepAlive)
      }

    } catch (err) {
      console.error("SignalR Connection Error: ", err)
    }
    setJoined(true)
  }

  const handleKeepAlive = async (value: boolean) => {
    setKeepAlive(value)
    if (roomId) localStorage.setItem(`easychat.keepAlive.${roomId}`, value ? 'true' : 'false')
    await fetch(`http://localhost:3000/Api/Chat/UpdateRoomLife`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ roomId, keepAlive: value })
    })
  }

  const handleFileUpload = async (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0]
    if (!file || !roomId) return
    const formData = new FormData()
    formData.append('RoomId', roomId)
    formData.append('User', name)
    formData.append('File', file)
    await fetch(`http://localhost:3000/Api/File/Upload`, {
      method: 'POST',
      body: formData
    })
    e.target.value = '' // Reset file input
  }

  useEffect(() => {
    connection.on("ReceiveMessage", (user, message) => {
      // Only add messages from other users, since we add our own messages optimistically in sendMessage
      setMessages(p => [...p, { kind: 'text', sender: user, message }])
    })

    connection.on("ReceiveFile", (user: string, fileName: string, fileId: number) => {
      // Store file message separately to show a download button instead of a text bubble
      setMessages(p => [...p, { kind: 'file', sender: user, fileName, fileId }])
    })

    connection.on("CatchError", (errorMessage) => {
      navigate('/')
    });
    return () => {
      connection.off("ReceiveMessage");
      connection.off("ReceiveFile");
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

      // Send /ai message to the room normally first
      try {
        await connection.invoke("SendMessage", name, text)
      } catch (err) {
        console.log(err)
        return
      }
      setMessages(p => [...p, { kind: 'text', sender: name, message: text }])

      // Hit the backend calls Ollama and broadcasts via SignalR to everyone
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
    setMessages(p => [...p, { kind: 'text', sender: name, message: text }])
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
          {joined && (
          <label className="keepalive-toggle" title="Keep this room alive past 3 days">
            <input
              type="checkbox"
              checked={keepAlive}
              onChange={e => handleKeepAlive(e.target.checked)}
            />
            Keep alive
          </label>
        )}
          <span className="chat-name">{name}</span>
        </div>

        <div className="messages-section">
          {messages.reduce((groups: any[], val, i) => {
            const prev = messages[i - 1]
            if (prev && prev.sender === val.sender) {
              if (val.kind === 'text') groups[groups.length - 1].items.push({kind: 'text', message: val.message})
                else groups[groups.length - 1].items.push({kind: 'file', fileName: val.fileName, fileId: val.fileId})
            } else {
              const item = val.kind === 'text' 
              ? {kind: 'text', message: val.message} 
              : {kind: 'file', fileName: val.fileName, fileId: val.fileId}
              groups.push({ sender: val.sender, items: [item] })
            }
            return groups
          }, []).map((group, i) => (
            <div key={i} className={`message-row
              ${group.sender === name ? 'mine' : ''}
              ${group.sender === 'EasyChat' ? 'system' : ''}
              ${group.sender === 'AI Assistant' ? 'ai' : ''}`}>
              <span className="message-sender">{group.sender}</span>
              {group.items.map((item: any, j: number) => (
                  item.kind === 'text' 
                  ? <div key={j} className="message-bubble">{item.message}</div>
                  : <div key={j} className="message-bubble file-bubble">
                  📎 <a href={`http://localhost:3000/Api/File/Download/${item.fileId}`} target="_blank" className="file-link">{item.fileName}</a>
                  </div>
              ))}
            </div>
          ))}

          {/* {fileMessages.map((f, i) => (
            <div key={i} className={`message-row ${f.sender === name ? 'mine' : ''}`}>
              <span className="message-sender">{f.sender}</span>
              <div className="message-bubble file-bubble">
                📎<a href={`http://localhost:3000/Api/File/Download/${f.fileId}`} target="_blank" className="file-link">{f.fileName}</a>
              </div>
            </div>
          ))} */}

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
          <input
            type="file"
            id="file-input"
            style={{ display: 'none' }}
            onChange={handleFileUpload}
          />
          <label htmlFor="file-input" className="attach-btn" title="Upload a file">📎</label>
        </form>
      </div>
    </div>
  )
}

export default Chat