import React, { createContext, useState, useRef, useEffect } from 'react';
import axios from '../../api/axios';
import APIs from '../../api/ApiURL';

import { tokenService } from '../../features/auth/services/tokenService';
import { userDataService } from '../../features/auth/services/userDataService';

export const AuthContext = createContext();

const AuthProvider = ({ children }) => {
    const [accessToken, setAccessToken] = useState('');
    const [loading, setLoading] = useState(true);

    const refreshAccessToken = async () => {
        console.log("Refreshuje token");

        try {
            const refreshToken = tokenService.load();
            if (!refreshToken) return;

            // fetch tokens
            const tokens = await tokenService.refresh({
                refreshToken: refreshToken
            });

            // save refresh token
            tokenService.save(tokens.refreshToken);
            // save access token
            setAccessToken(tokens.accessToken);

        } catch (err) {
            console.error("Error refreshing token:", err);
            tokenService.clear();
            userDataService.clear();

        }
    }

    const didInitRun = useRef(false);

    useEffect(() => {
        if(didInitRun.current) return;
        didInitRun.current = true;

        const initAuth = async () => {
            const refreshToken = tokenService.load();

            // do not refresh token if user is logged out
            if (!refreshToken) {
                setLoading(false);
                return;
            }

            // try refresh token only if there is user is logged
            // if (tokenService.isExpired(refreshToken)) {
            //     tokenService.clear();
            //     userDataService.clear();
            //     setLoading(false);
            //     return;
            // }
            console.log("initAuth");
            await refreshAccessToken();
            setLoading(false);
        };

        initAuth();
    }, []);





    return (
        <AuthContext.Provider value={{
            accessToken,
            setAccessToken,
            refreshAccessToken,
            loading
        }}>
            {children}
        </AuthContext.Provider>
    );
};

export default AuthProvider;