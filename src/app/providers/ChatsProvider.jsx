import React, { createContext, useState, useCallback } from "react";
import { useContext } from "react";
import { mapFriendsToInitialMessages, mapDtoToMessage } from "../../features/messages/mappers/messageMapper";

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

    // Add single message
    const addMessage = useCallback((messageDto) => {
        const newMessage = mapDtoToMessage(messageDto);
        const { conversationId } = newMessage;

        setMessages(prev => ({
            ...prev,
            [conversationId]: [
                newMessage,
                ...(prev[conversationId] || [])
            ]
        }));
    }, []);

    // Add list of history messages
    const addHistoryListOfMessages = useCallback((conversationId, messagesListDto) => {
        const historyMessages = messagesListDto.map(mapDtoToMessage);

        setMessages(prev => {
            const currentChatMessages = prev[conversationId] || [];

            return {
                ...prev,
                [conversationId]: [...currentChatMessages, ...historyMessages]
            };
        });
    }, []);

    const removeListOfMessages = () => {

    }

    const setActiveChat = () => {

    }


    return (
        <ChatsContext.Provider value={{
            messages,
            setUp,
            addMessage,
            addHistoryListOfMessages
        }}>
            {children}
        </ChatsContext.Provider>
    )
}

export default ChatsProvider;