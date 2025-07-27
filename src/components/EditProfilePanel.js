import React, { useState, useEffect, useContext } from "react";
import { Outlet, Link, useLocation } from 'react-router-dom';
import { AuthContext } from '../context/AuthProvider';

import axios from "../api/axios";
import APIs from "../api/ApiURL";
import Avatar from "./Avatar";

const EditProfilePanel = ({ togglePanel }) => {
    const { avatarUrl, setAvatarUrl, email, username, accessToken, refreshAccessToken, saveToCookie } = useContext(AuthContext);
    const [file, setFile] = useState(null);
    const [isEditing, setIsEditing] = useState(false);

    const handleFileChange = (e) => {
        setFile(e.target.files[0]);
    };

    const afterAvatarAction = (avatarPath) => {
        setAvatarUrl(avatarPath);
        setIsEditing(false);
        setFile(null);
        saveToCookie();
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
                console.log(res);
                let returnedUrl = APIs.SERVER_URL + "/avatars/" + res.data;
                afterAvatarAction(returnedUrl);
            }
        } catch (err) {
            console.error(err);
            if (err.response?.status === 401) {
                await refreshAccessToken();
                await uploadAvatar();
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
                console.log(res);
                let returnedUrl = APIs.SERVER_URL + "/avatars/" + res.data;
                afterAvatarAction(returnedUrl);
            }
        } catch (err) {
            console.error(err);
            if (err.response?.status === 401) {
                await refreshAccessToken();
                await uploadAvatar();
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
                console.log(res);
                afterAvatarAction(null);
            }
        } catch (err) {
            console.error(err);
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
                        <Avatar url={avatarUrl} size={150} />
                        {isEditing &&
                            <input type="file" onChange={handleFileChange} />
                        }
                    </div>
                    <div className="editProfileBtnWrapper">
                        {isEditing ? (
                            <>
                                <button className='btn2' onClick={avatarUrl == null ? uploadAvatar : updateAvatar}>Save</button>

                                {avatarUrl != null && (
                                    <button className='btn2' onClick={deleteAvatar}>Delete</button>
                                )}

                                <button className='btn2' onClick={() => setIsEditing(false)}>Cancel</button>
                            </>
                        ) : (
                            <button className='btn2' onClick={() => setIsEditing(true)}>
                                {avatarUrl == null ? <>Add</> : <>Edit</>}
                            </button>
                        )}
                    </div>
                </div>

            </div>
        </div>
    )
}

export default EditProfilePanel