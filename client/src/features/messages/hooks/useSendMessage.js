import React, { useContext } from 'react'
import { useSignalR } from "../../../app/providers/SignalRProvider"

import { ChatsContext } from '../../../app/providers/ChatsProvider'

import SIGNALR_HUBS from "../../../context/SignalRHubs"

const useSendMessage = () => {
    const { activeRecipientId, selectedId: selectedChatId } = useContext(ChatsContext);

    const { connection } = useSignalR();

    // send message
    const sendMessage = async (message) => {
        if (connection) {
            await connection.invoke(
                SIGNALR_HUBS.SEND_MESSAGE,
                activeRecipientId, selectedChatId,
                message);
        }
    };

    return {
        sendMessage
    };
}

export default useSendMessage;