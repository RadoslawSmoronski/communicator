import { useState } from "react";

export function useNonValidatedForm(initialState, onChangeCallback) {
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

    return [fields, handleFieldChange, setFields];
}
