import React, { useState, useRef, useContext, useEffect } from 'react';
import { Link } from 'react-router-dom';
import { AuthContext } from '../../app/providers/AuthProvider';
import axios from '../../api/axios';
import * as signalR from "@microsoft/signalr";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { faCircleInfo, faMagnifyingGlass, faMessage, faPaperPlane, faPhone, faBell } from "@fortawesome/free-solid-svg-icons";

import useSearchPeople from '../../features/users/hooks/useSearchPeople';

import APIs from '../../api/ApiURL';
import SIGNALR_HUBS from '../../context/SignalRHubs';
import eventBus from '../../shared/utils/eventBus';
import listUtils from '../../shared/utils/listUtils';
import eventUtils from '../../shared/utils/eventUtils';
import cookieUtils from '../../shared/utils/cookieUtils';
import Avatar from '../../shared/components/Avatar';

import FriendTile from '../../features/users/components/friends/FriendTile';
import MessageTile from '../../features/messages/components/messageBox/MessageTile';
import PersonTile from '../../features/users/components/people/PersonTile'
import FriendDetailsPanel from '../../features/messages/components/messageBox/FriendDetailsPanel';
import ConfirmationBox from '../../components/form/ConfirmationBox';

import { UserContext } from '../../app/providers/UserProvider';

