import React, { createContext, useState, useCallback, useEffect } from 'react';
import axios from '../../api/axios';
import APIs from '../../api/ApiURL';

export const AuthContext = createContext();

const AuthProvider = ({ children }) => {
    const [avatarUrl, setAvatarUrl] = useState(null);
    const [username, setUsername] = useState('');
    const [email, setEmail] = useState('');
    const [userId, setUserID] = useState('');
    const [role, setRole] = useState('');
    const [accessToken, setAccessToken] = useState('');
    const [loading, setLoading] = useState(true);

    const setAuth = useCallback((_avatarUrl, _email, _username, _userId, _role, _accessToken) => {
        setAvatarUrl(_avatarUrl);
        setEmail(_email);
        setUsername(_username);
        setUserID(_userId);
        setRole(_role);
        setAccessToken(_accessToken);
    }, []);

    const saveToCookie = ({ _avatarUrl = avatarUrl, _email = email, _username = username, _userId = userId, _role = role } = {}) => {
        // user info

        let userInfo = {
            avatarUrl: _avatarUrl,
            email: _email,
            username: _username,
            userID: _userId,
            role: _role
        };

        sessionStorage.setItem('userInfo', JSON.stringify(userInfo));
    }

    const refreshAccessToken = async () => {
        const refreshToken = sessionStorage.getItem('refreshToken');
        const userInfo = JSON.parse(sessionStorage.getItem('userInfo'));
        //fetch
        try {
            const data = await axios.post(APIs.REFRESH_TOKEN, {
                refreshToken: refreshToken
            },
                {
                    withCredentials: true, //pass a http only cookie
                    headers: {
                        Authorization: `Bearer ${accessToken}`,
                        'Content-Type': 'application/json'
                    }
                }
            );

            let res = data.data;

            if (data.status === 200) {
                // console.log("SUKCES REFRESH TOKEN:");
                setAuth(userInfo.avatarUrl, userInfo.email, userInfo.username, userInfo.userID, userInfo.role, res.accessToken);
                sessionStorage.setItem('refreshToken', res.refreshToken);
            }

        } catch (err) {
            console.error("Error: Can't refresh token: ", err);
        }

    }

    const tokenIsExpired = (token) => {
        if (!token) return true;

        try {
            const decoded = JSON.parse(atob(token.split('.')[1]));
            const expirationDate = decoded.exp * 1000;
            return expirationDate < Date.now();
        } catch (error) {
            console.error("Invalid token format", error);
            return true;
        }
    };

    // useEffect(() => {
    //     if (accessToken == "") {
    //         refreshAccessToken();
    //     }
    //     setLoading(false);
    // }, [setAuth]);

    useEffect(() => {
        const initAuth = async () => {
            const refreshToken = sessionStorage.getItem('refreshToken');
            if (!refreshToken) { // do not refresh token if user is logged out
                setLoading(false);
                return;
            }

             // try refresh token only if there is user is logged
            try {
                await refreshAccessToken();
            } catch (err) {
                sessionStorage.removeItem('refreshToken');
                sessionStorage.removeItem('userInfo');
            } finally {
                setLoading(false);
            }
        };

        initAuth();
    }, []);


    return (
        <AuthContext.Provider value={{
            avatarUrl,
            email,
            username,
            userId,
            role,
            accessToken,
            setAuth,
            setAvatarUrl,
            setUsername,
            saveToCookie,
            setAccessToken,
            refreshAccessToken,
            tokenIsExpired,
            loading
        }}>
            {children}
        </AuthContext.Provider>
    );
};

export default AuthProvider;