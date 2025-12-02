import axios from "../../../api/axios";
import APIs from "../../../api/ApiURL";

export const confirmAccountService = async (userId, token) => {
    if (!userId || !token) {
        return {
            success: false,
            message: "The address is incomplete or wrong."
        };
    }

    try {
        const response = await axios.post(
            APIs.CONFIRM_ACCOUNT,
            JSON.stringify({
                userId: userId,
                confirmationToken: token
            })
        );

        return {
            success: true,
            message: "Your account has been successfully activated."
        };

    } catch (err) {

        if (err.response?.status === 400) {
            return {
                success: false,
                message: "The address contains incorrect data."
            };
        }

        console.error(err);
        return {
            success: false,
            message: "An error occurred while activating the account."
        };
    }
};
