import React, { useState, useEffect, useContext } from "react";
import { AuthContext } from '../../../../app/providers/AuthProvider';
import { UserContext } from "../../../../app/providers/UserProvider";

import axios from "../../../../api/axios";
import APIs from "../../../../api/ApiURL";

import ValidatedInput from "../../../../shared/components/form/ValidatedInput"
import { useValidatedForm } from "../../../../shared/hooks/forms/useValidatedForm";
import regexUtils from "../../../../shared/utils/regexUtils";
import FeedbackText from "../../../../components/form/FeedbackText";

const EditUsername = () => {
    const { accessToken } = useContext(AuthContext);
    const { user, saveUser } = useContext(UserContext);

    const initForm = { username: user.username }
    const validator = { username: regexUtils.USERNAME }

    const onChangeAction = () => {
        setErrorFeedback(null);
        setFeedback(null);
    }

    const {
        fields,
        valid,
        focus,
        handleFieldChange,
        handleFieldFocus,
        resetForm,
        RESET_MODE
    } = useValidatedForm(
        initForm, validator, onChangeAction, true, true
    );

    const [isEditing, setIsEditing] = useState(false);
    const [feedback, setFeedback] = useState(null);
    const [errorFeedback, setErrorFeedback] = useState(null);


    const cancelAction = () => {
        onChangeAction();
        setIsEditing(false);

        resetForm(RESET_MODE.INIT);
    }

    const edit = () => {
        onChangeAction();
        setIsEditing(true);
    }

    const afterSaveAction = (newUsername, feedback) => {
        setIsEditing(false);
        setErrorFeedback(null);
        setFeedback(feedback);

        saveUser({
            ...user,
            username: newUsername
        });
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
                        formData={fields.username}
                        regexStatus={valid.username}
                        formFocus={isEditing}
                        isDisabled={!isEditing}
                        handleChange={handleFieldChange}
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