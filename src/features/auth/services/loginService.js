import { loginUser } from "./authService";

// returns errorMessage
export const submitLoginService = async (formData) => {
    if (!formData.email || !formData.password) {
        return {errorMessage : "Email or Password can't be null"};
    }

    try {
        // fetch
        const userData = await loginUser({
            Email: formData.email,
            Password: formData.password
        });

        return { data: userData }
    } catch (err) {
        console.error(err);
        return {errorMessage : err.response?.data?.title || "Invalid login request"};
    }
};
