import { loginUser } from "./authService";

export const submitLoginService = async (
    event, formData, setFormData, saveUserData, showPopUp, navigate
) => {
    event.preventDefault();
    if (!formData.email || !formData.password) {
        showPopUp?.("Email or Password can't be null");
        return;
    }

    try {
        // fetch
        const userData = await loginUser({
            Email: formData.email,
            Password: formData.password
        });

        // save to sessionStorage and authProvider
        saveUserData(userData); 

        // redirect
        navigate("/message");

    } catch (err) {
        console.error(err);
        const message = err.response?.data?.title || "Invalid login request";
        showPopUp?.(message);
    }
    setFormData({ email: "", password: "" });
};
