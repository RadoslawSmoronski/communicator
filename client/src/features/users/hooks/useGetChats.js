import { useState, useEffect, useContext } from "react";
import { useApi } from "../../../shared/hooks/useApi";
import { UserContext } from "../../../app/providers/UserProvider";
import { ChatsContext } from "../../../app/providers/ChatsProvider";
import APIs from "../../../api/ApiURL";

import { mapFriendList } from "../mappers/chatMapper";
import { chatDtoMock } from "../mocks/getChatsMock";

import listUtils from "../../../shared/utils/listUtils";

export const useGetChats = (useMock = false) => {
    const { user } = useContext(UserContext);
    const { setUp } = useContext(ChatsContext);
    const api = useApi();

    const [friendList, setFriendList] = useState([]);

    const [loading, setLoading] = useState(true);
    const [error, setError] = useState(null);

    const getChats = async () => {
        setLoading(true);

        const fetchChats = async () => {
            if (useMock) {
                return { data: chatDtoMock };
            } else {
                return api.get(APIs.GET_CHATS(user.userID));
            }
        };

        try {
            const { data } = await fetchChats();

            // no friends
            if (!data?.length) {
                setFriendList([]);
                return;
            }

            // set friend list
            const mapped = mapFriendList(data);
            setFriendList(mapped);

            // set messages (hashmap)
            setUp(mapped, user.userID);

        } catch (err) {
            console.error(err);
            setError(err);
            setFriendList([]);
        } finally {
            setLoading(false);
        }
    };

    useEffect(() => {
        getChats();
    }, [useMock]);

    return {
        friendList,
        setFriendList,
        loading,
        error,
        getChats,
    };
};
