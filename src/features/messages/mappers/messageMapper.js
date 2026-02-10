import { createMessageModel } from "../models/messageModels";

export const mapFriendsToInitialMessages = (friendList, currentUserId) => {
    const messagesMap = {};

    friendList.forEach(friend => {
        // new key (friend) for message hashMap
        if (!(friend.conversationId in messagesMap)) {
            // there is last message
            if (friend.lastMessageId != null) {
                const senderId = friend.isFriendSenderMessage ? friend.friendId : currentUserId;

                messagesMap[friend.conversationId] = [
                    createMessageModel({
                        messageId: friend.lastMessageId,
                        conversationId: friend.conversationId,
                        senderId: senderId,
                        content: friend.lastMessageContent,
                        timestamp: friend.lastMessageTimestamp,
                        isRead: false
                    })
                ];
            } else {
                // no messages
                messagesMap[friend.conversationId] = [];
            }
        }
    });

    return messagesMap;
};