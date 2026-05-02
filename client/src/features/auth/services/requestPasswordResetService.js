import axios from "../../../api/axios";
import APIs from "../../../api/ApiURL";

// returns message
export const requestPasswordResetService = async (
    email, valid
) => {
    if (!email) {
        return { message: "Email can't be null" };
    }
    if (!valid) {
        return { message: "Email does not meet the criteria." };
    }

    try {
        await axios.post(APIs.REQUEST_PASSWORD_RESET,
            {
                email: email
            }
        );
        return { message: "Password reset email sent successfully." };

    } catch (err) {
        const status = err.response?.status;
        let message = "An error occurred while requesting for password reset.";

        if (status === 401) {
            message = "Invalid request. Please check your email address.";
        } else if (status === 403) {
            message = "Account not activated.";
        } else if (status === 404) {
            message = "Could not process request for this email address.";
        }
        else if (status >= 500) {
            message = "Server error. Please try again later.";
        }

        return { message: message };
    }


}