import React, { useState, useContext, useCallback } from 'react'
import { useApi } from '../../../shared/hooks/useApi'
import APIs from '../../../api/ApiURL'

import { FriendsContext } from '../../../app/providers/FriendsProvider'
import { ChatsContext } from '../../../app/providers/ChatsProvider'
import { ChatUIContext } from '../../messages/providers/ChatUIProvider'
import { UserContext } from '../../../app/providers/UserProvider'

import { useLastOpenedChat } from '../../users/hooks/useLastOpenedChat'

const useRemoveFriend = () => {
    const api = useApi();

    const { user } = useContext(UserContext);
    const {
        setFriendList,
        removeActiveFriend
    } = useContext(FriendsContext);
    const {
        selectChat
    } = useContext(ChatsContext);
    const {
        hideFriendDetails,
        hideConfirmationBox
    } = useContext(ChatUIContext);

    const { removeLastOpenedChat } = useLastOpenedChat();

    const [error, setError] = useState(null);

    const afterRemovalActions = (friendshipId) => {
        // Remove from friend list
        setFriendList(prev => prev.filter(f => f.friendshipId !== friendshipId));

        // Clear active friend
        removeActiveFriend();

        // Clear selected chat
        selectChat("", "");

        // Close UI panels
        hideFriendDetails();
        hideConfirmationBox();

        // Delete from cookie
        removeLastOpenedChat(user.userId);
    }

    const removeFriend = async (friendshipId) => {
        setError(null);

        try {
            await api.delete(APIs.FRIENDSHIP(friendshipId));

            console.log(`Friendship ${friendshipId} removed successfully`);
            afterRemovalActions(friendshipId);

        } catch (err) {
            console.error("Error removing friend:", err);

            const errorFeedback = err.response?.data?.detail || "An error occurred while removing friend.";
            setError(errorFeedback);

            // setDisplayConfirmationBox(false); 
        }
    }

    return {
        removeFriend,
        error
    };
}

export default useRemoveFriend