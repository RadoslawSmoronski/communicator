import axios from "../../../api/axios";
import APIs from "../../../api/ApiURL";

export const resetPasswordService = async (
    fields, valid, userId, token
) =>{
    const { password, password2 } = fields;
    const { password: passIsValid, password2: pass2IsValid } = valid;

    if (!password || !password2) {
        return {errorMessage: "Password cannot be empty"};
    } else if (!passIsValid) {
        return {errorMessage:"Password does not meet the criteria."};
    } else if (!pass2IsValid) {
        return {errorMessage:"The given passwords are different!", resetForm: true};
    }

    try {
        const responce = await axios.post(APIs.RESET_PASSWORD,
            JSON.stringify({
                userId: userId,
                codedToken: token,
                newPassword: password
            })
        );

        return {success: true, resetForm: true};
    } catch (err) {
        return {errorMessage:err.response?.data.detail || err.message, resetForm: true};
    }
}
