import React, { useState, useContext, useEffect } from 'react'

import PeopleAndFriends from '../../features/users/components/PeopleAndFriends'
import Layout from '../../shared/components/Layout';
import ChatContent from '../../features/messages/components/ChatContent';
import ConfirmationBox from '../../shared/components/form/ConfirmationBox';

import { FriendsContext } from '../../app/providers/FriendsProvider';
import { ChatUIContext } from '../../features/messages/providers/ChatUIProvider';
import useRemoveFriend from '../../features/friendShips/hooks/useRemoveFriend';

export const MessagePageRefactor = () => {
    const { activeFriend } = useContext(FriendsContext);
    const { mobileActiveChat } = useContext(ChatUIContext);
    const { removeFriend, error } = useRemoveFriend();

    return (
        <div id='mainMessagePage' className={mobileActiveChat ? 'mobile-chat-open' : ''}>

            <Layout />
            <PeopleAndFriends />
            <ChatContent />

            <ConfirmationBox
                confirmFunc={() => removeFriend(activeFriend.friendshipId)}
                text={error ?? `Do you want to remove ${activeFriend?.username} from friendlist?`}
            />
        </div>
    )
}
