import React, { useState, useContext, useCallback } from 'react'
import { useApi } from '../../../shared/hooks/useApi'
import { UserContext } from '../../../app/providers/UserProvider'
import APIs from '../../../api/ApiURL'

const useChangeUsername = () => {
    const { user } = useContext(UserContext);
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
                const status = err.response?.status;
                let message = "An error occurred while changing username.";

                if (status === 400) {
                    message = "Username does not meet the criteria.";
                } else if (status === 403) {
                    message = "You are not authorized to change this username.";
                } else if (status === 404) {
                    message = "User account not found.";
                } else if (status === 409) {
                    message = "This username is already taken. Please try another one.";
                } else if (status >= 500) {
                    message = "Server error. Please try again later.";
                }

                setError(message);
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