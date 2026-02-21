import React, { useState, useEffect, useContext, useRef } from "react";
import { Link, useLocation } from 'react-router-dom';

import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { faKitMedical } from "@fortawesome/free-solid-svg-icons";

const LoggingHelpPage = () => {
  return (
    <div id="mainFormPage">
      <div id="logo" className="confirmAccount" />
      <div className="loggingHelpBody">
        <div className="loggingHelp mainText">
          <FontAwesomeIcon icon={faKitMedical} /> Logging In help center
        </div>

        <div className="loggingHelp btnWrapper">
          <Link to="/forgotpassword" className="btn2 link">I forgot my password</Link>
          <Link to="/sendnewemail" className="btn2 link">I didn't receive an email confirming my account registration</Link>
        </div>

        <Link to="/login" className="btn2 btn404">Back to login page</Link>
      </div>
    </div>
  )
}

export default LoggingHelpPage