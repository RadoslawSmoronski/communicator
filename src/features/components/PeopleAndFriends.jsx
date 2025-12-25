import React, { useState, useContext, useEffect } from 'react'
import FriendList from './friends/FriendList';
import PeopleList from './people/PeopleList';

const PeopleAndFriends = () => {
    const [display, setDisplay] = useState({
        yourChatIsActive: true,
    });

    return (
        <div id='friendsList'>
            {display.yourChatIsActive ?
                <FriendList /> :
                <PeopleList />
            }
        </div>
    )
}

export default PeopleAndFriends