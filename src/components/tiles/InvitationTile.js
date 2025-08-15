import React, { useContext } from 'react';
import Avatar from '../Avatar';

const InvitationTile = ({ invitationId, senderId,  username, invitationAction, avatarUrl }) => {
    const acceptInvitation = () => {
        invitationAction('accept', invitationId, senderId);
    };

    const rejectInvitation = () => {
        invitationAction('reject', invitationId, senderId);
    };

    return (
        <div className='friendTile invitationTile'>
            <Avatar url={avatarUrl}/>
            <div className='friendTileWrapper invitationWrapper'>
                <div className='friendTileUserName invitationTileText'>
                    <span className='highlightColor'>{username}</span> sent you an invitation
                </div>
                <div className='personBtnWrapper'>
                    <div className='btnPerson btnInvitation' onClick={acceptInvitation}>Accept</div>
                    <div className='btnPerson btnInvitation' onClick={rejectInvitation} style={{ backgroundColor: '#bf2d10' }}>Reject</div>
                </div>
            </div>
        </div>
    );
};

export default InvitationTile;
