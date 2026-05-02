import React from 'react'

const NonValidatedInput = ({
    htmlName,
    labelText,
    formData,
    handleChange,
    inputType
}) => {
  return (
    <>
        <label htmlFor={htmlName}>
            {labelText}: 
        </label>
        <input
            name={htmlName}
            id={htmlName}
            className="textInput"
            type={inputType}
            autoComplete="off"
            value={formData}
            onChange={handleChange}
        />
    </>
  )
}

export default NonValidatedInput