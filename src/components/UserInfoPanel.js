import React from 'react';
import { Link } from 'react-router-dom';

const UserInfoPanel = ({ username, fullname, email }) => {
    return (
        <div className='userInfoPanel'>
            <div className='imageAndNameWrapperIP'>
                <div
                    className='friendTileIcon'
                    style={{ width: '60px', height: '60px' }}
                />
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

            <Link
                to="/editprofile"
                style={{
                    textAlign: 'center',
                    marginTop: '5px'
                }}
            >
                Edit the profile
            </Link>
        </div>
    );
};

export default UserInfoPanel;
