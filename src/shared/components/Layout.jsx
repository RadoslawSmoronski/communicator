import React, { useState, useContext, useEffect } from 'react'

import InvitationList from '../../features/friendInvitations/components/InvitationList'
import { UserContext } from '../../app/providers/UserProvider';

const Layout = () => {
    const { user, removeUser } = useContext(UserContext);

    const [display, setDisplay] = useState({
        invitationList: true,
        userInfoPanel: false,
        editProfilePanel: false
    });


    return (
        <>
            <InvitationList display={display.invitationList} />
        </>
    )
}

export default Layout