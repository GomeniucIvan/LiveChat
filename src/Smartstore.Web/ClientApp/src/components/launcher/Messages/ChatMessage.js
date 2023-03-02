import React from 'react'
import ChatTextMessage from './ChatTextMessage'

const ChatMessage = (props) => {

    let contentClassList = [
        "app-message-content",
        (props.message.Sent ? "sent" : "received")
    ];

    return (
        <div className="app-message">
            <div className={contentClassList.join(" ")}>
                <div className="app-message-avatar" style={{ backgroundImage: `url(${props.message.IconUrl})` }}></div>
                {<ChatTextMessage {...props.message} /> }
            </div>
        </div>
    )
}

export default ChatMessage;