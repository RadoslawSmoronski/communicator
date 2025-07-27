
const APIs = {
    SERVER_URL: "http://localhost:5205",

    LOGIN : "/api/user/login",
    REGISTER : "/api/user/register",
    FIND_PEOPLE : "/api/users/get-users-by-text",
    FIND_FRIENDS : "/api/friends/get-friends",
    FIND_PEOPLE_TO_INVITE: "/api/friends/get-users-to-invite-by-text",
    REFRESH_TOKEN: "/api/user/refresh-access-token",

    SEND_INVITE: "/api/friends/send-invite",
    GET_INVITATIONS: "/api/friends/get-invitations",
    DECELINE_INVITE: "/api/friends/deceline-invite",
    ACCEPT_INVITE: "/api/friends/accept-invite",

    GET_CHATS: "/api/chat/get-chats",
    GET_FRIENDS: "/api/friends/get-friends",
    GET_MESSAGES: "/api/chat/get-paged-messages",

    AVATAR: "api/user/avatar"
}

export default APIs;