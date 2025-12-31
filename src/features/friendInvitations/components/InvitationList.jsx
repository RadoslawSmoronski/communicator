import React, { useContext } from 'react'
import InvitationTile from './InvitationTile'

import { useFriendInvitations } from '../hooks/useFriendInvitations'

const InvitationList = ({ display }) => {
    const {
        invitationsList,
        loading,
        error
    } = useFriendInvitations(true);

    return (
        <>
            {display && (
                <div id='invitationsList'>
                    {invitationsList && invitationsList.length > 0 ? (
                        invitationsList.map(inv => (
                            <InvitationTile
                                key={inv.id}
                                invitationId={inv.id}
                                senderId={inv.senderId}
                                username={inv.username}
                                avatarUrl={inv.avatarUrl}
                            // invitationAction={invitationActions}
                            />
                        ))
                    ) : (
                        <div className='infoText'>No new invitations</div>
                    )}
                </div>
            )}

        </>
    )
}

export default InvitationList