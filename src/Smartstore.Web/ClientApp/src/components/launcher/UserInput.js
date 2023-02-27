import React, { useRef } from 'react'
import SendIcon from './icons/SendIcon';
import { useState } from 'react';
import { postLauncher } from '../utils/HttpClient';
import { useLocation } from 'react-router-dom';

const UserInput = (props) => {
    const userInputRef = useRef(null);
    const location = useLocation();

    let [inputActive, setInputActive] = useState(false);
    const handleKey = async (event) => {
        if (event.keyCode === 13 && !event.shiftKey) {
            submitText(event);
        }
    }

    const submitText = async (event) => {
        event.preventDefault();
        const text = userInputRef.current.textContent;
        if (text && text.length > 0) {
            const model = {
                Message: text,
                Type: 'text'
            }
            const result = await postLauncher('chat/SendText', model, location);

            if (result.IsValid) {
                props.onSubmit(result.Data);
                userInputRef.current.innerHTML = '';               
            }
        }
    }

    return (
        <form className={`app-customer-input ${(inputActive ? 'active' : '')}`}>
            <div
                role="button"
                tabIndex="0"
                onFocus={() => { setInputActive(true); }}
                onBlur={() => { setInputActive(false); }}
                ref={userInputRef}
                onKeyDown={handleKey.bind(this)}
                contentEditable="true"
                placeholder="Write a reply..."
                className="app-customer-input-text"
            >
            </div>
            <div className="app-customer-input-buttons">
                <div className="app-customer-input-button"></div>
                <div className="app-customer-input-button">
                    <SendIcon onClick={submitText.bind(this)} />
                </div>
            </div>
        </form>
    );
}

export default UserInput;
