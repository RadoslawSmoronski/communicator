import React from "react";

import ValidatedInput from "../../../../shared/components/form/ValidatedInput";
import { useValidatedForm } from "../../../../shared/hooks/forms/useValidatedForm";
import useEditableState from "../../hooks/useEditableState";
import useChangePassword from "../../hooks/useChangePassword";

import regexUtils from "../../../../shared/utils/regexUtils";
import FeedbackText from "../../../../components/form/FeedbackText";

const EditPassword = () => {
    const {
        changePassword,
        error, setError,
        feedback, setFeedback,
        loading
    } = useChangePassword();

    const initForm = {
        oldpass: "",
        newpass: "",
        newpass2: ""
    }

    const validator = {
        newpass: regexUtils.PASSWORD,
        newpass2: (value, fields) => fields && value === fields.newpass && regexUtils.PASSWORD.test(value)
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

    const onChangeAction = () => {
        setError(null);
        setFeedback(null);
    }

    const { isEditing, edit, cancel, onSuccess } = useEditableState({
        onChangeAction,
        cancelCallback: () => resetForm(RESET_MODE.CLEAR),
        onSuccessCallback: (data) => resetForm(RESET_MODE.CLEAR)
    });

    const saveChanges = () => changePassword(fields, valid, onSuccess);

    return (
        <>
            <div className="editProfileTitle">Password</div>
            <div className="editProfileElement">
                <div className="editPanelInputWrapper">
                    <ValidatedInput
                        htmlName="oldpass"
                        labelText="Old password"
                        formData={fields.oldpass}
                        regexStatus={true}
                        formFocus={focus.oldpass}
                        isDisabled={!isEditing}
                        handleChange={handleFieldChange}
                        handleFocusOn={handleFieldFocus}
                        inputType="password"
                        addClassName="editProfile"
                        validationText={<></>}
                    />
                    <ValidatedInput
                        htmlName="newpass"
                        labelText="New password"
                        formData={fields.newpass}
                        regexStatus={valid.newpass}
                        formFocus={focus.newpass}
                        isDisabled={!isEditing}
                        handleChange={handleFieldChange}
                        handleFocusOn={handleFieldFocus}
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
                        formData={fields.newpass2}
                        regexStatus={valid.newpass2}
                        formFocus={focus.newpass2}
                        isDisabled={!isEditing}
                        handleChange={handleFieldChange}
                        handleFocusOn={handleFieldFocus}
                        inputType="password"
                        addClassName="editProfile"
                        validationText={
                            <>
                                Passwords have to match<br />
                                And meet the criteria.
                            </>}
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

export default EditPassword