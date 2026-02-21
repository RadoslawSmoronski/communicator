import React, { useState, useContext } from 'react';

import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { faMagnifyingGlass } from "@fortawesome/free-solid-svg-icons";


const SearchBar = ({ displayYourChats, setDisplayStatus, searchQuery, setSearchQuery }) => {

    // Search bar block
    // Actions when you type on search bar 
    const handleChangeTxt = async (event) => {
        setSearchQuery(event.target.value);
    }


    // Search bar
    // Handles switching search bar action
    const handleSwitchBtn = async (event) => {
        const { name } = event.target;
        const flag = name === "yourChatsBtn";

        setDisplayStatus(flag);
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