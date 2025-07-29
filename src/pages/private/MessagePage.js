import React, { useState, useRef, useContext, useEffect } from 'react';
import { Link } from 'react-router-dom';
import { AuthContext } from '../../context/AuthProvider';
import axios from '../../api/axios';
import * as signalR from "@microsoft/signalr";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { faCircleInfo, faMagnifyingGlass, faMessage, faPaperPlane, faPhone, faBell } from "@fortawesome/free-solid-svg-icons";

import APIs from '../../api/ApiURL';
import SIGNALR_HUBS from '../../context/SignalRHubs';
import eventBus from '../../utils/eventBus';
import listUtils from '../../utils/listUtils';
import eventUtils from '../../utils/eventUtils';
import cookieUtils from '../../utils/cookieUtils';
import Avatar from '../../components/Avatar';

import FriendTile from '../../components/tiles/FriendTile';
import MessageTile from '../../components/tiles/MessageTile';
import PersonTile from '../../components/tiles/PersonTile';

const MessagePage = () => {
    const { userId, accessToken, refreshAccessToken, setAuth } = useContext(AuthContext);
    const scrollMessageBoxRef = useRef(null);
    const abortControllerRef = useRef(null);

    const [searchBar, setSearchBar] = useState('');

    const [user, setUser] = useState({
        list: [],
        findStatus: 'not typed'
    });

    const [friend, setFriend] = useState({
        list: [],
        list_filtered: [],
        findStatus: 'not found',
        activeFriendName: '',
        activeFriendAvatarUrl: null
    });

    const [chat, setChat] = useState({
        selectedId: '',
        activeReciepientId: '',
        messages: {},
        noNewMessagesFlag: {},
        lastOpenedChat: null
    });
    // chatRef - solve the problem of old data "chat"
    const chatRef = useRef(chat);
    useEffect(() => {
        chatRef.current = chat;
    }, [chat]);

    const [messageInput, setMessageInput] = useState('');

    const [display, setDisplay] = useState({
        yourChatIsActive: true
    });

    const [signalRConnection, setSignalRConnection] = useState(null);

    // Search bar block
    // Actions when you type on search bar 
    const handleChangeTxt = async (event) => {
        const { name, value } = event.target;

        if (name === 'searchBar') {
            setSearchBar(value)

            if (!display.yourChatIsActive) {
                // Find friends
                setUser(prev => ({
                    ...prev,
                    findUsersStatus: 'searching...'
                }));
                searchPeople(value);
            } else {
                // Your chats
                const filtered = listUtils.returnFilteredFriends(friend.list, value);
                setFriend(prev => ({ ...prev, list_filtered: filtered }));
            }
        }

        if (name === 'messageInput') {
            setMessageInput(value);
        }
    }

    // Search bar
    // Handles switching search bar action
    const handleSwitchBtn = async (event) => {
        const { name } = event.target;
        const flag = name === "yourChatsBtn";

        if (display.yourChatIsActive !== flag) {
            // clearing the states
            setDisplay(prev => ({
                ...prev,
                yourChatIsActive: flag,
            }));

            setUser(prev => ({
                ...prev,
                findStatus: 'not typed',
            }));

            setFriend(prev => ({
                ...prev,
                list_filtered: friend.list
            }));

            setSearchBar('');
        }
    };

    // People list
    // Searches people to invite
    const searchPeople = async (searchText) => {
        // AbortController for canceling old requests
        if (abortControllerRef.current) {
            abortControllerRef.current.abort();
        }

        const abortCtr = new AbortController();
        abortControllerRef.current = abortCtr;

        if (searchText.trim() === '') {
            setUser(prev => ({
                ...prev,
                findStatus: 'not typed',
                list: [],
            }));
            return;
        }


        try {
            const data = await axios.get(`${APIs.FIND_PEOPLE_TO_INVITE}/${searchText}`, {
                withCredentials: true,
                headers: {
                    Authorization: `Bearer ${accessToken}`
                },
                signal: abortCtr.signal
            });

            if (data.status === 200) {

                if (data.data?.length) {
                    setUser(prev => ({
                        ...prev,
                        findStatus: 'found',
                        list: data.data,
                    }));
                } else {
                    setUser(prev => ({ ...prev, list: [], findStatus: 'not found' }));
                }


            }
        } catch (err) {
            if (err.name === 'CanceledError') { // cancel the request
                return;
            }

            if (err.response?.status === 401) {
                await refreshAccessToken();
                await searchPeople(searchText);
            } else if (err.response?.status === 400) {
                setUser(prev => ({ ...prev, findStatus: 'not typed' }));
            } else if (err.response?.status === 404) {
                // setUser(prev => ({ ...prev, findStatus: 'not found' }));
            } else {
                // console.error(err);
            }
        }
    };

    // Friend list
    // Fetches the user friend list
    const getFriends = async () => {
        try {
            const data = await axios.get(APIs.GET_CHATS, {
                withCredentials: true,
                headers: { Authorization: `Bearer ${accessToken}` },
            });

            if (data.status === 200) {
                const result = data.data;
                console.log("GetFriends");
                console.log(result);

                // add new value (newMessNotify - notification) to listOfFriends
                const friendsWithNotify = result.map(friend => ({
                    ...friend,
                    newMessNotify: false,
                }));

                const sortedFriendList = listUtils.returnSortedByLastMessDateFriendsList(friendsWithNotify);

                setFriend(prev => ({
                    ...prev,
                    list: sortedFriendList,
                    list_filtered: sortedFriendList,
                }));

                // add chats to allMessages, pageNumbersForMessages
                setChat(prev => {
                    const messages = { ...prev.messages };

                    sortedFriendList.forEach(friend => {
                        if (!(friend.conversationId in messages)) {
                            // last message as a last message in list
                            let senderId = friend.isFriendSenderMessage ? friend.friendId : userId;
                            if (friend.lastMessageId != null) {
                                messages[friend.conversationId] = [{
                                    messageId: friend.lastMessageId,
                                    conversationId: friend.conversationId,
                                    senderId: senderId,
                                    content: friend.lastMessageContent,
                                    timestamp: friend.lastMessageTimestamp,
                                    isRead: false
                                }];
                            } else {
                                messages[friend.conversationId] = [];
                            }


                        }
                    });

                    return {
                        ...prev,
                        messages
                    };
                });
            }
        } catch (err) {
            if (err.response?.status === 401) {
                await refreshAccessToken();
                await getFriends();
            } else if (err.response?.status === 400) {
                setFriend(prev => ({
                    ...prev,
                    findStatus: 'bad ID',
                }));
            } else if (err.response?.status === 404) {
                setFriend(prev => ({
                    ...prev,
                    findStatus: 'not found',
                }));
            } else {
                console.error(err);
            }
        }
    };

    // Send message box
    // It sends message to friend if the chat is selected
    const sendMessageToFriend = async () => {
        if (scrollMessageBoxRef.current) {
            scrollMessageBoxRef.current.scrollTop = 0;
        }

        if (
            signalRConnection &&
            signalRConnection.state === signalR.HubConnectionState.Connected &&
            messageInput.trim() !== ""
        ) {
            try {
                await signalRConnection.invoke(
                    SIGNALR_HUBS.SEND_MESSAGE,
                    chat.activeReciepientId,
                    chat.selectedId,
                    messageInput
                );
                setMessageInput("");
            } catch (err) {
                console.error("Error sending message: ", err);
            }
        } else {
            console.error("Connection not established or message is empty.");
        }
    };

    // Friend list
    // Handles clicking on chat
    // adds param (lastOpened) to userInfo localStorage
    const handleClickingOnChat = async (conversationId, friendId, friendName, friendAvatarUrl) => {
        // cookie override
        let lastOpenedChatsSet = cookieUtils.get('lastOpenedChatSet') || {};

        const lastOpenedChatObj = {
            conversationId,
            friendId,
            friendName,
            friendAvatarUrl
        };

        lastOpenedChatsSet[userId] = lastOpenedChatObj;

        cookieUtils.set('lastOpenedChatSet', lastOpenedChatsSet);

        selectChat(conversationId, friendId, friendName, friendAvatarUrl);
    }

    // Friend list
    // Handles selecting chat
    // newMessNotify - is for turning off new message notification from friend
    const selectChat = async (conversationId, friendId, friendName, friendAvatarUrl) => {
        scrollMessageBoxRef.current.scrollTop = 0;

        const updatedFriends = friend.list.map(friend =>
            friend.conversationId === conversationId
                ? { ...friend, newMessNotify: false } // modify the friend
                : friend
        );

        setFriend(prev => ({
            ...prev,
            activeFriendName: friendName,
            activeFriendAvatarUrl: friendAvatarUrl,
            list: updatedFriends,
            list_filtered: listUtils.returnFilteredFriends(updatedFriends, searchBar),
        }));

        setChat(prev => ({
            ...prev,
            selectedId: conversationId,
            activeReciepientId: friendId,
        }));

        // fetch new messages if there are no fetched messages
        if (chat.messages[conversationId].length === 1) {
            await eventUtils.waitForDOMUpdate();

            const box = scrollMessageBoxRef.current;
            let isScrollable = box.clientHeight < box.scrollHeight;

            // fetch as many messages as long there will be a scroll bar
            while (!isScrollable && !chatRef.current.noNewMessagesFlag[conversationId]) {
                await getMessagesForFriend(conversationId);
                await eventUtils.waitForDOMUpdate();

                isScrollable = box.clientHeight < box.scrollHeight;
            }

            // console.log(!isScrollable, !chatRef.current.noNewMessagesFlag[conversationId])

        }
    };

    // Message box
    // It makes sure that new messages (fetch via pages)
    // are fetched only once if scroll is at top
    const handleScrollMessageBox = async () => {
        const box = scrollMessageBoxRef.current;
        const isAtTop = box.clientHeight - box.scrollTop >= box.scrollHeight;

        if (isAtTop && !chatRef?.current.noNewMessagesFlag[chatRef?.current.selectedId]) {
            await getMessagesForFriend(chat.selectedId);

            await eventUtils.waitForDOMUpdate();
        }
    };

    // Message box
    // It fetches messages by current page
    const getMessagesForFriend = async (conversationId) => {
        let chatLenght = chatRef.current.messages[conversationId].length;
        let lastMessageId = null;
        if (chatLenght > 0) {
            lastMessageId = chatRef.current.messages[conversationId][chatLenght - 1].messageId;
        }

        try {
            const data = await axios.get(
                `${APIs.GET_MESSAGES}/?ConversationId=${conversationId}&fromMessageId=${lastMessageId}`,
                {
                    withCredentials: true,
                    headers: { Authorization: `Bearer ${accessToken}` },
                }
            );

            if (data.status === 200) {
                const newMessages = data.data;

                let newMessagesFlag = newMessages.length === 0;

                // add new messeges for [conversationId]
                // noNewMessagesFlag for stopping fetching new messages
                setChat(prev => ({
                    ...prev,
                    messages: {
                        ...prev.messages,
                        [conversationId]: [...(prev.messages[conversationId] || []), ...newMessages],
                    },
                    noNewMessagesFlag: {
                        ...prev.noNewMessagesFlag,
                        [conversationId]: newMessagesFlag
                    }
                }));
            }
        } catch (err) {
            if (err.response?.status === 401) {
                await refreshAccessToken();
                await getMessagesForFriend(conversationId);
            } else if (err.response?.status === 404) {
                // setChat(prev => ({
                //     ...prev,
                //     messages: {
                //         ...prev.messages,
                //         [conversationId]: [],
                //     },
                // }));
            } else {
                console.error(err);
            }
        }
    };

    // Message box and friend list
    // It handles new message, creates notification, add to the list
    const handleNewMessageFromFriend = async (messageDto) => {
        // add last message to FriendTile
        console.log(messageDto);

        setFriend(prev => {
            const newList = prev.list.map(f => {
                if (f.conversationId === messageDto.conversationId) {
                    return {
                        ...f,
                        isFriendSenderMessage: messageDto.senderId != userId,
                        lastMessageContent: messageDto.content,
                        lastMessageTimestamp: messageDto.timestamp,
                        newMessNotify: messageDto.conversationId != chatRef.current.selectedId,
                    };
                }
                return f;
            });

            const sorted = listUtils.returnSortedByLastMessDateFriendsList(newList);

            return {
                ...prev,
                list: sorted,
                list_filtered: listUtils.returnSortedByLastMessDateFriendsList(listUtils.returnFilteredFriends(sorted, searchBar)),
            };
        });

        // add message to messages list

        setChat(prev => ({
            ...prev,
            messages: {
                ...prev.messages,
                [messageDto.conversationId]: [
                    messageDto,
                    ...(prev.messages[messageDto.conversationId] || []),
                ],
            },
        }));
    };

    // SignalR connection
    useEffect(() => {
        if (accessToken != '') {
            getFriends();

            // get your last chat info
            const userChat = cookieUtils.get('lastOpenedChatSet');
            if (userChat) {
                if (userChat[userId]) {
                    setChat(prev => ({
                        ...prev,
                        lastOpenedChat: userChat[userId]
                    }))
                }
            }
        }

        const connection = new signalR.HubConnectionBuilder()
            .withUrl(`http://localhost:5205${SIGNALR_HUBS.CHATHUB}`, {
                accessTokenFactory: () => accessToken
            })
            .withAutomaticReconnect()
            .build();

        connection.start()
            .then(() => console.log("Connected to SignalR"))
            .catch(err => console.error("Connection failed: ", err));

        connection.on(SIGNALR_HUBS.RECEIVE_MESSAGE,
            (messageDto) => handleNewMessageFromFriend(messageDto)
        );
        setSignalRConnection(connection);

        // listen for 'refreshFriends'
        eventBus.on('refreshFriends', getFriends);

        return () => {
            connection.stop();
            eventBus.off('refreshFriends', getFriends);
        };
    }, []);

    // load last openned chat
    useEffect(() => {
        if (chat.lastOpenedChat && friend.list.length > 0) {
            const { conversationId, friendId, friendName, friendAvatarUrl } = chat.lastOpenedChat;
            selectChat(conversationId, friendId, friendName, friendAvatarUrl);

            setChat(prev => ({
                ...prev,
                lastOpenedChat: null
            }));
        }
    }, [chat.lastOpenedChat, friend.list]);


    return (
        <>
            {/* SEARCH BAR */}
            <div id='searchBar'>
                <div className="inputWrapper">
                    <FontAwesomeIcon icon={faMagnifyingGlass} className='inputIcon' />
                    <input
                        className='textInput2'
                        placeholder=' Search for people'
                        value={searchBar}
                        onChange={handleChangeTxt}
                        name='searchBar'
                        type="text"
                        autoComplete='off'
                    />
                </div>
                <div id='buttonsSearchBar'>
                    <button
                        name='yourChatsBtn'
                        className={display.yourChatIsActive ? 'btn2 btn2Active' : 'btn2'}
                        onClick={handleSwitchBtn}
                    >
                        Your chats
                    </button>
                    <button
                        name='findFriendsBtn'
                        className={!display.yourChatIsActive ? 'btn2 btn2Active' : 'btn2'}
                        onClick={handleSwitchBtn}
                    >
                        Find friends
                    </button>
                </div>
            </div>

            {/* FRIEND OR PEOPLE LIST */}
            <div id='friendsList'>
                {display.yourChatIsActive ? (
                    friend.list.length > 0 ? (
                        friend.list_filtered.length > 0 ? (
                            friend.list_filtered.map(f => (
                                <FriendTile
                                    key={f.friendId}
                                    username={f.friendUserName}
                                    onClick={() => handleClickingOnChat(f.conversationId, f.friendId, f.friendUserName, f.friendAvatarUrl)}
                                    author={f.isFriendSenderMessage ? '' : 'You: '}
                                    mess={f.lastMessageContent}
                                    messTimestamp={f.lastMessageTimestamp}
                                    selected={chat.selectedId === f.conversationId}
                                    newMessageNotify={f.newMessNotify}
                                    avatarUrl={f.friendAvatarUrl}
                                />
                            ))
                        ) : (
                            <div className='infoText'>There are no friends named {searchBar} ...</div>
                        )
                    ) : (
                        <div className='infoText'>You have zero friends</div>
                    )
                ) : (
                    user.findStatus === 'not typed' ? (
                        <div className='infoText'>Please type any character</div>
                    ) : user.findStatus === 'not found' ? (
                        <div className='infoText'>There are no users named {searchBar} ...</div>
                    ) : (
                        user.list.map(u => (
                            <PersonTile
                                key={u.id}
                                username={u.userName}
                                recipientId={u.id}
                                isInvited={u.isInvited}
                                avatarUrl={u.avatarUrl}
                            />
                        ))
                    )
                )}
            </div>

            {/* FRIEND BAR */}
            <div id='friendBar'>
                {friend.activeFriendName &&
                    <>
                        <div className='friendBarIconBox'>
                            <Avatar url={friend.activeFriendAvatarUrl} />
                        </div>
                        <div className='friendBarUserName'>{friend.activeFriendName}</div>
                        <div className='friendBarRightBox'>
                            <FontAwesomeIcon icon={faPhone} className='friendBarIcon' />
                            <FontAwesomeIcon icon={faCircleInfo} className='friendBarIcon' />
                        </div>
                    </>
                }
            </div>

            {/* MESSAGE BOX */}
            <div
                id='messageBox'
                ref={scrollMessageBoxRef}
                onScroll={handleScrollMessageBox}
            >
                {Array.isArray(chat.messages[chat.selectedId]) &&
                    chat.messages[chat.selectedId].map((message, index) => (
                        <MessageTile
                            key={index}
                            mess={message.content}
                            yours={message.senderId === userId}
                            time={message.timestamp}
                        />
                    ))
                }

                {/* info inside chat */}
                {chat.messages[chat.selectedId]?.length > 0 ?
                    chat.noNewMessagesFlag[chat.selectedId] &&
                    ( // there're some messages
                        <span className='textCenter'>--- End of conversation ---</span>
                    ) :
                    chat.selectedId ?
                        ( // there aren't any messages
                            <span className='textCenter'>--- Start a conversation ---</span>
                        ) :
                        ( // chat isn't selected
                            <span className='textCenter'>--- Select chat ---</span>
                        )
                }


            </div>

            {/* SEND MESSAGE BOX */}
            <div id='sendMessageBox'>
                <input
                    className='textInput2 sendMessageInput'
                    placeholder={chat.selectedId === null ? 'Select chat...' : 'Type a message...'}
                    value={messageInput}
                    onChange={handleChangeTxt}
                    name='messageInput'
                    type='text'
                    autoComplete='off'
                    disabled={chat.selectedId === null}
                />
                <FontAwesomeIcon
                    icon={faPaperPlane}
                    className={`friendBarIcon ${chat.selectedId === null ? 'disabledSendButton' : ''}`}
                    onClick={chat.selectedId === null ? null : sendMessageToFriend}
                />
            </div>
        </>
    );

};

export default MessagePage;