import React from "react";

import SignalRProvider from "../../app/providers/SignalRProvider";
import FriendsProvider from "../../app/providers/FriendsProvider";
import ChatsProvider from "../../app/providers/ChatsProvider";
import ChatUIProvider from "../../features/messages/providers/ChatUIProvider";

import { MessagePageRefactor } from "./MessagePageRefactor";

const MessageWrapper = () => {
    return (
        <SignalRProvider>
            <ChatsProvider>
                <FriendsProvider>
                    <ChatUIProvider>
                        <MessagePageRefactor />
                    </ChatUIProvider>
                </FriendsProvider>
            </ChatsProvider>
        </SignalRProvider>
    );
};

export default MessageWrapper;