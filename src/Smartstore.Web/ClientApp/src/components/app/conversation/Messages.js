import { useEffect, useState } from "react";
import { KTSVG } from '../../utils/Utils'
import Translate from '../../utils/Translate'
import Details from "./Details";
import { post } from "../../utils/HttpClient";
import { HubConnectionBuilder, LogLevel } from '@microsoft/signalr';

const Index = (props) => {
    const [loading, setLoading] = useState(false);
    let [messageList, setMessageList] = useState([]);
    let [visitorId, setVisitorId] = useState(null);

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
            let response = await post('Messages', /*object*/ null);

            if (response && response.IsValid) {
                setMessageList(response.Data);
            }
        }
        setLoading(false);
        PopulateComponent();
    }, [])

    function selectVisitor(visitorId) {
        setVisitorId(visitorId);
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

                        {messageList.map(message => {
                            return (
                                <div onClick={() => selectVisitor(message.VisitorId)} className='conversation-item' key={message.Id}>
                                    <div className='d-flex w-100'>
                                        <span className="current-user">
                                            <div className="user-thumbnail-box">
                                                <div className="avatar-container user-thumbnail thumbnail-rounded">
                                                    <span>IG</span>
                                                </div>
                                                <div className="source-badge user-online-status"></div>
                                            </div>
                                        </span>
                                        <span className='current-user-details'>
                                            <span>
                                                <div className='current-user-initials'>
                                                    {message.FullName}
                                                </div>
                                                <div className='current-user-description'>
                                                    {message.Description}
                                                </div>
                                            </span>
                                            <span>
                                                {message.LastMessageIncomeTimeAgoDisplay}
                                            </span>
                                        </span>
                                    </div>

                                    <div className='reply-text'>

                                    </div>
                                    <div className='sent-text'>
                                        {message.LastMessage}
                                    </div>
                                </div>
                            )
                        })}
                    </div>
                </div>

                {<Details visitorId={visitorId} />}

                <div className='conversation-details-summary'>

                </div>
            </section>
        </>
    )
}

export default Index