
const APIs = {
    LOGIN_URL : "/api/user/login",
    REGISTER_URL : "/api/user/register",
    FIND_PEOPLE_URL : "/api/users/getUsersByText",
    FIND_FRIENDS_URL : "/api/friends/getFriends",
    FIND_PEOPLE_TO_INVITE_URL: "/api/friends/getUsersToInviteByText",
    REFRESH_TOKEN_URL: "/api/user/refreshAccessToken",

    SEND_INVITE_URL: "/api/friends/sendInviteAsync",
    GET_INVITATIONS_URL: "/api/friends/getInvitations",
    DECELINE_INVITE_URL: "/api/friends/decelineInvite",
    ACCEPT_INVITE_URL: "/api/friends/acceptInvite"
}

export default APIs;