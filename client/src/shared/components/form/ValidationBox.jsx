import React from 'react';
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { faInfoCircle } from "@fortawesome/free-solid-svg-icons";

const ValidationBox = ({ regex, value, focus, text }) => {
  return (
    <>
      {!regex && value && focus && (
        <div className="validationBox">
          <FontAwesomeIcon icon={faInfoCircle} /><br />
          {text}
        </div>
      )}
    </>
  );
};

export default ValidationBox;
