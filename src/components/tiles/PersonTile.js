import React, { useContext, useState } from 'react';

import { AuthContext } from '../../context/AuthProvider';
import axios from '../../api/axios';
import APIs from '../../api/ApiURL';
import Avatar from '../Avatar';

const PersonTile = ({ recipientId, username, isInvited, avatarUrl }) => {
    const { userId, accessToken, refreshAccessToken } = useContext(AuthContext);
    const [sendBtnIsActive, setSendBtnIsActive] = useState(!isInvited);

    const sendInvitation = async () => {
        console.log("My id: " + userId + " yours id: " + recipientId);

        try {
            const data = await axios.post(
                APIs.SEND_INVITE,
                JSON.stringify({
                    senderId: userId,
                    recipientId: recipientId
                }),
                {
                    withCredentials: true,
                    headers: {
                        Authorization: `Bearer ${accessToken}`,
                        'Content-Type': 'application/json'
                    }
                }
            );

            if (data.status === 201) {
                setSendBtnIsActive(false);
            }

        } catch (err) {
            console.log(err);

            if (err.response && err.response.status === 401) {
                await refreshAccessToken();
                await sendInvitation(); // retry
            } else if (err.response?.status === 409) {
                console.error("An invitation has already exist");
                setSendBtnIsActive(false);
            } else if (err.response?.status === 400) {
                console.error("Validation error");
            } else {
                console.error(err);
            }
        }
    };

    return (
        <div className='friendTile'>
            <Avatar url={avatarUrl} />
            <div className='friendTileWrapper'>
                <div className='friendTileUserName'>{username}</div>
                <div className='personBtnWrapper'>
                    {sendBtnIsActive ? (
                        <div className='btnPerson' onClick={sendInvitation}>Add friend</div>
                    ) : (
                        <div className='btnPerson btnDisabled'>invitation sent</div>
                    )}
                    <div className='btnPerson' style={{ backgroundColor: '#bf2d10' }}>Block</div>
                </div>
            </div>
        </div>
    );
};

export default PersonTile;
