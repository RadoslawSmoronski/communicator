import React, { useState, useContext, useEffect } from 'react'
import FriendList from './friends/FriendList';
import PeopleList from './people/PeopleList';
import SearchBar from './SearchBar';

const PeopleAndFriends = () => {
    const [displayYourChats, setDisplayYourChats] = useState(true);
    const [searchQuery, setSearchQuery] = useState("");

    return (
        <>
            <SearchBar
                displayYourChats={displayYourChats}
                setDisplayStatus={setDisplayYourChats}
                searchQuery={searchQuery}
                setSearchQuery={setSearchQuery}
            />
            <div id='friendsList'>
                {displayYourChats ?
                    <FriendList searchQuery={searchQuery} /> :
                    <PeopleList searchQuery={searchQuery} />
                }
            </div>
        </>

    )
}

export default PeopleAndFriends