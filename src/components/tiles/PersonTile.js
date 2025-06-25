import React, { useContext, useState } from 'react';

import { AuthContext } from '../../context/AuthProvider';
import axios from '../../api/axios';
import APIs from '../../api/ApiURL';

const PersonTile = ({ userId, username, isInvited }) => {
    const { userID, accessToken, refreshAccessToken } = useContext(AuthContext);
    const [sendBtnIsActive, setSendBtnIsActive] = useState(!isInvited);

    const sendInvitation = async () => {
        console.log("My id: " + userID + " yours id: " + userId);

        try {
            const data = await axios.post(
                APIs.SEND_INVITE,
                JSON.stringify({
                    senderId: userID,
                    recipientId: userId
                }),
                {
                    withCredentials: true,
                    headers: {
                        Authorization: `Bearer ${accessToken}`,
                        'Content-Type': 'application/json'
                    }
                }
            );

            const res = data.data;

            if (data.status === 200) {
                console.log(res.title);
                console.log(res.traceId);
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
            <div className='friendTileIcon' />
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
