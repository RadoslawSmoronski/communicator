import React, { useState, useContext, useEffect } from 'react'

import PeopleAndFriends from '../../features/users/components/PeopleAndFriends'
import Layout from '../../shared/components/Layout';
import ChatContent from '../../features/messages/components/ChatContent';


export const MessagePageRefactor = () => {

    return (
        <div id='mainMessagePage'>

            <Layout />
            <PeopleAndFriends />
            <ChatContent />
        </div>
    )
}
