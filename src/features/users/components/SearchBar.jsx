import React, { useState, useContext } from 'react';

import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { faMagnifyingGlass } from "@fortawesome/free-solid-svg-icons";

import listUtils from '../../../shared/utils/listUtils';
import { FriendsContext } from '../../../app/providers/FriendsProvider';


const SearchBar = ({ displayYourChats, setDisplayStatus, searchQuery, setSearchQuery }) => {
    const {
        friendList,
        friendListFiltered,
        setFriendList,
        setFriendListFiltered
    } = useContext(FriendsContext);

    // Search bar block
    // Actions when you type on search bar 
    const handleChangeTxt = async (event) => {
        const { value } = event.target;

        setSearchQuery(value);

        if (displayYourChats) {
            const filteredFriendsList = listUtils.returnFilteredFriends(
                friendList, value
            );
            setFriendListFiltered(filteredFriendsList);
        }
    }


    // Search bar
    // Handles switching search bar action
    const handleSwitchBtn = async (event) => {
        const { name } = event.target;
        const flag = name === "yourChatsBtn";

        setDisplayStatus(flag);

        setFriendListFiltered(friendList); // reset filtering of friends
        setSearchQuery("");
    }

    return (
        <div id='searchBar'>
            <div className="inputWrapper">
                <FontAwesomeIcon icon={faMagnifyingGlass} className='inputIcon' />
                <input
                    className='textInput2'
                    placeholder={
                        displayYourChats ? ' Filter friends ...' : ' Search for people ...'
                    }
                    value={searchQuery}
                    onChange={handleChangeTxt}
                    name='searchBar'
                    type="text"
                    autoComplete='off'
                />
            </div>
            <div id='buttonsSearchBar'>
                <button
                    name='yourChatsBtn'
                    className={displayYourChats ? 'btn2 btn2Active' : 'btn2'}
                    onClick={handleSwitchBtn}
                >
                    Your chats
                </button>
                <button
                    name='findFriendsBtn'
                    className={!displayYourChats ? 'btn2 btn2Active' : 'btn2'}
                    onClick={handleSwitchBtn}
                >
                    Find friends
                </button>
            </div>
        </div>
    )
}

export default SearchBar