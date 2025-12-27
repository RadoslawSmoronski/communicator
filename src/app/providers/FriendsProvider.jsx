import React, { createContext, useState, useContext, useEffect } from "react";

import { useGetChats } from "../../features/users/hooks/useGetChats";
import { mapFriendToActiveFriend } from "../../features/users/mappers/activeFriendMapper";


export const FriendsContext = createContext();

const FriendsProvider = ({ children }) => {
    const [activeFriend, setActiveFriend] = useState(null);

    const {
        friendList,
        setFriendList,
        friendListFiltered,
        setFriendListFiltered,
        loading,
        error,
        getChats
    } = useGetChats(true);

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
    }


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