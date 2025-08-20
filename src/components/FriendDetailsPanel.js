import React, { useState, useContext } from "react";
import { AuthContext } from "../context/AuthProvider";

import axios from "../api/axios";
import APIs from "../api/ApiURL";

const FriendDetailsPanel = ({ friendName, friendshipId }) => {
    const { accessToken, refreshAccessToken, saveToCookie } = useContext(AuthContext);

    const removeFriend = async () => {
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

            if (res.status === 204) {
                console.log("Friendship removed");
                // after friendship removal actions
                // close chat or archive?
                // remove friend tile from chat list
                // edit cookie?
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

    return (
        <div className='friendDetailsPanel'>
            Details about {friendName}
            <button className='btn2 delete' onClick={removeFriend}>Remove friend</button>
        </div>
    )
}

export default FriendDetailsPanel