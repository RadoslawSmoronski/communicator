import React, { useState, useContext, useCallback } from "react";
import { useApi } from "../../../shared/hooks/useApi";
import { AuthContext } from "../../../app/providers/AuthProvider";
import { UserContext } from "../../../app/providers/UserProvider";
import APIs from "../../../api/ApiURL";

export const useSendInvitation = () => {
    const { user } = useContext(UserContext);
    const { accessToken } = useContext(AuthContext);
    const api = useApi(accessToken);

    const [loading, setLoading] = useState(false);
    const [error, setError] = useState(null);
    const [sent, setSent] = useState(false);

    const senderId = user.userID;

    const sendInvitation = useCallback(async (recipientId) => {
        setLoading(true);
        setError(null);

        try {
            const response = await api.post(
                APIs.SEND_INVITE,
                {
                    senderId,
                    recipientId
                }
            );

            setSent(true);
            return { success: true };

        } catch (err) {
            if (err.response?.status === 409) {
                setSent(true);
                return { success: false, reason: "ALREADY_EXISTS" };
            }

            if (err.response?.status === 400) {
                return { success: false, reason: "VALIDATION_ERROR" };
            }

            setError(err);
            return { success: false, reason: "UNKNOWN_ERROR" };
        } finally {
            setLoading(false);
        }
    }, [api]);

    return {
        sendInvitation,
        loading,
        error,
        sent
    };
};
