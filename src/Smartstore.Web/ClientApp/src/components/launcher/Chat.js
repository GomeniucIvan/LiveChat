import PropTypes from 'prop-types';
import React from 'react';
import ChatWindow from './ChatWindow';
import launcherIcon from './assets/logo-no-bg.svg';
import launcherIconActive from './assets/close-icon.png';
import './assets/scss/launcher.scss';

const Chat = (props) => {
    const classList = [
        'app-launcher',
        (props.isOpen ? 'opened' : ''),
    ];

    return (
        <div>
            <div>
            </div>
            <div className={classList.join(' ')} onClick={props.handleClick.bind(this)}>
                <MessageCount count={props.newMessagesCount} isOpen={props.isOpen} />
                <img className={"app-open-icon"} src={launcherIconActive} />
                <img className={"app-closed-icon"} src={launcherIcon} />
            </div>
            <ChatWindow
                msgListScrollRef={props.msgListScrollRef}
                messageList={props.messageList}
                onGuestSendMessage={props.onGuestSendMessage}
                agentProfile={props.agentProfile}
                isOpen={props.isOpen}
                companyTyping={props.companyTyping}
                onClose={props.handleClick.bind(this)}
            />
        </div>
    );
}

const MessageCount = (props) => {
  if (props.count === 0 || props.isOpen === true) { return null }
  return (
      <div className={"app-new-messsages-count"}>
      {props.count}
    </div>
  )
}

Chat.propTypes = {
  onMessageWasReceived: PropTypes.func,
  onGuestSendMessage: PropTypes.func,
  newMessagesCount: PropTypes.number,
  isOpen: PropTypes.bool,
  handleClick: PropTypes.func,
  messageList: PropTypes.arrayOf(PropTypes.object)
};

export default Chat;
