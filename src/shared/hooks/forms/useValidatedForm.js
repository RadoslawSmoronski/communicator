import { useState, useRef } from "react";

export function useValidatedForm(
    initialState, validators = {}, onChangeCallback, validateAllFields= false 
) {
    // for reset - return to init state
    const initialRef = useRef(initialState);
    
    const [fields, setFields] = useState(initialState);

    // regex validation state
    const [valid, setValid] = useState(
        Object.fromEntries(Object.keys(initialState).map(k => [k, false]))
    );

    // focus state
    const [focus, setFocus] = useState(
        Object.fromEntries(Object.keys(initialState).map(k => [k, false]))
    );


    const handleFieldChange = (e) => {
        const { name, value } = e.target;
        // pop up callback - hide
        onChangeCallback?.(); 

        setFields(prev => ({
            ...prev,
            [name]: value
        }));

        const updatedFields = {
            ...fields,
            [name]: value
        };


        const validateSingleField = (fieldName) => {
            const validator = validators[fieldName];
            if (!validator) return null;

            const fieldValue = updatedFields[fieldName];

            if (validator instanceof RegExp) {
                return validator.test(fieldValue);
            } else if (typeof validator === "function") {
                return validator(fieldValue, updatedFields);
            }

            return null;
        };

        let updatedValid = { ...valid };

        
        if (validateAllFields) {
            // validate all fields
            Object.keys(validators).forEach(fieldName => {
                updatedValid[fieldName] = validateSingleField(fieldName);
            });
        } else {
            // validate single field
            updatedValid[name] = validateSingleField(name);
        }

        setValid(updatedValid);
    };

    const handleFieldFocus = (e) => {
        const { name } = e.target;
        setFocus(prev => ({
            ...prev,
            [name]: true
        }));
    };

    const resetForm = () => {
        const emptyState = initialRef.current;

        setFields(emptyState);

        setValid(
            Object.fromEntries(Object.keys(emptyState).map(k => [k, false]))
        );

        setFocus(
            Object.fromEntries(Object.keys(emptyState).map(k => [k, false]))
        );
    };

    return {
        fields,
        valid,
        focus,
        handleFieldChange,
        handleFieldFocus,
        resetForm
    };
}
