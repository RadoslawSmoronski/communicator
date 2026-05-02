import { useState, useCallback } from "react";

const useEditableState = ({ onChangeAction, cancelCallback, onSuccessCallback } = {}) => {
    const [isEditing, setIsEditing] = useState(false);

    // edit callback
    const edit = useCallback(() => {
        if (onChangeAction) onChangeAction();
        setIsEditing(true);
    }, [onChangeAction]);

    // cancel callback
    const cancel = useCallback(() => {
        if (onChangeAction) onChangeAction();
        setIsEditing(false);
        if (cancelCallback) cancelCallback();
    }, [onChangeAction, cancelCallback]);

    // onSuccess callback
    const onSuccess = useCallback(
        (data) => {
            if (onSuccessCallback) onSuccessCallback(data);
            setIsEditing(false);
        },
        [onSuccessCallback]
    );

    return {
        isEditing,
        setIsEditing,
        edit,
        cancel,
        onSuccess
    };
}

export default useEditableState;