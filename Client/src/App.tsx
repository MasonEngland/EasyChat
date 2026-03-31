import { useEffect, useState } from 'react'
import './App.css'
import type {message} from './types/socketTypes';
import connection from './lib/signalr';



function App() {

  let [messages, setMessages] = useState([] as message[]);
  let [currMessage, setCurrMessage] = useState("");

  useEffect(() => {
    const startConnection = async () => {
        try {
            await connection.start();
            console.log("SignalR Connected");
        } catch (err) {
            console.error("SignalR Connection Error: ", err);
        }
    };

    startConnection();

    connection.on("RecieveMessage", (p1, p2) => {
      console.log("Received message");
      console.log(p1, p2);
    })

    return () => {
      if (connection) connection.stop()
    }

  }, []);

  const sendMessage = async () => {

    try {
      await connection.invoke("SendMessage", "test user",  currMessage) 
    } catch (err) {
      console.log(err);
      return;
    }
    setMessages(p => [...p, {sender: true, message: currMessage}]);
    setCurrMessage("");
  }

  


  return (
    <div className={"body"}>
      <div id="messages-section">
        {messages.map((val, i) => {
          if (val.sender == true) {
            return <span className={"sent-message"}></span>
          } 
          return <span className={"recieved-message"}></span>
        })}
      </div>
      <form onSubmit={(e) => {
        e.preventDefault();
        sendMessage();
      }}>
        <input type="text" className={"message-bar"} value={currMessage} onChange={(p) => setCurrMessage(p.target.value)}/>
        <button type="submit" className={"submit-button"}>Send</button>
      </form>
      
    </div>
  )
}

export default App
