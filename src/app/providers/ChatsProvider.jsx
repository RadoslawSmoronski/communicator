import React, { createContext, useState, useCallback } from "react";
import { useContext } from "react";
import { mapFriendsToInitialMessages } from "../../features/messages/mappers/messageMapper";

export const ChatsContext = createContext();

const ChatsProvider = ({ children }) => {
    // hashlist
    const [messages, setMessages] = useState({});
    const [noNewMessagesFlag, setNoNewMessagesFlag] = useState({});
    const [lastReadMessageIds, setLastReadMessageIds] = useState({});
    // object
    const [lastOpenedChat, setLastOpenedChat] = useState({});
    // ids (string)
    const [selectedId, setSelectedId] = useState("");
    const [activeReciepientId, setActiveReciepientId] = useState("");

    // Inits hash map messages
    // By result of api GET_CHATS
    const setUp = useCallback((friendList, currentUserId) => {
        const initialMessages = mapFriendsToInitialMessages(friendList, currentUserId);
        setMessages(initialMessages);

        console.log("Messages:");
        console.log(messages);
    }, []);

    const addMessage = () => {

    }

    const addListOfMessages = () => {

    }

    const removeListOfMessages = () => {

    }

    const setActiveChat = () => {

    }


    return (
        <ChatsContext.Provider value={{
            messages,
            setUp,
            addMessage
        }}>
            {children}
        </ChatsContext.Provider>
    )
}

export default ChatsProvider;