import { useState, useEffect, useContext } from "react";
import { useApi } from "../../../shared/hooks/useApi";
import { UserContext } from "../../../app/providers/UserProvider";
import APIs from "../../../api/ApiURL";

import { invitationsDtoMock } from "../mocks/invitationsDtoMock";
import { mapInvitationList } from "../mappers/invitationMapper";


export const useFriendInvitations = (useMock = false) => {
    const { user } = useContext(UserContext);
    const api = useApi();

    const [invitationsList, setInvitationsList] = useState([]);

    const [loading, setLoading] = useState(false);
    const [error, setError] = useState(null);

    const getInvitations = async () => {
        setLoading(true);

        const fetchInvitations = async () => {
            if (useMock) {
                return { data: invitationsDtoMock };
            } else {
                return api.get(APIs.GET_INVITATIONS(user.userID));
            }
        };

        try {
            const { data } = await fetchInvitations();
            if (!data?.length) {
                setInvitationsList([]);
                return;
            }

            const mapped = mapInvitationList(data);

            setInvitationsList(mapped);

        } catch (err) {
            console.error(err);
            setError(err);
            setInvitationsList([]);
        } finally {
            setLoading(false);
        }
    }

    useEffect(() => {
        getInvitations();
    }, [useMock]);

    const removeInvitationById = (id) => {
        setInvitationsList(prev =>
            prev.filter(inv => inv.id !== id)
        );
    }

    return {
        invitationsList,
        removeInvitationById,
        loading,
        error
    };
}