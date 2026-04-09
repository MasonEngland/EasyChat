import { useEffect, useState } from 'react'
import { useParams, useLocation, useNavigate } from 'react-router'
import connection from '../lib/signalr'
import type { message } from '../types/socketTypes'
import '../styles/Chat.css'

function Chat() {
  const { roomId } = useParams<{ roomId: string }>()
  const location = useLocation()
  const navigate = useNavigate()
  const name: string = location.state?.name ?? "Anonymous"

  const [messages, setMessages] = useState<message[]>([])
  const [currMessage, setCurrMessage] = useState("")

    // Connect to SignalR hub and set up listeners
    useEffect(() => {
    const startConnection = async () => {
      try {
        await connection.start()
        await connection.invoke("JoinRoom", name, roomId)
        console.log("SignalR Connected, Joined room: ", roomId)

        // Load message history
        const res = await fetch(`http://localhost:3000/api/Chat/GetMessages/${roomId}`)
        console.log("Message history response: ", res)
        if (res.ok) {
            const history = await res.json()
            setMessages(history.map((m: any) => ({ 
                sender: m.user === name, 
                message: m.text 
            })))
        }

        } catch (err) {
            console.error("SignalR Connection Error: ", err)
        }
    }
    startConnection()

    connection.on("ReceiveMessage", (user, message) => {
      setMessages(p => [...p, { sender: user === name, message }])
    })

    return () => {
      connection.off("ReceiveMessage")
      connection.stop()
    }
    }, [])
// Send message to SignalR hub
  const sendMessage = async () => {
    if (!currMessage.trim()) return
    try {
      await connection.invoke("SendMessage", name, currMessage)
    } catch (err) {
      console.log(err)
      return
    }
    setMessages(p => [...p, { sender: true, message: currMessage }])
    setCurrMessage("")
  }

  return (
    <div className="chat-wrapper">
      <div className="chat-container">
        <div className="chat-header">
            <button className="home-btn" onClick={() => navigate('/')}>← Home</button>
            <span className="chat-room-id">Room: <code>{roomId}</code></span>
            <span className="chat-name">{name}</span>
        </div>

        <div className="messages-section">
          {messages.map((val, i) =>
            val.sender
              ? <span key={i} className="sent-message">{val.message}</span>
              : <span key={i} className="received-message">{val.message}</span>
          )}
        </div>

        <form onSubmit={e => { e.preventDefault(); sendMessage() }} className="chat-form">
          <input
            type="text"
            className="message-bar"
            placeholder="Type a message..."
            value={currMessage}
            onChange={e => setCurrMessage(e.target.value)}
          />
          <button type="submit" className="submit-button">Send</button>
        </form>
      </div>
    </div>
  )
}

export default Chat