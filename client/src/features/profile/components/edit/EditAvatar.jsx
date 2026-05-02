import React, { useState, useContext } from "react";

import { UserContext } from "../../../../app/providers/UserProvider";

import useEditableState from "../../hooks/useEditableState";
import useChangeAvatar from "../../hooks/useChangeAvatar";

import Avatar from "../../../../shared/components/Avatar";
import FeedbackText from "../FeedbackText";

const EditAvatar = () => {
    const { user, saveAvatarUrl } = useContext(UserContext);
    // file reader
    const [previewAvatarUrl, setPreviewAvatarUrl] = useState(null);
    const [file, setFile] = useState(null);

    const handleFileChange = (e) => {
        const selectedFile = e.target.files[0];
        if (!selectedFile) return;

        setFile(selectedFile);

        const reader = new FileReader();
        reader.onloadend = () => {
            setPreviewAvatarUrl(reader.result);
        };
        reader.readAsDataURL(selectedFile);
    };

    const {
        uploadAvatar,
        changeAvatar,
        deleteAvatar,
        error,
        setError,
        feedback,
        setFeedback,
        loading
    } = useChangeAvatar();

    const onChangeAction = () => {
        setError(null);
        setFeedback(null);
    }

    const { isEditing, edit, cancel, onSuccess } = useEditableState({
        onChangeAction,
        cancelCallback: () => setPreviewAvatarUrl(null),
        onSuccessCallback: (data) => {
            setFile(null);
            // save avatarUrl
            const newAvatarUrl = data?.avatarUrl ?? null;
            saveAvatarUrl(newAvatarUrl);
            setPreviewAvatarUrl(newAvatarUrl);
        }
    });

    const saveAvatar = () => user.avatarUrl == null
        ? uploadAvatar(file, onSuccess)
        : changeAvatar(file, onSuccess);

    const removeAvatar = () => deleteAvatar(onSuccess);

    return (
        <>

            <div className="editProfileTitle">Avatar</div>
            <div className="editProfileElement">
                <div>
                    <Avatar
                        url={(previewAvatarUrl && file) ? previewAvatarUrl : user.avatarUrl}
                        className="editProfileAvatar"
                    />
                    {isEditing &&
                        <input type="file" onChange={handleFileChange} />
                    }
                </div>
                <div className="editProfileBtnWrapper">
                    {isEditing ? (
                        <>
                            <button className='editPanelBtn save' onClick={saveAvatar}>Save</button>

                            {user.avatarUrl != null && (
                                <button className='editPanelBtn delete' onClick={removeAvatar}>Delete</button>
                            )}

                            <button className='editPanelBtn default' onClick={cancel}>Cancel</button>
                        </>
                    ) : (
                        <button className='editPanelBtn default' onClick={edit}>
                            {user.avatarUrl == null ? <>Add</> : <>Edit</>}
                        </button>
                    )}
                </div>
            </div>
            <FeedbackText successText={feedback} failText={error} />

        </>
    )
}

export default EditAvatar