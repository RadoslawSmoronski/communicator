import { useState } from "react";

export const useUserData = (setAuth) => {
    // get userInfo from sessionStorage
    const [user, setUser] = useState(() => {
        const stored = sessionStorage.getItem('userInfo');
        return stored ? JSON.parse(stored) : null;
    });

    const save = (userData) => {
        setUser(userData);

        // save to sessionStorage
        sessionStorage.setItem('refreshToken', userData.refreshToken);
        sessionStorage.setItem('userInfo', JSON.stringify(userData));
        // save to authProvider
        if (setAuth) {
            setAuth(
                userData.avatarUrl,
                userData.email,
                userData.username,
                userData.userID,
                userData.role,
                userData.accessToken
            );
        }
    };

    const remove = () => {
        setUser(null);
        sessionStorage.removeItem('refreshToken');
        sessionStorage.removeItem('userInfo');
    };

    return { user, save, remove };
};