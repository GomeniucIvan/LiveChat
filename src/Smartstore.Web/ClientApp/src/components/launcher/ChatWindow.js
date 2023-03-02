import React, { useState } from 'react';
import ChatMessageList from './ChatMessageList'
import ChatUserInput from './ChatUserInput'
import ChatHeader from './ChatHeader'
import { Loading } from './../utils/Loading'

const ChatWindow = (props) => {

    const onGuestSendMessage = async (message) => {
        props.onGuestSendMessage(message);
    }

    let messageList = props.messageList || [];
    let classList = [
        "app-chat-window",
        (props.isOpen ? "opened" : "closed")
    ];

    return (
        <div className={classList.join(' ')}>
            <ChatHeader
                teamName={props.agentProfile.teamName}
                imageUrl={props.agentProfile.imageUrl}
                onClose={props.onClose}
            />
            {props.isOpen &&
                <ChatMessageList
                    msgListScrollRef={props.msgListScrollRef}
                    messages={messageList}
                    imageUrl={props.agentProfile.imageUrl}
                />
            }
            {props.companyTyping &&
                <span>Typing</span>
            }
            {!props.isOpen &&
                <Loading />
            }
            <ChatUserInput onSubmit={onGuestSendMessage.bind(this)} />
        </div>
    );
}
export default ChatWindow;
