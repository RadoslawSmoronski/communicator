import React, { useEffect } from "react";
import { useSignalR } from "../../app/providers/SignalRProvider";

export const useSignalREvent = (eventName, callback) => {
    const { connection } = useSignalR();

    useEffect(() => {
        if (!connection) return;

        // join new connection
        connection.on(eventName, callback);

        // clear old connection
        return () => {
            connection.off(eventName, callback);
        };
    }, [connection, eventName, callback]);
};