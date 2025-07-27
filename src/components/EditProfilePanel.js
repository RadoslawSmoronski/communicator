import React, { useState, useEffect, useContext } from "react";
import { Outlet, Link, useLocation } from 'react-router-dom';
import { AuthContext } from '../context/AuthProvider';

import axios from "../api/axios";
import APIs from "../api/ApiURL";
import Avatar from "./Avatar";

const EditProfilePanel = ({ togglePanel }) => {
    const { avatarUrl, setAvatarUrl, email, username, accessToken, refreshAccessToken, saveToCookie } = useContext(AuthContext);
    const [file, setFile] = useState(null);

    const handleFileChange = (e) => {
        setFile(e.target.files[0]);
    };

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
                let returnedUrl = APIs.SERVER_URL + "/avatars/" +res.data;
                setAvatarUrl(returnedUrl);
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
                let returnedUrl = APIs.SERVER_URL + "/avatars/" +res.data;
                setAvatarUrl(returnedUrl);
                saveToCookie();
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
                setAvatarUrl(null);
                saveToCookie();
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
                    <Avatar url={avatarUrl} size={150} />

                    <div className="editProfileBtnWrapper">
                        <input type="file" onChange={handleFileChange} />
                        {avatarUrl == null ?
                            (
                                <>
                                    
                                    <button className='btn2' onClick={uploadAvatar}>Save</button>
                                </>
                            ) :
                            (
                                <>
                                    <button className='btn2' onClick={updateAvatar}>Edit</button>
                                    <button className='btn2' onClick={deleteAvatar}>Delete</button>
                                </>
                            )
                        }

                    </div>
                </div>

            </div>
        </div>
    )
}

export default EditProfilePanel