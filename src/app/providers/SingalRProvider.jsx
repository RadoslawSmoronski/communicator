import React, { createContext, useState } from "react";
import { useContext } from "react";

export const SignalRContext = createContext();

const SignalRProvider = ({ children }) => {
    const [signalRConnection, setSignalRConnection] = useState(null);
    const [signalRConnectionRef, setSignalRConnectionRef] = useState(null);

    const signalRConnect = () => {

    }

    const signalRDisconnect = () => {

    }

    return (
        <SignalRContext.Provider value={{

        }}>
            {children}
        </SignalRContext.Provider>
    )
}

export default SignalRProvider;