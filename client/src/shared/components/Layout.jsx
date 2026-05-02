import React, { useState, useContext, useEffect } from 'react'

import InvitationsProvider from '../../features/friendInvitations/providers/InvitationsProvider';

import InvitationList from '../../features/friendInvitations/components/InvitationList'
import UserInfoPanel from '../../features/profile/components/UserInfoPanel';
import EditProfilePanel from '../../features/profile/components/EditProfilePanel';
import MenuBar from './MenuBar';

import { PanelUIContext } from '../../app/providers/PanelUIProvider';

const Layout = () => {
    const { display, togglePanel } = useContext(PanelUIContext);

    return (
        <>
            <InvitationsProvider>
                <InvitationList display={display.invitationList} />
                <MenuBar
                    togglePanel={togglePanel}
                />
            </InvitationsProvider>


            <UserInfoPanel
                display={display.userInfoPanel}
                togglePanel={togglePanel}
            />

            <EditProfilePanel
                display={display.editProfilePanel}
                togglePanel={togglePanel}
            />
        </>
    )
}

export default Layout