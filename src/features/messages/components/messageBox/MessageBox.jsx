import React, { useEffect, useContext, useRef } from 'react'

import { ChatsContext } from '../../../../app/providers/ChatsProvider';
import { UserContext } from '../../../../app/providers/UserProvider';
import { FriendsContext } from '../../../../app/providers/FriendsProvider';

import { useGetMessages } from '../../hooks/useGetMessages';
import MessageTile from './MessageTile';
import FriendDetailsPanel from './FriendDetailsPanel';

const MessageBox = () => {
    const { user } = useContext(UserContext);
    const { activeFriend } = useContext(FriendsContext);
    const {
        selectedId: selectedChatId,
        messages,
        noNewMessagesFlagsMap,
        lastReadMessageIdsMap
    } = useContext(ChatsContext);

    const { getMessagesForFriend, loading } = useGetMessages();

    // scroll bar ref
    const scrollMessageBoxRef = useRef(null);

    // UI variables
    const currentMessages = messages?.[selectedChatId] || [];
    const hasNoMore = noNewMessagesFlagsMap?.[selectedChatId];
    const lastReadId = lastReadMessageIdsMap?.[selectedChatId];

    // fetch messages
    useEffect(() => {
        if (!selectedChatId || hasNoMore || loading) return;

        const checkAndFetch = async () => {
            const box = scrollMessageBoxRef.current;
            if (!box) return;

            // 1. There is only one message (init fetch)
            const isInitialFetch = currentMessages.length === 1;

            // 2. Messages aren't filled up to the scroll box
            const isNotScrollable = box.clientHeight >= box.scrollHeight;

            if (isInitialFetch || isNotScrollable) {
                await getMessagesForFriend(selectedChatId);
            }
        };

        checkAndFetch();

    }, [selectedChatId, currentMessages.length, hasNoMore, loading]);

    // scroll bar logic
    const handleScrollMessageBox = (e) => {
        // TODO fetch history of chat
    };


    return (
        <div
            id='messageBox'
        >
            <div className='messageBoxContent'
                ref={scrollMessageBoxRef}
                onScroll={handleScrollMessageBox}
            >
                {/* Message list */}
                {currentMessages.length > 0 && currentMessages.map((message) => (
                    <MessageTile
                        key={message.messageId}
                        mess={message.content}
                        yours={message.senderId === user.userID}
                        time={message.timestamp}
                        isLastReadByFriend={lastReadId === message.messageId}
                    />
                ))}

                {/* info inside chat */}
                <div className="status-container" style={{ textAlign: 'center', padding: '10px' }}>
                    {selectedChatId ? (
                        currentMessages.length > 0 ? (
                            hasNoMore && <span className='textCenter'>--- End of conversation ---</span>
                        ) : (
                            !loading && <span className='textCenter'>--- Start a conversation ---</span>
                        )
                    ) : (
                        <span className='textCenter'>--- Select a chat to start messaging ---</span>
                    )}

                    {loading && <div className="spinner">Loading messages...</div>}
                </div>
            </div>

            {/* TODO - create flag for displaying UI */}
            {activeFriend &&
                <FriendDetailsPanel
                    friendName={activeFriend.friendUsername}
                    friendshipId={activeFriend.friendshipId}
                    setDisplay={null}
                    showConfirmationBox={() => toggleUI("confirmationBox", true)}
                    closeConfirmationBox={() => toggleUI("confirmationBox", false)}
                    setFriend={null}
                    setChat={null}
                    friendState={activeFriend}
                />
            }
        </div>
    )
}

export default MessageBox