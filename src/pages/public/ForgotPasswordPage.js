import React, { useState, useEffect, useContext, useRef } from "react";
import { Link, useLocation } from 'react-router-dom';
import axios from "../../api/axios";

import APIs from "../../api/ApiURL";
import ValidatedInput from "../../components/form/ValidatedInput";
import regexUtils from "../../shared/utils/regexUtils";
import PopUp from "../../components/PopUp";

import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { faUnlock } from "@fortawesome/free-solid-svg-icons";

const ForgotPasswordPage = () => {
    const popUpRef = useRef();
    const [email, setEmail] = useState("");
    const [emailRegex, setEmailRegex] = useState(false);
    const [emailFocus, setEmailFocus] = useState(false);

    const submitForm = async (event) => {
        event.preventDefault();

        if (!email) {
            popUpRef.current?.show("Email can't be empty.");
            return;
        }
        else if (!emailRegex) {
            popUpRef.current?.show("Email does not meet the criteria.");
            return;
        }

        try {
            const data = await axios.post(APIs.REQUEST_PASSWORD_RESET,
                JSON.stringify({
                    email: email
                }),
                { headers: { 'Content-Type': 'application/json' } }
            );

            if (data.status === 200) {
                popUpRef.current?.show("Password reset email sent successfully.");
            }
        } catch (err) {
            popUpRef.current?.show(err.response?.data.detail || err.message);
        }

        setEmail("");
        setEmailRegex(false);
    }

    const handleChange = (e) => {
        popUpRef.current?.hide();

        setEmail(e.target.value);
        setEmailRegex(regexUtils.EMAIL.test(e.target.value));
    };

    const handleFocusOn = (e) => {
        setEmailFocus(true);
    };

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
                        formData={email}
                        regexStatus={emailRegex}
                        formFocus={emailFocus}
                        handleChange={handleChange}
                        handleFocusOn={handleFocusOn}
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

                <Link to="/help" className="btn2 goBack">Back to help page</Link>
            </form>

        </div>
    )
}

export default ForgotPasswordPage