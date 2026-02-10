import { useState, useCallback, useEffect } from "react";
import { lastOpenedChatService } from "../services/lastOpenedChatService";

export const useLastOpenedChat = () => {
    const [lastOpenedChatSet, setLastOpenedChatSet] = useState({});

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

    // Clears cookie
    const clearLastOpenedChat = () => {
        lastOpenedChatService.clear();
        setLastOpenedChatSet({});
    }

    useEffect(() => {
        setLastOpenedChatSet(
            lastOpenedChatService.load()
        );
    }, [])

    return {
        lastOpenedChatSet,
        updateLastOpenedChat,
        clearLastOpenedChat
    }
}