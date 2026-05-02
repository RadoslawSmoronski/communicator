// mocks/chatDto.mock.js
export const chatDtoMock = [
    {
        friendId: "1",
        friendUsername: "Janek",
        conversationId: "conv-1",
        isFriendOnline: true,
        friendAvatarUrl: "/avatars/janek.png",
        friendshipId: "fs-1",
        lastMessageId: "m-1",
        lastMessageContent: "Hej, co słychać?",
        isFriendSenderMessage: true,
        lastMessageTimestamp: "2025-03-01T18:20:00Z"
    },
    {
        friendId: "2",
        friendUsername: "Ola",
        conversationId: "conv-2",
        isFriendOnline: false,
        friendAvatarUrl: "/avatars/ola.png",
        friendshipId: "fs-2",
        lastMessageId: null,
        lastMessageContent: null,
        isFriendSenderMessage: false,
        lastMessageTimestamp: null
    }
];