import React, { createContext, useState } from "react";
import { useContext } from "react";

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

    const setUp = () => { // na podstawie get chats

    }

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

        }}>
            {children}
        </ChatsContext.Provider>
    )
}

export default ChatsProvider;