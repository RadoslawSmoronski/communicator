import React, { createContext, useState, useContext, useEffect, useMemo, useCallback } from "react";

import listUtils from "../../shared/utils/listUtils";
import { useGetChats } from "../../features/users/hooks/useGetChats";
import { useLastOpenedChat } from "../../features/users/hooks/useLastOpenedChat";

import { mapFriendToActiveFriend, mapCookieToActiveFriend } from "../../features/users/mappers/activeFriendMapper";

import { ChatsContext } from "./ChatsProvider";
import { UserContext } from "./UserProvider";

export const FriendsContext = createContext();

const FriendsProvider = ({ children }) => {
    const { activeRecipientId, selectedId: selectedChatId } = useContext(ChatsContext);
    const { user } = useContext(UserContext);

    const [activeFriend, setActiveFriend] = useState(null);

    const [searchQuery, setSearchQuery] = useState("");

    const {
        friendList,
        setFriendList,
        loading,
        error,
        getChats
    } = useGetChats();
    const {
        getLastOpenedChatByUserId,
        removeLastOpenedChat
    } = useLastOpenedChat();

    // auto filter friend list
    const friendListFiltered = useMemo(() => {
        const filtered = listUtils.returnFilteredFriends(friendList, searchQuery);
        return listUtils.returnSortedByLastMessDateFriendsList(filtered);
    }, [friendList, searchQuery]);

    const removeActiveFriend = () => {
        setActiveFriend(null);
    }

    const refreshFriendList = async () => {
        await getChats();
    }

    // sets active friend
    const selectFriend = (friend) => {
        setActiveFriend(
            mapFriendToActiveFriend(friend)
        );

        clearNotification(friend.conversationId);
    }

    // clears notifitions
    const clearNotification = (conversationId) => {
        setFriendList(prev => prev.map(f =>
            f.conversationId === conversationId
                ? { ...f, newMessNotify: false }
                : f
        ));
    };

    // it updates last message on friend tile
    const updateFriendFromMessage = useCallback((messageDto) => {
        const isInTheCurrentChat = messageDto.conversationId === selectedChatId;
        const isFromFriend = messageDto.senderId !== user.userID;

        setFriendList(prev => {
            const updated = prev.map(f => {
                if (f.conversationId === messageDto.conversationId) {
                    return {
                        ...f,
                        isFriendSenderMessage: isFromFriend,
                        lastMessageContent: messageDto.content,
                        lastMessageTimestamp: messageDto.timestamp,
                        newMessNotify: !isInTheCurrentChat && isFromFriend,
                    };
                }
                return f;
            });

            return listUtils.returnSortedByLastMessDateFriendsList(updated);
        });
    }, [selectedChatId, user?.userID]);

    // updates friend online status
    // it updates online status of a friend
    const updateFriendOnlineStatus = useCallback((friendId, isOnline) => {
        // 1. Update status on the list
        setFriendList(prev => prev.map(f =>
            f.friendId === friendId
                ? { ...f, isFriendOnline: isOnline }
                : f
        ));

        // 2. Update activeFriend if it's the person user is currently chatting with
        if (activeRecipientId === friendId) {
            setActiveFriend(prev => {
                return { ...prev, isOnline: isOnline };
            });
        }

    }, [activeRecipientId]);

    // auto update selected friend
    // after page refresh
    useEffect(() => {
        if (activeRecipientId && activeFriend == null && user) {
            // load active friend from cookie
            const lastOpenedData = getLastOpenedChatByUserId(user.userID);

            if (lastOpenedData && lastOpenedData.friendId === activeRecipientId) {

                const friendFromList = friendList.find(f => f.friendId === activeRecipientId);

                if (friendFromList) {
                    // update online status
                    const restoredFriend = mapCookieToActiveFriend(lastOpenedData);
                    restoredFriend.isOnline = friendFromList.isFriendOnline;
                    setActiveFriend(restoredFriend);
                }
            }
        }
    }, [activeRecipientId]);

    return (
        <FriendsContext.Provider value={{
            friendList,
            setFriendList,
            friendListFiltered,

            activeFriend,
            loading,
            error,

            selectFriend,
            refreshFriendList,
            removeActiveFriend,
            updateFriendFromMessage,
            updateFriendOnlineStatus,

            searchQuery,
            setSearchQuery
        }}>
            {children}
        </FriendsContext.Provider>
    )
}

export default FriendsProvider;