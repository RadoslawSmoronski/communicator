import React, { useContext } from 'react'
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { faBell } from "@fortawesome/free-solid-svg-icons";

import { InvitationsContext } from '../../features/friendInvitations/providers/InvitationsProvider';
import { UserContext } from '../../app/providers/UserProvider';

import Avatar from './Avatar';

const MenuBar = ({ togglePanel }) => {
    const { user, removeUser } = useContext(UserContext);
    const { invitationsList } = useContext(InvitationsContext);

    const numberOfInvitations = invitationsList.length;

    // Sings out user - reset states, session Storage
    const signOut = () => {
        removeUser();
    };

    return (
        <div id='menuBar'>
            <div id='logoInMenu' />
            <div id='profileBox'>

                <div className='bellWrapper' onClick={() => togglePanel('invitationList')}>
                    <FontAwesomeIcon icon={faBell} className='friendBarIcon bell' />
                    {invitationsList && numberOfInvitations > 0 && (
                        <div className='notificationBadge'>{numberOfInvitations}</div>
                    )}
                </div>


                <button className='btn2 sing-out' onClick={signOut}>Sign out</button>
                <div className='profileInfoWrapper' onClick={() => togglePanel('userInfoPanel')}>
                    <div className='profileUsername'>
                        {user.username}
                    </div>
                    <Avatar url={user.avatarUrl} />
                </div>
            </div>
        </div>
    )
}

export default MenuBar