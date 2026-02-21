import React, { createContext, useState } from "react";

export const ChatUIContext = createContext();

const ChatUIProvider = ({ children }) => {
    const [displayConfirmationBox, setDisplayConfimationBox] = useState(false);
    const [displayFriendDetails, setDisplayFriendDetails] = useState(false);
    const [mobileActiveChat, setMobileActiveChat] = useState(false);
    const [newNotificationBackBtn, setNewNotificationBackBtn] = useState(false);

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

    // mobile
    const showMobileChat = () => {
        setMobileActiveChat(true);
    }
    const showMobileFriendList = () => {
        setMobileActiveChat(false);
    }

    return (
        <ChatUIContext.Provider value={{
            displayConfirmationBox,
            displayFriendDetails,
            mobileActiveChat,
            newNotificationBackBtn,

            showConfirmationBox,
            hideConfirmationBox,
            toggleFriendDetails,
            hideFriendDetails,
            showMobileChat,
            showMobileFriendList,
            setNewNotificationBackBtn
        }}>
            {children}
        </ChatUIContext.Provider>
    )
}

export default ChatUIProvider;