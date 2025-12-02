import axios from "../../../api/axios";
import APIs from "../../../api/ApiURL";

// returns message
export const requestPasswordResetService = async (
    email, isValid
) => {
    if(!email){
        return {message :"Email can't be null"};
    }
    if(!isValid){
        return {message :"Email does not meet the criteria."};
    }

    try {
        await axios.post(APIs.REQUEST_PASSWORD_RESET,
            JSON.stringify({
                email: email
            })
        );
        return {message: "Password reset email sent successfully."};

    } catch (err) {
         return {message : err.response?.data?.detail || "Invalid reset password request"};
    }


}