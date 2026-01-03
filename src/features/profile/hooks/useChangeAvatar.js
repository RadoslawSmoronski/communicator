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
                setError(
                    err.response?.data?.detail ?? "Avatar operation failed"
                );
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
                setError(
                    err.response?.data?.detail ?? "Avatar operation failed"
                );
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