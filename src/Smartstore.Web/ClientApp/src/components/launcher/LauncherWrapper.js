import React, { useEffect, useRef, useState } from 'react';
import { HubConnectionBuilder, LogLevel } from '@microsoft/signalr';
import { postLauncher } from '../utils/HttpClient';
import Launcher from './Launcher';
import { useLocation } from 'react-router-dom';
import { Storage } from '../utils/StorageHelper';
let isInitCall = true;

let messageArrayList = []; //todo ?! setMessageList reload component!!

const LauncherWrapper = (props) => {
    let [messageList, setMessageList] = useState([]);
    let [newMessagesCount, setNewMessagesCount] = useState(0);
    let [isOpen, setIsOpen] = useState(false);
    let [loading, setLoading] = useState(true);
    const msgListScrollRef = useRef(null);
    const location = useLocation();

    useEffect(() => {
        const PopulateComponent = async () => {

            if (isInitCall) {
                isInitCall = false;
                const searchParams = new URLSearchParams(location.search);
                const settingsString = searchParams.get('settings');
                var data = JSON.parse(settingsString);
                let response = await postLauncher(`chat/VisitorData`, data, /*location*/ null, /*isInitCall*/ true);

                if (response && response.IsValid) {
                    const companyVisitorInfo = response.Data;

                    Storage.SetCompanyId(companyVisitorInfo.CompanyId)
                    Storage.SetVisitorId(companyVisitorInfo.VisitorId)
                }
            }

            let response = await postLauncher(`chat/LauncherMessages`, null);
            if (response && response.IsValid) {
                setMessageList(response.Data);
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

        connection.on(`visitor_${Storage.CompanyId}_${Storage.VisitorId}_new_message`, function (message) {
            PopulateComponent();
        });

        connection.on(`company_${Storage.CompanyId}_${Storage.VisitorId}_typing`, function (message) {

        });
    }, []);

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
            <Launcher
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
            />
        </div>
    )
}

export default LauncherWrapper;
