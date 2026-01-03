import React, { useContext } from 'react';

import Avatar from '../../../shared/components/Avatar';
import { UserContext } from '../../../app/providers/UserProvider';

const UserInfoPanel = ({ display, togglePanel }) => {
    const { user } = useContext(UserContext);

    return (
        <>
            {
                display &&
                <div className='userInfoPanel'>
                    <div className='imageAndNameWrapperIP'>
                        <Avatar url={user.avatarUrl} size={60} />
                        <div
                            className='friendTileUserName'
                            style={{
                                width: '65%',
                                display: 'flex',
                                alignItems: 'center',
                                paddingLeft: '15px'
                            }}
                        >
                            {user.username}
                        </div>
                    </div>
                    <div className='friendTile userInfoTile'>
                        Fullname: {user.fullname == null ? 'not given' : user.fullname}
                    </div>
                    <div className='friendTile userInfoTile'>
                        Email: {user.email == null ? 'not given' : user.email}
                    </div>

                    <button className='btn2' onClick={() => togglePanel('editProfilePanel')}>
                        Edit the profile
                    </button>
                </div>
            }
        </>

    );
};

export default UserInfoPanel;
