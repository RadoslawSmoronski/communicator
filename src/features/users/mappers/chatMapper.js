// mapper single friend from API + flag 'newMessNotify'
export const mapChatDtoToFriend = (dto) => ({
    friendId: dto.friendId,
    friendUsername: dto.friendUsername,
    conversationId: dto.conversationId,
    isFriendOnline: dto.isFriendOnline,
    friendAvatarUrl: dto.friendAvatarUrl,
    friendshipId: dto.friendshipId,
    lastMessageId: dto.lastMessageId,
    lastMessageContent: dto.lastMessageContent,
    isFriendSenderMessage: dto.isFriendSenderMessage,
    lastMessageTimestamp: dto.lastMessageTimestamp,
    newMessNotify: false // client flag
});

// list mapper
export const mapFriendList = (dtoList) =>
    dtoList.map(mapChatDtoToFriend);

// last opened chat
export const mapFriendToLastOpenedChat = (friend) => ({
    conversationId: friend.conversationId,
    friendId: friend.friendId,
    friendName: friend.friendUsername,
    friendAvatarUrl: friend.friendAvatarUrl,
    friendshipId: friend.friendshipId
});