import { useEffect, useRef, useState } from "react";
import { postLauncher } from "../../utils/HttpClient";
import { isNullOrEmpty } from "../../utils/Utils";
import { HubConnectionBuilder, LogLevel } from '@microsoft/signalr';
import { Loading } from "../../utils/Loading";
import { Storage } from "../../utils/StorageHelper";

const DetailsBody = (props) => {
    let [messageList, setMessageList] = useState([]);
    let [loading, setLoading] = useState(true);
    let [visitorTyping, setVisitorTyping] = useState(false);
    const scrollRef = useRef(null);
    let typingTimer;

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

        connection.on(`visitor_${Storage.CompanyId}_${props.visitorId}_typing`, function (message) {
            clearTimeout(typingTimer);
            setVisitorTyping(true);
            typingTimer = setTimeout(setVisitorTypingToFalse, 1000);
        });

        const PopulateComponent = async () => {
            let response = await postLauncher('VisitorMessages', /*visitorId*/ props.visitorId, /*model*/ null);

            if (response && response.IsValid) {
                setMessageList(response.Data);
            }

            scrollMessageList();
        }
        setLoading(false);
        PopulateComponent();
    }, []);

    function setVisitorTypingToFalse() {
        setVisitorTyping(false)
    }

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

                {visitorTyping &&
                    <div className="type-container">
                        <div className="type-block">
                            <div className="type-dot"></div>
                            <div className="type-dot"></div>
                            <div className="type-dot"></div>
                        </div>
                    </div>
                }
            </div>
        </>
    )
}

export default DetailsBody