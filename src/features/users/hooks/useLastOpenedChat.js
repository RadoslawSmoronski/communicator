import { useState, useCallback, useEffect } from "react";
import { lastOpenedChatService } from "../services/lastOpenedChatService";

export const useLastOpenedChat = () => {
    const [lastOpenedChatSet, setLastOpenedChatSet] = useState({});

    useEffect(() => {
        setLastOpenedChatSet(
            lastOpenedChatService.load()
        );
    }, [])

    const getLastOpenedChatByUserId = useCallback((userId) => {
        return lastOpenedChatSet[userId] || null;
    }, [lastOpenedChatSet]);

    // Updates last opened chat 
    // by saving new chatDataDto on userId key
    const updateLastOpenedChat = useCallback((userId, chatDataDto) => {
        setLastOpenedChatSet(prev => {
            const newState = {
                ...prev,
                [userId]: chatDataDto
            };

            lastOpenedChatService.save(newState);
            return newState;
        });
    }, []);

    const removeLastOpenedChat = useCallback((userId) => {
        setLastOpenedChatSet(prev => {
            const newState = { ...prev };

            delete newState[userId];
            lastOpenedChatService.save(newState);

            return newState;
        });
    }, []);

    // Clears cookie
    const clearLastOpenedChat = () => {
        lastOpenedChatService.clear();
        setLastOpenedChatSet({});
    }

    return {
        lastOpenedChatSet,
        getLastOpenedChatByUserId,
        updateLastOpenedChat,
        removeLastOpenedChat,
        clearLastOpenedChat
    }
}