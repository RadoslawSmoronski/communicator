import { useState, useEffect, useContext } from "react";
import { useApi } from "../../../shared/hooks/useApi";
import { AuthContext } from "../../../app/providers/AuthProvider";
import { UserContext } from "../../../app/providers/UserProvider";
import APIs from "../../../api/ApiURL";

import { mapFriendList } from "../mappers/chatMapper";
import { chatDtoMock } from "../mocks/getChatsMock";

import listUtils from "../../../shared/utils/listUtils";

export const useGetChats = (useMock = false) => {
    const { accessToken } = useContext(AuthContext);
    const { user } = useContext(UserContext);
    const api = useApi(accessToken);

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
            if (!data?.length) {
                setFriendList([]);
                return;
            }

            const mapped = mapFriendList(data);
            const sorted = listUtils.returnSortedByLastMessDateFriendsList(mapped);

            setFriendList(sorted);
            setFriendListFiltered(sorted);
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
    }, [accessToken, useMock]);

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
