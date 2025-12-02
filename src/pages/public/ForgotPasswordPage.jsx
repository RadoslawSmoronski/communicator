import React, { useState, useEffect, useContext, useRef } from "react";
import { Link, useLocation } from 'react-router-dom';

import { ROUTES } from "../../app/router/routePaths"
import ValidatedInput from "../../components/form/ValidatedInput";
import { useValidatedForm } from "../../shared/hooks/forms/useValidatedForm";
import { requestPasswordResetService } from "../../features/auth/services/requestPasswordResetService";

import regexUtils from "../../shared/utils/regexUtils";
import PopUp from "../../components/PopUp";

import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { faUnlock } from "@fortawesome/free-solid-svg-icons";

const ForgotPasswordPage = () => {
    const popUpRef = useRef();
    const initForm = { email: "" };
    const validator = { email: regexUtils.EMAIL };

    const {
        fields,
        valid,
        focus,
        handleFieldChange,
        handleFieldFocus,
        resetForm
    } = useValidatedForm(
        initForm, validator, () => popUpRef.current?.hide()
    );

    const submitForm = async (event) => {
        event.preventDefault();

        const result = await requestPasswordResetService(
            fields.email, valid.email
        );
        if(result){
            popUpRef.current?.show(result.message);
        }

        resetForm();
    }

    return (
        <div id="mainregisterPage" style={{ justifyContent: 'right' }}>
            <div id="logo" className="confirmAccount" />
            <form className="loginPanel small">
                <div className="loggingHelp mainText">
                    <FontAwesomeIcon icon={faUnlock} /> Please type your email to reset password
                </div>

                <div className="loggingHelp formWrapper">
                    <ValidatedInput
                        htmlName="email"
                        labelText="Email"
                        formData={fields.email}
                        regexStatus={valid.email}
                        formFocus={focus.email}
                        handleChange={handleFieldChange}
                        handleFocusOn={handleFieldFocus}
                        inputType="email"
                        validationText={
                            <>
                                Must be a valid email format<br />
                                Must contain "@" and a domain name<br />
                                No spaces or special characters outside local part
                            </>
                        }
                    />

                    <button className="btn" onClick={submitForm}>Send</button>

                </div>
                <PopUp ref={popUpRef} />

                <Link to={ROUTES.HELP} className="btn2 goBack">Back to help page</Link>
            </form>

        </div>
    )
}

export default ForgotPasswordPage