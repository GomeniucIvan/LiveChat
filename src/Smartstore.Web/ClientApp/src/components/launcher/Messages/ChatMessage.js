import React from 'react'
import { isNullOrEmpty } from '../../utils/Utils';
import ChatTextMessage from './ChatTextMessage'

const ChatMessage = (props) => {

    let contentClassList = [
        "app-message-content",
        (props.message.IsVisitorMessage ? "sent" : "received"),
        (!props.message.IsVisitorMessage && isNullOrEmpty(props.message.ReadOnUtc) ? "new" : "")
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