import React, { useState, useContext, useEffect } from 'react'

import InvitationsProvider from '../../features/friendInvitations/providers/InvitationsProvider';

import InvitationList from '../../features/friendInvitations/components/InvitationList'
import UserInfoPanel from '../../features/profile/components/UserInfoPanel';
import EditProfilePanel from '../../features/profile/components/EditProfilePanel';
import MenuBar from './MenuBar';

const Layout = () => {
    const [display, setDisplay] = useState({
        invitationList: false,
        userInfoPanel: false,
        editProfilePanel: false
    });

    // Displays / hides given panel
    const togglePanel = (panelName) => {
        setDisplay(prev => {
            const panelsState = {
                invitationList: false,
                userInfoPanel: false,
                editProfilePanel: false,
            };

            panelsState[panelName] = !prev[panelName];
            return panelsState;
        });
    };

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