import React, { useState, useContext, useCallback } from 'react'
import { useApi } from '../../../shared/hooks/useApi'
import { UserContext } from '../../../app/providers/UserProvider'
import APIs from '../../../api/ApiURL'

const useChangePassword = () => {
    const { user } = useContext(UserContext);
    const api = useApi();

    const [error, setError] = useState(null);
    const [feedback, setFeedback] = useState(null);
    const [loading, setLoading] = useState(false);

    const changePassword = useCallback(
        async (formData, valid, onSuccess) => {
            setError(null);
            setFeedback(null);

            const { oldpass, newpass, newpass2 } = formData;
            const { newpass: newpassValid, newpass2: newpass2Valid } = valid;

            if (!oldpass || !newpass || !newpass2) {
                setError("Fields cann't be null!");
                return;
            }
            else if (!newpassValid) {
                setError("Password does not meet the criteria!");
                return;
            } else if (!newpass2Valid) {
                setError("Passwords are not the same!");
                return;
            }

            setLoading(true);

            try {
                const { data } = await api.patch(
                    APIs.CHANGE_PASSWORD(user.userID),
                    JSON.stringify({
                        oldPassword: oldpass,
                        newPassword: newpass
                    })
                );

                if (onSuccess) onSuccess(data);

                setFeedback("Password successfully changed.");
            } catch (err) {
                const status = err.response?.status;
                let message = "An error occurred while changing username.";

                if (status === 400) {
                    message = "Password does not meet the criteria.";
                } else if (status === 401) {
                    message = "Wrong password. Can't change password.";
                } else if (status === 403) {
                    message = "You are not authorized to change password.";
                } else if (status === 404) {
                    message = "User account not found.";
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
        changePassword,
        error,
        setError,
        feedback,
        setFeedback,
        loading
    })
}

export default useChangePassword