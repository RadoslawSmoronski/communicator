import axios from "../../../api/axios";
import APIs from "../../../api/ApiURL";

export const registerService = async (
  fields, valid
) => {
  const { email, username, password, password2 } = fields;
  const { email: emailValid, username: usernameValid, password: passValid, password2: pass2Valid } = valid;

  if (!username || !password || !password2 || !email) {
    return { message: "Username, email or password cannot be empty." };
  } else if (!usernameValid || !emailValid) {
    return { message: "Username or email does not meet the criteria." };
  } else if (!passValid) {
    return { message: "Password does not meet the criteria.", resetPasswordFields: true };
  } else if (!pass2Valid) {
    return { message: "The given passwords are different!", resetPasswordFields: true };
  }

  try {
    const response = await axios.post(APIs.REGISTER,
      {
        email: email,
        username: username,
        password: password
      }
    );

    return { message: "User successfully created.", resetForm: true };
  } catch (err) {
    const status = err.response?.status;
    let message = err.response?.data.detail;

    if (status === 400) {
      message = "Register form doesn't meet the criteria.";
    } else if (status >= 500) {
      message = "Server error. Please try again later.";
    }

    return { message: message };
  }
};