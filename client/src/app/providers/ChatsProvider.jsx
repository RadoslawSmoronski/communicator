import React, { createContext, useState, useCallback } from "react";
import { mapFriendsToInitialMessages, mapDtoToMessage } from "../../features/messages/mappers/messageMapper";
import { lastOpenedChatService } from "../../features/users/services/lastOpenedChatService";

export const ChatsContext = createContext();

const ChatsProvider = ({ children }) => {
    // hashlist
    const [messages, setMessages] = useState({});
    const [noNewMessagesFlagsMap, setNoNewMessagesFlagsMap] = useState({});
    const [lastReadMessageIdsMap, setLastReadMessageIdsMap] = useState({});
    // ids (string)
    const [selectedId, setSelectedId] = useState("");
    const [activeRecipientId, setActiveRecipientId] = useState("");

    // Inits hash map messages by result of api GET_CHATS
    // And restores last opened chat
    const setUp = useCallback((friendList, currentUserId) => {
        // 1. Init messages hash map
        const initialMessages = mapFriendsToInitialMessages(friendList, currentUserId);
        setMessages(initialMessages);

        // 2. Restore last opened chat
        const savedChatSet = lastOpenedChatService.load();
        const userSavedChat = savedChatSet[currentUserId];

        if (userSavedChat) {
            const friendExists = friendList.some(f => f.friendId === userSavedChat.friendId);

            if (friendExists) {
                setSelectedId(userSavedChat.conversationId);
                setActiveRecipientId(userSavedChat.friendId);
            } else {
                console.warn("Last opened friend no longer in friend list. Cleaning up.");

                const updatedChatSet = { ...savedChatSet };
                delete updatedChatSet[currentUserId];
                lastOpenedChatService.save(updatedChatSet);
            }
        }
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

    const selectChat = useCallback((conversationId, recipientId) => {
        setSelectedId(conversationId);
        setActiveRecipientId(recipientId);
    }, []);


    const setNoNewMessagesFlag = useCallback((conversationId, isFinished) => {
        setNoNewMessagesFlagsMap(prev => ({
            ...prev,
            [conversationId]: isFinished
        }));
    }, []);

    const setLastReadMessageIds = useCallback((conversationId, messageId) => {
        setLastReadMessageIdsMap(prev => ({
            ...prev,
            [conversationId]: messageId
        }));
    }, []);

    return (
        <ChatsContext.Provider value={{
            messages,
            selectedId,
            activeRecipientId,

            setUp,
            addMessage,
            addHistoryListOfMessages,
            selectChat,

            noNewMessagesFlagsMap,
            lastReadMessageIdsMap,

            setNoNewMessagesFlag,
            setLastReadMessageIds
        }}>
            {children}
        </ChatsContext.Provider>
    )
}

export default ChatsProvider;