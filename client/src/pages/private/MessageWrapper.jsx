import React from "react";

import SignalRProvider from "../../app/providers/SignalRProvider";
import FriendsProvider from "../../app/providers/FriendsProvider";
import ChatsProvider from "../../app/providers/ChatsProvider";
import ChatUIProvider from "../../features/messages/providers/ChatUIProvider";
import PanelUIProvider from "../../app/providers/PanelUIProvider";

import { MessagePage } from "./MessagePage";

const MessageWrapper = () => {
    return (
        <SignalRProvider>
            <ChatsProvider>
                <ChatUIProvider>
                    <PanelUIProvider>
                        <FriendsProvider>
                            <MessagePage />
                        </FriendsProvider>
                    </PanelUIProvider>
                </ChatUIProvider>
            </ChatsProvider>
        </SignalRProvider>
    );
};

export default MessageWrapper;