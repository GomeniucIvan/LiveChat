import { useEffect, useState } from "react";
import { equal, isNullOrEmpty, KTSVG, showResponsePNotifyMessage, T } from '../../utils/Utils'
import Translate from '../../utils/Translate'
import { post } from "../../utils/HttpClient";
import { HubConnectionBuilder, LogLevel } from '@microsoft/signalr';
import { ReactComponent as NoSelectedVisitorSvf } from '../assets/svgs/no-selected-visitor.svg';
import { useParams } from "react-router-dom";
import VisitorMessageDetails from "./VisitorMessageDetails";
import { Loading } from "../../utils/Loading";
import { Storage } from "../../utils/StorageHelper";

const Index = (props) => {
    const [loading, setLoading] = useState(false);
    let [messageList, setMessageList] = useState([]);
    let [message, setMessage] = useState(null);
    let { urlVisitorId } = useParams();

    useEffect(() => {
        const connection = new HubConnectionBuilder()
            .withUrl("/chatHub")
            .withAutomaticReconnect()
            .configureLogging(LogLevel.None)
            .build();

        connection.start().catch(function (err) {
            console.log('Hub Error' + err);
        });

        connection.on(`company_${Storage.CompanyId}_new_message`, function (message) {
            PopulateComponent();
        });

        const PopulateComponent = async () => {
            if (!isNullOrEmpty(urlVisitorId)) {
                const dataMessage = {
                    visitorId: urlVisitorId
                };

                const response = await post('MessageDetails', /*object*/ dataMessage);

                if (response && response.IsValid) {
                    setMessage(response.Data);
                } else {
                    showResponsePNotifyMessage(response);
                }
            }

            let response = await post('Messages', /*object*/ null);

            if (response && response.IsValid) {
                setMessageList(response.Data);
            }
        }
        setLoading(false);
        PopulateComponent();
    }, []);

    function selectVisitor(message) {
        window.history.pushState({}, "", `/message/${message.VisitorId}`);
        setMessage(message);
    };

    return (
        <>
            <section className='conversation-page'>
                <div className='conversations-list-wrap'>
                    <div className='conversations-list-wrap-head d-flex flex-row align-items-center'>
                        <div className='flex flex-row items-center '>
                            <KTSVG width='30' height='30' icon='hamburger' className='svg-icon-2' />
                            <span className='conversations-list-wrap-head-status'>
                                <Translate text='App.Conversation.Status.All' />
                            </span>
                        </div>
                    </div>
                    <div className='conversations-list'>

                        {messageList.map(visMessage => {
                            return (
                                <div onClick={() => selectVisitor(visMessage)}
                                    className={`conversation-item ${!isNullOrEmpty(message) && equal(visMessage.VisitorId, message.VisitorId) ? 'selected' : ''}` }
                                    key={visMessage.Id}>
                                    <div className='d-flex w-100'>
                                        <span className="current-user">
                                            <div className="user-thumbnail-box">
                                                <div className="avatar-container user-thumbnail thumbnail-rounded">
                                                    <span>{visMessage.CustomerInitials}</span>
                                                </div>
                                                <div className="source-badge user-online-status"></div>
                                            </div>
                                        </span>
                                        <span className='current-user-details'>
                                            <span>
                                                <div className='current-user-initials'>
                                                    {visMessage.FullName}
                                                </div>
                                                <div className='current-user-message'>
                                                    {!visMessage.IsVisitorMessage &&
                                                        <KTSVG icon='arrow-reply-outline' />
                                                    }
                                                    {visMessage.MessageShort}
                                                </div>
                                            </span>
                                            <span>
                                                {visMessage.LastMessageIncomeTimeAgoDisplay}
                                            </span>
                                        </span>
                                    </div>
                                </div>
                            )
                        })}
                    </div>
                </div>

                {loading &&
                    <Loading />
                }

                {!loading &&
                    <>
                        {isNullOrEmpty(message) &&
                            <div className='conversation-empty-visitor'>
                                <NoSelectedVisitorSvf />
                                <div>
                                    <Translate text="App.Conversation.SelectMessage" />
                                </div>
                            </div>
                        }

                        {!isNullOrEmpty(message) &&
                            <VisitorMessageDetails message={message} />
                        }
                    </>
                }

            </section>
        </>
    )
}

export default Index