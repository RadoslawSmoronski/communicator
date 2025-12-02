import { useState, useRef } from "react";

export function useValidatedForm(initialState, validators = {}, onChangeCallback) {
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

        // validation
        if (validators[name]) {
            const validator = validators[name];

            let isValid = false;

            if (validator instanceof RegExp) {
                isValid = validator.test(value);
            } 
            else if (typeof validator === "function") {
                const result = validator(value);
                isValid = result === true;
            }

            setValid(prev => ({
                ...prev,
                [name]: isValid
            }));
        }
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
        setFields,
        setValid,
        setFocus,
        resetForm
    };
}
