import React, { createContext, useState, useContext, useEffect } from "react";

import APIs from "../../api/ApiURL";
import axios from "../../api/axios";
import { AuthContext } from "./AuthProvider";
import { UserContext } from "./UserProvider";
import listUtils from "../../shared/utils/listUtils";
import { mapFriendToActiveFriend } from "../../features/users/mappers/activeFriendMapper";
import { mapFriendList } from "../../features/users/mappers/chatMapper";

import { chatDtoMock } from "../../features/users/mocks/getChatsMock";

export const FriendsContext = createContext();

const FriendsProvider = ({ children }) => {
    const { accessToken, refreshAccessToken, setAuth } = useContext(AuthContext);
    const { user } = useContext(UserContext);

    const [friendList, setFriendList] = useState([]); // from useFriendList
    const [friendListFiltered, setFriendListFiltered] = useState([]);
    const [activeFriend, setActiveFriend] = useState(null);
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState(null);


    const clearFriendList = () => {
        setFriendList([]);
        setActiveFriend(null);
    }

    const refreshFriendList = async () => {
        await fetchFriends();
    }

    const selectFriend = (friend) => {
        setActiveFriend(
            mapFriendToActiveFriend(friend)
        );
    }

    const fetchFriendsMock = async () => {
        try {
            setLoading(true);

            const result = chatDtoMock;

            const friendsWithNotify = mapFriendList(result);
            const sortedFriendList =
                listUtils.returnSortedByLastMessDateFriendsList(friendsWithNotify);

            setFriendList(sortedFriendList);
            setFriendListFiltered(sortedFriendList);
        } catch (err) {
            console.error(err);
        } finally {
            setLoading(false);
        }
    };

    const fetchFriends = async () => {
        try {
            const data = await axios.get(APIs.GET_CHATS(user.userID), {
                withCredentials: true,
                headers: { Authorization: `Bearer ${accessToken}` },
            });

            if (data.status === 200) {
                const result = data.data;
                console.log("GetFriends");
                console.log(result);

                // add new value (newMessNotify - notification) to listOfFriends
                const friendsWithNotify = mapFriendList(result);

                const sortedFriendList = listUtils.returnSortedByLastMessDateFriendsList(friendsWithNotify);

                setFriendList(sortedFriendList);

                // add chats to allMessages, pageNumbersForMessages
                // setChat(prev => {
                //     const messages = { ...prev.messages };

                //     sortedFriendList.forEach(friend => {
                //         if (!(friend.conversationId in messages)) {
                //             // last message as a last message in list
                //             let senderId = friend.isFriendSenderMessage ? friend.friendId : userId;
                //             if (friend.lastMessageId != null) {
                //                 messages[friend.conversationId] = [{
                //                     messageId: friend.lastMessageId,
                //                     conversationId: friend.conversationId,
                //                     senderId: senderId,
                //                     content: friend.lastMessageContent,
                //                     timestamp: friend.lastMessageTimestamp,
                //                     isRead: false
                //                 }];
                //             } else {
                //                 messages[friend.conversationId] = [];
                //             }


                //         }
                //     });

                //     return {
                //         ...prev,
                //         messages
                //     };
                // });
            }
        } catch (err) {
            if (err.response?.status === 401) {
                // await refreshAccessToken();
                // await getFriends();
            } else if (err.response?.status === 400) {
                // setFriend(prev => ({
                //     ...prev,
                //     findStatus: 'bad ID',
                // }));
            } else if (err.response?.status === 404) {
                // setFriend(prev => ({
                //     ...prev,
                //     findStatus: 'not found',
                // }));
            } else {
                console.error(err);
            }
        } finally {
            setLoading(false);
        }
    }

    useEffect(() => {
        fetchFriendsMock();
    }, [accessToken]);

    return (
        <FriendsContext.Provider value={{
            friendList,
            friendListFiltered,
            setFriendList,
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