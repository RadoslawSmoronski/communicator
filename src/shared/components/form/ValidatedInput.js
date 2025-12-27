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
  inputType,
  isDisabled,
  addClassName = ""
}) => {
  return (<>
    <label htmlFor={htmlName} className={formData && !isDisabled ? (regexStatus ? "correctValidation" : "wrongValidation") : ""}>
      {labelText}:
    </label>
    <input
      name={htmlName}
      id={htmlName}
      className={"textInput" + " " + addClassName}
      autoComplete="off"
      type={inputType}
      value={formData}
      onChange={handleChange}
      onFocus={handleFocusOn}
      disabled={isDisabled}
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