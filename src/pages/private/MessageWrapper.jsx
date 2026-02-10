import React from "react";

import SignalRProvider from "../../app/providers/SingalRProvider";
import FriendsProvider from "../../app/providers/FriendsProvider";
import ChatsProvider from "../../app/providers/ChatsProvider";

import { MessagePageRefactor } from "./MessagePageRefactor";

const MessageWrapper = () => {
    return (
        <SignalRProvider>
            <ChatsProvider>
                <FriendsProvider>
                    <MessagePageRefactor />
                </FriendsProvider>
            </ChatsProvider>
        </SignalRProvider>
    );
};

export default MessageWrapper;