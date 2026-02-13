import React, { useRef } from "react";
import { Link } from 'react-router-dom';

import ValidatedInput from "../../shared/components/form/ValidatedInput"
import regexUtils from "../../shared/utils/regexUtils";
import PopUp from "../../shared/components/PopUp";

import { ROUTES } from "../../app/router/routePaths";
import { useValidatedForm } from "../../shared/hooks/forms/useValidatedForm";
import { registerService } from "../../features/users/services/registerService";


const RegisterPage = () => {
  const popUpRef = useRef();

  const initForm = {
    email: "",
    username: "",
    password: "",
    password2: "",
  };
  const validator = {
    email: regexUtils.EMAIL,
    username: regexUtils.USERNAME,
    password: regexUtils.PASSWORD,
    password2: (value, fields) => fields && value === fields.password && regexUtils.PASSWORD.test(value)
  };

  const {
    fields,
    valid,
    focus,
    handleFieldChange,
    handleFieldFocus,
    resetForm,
    setFields
  } = useValidatedForm(
    initForm, validator, () => popUpRef.current?.hide(), true
  );

  const submitRegister = async (e) => {
    e.preventDefault();

    const result = await registerService(fields, valid);

    if (result?.resetPasswordFields) {
      setFields(prev => ({
        ...prev,
        password: "",
        password2: ""
      }))
    } else if (result?.resetForm) {
      resetForm();
    }

    popUpRef.current?.show(result.message);
  };


  return (
    <div id="mainregisterPage">
      <form className="loginPanel">
        <ValidatedInput
          htmlName="username"
          labelText="Username"
          formData={fields.username}
          regexStatus={valid.username}
          formFocus={focus.username}
          handleChange={handleFieldChange}
          handleFocusOn={handleFieldFocus}
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

        <PopUp ref={popUpRef} />
        <button className="btn" onClick={submitRegister}>Register</button><br /><br />
        <div>Already have an account? Log in below</div>
        <Link to={ROUTES.LOGIN}>Log in</Link>
      </form>
      <div id="logo" />
    </div>
  );
};

export default RegisterPage;
