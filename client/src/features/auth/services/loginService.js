import { loginUser } from "./authService";

// returns errorMessage
export const submitLoginService = async (formData) => {
    if (!formData.email || !formData.password) {
        return { errorMessage: "Email or Password can't be null" };
    }

    try {
        // fetch
        const userData = await loginUser({
            Email: formData.email,
            Password: formData.password
        });

        return { data: userData }
    } catch (err) {
        const status = err.response?.status;
        let message = "An unexpected error occurred";

        if (status === 400 || status === 401 || status === 404) {
            message = "Invalid email or password.";
        } else if (status === 403) {
            message = "Access denied. Please contact support.";
        } else if (status >= 500) {
            message = "Server error. Please try again later.";
        }

        return { errorMessage: message };
    }
};
