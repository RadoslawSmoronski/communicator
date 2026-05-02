// hooks/useConfirmAccount.js
import { useEffect, useState } from "react";
import { confirmAccountService } from "../services/confirmAccountService";

export const useConfirmAccount = (userId, token) => {
    const [loading, setLoading] = useState(true);
    const [success, setSuccess] = useState(null);
    const [message, setMessage] = useState("");

    useEffect(() => {
        const run = async () => {
            const result = await confirmAccountService(userId, token);

            setSuccess(result.success);
            setMessage(result.message);
            setLoading(false);
        };

        run();
    }, []);

    return { loading, success, message };
};
