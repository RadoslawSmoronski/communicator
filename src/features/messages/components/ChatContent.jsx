import React from 'react';

import MessageBox from './messageBox/MessageBox'
import FriendBar from './friendBar/FriendBar';
import SendMessageBox from './sendMessageBox/SendMessageBox';

const ChatContent = () => {

    return (
        <>
            <FriendBar />
            <MessageBox />
            <SendMessageBox />
        </>
    )
}

export default ChatContent