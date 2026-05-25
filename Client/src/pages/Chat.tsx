import { useEffect, useState } from 'react';
import { useParams, useNavigate } from 'react-router';
import connection from '../lib/signalr';
import NickNameOverlay from '../components/nickNameOverlay';
import '../styles/Chat.css';
import MessagesSection from '../components/messagesSection';
import MessagesForm from '../components/messagesForm';
import CorePopup from '../components/corePopup';
import VideoView from '../components/videoView';

// test roomId: 0a2ce9e6-8a23-42d5-add8-4e22c31ac0fe


type ChatEntry = 
  | {kind: 'text', sender: string, message: string}
  | {kind: 'file', sender: string, fileName: string, fileId: number}


function Chat() {
  const { roomId } = useParams<{ roomId: string }>()
  const navigate = useNavigate()
  const [name, setName] = useState("")
  const [joined, setJoined] = useState(false)
  const [aiLoading, setAiLoading] = useState(false)
  const [keepAlive, setKeepAlive] = useState(false)
  const [messages, setMessages] = useState<ChatEntry[]>([])
  const [errorMessage, setErrorMessage] = useState(null as string | null);
  const [isStreaming, setIsStreaming] = useState(false);
  const [isHost, setIsHost] = useState(false);

  console.log(name);

  const handleKeepAlive = async (value: boolean) => {
    setKeepAlive(value)
    if (roomId) localStorage.setItem(`easychat.keepAlive.${roomId}`, value ? 'true' : 'false')
    await fetch(`http://localhost:3000/Api/Chat/UpdateRoomLife`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ roomId, keepAlive: value })
    })
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

    connection.on("ReceiveVideo", async (user: string) => {
      setIsStreaming(true);
      console.log("Received video stream event from user: ", user);
      console.log("Current user: ", name);
      if (user === name) {
        setIsHost(true);
      }
    });

    connection.on("CatchError", (errorMessage) => {
      setErrorMessage(errorMessage)
      navigate('/')
    });
    return () => {
      connection.off("ReceiveMessage");
      connection.off("ReceiveFile");
      connection.off("CatchError");
    }
  }, [name, navigate])

  useEffect(() => {
    return () => {
      connection.stop()
    }
  }, [])

  
  return (
    <>
      {isStreaming && (
        <VideoView roomId={roomId!} host={isHost} setIsStreaming={setIsStreaming} setIsHost={setIsHost} />
      )}
      <div className="chat-wrapper">
        {errorMessage ? (
          <CorePopup
            message={"An error occurred: " + errorMessage}
          />
        ): <></>}
        {!joined && (
          <NickNameOverlay
            roomId={roomId!}
            setName={setName}
            setMessages={setMessages}
            setJoined={setJoined}
            setKeepAlive={setKeepAlive}
          />
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

          <MessagesSection messages={messages} name={name} aiLoading={aiLoading} />

          <MessagesForm
            roomId={roomId!}
            name={name}
            setMessages={setMessages}
            setAiLoading={setAiLoading}
            aiLoading={aiLoading}
          />

        </div>
      </div>
    </>
  )
}

export default Chat