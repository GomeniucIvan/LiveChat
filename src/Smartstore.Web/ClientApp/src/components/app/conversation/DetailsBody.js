import { useEffect, useRef, useState } from "react";
import { postLauncher } from "../../utils/HttpClient";
import { isNullOrEmpty } from "../../utils/Utils";
import { HubConnectionBuilder, LogLevel } from '@microsoft/signalr';
import { Loading } from "../../utils/Loading";
import { Storage } from "../../utils/StorageHelper";
import { useLocation } from "react-router-dom";

const DetailsBody = (props) => {
    let [messageList, setMessageList] = useState([]);
    let [loading, setLoading] = useState(true);
    const scrollRef = useRef(null);
    const location = useLocation();

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
            scrollMessageList();
        });

        const PopulateComponent = async () => {
            let response = await postLauncher('VisitorMessages', /*visitorId*/ props.VisitorId, /*model*/ null);

            if (response && response.IsValid) {
                setMessageList(response.Data);
            }

            scrollMessageList();
        }
        setLoading(false);
        PopulateComponent();
    }, []);

    const scrollMessageList = async () => {
        setTimeout(() => {
            if (scrollRef.current) {
                scrollRef.current.scrollTop = scrollRef.current.scrollHeight;
            }
        }, 50);
    }

    if (loading) {
        return <Loading />
    }

    return (
        <>
            <div className='conv-body' ref={scrollRef}>
                {messageList.map(message => {
                    return (
                        <div key={message.Id} className={`conv-message ${isNullOrEmpty(message.CompanyCustomerId) ? 'guest-message' : 'customer-message'}`}>
                            {!isNullOrEmpty(message.CompanyCustomerId) &&
                                <>
                                    <span className='message'>
                                        <span dangerouslySetInnerHTML={{ __html: message.Message }}></span>
                                    </span>
                                    <span className='message-icon'>
                                        <img src={message.IconUrl} />
                                    </span>
                                </>
                            }

                            {isNullOrEmpty(message.CompanyCustomerId) &&
                                <>
                                    <span className='message-icon'>
                                        <img src={message.IconUrl} />
                                    </span>

                                    <span className='message'>
                                        <span dangerouslySetInnerHTML={{ __html: message.Message }}></span>
                                    </span>

                                </>
                            }
                        </div>
                    )
                })}
            </div>
        </>
    )
}

export default DetailsBody