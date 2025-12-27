import React from 'react';
import { Link } from 'react-router-dom';

import Avatar from '../shared/components/Avatar';

const UserInfoPanel = ({ avatarUrl, username, fullname, email, togglePanel }) => {
    return (
        <div className='userInfoPanel'>
            <div className='imageAndNameWrapperIP'>
                <Avatar url={avatarUrl} size={60} />
                <div
                    className='friendTileUserName'
                    style={{
                        width: '65%',
                        display: 'flex',
                        alignItems: 'center',
                        paddingLeft: '15px'
                    }}
                >
                    {username}
                </div>
            </div>
            <div className='friendTile userInfoTile'>
                Fullname: {fullname == null ? 'not given' : fullname}
            </div>
            <div className='friendTile userInfoTile'>
                Email: {email == null ? 'not given' : email}
            </div>

            <button className='btn2' onClick={() => togglePanel('editProfilePanel')}>
                Edit the profile
            </button>
        </div>
    );
};

export default UserInfoPanel;
