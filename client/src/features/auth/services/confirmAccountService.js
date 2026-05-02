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
            {
                userId: userId,
                confirmationToken: token
            }
        );

        return {
            success: true,
            message: "Your account has been successfully activated."
        };

    } catch (err) {
        const status = err.response?.status;
        let message = "An error occurred while activating the account.";

        if (status === 400) {
            message = "The activation link is invalid.";
        } else if (status === 404) {
            message = "Account not found or already activated.";
        } else if (status === 410) {
            message = "This activation link has expired. Please request a new one.";
        }
        else if (status >= 500) {
            message = "Server error. Please try again later.";
        }

        return {
            success: false,
            message: message
        };
    }
};
