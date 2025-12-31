import React, { createContext, useContext } from 'react'
import { useFriendInvitations } from '../hooks/useFriendInvitations'

export const InvitationsContext = createContext();

const InvitationsProvider = ({ children }) => {
    const {
        invitationsList,
        removeInvitationById,
        loading,
        error
    } = useFriendInvitations();

    return (
        <InvitationsContext.Provider value={{
            invitationsList,
            removeInvitationById,
            loading,
            error
        }}>
            {children}
        </InvitationsContext.Provider>
    )
}

export default InvitationsProvider;