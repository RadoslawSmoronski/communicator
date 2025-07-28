import React, { useState, useContext, useRef } from "react";
import { Link } from 'react-router-dom';


import PopUp from "../../components/PopUp";
import ValidatedInput from "../../components/form/ValidatedInput";
import axios from "../../api/axios";
import APIs from "../../api/ApiURL";
import regexUtils from "../../utils/regexUtils";

const RegisterPage = () => {
  const popUpRef = useRef();

  const [formData, setFormData] = useState({
    email: "",
    username: "",
    password: "",
    password2: "",
  });

  const [regexStatus, setRegexStatus] = useState({
    email: false,
    username: false,
    password: false,
    password2: false,
  });

  const [formFocus, setFormFocus] = useState({
    email: false,
    username: false,
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

    if (name === "username") {
      setRegexStatus((prev) => ({ ...prev, username: regexUtils.USERNAME.test(value) }));
    } else if (name === "email") {
      setRegexStatus((prev) => ({ ...prev, email: regexUtils.EMAIL.test(value) }));
    } else if (name === "password") {
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
    setFormFocus({ email: false, username: false, password: false, password2: false });
    setFormFocus((prev) => ({ ...prev, [name]: true }));
  };


  const submitRegister = async (e) => {
    e.preventDefault();

    const { email, username, password, password2 } = formData;
    const { email: emailRegex, username: usernameRegex, password: passRegex, password2: pass2Regex } = regexStatus;

    if (!username || !password || !password2 || !email) {
      popUpRef.current?.show("Nazwa użytkownika, email lub hasło nie może być puste");
      return;
    }

    if (!usernameRegex || !passRegex || !emailRegex) {
      popUpRef.current?.show("Nazwa użytkownika, email lub hasło nie spełniają kryteriów");
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
          email: email,
          username: username,
          password: password
        }),
        { headers: { 'Content-Type': 'application/json' } }
      );

      if (data.status === 201) {
        popUpRef.current?.show("User successfully created.");
      }
    } catch (err) {
      popUpRef.current?.show(err.response?.data.detail || err.message);
    }

    setFormData({ email: "", username: "", password: "", password2: "" });
    setRegexStatus({ email: false, username: false, password: false, password2: false });
  };


  return (
    <div id="mainregisterPage">
      <form className="loginPanel">
        <ValidatedInput
          htmlName="username"
          labelText="Username"
          formData={formData.username}
          regexStatus={regexStatus.username}
          formFocus={formFocus.username}
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
          htmlName="email"
          labelText="Email"
          formData={formData.email}
          regexStatus={regexStatus.email}
          formFocus={formFocus.email}
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
      <div id="logo" />
    </div>
  );
};

export default RegisterPage;
