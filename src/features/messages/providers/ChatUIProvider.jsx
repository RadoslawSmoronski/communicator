import React, { createContext, useState } from "react";

export const ChatUIContext = createContext();

const ChatUIProvider = ({ children }) => {
    const [displayConfirmationBox, setDisplayConfimationBox] = useState(false);
    const [displayFriendDetails, setDisplayFriendDetails] = useState(false);

    const toggleFriendDetails = () => {
        setDisplayFriendDetails(!displayFriendDetails);
    }

    const hideFriendDetails = () => {
        setDisplayFriendDetails(false);
    }

    const showConfirmationBox = () => {
        setDisplayConfimationBox(true);
    }

    const hideConfirmationBox = () => {
        setDisplayConfimationBox(false);
    }

    return (
        <ChatUIContext.Provider value={{
            displayConfirmationBox,
            displayFriendDetails,

            showConfirmationBox,
            hideConfirmationBox,
            toggleFriendDetails,
            hideFriendDetails
        }}>
            {children}
        </ChatUIContext.Provider>
    )
}

export default ChatUIProvider;