import React, { useState, useContext, useEffect } from "react";
import { AuthContext } from "../context/AuthProvider";

import axios from "../api/axios";
import APIs from "../api/ApiURL";
import cookieUtils from "../utils/cookieUtils";

const FriendDetailsPanel = ({
    friendName, friendshipId, setDisplay, showConfirmationBox, closeConfirmationBox, friendState, setFriend, setChat
}) => {
    const { accessToken, refreshAccessToken, userId } = useContext(AuthContext);
    const confText = `Do you want to remove ${friendName} from friendlist?`

    useEffect(() => {
        setDisplay(prev => ({
            ...prev,
            confirmationBoxFunc: removeFriend,
            confirmationBoxText: confText
        }))
    }, []);

    const removeFriend = async () => {
        console.log("friend removed!");
        afterRemovalActions();
        try {
            const res = await axios.delete(APIs.FRIENDSHIP(friendshipId),
                {
                    withCredentials: true,
                    headers: {
                        'Authorization': `Bearer ${accessToken}`,
                        'Content-Type': 'application/json'
                    }
                }
            );

            if (res.status === 200) {
                console.log("Friendship removed");
                // after friendship removal actions
                afterRemovalActions();
            }
        } catch (err) {
            if (err.response?.status === 401) {
                await refreshAccessToken();
                await removeFriend();
            } else {
                let errorFeedback = err.response?.data.detail;
                if (errorFeedback) {
                    console.error(errorFeedback);
                }
            }
        }
    }


    const afterRemovalActions = () => {
        // remove from lists
        let friendList = friendState.list.filter(f => f.friendshipId !== friendshipId);
        let friendListFiltered = friendState.list_filtered.filter(f => f.friendshipId !== friendshipId);

        // reset last opened chat info
        setFriend(prev => ({
            ...prev,
            list: friendList,
            list_filtered: friendListFiltered,
            activeFriendName: '',
            activeFriendAvatarUrl: null,
            activeFriendOnlineStatus: false,
            activeFriendshipId: null
        }))
        setChat(prev => ({
            ...prev,
            selectedId: null,
            activeReciepientId: null
        }))
        setDisplay(prev =>({
            ...prev,
            friendDetailsPanel: false
        }))

        // delete chatId from cookie
        let lastOpenedChatsSet = cookieUtils.get('lastOpenedChatSet') || {};
        delete lastOpenedChatsSet[userId];
        cookieUtils.set('lastOpenedChatSet', lastOpenedChatsSet);
        
        closeConfirmationBox();
    }

    return (
        <div className='friendDetailsPanel'>
            Details about {friendName}
            <button className='btn2 delete' onClick={() => showConfirmationBox()}>Remove friend</button>
        </div>
    )
}

export default FriendDetailsPanel