import { useState } from "react";
import connection from '../lib/signalr';
import config from "../lib/config";


interface Props {
    roomId: string;
    setName: (name: string) => void;
    setMessages: (messages: any[]) => void;
    setJoined: (joined: boolean) => void;
    setKeepAlive: (keepAlive: boolean) => void;
}

export default function NickNameOverlay({ roomId, setName, setMessages, setJoined, setKeepAlive }: Props) {
    const [nameInput, setNameInput] = useState("")
    

    const joinRoom = async () => {
        if (!nameInput.trim()) return
        setName(nameInput)
        try {
            await connection.start()
            await connection.invoke("JoinRoom", nameInput, roomId)

            const res = await fetch(`${config.serverUrl}/Api/Chat/GetMessages/${roomId}`)
            if (res.ok) {
                const history = await res.json()
                setMessages(history.map((m: any) => ({
                kind: 'text' as const,
                sender: m.user,
                message: m.text
                })))
            }

            const keepAliveRes = await fetch(`${config.serverUrl}/Api/Chat/KeepAlive/${roomId}`)
            if (keepAliveRes.ok) {
                const isKeepAlive = await keepAliveRes.json()
                setKeepAlive(isKeepAlive)
            }

        } catch (err) {
            console.error("SignalR Connection Error: ", err)
        }
        setJoined(true)
    }


    return (
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
    )
}
