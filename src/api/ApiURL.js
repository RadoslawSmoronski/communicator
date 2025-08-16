
const APIs = {
    SERVER_URL: "http://localhost:5205",

    LOGIN : "/api/auth/login",
    REGISTER : "/api/users",
    REFRESH_TOKEN: "/api/auth/refresh-token",

    GET_CHATS: (userId) => `/api/users/${userId}/chats`,
    GET_MESSAGES: (conversationId, fromMessageId) => `api/chats/${conversationId}/messages?fromMessageId=${fromMessageId}`,
    GET_MESSAGES_NULL_FROM_MESSAGE_ID: (conversationId) => `api/chats/${conversationId}/messages`,

    SEND_INVITE: "/api/friend-invitations",
    GET_INVITATIONS: (userId) => `/api/users/${userId}/friend-invitations`,
    DECELINE_INVITE:(friendInvitationId) => `/api/friend-invitations/${friendInvitationId}/decline`,
    ACCEPT_INVITE: (friendInvitationId)=> `/api/friend-invitations/${friendInvitationId}/accept`,
    
    FIND_PEOPLE_TO_INVITE: (text, userId) => `/api/users?search=${text}&invitableFor=${userId}`,
    GET_FRIENDS: (userId) => `/api/users/${userId}/friend`,

    AVATAR: (userId) => `/api/users/${userId}/avatar`,
    CHANGE_USERNAME: (userId) => `/api/users/${userId}/username`,
    CHANGE_PASSWORD: (userId) => `/api/users/${userId}/password`,
}

export default APIs;