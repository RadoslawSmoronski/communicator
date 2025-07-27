import React, { useState, useEffect, useContext } from "react";
import { Outlet, Link, useLocation } from 'react-router-dom';
import { AuthContext } from '../context/AuthProvider';

import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { faCircleInfo } from "@fortawesome/free-solid-svg-icons";

import axios from "../api/axios";
import APIs from "../api/ApiURL";
import Avatar from "./Avatar";

const EditProfilePanel = ({ togglePanel }) => {
    const { avatarUrl, setAvatarUrl, email, username, accessToken, refreshAccessToken, saveToCookie } = useContext(AuthContext);
    const [previewAvatarUrl, setPreviewAvatarUrl] = useState(null);
    const [file, setFile] = useState(null);
    const [isEditingAvatar, setIsEditingAvatar] = useState(false);
    const [feedbackAvatar, setFeedbackAvatar] = useState(null);
    const [errorFeedbackAvatar, setErrorFeedbackAvatar] = useState(null);

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

    const afterAvatarAction = (avatarPath, feedback) => {
        setAvatarUrl(avatarPath);
        setIsEditingAvatar(false);
        setFile(null);
        setErrorFeedbackAvatar(null);
        setFeedbackAvatar(feedback);
        saveToCookie({ _avatarUrl: avatarPath });
    }

    const cancelAvatarAction = () => {
        setPreviewAvatarUrl(null);
        setErrorFeedbackAvatar(null);
        setFeedbackAvatar(null);
        setIsEditingAvatar(false);
    }

    const editAvatar = () => {
        setIsEditingAvatar(true);
        setErrorFeedbackAvatar(null);
        setFeedbackAvatar(null);
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
                afterAvatarAction(returnedUrl, "Avatar added successively!");
            }
        } catch (err) {
            if (err.response?.status === 401) {
                await refreshAccessToken();
                await uploadAvatar();
            } else {
                let errorFeedback = err.response?.data.detail;
                if (errorFeedback) {
                    setErrorFeedbackAvatar(errorFeedback);
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
                afterAvatarAction(returnedUrl, "Avatar updated successively!");
            }
        } catch (err) {
            if (err.response?.status === 401) {
                await refreshAccessToken();
                await uploadAvatar();
            } else {
                let errorFeedback = err.response?.data.detail;
                if (errorFeedback) {
                    setErrorFeedbackAvatar(errorFeedback);
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
                afterAvatarAction(null, "Avatar deleted successively!");
            }
        } catch (err) {
            if (err.response?.status === 401) {
                await refreshAccessToken();
                await uploadAvatar();
            }
        }
    }

    return (
        <div className="offFocusBK" onClick={() => togglePanel('editProfilePanel')}>
            <div
                className="editProfilePanel"
                onClick={(e) => e.stopPropagation()}
            >
                <div className="closeBtnBox">
                    <div className="closeBtn" onClick={() => togglePanel('editProfilePanel')} />
                </div>

                <div className="editProfileTitle">Avatar</div>
                <div className="editProfileElement">
                    <div>
                        <Avatar url={(previewAvatarUrl && file) ? previewAvatarUrl : avatarUrl} size={150} />
                        {isEditingAvatar &&
                            <input type="file" onChange={handleFileChange} />
                        }
                    </div>
                    <div className="editProfileBtnWrapper">
                        {isEditingAvatar ? (
                            <>
                                <button className='editPanelBtn save' onClick={avatarUrl == null ? uploadAvatar : updateAvatar}>Save</button>

                                {avatarUrl != null && (
                                    <button className='editPanelBtn delete' onClick={deleteAvatar}>Delete</button>
                                )}

                                <button className='editPanelBtn default' onClick={cancelAvatarAction}>Cancel</button>
                            </>
                        ) : (
                            <button className='editPanelBtn default' onClick={editAvatar}>
                                {avatarUrl == null ? <>Add</> : <>Edit</>}
                            </button>
                        )}
                    </div>
                </div>


                {
                feedbackAvatar ?
                    <div className="editProfileBtnInfo success">
                        <FontAwesomeIcon icon={faCircleInfo} /> {feedbackAvatar}
                    </div>
                    :
                    errorFeedbackAvatar &&
                    <div className="editProfileBtnInfo fail">
                        <FontAwesomeIcon icon={faCircleInfo} /> {errorFeedbackAvatar}
                    </div>
                }


            </div>
        </div>
    )
}

export default EditProfilePanel