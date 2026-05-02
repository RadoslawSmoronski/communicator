import { useState, useCallback, useContext } from 'react'
import { useApi } from '../../../shared/hooks/useApi'
import APIs from '../../../api/ApiURL'

import { UserContext } from '../../../app/providers/UserProvider'


const useChangeAvatar = () => {
    const { user } = useContext(UserContext);
    const mediaApi = useApi({ mediaContent: true })
    const api = useApi();

    const [feedback, setFeedback] = useState(null);
    const [error, setError] = useState(null);
    const [loading, setLoading] = useState(false);

    const addAvatar = useCallback(
        async (action, file, onSuccess) => {

            if (!file) return;

            setError(null);
            setFeedback(null);

            // append file
            const formData = new FormData();
            formData.append("File", file);

            // api call
            const isPost = action === "post";
            const apiCall = isPost
                ? () => mediaApi.post(APIs.AVATAR(user.userID), formData)
                : () => mediaApi.put(APIs.AVATAR(user.userID), formData);

            try {
                const { data } = await apiCall();

                if (onSuccess) onSuccess(data);

                setFeedback(`Avatar ${isPost ? "added" : "updated"} successively!`);
            } catch (err) {
                const status = err.response?.status;
                let message = "Avatar operation failed.";

                if (status === 400) {
                    message = "The provided file is invalid or too large.";
                } else if (status === 403) {
                    message = "You don't have permission to change this avatar.";
                } else if (status === 404) {
                    message = "User account not found.";
                } else if (status === 415) {
                    message = "Unsupported file format. Please use JPG, JPEG, PNG, GIF.";
                } else if (status === 413) {
                    message = "The file is too large for the server to process.";
                } else if (status >= 500) {
                    message = "Server error. Please try again later.";
                }

                setError(message);
            }
        }, [mediaApi]
    );

    const deleteAvatar = useCallback(
        async (onSuccess) => {
            try {
                const { data } = await api.delete(APIs.AVATAR(user.userID));

                if (onSuccess) onSuccess(null);

                setFeedback(`Avatar deleted successively!`);
            } catch (err) {
                const status = err.response?.status;
                let message = "Avatar operation failed.";

                if (status === 403) {
                    message = "You don't have permission to delete this avatar.";
                } else if (status === 404) {
                    message = "User account not found.";
                } else if (status >= 500) {
                    message = "Server error. Please try again later.";
                }

                setError(message);
            }
        }, [api]
    )



    return {
        uploadAvatar: (file, onSuccess) =>
            addAvatar("post", file, onSuccess),

        changeAvatar: (file, onSuccess) =>
            addAvatar("put", file, onSuccess),

        deleteAvatar,

        error,
        setError,
        feedback,
        setFeedback,
        loading
    }
}

export default useChangeAvatar