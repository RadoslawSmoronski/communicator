import React, { useState, useEffect, useContext, useRef } from "react";
import { Link, useLocation, useNavigate } from 'react-router-dom';

import PopUp from "../../components/PopUp";
import ValidatedInput from "../../components/form/ValidatedInput";
import axios from "../../api/axios";
import APIs from "../../api/ApiURL";
import regexUtils from "../../shared/utils/regexUtils";
import useRawQueryParam from "../../shared/hooks/useRawQueryParam";

import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { faUnlock } from "@fortawesome/free-solid-svg-icons";

const ResetPasswordPage = () => {
    const navigate = useNavigate();
    const location = useLocation();
    const popUpRef = useRef();

    const queryParams = new URLSearchParams(location.search);
    const userId = useRawQueryParam("userId");
    const token = useRawQueryParam("token");

    const [isSuccess, setIsSuccess] = useState(false);

    const [formData, setFormData] = useState({
        password: "",
        password2: "",
    });

    const [regexStatus, setRegexStatus] = useState({
        password: false,
        password2: false,
    });

    const [formFocus, setFormFocus] = useState({
        password: false,
        password2: false,
    });

    const handleChange = (e) => {
        popUpRef.current?.hide();
        const { name, value } = e.target;

        setFormData(prev => ({
            ...prev,
            [name]: value
        }));

        if (name === "password") {
            setRegexStatus((prev) => ({
                ...prev,
                password: regexUtils.PASSWORD.test(value),
                password2: formData.password2 === value,
            }));
        } else if (name === "password2") {
            setRegexStatus((prev) => ({
                ...prev,
                password2: formData.password === value,
            }));
        }
    };

    const handleFocusOn = (e) => {
        const { name } = e.target;
        setFormFocus({ password: false, password2: false });
        setFormFocus((prev) => ({ ...prev, [name]: true }));
    };

    const resetPassword = async (e) => {
        e.preventDefault();

        const { password, password2 } = formData;
        const { password: passRegex, password2: pass2Regex } = regexStatus;

        if (!password || !password2) {
            popUpRef.current?.show("Password cannot be empty");
            return;
        } else if (!passRegex) {
            popUpRef.current?.show("Password does not meet the criteria.");
            return;
        } else if (!pass2Regex) {
            popUpRef.current?.show("The given passwords are different!");
            setFormData((prev) => ({ ...prev, password: "", password2: "" }));
            return;
        }

        try {
            const data = await axios.post(APIs.RESET_PASSWORD,
                JSON.stringify({
                    userId: userId,
                    codedToken: token,
                    newPassword: password
                }),
                { headers: { 'Content-Type': 'application/json' } }
            );

            if (data.status === 200) {
                setIsSuccess(true);
            }
        } catch (err) {
            popUpRef.current?.show(err.response?.data.detail || err.message);
        }

        setFormData({ password: "", password2: "" });
        setRegexStatus({ password: false, password2: false });
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
                    !isSuccess ?
                        <>
                            <div className="loggingHelp mainText">
                                <FontAwesomeIcon icon={faUnlock} /> Please type new password
                            </div>

                            <div className="loggingHelp formWrapper">
                                <ValidatedInput
                                    htmlName="password"
                                    labelText="Password"
                                    formData={formData.password}
                                    regexStatus={regexStatus.password}
                                    formFocus={formFocus.password}
                                    handleChange={handleChange}
                                    handleFocusOn={handleFocusOn}
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
                                    formData={formData.password2}
                                    regexStatus={regexStatus.password2}
                                    formFocus={formFocus.password2}
                                    handleChange={handleChange}
                                    handleFocusOn={handleFocusOn}
                                    inputType="password"
                                    validationText={<>Passwords have to match</>}
                                />

                                <button className="btn" onClick={resetPassword}>Reset Password</button>

                            </div>
                            <PopUp ref={popUpRef} />
                        </>
                        :
                        <>
                            <div className="confirmAccount mainText">Password reset successfully.</div>
                            <Link to="/login" className="btn2 goBack">Back to login page</Link>
                        </>
                }

            </form>

        </div>
    )
}

export default ResetPasswordPage