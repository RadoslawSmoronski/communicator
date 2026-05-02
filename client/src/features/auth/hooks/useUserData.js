import { useState, useEffect, useContext } from "react";
import { tokenService } from "../services/tokenService";
import { userDataService } from "../services/userDataService";
import { mapUserToSessionStorage } from "../mappers/userSessionStorageMapper"

import { AuthContext } from "../../../app/providers/AuthProvider";

export const useUserData = (setAccessToken) => {
    // get userInfo from sessionStorage
    const { accessToken, loading: tokenLoading } = useContext(AuthContext);
    const [user, setUser] = useState(null);
    const [loading, setLoading] = useState(true);

    const load = () => {
        const stored = userDataService.load();
        const userObj = stored ? JSON.parse(stored) : null;
        setUser(userObj);
    }

    const save = (userData) => {
        // save to sessionStorage
        const refreshToken = userData.refreshToken;
        tokenService.save(refreshToken);
        const accessToken = userData.accessToken;
        setAccessToken(accessToken);

        // save user to localStorage
        const userDataToSave = mapUserToSessionStorage(userData);
        userDataService.save(userDataToSave);
        // save user
        setUser(userDataToSave);

    };

    const saveUsername = (newUsername) => {
        setUser(prev => {
            if (!prev) return prev;

            const updated = { ...prev, username: newUsername };

            const userToSave = mapUserToSessionStorage(updated);
            userDataService.save(userToSave);

            return userToSave;
        });
    };

    const saveAvatarUrl = (newAvatarUrl) => {
        setUser(prev => {
            if (!prev) return prev;

            const updated = { ...prev, avatarUrl: newAvatarUrl };

            const userToSave = mapUserToSessionStorage(updated);
            userDataService.save(userToSave);

            return userToSave;
        });
    };

    const remove = () => {
        setUser(null);
        tokenService.clear();
        userDataService.clear();
    };

    // get data when tokens are fetched
    useEffect(() => {
        const stored = userDataService.load();
        const userObj = stored ? JSON.parse(stored) : null;
        setUser(userObj);
        setLoading(false);
    }, [accessToken]);


    return { user, save, saveUsername, saveAvatarUrl, remove, loading };
};