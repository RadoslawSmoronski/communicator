import React, { useContext, useRef } from "react";
import { Link, useNavigate } from 'react-router-dom';

import PopUp from "../../shared/components/PopUp";
import NonValidatedInput from "../../features/auth/components/NonValidatedInput";

import { AuthContext } from "../../app/providers/AuthProvider";
import { ROUTES } from "../../app/router/routePaths";

import { useNonValidatedForm } from "../../shared/hooks/forms/useNonValidatedForm";
import { useUserData } from "../../features/auth/hooks/useUserData";
import { submitLoginService } from "../../features/auth/services/loginService";


const LoginPage = () => {
    const navigate = useNavigate();
    const popUpRef = useRef();
    const { setAccessToken } = useContext(AuthContext);
    const [formData, handleChange, resetForm] = useNonValidatedForm(
        { email: "", password: "" },
        () => popUpRef.current?.hide()
    );
    const { save: saveUserData } = useUserData(setAccessToken);


    const submitLogin = async (event) => {
        event.preventDefault();

        const result = await submitLoginService(formData)

        resetForm();

        if (result.errorMessage) {
            popUpRef.current?.show(result.errorMessage);
            return;
        }

        // save user data
        saveUserData(result.data);

        // redirect
        navigate("/message");
    };


    return (
        <div id="mainloginPage">
            <form className="loginPanel">
                <NonValidatedInput
                    htmlName="email"
                    labelText="Email"
                    formData={formData.email}
                    handleChange={handleChange}
                    inputType="email"
                />
                <br /><br />
                <NonValidatedInput
                    htmlName="password"
                    labelText="Password"
                    formData={formData.password}
                    handleChange={handleChange}
                    inputType="password"
                /><br />

                <PopUp ref={popUpRef} />

                <button className="btn" onClick={submitLogin}>Login</button><br /><br />
                <div>Don't have an account? Sign up below</div>
                <Link to={ROUTES.REGISTER}>Create an account</Link>
                <Link to={ROUTES.MESSAGE}>Message Page</Link>

                <br />
                <div>Having trouble logging in?</div>
                <Link to={ROUTES.HELP}>Troubleshooting center</Link>
            </form>

            <div className="welcomeBlock">
                <div className="logoAndText">
                    <div id="logo" />
                    <div className="bottomText"><div className="highlightText">Lorem ipsum dolor sit amet.</div> consectetur adipiscing elit. Nam egestas arcu quis ex vehicula facilisis. Sed maximus nunc vitae tincidunt porttitor. Phasellus congue imperdiet vestibulum. Phasellus a diam iaculis urna condimentum dictum vel in nulla. Proin velit velit, aliquam sed consectetur vitae, porttitor eget turpis. Cras sollicitudin eros eget libero tempor, quis posuere justo laoreet. </div>
                </div>
            </div>

            <div id="welcomeBlockSmall">
                <div id="logo" />
            </div>
        </div>
    );
};

export default LoginPage;
