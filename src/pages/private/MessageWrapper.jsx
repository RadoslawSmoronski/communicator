import React from "react";

import SignalRProvider from "../../app/providers/SingalRProvider";
import FriendsProvider from "../../app/providers/FriendsProvider";
import ChatsProvider from "../../app/providers/ChatsProvider";

import { MessagePageRefactor } from "./MessagePageRefactor";

const MessageWrapper = () => {
    return (
        <SignalRProvider>
            <FriendsProvider>
                <ChatsProvider>
                    <MessagePageRefactor />
                </ChatsProvider>
            </FriendsProvider>
        </SignalRProvider>
    );
};

export default MessageWrapper;