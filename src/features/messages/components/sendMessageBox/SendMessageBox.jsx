import React, { useState, useContext } from 'react';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import { faPaperPlane } from '@fortawesome/free-solid-svg-icons';
import { ChatsContext } from '../../../../app/providers/ChatsProvider';

import useSendMessage from '../../hooks/useSendMessage';

const SendMessageBox = () => {
    const { selectedId } = useContext(ChatsContext);

    const { sendMessage } = useSendMessage();

    const [messageInput, setMessageInput] = useState('');

    const handleChangeTxt = (e) => {
        setMessageInput(e.target.value);
    };

    const sendMessageToFriend = async () => {
        if (!messageInput.trim() || !selectedId) return;

        // send message
        try {
            await sendMessage(messageInput);

            setMessageInput('');
        } catch (err) {
            console.error("Error while sending message:", err);
        }

        setMessageInput('');
    };
    const handleKeyDown = (e) => {
        if (e.key === 'Enter') {
            sendMessageToFriend();
        }
    };

    const isChatSelected = !!selectedId;

    return (
        <div id='sendMessageBox'>
            <input
                className='textInput2 sendMessageInput'
                placeholder={isChatSelected ? 'Select chat...' : 'Type a message...'}
                value={messageInput}
                onChange={handleChangeTxt}
                onKeyDown={handleKeyDown}
                name='messageInput'
                type='text'
                autoComplete='off'
                disabled={!isChatSelected}
            />
            <FontAwesomeIcon
                icon={faPaperPlane}
                className={`friendBarIcon ${(!isChatSelected || !messageInput) ? 'disabledSendButton' : ''}`}
                onClick={isChatSelected ? sendMessageToFriend : null}
            />
        </div>

    )
}

export default SendMessageBox