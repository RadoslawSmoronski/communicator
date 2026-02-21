import React, { useState, useContext, useEffect } from 'react'

import { FriendsContext } from '../../../../app/providers/FriendsProvider';
import { UserContext } from '../../../../app/providers/UserProvider';
import { ChatsContext } from '../../../../app/providers/ChatsProvider';
import { ChatUIContext } from '../../../messages/providers/ChatUIProvider';

import FriendTile from './FriendTile';
import Spinner from '../../../../shared/components/Spinner';

import { useLastOpenedChat } from '../../hooks/useLastOpenedChat';
import { mapFriendToLastOpenedChat } from '../../mappers/chatMapper';

const FriendList = ({ searchQuery }) => {
    const { user } = useContext(UserContext);
    const {
        friendList,
        friendListFiltered,
        selectFriend,
        loading
    } = useContext(FriendsContext);
    const {
        selectedId: selectedChatId,
        selectChat,
        activeRecipientId
    } = useContext(ChatsContext);
    const { hideFriendDetails, showMobileChat } = useContext(ChatUIContext);

    const { updateLastOpenedChat } = useLastOpenedChat();

    const handleClickingOnChat = (friend) => {
        showMobileChat(); // mobile

        if (friend.friendId === activeRecipientId) return;

        // save last opened chat
        // to cookie
        const lastOpenedChatData = mapFriendToLastOpenedChat(friend);
        updateLastOpenedChat(user.userID, lastOpenedChatData);

        // set active friend
        selectFriend(friend);

        // set active chatId and recipientId
        selectChat(friend.conversationId, friend.friendId);

        // hide friend details panel
        hideFriendDetails();
    }

    if (loading) return <Spinner />

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
                                    onClick={() => handleClickingOnChat(f)}
                                    author={f.isFriendSenderMessage ? '' : 'You: '}
                                    mess={f.lastMessageContent}
                                    messTimestamp={f.lastMessageTimestamp}
                                    selected={selectedChatId === f.conversationId}
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