import axios from "../../../api/axios";
import APIs from "../../../api/ApiURL";

export const resetPasswordService = async (
    fields, valid, userId, token
) => {
    const { password, password2 } = fields;
    const { password: passIsValid, password2: pass2IsValid } = valid;

    if (!password || !password2) {
        return { errorMessage: "Password cannot be empty" };
    } else if (!passIsValid) {
        return { errorMessage: "Password does not meet the criteria." };
    } else if (!pass2IsValid) {
        return { errorMessage: "The given passwords are different!", resetForm: true };
    }

    try {
        const response = await axios.post(APIs.RESET_PASSWORD,
            {
                userId: userId,
                codedToken: token,
                newPassword: password
            }
        );

        return { success: true, resetForm: true };
    } catch (err) {
        const status = err.response?.status;
        let message = "An error occurred while reseting the password.";

        if (status === 400) {
            message = "Password does not meet the criteria.";
        } else if (status === 401) {
            message = "The activation link is invalid.";
        } else if (status === 404) {
            message = "Account not found or the reset session has expired.";
        } else if (status === 410) {
            message = "This activation link has expired. Please request a new one.";
        }
        else if (status >= 500) {
            message = "Server error. Please try again later.";
        }

        return {
            success: false,
            errorMessage: message,
            resetForm: true
        };
    }
}
