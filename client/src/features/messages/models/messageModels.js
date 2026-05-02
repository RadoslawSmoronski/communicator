// single message
export const createMessageModel = ({
    messageId,
    conversationId,
    senderId,
    content,
    timestamp,
    isRead = false
}) => ({
    messageId,
    conversationId,
    senderId,
    content,
    timestamp,
    isRead
});