import React, { useState, useContext, useRef } from "react";
import { Link } from 'react-router-dom';


import PopUp from "../../components/PopUp";
import ValidatedInput from "../../components/form/ValidatedInput";
import axios from "../../api/axios";
import APIs from "../../api/ApiURL";

const USER_REGEX = /^[a-zA-Z][a-zA-Z0-9-_#]{4,24}$/;
const PASS_REGEX = /^(?=.*?[A-Z])(?=.*?[a-z])(?=.*?[0-9])(?=.*?[#?!@$%^&*-]).{8,64}$/;

const RegisterPage = () => {
  const popUpRef = useRef();

  const [formData, setFormData] = useState({
    login: "",
    password: "",
    password2: "",
  });

  const [regexStatus, setRegexStatus] = useState({
    login: false,
    password: false,
    password2: false,
  });

  const [formFocus, setFormFocus] = useState({
    login: false,
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

    if (name === "login") {
      setRegexStatus((prev) => ({ ...prev, login: USER_REGEX.test(value) }));
    } else if (name === "password") {
      setRegexStatus((prev) => ({
        ...prev,
        password: PASS_REGEX.test(value),
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
    setFormFocus({ login: false, password: false, password2: false });
    setFormFocus((prev) => ({ ...prev, [name]: true }));
  };


  const submitRegister = async (e) => {
    e.preventDefault();

    const { login, password, password2 } = formData;
    const { login: loginRegex, password: passRegex, password2: pass2Regex } = regexStatus;

    if (!login || !password || !password2) {
      popUpRef.current?.show("Nazwa użytkownika lub hasło nie może być puste");
      return;
    }

    if (!loginRegex || !passRegex) {
      popUpRef.current?.show("Nazwa użytkownika lub hasło nie spełniają kryteriów");
      return;
    }

    if (!pass2Regex) {
      popUpRef.current?.show("Podane hasła są różne!");
      setFormData((prev) => ({ ...prev, password: "", password2: "" }));
      return;
    }

    try {
      const data = await axios.post(APIs.REGISTER,
        JSON.stringify({
          UserName: login,
          Password: password
        }),
        { headers: { 'Content-Type': 'application/json' } }
      );

      if (data.status === 201) {
        popUpRef.current?.show("User successfully created.");
      }
    } catch (err) {
      popUpRef.current?.show(err.response?.data.detail || err.message);
    }

    setFormData({ login: "", password: "", password2: "" });
    setRegexStatus({ login: false, password: false, password2: false });
  };


  return (
    <div id="mainregisterPage">
      <form className="loginPanel">
        <ValidatedInput
          htmlName="login"
          labelText="Login"
          formData={formData.login}
          regexStatus={regexStatus.login}
          formFocus={formFocus.login}
          handleChange={handleChange}
          handleFocusOn={handleFocusOn}
          inputType="text"
          validationText={
            <>
              Has 5 - 24 characters in length<br />
              Has to start with English letter<br />
              Can contain English letters, digits and -_#
            </>
          }
        />

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

        <PopUp ref={popUpRef} />
        <button className="btn" onClick={submitRegister}>Register</button><br /><br />
        <div>Already have an account? Log in below</div>
        <Link to="/login">Log in</Link>
      </form>
      <div id="logo"/>
    </div>
  );
};

export default RegisterPage;
