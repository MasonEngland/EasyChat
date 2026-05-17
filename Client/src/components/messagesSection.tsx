
interface Props {
    messages: any[];
    name: string;
    aiLoading: boolean;
}

export default function MessagesSection({ messages, name, aiLoading }: Props) {


    return (
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

          {aiLoading && (
            <div className="message-row ai">
              <span className="message-sender">AI Assistant</span>
              <div className="message-bubble ai-typing">
                <span /><span /><span />
              </div>
            </div>
          )}
        </div>
    )
}