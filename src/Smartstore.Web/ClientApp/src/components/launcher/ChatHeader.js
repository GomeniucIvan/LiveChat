import React, { Component } from 'react';
import closeIcon from './assets/close-icon.png';


const ChatHeader = (props) => {
    return (
        <div className="app-header">
            <img className="app-header-image" src={props.imageUrl} alt="" />
            <div className="app-header-member"> {props.teamName} </div>
            <div className="app-header-close-btn" onClick={props.onClose}>
                <img src={closeIcon} alt="" />
            </div>
        </div>
    );
}

export default ChatHeader;
