import React, { useState, useEffect, useContext } from "react";
import { Outlet, Link, useLocation } from 'react-router-dom';
import { AuthContext } from '../../context/AuthProvider';

import axios from '../../api/axios';
import APIs from "../../api/ApiURL";
import Avatar from "../Avatar";
import FeedbackText from "../form/FeedbackText";

const EditAvatar = () => {
    const { avatarUrl, setAvatarUrl, accessToken, refreshAccessToken, saveToCookie } = useContext(AuthContext);
    const [previewAvatarUrl, setPreviewAvatarUrl] = useState(null);
    const [file, setFile] = useState(null);

    const [isEditing, setIsEditing] = useState(false);
    const [feedback, setFeedback] = useState(null);
    const [errorFeedback, setErrorFeedback] = useState(null);

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

    const afterSaveAction = (avatarPath, feedback) => {
        setAvatarUrl(avatarPath);
        setIsEditing(false);
        setFile(null);
        setErrorFeedback(null);
        setFeedback(feedback);
        saveToCookie({ _avatarUrl: avatarPath });
    }

    const cancelAction = () => {
        setPreviewAvatarUrl(null);
        setErrorFeedback(null);
        setFeedback(null);
        setIsEditing(false);
    }

    const edit = () => {
        setIsEditing(true);
        setErrorFeedback(null);
        setFeedback(null);
    }

    const uploadAvatar = async () => {
        if (!file) return;

        const formData = new FormData();
        formData.append("file", file);

        try {
            const res = await axios.post(APIs.AVATAR,
                formData,
                {
                    withCredentials: true,
                    headers: {
                        Authorization: `Bearer ${accessToken}`,
                        "Content-Type": "multipart/form-data",
                    },
                });

            if (res.status === 200) {
                let returnedUrl = APIs.SERVER_URL + "/avatars/" + res.data;
                afterSaveAction(returnedUrl, "Avatar added successively!");
            }
        } catch (err) {
            if (err.response?.status === 401) {
                await refreshAccessToken();
                await uploadAvatar();
            } else {
                let errorFeedback = err.response?.data.detail;
                if (errorFeedback) {
                    setErrorFeedback(errorFeedback);
                }
            }
        }
    };

    const updateAvatar = async () => {
        if (!file) return;

        const formData = new FormData();
        formData.append("file", file);

        try {
            const res = await axios.put(APIs.AVATAR,
                formData,
                {
                    withCredentials: true,
                    headers: {
                        Authorization: `Bearer ${accessToken}`,
                        "Content-Type": "multipart/form-data",
                    },
                });

            if (res.status === 200) {
                let returnedUrl = APIs.SERVER_URL + "/avatars/" + res.data;
                afterSaveAction(returnedUrl, "Avatar updated successively!");
            }
        } catch (err) {
            if (err.response?.status === 401) {
                await refreshAccessToken();
                await uploadAvatar();
            } else {
                let errorFeedback = err.response?.data.detail;
                if (errorFeedback) {
                    setErrorFeedback(errorFeedback);
                }
            }
        }
    };

    const deleteAvatar = async () => {
        try {
            const res = await axios.delete(APIs.AVATAR,
                {
                    withCredentials: true,
                    headers: {
                        Authorization: `Bearer ${accessToken}`
                    },
                });

            if (res.status === 200) {
                afterSaveAction(null, "Avatar deleted successively!");
            }
        } catch (err) {
            if (err.response?.status === 401) {
                await refreshAccessToken();
                await uploadAvatar();
            }
        }
    }


    return (
        <>

            <div className="editProfileTitle">Avatar</div>
            <div className="editProfileElement">
                <div>
                    <Avatar url={(previewAvatarUrl && file) ? previewAvatarUrl : avatarUrl} size={150} />
                    {isEditing &&
                        <input type="file" onChange={handleFileChange} />
                    }
                </div>
                <div className="editProfileBtnWrapper">
                    {isEditing ? (
                        <>
                            <button className='editPanelBtn save' onClick={avatarUrl == null ? uploadAvatar : updateAvatar}>Save</button>

                            {avatarUrl != null && (
                                <button className='editPanelBtn delete' onClick={deleteAvatar}>Delete</button>
                            )}

                            <button className='editPanelBtn default' onClick={cancelAction}>Cancel</button>
                        </>
                    ) : (
                        <button className='editPanelBtn default' onClick={edit}>
                            {avatarUrl == null ? <>Add</> : <>Edit</>}
                        </button>
                    )}
                </div>
            </div>
            <FeedbackText  successText={feedback} failText={errorFeedback}/>

        </>
    )
}

export default EditAvatar