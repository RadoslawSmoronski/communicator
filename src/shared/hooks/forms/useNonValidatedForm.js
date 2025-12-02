import { useState, useRef } from "react";

export function useNonValidatedForm(initialState, onChangeCallback) {
    // for reset - return to init state
    const initialRef = useRef(initialState);

    const [fields, setFields] = useState(initialState);

    const handleFieldChange = (e) => {
        // pop up callback - hide
        onChangeCallback?.();

        const { name, value } = e.target;
        setFields(prev => ({
            ...prev,
            [name]: value
        }));
    };

    const resetForm = () => {
        const emptyState = initialRef.current;

        setFields(emptyState);
    }

    return [fields, handleFieldChange, setFields, resetForm];
}
