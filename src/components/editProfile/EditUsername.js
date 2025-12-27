import React, { useState, useEffect, useContext } from "react";
import { AuthContext } from '../../app/providers/AuthProvider';

import axios from "../../api/axios";
import APIs from "../../api/ApiURL";

import ValidatedInput from "../../shared/components/form/ValidatedInput"
import regexUtils from "../../shared/utils/regexUtils";
import FeedbackText from "../form/FeedbackText";

const EditUsername = () => {
    const { userId, username, setUsername, accessToken, refreshAccessToken, saveToCookie } = useContext(AuthContext);

    const [formData, setFormData] = useState(username);
    const [regexStatus, setRegexStatus] = useState(true);

    const [isEditing, setIsEditing] = useState(false);
    const [feedback, setFeedback] = useState(null);
    const [errorFeedback, setErrorFeedback] = useState(null);

    const handleChange = (e) => {
        setErrorFeedback(null);
        setFeedback(null);

        if (isEditing) {
            const { name, value } = e.target;

            if (name === "username") {
                setFormData(value);
                setRegexStatus(regexUtils.USERNAME.test(value));
            }
        }
    }

    const cancelAction = () => {
        setErrorFeedback(null);
        setFeedback(null);
        setIsEditing(false);
        setFormData(username);
    }

    const edit = () => {
        setRegexStatus(true);
        setIsEditing(true);
        setErrorFeedback(null);
        setFeedback(null);
    }

    const afterSaveAction = (newUsername, feedback) => {
        setUsername(newUsername);
        setIsEditing(false);
        setErrorFeedback(null);
        setFeedback(feedback);
        saveToCookie({ _username: newUsername });
    }


    const saveChanges = async () => {
        if (formData == "") {
            setErrorFeedback("Field cann't be null!");
            return;
        }
        else if (!regexStatus) {
            setErrorFeedback("Username does not meet the criteria!");
            return;
        } else if (formData === username) {
            return;
        }

        try {
            const res = await axios.patch(APIs.CHANGE_USERNAME(userId),
                {
                    newUsername: formData
                },
                {
                    withCredentials: true,
                    headers: {
                        'Authorization': `Bearer ${accessToken}`,
                        'Content-Type': 'application/json'
                    }
                }
            );

            if (res.status === 200) {
                let newUsername = res.data.username;
                afterSaveAction(newUsername, "Username changed successfully!");
            }
        } catch (err) {
            if (err.response?.status === 401) {
                await refreshAccessToken();
                await saveChanges();
            } else {
                let errorFeedback = err.response?.data.detail;
                if (errorFeedback) {
                    setErrorFeedback(errorFeedback);
                }
            }
        }
    }

    return (
        <>
            <div className="editProfileTitle">Username</div>
            <div className="editProfileElement">
                <div className="editPanelInputWrapper">
                    <ValidatedInput
                        htmlName="username"
                        labelText="New username"
                        formData={formData}
                        regexStatus={regexStatus}
                        formFocus={isEditing}
                        isDisabled={!isEditing}
                        handleChange={handleChange}
                        inputType="text"
                        addClassName="editProfile"
                        validationText={
                            <>
                                Has 5 - 24 characters in length<br />
                                Has to start with English letter<br />
                                Can contain English letters, digits and -_#
                            </>
                        }
                    />
                </div>
                <div className="editProfileBtnWrapper">
                    {isEditing ? (
                        <>
                            <button className='editPanelBtn save' onClick={saveChanges}>Save</button>
                            <button className='editPanelBtn default' onClick={cancelAction}>Cancel</button>
                        </>
                    ) : (
                        <button className='editPanelBtn default' onClick={edit}>Edit</button>
                    )}
                </div>
            </div>
            <FeedbackText successText={feedback} failText={errorFeedback} />
        </>
    )
}

export default EditUsername