import React from 'react';

import MessageBox from './messageBox/MessageBox'
import FriendBar from './friendBar/FriendBar';
import SendMessageBox from './sendMessageBox/SendMessageBox';
import useChatListeners from '../hooks/useChatListeners';

const ChatContent = () => {
    useChatListeners();

    return (
        <>
            <FriendBar />
            <MessageBox />
            <SendMessageBox />
        </>
    )
}

export default ChatContent