import React, { useState, useEffect, useContext } from "react";
import { AuthContext } from '../../context/AuthProvider';

import axios from "../../api/axios";
import APIs from "../../api/ApiURL";

import ValidatedInput from "../form/ValidatedInput";
import regexUtils from "../../utils/regexUtils";
import FeedbackText from "../form/FeedbackText";

const EditPassword = () => {
    const { accessToken, refreshAccessToken } = useContext(AuthContext);

    const [formData, setFormData] = useState({
        oldpass: "",
        newpass: "",
        newpass2: ""
    });

    const [regexStatus, setRegexStatus] = useState({
        newpass: false,
        newpass2: false
    });

    const [formFocus, setFormFocus] = useState({
        oldpass: false,
        newpass: false,
        newpass2: false
    });

    const [isEditing, setIsEditing] = useState(false);
    const [feedback, setFeedback] = useState(null);
    const [errorFeedback, setErrorFeedback] = useState(null);

    const handleChange = (e) => {
        setErrorFeedback(null);
        setFeedback(null);

        if (isEditing) {
            const { name, value } = e.target;

            setFormData(prev => ({
                ...prev,
                [name]: value
            }));

            if (name === "newpass") {
                setRegexStatus((prev) => ({
                    ...prev,
                    newpass: regexUtils.PASSWORD.test(value),
                    newpass2: formData.newpass2 === value,
                }));
            }
            else if (name === "newpass2") {
                setRegexStatus((prev) => ({
                    ...prev,
                    newpass2: formData.newpass === value,
                }));
            }
        }
    };

    const cancelAction = () => {
        setErrorFeedback(null);
        setFeedback(null);
        setIsEditing(false);
        setFormData({
            oldpass: "",
            newpass: "",
            newpass2: ""
        });
    }

    const edit = () => {
        // setRegexStatus(true);
        setIsEditing(true);
        setErrorFeedback(null);
        setFeedback(null);
    }

    const afterSaveAction = (feedback) => {
        setIsEditing(false);
        setErrorFeedback(null);
        setFeedback(feedback);
    }


    const handleFocusOn = (e) => {
        if (isEditing) {
            const { name } = e.target;
            setFormFocus({ oldpass: false, newpass: false, newpass2: false });
            setFormFocus((prev) => ({ ...prev, [name]: true }));
        }
    };

    const saveChanges = async () => {
        const { oldpass, newpass, newpass2 } = formData;
        const { newpass: newpassRegex, newpass2: newpass2Regex } = regexStatus;

        if (!oldpass || !newpass || !newpass2) {
            setErrorFeedback("Fields cann't be null!");
            return;
        }
        else if (!newpassRegex) {
            setErrorFeedback("Password does not meet the criteria!");
            return;
        } else if (!newpass2Regex) {
            setErrorFeedback("Passwords are not the same!");
            return;
        }

        try {
            const res = await axios.patch(`${APIs.CHANGE_PASSWORD}?OldPassword=${oldpass}&NewPassword=${newpass}`,
                null,
                {
                    withCredentials: true,
                    headers: {
                        'Authorization': `Bearer ${accessToken}`,
                        'Content-Type': 'application/json'
                    }
                }
            );

            if (res.status === 200) {
                afterSaveAction("Password successfully changed.");
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
        setFormData({
            oldpass: "",
            newpass: "",
            newpass2: ""
        });
    }


    return (
        <>
            <div className="editProfileTitle">Password</div>
            <div className="editProfileElement">
                <div className="editPanelInputWrapper">
                    <ValidatedInput
                        htmlName="oldpass"
                        labelText="Old password"
                        formData={formData.oldpass}
                        regexStatus={true}
                        formFocus={formFocus.oldpass}
                        isDisabled={!isEditing}
                        handleChange={handleChange}
                        handleFocusOn={handleFocusOn}
                        inputType="password"
                        addClassName="editProfile"
                        validationText={<></>}
                    />
                    <ValidatedInput
                        htmlName="newpass"
                        labelText="New password"
                        formData={formData.newpass}
                        regexStatus={regexStatus.newpass}
                        formFocus={formFocus.newpass}
                        isDisabled={!isEditing}
                        handleChange={handleChange}
                        handleFocusOn={handleFocusOn}
                        inputType="password"
                        addClassName="editProfile"
                        validationText={
                            <>
                                Has 8 - 64 characters in length<br />
                                At least one uppercase English letter <br />
                                At least one lowercase English letter<br />
                                At least one digit and special character
                            </>
                        }
                    />
                    <ValidatedInput
                        htmlName="newpass2"
                        labelText="Repeat password"
                        formData={formData.newpass2}
                        regexStatus={regexStatus.newpass2}
                        formFocus={formFocus.newpass2}
                        isDisabled={!isEditing}
                        handleChange={handleChange}
                        handleFocusOn={handleFocusOn}
                        inputType="password"
                        addClassName="editProfile"
                        validationText={<>Passwords have to match</>}
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

export default EditPassword