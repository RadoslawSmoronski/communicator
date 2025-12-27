import React, { useState, useEffect, useRef } from "react";
import { Link, useNavigate } from 'react-router-dom';

import PopUp from "../../components/PopUp";
import ValidatedInput from "../../shared/components/form/ValidatedInput";

import { ROUTES } from "../../app/router/routePaths";
import regexUtils from "../../shared/utils/regexUtils";

import useRawQueryParam from "../../shared/hooks/useRawQueryParam";
import { useValidatedForm } from "../../shared/hooks/forms/useValidatedForm";
import { resetPasswordService } from "../../features/auth/services/resetPasswordService";

import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { faUnlock } from "@fortawesome/free-solid-svg-icons";

const ResetPasswordPage = () => {
    const navigate = useNavigate();
    const popUpRef = useRef();

    const userId = useRawQueryParam("userId");
    const token = useRawQueryParam("token");

    const [success, setSuccess] = useState(false);

    const initForm = { password: "", password2: "" };
    const validator = {
        password: regexUtils.PASSWORD,
        password2: (value, fields) => fields && value === fields.password && regexUtils.PASSWORD.test(value)
    }

    const {
        fields,
        valid,
        focus,
        handleFieldChange,
        handleFieldFocus,
        resetForm
    } = useValidatedForm(
        initForm, validator, () => popUpRef.current?.hide(), true
    );

    const resetPassword = async (e) => {
        e.preventDefault();

        const result = await resetPasswordService(
            fields, valid, userId, token
        );

        if (result?.errorMessage) {
            popUpRef.current?.show(result.errorMessage);

        } else if (result?.success) {
            setSuccess(true);
        }

        if (result?.resetForm) {
            resetForm();
        }
    }

    useEffect(() => {
        if (!userId || !token) {
            navigate('/login');
        }
    }, []);

    return (
        <div id="mainregisterPage" style={{ justifyContent: 'right' }}>
            <div id="logo" className="confirmAccount" />
            <form className="loginPanel small">
                {
                    !success ?
                        <>
                            <div className="loggingHelp mainText">
                                <FontAwesomeIcon icon={faUnlock} /> Please type new password
                            </div>

                            <div className="loggingHelp formWrapper">
                                <ValidatedInput
                                    htmlName="password"
                                    labelText="Password"
                                    formData={fields.password}
                                    regexStatus={valid.password}
                                    formFocus={focus.password}
                                    handleChange={handleFieldChange}
                                    handleFocusOn={handleFieldFocus}
                                    inputType="password"
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
                                    htmlName="password2"
                                    labelText="Repeat password"
                                    formData={fields.password2}
                                    regexStatus={valid.password2}
                                    formFocus={focus.password2}
                                    handleChange={handleFieldChange}
                                    handleFocusOn={handleFieldFocus}
                                    inputType="password"
                                    validationText={
                                        <>
                                            Passwords have to match<br />
                                            And meet the criteria.
                                        </>}
                                />

                                <button className="btn" onClick={resetPassword}>Reset Password</button>

                            </div>
                            <PopUp ref={popUpRef} />
                        </>
                        :
                        <>
                            <div className="confirmAccount mainText">Password reset successfully.</div>
                            <Link to={ROUTES.LOGIN} className="btn2 goBack">Back to login page</Link>
                        </>
                }

            </form>

        </div>
    )
}

export default ResetPasswordPage