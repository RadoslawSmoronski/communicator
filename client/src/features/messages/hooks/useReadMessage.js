import React, { useContext } from 'react'
import { useSignalR } from "../../../app/providers/SignalRProvider"

import { ChatsContext } from '../../../app/providers/ChatsProvider'

import SIGNALR_HUBS from "../../../context/SignalRHubs"

const useReadMessage = () => {
    const { activeRecipientId, selectedId: selectedChatId } = useContext(ChatsContext);

    const { connection } = useSignalR();

    const readMessage = async () => {
        if (connection) {
            await connection.invoke(
                SIGNALR_HUBS.READ_MESSAGE_POST,
                activeRecipientId,
                selectedChatId
            );
        }
    }

    return {
        readMessage
    };
}

export default useReadMessage