import axios from "../../../api/axios";
import APIs from "../../../api/ApiURL";

export const registerService = async (
    fields, valid
) => {
    const { email, username, password, password2 } = fields;
    const { email: emailValid, username: usernameValid, password: passValid, password2: pass2Valid } = valid;

    if (!username || !password || !password2 || !email) {
      return {message: "Username, email or password cannot be empty."};
    }else if (!usernameValid || !emailValid) {
      return {message: "Username or email does not meet the criteria."};
    } else if(!passValid){
      return {message: "Password does not meet the criteria.", resetPasswordFields: true};
    } else if (!pass2Valid) {
      return {message:"The given passwords are different!", resetPasswordFields: true};
    }

    try {
      const responce = await axios.post(APIs.REGISTER,
        JSON.stringify({
          email: email,
          username: username,
          password: password
        })
      );

    return { message: "User successfully created.", resetForm: true};
    } catch (err) {
      return { message: err.response?.data.detail || err.message};
    }
};