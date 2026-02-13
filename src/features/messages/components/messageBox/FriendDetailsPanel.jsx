import React, { useState, useContext, useEffect } from "react";
import { AuthContext } from "../../../../app/providers/AuthProvider";
import { ChatUIContext } from "../../providers/ChatUIProvider";

import axios from "../../../../api/axios";
import APIs from "../../../../api/ApiURL";
import cookieUtils from "../../../../shared/utils/cookieUtils";

const FriendDetailsPanel = ({
    friendName
}) => {
    const { showConfirmationBox } = useContext(ChatUIContext);

    return (
        <div className='friendDetailsPanel'>
            Details about {friendName}
            <button className='btn2 delete' onClick={() => showConfirmationBox()}>Remove friend</button>
        </div>
    )
}

export default FriendDetailsPanel