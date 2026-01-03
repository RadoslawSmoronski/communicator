import React, { useState, useEffect, useContext } from "react";
import { UserContext } from "../../../../app/providers/UserProvider";

import ValidatedInput from "../../../../shared/components/form/ValidatedInput"
import { useValidatedForm } from "../../../../shared/hooks/forms/useValidatedForm";
import useChangeUsername from "../../hooks/useChangeUsername";
import useEditableState from "../../hooks/useEditableState";

import regexUtils from "../../../../shared/utils/regexUtils";
import FeedbackText from "../../../../components/form/FeedbackText";

const EditUsername = () => {
    const { user, saveUsername } = useContext(UserContext);

    const {
        changeUsername,
        error, setError,
        feedback, setFeedback,
        loading } = useChangeUsername();

    const initForm = { username: user.username }
    const validator = { username: regexUtils.USERNAME }

    const {
        fields,
        valid,
        handleFieldChange,
        resetForm,
        setInitState,
        RESET_MODE
    } = useValidatedForm(
        initForm, validator, onChangeAction, true, true
    );

    const onChangeAction = () => {
        setError(null);
        setFeedback(null);
    }

    const { isEditing, edit, cancel, onSuccess } = useEditableState({
        onChangeAction,
        cancelCallback: () => resetForm(RESET_MODE.INIT),
        onSuccessCallback: (data) => {
            // init new form data
            setInitState({ username: data.newUsername });
            // save username
            saveUsername(data.newUsername);
        }
    });

    const saveChanges = () => changeUsername(fields.username, valid.username, onSuccess);

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
                            <button className='editPanelBtn default' onClick={cancel}>Cancel</button>
                        </>
                    ) : (
                        <button className='editPanelBtn default' onClick={edit}>Edit</button>
                    )}
                </div>
            </div>
            <FeedbackText successText={feedback} failText={error} />
        </>
    )
}

export default EditUsername