import { useState, useRef, useEffect } from "react";

export function useValidatedForm(
    initialState, validators = {}, onChangeCallback, validateAllFields = false, validateOnInit = false
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

    const setInitState = (newInitState) => {
        initialRef.current = newInitState;

        // update fields and valid
        setFields(newInitState);
        if (validateOnInit) {
            setValid(prev => ({
                ...prev,
                ...validateFields(newInitState)
            }));
        }
    };

    // validate action
    const validateField = (fieldName, fieldsState) => {
        const validator = validators[fieldName];
        if (!validator) return null;

        const value = fieldsState[fieldName];

        if (validator instanceof RegExp) {
            return validator.test(value);
        }

        if (typeof validator === "function") {
            return validator(value, fieldsState);
        }

        return null;
    };

    const validateFields = (fieldsState) => {
        const result = {};

        Object.keys(validators).forEach(field => {
            result[field] = validateField(field, fieldsState);
        });

        return result;
    };

    // init validation
    useEffect(() => {
        if (!validateOnInit) return;

        setValid(prev => ({
            ...prev,
            ...validateFields(initialState)
        }));
    }, []);

    const handleFieldChange = (e) => {
        const { name, value } = e.target;
        onChangeCallback?.();

        setFields(prev => {
            const updated = { ...prev, [name]: value };

            setValid(prevValid =>
                validateAllFields
                    ? { ...prevValid, ...validateFields(updated) } // validate all fields
                    : { ...prevValid, [name]: validateField(name, updated) } // validate field
            );

            return updated;
        });
    };

    const handleFieldFocus = (e) => {
        const { name } = e.target;

        setFocus(prev => {
            const reset = Object.fromEntries(
                Object.keys(prev).map(key => [key, false])
            );

            return {
                ...reset,
                [name]: true
            };
        });
    };

    const resetForm = (mode = RESET_MODE.CLEAR) => {
        const emptyState = initialRef.current;

        setFields(emptyState);

        setFocus(
            Object.fromEntries(Object.keys(emptyState).map(k => [k, false]))
        );

        if (mode === RESET_MODE.INIT && validateOnInit) {
            setValid(prev => ({
                ...prev,
                ...validateFields(emptyState)
            }));
        } else {
            setValid(
                Object.fromEntries(Object.keys(emptyState).map(k => [k, false]))
            );
        }
    };

    return {
        fields,
        valid,
        focus,
        handleFieldChange,
        handleFieldFocus,
        resetForm,
        setFields,
        setInitState,
        RESET_MODE
    };
}

export const RESET_MODE = {
    CLEAR: "clear",
    INIT: "init"
};

