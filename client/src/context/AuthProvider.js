import React, { createContext, Component } from 'react';

export const AuthContext = createContext();
import axios from '../api/axios';

import APIs from '../context/ApiURL';


class AuthProvider extends Component {
    constructor(props) {
        super(props);
        this.state = {
          username: '',
          userID: null,
          roles: [],
          accessToken: ''
        };
        this.setAuth = this.setAuth.bind(this);
        this.setAccessToken = this.setAccessToken.bind(this);
        this.refreshAccessToken = this.refreshAccessToken.bind(this);
        this.tokenIsExpired = this.tokenIsExpired.bind(this);
      }

    setAuth(username,userID ,roles, accessToken){
        console.log(username,userID ,roles, accessToken)
        this.setState({username,userID ,roles, accessToken});
    }

    setAccessToken(aToken){
        this.state({accessToken: aToken});
    }

    async refreshAccessToken(){
        let accessToken = this.state.accessToken;
        const refreshToken = sessionStorage.getItem('refreshToken');
        const userInfo = JSON.parse(sessionStorage.getItem('userInfo'));
        //fetch
        try{
            const data = await axios.post(APIs.REFRESH_TOKEN_URL,{
                refreshToken: refreshToken
            }, // Pass as a plain object
            {
                withCredentials: true, //pass a http only cookie
                headers: {
                Authorization: `Bearer ${accessToken}`,
                'Content-Type': 'application/json'
                }
            }
            );
        
            let res = data.data;
            //is ok
            if(data.status == 200){
                console.log("SUKCES: ", res.title);
                console.log(res.resultData);
                await this.setAuth(userInfo.name,userInfo.userID ,userInfo.rules, res.resultData.accessToken);
                sessionStorage.setItem('refreshToken', res.resultData.refreshToken);
            }
        
        } catch(err){
            console.log("Error: Can't refresh token: ", err);
        }
        
    }

    tokenIsExpired = (token) => {
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

    render() {
        return (
            <AuthContext.Provider value={{
                username: this.state.username,
                userID: this.state.userID, 
                roles: this.state.roles,
                accessToken: this.state.accessToken,
                setAuth: this.setAuth,
                setAccessToken: this.setAccessToken,
                refreshAccessToken: this.refreshAccessToken,
                tokenIsExpired: this.tokenIsExpired
            }}>
                {this.props.children}
            </AuthContext.Provider>
        );
    }
}

export default AuthProvider;