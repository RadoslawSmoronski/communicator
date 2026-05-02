export const mapFriendToActiveFriend = (friend) => ({
    friendshipId: friend.friendshipId,
    username: friend.friendUsername,
    avatarUrl: friend.friendAvatarUrl,
    isOnline: friend.isFriendOnline
});

// from last opened chat cookie to active friend
export const mapCookieToActiveFriend = (cookieData) => ({
    friendshipId: cookieData.friendshipId,
    username: cookieData.friendName,
    avatarUrl: cookieData.friendAvatarUrl,
    isOnline: false,
});