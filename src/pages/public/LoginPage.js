import React, { useState, useEffect, useContext, useRef } from "react";
import { Link, Navigate, useNavigate } from 'react-router-dom';
import PopUp from "../../components/PopUp";
import axios from "../../api/axios";

import { AuthContext } from "../../context/AuthProvider";
import APIs from "../../api/ApiURL";



const LoginPage = () => {
    const navigate = useNavigate();
    const popUpRef = useRef();
    const { username, userId, role, setAuth } = useContext(AuthContext);
    const [formState, setFormState] = useState({
        username: "",
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

        if (formState.username === "" || formState.password === "") {
            popUpRef.current?.show("Nazwa użytkownika lub hasło nie może być puste");
            return;
        }

        try {
            const response = await axios.post(APIs.LOGIN,
                JSON.stringify({
                    UserName: formState.username,
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

                setAuth(userData.userName, userData.id, role, userData.accessToken);

                // refreshToken
                sessionStorage.setItem('refreshToken', userData.refreshToken);

                // user info
                let userInfo = {
                    username: userData.userName,
                    userID: userData.id,
                    role: role,
                    currentChat: ''
                };
                sessionStorage.setItem('userInfo', JSON.stringify(userInfo));

                navigate("/message");
            }

        } catch (err) {
            console.log(err);
            let mess = err.response?.data.detail || "Invalid login request";
            popUpRef.current?.show(mess);
        }

        setFormState({ username: "", password: "" });
    };


    return (
        <div id="mainloginPage">
            <form className="loginPanel">
                <label htmlFor="username">Login: </label>
                <input
                    value={formState.username}
                    onChange={handleChange}
                    name="username"
                    id="username"
                    autoComplete="off"
                    type="text"
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
                <Link to="/comment">Comment Page</Link>
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
