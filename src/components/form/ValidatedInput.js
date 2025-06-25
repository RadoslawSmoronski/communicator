import React from 'react'
import ValidationBox from './ValidationBox'

const ValidatedInput = ({
  htmlName,
  labelText,
  formData,
  regexStatus,
  formFocus,
  validationText,
  handleChange,
  handleFocusOn,
  inputType
}) => {
  return (<>
    <label htmlFor={htmlName} className={formData ? (regexStatus ? "correctValidation" : "wrongValidation") : ""}>
      {labelText}:
    </label>
    <input
      name={htmlName}
      id={htmlName}
      className="textInput"
      autoComplete="off"
      type={inputType}
      value={formData}
      onChange={handleChange}
      onFocus={handleFocusOn}
    /><br />
    <ValidationBox
      regex={regexStatus}
      value={formData}
      focus={formFocus}
      text={validationText}
    />
  </>
  )
}

export default ValidatedInput