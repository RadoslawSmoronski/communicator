import React from "react";

import SignalRProvider from "../../app/providers/SignalRProvider";
import FriendsProvider from "../../app/providers/FriendsProvider";
import ChatsProvider from "../../app/providers/ChatsProvider";
import ChatUIProvider from "../../features/messages/providers/ChatUIProvider";

import { MessagePage } from "./MessagePage";

const MessageWrapper = () => {
    return (
        <SignalRProvider>
            <ChatsProvider>
                <ChatUIProvider>
                    <FriendsProvider>
                        <MessagePage />
                    </FriendsProvider>
                </ChatUIProvider>
            </ChatsProvider>
        </SignalRProvider>
    );
};

export default MessageWrapper;