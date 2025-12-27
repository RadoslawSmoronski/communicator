import React, { useState, useContext, useEffect } from 'react'

import { FriendsContext } from '../../../../app/providers/FriendsProvider';
import FriendTile from './FriendTile';

const FriendList = ({ searchQuery }) => {
    const {
        friendList,
        friendListFiltered,
        setFriendList,
        activeFriend,
        loading,
        error,
        selectFriend,
        refreshFriendList,
        clearFriendList
    } = useContext(FriendsContext);

    return (

        <>
            {
                friendList.length > 0 ? (
                    friendListFiltered.length > 0 ? (
                        friendListFiltered
                            .filter(f => f.friendshipId !== null)
                            .map(f => (
                                <FriendTile
                                    key={f.friendId}
                                    username={f.friendUsername}
                                    onClick={() => handleClickingOnChat(f.conversationId, f.friendId, f.friendUsername, f.friendAvatarUrl, f.friendshipId)}
                                    author={f.isFriendSenderMessage ? '' : 'You: '}
                                    mess={f.lastMessageContent}
                                    messTimestamp={f.lastMessageTimestamp}
                                    // selected={chat.selectedId === f.conversationId}
                                    selected={null}
                                    newMessageNotify={f.newMessNotify}
                                    avatarUrl={f.friendAvatarUrl}
                                    isOnline={f.isFriendOnline}
                                />
                            ))
                    ) : (
                        <div className='infoText'>There are no friends like {searchQuery} ...</div>
                    )
                ) : (
                    <div className='infoText'>You have zero friends</div>
                )
            }
        </>
    )
}

export default FriendList