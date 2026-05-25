import connection from "../lib/signalr";
import { useState } from "react";
import CoreButton from "./coreButton";

interface Props {
    roomId: string;
    name: string;
    setMessages: any;
    setAiLoading: (loading: boolean) => void;
    aiLoading: boolean;

}

export default function MessagesForm({ roomId, name, setMessages, setAiLoading, aiLoading }: Props) {

    const [currMessage, setCurrMessage] = useState("")
    
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

    const handleVideoUpload = async (e: React.ChangeEvent<HTMLInputElement>) => {
        const file = e.target.files?.[0]
        if (!file || !roomId) return
        const formData = new FormData()
        formData.append('RoomId', roomId)
        formData.append('User', name)
        formData.append('File', file)
        await fetch(`http://localhost:3000/Api/Streaming/UploadVideo`, {
        method: 'POST',
        body: formData
        })
        e.target.value = '' // Reset file input
    }

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
            setMessages((p: any) => [...p, { kind: 'text', sender: name, message: text }])

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
        setMessages((p: any) => [...p, { kind: 'text', sender: name, message: text }])
    }


    return (
        <form onSubmit={e => { e.preventDefault(); sendMessage() }} className="chat-form">
          <input
            type="text"
            className="message-bar"
            placeholder="Type a message... or /ai <question>"
            value={currMessage}
            onChange={e => setCurrMessage(e.target.value)}
          />
          <CoreButton text="Send" onClick={sendMessage} disabled={aiLoading} />
          <input
            type="file"
            id="file-input"
            style={{ display: 'none' }}
            onChange={handleFileUpload}
          />
          <label htmlFor="file-input" className="attach-btn" title="Upload a file">📎</label>
          <input
            type="file"
            id="video-input"
            style={{ display: 'none' }}
            onChange={handleVideoUpload}
          />
          <label htmlFor="video-input" className="attach-btn" title="Start Streaming Video">📹</label>
        </form>
    )
}