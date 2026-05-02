import React, { useContext } from 'react';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import { faPhone, faCircleInfo, faArrowLeft } from '@fortawesome/free-solid-svg-icons';

import { FriendsContext } from '../../../../app/providers/FriendsProvider';
import { ChatUIContext } from '../../providers/ChatUIProvider';
import { PanelUIContext } from '../../../../app/providers/PanelUIProvider';
import Avatar from '../../../../shared/components/Avatar';

const FriendBar = () => {
    const { activeFriend } = useContext(FriendsContext);
    const {
        toggleFriendDetails,
        showMobileFriendList,
        newNotificationBackBtn,
    } = useContext(ChatUIContext);
    const { hidePanels } = useContext(PanelUIContext);

    const onMobileBackBtn = () => {
        showMobileFriendList();
        hidePanels();
    }

    return (
        <div id='friendBar'>
            {activeFriend &&
                <>
                    <div className='backArrowCointainer'>
                        <div className='friendBarIcon backArrow' onClick={onMobileBackBtn}>
                            <FontAwesomeIcon icon={faArrowLeft} />
                            {newNotificationBackBtn &&
                                <div className="newMessageNotificationBackBtn" />
                            }
                        </div>
                    </div>
                    <div className='friendBarIconBox'>
                        <Avatar url={activeFriend.avatarUrl} >
                            <div className={activeFriend.isOnline ? "onlineBadge online" : "onlineBadge offline"} />
                        </Avatar>
                    </div>
                    <div className='friendBarUserName'>{activeFriend.username}</div>
                    <div className='friendBarRightBox'>
                        <FontAwesomeIcon icon={faPhone} className='friendBarIcon' />
                        <FontAwesomeIcon icon={faCircleInfo} className='friendBarIcon' onClick={() =>
                            toggleFriendDetails()}
                        />
                    </div>
                </>
            }
        </div>
    )
}

export default FriendBar