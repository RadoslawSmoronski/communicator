import React, { useContext } from 'react'
import InvitationTile from './InvitationTile'

import { useInvitationAction } from '../hooks/useInvitationAction'
import { InvitationsContext } from '../providers/InvitationsProvider'

const InvitationList = ({ display }) => {
    const { invitationsList,
        removeInvitationById } = useContext(InvitationsContext);

    const { acceptInvitation, declineInvitation } =
        useInvitationAction(removeInvitationById);


    return (
        <>
            {display && (
                <div id='invitationsList'>
                    {invitationsList && invitationsList.length > 0 ? (
                        invitationsList.map(inv => (
                            <InvitationTile
                                key={inv.id}
                                username={inv.username}
                                avatarUrl={inv.avatarUrl}
                                acceptInvitation={() => acceptInvitation(inv.id)}
                                declineInvitation={() => declineInvitation(inv.id)}
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