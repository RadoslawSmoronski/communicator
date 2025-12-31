import { useContext, useCallback } from "react";
import { useApi } from "../../../shared/hooks/useApi";
import { AuthContext } from "../../../app/providers/AuthProvider";
import { FriendsContext } from "../../../app/providers/FriendsProvider";
import APIs from "../../../api/ApiURL";

export const useInvitationAction = (removeInvitationById) => {
    const { accessToken } = useContext(AuthContext);
    const api = useApi(accessToken);

    const { refreshFriendList } = useContext(FriendsContext);

    const invitationAction = useCallback(
        async (action, invitationId) => {
            const isAccept = action === "accept";
            const apiCall = isAccept
                ? () => api.post(APIs.ACCEPT_INVITE(invitationId), {})
                : () => api.delete(APIs.DECELINE_INVITE(invitationId));

            try {
                const { data } = await apiCall();

                // delete invitation
                removeInvitationById(invitationId);

                // refresh friends
                refreshFriendList();
            } catch (err) {
                console.error("Invitation action failed:", err);
            }
        },
        [api]
    );

    return {
        acceptInvitation: (invitationId) =>
            invitationAction("accept", invitationId),

        declineInvitation: (invitationId) =>
            invitationAction("decline", invitationId),
    };
};
