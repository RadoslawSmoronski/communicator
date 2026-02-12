import { useState, useContext } from "react";
import { useApi } from "../../../shared/hooks/useApi";
import { ChatsContext } from "../../../app/providers/ChatsProvider";
import APIs from "../../../api/ApiURL";

export const useGetMessages = () => {
    const api = useApi();
    const {
        messages,
        addHistoryListOfMessages,
        setNoNewMessagesFlag,
        setLastReadMessageIds
    } = useContext(ChatsContext);

    const [loading, setLoading] = useState(false);
    const [error, setError] = useState(null);

    const getMessagesForFriend = async (conversationId) => {
        // current chat messages
        const currentChatMessages = messages[conversationId] || [];
        const chatLength = currentChatMessages.length;

        setLoading(true);
        setError(null);

        try {
            let response;

            if (chatLength > 0) {
                // fetch by last message id
                const lastMessageId = currentChatMessages[chatLength - 1].messageId;
                response = await api.get(APIs.GET_MESSAGES(conversationId, lastMessageId));
            } else {
                return;
            }

            const { messages: newMessages, lastFriendReadMessageId } = response.data;

            // 1. Save list of messages to provider
            addHistoryListOfMessages(conversationId, newMessages);

            // 2. Set no new messages flag 
            // if result is empty list
            if (newMessages.length === 0) {
                setNoNewMessagesFlag(conversationId, true);
            }

            // 3. Update last friend read messageId
            if (lastFriendReadMessageId) {
                setLastReadMessageIds(conversationId, lastFriendReadMessageId);
            }

        } catch (err) {
            console.error("Error fetching messages:", err);
            setError(err);

            setNoNewMessagesFlag(conversationId, true);
        } finally {
            setLoading(false);
        }
    };

    return {
        getMessagesForFriend,
        loading,
        error
    };
};