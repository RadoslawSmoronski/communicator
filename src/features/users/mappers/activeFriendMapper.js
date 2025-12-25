export const mapFriendToActiveFriend = (friend) => ({
    friendshiId: friend.friendshipId,
    username: friend.friendUsername,
    avatarUrl: friend.friendAvatarUrl,
    isOnline: friend.isFriendOnline
});