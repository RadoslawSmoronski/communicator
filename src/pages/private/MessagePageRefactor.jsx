import React, { useState, useContext, useEffect } from 'react'

import PeopleAndFriends from '../../features/users/components/PeopleAndFriends'
import Layout from '../../shared/components/Layout';
import ChatContent from '../../features/messages/components/ChatContent';
import ConfirmationBox from '../../shared/components/form/ConfirmationBox';

import { FriendsContext } from '../../app/providers/FriendsProvider';

export const MessagePageRefactor = () => {
    const { activeFriend } = useContext(FriendsContext);

    return (
        <div id='mainMessagePage'>

            <Layout />
            <PeopleAndFriends />
            <ChatContent />

            <ConfirmationBox
                // confirmFunc={display.confirmationBoxFunc}
                text={`Do you want to remove ${activeFriend?.username} from friendlist?`}
            />
        </div>
    )
}
