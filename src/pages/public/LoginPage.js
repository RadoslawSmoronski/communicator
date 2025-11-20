import React, { useState, useEffect, useContext, useRef } from "react";
import { Link, Navigate, useNavigate } from 'react-router-dom';
import PopUp from "../../components/PopUp";
import axios from "../../api/axios";

import { AuthContext } from "../../context/AuthProvider";
import APIs from "../../api/ApiURL";



const LoginPage = () => {
    const navigate = useNavigate();
    const popUpRef = useRef();
    const { setAuth } = useContext(AuthContext);
    const [formState, setFormState] = useState({
        email: "",
        password: ""
    });


    const handleChange = (event) => {
        popUpRef.current?.hide();

        const { name, value } = event.target;
        setFormState(prev => ({
            ...prev,
            [name]: value
        }));
    };

    const submitLogin = async (event) => {
        event.preventDefault();

        if (formState.email === "" || formState.password === "") {
            popUpRef.current?.show("Nazwa użytkownika lub hasło nie może być puste");
            return;
        }

        try {
            const response = await axios.post(APIs.LOGIN,
                JSON.stringify({
                    Email: formState.email,
                    Password: formState.password
                }),
                {
                    headers: {
                        'Content-Type': 'application/json'
                    },
                    withCredentials: true
                }
            );

            const userData = response.data;

            console.log(userData);

            if (response.status === 200) {

                console.log("userData: ", userData);
                let role = 'user';

                setAuth(userData.avatarUrl, formState.email, userData.username, userData.id, role, userData.accessToken);

                // refreshToken
                sessionStorage.setItem('refreshToken', userData.refreshToken);

                // user info
                let userInfo = {
                    avatarUrl: userData.avatarUrl,
                    email: formState.email,
                    username: userData.username,
                    userID: userData.id,
                    role: role,
                    currentChat: ''
                };
                sessionStorage.setItem('userInfo', JSON.stringify(userInfo));

                navigate("/message");
            }

        } catch (err) {
            console.log(err);
            let mess = err.response?.data.title || "Invalid login request";
            popUpRef.current?.show(mess);
        }

        setFormState({ email: "", password: "" });
    };


    return (
        <div id="mainloginPage">
            <form className="loginPanel">
                <label htmlFor="email">Email: </label>
                <input
                    value={formState.email}
                    onChange={handleChange}
                    name="email"
                    id="email"
                    autoComplete="off"
                    type="email"
                    className="textInput"
                /><br /><br />
                <label htmlFor="password">Password: </label>
                <input
                    value={formState.password}
                    onChange={handleChange}
                    name="password"
                    id="password"
                    type="password"
                    className="textInput"
                /><br />

                <PopUp ref={popUpRef} />

                <button className="btn" onClick={submitLogin}>Login</button><br /><br />
                <div>Don't have an account? Sign up below</div>
                <Link to="/register">Create an account</Link>
                <Link to="/message">Message Page</Link>

                <br />
                <div>Having trouble logging in?</div>
                <Link to="/help">Troubleshooting center</Link>
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
