import React, { createContext } from "react";
import { useUserData } from "../../features/auth/hooks/useUserData";
import { useContext } from "react";
import { AuthContext } from "./AuthProvider";

export const UserContext = createContext();

const UserProvider = ({ children }) => {
    const { setAccessToken } = useContext(AuthContext);
    const { user, save, saveUsername, saveAvatarUrl, remove, loading } = useUserData(setAccessToken);

    // console.log("UserProvider user:", user, loading);

    return (
        <UserContext.Provider value={{ user, saveUser: save, saveUsername, saveAvatarUrl, removeUser: remove, loading }}>
            {children}
        </UserContext.Provider>
    );
};

export default UserProvider;
