import React from 'react';
import ChatMessage from './Messages/ChatMessage'

const ChatMessageList = (props) => {
    return (
        <div className="app-message-list" ref={props.msgListScrollRef}>
            {props.messages.map((message, i) => {
                console.log(message);

                return <ChatMessage message={message} key={i} />
            })}
        </div>
    )
}

export default ChatMessageList;