import React, { createContext, useState, useContext, useEffect } from "react";

import { useGetChats } from "../../features/users/hooks/useGetChats";
import { lastOpenedChatService } from "../../features/users/services/lastOpenedChatService";

import { mapFriendToActiveFriend, mapCookieToActiveFriend } from "../../features/users/mappers/activeFriendMapper";

import { ChatsContext } from "./ChatsProvider";
import { UserContext } from "./UserProvider";

export const FriendsContext = createContext();

const FriendsProvider = ({ children }) => {
    const { activeRecipientId } = useContext(ChatsContext);
    const { user } = useContext(UserContext);

    const [activeFriend, setActiveFriend] = useState(null);

    const {
        friendList,
        setFriendList,
        friendListFiltered,
        setFriendListFiltered,
        loading,
        error,
        getChats
    } = useGetChats();

    const clearFriendList = () => {
        setFriendList([]);
        setActiveFriend(null);
    }

    const refreshFriendList = async () => {
        await getChats();
    }

    const selectFriend = (friend) => {
        setActiveFriend(
            mapFriendToActiveFriend(friend)
        );

        clearNotification(friend.conversationId);
    }

    const clearNotification = (conversationId) => {
        setFriendList(prev => prev.map(f =>
            f.conversationId === conversationId
                ? { ...f, newMessNotify: false }
                : f
        ));
    };

    // auto update selected friend
    // after page refresh
    useEffect(() => {
        if (activeRecipientId && activeFriend == null && user) {
            // load active friend from cookie
            const savedChatMap = lastOpenedChatService.load();
            const lastOpenedData = savedChatMap[user.userID];

            if (lastOpenedData && lastOpenedData.friendId === activeRecipientId) {
                const restoredFriend = mapCookieToActiveFriend(lastOpenedData);
                setActiveFriend(restoredFriend);
            }
        }
    }, [activeRecipientId]);

    return (
        <FriendsContext.Provider value={{
            friendList,
            friendListFiltered,
            setFriendList,
            setFriendListFiltered,

            activeFriend,
            loading,
            error,

            selectFriend,
            refreshFriendList,
            clearFriendList
        }}>
            {children}
        </FriendsContext.Provider>
    )
}

export default FriendsProvider;