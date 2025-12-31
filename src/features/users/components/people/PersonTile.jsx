import React, { useContext, useState } from 'react';

import { useSendInvitation } from '../../../friendInvitations/hooks/useSendInvitation';

import Avatar from '../../../../shared/components/Avatar';

const PersonTile = ({ recipientId, username, isInvited, avatarUrl }) => {
    const { sendInvitation, loading, sent } = useSendInvitation();

    const [sendBtnIsActive, setSendBtnIsActive] = useState(!isInvited);

    const handleSendInvitation = async () => {
        const result = await sendInvitation(recipientId);

        if (result?.success || result?.reason === "ALREADY_EXISTS") {
            setSendBtnIsActive(false);
        }
    };


    return (
        <div className='friendTile'>
            <Avatar url={avatarUrl} />
            <div className='friendTileWrapper'>
                <div className='friendTileUserName'>{username}</div>
                <div className='personBtnWrapper'>
                    {sendBtnIsActive ? (
                        <div className='btnPerson' onClick={handleSendInvitation}>Add friend</div>
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
