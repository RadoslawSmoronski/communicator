import { useContext } from 'react';

import { ChatsContext } from '../../../app/providers/ChatsProvider';
import { FriendsContext } from '../../../app/providers/FriendsProvider';

import { useSignalREvent } from '../../../shared/hooks/useSignalREvent';
import SIGNALR_HUBS from "../../../context/SignalRHubs";

const useChatListeners = () => {
    const {
        addMessage,
        setLastReadMessageIds,
    } = useContext(ChatsContext);
    const { updateFriendFromMessage } = useContext(FriendsContext);

    useSignalREvent(SIGNALR_HUBS.RECEIVE_MESSAGE, (messageDto) => {
        // add message
        addMessage(messageDto);

        // update friend tiles
        updateFriendFromMessage(messageDto);
    });

    useSignalREvent(SIGNALR_HUBS.READ_MESSAGE_GET, (lastReadMessageDto) => {
        setLastReadMessageIds(
            lastReadMessageDto.conversationId,
            lastReadMessageDto.messageId
        );
    });

    // useSignalREvent(SIGNALR_HUBS.FRIEND_CONNECT, (friendId) => {});
};

export default useChatListeners;