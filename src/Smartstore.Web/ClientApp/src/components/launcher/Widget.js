import React, { useEffect, useRef, useState } from 'react';
import { HubConnectionBuilder, LogLevel } from '@microsoft/signalr';
import { postChat } from '../utils/HttpClient';
import Chat from './Chat';
import { useLocation } from 'react-router-dom';
import { Storage } from '../utils/StorageHelper';
import { useDispatch } from 'react-redux';

let isInitCall = true;

let messageArrayList = []; //todo ?! setMessageList reload component!!

const Widget = (props) => {
    let [messageList, setMessageList] = useState([]);
    let [newMessagesCount, setNewMessagesCount] = useState(0);
    let [isOpen, setIsOpen] = useState(false);
    let [loading, setLoading] = useState(true);
    let [companyTyping, setCompanyTyping] = useState(false);
    let [company, setCompany] = useState(null);

    const msgListScrollRef = useRef(null);
    const location = useLocation();
    const dispatch = useDispatch();

    let visitorId = null;
    let typingTimer;

    //todo
    //read typing indicator to setting

    useEffect(() => {
        const PopulateComponent = async () => {

            if (isInitCall) {
                isInitCall = false;
                const searchParams = new URLSearchParams(location.search);
                const settingsString = searchParams.get('settings');
                const data = JSON.parse(settingsString);
                visitorId = data.visitorId;

                dispatch({ type: 'VISITOR_ID', payload: visitorId });
                let response = await postChat(`Data`,
                    /*visitorId*/ visitorId,
                    /*object*/ data,
                    /*isInitCall*/ true);

                if (response && response.IsValid) {
                    const companyVisitorInfo = response.Data;
                    Storage.SetCompanyId(companyVisitorInfo.CompanyId);
                }
            }

            let response = await postChat(`Messages`, /*visitorId*/ visitorId);

            if (response && response.IsValid) {
                setMessageList(response.Data.Messages);
                setNewMessagesCount(response.Data.NewMessagesCount);
                messageArrayList = response.Data;

                await scrollMessageList();
            }
            setLoading(false);
        }
        PopulateComponent();

        const connection = new HubConnectionBuilder()
            .withUrl("/chatHub")
            .withAutomaticReconnect()
            .configureLogging(LogLevel.None)
            .build();

        connection.start().catch(function (err) {
            console.log('Hub Error' + err);
        });

        connection.on(`visitor_${Storage.CompanyId}_${visitorId}_new_message`, function (message) {
            PopulateComponent();
        });

        connection.on(`company_${Storage.CompanyId}_${visitorId}_typing`, function (message) {
            clearTimeout(typingTimer);
            setCompanyTyping(true);
            typingTimer = setTimeout(setCompanyTypingToFalse, 1000);
        });
    }, []);

    function setCompanyTypingToFalse() {
        setCompanyTyping(false)
    }

    const onGuestSendMessage = async (message) => {
        setMessageList([...messageList, message]);

        await scrollMessageList();
    }

    const handleIconClick = async () => {
        setIsOpen(!isOpen);
        if (isOpen) {
            setNewMessagesCount(0);
        }
        await scrollMessageList();
    }

    const scrollMessageList = async () => {
        setTimeout(() => {
            if (msgListScrollRef.current) {
                msgListScrollRef.current.scrollTop = msgListScrollRef.current.scrollHeight;
            }
        }, 50);
    }

    return (
        <div className="app-laucher-container">
            <Chat
                agentProfile={{
                    teamName: 'react-live-chat',
                    imageUrl: 'https://a.slack-edge.com/66f9/img/avatars-teams/ava_0001-34.png'
                }}
                msgListScrollRef={msgListScrollRef}
                onGuestSendMessage={onGuestSendMessage.bind(this)}
                messageList={messageList}
                newMessagesCount={newMessagesCount}
                handleClick={handleIconClick.bind(this)}
                isOpen={isOpen}
                companyTyping={companyTyping}
            />
        </div>
    )
}

export default Widget;
