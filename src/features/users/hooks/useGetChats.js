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
    const [friendListFiltered, setFriendListFiltered] = useState([]);

    const [loading, setLoading] = useState(false);
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
            const sorted = listUtils.returnSortedByLastMessDateFriendsList(mapped);

            setFriendList(sorted);
            setFriendListFiltered(sorted);

            // set messages (hashmap)
            setUp(sorted, user.userID);

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
        friendListFiltered,
        setFriendListFiltered,
        loading,
        error,
        getChats,
    };
};
