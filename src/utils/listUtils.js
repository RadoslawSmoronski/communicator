const listUtils = {
    // Friend list
    // Returns filtered friends by searchValue from Search bar
    returnFilteredFriends: (friendList, searchValue) => {
        if (friendList) {
            return friendList.filter(user =>
                user.friendUserName.toLowerCase().includes(searchValue.toLowerCase())
            );
        }
        return [];
    },

    // Friend list
    // Returns sorted friend list by last message send/received date
    returnSortedByLastMessDateFriendsList: (friendList) => {
        const sortedList = [...friendList].sort(
            (a, b) => new Date(b.lastMessageTimestamp) - new Date(a.lastMessageTimestamp)
        );
        return sortedList;
    }
}

export default listUtils;

