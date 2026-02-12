import React, { useContext } from 'react';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import { faPhone, faCircleInfo } from '@fortawesome/free-solid-svg-icons';

import { FriendsContext } from '../../../../app/providers/FriendsProvider';
import Avatar from '../../../../shared/components/Avatar';

const FriendBar = () => {
    const { activeFriend } = useContext(FriendsContext);

    return (
        <div id='friendBar'>
            {activeFriend &&
                <>
                    <div className='friendBarIconBox'>
                        <Avatar url={activeFriend.avatarUrl} >
                            <div className={activeFriend.isOnline ? "onlineBadge online" : "onlineBadge offline"} />
                        </Avatar>
                    </div>
                    <div className='friendBarUserName'>{activeFriend.username}</div>
                    <div className='friendBarRightBox'>
                        <FontAwesomeIcon icon={faPhone} className='friendBarIcon' />
                        <FontAwesomeIcon icon={faCircleInfo} className='friendBarIcon' onClick={() => toggleUI("friendDetailsPanel", !display.friendDetailsPanel)} />
                    </div>
                </>
            }
        </div>
    )
}

export default FriendBar