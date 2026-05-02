import React, { createContext, useContext, useEffect, useRef, useState } from "react";
import * as signalR from "@microsoft/signalr";
import SIGNALR_HUBS from "../../context/SignalRHubs";
import APIs from "../../api/ApiURL";

import { AuthContext } from "./AuthProvider";

export const SignalRContext = createContext(null);

const SignalRProvider = ({ children }) => {
    const { accessToken } = useContext(AuthContext);

    const [isConnected, setIsConnected] = useState(false);
    const connectionRef = useRef(null);

    useEffect(() => {
        // init connection
        const connection = new signalR.HubConnectionBuilder()
            .withUrl(`${APIs.SERVER_URL}${SIGNALR_HUBS.CHATHUB}`, {
                accessTokenFactory: () => accessToken
            })
            .withAutomaticReconnect()
            .build();

        // options
        connection.onreconnecting(() => setIsConnected(false));
        connection.onreconnected(() => setIsConnected(true));
        connection.onclose(() => setIsConnected(false));

        // start connection
        const startConnection = async () => {
            try {
                await connection.start();
                console.log("SignalR Connected");

                setIsConnected(true);
                connectionRef.current = connection;
            } catch (err) {
                console.error("SignalR Connection Error: ", err);
                setTimeout(startConnection, 5000); // reconnect
            }
        };

        startConnection();

        return () => {
            if (connectionRef.current) {
                connectionRef.current.stop();
            }
        };
    }, [accessToken]);

    return (
        <SignalRContext.Provider value={{ connection: connectionRef.current, isConnected }}>
            {children}
        </SignalRContext.Provider>
    );
};

export default SignalRProvider;

// getter for signalR connection
export const useSignalR = () => useContext(SignalRContext);