const MessagePage = () => {
    const { user: userData } = useContext(UserContext);
    const { accessToken, refreshAccessToken, setAuth } = useContext(AuthContext);
    const scrollMessageBoxRef = useRef(null);
    const abortControllerRef = useRef(null);

    const [searchBar, setSearchBar] = useState('');

    const [user, setUser] = useState({
        list: [],
        findStatus: 'not typed'
    });
    // const { peopleResult, searchPeople } = useSearchPeople(userId);

    // zamiast useState:
    // const user = peopleResult;  


    const [friend, setFriend] = useState({
        list: [],
        list_filtered: [],
        findStatus: 'not found',
        activeFriendName: '',
        activeFriendAvatarUrl: null,
        activeFriendOnlineStatus: false,
        activeFriendshipId: null
    });

    const [chat, setChat] = useState({
        selectedId: '',
        activeReciepientId: '',
        messages: {},
        noNewMessagesFlag: {},
        lastReadMessageIds: {},
        lastOpenedChat: null
    });

    // chatRef - solve the problem of old data "chat"
    const chatRef = useRef(chat);
    useEffect(() => {
        chatRef.current = chat;
    }, [chat]);

    const [messageInput, setMessageInput] = useState('');

    const [display, setDisplay] = useState({
        yourChatIsActive: true,
        friendDetailsPanel: false,
        confirmationBox: false,
        confirmationBoxText: "Are you sure?",
        confirmationBoxFunc: null
    });

    const [signalRConnection, setSignalRConnection] = useState(null);
    const signalRConnectionRef = useRef(null);

    // Set UI visibility by name
    const toggleUI = (uiName, isVisible) => {
        setDisplay(prev => ({
            ...prev,
            [uiName]: isVisible
        }));

    }

    // Search bar block
    // Actions when you type on search bar 
    const handleChangeTxt = async (event) => {
        const { name, value } = event.target;

        if (name === 'searchBar') {
            setSearchBar(value)

            if (!display.yourChatIsActive) {
                // Find friends
                // setUser(prev => ({
                //     ...prev,
                //     findStatus: 'searching...'
                // }));
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

            // setUser(prev => ({
            //     ...prev,
            //     findStatus: 'not typed',
            // }));

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
            const data = await axios.get(APIs.FIND_PEOPLE_TO_INVITE(searchText, userData.userID), {
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
            const data = await axios.get(APIs.GET_CHATS(userId), {
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

    // Reads message and send notifiation to friend
    const readMessage = async () => {
        console.log("readMessage_POST was invoked");

        const connection = await waitForConnection();

        let recipientId = chatRef.current.activeReciepientId;
        let conversationId = chatRef.current.selectedId;

        console.log("recipientId: " + recipientId + ", conversationId: " + conversationId);

        try {
            await connection.invoke(
                SIGNALR_HUBS.READ_MESSAGE_POST,
                recipientId,
                conversationId
            );
        } catch (err) {
            console.error("Error reading message: ", err);
        }

    }

    // Friend list
    // Handles clicking on chat
    // adds param (lastOpened) to userInfo localStorage
    const handleClickingOnChat = async (conversationId, friendId, friendName, friendAvatarUrl, friendshipId) => {
        // cookie override
        let lastOpenedChatsSet = cookieUtils.get('lastOpenedChatSet') || {};

        const lastOpenedChatObj = {
            conversationId,
            friendId,
            friendName,
            friendAvatarUrl,
            friendshipId
        };

        lastOpenedChatsSet[userId] = lastOpenedChatObj;

        cookieUtils.set('lastOpenedChatSet', lastOpenedChatsSet);

        selectChat(conversationId, friendId, friendName, friendAvatarUrl, friendshipId);
    }

    // Friend list
    // Handles selecting chat
    // newMessNotify - is for turning off new message notification from friend
    const selectChat = async (conversationId, friendId, friendName, friendAvatarUrl, friendshipId) => {
        const isInTheSameChat = chat.selectedId == conversationId;
        if (!isInTheSameChat) {
            scrollMessageBoxRef.current.scrollTop = 0;
            toggleUI("friendDetailsPanel", false);
        }


        const updatedFriends = friend.list.map(friend =>
            friend.conversationId === conversationId
                ? { ...friend, newMessNotify: false } // modify the friend
                : friend
        );

        setFriend(prev => ({
            ...prev,
            activeFriendName: friendName,
            activeFriendAvatarUrl: friendAvatarUrl,
            activeFriendshipId: friendshipId,
            list: updatedFriends,
            list_filtered: listUtils.returnFilteredFriends(updatedFriends, searchBar),
        }));

        setChat(prev => {
            const updatedChat = {
                ...prev,
                selectedId: conversationId,
                activeReciepientId: friendId,
            };
            chatRef.current = updatedChat;
            return updatedChat;
        });

        // fetch new messages if there are no fetched messages
        if (chatRef.current.messages[conversationId].length === 1) {
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

        } else if (chatRef.current.messages[conversationId].length > 1 && !isInTheSameChat) {
            // messages already fetched, invoke read message
            await readMessage();
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
        let API_URL;

        if (chatLenght > 0) {
            lastMessageId = chatRef.current.messages[conversationId][chatLenght - 1].messageId;
            API_URL = APIs.GET_MESSAGES(conversationId, lastMessageId);
        } else {
            API_URL = APIs.GET_MESSAGES_NULL_FROM_MESSAGE_ID(conversationId);
        }

        try {
            const data = await axios.get(
                API_URL,
                {
                    withCredentials: true,
                    headers: { Authorization: `Bearer ${accessToken}` }
                }
            );

            if (data.status === 200) {
                const newMessages = data.data.messages;
                const lastFriendReadMessageId = data.data.lastFriendReadMessageId;
                console.log("GET_MESSAGES:");
                console.log(newMessages);

                let newMessagesFlag = newMessages.length === 0;

                // add new messeges for [conversationId]
                // noNewMessagesFlag for stopping fetching new messages
                setChat(prev => {
                    const oldMessages = prev.messages[conversationId] || [];
                    const newMessagesArr = [...oldMessages, ...newMessages];

                    chatRef.current.messages[conversationId] = newMessagesArr;

                    return {
                        ...prev,
                        messages: {
                            ...prev.messages,
                            [conversationId]: newMessagesArr,
                        },
                        noNewMessagesFlag: {
                            ...prev.noNewMessagesFlag,
                            [conversationId]: newMessagesFlag
                        },
                        lastReadMessageIds: {
                            ...prev.lastReadMessageIds,
                            [conversationId]: lastFriendReadMessageId
                        }
                    }
                });
            }
        } catch (err) {
            if (err.response?.status === 401) {
                await refreshAccessToken();
                await getMessagesForFriend(conversationId);
            } else if (err.response?.status === 404) {

            } else {
                console.error(err);
            }

            setChat(prev => {
                return {
                    ...prev,
                    noNewMessagesFlag: {
                        ...prev.noNewMessagesFlag,
                        [conversationId]: true
                    }
                }
            });
        }
    };

    // Message box and friend list
    // It handles new message, creates notification, add to the list
    const handleNewMessageFromFriend = async (messageDto) => {
        let isInTheSameChat = messageDto.conversationId === chatRef.current.selectedId;
        let isFromFriend = messageDto.senderId != userId;
        // invoke read message if user is in the same chat
        // and message is from friend not from user
        if (isInTheSameChat && isFromFriend) {
            console.log("Message is from the active chat");
            readMessage();
        }
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
                        newMessNotify: !isInTheSameChat,
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

    // handles reading message by friend 
    // and saves lastReadMessageId
    const handleReadMessageByFriend = async (lastReadMessageDto) => {
        console.log("Friend read message:");
        console.log(lastReadMessageDto)

        let convId = lastReadMessageDto.conversationId;
        let messId = lastReadMessageDto.messageId;

        setChat(prev => ({
            ...prev,
            lastReadMessageIds: {
                ...prev.lastReadMessageIds,
                [convId]: messId
            }
        }));
    }

    // handles change of friend online status
    const handleFriendChangeOnlineStatus = async (friendId, isOnline) => {
        if (isOnline) {
            console.log(friendId + " is Online");
        } else {
            console.log(friendId + " is Offline");
        }

        // update online status to friend list
        setFriend(prev => {
            const updatedFriends = prev.list.map(f =>
                f.friendId === friendId ? { ...f, isFriendOnline: isOnline } : f
            );

            let updatedActiveStatus = prev.activeFriendOnlineStatus;

            // update online status at friend bar
            if (friendId === chatRef.current.activeReciepientId) {
                const activeFriend = updatedFriends.find(f => f.friendId === chatRef.current.activeReciepientId);
                if (activeFriend) {
                    updatedActiveStatus = activeFriend.isFriendOnline;
                }
            }


            return {
                ...prev,
                list: updatedFriends,
                list_filtered: listUtils.returnFilteredFriends(updatedFriends, searchBar),
                activeFriendOnlineStatus: updatedActiveStatus
            };
        });
    }

    // SignalR connection
    useEffect(() => {
        if (accessToken != '') {
            getFriends();

            // get your last chat info
            const userChat = cookieUtils.get('lastOpenedChatSet');
            if (userChat) { // if cookie exists
                if (userChat[userId]) { // if there is a record from logged user
                    setChat(prev => ({
                        ...prev,
                        lastOpenedChat: userChat[userId]
                    }))

                }
            }

            const connection = new signalR.HubConnectionBuilder()
                .withUrl(`http://localhost:5205${SIGNALR_HUBS.CHATHUB}`, {
                    accessTokenFactory: () => accessToken
                })
                .withAutomaticReconnect()
                .build();

            connection.start()
                .then(() => {
                    console.log("Connected to SignalR");

                    setSignalRConnection(connection);
                    signalRConnectionRef.current = connection;
                })
                .catch(err => console.error("Connection failed: ", err));

            connection.on(SIGNALR_HUBS.RECEIVE_MESSAGE,
                (messageDto) => handleNewMessageFromFriend(messageDto)
            );
            connection.on(SIGNALR_HUBS.READ_MESSAGE_GET,
                (lastReadMessageDto) => handleReadMessageByFriend(lastReadMessageDto)
            )
            connection.on(SIGNALR_HUBS.FRIEND_CONNECT,
                (friendId) => handleFriendChangeOnlineStatus(friendId, true)
            )
            connection.on(SIGNALR_HUBS.FRIEND_DISCONNECT,
                (friendId) => handleFriendChangeOnlineStatus(friendId, false)
            )

            // listen for 'refreshFriends'
            eventBus.on('refreshFriends', getFriends);

            return () => {
                connection.stop();
                eventBus.off('refreshFriends', getFriends);
            };
        }
    }, []);

    // wait for signalR connection
    const waitForConnection = async () => {
        while (
            !signalRConnectionRef.current ||
            signalRConnectionRef.current.state !== signalR.HubConnectionState.Connected
        ) {
            await new Promise(resolve => setTimeout(resolve, 100));
        }
        return signalRConnectionRef.current;
    };


    // load last openned chat
    useEffect(() => {
        if (chat.lastOpenedChat && friend.list.length > 0) {
            const { conversationId, friendId, friendName, friendAvatarUrl, friendshipId } = chat.lastOpenedChat;
            let lastChatObj = friend.list.filter(f => f.friendId == chat.lastOpenedChat.friendId);
            if (lastChatObj[0]?.friendshipId) { // load if last chat opened is still with friend
                selectChat(conversationId, friendId, friendName, friendAvatarUrl, friendshipId);
            }


            setChat(prev => ({
                ...prev,
                lastOpenedChat: null
            }));
        }
    }, [chat.lastOpenedChat, friend.list]);

    // auto update online status at friend bar
    useEffect(() => {
        setFriend(prev => {
            let updatedActiveStatus = prev.activeFriendOnlineStatus;

            // update online status at friend bar
            const activeFriend = friend.list.find(f => f.friendId === chatRef.current.activeReciepientId);
            if (activeFriend != null) {
                updatedActiveStatus = activeFriend.isFriendOnline;
            }

            return {
                ...prev,
                activeFriendOnlineStatus: updatedActiveStatus
            };
        });
    }, [friend.list, chat.activeReciepientId])

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
                            friend.list_filtered
                                .filter(f => f.friendshipId !== null)
                                .map(f => (
                                    <FriendTile
                                        key={f.friendId}
                                        username={f.friendUsername}
                                        onClick={() => handleClickingOnChat(f.conversationId, f.friendId, f.friendUsername, f.friendAvatarUrl, f.friendshipId)}
                                        author={f.isFriendSenderMessage ? '' : 'You: '}
                                        mess={f.lastMessageContent}
                                        messTimestamp={f.lastMessageTimestamp}
                                        selected={chat.selectedId === f.conversationId}
                                        newMessageNotify={f.newMessNotify}
                                        avatarUrl={f.friendAvatarUrl}
                                        isOnline={f.isFriendOnline}
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
                                username={u.username}
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
                            <Avatar url={friend.activeFriendAvatarUrl} >
                                <div className={friend.activeFriendOnlineStatus ? "onlineBadge online" : "onlineBadge offline"} />
                            </Avatar>
                        </div>
                        <div className='friendBarUserName'>{friend.activeFriendName}</div>
                        <div className='friendBarRightBox'>
                            <FontAwesomeIcon icon={faPhone} className='friendBarIcon' />
                            <FontAwesomeIcon icon={faCircleInfo} className='friendBarIcon' onClick={() => toggleUI("friendDetailsPanel", !display.friendDetailsPanel)} />
                        </div>
                    </>
                }
            </div>

            {/* MESSAGE BOX */}
            <div
                id='messageBox'
            >
                <div className='messageBoxContent'
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
                                isLastReadByFriend={chat.lastReadMessageIds[chat.selectedId] === message.messageId}
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
                {display.friendDetailsPanel &&
                    <FriendDetailsPanel
                        friendName={friend.activeFriendName}
                        friendshipId={friend.activeFriendshipId}
                        setDisplay={setDisplay}
                        showConfirmationBox={() => toggleUI("confirmationBox", true)}
                        closeConfirmationBox={() => toggleUI("confirmationBox", false)}
                        setFriend={setFriend}
                        setChat={setChat}
                        friendState={friend}
                    />
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

            {display.confirmationBox &&
                <ConfirmationBox
                    confirmFunc={display.confirmationBoxFunc}
                    closePanel={() => toggleUI("confirmationBox", false)}
                    text={display.confirmationBoxText}
                />
            }
        </>
    );

};

export default MessagePage;