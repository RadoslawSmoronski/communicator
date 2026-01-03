import React, { useState, useContext, useCallback } from 'react'
import { useApi } from '../../../shared/hooks/useApi'
import { UserContext } from '../../../app/providers/UserProvider'
import APIs from '../../../api/ApiURL'

const useChangeUsername = () => {
    const { user, saveUsername } = useContext(UserContext);
    const api = useApi();

    const [error, setError] = useState(null);
    const [feedback, setFeedback] = useState(null);
    const [loading, setLoading] = useState(false);

    const changeUsername = useCallback(
        async (formData, valid, onSuccess) => {
            setError(null);
            setFeedback(null);

            if (!formData) {
                setError("Field can't be null!");
                return;
            }

            if (!valid) {
                setError("Username does not meet the criteria!");
                return;
            }

            if (formData === user.username) {
                return;
            }

            setLoading(true);

            try {
                const { data } = await api.patch(
                    APIs.CHANGE_USERNAME(user.userID),
                    { newUsername: formData }
                );

                if (onSuccess) onSuccess(data);

                setFeedback("Username changed successfully!");

            } catch (err) {
                setError(err.response?.data?.detail ?? "Something went wrong");
            } finally {
                setLoading(false);
            }
        },
        [api]
    );

    return ({
        changeUsername,
        error,
        setError,
        feedback,
        setFeedback,
        loading
    })
}

export default useChangeUsername